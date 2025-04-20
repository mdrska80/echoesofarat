using EchoesOfArat.Core.Models;
using Microsoft.Extensions.Logging;
using System.Linq; // Needed for ToList() to iterate safely while potentially modifying collection

namespace EchoesOfArat.Core.Services;

public class ExplorationService
{
    private readonly WorldState _worldState;
    private readonly ILogger<ExplorationService> _logger;
    private readonly List<Encounter> _staticEncounters; // Inject loaded encounters
    // TODO: Inject NpcService, LocationService if needed for validation

    public ExplorationService(WorldState worldState, ILogger<ExplorationService> logger, List<Encounter> staticEncounters)
    {
        _worldState = worldState;
        _logger = logger;
        _staticEncounters = staticEncounters;
        // Initialize the ActiveExpeditions list if it's null (e.g., after deserialization)
        _worldState.ActiveExpeditions ??= new List<ActiveExpedition>(); 
        _logger.LogInformation("ExplorationService initialized with {EncounterCount} static encounters.", _staticEncounters.Count);
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
    /// Updates all active expeditions for a given time step.
    /// Called by TimeService.
    /// </summary>
    /// <param name="hoursElapsed">Number of hours passed in this turn.</param>
    /// <param name="turnLogs">List to add log messages to.</param>
    public void UpdateExpeditions(int hoursElapsed, List<string> turnLogs)
    {
        _logger.LogDebug("Updating all active expeditions ({Count} total).", _worldState.ActiveExpeditions.Count);
        
        // Iterate over a copy in case expeditions complete and are removed during the loop
        foreach (var expedition in _worldState.ActiveExpeditions.ToList())
        {
            UpdateExpeditionProgress(expedition, hoursElapsed, turnLogs);
        }
    }

    /// <summary>
    /// Updates progress for a single expedition and handles potential encounters/completion.
    /// Now includes turnLogs parameter.
    /// </summary>
    public void UpdateExpeditionProgress(ActiveExpedition expedition, int hoursElapsed, List<string> turnLogs)
    {
        _logger.LogDebug("Updating progress for expedition {ExpeditionId} ({Hours}h elapsed)", expedition.Id, hoursElapsed);

        // TODO: Implement real progress calculation based on:
        // - hoursElapsed
        // - expedition.Stance
        // - NPCs involved (skills, stats)
        // - TargetLocation difficulty/type
        // - Random factors

        // Example placeholder progress:
        double progressIncrement = hoursElapsed * 0.1; // Very basic
        expedition.Progress = Math.Clamp(expedition.Progress + progressIncrement, 0.0, 1.0);

        string npcNames = string.Join(", ", expedition.NpcIds.Select(id => _worldState.Npcs.FirstOrDefault(n => n.Id == id)?.Name ?? "Unknown NPC"));
        string progressLog = $"[Expedition {expedition.Id.ToString().Substring(0, 4)} ({npcNames})]: Progress update. Now at {(expedition.Progress * 100):F0}%";
        _logger.LogDebug(progressLog); // Log detailed progress
        // turnLogs.Add(progressLog); // Optionally add detailed progress to turn log for client?

        // TODO: Check for encounter triggers based on progress, location, stance, RNG
        CheckForEncounters(expedition, turnLogs);

        // Check for expedition completion
        if (expedition.Progress >= 1.0)
        {
            CompleteExpedition(expedition, turnLogs);
        }
    }

    // Updated method signature
    private void CheckForEncounters(ActiveExpedition expedition, List<string> turnLogs)
    {
        _logger.LogDebug("Checking for encounters for expedition {ExpeditionId}...", expedition.Id);
        
        // Only check for encounters if there are static encounters loaded
        if (!_staticEncounters.Any()) return;

        // Simple placeholder: 30% chance of *any* encounter per update
        if (Random.Shared.NextDouble() < 0.30) 
        {
            // Select a random encounter from the loaded list
            int randomIndex = Random.Shared.Next(_staticEncounters.Count);
            Encounter triggeredEncounter = _staticEncounters[randomIndex];

            // Get involved NPC(s) for logging/effects
            var involvedNpcs = expedition.NpcIds
                .Select(id => _worldState.Npcs.FirstOrDefault(n => n.Id == id))
                .Where(npc => npc != null)
                .ToList();
            
            if (!involvedNpcs.Any()) return; // Should not happen if expedition is valid

            string npcNames = string.Join(", ", involvedNpcs.Select(n => n!.Name));
            string locationName = _worldState.DiscoveredLocations.GetValueOrDefault(expedition.TargetLocationId)?.Name ?? "Unknown Location";

            // Format log message
            string encounterLog = $"[Encounter @ {locationName} ({npcNames})]: {triggeredEncounter.Description}"; 
            
            _logger.LogInformation(encounterLog);
            turnLogs.Add(encounterLog); // Add encounter description to turn log for client

            // Apply simple effects (e.g., stress)
            if (triggeredEncounter.StressChange != 0)
            {
                foreach (var npc in involvedNpcs)
                {
                    npc!.CurrentMentalState.AddStress(triggeredEncounter.StressChange);
                    string stressLog = $"  -> {npc.Name} stress {(triggeredEncounter.StressChange > 0 ? "increased" : "decreased")} to {npc.CurrentMentalState.CurrentStress}.";
                    _logger.LogInformation(stressLog);
                    // Optionally add stress change details to turnLogs too?
                    // turnLogs.Add(stressLog);
                }
            }

            // TODO: Replace with actual encounter resolution logic 
            // - Filter encounters based on location, stance, NPC skills/traits
            // - Perform skill checks
            // - Grant rewards/XP/discoveries
            // - Update WorldState (new locations, items)
            // - Potentially pause expedition / require player input via API callback?
        }
    }

    // Updated method signature
    private void CompleteExpedition(ActiveExpedition expedition, List<string> turnLogs)
    {
        _logger.LogInformation("Expedition {ExpeditionId} completed.", expedition.Id);
        string npcNames = string.Join(", ", expedition.NpcIds.Select(id => _worldState.Npcs.FirstOrDefault(n => n.Id == id)?.Name ?? "Unknown NPC"));
        string completionLog = $"[Expedition {expedition.Id.ToString().Substring(0, 4)} ({npcNames})]: Expedition completed successfully!";
        turnLogs.Add(completionLog);

        // TODO: Process results (loot, discoveries, XP)
        // TODO: Update NPC status back to Idle or Traveling back?
        // TODO: Update WorldState (e.g., add discovered items/lore)

        // Remove completed expedition
        bool removed = _worldState.ActiveExpeditions.Remove(expedition);
        if(!removed) _logger.LogWarning("Failed to remove completed expedition {ExpeditionId}", expedition.Id);

        // Set NPCs back to idle (simplified for now)
        foreach (var npcId in expedition.NpcIds)
        {
            var npc = _worldState.Npcs.FirstOrDefault(n => n.Id == npcId);
            if (npc != null) npc.CurrentActivity = NpcActivity.Idle;
        }
    }

    // Method to get active expeditions (for API maybe)
    public IEnumerable<ActiveExpedition> GetActiveExpeditions()
    {
        return _worldState.ActiveExpeditions;
    }
} 