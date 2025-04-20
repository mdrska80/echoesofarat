namespace EchoesOfArat.Core.Models;

/// <summary>
/// Represents the core physical statistics of an NPC.
/// Using record for potential immutability benefits if needed, but could be class.
/// </summary>
public record Stats
{
    // Consider making these mutable if they change frequently, 
    // or use separate Current/Max properties.
    public int MaxHealth { get; init; } = 100;
    public int CurrentHealth { get; set; } = 100;

    public int MaxStamina { get; init; } = 100;
    public int CurrentStamina { get; set; } = 100;

    public double MaxCarryCapacity { get; init; } = 50.0; // Example value in weight units

    public Stats()
    {
        CurrentHealth = MaxHealth;
        CurrentStamina = MaxStamina;
    }
} 