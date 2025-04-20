using EchoesOfArat.Core.Models;

namespace EchoesOfArat.Core.Persistence;

public interface IGameStateRepository
{
    Task SaveAsync(WorldState worldState, string slotName = "default");
    Task<WorldState?> LoadAsync(string slotName = "default");
    Task<List<string>> ListSaveSlotsAsync();
} 