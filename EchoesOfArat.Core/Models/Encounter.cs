namespace EchoesOfArat.Core.Models;

/// <summary>
/// Represents a static definition of an encounter.
/// </summary>
public record Encounter(
    string Id, // Unique ID for this encounter definition
    string Description, // Text describing the encounter outcome
    EncounterType Type = EncounterType.Generic, // Optional type for filtering later
    int StressChange = 0 // Optional: Amount of stress added/removed
    // Add properties for rewards, skill checks, choices later
);

public enum EncounterType
{
    Generic,
    Discovery,
    Hazard,
    Social
} 