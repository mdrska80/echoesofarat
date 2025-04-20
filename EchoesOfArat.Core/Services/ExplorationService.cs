using EchoesOfArat.Core.Models;
using EchoesOfArat.Core.Models.Encounters;
using Microsoft.Extensions.Logging;
using System.Linq; // Needed for ToList() to iterate safely while potentially modifying collection

namespace EchoesOfArat.Core.Services;

public class ExplorationService
{
    private readonly WorldState _worldState;
    private readonly ILogger<ExplorationService> _logger;
    private readonly List<Encounter> _staticEncounters; // Inject loaded encounters
    private readonly List<Item> _staticItems; // Inject static items for rewards
    // TODO: Inject NpcService, LocationService if needed for validation

    public ExplorationService(WorldState worldState, ILogger<ExplorationService> logger, List<Encounter> staticEncounters, List<Item> staticItems)
    {
        _worldState = worldState;
        _logger = logger;
        _staticEncounters = staticEncounters;
        _staticItems = staticItems;
        // Initialize the ActiveExpeditions list if it's null (e.g., after deserialization)
        _worldState.ActiveExpeditions ??= new List<ActiveExpedition>(); 
        _logger.LogInformation("ExplorationService initialized with {EncounterCount} detailed encounters and {ItemCount} static items.", 
            _staticEncounters.Count, _staticItems.Count);
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

    private void CheckForEncounters(ActiveExpedition expedition, List<string> turnLogs)
    {
        _logger.LogDebug("Checking for encounters for expedition {ExpeditionId}...", expedition.Id);
        
        if (!_staticEncounters.Any()) return;

        // Simple placeholder: Increased chance for testing
        if (Random.Shared.NextDouble() < 0.50) 
        {
            int randomIndex = Random.Shared.Next(_staticEncounters.Count);
            Encounter triggeredEncounter = _staticEncounters[randomIndex];

            var involvedNpcs = expedition.NpcIds
                .Select(id => _worldState.Npcs.FirstOrDefault(n => n.Id == id))
                .Where(npc => npc != null)
                .ToList();
            
            if (!involvedNpcs.Any()) return; 

            // --- Log Encounter Start --- 
            string npcNames = string.Join(", ", involvedNpcs.Select(n => n!.Name));
            string locationName = _worldState.DiscoveredLocations.GetValueOrDefault(expedition.TargetLocationId)?.Name ?? "Unknown Location";
            string encounterStartLog = $"[Encounter @ {locationName} ({npcNames})]: {triggeredEncounter.Title} - {triggeredEncounter.InitialDescription}";
            _logger.LogInformation(encounterStartLog);
            turnLogs.Add(encounterStartLog);

            // --- Simulate Choice & Resolution (NO PLAYER INPUT YET) ---
            // Simple simulation: Always try the first choice available
            EncounterChoice chosenChoice = triggeredEncounter.Choices.FirstOrDefault();
            if (chosenChoice == null)
            {
                _logger.LogWarning("Encounter {EncounterId} has no choices defined.", triggeredEncounter.Id);
                return; // No choice to make
            }

            string choiceLog = $"  -> Simulating choice: '{chosenChoice.Text}'";
            _logger.LogInformation(choiceLog);
            turnLogs.Add(choiceLog);

            EncounterOutcome finalOutcome;
            bool skillCheckPassed = true; // Assume success if no check

            // Simulate Skill Check if present
            if (chosenChoice.SkillCheck != null)
            {
                // Use the first available NPC for the check for simplicity
                Npc checkingNpc = involvedNpcs.First()!;
                SkillCheckInfo checkInfo = chosenChoice.SkillCheck;
                
                // TODO: Implement actual skill check logic (roll dice, add bonuses)
                int npcSkillLevel = checkingNpc.Skills.GetValueOrDefault(checkInfo.Skill, 0); // Get skill level (default 0)
                int roll = Random.Shared.Next(1, 21); // Simulate d20 roll
                int totalResult = roll + npcSkillLevel; // Simple check, no bonuses yet
                skillCheckPassed = totalResult >= checkInfo.DifficultyClass;

                string checkLog = $"    (Skill Check: {checkingNpc.Name} attempts {checkInfo.Skill} DC {checkInfo.DifficultyClass}. Roll {roll} + Skill {npcSkillLevel} = {totalResult} -> {(skillCheckPassed ? "Success!" : "Failure!")})";
                _logger.LogInformation(checkLog);
                turnLogs.Add(checkLog);
            }

            // Determine the outcome based on skill check
            if (skillCheckPassed)
            {
                finalOutcome = chosenChoice.SuccessOutcome;
            }
            else
            {
                // Use failure outcome if it exists, otherwise maybe default success?
                // Or maybe failure means nothing happens? Let's use FailureOutcome if present.
                finalOutcome = chosenChoice.FailureOutcome ?? chosenChoice.SuccessOutcome; 
            }

            // --- Apply Outcome --- 
            _logger.LogInformation("    Outcome: {OutcomeDescription}", finalOutcome.Description);
            turnLogs.Add($"    Outcome: {finalOutcome.Description}");

            // Apply Consequences
            if (finalOutcome.Consequences != null)
            {
                foreach (var consequence in finalOutcome.Consequences)
                {
                    ApplyConsequence(involvedNpcs, consequence, turnLogs);
                }
            }

            // Apply Rewards
            if (finalOutcome.Rewards != null)
            {
                foreach (var reward in finalOutcome.Rewards)
                {
                    ApplyReward(involvedNpcs, reward, turnLogs);
                }
            }
        }
    }

    // Helper to apply consequences
    private void ApplyConsequence(List<Npc?> involvedNpcs, Consequence consequence, List<string> turnLogs)
    {
        _logger.LogDebug("Applying consequence: {Type} Amount: {Amount}", consequence.Type, consequence.Amount);
        switch (consequence.Type)
        {
            case ConsequenceType.Stress:
                foreach (var npc in involvedNpcs)
                {
                    if (npc == null) continue;
                    int oldStress = npc.CurrentMentalState.CurrentStress;
                    npc.CurrentMentalState.AddStress(consequence.Amount);
                    string log = $"      -> {npc.Name} stress changed from {oldStress} to {npc.CurrentMentalState.CurrentStress} ({consequence.Amount:+0;-#})";
                    _logger.LogInformation(log);
                    turnLogs.Add(log);
                }
                break;
            case ConsequenceType.ItemLoss:
                // TODO: Implement item loss logic
                _logger.LogWarning("ItemLoss consequence not implemented yet.");
                break;
            case ConsequenceType.Injury:
                // TODO: Implement injury logic
                _logger.LogWarning("Injury consequence not implemented yet.");
                break;
            case ConsequenceType.Reputation:
                // TODO: Implement reputation consequence
                 _logger.LogWarning("Reputation consequence not implemented yet.");
                 break;
        }
    }

    // Helper to apply rewards
    private void ApplyReward(List<Npc?> involvedNpcs, Reward reward, List<string> turnLogs)
    {
         _logger.LogDebug("Applying reward: {Type} Value: {Value} Amount: {Amount}", reward.Type, reward.Value, reward.Amount);
         switch (reward.Type)
         {
             case RewardType.Item:
                 Item? itemToAdd = _staticItems.FirstOrDefault(item => item.Id == reward.Value);
                 if (itemToAdd != null)
                 {   
                     // Give item to the first NPC for simplicity
                     Npc? receivingNpc = involvedNpcs.FirstOrDefault();
                     if (receivingNpc != null) {
                         for(int i=0; i < reward.Amount; ++i) { // Add correct amount
                             bool added = receivingNpc.CurrentInventory.TryAddItem(itemToAdd);
                             string log = $"      -> {receivingNpc.Name} received {itemToAdd.Name}. {(added ? "" : "(Inventory full!)")}";
                             _logger.LogInformation(log);
                             turnLogs.Add(log);
                         }
                     } else { _logger.LogWarning("No valid NPC found to receive item reward: {ItemId}", itemToAdd.Id); }
                 } 
                 else { _logger.LogWarning("Could not find static item for reward: {ItemId}", reward.Value); }
                 break;
            case RewardType.Xp:
                // TODO: Implement XP reward
                _logger.LogWarning("XP reward not implemented yet.");
                break;
            case RewardType.Discovery:
                // TODO: Implement discovery reward (e.g., new location, lore)
                _logger.LogWarning("Discovery reward not implemented yet.");
                break;
             case RewardType.Reputation:
                 // TODO: Implement reputation reward
                 _logger.LogWarning("Reputation reward not implemented yet.");
                 break;
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