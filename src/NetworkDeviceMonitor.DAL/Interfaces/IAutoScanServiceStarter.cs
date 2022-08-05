namespace NetworkDeviceMonitor.DAL.Interfaces;

public interface IAutoScanServiceStarter
{
    Task StartAsync(CancellationToken token = default);
}