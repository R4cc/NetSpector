using NetworkDeviceMonitor.DAL.Repositories;

namespace NetworkDeviceMonitor.DAL.Interfaces;

public interface IUnitOfWork
{
    IDeviceRepository IDeviceRepository { get; }
    INetworkRepository INetworkRepository { get; }
    IManufacturerRepository IManufacturerRepository { get; }
    Task SaveChangesAsync();
}