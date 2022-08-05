using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetworkDeviceMonitor.Domain.Models;
using Microsoft.EntityFrameworkCore.Design;

namespace NetworkDeviceMonitor.DAL.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var network = builder.Entity<Network>();

        network.HasMany<Device>().WithOne().OnDelete(DeleteBehavior.Cascade);
        network.HasOne<Scan>().WithOne().OnDelete(DeleteBehavior.Cascade);
        network.HasData(new Network
        {
            Name = "Network #1",
            NetworkId = 1,
            IpNetworkId = "192.168.0.1",
            SubnetMask = 24
        });
        
        var device = builder.Entity<Device>();
        device.Property(x => x.ManufacturerId).IsRequired(false);
    }

    public virtual DbSet<Network> Networks { get; set; }
    public virtual DbSet<Device> Devices { get; set; }
    public virtual DbSet<Manufacturer> Manufacturers { get; set; }
    public virtual DbSet<Scan> Scans { get; set; }
}