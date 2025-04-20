using EchoesOfArat.Core.Models;
using Microsoft.Extensions.Logging;

namespace EchoesOfArat.Core.Services;

public class TimeService
{
    private readonly WorldState _worldState;
    private readonly ILogger<TimeService> _logger;
    private readonly ExplorationService _explorationService;
    // TODO: Inject other services needed for time-based updates (e.g., NpcNeedsService)

    public TimeService(WorldState worldState, ILogger<TimeService> logger, ExplorationService explorationService)
    {
        _worldState = worldState;
        _logger = logger;
        _explorationService = explorationService;
        _logger.LogInformation("TimeService initialized.");
    }

    public void AdvanceTurn(int hours = 1) // Allow advancing multiple hours?
    {
        if (hours <= 0) return;

        _worldState.ClearTurnLogs(); // Clear logs from previous turn

        var previousTime = _worldState.CurrentGameTime;
        _worldState.CurrentGameTime = _worldState.CurrentGameTime.AddHours(hours);
        _logger.LogInformation("Advanced game time from {PreviousTime} to {CurrentTime} ({Hours}h)", 
            previousTime, _worldState.CurrentGameTime, hours);

        // --- Trigger Time-Based Updates --- 
        // TODO: Update NPC needs (hunger, thirst, rest)
        // UpdateNeeds(hours);

        // Update active expeditions
        _explorationService.UpdateExpeditions(hours, _worldState.CurrentTurnLogs);

        // TODO: Update base camp activities/production
        // UpdateBaseCamp(hours);

        // TODO: Trigger random events?
        // CheckForRandomEvents();

        // TODO: Update world state based on events (e.g., faction shifts?)

    }

    // Placeholder methods for sub-updates
    private void UpdateNeeds(int hours)
    {
        _logger.LogDebug("Updating NPC needs...");
        // foreach npc... reduce stats based on activity/time
    }

    private void CheckForRandomEvents()
    {
        _logger.LogDebug("Checking for random events...");
        // Roll dice, check conditions, potentially add event to queue
    }
} 