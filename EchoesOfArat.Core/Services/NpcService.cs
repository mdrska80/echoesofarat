using EchoesOfArat.Core.Models;
using Microsoft.Extensions.Logging;

namespace EchoesOfArat.Core.Services;

public class NpcService
{
    private readonly WorldState _worldState;
    private readonly ILogger<NpcService> _logger;

    public NpcService(WorldState worldState, ILogger<NpcService> logger)
    {
        _worldState = worldState;
        _logger = logger;
        _logger.LogInformation("NpcService initialized.");
    }

    // Basic retrieval methods
    public Npc? GetNpcById(Guid id)
    {
        _logger.LogDebug("Attempting to get NPC with ID: {NpcId}", id);
        var npc = _worldState.Npcs.FirstOrDefault(n => n.Id == id);
        if (npc == null)
        {
            _logger.LogWarning("NPC with ID {NpcId} not found.", id);
        }
        return npc;
    }

    public IEnumerable<Npc> GetAllNpcs()
    {
        _logger.LogDebug("Getting all NPCs. Count: {NpcCount}", _worldState.Npcs.Count);
        return _worldState.Npcs;
    }

    // Placeholder for adding a new NPC (can be expanded)
    public void AddNewNpc(string name)
    {
        Guid placeholderLocationId = Guid.Empty; // Default location
        var newNpc = new Npc(Guid.NewGuid(), name, placeholderLocationId);
        _worldState.AddNpc(newNpc); // Use method on WorldState
        _logger.LogInformation("Added new NPC: {NpcName} with ID: {NpcId}", name, newNpc.Id);
    }

    // TODO: Add methods for NPC updates, AI ticks, skill checks etc. based on game logic
} 