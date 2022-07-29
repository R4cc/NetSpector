using System.ComponentModel.DataAnnotations;

namespace NetworkDeviceMonitor.Domain.Models;

public class Manufacturer
{
    [Key]
    public int ManufacturerId { get; set; }
    public string Prefix { get; set; }
    public string Name { get; set; }
    public DateTime LastUpdated { get; set; }
}