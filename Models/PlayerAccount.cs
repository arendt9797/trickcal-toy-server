namespace TrickcalServer.Models;

/// <summary>
/// 플레이어 계정 정보
/// </summary>
public class PlayerAccount
{
    public Guid Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public long Exp { get; set; }
    public long Gold { get; set; }
    public int Stamina { get; set; } = 120;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    public ICollection<PlayerCharacter> Characters { get; set; } = new List<PlayerCharacter>();
}
