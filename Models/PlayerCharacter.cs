namespace TrickcalServer.Models;

/// <summary>
/// 플레이어가 보유한 캐릭터
/// </summary>
public class PlayerCharacter
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public string CharacterId { get; set; } = string.Empty;
    public int Star { get; set; } = 1;
    public int Level { get; set; } = 1;
    public long Exp { get; set; }
    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;

    public PlayerAccount Player { get; set; } = null!;
}
