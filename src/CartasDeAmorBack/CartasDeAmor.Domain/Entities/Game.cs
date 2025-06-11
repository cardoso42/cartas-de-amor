using System.Diagnostics.Contracts;
using CartasDeAmor.Domain.Enums;

namespace CartasDeAmor.Domain.Entities;

public class Game
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? Password { get; set; } = null;
    public bool PasswordProtected => !string.IsNullOrEmpty(Password);
    public required string HostEmail { get; set; }
    public IList<Player> Players { get; set; } = [];
    public ICollection<CardType> CardsDeck { get; set; } = [];
    public CardType ReservedCard { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
