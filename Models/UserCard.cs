namespace TrickcalServer.Models;

/// <summary>
/// 유저 보유 사도
/// </summary>
public class UserCard
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int CardId { get; set; }
    public int Level { get; set; } = 1;
    public int DuplicateCount { get; set; }

    public Card Card { get; set; } = null!;
}
