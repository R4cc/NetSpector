using Microsoft.EntityFrameworkCore;
using NetworkDeviceMonitor.DAL.Data;
using NetworkDeviceMonitor.DAL.Interfaces;
using NetworkDeviceMonitor.Domain.Models;

namespace NetworkDeviceMonitor.DAL.Repositories;

public class DeviceRepository : IDeviceRepository
{
    ApplicationDbContext _context;
    public DeviceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Device>> GetDevicesFromNetworkId(int networkId)
    {
        return await _context.Devices.Include(d => d.Manufacturer).Where(d => d.NetworkId == networkId).ToListAsync();
    }

    public async Task<Device> GetDetachedDeviceById(int deviceId)
    {
        var device = await _context.Devices.FirstOrDefaultAsync(d => d.DeviceId == deviceId);
        _context.Entry(device).State = EntityState.Detached;
        return device;
    }

    public async Task Update(Device device)
    {
        _context.Update(device);
    }
    
    public async Task BulkUpdate(List<Device> devices)
    {
        await _context.Devices.BulkUpdateAsync(devices, options => { options.BatchSize = 100; });
    }
    
    public async Task BulkCreate(List<Device> devices)
    {
        await _context.Devices.BulkInsertAsync(devices, options => { options.BatchSize = 100; });
    }    
}