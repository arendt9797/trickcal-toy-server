using Microsoft.EntityFrameworkCore;
using TrickcalServer.Data;
using TrickcalServer.GrainInterfaces;
using TrickcalServer.Models;

namespace TrickcalServer.Grains;

public class PlayerGrain : Grain, IPlayerGrain
{
    private readonly IServiceProvider _serviceProvider;
    private PlayerAccount? _player;

    public PlayerGrain(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _player = await db.Players.FindAsync([this.GetPrimaryKey()], cancellationToken);
    }

    public Task<PlayerInfoResponse> GetPlayerInfo()
    {
        if (_player is null)
            throw new InvalidOperationException("Player not found.");

        return Task.FromResult(MapToResponse(_player));
    }

    public async Task<PlayerInfoResponse> CreatePlayer(string nickname)
    {
        if (_player is not null)
            throw new InvalidOperationException("Player already exists.");

        _player = new PlayerAccount
        {
            Id = this.GetPrimaryKey(),
            Nickname = nickname
        };

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Players.Add(_player);
        await db.SaveChangesAsync();

        return MapToResponse(_player);
    }

    public async Task<bool> AddGold(long amount)
    {
        if (_player is null) return false;

        _player.Gold += amount;

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Players.Attach(_player);
        db.Entry(_player).Property(p => p.Gold).IsModified = true;
        await db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AddStamina(int amount)
    {
        if (_player is null) return false;

        _player.Stamina += amount;

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Players.Attach(_player);
        db.Entry(_player).Property(p => p.Stamina).IsModified = true;
        await db.SaveChangesAsync();

        return true;
    }

    private static PlayerInfoResponse MapToResponse(PlayerAccount player) => new()
    {
        Id = player.Id,
        Nickname = player.Nickname,
        Level = player.Level,
        Exp = player.Exp,
        Gold = player.Gold,
        Stamina = player.Stamina
    };
}
