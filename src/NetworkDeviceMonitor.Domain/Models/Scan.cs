using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetworkDeviceMonitor.Domain.Models;

public class Scan
{
    public int ScanId { get; set; }
    public int NetworkId { get; set; } = -1;
    [ForeignKey("NetworkId")]
    public Network Network { get; set; }
    public string CronSchedule { get; set; }
    public DateTime? FirstExecuted { get; set; }
    public DateTime? LastExecuted { get; set; }
    public bool IsActive { get; set; } = true;
}