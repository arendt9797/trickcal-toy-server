using TrickcalServer.Models;

namespace TrickcalServer.GrainInterfaces;

public interface IUserGrain : IGrainWithStringKey
{
    Task<UserCurrencyDto> GetCurrencyAsync();
    Task DeductEleafAsync(int amount);
    Task AddCardAsync(int cardId);
    Task<int> GetFaithAsync();
    Task AddFaithAsync(int amount);
    Task DeductFaithAsync(int amount);
}
