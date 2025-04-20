namespace EchoesOfArat.Core.Models;

/// <summary>
/// Represents the mental state of an NPC, including stress and trauma.
/// Based on section 2.2.4 of game_concept.md.
/// </summary>
public class MentalState
{
    /// <summary>
    /// Current stress level (e.g., 0-100).
    /// Temporary penalties might apply at high levels.
    /// </summary>
    public int CurrentStress { get; set; } = 0;

    /// <summary>
    /// List of permanent trauma-induced traits affecting the NPC.
    /// These typically correspond to specific negative TraitType values.
    /// </summary>
    public List<TraitType> TraumaEffects { get; private set; } = new();

    // Methods to add/remove stress, potentially add trauma effects
    public void AddStress(int amount)
    {
        CurrentStress = Math.Clamp(CurrentStress + amount, 0, 100); // Example max stress
        // TODO: Check thresholds for negative effects or potential trauma gain
    }

    public void ReduceStress(int amount)
    {
        CurrentStress = Math.Clamp(CurrentStress - amount, 0, 100);
    }

    public bool AddTrauma(TraitType traumaTrait)
    {
        // Check if the trait is actually a trauma-related one if needed
        if (!TraumaEffects.Contains(traumaTrait))
        {
            TraumaEffects.Add(traumaTrait);
            return true;
        }
        return false;
    }

    // Potentially add methods for mitigating trauma later
} 