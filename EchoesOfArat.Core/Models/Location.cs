namespace EchoesOfArat.Core.Models;

public record Location(
    Guid Id, // Unique identifier for runtime instances 
    string StaticId, // Identifier linking to static data (e.g., from JSON)
    string Name,
    string Description,
    LocationType Type, // e.g., Oasis, Ruin, Settlement
    List<Guid> ConnectedLocationIds // IDs of directly reachable locations
    // Add properties for resources, potential encounters, faction control etc. later
);

public enum LocationType
{
    Generic,
    Settlement,
    Oasis,
    RuinKhemri,
    RuinPrecursorElder,
    RuinPrecursorSerpent,
    RuinPrecursorStarWeaver,
    RuinPrecursorChronomancer,
    Wadi,
    Swamp,
    Quarry,
    Shrine,
    Outpost,
    Farmland,
    Encampment
} 