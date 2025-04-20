using EchoesOfArat.Core.Models.Encounters; // Use the new namespace
using System.Collections.Generic;

namespace EchoesOfArat.Core.Models; // Keep this in the main Models namespace

/// <summary>
/// Represents a richer definition of an encounter, including choices and outcomes.
/// </summary>
public record Encounter(
    string Id, // Unique ID for this encounter definition (e.g., "discovery_encounter_1")
    string Title, // Display title for the encounter
    EncounterType Type, // Type for filtering/categorization
    string InitialDescription, // Text presented when the encounter starts
    List<EncounterChoice> Choices // List of available choices/actions
    // Add Trigger conditions later (e.g., List<LocationType>, Probability)
);

// Keep EncounterType enum here or move to separate file
public enum EncounterType
{
    Generic,
    Discovery,
    Hazard,
    Social
} 