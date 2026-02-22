namespace TrickcalServer.Models;

/// <summary>
/// 사도 (캐릭터 원본 데이터)
/// </summary>
public class Card
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CardGrade Grade { get; set; }
    public string Tribe { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public int BaseAtk { get; set; }
    public int BaseDef { get; set; }
}
