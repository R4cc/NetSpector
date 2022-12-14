using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    
    public async Task<Scan> GetDetachedByID(int scanId)
    {
        var scan = await _context.Scans.FirstOrDefaultAsync(s => s.ScanId == scanId);
        _context.Entry(scan).State = EntityState.Detached;
        return scan;
    } 
    
    public async Task<List<Scan>> GetAllActiveDetached()
    {
        var scans = await _context.Scans.Include(s => s.Network).ThenInclude(n => n.Devices).Where(s => s.IsActive).ToListAsync();
        foreach (var scan in scans)
        {
            _context.Entry(scan).State = EntityState.Detached;
        }
        return scans;
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