using NetworkDeviceMonitor.Domain.Models;

namespace NetworkDeviceMonitor.DAL.Interfaces;

public interface IScanRepository
{
    Task<List<Scan>> GetAll();
    Task<List<Scan>> GetActive();
    Task Create(Scan scan);
    Task Update(Scan scan);
    Task Remove(Scan scan);
}