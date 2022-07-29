using Microsoft.EntityFrameworkCore;
using NetworkDeviceMonitor.DAL.Data;
using NetworkDeviceMonitor.DAL.Interfaces;
using NetworkDeviceMonitor.Domain.Models;

namespace NetworkDeviceMonitor.DAL.Repositories;

public class ManufacturerRepository : IManufacturerRepository
{
    ApplicationDbContext _context;
    public ManufacturerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task BulkCreate(List<Manufacturer> manufacturers)
    {
        await _context.Manufacturers.BulkInsertAsync(manufacturers, options => { options.BatchSize = 100; });
    }
    
    public async Task BulkUpdate(List<Manufacturer> manufacturers)
    {
        await _context.Manufacturers.BulkUpdateAsync(manufacturers, options => { options.BatchSize = 100; });
    }

    public async Task<List<Manufacturer>> GetAll()
    {
        return await _context.Manufacturers.ToListAsync();
    }
    
    public async Task Update(Manufacturer manufacturer)
    {
        _context.Update(manufacturer);
    }
}