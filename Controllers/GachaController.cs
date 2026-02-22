using Microsoft.AspNetCore.Mvc;
using TrickcalServer.GrainInterfaces;
using TrickcalServer.Models;

namespace TrickcalServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GachaController : ControllerBase
{
    private readonly IGrainFactory _grainFactory;

    public GachaController(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    /// <summary>
    /// 단일 뽑기
    /// </summary>
    [HttpPost("{bannerId}/pull")]
    public async Task<ActionResult<GachaResultDto>> Pull(int bannerId, [FromBody] GachaRequest request)
    {
        try
        {
            var grain = _grainFactory.GetGrain<IGachaGrain>(bannerId.ToString());
            var result = await grain.PullAsync(request.UserId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 10연차
    /// </summary>
    [HttpPost("{bannerId}/pull-ten")]
    public async Task<ActionResult<GachaTenResultDto>> PullTen(int bannerId, [FromBody] GachaRequest request)
    {
        try
        {
            var grain = _grainFactory.GetGrain<IGachaGrain>(bannerId.ToString());
            var result = await grain.PullTenAsync(request.UserId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 천장 교환
    /// </summary>
    [HttpPost("{bannerId}/exchange")]
    public async Task<ActionResult<GachaResultDto>> Exchange(int bannerId, [FromBody] GachaRequest request)
    {
        try
        {
            var grain = _grainFactory.GetGrain<IGachaGrain>(bannerId.ToString());
            var result = await grain.ExchangePickupAsync(request.UserId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 배너 확률 테이블 조회
    /// </summary>
    [HttpGet("{bannerId}/rates")]
    public async Task<ActionResult<GachaRateTable>> GetRates(int bannerId)
    {
        try
        {
            var grain = _grainFactory.GetGrain<IGachaTableGrain>(bannerId.ToString());
            var result = await grain.GetRatesAsync();
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 유저 신앙심 조회
    /// </summary>
    [HttpGet("user/{userId}/faith")]
    public async Task<ActionResult> GetFaith(string userId)
    {
        var grain = _grainFactory.GetGrain<IUserGrain>(userId);
        var faith = await grain.GetFaithAsync();
        return Ok(new { faith });
    }
}

public record GachaRequest(string UserId);
