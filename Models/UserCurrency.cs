namespace TrickcalServer.Models;

/// <summary>
/// 유저 재화
/// </summary>
public class UserCurrency
{
    public string UserId { get; set; } = string.Empty;
    public int Eleaf { get; set; }
    public int Gold { get; set; }
}
