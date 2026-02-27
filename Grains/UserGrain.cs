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
    private bool _isDirty = false;
    private IGrainTimer? _flushTimer;

    public UserGrain(
        [PersistentState("user", "Default")] IPersistentState<UserGrainState> state,
        IServiceProvider serviceProvider)
    {
        _state = state;
        _serviceProvider = serviceProvider;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        // Redis에 데이터 없으면 DB에서 로드 (신규 유저 or Redis 유실)
        if (string.IsNullOrEmpty(_state.State.UserId))
        {
            await LoadFromDatabaseAsync(cancellationToken);
        }

        // 30초마다 DB에 변경사항 flush
        _flushTimer = this.RegisterGrainTimer(
            FlushToDatabaseAsync,
            new GrainTimerCreationOptions
            {
                DueTime = TimeSpan.FromSeconds(30),
                Period = TimeSpan.FromSeconds(30)
            });
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        // Grain 내려갈 때 미flush 데이터 보장
        if (_isDirty)
            await FlushToDatabaseAsync(cancellationToken);
    }

    private async Task LoadFromDatabaseAsync(CancellationToken ct)
    {
        var userId = this.GetPrimaryKeyString();

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var currency = await db.UserCurrencies.FindAsync([userId], ct);
        var cards = await db.UserCards
            .Where(c => c.UserId == userId)
            .ToListAsync(ct);

        if (currency is null)
        {
            // 신규 유저: DB에 기본 레코드 생성
            currency = new UserCurrency { UserId = userId, Eleaf = 0, Gold = 0, Faith = 0 };
            db.UserCurrencies.Add(currency);
            await db.SaveChangesAsync(ct);
        }

        _state.State.UserId = userId;
        _state.State.Eleaf = currency.Eleaf;
        _state.State.Gold = currency.Gold;
        _state.State.Faith = currency.Faith;
        _state.State.Cards = cards.Select(c => new UserCardState
        {
            CardId = c.CardId,
            Level = c.Level,
            DuplicateCount = c.DuplicateCount
        }).ToList();

        await _state.WriteStateAsync();
    }

    // Write-Behind: 타이머 또는 Grain 종료 시 DB 동기화
    private async Task FlushToDatabaseAsync(CancellationToken ct)
    {
        if (!_isDirty) return;

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Currency upsert
        var currency = await db.UserCurrencies.FindAsync([_state.State.UserId], ct);
        if (currency is null)
        {
            db.UserCurrencies.Add(new UserCurrency
            {
                UserId = _state.State.UserId,
                Eleaf = _state.State.Eleaf,
                Gold = _state.State.Gold,
                Faith = _state.State.Faith
            });
        }
        else
        {
            currency.Eleaf = _state.State.Eleaf;
            currency.Gold = _state.State.Gold;
            currency.Faith = _state.State.Faith;
        }

        // Cards 전체 스냅샷 upsert
        foreach (var card in _state.State.Cards)
        {
            var dbCard = await db.UserCards
                .FirstOrDefaultAsync(c => c.UserId == _state.State.UserId && c.CardId == card.CardId, ct);

            if (dbCard is null)
            {
                db.UserCards.Add(new UserCard
                {
                    UserId = _state.State.UserId,
                    CardId = card.CardId,
                    Level = card.Level,
                    DuplicateCount = card.DuplicateCount
                });
            }
            else
            {
                dbCard.Level = card.Level;
                dbCard.DuplicateCount = card.DuplicateCount;
            }
        }

        await db.SaveChangesAsync(ct);
        _isDirty = false;
    }

    public Task<UserCurrencyDto> GetCurrencyAsync()
    {
        return Task.FromResult(new UserCurrencyDto
        {
            UserId = _state.State.UserId,
            Eleaf = _state.State.Eleaf,
            Gold = _state.State.Gold,
            Faith = _state.State.Faith
        });
    }

    public async Task DeductEleafAsync(int amount)
    {
        if (_state.State.Eleaf < amount)
            throw new InvalidOperationException($"엘리프가 부족합니다. (보유: {_state.State.Eleaf}, 필요: {amount})");

        _state.State.Eleaf -= amount;
        _isDirty = true;
        await _state.WriteStateAsync();
    }

    public async Task AddCardAsync(int cardId)
    {
        var existing = _state.State.Cards.FirstOrDefault(c => c.CardId == cardId);

        if (existing is not null)
            existing.DuplicateCount++;
        else
            _state.State.Cards.Add(new UserCardState { CardId = cardId, Level = 1, DuplicateCount = 0 });

        _isDirty = true;
        await _state.WriteStateAsync();
    }

    public Task<int> GetFaithAsync()
    {
        return Task.FromResult(_state.State.Faith);
    }

    public async Task AddFaithAsync(int amount)
    {
        _state.State.Faith += amount;
        _isDirty = true;
        await _state.WriteStateAsync();
    }

    public async Task DeductFaithAsync(int amount)
    {
        if (_state.State.Faith < amount)
            throw new InvalidOperationException($"신앙심이 부족합니다. (보유: {_state.State.Faith}, 필요: {amount})");

        _state.State.Faith -= amount;
        _isDirty = true;
        await _state.WriteStateAsync();
    }
}
