using TrickcalServer.Models;

namespace TrickcalServer.Services;

public interface IBannerService
{
    Task<BannerDto> CreateBannerAsync(CreateBannerRequest request);
    Task<List<BannerDto>> GetActiveBannersAsync();
    Task<BannerDto?> GetBannerByIdAsync(int bannerId);
    Task<BannerDto> UpdateBannerAsync(int bannerId, UpdateBannerRequest request);
    Task DeactivateBannerAsync(int bannerId);
}
