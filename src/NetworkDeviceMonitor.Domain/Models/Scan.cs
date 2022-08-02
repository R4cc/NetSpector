using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetworkDeviceMonitor.Domain.Models;

public class Scan
{
    [Key]
    public int ScanId { get; set; }
    public int NetworkId { get; set; }
    public Network Network { get; set; }
    public string CronSchedule { get; set; }
    public DateTime? FirstExecuted { get; set; }
    public DateTime? LastExecuted { get; set; }
    public bool IsActive { get; set; } = true;
}