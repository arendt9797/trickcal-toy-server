using TrickcalServer.Models;

namespace TrickcalServer.GrainInterfaces;

public interface IGachaGrain : IGrainWithStringKey
{
    Task<GachaResultDto> PullAsync(string userId);
    Task<GachaTenResultDto> PullTenAsync(string userId);
    Task<GachaResultDto> ExchangePickupAsync(string userId);
}
