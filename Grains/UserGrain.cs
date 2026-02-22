using Microsoft.EntityFrameworkCore;
using Orleans.Runtime;
using TrickcalServer.Data;
using TrickcalServer.GrainInterfaces;
using TrickcalServer.Models;

namespace TrickcalServer.Grains;

public class UserGrain : Grain, IUserGrain
{
    private readonly IPersistentState<UserGrainState> _state;
    private readonly IServiceProvider _serviceProvider;

    public UserGrain(
        [PersistentState("user", "Default")] IPersistentState<UserGrainState> state,
        IServiceProvider serviceProvider)
    {
        _state = state;
        _serviceProvider = serviceProvider;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        if (!_state.State.IsInitialized)
        {
            await LoadFromDatabaseAsync(cancellationToken);
        }
    }

    private async Task LoadFromDatabaseAsync(CancellationToken ct)
    {
        var userId = this.GetPrimaryKeyString();

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var currency = await db.UserCurrencies.FindAsync([userId], ct);
        var pity = await db.UserPities.FindAsync([userId], ct);
        var cards = await db.UserCards
            .Where(c => c.UserId == userId)
            .ToListAsync(ct);

        if (currency is null)
        {
            // 신규 유저: DB에 기본 레코드 생성
            currency = new UserCurrency { UserId = userId, Eleaf = 0, Gold = 0 };
            db.UserCurrencies.Add(currency);

            pity = new UserPity { UserId = userId, Faith = 0 };
            db.UserPities.Add(pity);

            await db.SaveChangesAsync(ct);
        }

        _state.State.UserId = userId;
        _state.State.Eleaf = currency.Eleaf;
        _state.State.Gold = currency.Gold;
        _state.State.Faith = pity?.Faith ?? 0;
        _state.State.Cards = cards.Select(c => new UserCardState
        {
            CardId = c.CardId,
            Level = c.Level,
            DuplicateCount = c.DuplicateCount
        }).ToList();
        _state.State.IsInitialized = true;

        await _state.WriteStateAsync();
    }

    public Task<UserCurrencyDto> GetCurrencyAsync()
    {
        return Task.FromResult(new UserCurrencyDto
        {
            UserId = _state.State.UserId,
            Eleaf = _state.State.Eleaf,
            Gold = _state.State.Gold
        });
    }

    public async Task DeductEleafAsync(int amount)
    {
        if (_state.State.Eleaf < amount)
            throw new InvalidOperationException($"엘리프가 부족합니다. (보유: {_state.State.Eleaf}, 필요: {amount})");

        _state.State.Eleaf -= amount;

        await PersistCurrencyAsync();
        await _state.WriteStateAsync();
    }

    public async Task AddCardAsync(int cardId)
    {
        var existing = _state.State.Cards.FirstOrDefault(c => c.CardId == cardId);

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (existing is not null)
        {
            existing.DuplicateCount++;

            var dbCard = await db.UserCards
                .FirstOrDefaultAsync(c => c.UserId == _state.State.UserId && c.CardId == cardId);
            if (dbCard is not null)
            {
                dbCard.DuplicateCount = existing.DuplicateCount;
                await db.SaveChangesAsync();
            }
        }
        else
        {
            var newCard = new UserCardState { CardId = cardId, Level = 1, DuplicateCount = 0 };
            _state.State.Cards.Add(newCard);

            db.UserCards.Add(new UserCard
            {
                UserId = _state.State.UserId,
                CardId = cardId,
                Level = 1,
                DuplicateCount = 0
            });
            await db.SaveChangesAsync();
        }

        await _state.WriteStateAsync();
    }

    public Task<int> GetFaithAsync()
    {
        return Task.FromResult(_state.State.Faith);
    }

    public async Task AddFaithAsync(int amount)
    {
        _state.State.Faith += amount;

        await PersistPityAsync();
        await _state.WriteStateAsync();
    }

    public async Task DeductFaithAsync(int amount)
    {
        if (_state.State.Faith < amount)
            throw new InvalidOperationException($"신앙심이 부족합니다. (보유: {_state.State.Faith}, 필요: {amount})");

        _state.State.Faith -= amount;

        await PersistPityAsync();
        await _state.WriteStateAsync();
    }

    private async Task PersistCurrencyAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var currency = await db.UserCurrencies.FindAsync(_state.State.UserId);
        if (currency is not null)
        {
            currency.Eleaf = _state.State.Eleaf;
            currency.Gold = _state.State.Gold;
            await db.SaveChangesAsync();
        }
    }

    private async Task PersistPityAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pity = await db.UserPities.FindAsync(_state.State.UserId);
        if (pity is not null)
        {
            pity.Faith = _state.State.Faith;
            await db.SaveChangesAsync();
        }
    }
}
