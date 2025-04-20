namespace EchoesOfArat.Core.Models;

public class WorldState
{
    public DateTime CurrentGameTime { get; set; } = DateTime.UtcNow; // Start time
    public List<Npc> Npcs { get; set; } = new();

    /// <summary>
    /// Dictionary mapping runtime Location ID to Location object.
    /// Only includes locations the player has discovered/knows about.
    /// </summary>
    public Dictionary<Guid, Location> DiscoveredLocations { get; set; } = new();

    /// <summary>
    /// Dictionary mapping FactionType to Faction object containing current reputation.
    /// </summary>
    public Dictionary<FactionType, Faction> Factions { get; set; } = new();

    // TODO: Add ActiveExpeditions list later
    public List<ActiveExpedition> ActiveExpeditions { get; set; } = new();

    // Example method to add an NPC
    public void AddNpc(Npc npc)
    {
        if (!Npcs.Any(n => n.Id == npc.Id))
        {
            Npcs.Add(npc);
        }
    }

    // Method to add a discovered location
    public void AddDiscoveredLocation(Location location)
    {
        DiscoveredLocations.TryAdd(location.Id, location);
    }

    // Method to initialize or update faction state
    public void UpdateFaction(Faction faction)
    {
        Factions[faction.Type] = faction; // Add or update
    }
} 