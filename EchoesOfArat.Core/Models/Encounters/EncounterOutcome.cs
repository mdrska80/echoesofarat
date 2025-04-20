using EchoesOfArat.Core.Models;
using System.Collections.Generic;

namespace EchoesOfArat.Core.Models.Encounters;

/// <summary>
/// Defines the result of taking a specific choice in an encounter.
/// </summary>
public record EncounterOutcome(
    string Description, // Text describing the outcome
    List<Reward>? Rewards = null, // Optional rewards
    List<Consequence>? Consequences = null // Optional consequences
);

/// <summary>
/// Base class/interface for different reward types (e.g., Item, XP, Discovery).
/// Using a simple string ID for items for now.
/// </summary>
public record Reward(RewardType Type, string Value, int Amount = 1);

/// <summary>
/// Base class/interface for different consequence types (e.g., Stress, ItemLoss, Injury).
/// </summary>
public record Consequence(ConsequenceType Type, int Amount = 0);

public enum RewardType { Item, Xp, Discovery, Reputation }
public enum ConsequenceType { Stress, ItemLoss, Injury, Reputation } 