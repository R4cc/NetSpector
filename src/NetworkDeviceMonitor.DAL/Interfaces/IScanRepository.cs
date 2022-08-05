using NetworkDeviceMonitor.Domain.Models;

namespace NetworkDeviceMonitor.DAL.Interfaces;

public interface IScanRepository
{
    Task<List<Scan>> GetAll();
    Task<List<Scan>> GetAllActive();
    Task Create(Scan scan);
    Task Update(Scan scan);
    Task Remove(Scan scan);
    Task<Scan> GetByID(int scanId);
}