using Microsoft.EntityFrameworkCore;
using NetworkDeviceMonitor.DAL.Data;
using NetworkDeviceMonitor.DAL.Interfaces;
using NetworkDeviceMonitor.Domain.Models;

namespace NetworkDeviceMonitor.DAL.Repositories;

public class ScanRepository : IScanRepository
{
    readonly ApplicationDbContext _context;
    public ScanRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Scan>> GetAll()
    {
        return await _context.Scans.Include(s => s.Network).ToListAsync();
    } 
    
    public async Task<List<Scan>> GetActive()
    {
        return await _context.Scans.Include(s => s.Network).Where(s => s.IsActive).ToListAsync();
    } 
    
    public async Task Create(Scan scan)
    {
        await _context.Scans.AddAsync(scan);
    }     
    
    public async Task Update(Scan scan)
    {
        await _context.Scans.SingleUpdateAsync(scan);
    }     
    
    public async Task Remove(Scan scan)
    {
        _context.Scans.Remove(scan);
    } 
}