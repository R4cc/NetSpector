using System.Diagnostics;
using System.Net;
using NetworkDeviceMonitor.Domain.Models;
using System.Net.NetworkInformation;
using System.Text;
using IsAliveLib;
using Microsoft.Extensions.Logging;
using NetworkDeviceMonitor.DAL.Interfaces;

namespace NetworkDeviceMonitor.DAL.Services;

public class PingService
{
    private readonly IUnitOfWork _uow;
    
    public PingService(IUnitOfWork uow)
    {
        _uow = uow;
    }
    public async Task<Network> ScanNetwork(Network network)
    {
        // IPs that are associated with a device
        List<Device> knownDevices = network.Devices;
        // get all IPs that are not associated with a device
        List<IPAddress> unknownIps = (await GetIpHostRange(network.IpNetworkId, network.SubnetMask)).Where(x => knownDevices.All(d => d.IpAddress != x.ToString())).ToList();

        List<Device> devicesToUpdate = new();
        List<Device> devicesToCreate = new();
        
        // mac vendors for creation of new Devices
        var manufacturers = await _uow.IManufacturerRepository.GetAll();
        
        // shortened scanning datetime 
        DateTime scanDateTime = Convert.ToDateTime(DateTime.Now.ToString(("g")));

        // loop through unknown IPs and ping each one
        Parallel.ForEach(unknownIps, async ip =>
        {
            var reply = await PingDevice(ip);

            if (!reply.success)
            {
                return;
            }

            // retrieve hostname and mac
            string hostnameFromIp = await GetHostnameFromIp(ip);
            string macAddressFromIp = MacFormatService.GetRemoteMac(ip.ToString(), ':');
            int? manufacturerId = null;

            // Remove device form list if it contains a device with the exact mac or hostname
            switch (1)
            {
                case 1 when macAddressFromIp is "" && hostnameFromIp is "":
                    break;                
                case 1 when macAddressFromIp is "" && !String.IsNullOrEmpty(hostnameFromIp) && network.Devices.Any(x => x.Hostname == hostnameFromIp):
                    network.Devices.Remove(network.Devices.FirstOrDefault(x => x.Hostname == hostnameFromIp));
                    break;
                case 1 when !String.IsNullOrEmpty(macAddressFromIp) && String.IsNullOrEmpty(hostnameFromIp) && network.Devices.Any(x => x.MacAddress == macAddressFromIp):
                    network.Devices.Remove(network.Devices.FirstOrDefault(x => x.MacAddress == macAddressFromIp));
                    break;
            }

            // set manufacturer if mac is set
            if (manufacturers is not null && !String.IsNullOrEmpty(macAddressFromIp))
            {
                manufacturerId = manufacturers.FirstOrDefault(x => x.Prefix == macAddressFromIp.Substring(0, 8))?.ManufacturerId;
            }
            
            // New device found; create it
            devicesToCreate.Add( new Device
            {
                IpAddress = ip.ToString(),
                NetworkId = network.NetworkId,
                MacAddress = macAddressFromIp,
                FirstSeen = scanDateTime,
                LastSeen = scanDateTime,
                Hostname = hostnameFromIp,
                ManufacturerId = manufacturerId
            });
        });
        
        Parallel.ForEach(knownDevices, async _device =>
        {
            var device = _device;
            var ip = IPAddress.Parse(device.IpAddress);
            var reply = await PingDevice(ip);
            
            // Set device to offline if not reachable
            if (!reply.success)
            {
                device.IsOnline = false;
                devicesToUpdate.Add(device);
                return;
            }
            
            // Get hostname and macaddress from the IP
            string hostnameFromIp = await GetHostnameFromIp(ip);
            string macAddressFromIp = MacFormatService.GetRemoteMac(ip.ToString(), ':');
            
            // Set manufacturer
            if (!String.IsNullOrEmpty(device.MacAddress) && device.ManufacturerId is null)
            {
                device.ManufacturerId = manufacturers.FirstOrDefault(x => x.Prefix == device.MacAddress.Substring(0, 8))?.ManufacturerId;
            }
            
            device.LastSeen = scanDateTime;
            device.IsOnline = true;
            device.Hostname = hostnameFromIp;
            device.MacAddress = macAddressFromIp;
            
            devicesToUpdate.Add(device);
        });

        network.Devices.AddRange(devicesToUpdate);
        network.Devices.AddRange(devicesToCreate);
        
        return network;
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
            return String.Empty;
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
    /// <returns>Ping reply</returns>
    public async Task<IsAlivePayload> PingDevice(IPAddress ip)
    {
        IsAlive isAlive = new IsAlive(500);

        IsAlivePayload payload = isAlive.check(ip, 0, IsAlive.NETWORK_PROTOCOL.ICMP);

        return payload;
    }
}