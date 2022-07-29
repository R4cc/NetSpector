using NetworkDeviceMonitor.Domain.Models;

namespace NetworkDeviceMonitor.DAL.Interfaces;

public interface IDeviceRepository
{
    Task<List<Device>> GetDevicesFromNetworkId(int networkId);
    Task<Device> GetDetachedDeviceById(int deviceId);
    Task Update(Device device);
    Task BulkUpdate(List<Device> devices);
    Task BulkCreate(List<Device> devices);
}