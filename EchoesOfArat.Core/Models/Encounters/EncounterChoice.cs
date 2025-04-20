using System.Collections.Generic;

namespace EchoesOfArat.Core.Models.Encounters;

/// <summary>
/// Represents a single choice or action available within an encounter.
/// </summary>
public record EncounterChoice(
    // Required parameters first
    string Id, 
    string Text, 
    EncounterOutcome SuccessOutcome, 
    // Optional parameters last
    SkillCheckInfo? SkillCheck = null, 
    EncounterOutcome? FailureOutcome = null 
); 