namespace EchoesOfArat.Core.Models;

// Placeholder for Expedition Stance enum
public enum ExpeditionStance 
{
    CautiousExploration,
    AggressiveSalvage,
    FocusedResearch,
    ResourceGathering
}

/// <summary>
/// Represents an expedition currently in progress.
/// </summary>
public class ActiveExpedition
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public List<Guid> NpcIds { get; init; } = new();
    public Guid TargetLocationId { get; init; }
    public ExpeditionStance Stance { get; set; }
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
    public double Progress { get; set; } = 0.0; // e.g., 0.0 to 1.0 for completion?
    // Add EstimatedDuration, CurrentPhase, EncounterQueue etc. later

    public ActiveExpedition(List<Guid> npcIds, Guid targetLocationId, ExpeditionStance stance)
    {
        NpcIds = npcIds;
        TargetLocationId = targetLocationId;
        Stance = stance;
    }
} 