using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace NetworkDeviceMonitor.Domain.Models;

public class Network
{
    public int NetworkId { get; set; }
    public string IpNetworkId { get; set; }
    public int SubnetMask { get; set; } = 24;
    public string Name { get; set; }
    //[ForeignKey("ScanId")]
    public Scan Scan { get; set; }
    public List<Device> Devices { get; set; } = new();
    public List<Exclusion> Exclusions { get; set; } = new();
}