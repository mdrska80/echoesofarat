using EchoesOfArat.Core.Models;
using Microsoft.Extensions.Logging;

namespace EchoesOfArat.Core.Services;

public class ExplorationService
{
    private readonly WorldState _worldState;
    private readonly ILogger<ExplorationService> _logger;
    // TODO: Inject NpcService, LocationService if needed for validation

    public ExplorationService(WorldState worldState, ILogger<ExplorationService> logger)
    {
        _worldState = worldState;
        _logger = logger;
        // Initialize the ActiveExpeditions list if it's null (e.g., after deserialization)
        _worldState.ActiveExpeditions ??= new List<ActiveExpedition>(); 
        _logger.LogInformation("ExplorationService initialized.");
    }

    /// <summary>
    /// Attempts to start a new expedition.
    /// </summary>
    /// <returns>True if successful, False otherwise (e.g., invalid NPCs/location).</returns>
    public bool StartExpedition(List<Guid> npcIds, Guid targetLocationId, ExpeditionStance stance)
    {
        _logger.LogInformation("Attempting to start expedition for NPCs: {NpcIds} to Location: {LocationId} with Stance: {Stance}", 
            string.Join(", ", npcIds), targetLocationId, stance);

        // --- Validation --- 
        if (npcIds == null || !npcIds.Any())
        {
            _logger.LogWarning("Cannot start expedition: No NPCs provided.");
            return false;
        }

        // TODO: Validate NPCs exist and are available (not already on expedition, not incapacitated)
        // foreach (var npcId in npcIds)
        // {
        //     var npc = _worldState.Npcs.FirstOrDefault(n => n.Id == npcId);
        //     if (npc == null || npc.CurrentActivity != NpcActivity.Idle) // Basic check
        //     {
        //          _logger.LogWarning("Cannot start expedition: NPC {NpcId} invalid or busy.", npcId);
        //          return false;
        //     }
        // }

        // TODO: Validate TargetLocationId exists (maybe check DiscoveredLocations? Or allow exploring unknown?)
        // if (!_worldState.DiscoveredLocations.ContainsKey(targetLocationId)) 
        // {
        //     _logger.LogWarning("Cannot start expedition: Target location {LocationId} not known.", targetLocationId);
        //     return false;
        // }

        // --- Create and Add Expedition --- 
        var newExpedition = new ActiveExpedition(npcIds, targetLocationId, stance)
        {
            StartTime = _worldState.CurrentGameTime // Use current game time
        };

        _worldState.ActiveExpeditions.Add(newExpedition);

        // --- Update NPC Status --- 
        // foreach (var npcId in npcIds)
        // {
        //     var npc = _worldState.Npcs.FirstOrDefault(n => n.Id == npcId);
        //     if (npc != null) npc.CurrentActivity = NpcActivity.Exploring;
        // }

        _logger.LogInformation("Expedition {ExpeditionId} started successfully.", newExpedition.Id);
        return true;
    }

    /// <summary>
    /// Called by TimeService to update progress of a specific expedition.
    /// </summary>
    public void UpdateExpeditionProgress(ActiveExpedition expedition, int hoursElapsed)
    {
        _logger.LogDebug("Updating progress for expedition {ExpeditionId} ({Hours}h elapsed)", expedition.Id, hoursElapsed);

        // TODO: Implement progress calculation based on:
        // - hoursElapsed
        // - expedition.Stance
        // - NPCs involved (skills, stats)
        // - TargetLocation difficulty/type
        // - Random factors

        // Example placeholder progress:
        double progressIncrement = hoursElapsed * 0.1; // Very basic
        expedition.Progress = Math.Clamp(expedition.Progress + progressIncrement, 0.0, 1.0);

        _logger.LogDebug("Expedition {ExpeditionId} progress: {Progress}", expedition.Id, expedition.Progress);

        // TODO: Check for encounter triggers based on progress, location, stance, RNG
        // CheckForEncounters(expedition);

        // TODO: Check for expedition completion
        if (expedition.Progress >= 1.0)
        {
            CompleteExpedition(expedition);
        }
    }

    private void CheckForEncounters(ActiveExpedition expedition)
    {
        _logger.LogDebug("Checking for encounters for expedition {ExpeditionId}...", expedition.Id);
        // Logic to determine if an encounter occurs
    }

    private void CompleteExpedition(ActiveExpedition expedition)
    {
        _logger.LogInformation("Expedition {ExpeditionId} completed.", expedition.Id);

        // TODO: Process results (loot, discoveries, XP)
        // TODO: Update NPC status back to Idle or Traveling back?
        // TODO: Update WorldState (e.g., add discovered items/lore)

        // Remove completed expedition
        _worldState.ActiveExpeditions.Remove(expedition);

        // Set NPCs back to idle (simplified for now)
        // foreach (var npcId in expedition.NpcIds)
        // {
        //     var npc = _worldState.Npcs.FirstOrDefault(n => n.Id == npcId);
        //     if (npc != null) npc.CurrentActivity = NpcActivity.Idle;
        // }
    }

    // Method to get active expeditions (for API maybe)
    public IEnumerable<ActiveExpedition> GetActiveExpeditions()
    {
        return _worldState.ActiveExpeditions;
    }
} 