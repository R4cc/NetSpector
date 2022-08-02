using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;
using NetworkDeviceMonitor.DAL.Interfaces;
using NetworkDeviceMonitor.DAL.Services;
using NetworkDeviceMonitor.Domain.Models;

namespace NetworkDeviceMonitor.DAL.BackgroundServices;

public class AutoscanBgService : BackgroundService
{
    //private readonly IUnitOfWork _uow; 
    //private readonly PingService _pingService; 
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutoscanBgService> _logger;
    //private Timer? _timer = null;
    
    public AutoscanBgService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //_logger.LogInformation("Timed Hosted Service AutoScanBgService running.");

        List<Scan> scanList;
        List<Scan> scanningList = new ();

        using (IServiceScope scope = _serviceProvider.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            scanList = await uow.IScanRepository.GetActive();
        }

        foreach (var scan in scanList.Where(s => !scanningList.Contains(s)))
        {
            Task.Factory.StartNew(async () =>
            {
                scanningList.Add(scan);
                Task.Delay(
                    UntilNextExecution(CrontabSchedule.Parse(scan.CronSchedule).GetNextOccurrence(DateTime.Now)),
                    stoppingToken); // wait until next time
                await DoWork(scan);
            }, stoppingToken);
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