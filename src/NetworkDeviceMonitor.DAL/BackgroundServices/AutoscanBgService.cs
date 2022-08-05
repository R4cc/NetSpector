using System.Runtime.CompilerServices;
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
        List<Scan> scanList = new();
        
        while(scanList.Count == 0){

            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                scanList = await uow.IScanRepository.GetAllActiveDetached();
            }

            if (scanList.Count == 0)
            {
                await Task.Delay(20000);
            }
        }
        
        foreach (var scan in scanList)
        {
                Task.Run( async () =>
                {
                    while(!stoppingToken.IsCancellationRequested)
                    {
                        // Calculate time until next execution from crontab schedule
                        await Task.Delay(UntilNextExecution(CrontabSchedule.Parse(scan.CronSchedule).GetNextOccurrence(DateTime.Now)), stoppingToken); // wait until next time

                        // Refresh data before execution to ensure scan has not been deleted
                        Scan currentScan;
                        using (IServiceScope scope = _serviceProvider.CreateScope())
                        {
                            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                            currentScan = await uow.IScanRepository.GetDetachedByID(scan.ScanId);
                        }
                        
                        if (currentScan is not null && currentScan.IsActive)
                        {
                            await DoWork(scan);
                        }
                    }
                }, stoppingToken);
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
        scan.Network = updatedNetwork;
        
        // make sure network hasn't been deleted while scanning
        var confirmationNetwork = context.Networks.Any(n => n.NetworkId == scan.NetworkId);
        if (confirmationNetwork)
        {
            context.Networks.Update(updatedNetwork);
            context.Scans.Update(scan);
            await context.SaveChangesAsync();
        }
        
        scope.Dispose();
    }
}