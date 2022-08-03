using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;
using NetworkDeviceMonitor.DAL.Interfaces;
using NetworkDeviceMonitor.DAL.Services;
using NetworkDeviceMonitor.Domain.Models;
using Radzen;

namespace NetworkDeviceMonitor.DAL.BackgroundServices;

public class AutoscanBgService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    
    public AutoscanBgService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //_logger.LogInformation("Timed Hosted Service AutoScanBgService running.");
        return;
        List<Scan> scanList;

        using (IServiceScope scope = _serviceProvider.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            scanList = await uow.IScanRepository.GetActive();
        }
        
        while(!stoppingToken.IsCancellationRequested){
            foreach (var scan in scanList)
            {
                await new Task( async () =>
                {
                    // Calculate time until next execution from crontab schedule
                    var time = UntilNextExecution(CrontabSchedule.Parse(scan.CronSchedule).GetNextOccurrence(DateTime.Now));
                    
                    Console.WriteLine($"STARTED TASK FOR {scan.Network.Name}, WAITING FOR {time}");
                    await Task.Delay(time, stoppingToken); // wait until next time
                    
                    await DoWork(scan);
                }, stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private async Task DoWork(Scan scan)
    {
        using (IServiceScope scope = _serviceProvider.CreateScope())
        {
            var pingService = scope.ServiceProvider.GetRequiredService<PingService>();
            await pingService.ScanNetwork(scan.Network);
        }
        
        scan.FirstExecuted ??= DateTime.Now;
        scan.LastExecuted = DateTime.Now;
    }
    
    private int UntilNextExecution(DateTime _nextRun) => Math.Max(0, (int)_nextRun.Subtract(DateTime.Now).TotalMilliseconds);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}