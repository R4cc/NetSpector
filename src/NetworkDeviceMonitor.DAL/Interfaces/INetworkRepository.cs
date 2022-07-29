using NetworkDeviceMonitor.Domain.Models;

namespace NetworkDeviceMonitor.DAL.Interfaces;

public interface INetworkRepository
{
    Task<List<Network>> GetAll();
    Task<Network> GetNetworkFromId(int networkId);
    Task Update(Network network);
    Task Remove(Network network);
}