using Microsoft.AspNetCore.Mvc;
using TrickcalServer.Models;
using TrickcalServer.Services;

namespace TrickcalServer.Controllers;

[ApiController]
[Route("api/banners")]
public class BannerController(IBannerService bannerService) : ControllerBase
{
    /// <summary>
    /// 배너 생성
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BannerDto>> Create([FromBody] CreateBannerRequest request)
    {
        var banner = await bannerService.CreateBannerAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = banner.Id }, banner);
    }

    /// <summary>
    /// 활성 배너 목록 조회
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<BannerDto>>> GetActive()
    {
        var banners = await bannerService.GetActiveBannersAsync();
        return Ok(banners);
    }

    /// <summary>
    /// 배너 상세 조회
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BannerDto>> GetById(int id)
    {
        var banner = await bannerService.GetBannerByIdAsync(id);
        return banner is null ? NotFound(new { error = $"배너를 찾을 수 없습니다. (ID: {id})" }) : Ok(banner);
    }

    /// <summary>
    /// 배너 수정 (partial update)
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<ActionResult<BannerDto>> Update(int id, [FromBody] UpdateBannerRequest request)
    {
        var banner = await bannerService.UpdateBannerAsync(id, request);
        return Ok(banner);
    }

    /// <summary>
    /// 배너 비활성화 (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await bannerService.DeactivateBannerAsync(id);
        return NoContent();
    }
}
