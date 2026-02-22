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
        const decimal highTierTotalRate = 0.03m; // 3성 이상 전체

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

        // 3성 이상: 3% 분배
        var highTierCards = star3Cards.Concat(eldainCards).ToList();

        if (highTierCards.Count > 0)
        {
            var pickupRate = banner.PickupRate;
            var nonPickupCards = highTierCards.Where(c => c.Id != banner.PickupCardId).ToList();
            var remainingRate = highTierTotalRate - pickupRate;

            // 픽업 사도
            var pickupCard = highTierCards.FirstOrDefault(c => c.Id == banner.PickupCardId);
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

            // 나머지 3성 + 엘다인 균등 분배
            if (nonPickupCards.Count > 0)
            {
                var perCard = remainingRate / nonPickupCards.Count;
                foreach (var card in nonPickupCards)
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
