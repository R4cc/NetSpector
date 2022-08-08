using System.Net;

namespace NetworkDeviceMonitor.Domain.Models;

public class Exclusion
{
    public int ExclusionId { get; set; }
    public  string StartIpAddress { get; set; }
    public  string? EndIpAddress { get; set; }
    public int NetworkId { get; set; }
    public Network Network { get; set; }
    public string Note { get; set; }
}