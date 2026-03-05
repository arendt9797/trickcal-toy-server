namespace TrickcalServer.Models;

/// <summary>
/// 가챠 배너
/// </summary>
public class GachaBanner
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int PickupCardId { get; set; }
    public bool IsPickupEldain { get; set; }
    public decimal PickupRate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Card PickupCard { get; set; } = null!;
}
