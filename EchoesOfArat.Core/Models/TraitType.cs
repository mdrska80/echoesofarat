namespace EchoesOfArat.Core.Models;

/// <summary>
/// Defines potential positive and negative traits an NPC can possess.
/// Based on section 2.2.5 of game_concept.md.
/// </summary>
public enum TraitType
{
    // Positive Examples
    Optimistic,
    NaturalLeader,
    DesertAcclimatized,
    EagleEyed,
    Honest,
    NimbleFingers,
    IronStomach,

    // Negative Examples
    Greedy,
    Pessimistic,
    Superstitious,
    Clumsy,
    Lazy,
    HotHeaded,

    // Trauma-Related (May link to MentalState or be separate)
    PhobiaUndead,
    PhobiaConfinedSpaces,
    PhobiaDarkness,
    Paranoia,
    Apathy,
    Obsession,
    NightTerrors,
    // Add more specific phobias/traumas as needed
} 