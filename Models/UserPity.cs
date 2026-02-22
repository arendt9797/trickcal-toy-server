namespace TrickcalServer.Models;

/// <summary>
/// 유저 신앙심 (천장 시스템)
/// </summary>
public class UserPity
{
    public string UserId { get; set; } = string.Empty;
    public int Faith { get; set; }
}
