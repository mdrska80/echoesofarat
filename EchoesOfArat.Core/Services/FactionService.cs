using EchoesOfArat.Core.Models;
using Microsoft.Extensions.Logging;

namespace EchoesOfArat.Core.Services;

public class FactionService
{
    private readonly WorldState _worldState;
    private readonly ILogger<FactionService> _logger;

    public FactionService(WorldState worldState, ILogger<FactionService> logger)
    {
        _worldState = worldState;
        _logger = logger;
        // Initialize faction dictionary if needed (e.g., after deserialization)
        _worldState.Factions ??= new Dictionary<FactionType, Faction>();
        _logger.LogInformation("FactionService initialized.");
        // Ensure all factions from enum exist in the state? Or load from static data?
    }

    public Faction? GetFaction(FactionType type)
    {
        if (_worldState.Factions.TryGetValue(type, out var faction))
        {
            return faction;
        }
        _logger.LogWarning("Attempted to get unknown or uninitialized faction: {FactionType}", type);
        return null;
    }

    public IEnumerable<Faction> GetAllFactions()
    {
        return _worldState.Factions.Values;
    }

    /// <summary>
    /// Modifies the reputation with a faction.
    /// </summary>
    /// <param name="type">The faction type.</param>
    /// <param name="changeAmount">The amount to change reputation by (can be negative).</param>
    public void ChangeReputation(FactionType type, int changeAmount)
    {
        if (_worldState.Factions.TryGetValue(type, out var faction))
        {
            int oldRep = faction.Reputation;
            // Clamp reputation within bounds (e.g., -100 to 100)
            faction.Reputation = Math.Clamp(faction.Reputation + changeAmount, -100, 100); 
            _logger.LogInformation("Changed reputation for {FactionType} from {OldRep} to {NewRep} (Change: {Change})",
                type, oldRep, faction.Reputation, changeAmount);
        }
        else
        {
            _logger.LogWarning("Cannot change reputation for unknown or uninitialized faction: {FactionType}", type);
        }
    }

    // TODO: Add methods for checking reputation levels, triggering faction events, etc.
} 