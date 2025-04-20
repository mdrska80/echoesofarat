using EchoesOfArat.Core.Models;

namespace EchoesOfArat.Core.Models.Encounters;

/// <summary>
/// Defines information for a skill check within an encounter.
/// </summary>
public record SkillCheckInfo(
    SkillType Skill, // The skill being checked
    int DifficultyClass // The target number to beat (e.g., roll d20 + skill bonus)
); 