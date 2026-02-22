using TrickcalServer.Models;

namespace TrickcalServer.GrainInterfaces;

public interface IGachaTableGrain : IGrainWithStringKey
{
    Task<GachaRateTable> GetRatesAsync();
}
