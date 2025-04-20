using EchoesOfArat.Core.Models;
using Microsoft.Extensions.Logging; // Required for ILogger
using System.IO;                  // Required for Path, Directory, File
using System.Text.Json;           // Required for JsonSerializer
using System.Threading.Tasks;     // Required for Task
using System.Linq;                // Required for Linq methods like Select, Where

namespace EchoesOfArat.Core.Persistence;

public class JsonGameStateRepository : IGameStateRepository
{
    private readonly string _saveGameDirectory;
    private readonly ILogger<JsonGameStateRepository> _logger; // Add logger

    // Inject ILogger for logging
    public JsonGameStateRepository(ILogger<JsonGameStateRepository> logger)
    {
        _logger = logger;
        // Determine save location robustly
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _saveGameDirectory = Path.Combine(appData, "EchoesOfArat", "Saves");
        try
        {
            Directory.CreateDirectory(_saveGameDirectory);
            _logger.LogInformation("Save directory ensured: {SavePath}", _saveGameDirectory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create save directory: {SavePath}", _saveGameDirectory);
            // Handle critical error - perhaps throw or fallback?
        }
    }

    public async Task SaveAsync(WorldState worldState, string slotName = "default")
    {
        // Sanitize slotName to prevent directory traversal issues
        slotName = Path.GetFileName(slotName); 
        string filePath = Path.Combine(_saveGameDirectory, $"{slotName}.json");
        try
        {
            _logger.LogInformation("Saving game state to slot '{Slot}' at {FilePath}", slotName, filePath);
            string json = JsonSerializer.Serialize(worldState, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
            _logger.LogInformation("Game state saved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save game state to {FilePath}", filePath);
            // Potentially throw a custom exception
        }
    }

    public async Task<WorldState?> LoadAsync(string slotName = "default")
    {
        // Sanitize slotName
        slotName = Path.GetFileName(slotName); 
        string filePath = Path.Combine(_saveGameDirectory, $"{slotName}.json");
        _logger.LogInformation("Attempting to load game state from slot '{Slot}' at {FilePath}", slotName, filePath);

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Save file not found: {FilePath}", filePath);
            return null; // Or throw, or return new game state
        }
        try
        {
            string json = await File.ReadAllTextAsync(filePath);
            var loadedState = JsonSerializer.Deserialize<WorldState>(json);
            _logger.LogInformation("Game state loaded successfully.");
            return loadedState;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load or deserialize game state from {FilePath}", filePath);
            return null; // Or throw custom exception
        }
    }

    public Task<List<string>> ListSaveSlotsAsync()
    {
        try
        {
            var files = Directory.GetFiles(_saveGameDirectory, "*.json")
                               .Select(Path.GetFileNameWithoutExtension)
                               .Where(name => !string.IsNullOrEmpty(name)) // Ensure not null or empty
                               .Select(name => name!) // Assert non-null here after Where clause
                               .ToList();
            _logger.LogInformation("Found save slots: {Slots}", string.Join(", ", files));
            return Task.FromResult(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list save slots in {SavePath}", _saveGameDirectory);
            return Task.FromResult(new List<string>()); // Return empty list on error
        }
    }
} 