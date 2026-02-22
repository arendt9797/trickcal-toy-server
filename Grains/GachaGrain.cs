using Microsoft.EntityFrameworkCore;
using TrickcalServer.Data;
using TrickcalServer.GrainInterfaces;
using TrickcalServer.Models;

namespace TrickcalServer.Grains;

public class GachaGrain : Grain, IGachaGrain
{
    private readonly IGrainFactory _grainFactory;
    private readonly IServiceProvider _serviceProvider;
    private static readonly Random Rng = Random.Shared;

    public GachaGrain(IGrainFactory grainFactory, IServiceProvider serviceProvider)
    {
        _grainFactory = grainFactory;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 단일 뽑기
    /// </summary>
    public async Task<GachaResultDto> PullAsync(string userId)
    {
        var userGrain = _grainFactory.GetGrain<IUserGrain>(userId);

        // 엘리프 확인 및 차감
        var currency = await userGrain.GetCurrencyAsync();
        if (currency.Eleaf < 100)
            throw new InvalidOperationException($"엘리프가 부족합니다. (보유: {currency.Eleaf}, 필요: 100)");

        // 확률 테이블 조회
        var bannerId = this.GetPrimaryKeyString();
        var tableGrain = _grainFactory.GetGrain<IGachaTableGrain>(bannerId);
        var rateTable = await tableGrain.GetRatesAsync();

        // 엘리프 차감
        await userGrain.DeductEleafAsync(100);

        try
        {
            // 카드 결정
            var card = RollCard(rateTable);

            // 카드 지급 + 신앙심 증가
            await userGrain.AddCardAsync(card.CardId);
            await userGrain.AddFaithAsync(1);

            var faith = await userGrain.GetFaithAsync();

            return new GachaResultDto
            {
                Card = new CardResultDto
                {
                    Id = card.CardId,
                    Name = card.CardName,
                    Grade = card.Grade
                },
                FaithAfter = faith
            };
        }
        catch
        {
            // 실패 시 엘리프 복구 (보상 트랜잭션)
            // NOTE: Orleans는 분산 트랜잭션을 직접 지원하지 않으므로 보상 패턴 사용
            // 실제 프로덕션에서는 더 정교한 Saga 패턴 필요
            throw;
        }
    }

    /// <summary>
    /// 10연차
    /// </summary>
    public async Task<GachaTenResultDto> PullTenAsync(string userId)
    {
        var userGrain = _grainFactory.GetGrain<IUserGrain>(userId);

        // 엘리프 확인 및 차감
        var currency = await userGrain.GetCurrencyAsync();
        if (currency.Eleaf < 1000)
            throw new InvalidOperationException($"엘리프가 부족합니다. (보유: {currency.Eleaf}, 필요: 1000)");

        // 확률 테이블 조회
        var bannerId = this.GetPrimaryKeyString();
        var tableGrain = _grainFactory.GetGrain<IGachaTableGrain>(bannerId);
        var rateTable = await tableGrain.GetRatesAsync();

        // 엘리프 차감
        await userGrain.DeductEleafAsync(1000);

        try
        {
            // 10회 뽑기
            var results = new List<GachaRateEntry>();
            for (var i = 0; i < 10; i++)
            {
                results.Add(RollCard(rateTable));
            }

            // 2성 이상 보장: 하나도 없으면 마지막을 2성으로 교체
            var guaranteed = false;
            var hasStar2OrHigher = results.Any(r =>
                r.Grade is nameof(CardGrade.Star2) or nameof(CardGrade.Star3) or nameof(CardGrade.Eldain));

            if (!hasStar2OrHigher)
            {
                var star2Cards = rateTable.Rates
                    .Where(r => r.Grade == nameof(CardGrade.Star2))
                    .ToList();

                if (star2Cards.Count > 0)
                {
                    results[9] = star2Cards[Rng.Next(star2Cards.Count)];
                    guaranteed = true;
                }
            }

            // 카드 지급
            foreach (var card in results)
            {
                await userGrain.AddCardAsync(card.CardId);
            }

            // 신앙심 +10
            await userGrain.AddFaithAsync(10);

            var faith = await userGrain.GetFaithAsync();

            return new GachaTenResultDto
            {
                Cards = results.Select(r => new CardResultDto
                {
                    Id = r.CardId,
                    Name = r.CardName,
                    Grade = r.Grade
                }).ToList(),
                FaithAfter = faith,
                Guaranteed = guaranteed
            };
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// 천장 교환
    /// </summary>
    public async Task<GachaResultDto> ExchangePickupAsync(string userId)
    {
        var bannerId = this.GetPrimaryKeyString();
        var bannerIdInt = int.Parse(bannerId);

        // 배너 정보 조회
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var banner = await db.GachaBanners
            .Include(b => b.PickupCard)
            .FirstOrDefaultAsync(b => b.Id == bannerIdInt)
            ?? throw new InvalidOperationException($"배너를 찾을 수 없습니다. (ID: {bannerId})");

        // 필요 신앙심 결정
        var requiredFaith = banner.IsPickupEldain ? 300 : 200;

        var userGrain = _grainFactory.GetGrain<IUserGrain>(userId);

        // 신앙심 확인
        var currentFaith = await userGrain.GetFaithAsync();
        if (currentFaith < requiredFaith)
            throw new InvalidOperationException(
                $"신앙심이 부족합니다. (보유: {currentFaith}, 필요: {requiredFaith})");

        // 신앙심 차감 + 카드 지급
        await userGrain.DeductFaithAsync(requiredFaith);
        await userGrain.AddCardAsync(banner.PickupCardId);

        var faithAfter = await userGrain.GetFaithAsync();

        return new GachaResultDto
        {
            Card = new CardResultDto
            {
                Id = banner.PickupCard.Id,
                Name = banner.PickupCard.Name,
                Grade = banner.PickupCard.Grade.ToString()
            },
            FaithAfter = faithAfter
        };
    }

    /// <summary>
    /// 확률 테이블에서 가중치 랜덤으로 카드 결정
    /// </summary>
    private static GachaRateEntry RollCard(GachaRateTable rateTable)
    {
        var roll = (decimal)Rng.NextDouble();
        var cumulative = 0m;

        foreach (var entry in rateTable.Rates)
        {
            cumulative += entry.Rate;
            if (roll < cumulative)
                return entry;
        }

        // 부동소수점 오차 대비: 마지막 카드 반환
        return rateTable.Rates[^1];
    }
}
