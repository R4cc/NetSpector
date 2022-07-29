using NetworkDeviceMonitor.DAL.Data;
using NetworkDeviceMonitor.DAL.Interfaces;
using NetworkDeviceMonitor.DAL.Repositories;

namespace NetworkDeviceMonitor.DAL.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private ApplicationDbContext _context;
    private IDeviceRepository _deviceRepo;
    private INetworkRepository _networkRepo;
    private IManufacturerRepository _manufacturerRepo;
    
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IDeviceRepository IDeviceRepository => _deviceRepo ?? new DeviceRepository(_context);
    public INetworkRepository INetworkRepository => _networkRepo ?? new NetworkRepository(_context);
    public IManufacturerRepository IManufacturerRepository => _manufacturerRepo ?? new ManufacturerRepository(_context);

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}