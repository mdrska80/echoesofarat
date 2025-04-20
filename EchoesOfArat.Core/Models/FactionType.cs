namespace EchoesOfArat.Core.Models;

/// <summary>
/// Represents the different factions present in the game world.
/// Based on game_concept.md section 2.7.
/// </summary>
public enum FactionType
{
    None, // For neutral entities or areas
    RoyalCourt,
    PriesthoodAshar,
    Wardens,
    SeekersVeiledPath,
    SerpentCultists,
    ScavengersRedWaste,
    DesertNomads,
    PriesthoodNefet, // Example lesser priesthood
    PriesthoodUrGhul // Example lesser priesthood
    // Add more factions as defined
} 