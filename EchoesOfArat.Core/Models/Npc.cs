namespace EchoesOfArat.Core.Models;

/// <summary>
/// Represents a Non-Player Character in the game.
/// Combining definitions from game_concept.md section 2.2 onwards.
/// </summary>
public class Npc // Changed to class to easily accommodate complex state
{
    public Guid Id { get; init; }
    public string Name { get; set; } // Name might change?

    public Stats CurrentStats { get; set; }
    public Inventory CurrentInventory { get; set; }
    public MentalState CurrentMentalState { get; set; }

    /// <summary>
    /// NPC's skills and their current level/XP.
    /// Key: SkillType, Value: Current Level (or XP total).
    /// </summary>
    public Dictionary<SkillType, int> Skills { get; set; } = new();

    /// <summary>
    /// Inherent positive and negative traits (excluding trauma-induced ones).
    /// </summary>
    public List<TraitType> BaseTraits { get; set; } = new();

    /// <summary>
    /// Calculated list combining BaseTraits and TraumaEffects from MentalState.
    /// </summary>
    public IEnumerable<TraitType> AllTraits => BaseTraits.Concat(CurrentMentalState.TraumaEffects).Distinct();

    /// <summary>
    /// NPC's long-term loyalty to the player/expedition (e.g., 0-100).
    /// </summary>
    public int Loyalty { get; set; } = 50; // Default starting loyalty

    /// <summary>
    /// NPC's short-term morale (e.g., 0-100).
    /// </summary>
    public int Morale { get; set; } = 50; // Default starting morale

    /// <summary>
    /// ID of the Location the NPC is currently at.
    /// Might be Base Camp ID or an exploration location ID.
    /// </summary>
    public Guid CurrentLocationId { get; set; } // Needs a default, maybe Base Camp Guid?

    /// <summary>
    /// What the NPC is currently doing.
    /// </summary>
    public NpcActivity CurrentActivity { get; set; } = NpcActivity.Idle;

    // TODO: Add Personal Goals (string? dedicated class?)
    // public string? PersonalGoal { get; set; }

    // Constructor to initialize required components
    public Npc(Guid id, string name, Guid startingLocationId, Stats? baseStats = null)
    {
        Id = id;
        Name = name;
        CurrentLocationId = startingLocationId;
        CurrentStats = baseStats ?? new Stats();
        CurrentInventory = new Inventory(CurrentStats.MaxCarryCapacity); // Link inventory capacity to stats
        CurrentMentalState = new MentalState();
        // Initialize default skills/traits based on archetype later?
    }

    // Private parameterless constructor for deserialization if needed
    // Making it public for System.Text.Json
    public Npc() 
    {
        // Initialize non-nullable reference types to satisfy warnings
        // Use default values or 'null!' if appropriate for deserialization context
        Name = string.Empty;
        CurrentStats = new Stats(); // Assume default stats are okay
        CurrentInventory = new Inventory(0); // Capacity will be overwritten by stats
        CurrentMentalState = new MentalState();
        // Value types like Guid, int, enums have default values already.
    }
}

/// <summary>
/// Defines the possible activities an NPC can be engaged in.
/// </summary>
public enum NpcActivity
{
    Idle,
    Exploring,
    WorkingInBase, // Specify task?
    Recovering, // Resting, healing
    Traveling
} 