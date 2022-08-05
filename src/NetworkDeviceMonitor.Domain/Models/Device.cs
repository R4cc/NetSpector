using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetworkDeviceMonitor.Domain.Models;

public class Device
{
    [Key]
    public int DeviceId { get; set; }
    public string? Name { get; set; }
    public string? Hostname { get; set; }
    public string IpAddress { get; set; }

    // Returns a int32 of an IP address to sort by ip
    public int Int32IpAddress
    {
        get
        {
            int sum = 0;
            var arr = IpAddress.Split('.');

            sum += Convert.ToInt32(arr[0]) * 256 * 256 * 256;
            sum += Convert.ToInt32(arr[1]) * 256 * 256;
            sum += Convert.ToInt32(arr[2]) * 256;
            sum += Convert.ToInt32(arr[3]);
            
            return sum;
        }
    }

    public string MacAddress { get; set; }
    public DateTime LastSeen { get; set; }
    public DateTime FirstSeen { get; set; }
    public Manufacturer? Manufacturer { get; set; }
    public int? ManufacturerId { get; set; }
    [ForeignKey("Network")]
    public int NetworkId { get; set; }
    public Network Network { get; set; }
    public bool IsOnline { get; set; } = true;
}