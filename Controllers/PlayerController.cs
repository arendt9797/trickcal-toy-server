using Microsoft.AspNetCore.Mvc;
using TrickcalServer.GrainInterfaces;

namespace TrickcalServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly IGrainFactory _grainFactory;

    public PlayerController(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    [HttpPost("create")]
    public async Task<ActionResult<PlayerInfoResponse>> CreatePlayer([FromBody] CreatePlayerRequest request)
    {
        var playerId = Guid.NewGuid();
        var grain = _grainFactory.GetGrain<IPlayerGrain>(playerId);
        var result = await grain.CreatePlayer(request.Nickname);
        return Ok(result);
    }

    [HttpGet("{playerId:guid}")]
    public async Task<ActionResult<PlayerInfoResponse>> GetPlayer(Guid playerId)
    {
        var grain = _grainFactory.GetGrain<IPlayerGrain>(playerId);
        try
        {
            var result = await grain.GetPlayerInfo();
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost("{playerId:guid}/gold")]
    public async Task<ActionResult> AddGold(Guid playerId, [FromBody] AddGoldRequest request)
    {
        var grain = _grainFactory.GetGrain<IPlayerGrain>(playerId);
        var success = await grain.AddGold(request.Amount);
        return success ? Ok() : NotFound();
    }

    [HttpPost("{playerId:guid}/stamina")]
    public async Task<ActionResult> AddStamina(Guid playerId, [FromBody] AddStaminaRequest request)
    {
        var grain = _grainFactory.GetGrain<IPlayerGrain>(playerId);
        var success = await grain.AddStamina(request.Amount);
        return success ? Ok() : NotFound();
    }
}

public record CreatePlayerRequest(string Nickname);
public record AddGoldRequest(long Amount);
public record AddStaminaRequest(int Amount);
