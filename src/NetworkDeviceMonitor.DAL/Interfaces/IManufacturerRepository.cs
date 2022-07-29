using NetworkDeviceMonitor.Domain.Models;

namespace NetworkDeviceMonitor.DAL.Interfaces;

public interface IManufacturerRepository
{
    Task BulkCreate(List<Manufacturer> manufacturers);
    Task BulkUpdate(List<Manufacturer> manufacturers);
    Task<List<Manufacturer>> GetAll();
    Task Update(Manufacturer manufacturer);
}