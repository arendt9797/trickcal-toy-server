using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using TrickcalServer.Data;
using TrickcalServer.GrainInterfaces;
using TrickcalServer.Models;

namespace TrickcalServer.Grains;

public class GachaTableGrain : Grain, IGachaTableGrain
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    public GachaTableGrain(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<GachaRateTable> GetRatesAsync()
    {
        var bannerId = this.GetPrimaryKeyString();
        var cacheKey = $"gacha:banner:{bannerId}";

        using var scope = _serviceProvider.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

        // Redis 캐시 조회
        var cached = await cache.GetStringAsync(cacheKey);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<GachaRateTable>(cached)!;
        }

        // DB에서 배너 + 카드 데이터 조회
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var bannerIdInt = int.Parse(bannerId);
        var banner = await db.GachaBanners
            .Include(b => b.PickupCard)
            .FirstOrDefaultAsync(b => b.Id == bannerIdInt)
            ?? throw new InvalidOperationException($"배너를 찾을 수 없습니다. (ID: {bannerId})");

        var allCards = await db.Cards.ToListAsync();

        var rateTable = BuildRateTable(banner, allCards);

        // Redis에 캐싱
        var json = JsonSerializer.Serialize(rateTable);
        await cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheTtl
        });

        return rateTable;
    }

    private static GachaRateTable BuildRateTable(GachaBanner banner, List<Card> allCards)
    {
        var rates = new List<GachaRateEntry>();

        // 고정 확률
        const decimal star1TotalRate = 0.76m;
        const decimal star2TotalRate = 0.21m;

        var star1Cards = allCards.Where(c => c.Grade == CardGrade.Star1).ToList();
        var star2Cards = allCards.Where(c => c.Grade == CardGrade.Star2).ToList();
        var star3Cards = allCards.Where(c => c.Grade == CardGrade.Star3).ToList();
        var eldainCards = allCards.Where(c => c.Grade == CardGrade.Eldain).ToList();

        // 1성 균등 분배
        if (star1Cards.Count > 0)
        {
            var perCard = star1TotalRate / star1Cards.Count;
            foreach (var card in star1Cards)
            {
                rates.Add(new GachaRateEntry
                {
                    CardId = card.Id,
                    CardName = card.Name,
                    Grade = card.Grade.ToString(),
                    Rate = perCard
                });
            }
        }

        // 2성 균등 분배
        if (star2Cards.Count > 0)
        {
            var perCard = star2TotalRate / star2Cards.Count;
            foreach (var card in star2Cards)
            {
                rates.Add(new GachaRateEntry
                {
                    CardId = card.Id,
                    CardName = card.Name,
                    Grade = card.Grade.ToString(),
                    Rate = perCard
                });
            }
        }

        // 3성 이상: 3% 분배 (3성:엘다인 = 9:1 고정 비율)
        const decimal highTierTotalRate = 0.03m;
        const decimal star3Ratio = 0.9m;  // 2.7%
        const decimal eldainRatio = 0.1m; // 0.3%

        var highTierCards = star3Cards.Concat(eldainCards).ToList();
        if (highTierCards.Count > 0)
        {
            var pickupCard = highTierCards.FirstOrDefault(c => c.Id == banner.PickupCardId);
            var pickupRate = banner.PickupRate;

            // 픽업 사도
            if (pickupCard is not null)
            {
                rates.Add(new GachaRateEntry
                {
                    CardId = pickupCard.Id,
                    CardName = pickupCard.Name,
                    Grade = pickupCard.Grade.ToString(),
                    Rate = pickupRate
                });
            }

            // 픽업 제외 잔여 확률을 3성:엘다인 = 9:1 비율로 분배
            var remainingRate = highTierTotalRate - (pickupCard is not null ? pickupRate : 0m);
            var remainingStar3Rate = remainingRate * star3Ratio;
            var remainingEldainRate = remainingRate * eldainRatio;

            // 나머지 3성 카드 분배
            var nonPickupStar3Cards = star3Cards.Where(c => c.Id != banner.PickupCardId).ToList();
            if (nonPickupStar3Cards.Count > 0 && remainingStar3Rate > 0)
            {
                var perCard = remainingStar3Rate / nonPickupStar3Cards.Count;
                foreach (var card in nonPickupStar3Cards)
                {
                    rates.Add(new GachaRateEntry
                    {
                        CardId = card.Id,
                        CardName = card.Name,
                        Grade = card.Grade.ToString(),
                        Rate = perCard
                    });
                }
            }

            // 나머지 엘다인 카드 분배
            var nonPickupEldainCards = eldainCards.Where(c => c.Id != banner.PickupCardId).ToList();
            if (nonPickupEldainCards.Count > 0 && remainingEldainRate > 0)
            {
                var perCard = remainingEldainRate / nonPickupEldainCards.Count;
                foreach (var card in nonPickupEldainCards)
                {
                    rates.Add(new GachaRateEntry
                    {
                        CardId = card.Id,
                        CardName = card.Name,
                        Grade = card.Grade.ToString(),
                        Rate = perCard
                    });
                }
            }
        }

        return new GachaRateTable
        {
            BannerId = banner.Id,
            BannerName = banner.Name,
            Rates = rates
        };
    }
}
