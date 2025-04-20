namespace EchoesOfArat.Core.Models;

// Use string Id for items defined in static data (e.g., JSON)
public record Item(
    string Id, 
    string Name,
    string Description,
    double Weight // Added weight property
    // Add Type, Weight, Effects, etc. later
); 