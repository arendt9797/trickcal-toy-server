namespace TrickcalServer.GrainInterfaces;

public interface IPlayerGrain : IGrainWithGuidKey
{
    Task<PlayerInfoResponse> GetPlayerInfo();
    Task<PlayerInfoResponse> CreatePlayer(string nickname);
    Task<bool> AddGold(long amount);
    Task<bool> AddStamina(int amount);
}

[GenerateSerializer]
public record PlayerInfoResponse
{
    [Id(0)] public Guid Id { get; init; }
    [Id(1)] public string Nickname { get; init; } = string.Empty;
    [Id(2)] public int Level { get; init; }
    [Id(3)] public long Exp { get; init; }
    [Id(4)] public long Gold { get; init; }
    [Id(5)] public int Stamina { get; init; }
}
