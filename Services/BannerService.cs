using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using TrickcalServer.Data;
using TrickcalServer.Models;

namespace TrickcalServer.Services;

public class BannerService(AppDbContext db, IDistributedCache cache) : IBannerService
{
    public async Task<BannerDto> CreateBannerAsync(CreateBannerRequest request)
    {
        // 카드 존재 검증
        var card = await db.Cards.FindAsync(request.PickupCardId)
            ?? throw new InvalidBannerConfigException($"카드를 찾을 수 없습니다. (ID: {request.PickupCardId})");

        // IsPickupEldain과 카드 등급 일치 검증
        var actuallyEldain = card.Grade == CardGrade.Eldain;
        if (request.IsPickupEldain != actuallyEldain)
            throw new InvalidBannerConfigException(
                $"IsPickupEldain 값이 카드 등급과 일치하지 않습니다. (카드 등급: {card.Grade}, 입력값: {request.IsPickupEldain})");

        // PickupRate 범위 검증 (0 초과, 0.03 미만)
        if (request.PickupRate <= 0m || request.PickupRate >= 0.03m)
            throw new InvalidBannerConfigException(
                $"PickupRate는 0 초과 0.03 미만이어야 합니다. (입력값: {request.PickupRate})");

        // 날짜 검증
        if (request.StartDate >= request.EndDate)
            throw new InvalidBannerConfigException("StartDate는 EndDate보다 이전이어야 합니다.");

        var now = DateTime.UtcNow;
        var banner = new GachaBanner
        {
            Name = request.Name,
            Description = request.Description,
            PickupCardId = request.PickupCardId,
            IsPickupEldain = request.IsPickupEldain,
            PickupRate = request.PickupRate,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.GachaBanners.Add(banner);
        await db.SaveChangesAsync();

        banner.PickupCard = card;
        return ToDto(banner);
    }

    public async Task<List<BannerDto>> GetActiveBannersAsync()
    {
        var now = DateTime.UtcNow;
        var banners = await db.GachaBanners
            .Include(b => b.PickupCard)
            .Where(b => b.IsActive && b.StartDate <= now && b.EndDate >= now)
            .ToListAsync();

        return banners.Select(ToDto).ToList();
    }

    public async Task<BannerDto?> GetBannerByIdAsync(int bannerId)
    {
        var banner = await db.GachaBanners
            .Include(b => b.PickupCard)
            .FirstOrDefaultAsync(b => b.Id == bannerId);

        return banner is null ? null : ToDto(banner);
    }

    public async Task<BannerDto> UpdateBannerAsync(int bannerId, UpdateBannerRequest request)
    {
        var banner = await db.GachaBanners
            .Include(b => b.PickupCard)
            .FirstOrDefaultAsync(b => b.Id == bannerId)
            ?? throw new BannerNotFoundException(bannerId);

        // PickupCardId 변경 시 카드 확인 + IsPickupEldain 자동 갱신
        if (request.PickupCardId.HasValue)
        {
            var card = await db.Cards.FindAsync(request.PickupCardId.Value)
                ?? throw new InvalidBannerConfigException($"카드를 찾을 수 없습니다. (ID: {request.PickupCardId.Value})");

            banner.PickupCardId = card.Id;
            banner.IsPickupEldain = card.Grade == CardGrade.Eldain;
            banner.PickupCard = card;
        }

        // PickupRate 검증
        if (request.PickupRate.HasValue)
        {
            if (request.PickupRate.Value <= 0m || request.PickupRate.Value >= 0.03m)
                throw new InvalidBannerConfigException(
                    $"PickupRate는 0 초과 0.03 미만이어야 합니다. (입력값: {request.PickupRate.Value})");

            banner.PickupRate = request.PickupRate.Value;
        }

        // 날짜 변경 시 최종 조합으로 검증
        var finalStart = request.StartDate ?? banner.StartDate;
        var finalEnd = request.EndDate ?? banner.EndDate;
        if (request.StartDate.HasValue || request.EndDate.HasValue)
        {
            if (finalStart >= finalEnd)
                throw new InvalidBannerConfigException("StartDate는 EndDate보다 이전이어야 합니다.");

            banner.StartDate = finalStart;
            banner.EndDate = finalEnd;
        }

        if (request.Name is not null) banner.Name = request.Name;
        if (request.Description is not null) banner.Description = request.Description;
        if (request.IsActive.HasValue) banner.IsActive = request.IsActive.Value;

        banner.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await cache.RemoveAsync($"gacha:banner:{bannerId}");

        return ToDto(banner);
    }

    public async Task DeactivateBannerAsync(int bannerId)
    {
        var banner = await db.GachaBanners.FindAsync(bannerId)
            ?? throw new BannerNotFoundException(bannerId);

        banner.IsActive = false;
        banner.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await cache.RemoveAsync($"gacha:banner:{bannerId}");
    }

    private static BannerDto ToDto(GachaBanner banner) => new()
    {
        Id = banner.Id,
        Name = banner.Name,
        Description = banner.Description,
        PickupCard = new CardSummaryDto
        {
            Id = banner.PickupCard.Id,
            Name = banner.PickupCard.Name,
            Grade = banner.PickupCard.Grade.ToString()
        },
        PickupRate = banner.PickupRate,
        StartDate = banner.StartDate,
        EndDate = banner.EndDate,
        IsActive = banner.IsActive
    };
}
