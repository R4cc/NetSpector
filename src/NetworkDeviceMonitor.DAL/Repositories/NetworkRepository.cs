using Microsoft.EntityFrameworkCore;
using NetworkDeviceMonitor.DAL.Data;
using NetworkDeviceMonitor.DAL.Interfaces;
using NetworkDeviceMonitor.Domain.Models;

namespace NetworkDeviceMonitor.DAL.Repositories;

public class NetworkRepository : INetworkRepository
{
    readonly ApplicationDbContext _context;
    public NetworkRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Network>> GetAll()
    {
        return await _context.Networks.Include(n => n.Devices).ToListAsync();
    }    
    
    public async Task<Network> GetNetworkFromId(int networkId)
    {
        return await _context.Networks
            .Include(n => n.Devices)
            .Include(n => n.Exclusions)
            .FirstOrDefaultAsync(n => n.NetworkId == networkId);
    }
    
    public async Task Update(Network network)
    {
        _context.Networks.Update(network);
    }
    
    public async Task Remove(Network network)
    {
        _context.Networks.Remove(network);
    }    
    
    public async Task Create(Network network)
    {
        await _context.Networks.AddAsync(network);
    }
}