using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NCrontab;

namespace NetworkDeviceMonitor.Domain.Models;

public class Scan
{
    public int ScanId { get; set; }
    public int NetworkId { get; set; } = -1;
    [ForeignKey("NetworkId")]
    public Network Network { get; set; }
    public string CronSchedule { get; set; }

    public DateTime NextExecute
    {
        get
        {
            return CrontabSchedule.Parse(CronSchedule).GetNextOccurrence(DateTime.Now);
        }
    }

    public DateTime? FirstExecuted { get; set; }
    public DateTime? LastExecuted { get; set; }
    public bool IsActive { get; set; } = true;
}