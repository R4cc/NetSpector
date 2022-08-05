using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;
using NetworkDeviceMonitor.DAL.Data;
using NetworkDeviceMonitor.DAL.Interfaces;
using NetworkDeviceMonitor.DAL.Services;
using NetworkDeviceMonitor.Domain.Models;

namespace NetworkDeviceMonitor.DAL.BackgroundServices;

public class AutoscanBgService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private int UntilNextExecution(DateTime _nextRun) => Math.Max(0, (int)_nextRun.Subtract(DateTime.Now).TotalMilliseconds);

    public AutoscanBgService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        List<Scan> scanList;

        using (IServiceScope scope = _serviceProvider.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            scanList = await uow.IScanRepository.GetAllActive();
        }

        foreach (var scan in scanList)
        {
            while(!stoppingToken.IsCancellationRequested){
                await Task.Run( async () =>
                {
                    // Calculate time until next execution from crontab schedule
                    await Task.Delay(UntilNextExecution(CrontabSchedule.Parse(scan.CronSchedule).GetNextOccurrence(DateTime.Now)), stoppingToken); // wait until next time

                    // Refresh data before execution to ensure scan has not been deleted
                    Scan currentScan;
                    using (IServiceScope scope = _serviceProvider.CreateScope())
                    {
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        currentScan = await uow.IScanRepository.GetByID(scan.ScanId);
                    }
                    
                    if (currentScan is not null && currentScan.IsActive)
                    {
                        await DoWork(scan);
                    }
                }, stoppingToken);
            }
        }
    }

    private async Task DoWork(Scan scan)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        
        var pingService = scope.ServiceProvider.GetRequiredService<PingService>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
        scan.FirstExecuted ??= DateTime.Now;
        scan.LastExecuted = DateTime.Now;
            
        var updatedNetwork = await pingService.ScanNetwork(scan.Network);
        await Task.Delay(5000);
        // make sure network hasn't been deleted while scanning
        var confirmationNetwork = await context.Networks.FirstOrDefaultAsync(n => n.NetworkId == scan.Network.NetworkId);
        if (confirmationNetwork is not null || confirmationNetwork.IpNetworkId != scan.Network.IpNetworkId)
        {
            context.Networks.Update(updatedNetwork);
            await context.SaveChangesAsync();
        }
        
        scope.Dispose();
    }
}