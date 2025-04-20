namespace EchoesOfArat.Core.Models;

/// <summary>
/// Defines the different skills NPCs can possess.
/// Based on section 2.2.1 of game_concept.md.
/// </summary>
public enum SkillType
{
    // Core Exploration & Interaction
    Archaeology,
    Survival,
    CombatMelee, // Specify Melee/Ranged
    CombatRanged,
    Negotiation,
    Cartography,
    Medicine,
    Stealth,
    Observation,
    Labor,

    // Advanced Knowledge & Crafting
    LinguisticsEpigraphy,
    TrapsmithingMechanics,
    TheologyRitualism,
    EngineeringPrecursorTech,
    AlloyAnalysisStarMetalLore,

    // Social & Command
    LeadershipCommand,
    FactionLoreDiplomacy,

    // Environmental & Specialized
    AnimalHandlingTaming,
    PsychicResistanceSensitivity,
    HydrologyAquaticAdaptation,
    TemporalSenseAnomalyNavigation,
} 