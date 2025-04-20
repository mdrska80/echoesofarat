using System.Linq; // For Sum()

namespace EchoesOfArat.Core.Models;

/// <summary>
/// Manages an NPC's inventory.
/// </summary>
public class Inventory
{
    // Injected or calculated based on NPC stats + potentially equipped bags?
    public double MaxCapacity { get; init; } 

    // Use a list to store the items themselves. 
    // Consider Dictionary<string, int> if stacking is needed later.
    public List<Item> Items { get; private set; } = new();

    // Calculated property for current weight
    public double CurrentWeight => Items.Sum(item => item.Weight); // Assumes Item has a Weight property

    // Constructor requires max capacity
    public Inventory(double maxCapacity)
    {
        MaxCapacity = maxCapacity;
    }

    /// <summary>
    /// Attempts to add an item to the inventory.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>True if item was added, false otherwise (e.g., capacity exceeded).</returns>
    public bool TryAddItem(Item item)
    {
        if (CurrentWeight + item.Weight <= MaxCapacity)
        {
            Items.Add(item);
            Console.WriteLine($"Added item {item.Name} to inventory."); // Example Log
            return true;
        }
        Console.WriteLine($"Failed to add item {item.Name}: Inventory full."); // Example Log
        return false;
    }

    /// <summary>
    /// Attempts to remove an item from the inventory.
    /// </summary>
    /// <param name="item">The specific item instance to remove.</param>
    /// <returns>True if the item was found and removed, false otherwise.</returns>
    public bool TryRemoveItem(Item item)
    {
        bool removed = Items.Remove(item);
        if (removed)
        {
            Console.WriteLine($"Removed item {item.Name} from inventory."); // Example Log
        }
        return removed;
    }

    // Add more methods as needed: FindItem, CheckForItem, etc.
} 