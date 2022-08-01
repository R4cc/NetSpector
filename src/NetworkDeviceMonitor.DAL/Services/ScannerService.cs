using System.Diagnostics;
using System.Net;
using NetworkDeviceMonitor.Domain.Models;
using System.Net.NetworkInformation;
using System.Text;
using IsAliveLib;
using Microsoft.Extensions.Logging;
using NetworkDeviceMonitor.DAL.Interfaces;

namespace NetworkDeviceMonitor.DAL.Services;

public class ScannerService
{
    private readonly IUnitOfWork _uow;
    
    public ScannerService(IUnitOfWork uow)
    {
        _uow = uow;
    }
    public async Task StartScan(Network network)
    {
        Stopwatch sw = new();
        sw.Start();
        
        // get all IPs in specified network
        List<IPAddress> ips = await GetIpHostRange(network.IpNetworkId, network.SubnetMask);

        // mac vendors for creation of new Devices
        var manufacturers = await _uow.IManufacturerRepository.GetAll();
        var devicesToUpdate = new List<Device>();
        var devicesToCreate = new List<Device>();
        
        // shortened scanning datetime 
        DateTime scanDateTime = Convert.ToDateTime(DateTime.Now.ToString(("g")));

        // loop through IPs and ping each one
        Parallel.ForEach(ips, async ip =>
        {
            var reply = await PingDevice(ip);

            if (!reply.success)
            {
                return;
            }

            // try to retrieve hostname
            string hostname = await GetHostnameFromIp(ip);
            Manufacturer manufacturer = null;

            //var hasWebInterface = await HasWebInterface(hostname);
            
            // MAC address retrieval 
            var macAddress = MacResolverService.GetRemoteMac(ip.ToString(), ':');

            if (!String.IsNullOrEmpty(macAddress))
            {
                // get manufacturer by matching MAC address prefix
                manufacturer = manufacturers.FirstOrDefault(x => x.Prefix == macAddress.Substring(0, 8));
            }
            
            // IP based updating
            if (String.IsNullOrEmpty(macAddress))
            {
                if (network.Devices.Any(d => d.IpAddress == ip.ToString() && d.MacAddress == macAddress))
                {
                    var existingDevice = network.Devices.FirstOrDefault(i => i.IpAddress == ip.ToString());
                    if (existingDevice is null) return;
                    
                    existingDevice.LastSeen = scanDateTime;
                    existingDevice.NetworkId = network.NetworkId;
                    existingDevice.Hostname = hostname;
                    existingDevice.ManufacturerId = manufacturer?.ManufacturerId;
                    
                    devicesToUpdate.Add(existingDevice);
                    return;
                }
            }

            // MAC Based updating
            if(network.Devices.Any(d => d.MacAddress == macAddress))
            {
                var existingDevice = network.Devices.FirstOrDefault(i => i.MacAddress == macAddress);
                if (existingDevice is null) return;
                
                existingDevice.IpAddress = ip.ToString();
                existingDevice.LastSeen = scanDateTime;
                existingDevice.NetworkId = network.NetworkId;
                existingDevice.Hostname = hostname;
                existingDevice.ManufacturerId = manufacturer?.ManufacturerId;
                
                devicesToUpdate.Add(existingDevice);
                return;
            }

            // New device found; create it
            var deviceToAdd = new Device
            {
                IpAddress = ip.ToString(),
                NetworkId = network.NetworkId,
                MacAddress = macAddress,
                FirstSeen = scanDateTime,
                LastSeen = scanDateTime,
                Hostname = hostname,
                ManufacturerId = manufacturer?.ManufacturerId
            };

            devicesToCreate.Add(deviceToAdd);
        });
        
        await _uow.IDeviceRepository.BulkUpdate(devicesToUpdate);
        await _uow.IDeviceRepository.BulkCreate(devicesToCreate);
        
        sw.Stop();
        Console.WriteLine($"{sw.ElapsedMilliseconds}");
    }

    /// <summary>
    /// This method finds the hostname to a given IP via DNS
    /// </summary>
    /// <param name="ip">IP Address</param>
    /// <returns>Hostname as string</returns>
    private async Task<string> GetHostnameFromIp(IPAddress ip)
    {
        try
        {
            return (await Dns.GetHostEntryAsync(ip)).HostName;
        }
        catch
        {
            return "N/A";
        }
    }

    private async Task<List<IPAddress>> GetIpHostRange(string ipAddress, int subnetMaskSuffix)
    {
        var strArr = ipAddress.Split(".");
        var byteArr = strArr.Select(byte.Parse).ToArray();
        
        IPAddress ip = new IPAddress(byteArr);
        
        uint mask = ~(uint.MaxValue >> subnetMaskSuffix);

        // Convert the IP address to bytes.
        byte[] ipBytes = ip.GetAddressBytes();

        // BitConverter gives bytes in opposite order to GetAddressBytes().
        byte[] maskBytes = BitConverter.GetBytes(mask).Reverse().ToArray();

        byte[] startIpBytes = new byte[ipBytes.Length];
        byte[] endIpBytes = new byte[ipBytes.Length];

        // Calculate the bytes of the start and end IP addresses.
        for (int i = 0; i < ipBytes.Length; i++)
        {
            startIpBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
            endIpBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
        }

        return await GetIpRange(startIpBytes, endIpBytes);
    }

    private async Task<List<IPAddress>> GetIpRange(byte[] startArr, byte[] endArr)
    {
        // reverse byte array
        Array.Reverse(startArr);
        Array.Reverse(endArr);
        
        var start = BitConverter.ToInt32(startArr, 0);
        var end = BitConverter.ToInt32(endArr, 0);
        
        List<IPAddress> addresses = new List<IPAddress>();
        for (int i = start+1; i <= end-1; i++)
        {
            byte[] bytes = BitConverter.GetBytes(i);
            addresses.Add(new IPAddress(new[] { bytes[3], bytes[2], bytes[1], bytes[0] }));
        }

        return addresses;
    }

    /// <summary>
    /// Pings a single device
    /// </summary>
    /// <param name="ip">IP Address of a device</param>
    /// <param name="pingSender">Ping instance</param>
    /// <returns>Ping reply</returns>
    public async Task<IsAlivePayload> PingDevice(IPAddress ip)
    {
        IsAlive isAlive = new IsAlive(500);

        IsAlivePayload payload = isAlive.check(ip, 0, IsAlive.NETWORK_PROTOCOL.ICMP);

        return payload;
    }

    private async Task<bool> HasWebInterface(string hostname)
    {
        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://" + hostname);
        request.AllowAutoRedirect = false; // find out if this site is up and don't follow a redirector
        request.Method = "HEAD";
        try
        {
            var response = request.GetResponse();
            // do something with response.Headers to find out information about the request
            return true;
        }
        catch (WebException wex)
        {
            //set flag if there was a timeout or some other issues
            return false;
        }
        return false;
    }
}
