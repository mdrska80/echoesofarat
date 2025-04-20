namespace EchoesOfArat.Core.Models;

/// <summary>
/// Represents the player's relationship and knowledge about a specific faction.
/// </summary>
public class Faction // Class as reputation will change
{
    public FactionType Type { get; init; }
    public string Name { get; init; } // Loaded from static data

    /// <summary>
    /// Player's reputation with this faction (e.g., -100 to 100).
    /// </summary>
    public int Reputation { get; set; } = 0; // Default neutral

    // Add more properties later: KnownLeaders, Goals, Allies/Rivals etc.

    public Faction(FactionType type, string name)
    {
        Type = type;
        Name = name;
    }
} 