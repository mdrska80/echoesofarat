using EchoesOfArat.Core.Models;
// using Microsoft.Extensions.Logging; // Removed logger dependency for now
using System.Text.Json;
using System.Text.Json.Serialization; // Required for JsonStringEnumConverter
using System.IO;

namespace EchoesOfArat.Core.Data;

/// <summary>
/// Helper class to load static game data definitions from files.
/// Consider making this non-static and using DI if complex dependencies arise.
/// </summary>
public static class GameDataLoader
{
    // Simple internal record to match the locations.json structure
    private record StaticLocationData(
        string StaticId,
        string Name,
        string Description,
        LocationType Type,
        List<string> ConnectedLocationStaticIds
    );

    // Define shared options for deserialization
    private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() } // Handle enums as strings
    };

    public static List<Item> LoadItems(string filePath = "Data/items.json")
    {
        var fullPath = Path.GetFullPath(filePath);
        // logger?.LogInformation("Attempting to load items from: {FilePath}", fullPath);
        Console.WriteLine($"Attempting to load items from: {fullPath}"); // Use Console for basic logging
        try
        {
            string json = File.ReadAllText(fullPath);
            // Use shared options
            var items = JsonSerializer.Deserialize<List<Item>>(json, jsonOptions);
            // logger?.LogInformation("Loaded {ItemCount} items successfully.", items?.Count ?? 0);
            Console.WriteLine($"Loaded {items?.Count ?? 0} items successfully.");
            return items ?? new List<Item>();
        }
        catch (Exception ex)
        {
            // logger?.LogError(ex, "Error loading items from {FilePath}", fullPath);
            Console.WriteLine($"[ERROR] Error loading items from {fullPath}: {ex.Message}");
            return new List<Item>(); // Return empty list on error
        }
    }

    public static Dictionary<string, Location> LoadLocations(string filePath = "Data/locations.json")
    {
        var fullPath = Path.GetFullPath(filePath);
        // logger?.LogInformation("Attempting to load locations from: {FilePath}", fullPath);
        Console.WriteLine($"Attempting to load locations from: {fullPath}");
        var runtimeLocations = new Dictionary<string, Location>();
        try
        {
            string json = File.ReadAllText(fullPath);
            // Use shared options
            var staticDataList = JsonSerializer.Deserialize<List<StaticLocationData>>(json, jsonOptions);

            if (staticDataList == null) {
                // logger?.LogError("Failed to deserialize location data from {FilePath}. Result was null.", fullPath);
                Console.WriteLine($"[ERROR] Failed to deserialize location data from {fullPath}. Result was null.");
                return runtimeLocations; // Return empty
            }

            var staticDataMap = staticDataList.ToDictionary(s => s.StaticId, s => s);

            // First pass: Create runtime Location objects with runtime Guids
            var guidMap = new Dictionary<string, Guid>(); // Map static ID to runtime Guid
            foreach (var staticData in staticDataList)
            {
                var runtimeId = Guid.NewGuid();
                guidMap[staticData.StaticId] = runtimeId;
                var runtimeLocation = new Location(
                    runtimeId,
                    staticData.StaticId,
                    staticData.Name,
                    staticData.Description,
                    staticData.Type,
                    new List<Guid>() // Connections resolved in second pass
                );
                runtimeLocations[staticData.StaticId] = runtimeLocation;
            }

            // Second pass: Resolve connections using the guidMap
            foreach (var kvp in runtimeLocations)
            {
                var staticId = kvp.Key;
                var runtimeLocation = kvp.Value;
                var staticData = staticDataMap[staticId]; // Get original static data
                
                foreach (var connectedStaticId in staticData.ConnectedLocationStaticIds ?? new List<string>())
                {
                    if (guidMap.TryGetValue(connectedStaticId, out var connectedGuid))
                    {
                        runtimeLocation.ConnectedLocationIds.Add(connectedGuid);
                    }
                    else
                    {
                        // logger?.LogWarning("Location '{LocationId}' references unknown connected location '{ConnectedLocationId}'", 
                        //     staticId, connectedStaticId);
                        Console.WriteLine($"[WARN] Location '{staticId}' references unknown connected location '{connectedStaticId}'");
                    }
                }
            }

            // logger?.LogInformation("Loaded and processed {LocationCount} locations successfully.", runtimeLocations.Count);
            Console.WriteLine($"Loaded and processed {runtimeLocations.Count} locations successfully.");
            return runtimeLocations; // Return map of StaticID -> Runtime Location
        }
        catch (Exception ex)
        {
            // logger?.LogError(ex, "Error loading locations from {FilePath}", fullPath);
            Console.WriteLine($"[ERROR] Error loading locations from {fullPath}: {ex.Message}");
            return new Dictionary<string, Location>(); // Return empty dictionary on error
        }
    }

    // Added method to load encounters
    public static List<Encounter> LoadEncounters(string filePath = "Data/encounters.json")
    {
        var fullPath = Path.GetFullPath(filePath);
        Console.WriteLine($"Attempting to load encounters from: {fullPath}");
        try
        {
            string json = File.ReadAllText(fullPath);
            // Use shared options
            var encounters = JsonSerializer.Deserialize<List<Encounter>>(json, jsonOptions);
            Console.WriteLine($"Loaded {encounters?.Count ?? 0} encounters successfully.");
            return encounters ?? new List<Encounter>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error loading encounters from {fullPath}: {ex.Message}");
            return new List<Encounter>(); // Return empty list on error
        }
    }

    // TODO: Add LoadFactions, LoadSkills, LoadTraits etc.
} 