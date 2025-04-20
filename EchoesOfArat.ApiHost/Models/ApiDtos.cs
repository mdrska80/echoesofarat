namespace EchoesOfArat.ApiHost.Models;

using EchoesOfArat.Core.Models; // Needed for ExpeditionStance

public record SaveGameRequest(string? SlotName);

public record StartExpeditionRequest(List<Guid> NpcIds, Guid TargetLocationId, ExpeditionStance Stance); 