namespace TrickcalServer.Models;

// ── Gacha DTOs ──

[GenerateSerializer]
public record GachaRateEntry
{
    [Id(0)] public int CardId { get; init; }
    [Id(1)] public string CardName { get; init; } = string.Empty;
    [Id(2)] public string Grade { get; init; } = string.Empty;
    [Id(3)] public decimal Rate { get; init; }
}

[GenerateSerializer]
public record GachaRateTable
{
    [Id(0)] public int BannerId { get; init; }
    [Id(1)] public string BannerName { get; init; } = string.Empty;
    [Id(2)] public List<GachaRateEntry> Rates { get; init; } = [];
}

[GenerateSerializer]
public record CardResultDto
{
    [Id(0)] public int Id { get; init; }
    [Id(1)] public string Name { get; init; } = string.Empty;
    [Id(2)] public string Grade { get; init; } = string.Empty;
}

[GenerateSerializer]
public record GachaResultDto
{
    [Id(0)] public CardResultDto Card { get; init; } = null!;
    [Id(1)] public int FaithAfter { get; init; }
}

[GenerateSerializer]
public record GachaTenResultDto
{
    [Id(0)] public List<CardResultDto> Cards { get; init; } = [];
    [Id(1)] public int FaithAfter { get; init; }
    [Id(2)] public bool Guaranteed { get; init; }
}

[GenerateSerializer]
public record UserCurrencyDto
{
    [Id(0)] public string UserId { get; init; } = string.Empty;
    [Id(1)] public int Eleaf { get; init; }
    [Id(2)] public int Gold { get; init; }
}

// ── Grain 상태 ──

[GenerateSerializer]
public class UserGrainState
{
    [Id(0)] public string UserId { get; set; } = string.Empty;
    [Id(1)] public int Eleaf { get; set; }
    [Id(2)] public int Gold { get; set; }
    [Id(3)] public int Faith { get; set; }
    [Id(4)] public List<UserCardState> Cards { get; set; } = [];
    [Id(5)] public bool IsInitialized { get; set; }
}

[GenerateSerializer]
public class UserCardState
{
    [Id(0)] public int CardId { get; set; }
    [Id(1)] public int Level { get; set; } = 1;
    [Id(2)] public int DuplicateCount { get; set; }
}
