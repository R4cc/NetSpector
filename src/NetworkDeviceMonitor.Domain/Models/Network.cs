using System.ComponentModel.DataAnnotations;

namespace NetworkDeviceMonitor.Domain.Models;

public class Network
{
    [Key]
    public int NetworkId { get; set; }
    public string IpNetworkId { get; set; }
    public int SubnetMask { get; set; }
    public string Name { get; set; }
    public List<Device> Devices { get; set; } = new();
}