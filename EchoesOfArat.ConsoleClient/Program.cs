using System;
using System.Net.Http;
using System.Net.Http.Json; // Required for ReadFromJsonAsync
using System.Threading.Tasks;
using System.Collections.Generic; // For List<T>
using EchoesOfArat.Core.Models; // Use models from the Core project
using System.Linq; // For Any()

namespace EchoesOfArat.ConsoleClient;

// Record to deserialize the enhanced response from /api/gametime/advance
internal record AdvanceTimeResponse(DateTime CurrentGameTime, List<string> TurnLogs);

// Record to deserialize the response from /api/expeditions POST
// We might not need this if we just check status code
// internal record StartExpeditionResponse(); 

class Program
{
    // Shared HttpClient instance
    private static readonly HttpClient httpClient = new HttpClient();
    private static readonly string apiBaseUrl = "http://localhost:5123"; // Ensure this matches ApiHost port
    private static readonly TimeSpan autoAdvanceInterval = TimeSpan.FromSeconds(3); // How often to advance time

    // Store the last known state locally in the client
    private static List<Npc> currentNpcs = new List<Npc>();
    private static List<ActiveExpedition> currentExpeditions = new List<ActiveExpedition>();
    private static DateTime? currentGameTime = null;

    static async Task Main(string[] args)
    {
        Console.WriteLine("Echoes of Arat - Console Client (Auto-Advance Mode)");
        Console.WriteLine("===================================================");

        // Configure HttpClient
        httpClient.BaseAddress = new Uri(apiBaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        // Initial state load
        bool initialStateLoaded = await FetchInitialStateAsync();
        if (!initialStateLoaded) {
            Console.WriteLine("Failed to load initial state. Exiting.");
            return;
        }

        // Send Bek on an expedition if he isn't already
        await EnsureBekIsExploringAsync();

        // Auto-advance loop
        Console.WriteLine($"\nStarting auto-advance every {autoAdvanceInterval.TotalSeconds} seconds. Press Ctrl+C to exit.");
        while (true) // Loop indefinitely until Ctrl+C
        {
            await Task.Delay(autoAdvanceInterval);
            await AdvanceTimeAndDisplayLogsAsync();
        }
    }

    static async Task<bool> FetchInitialStateAsync()
    {
        Console.WriteLine("Fetching initial state...");
        var npcs = await GetNpcsAsync();
        var expeditions = await GetExpeditionsAsync();
        // Maybe fetch current time separately if needed?
        
        if (npcs != null && expeditions != null) {
             currentNpcs = npcs;
             currentExpeditions = expeditions;
             Console.WriteLine("Initial state loaded.");
             DisplayStaticState(); // Display the initial fetched state
             return true;
        } else {
            Console.WriteLine("Error loading initial state from API.");
            return false;
        }
    }

    static async Task EnsureBekIsExploringAsync()
    {
        var bek = currentNpcs.FirstOrDefault(n => n.Name.Contains("Bek"));
        if (bek == null) {
             Console.WriteLine("Couldn't find Bek to send on expedition.");
             return;
        }

        // Check if Bek is already on an expedition
        bool isBekExploring = currentExpeditions.Any(e => e.NpcIds.Contains(bek.Id));
        if (isBekExploring) {
            Console.WriteLine($"Bek (ID: {bek.Id}) is already exploring.");
            return;
        }

        // Find the specific location "nearby_oasis" using the new endpoint
        Console.WriteLine("Looking up 'nearby_oasis' location...");
        var targetLocation = await GetLocationByStaticIdAsync("nearby_oasis");

        if (targetLocation == null) {
             Console.WriteLine("Could not find location 'nearby_oasis' via API.");
             return;
        }

        Guid targetLocationId = targetLocation.Id; // Get the runtime Guid

        Console.WriteLine($"Sending {bek.Name} (ID: {bek.Id}) to explore {targetLocation.Name} (ID: {targetLocationId})...");
        await StartExpeditionAsync(new List<Guid>{ bek.Id }, targetLocationId, ExpeditionStance.CautiousExploration);
        
        // Refresh expeditions state after attempting to start
        await Task.Delay(200); // Short delay to allow API to process (optional)
        var expeditions = await GetExpeditionsAsync();
        if (expeditions != null) currentExpeditions = expeditions;
        
        // Display updated expedition status
        Console.WriteLine("Current Active Expeditions:");
        if (!currentExpeditions.Any()) Console.WriteLine("  (None)");
        foreach (var exp in currentExpeditions) {
             string npcNames = string.Join(", ", exp.NpcIds.Select(id => currentNpcs.FirstOrDefault(n => n.Id == id)?.Name ?? "?"));
             string locName = targetLocation?.Name ?? exp.TargetLocationId.ToString(); // Use name if we have it
             Console.WriteLine($"  - ID: {exp.Id.ToString().Substring(0,4)}, NPCs: {npcNames}, Target: {locName}, Progress: {exp.Progress * 100:F0}%");
        }
    }

    static async Task AdvanceTimeAndDisplayLogsAsync()
    {
        try
        {
            var response = await httpClient.PostAsync("/api/gametime/advance", null);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"\nError advancing time: {(int)response.StatusCode} {response.ReasonPhrase}");
                return;
            }

            var result = await response.Content.ReadFromJsonAsync<AdvanceTimeResponse>();
            if (result != null)
            {
                currentGameTime = result.CurrentGameTime;
                Console.WriteLine($"\n--- Time: {currentGameTime:yyyy-MM-dd HH:mm} ---");
                if (result.TurnLogs != null && result.TurnLogs.Any())
                {
                    Console.WriteLine("Logs for this turn:");
                    foreach (var log in result.TurnLogs)
                    {
                        Console.WriteLine($"  {log}");
                    }
                } 
                else
                {
                    Console.WriteLine("  (No significant events)");
                }
                 // Optionally refresh NPC/Expedition state if logs suggest completion
                 if (result.TurnLogs != null && result.TurnLogs.Any(log => log.Contains("completed"))) {
                     await RefreshDynamicStateAsync();
                 }
            }
            else
            {
                Console.WriteLine("\nTime advanced, but failed to parse response.");
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"\nError connecting to API: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"\nUnexpected error during time advance: {e.Message}");
        }
    }

    static async Task RefreshDynamicStateAsync() {
        var expeditions = await GetExpeditionsAsync();
        if (expeditions != null) currentExpeditions = expeditions;
        var npcs = await GetNpcsAsync(); // Refresh NPC status (e.g., activity)
        if (npcs != null) currentNpcs = npcs;
        // We don't need to display here, main loop does that implicitly
    }

    static void DisplayStaticState() {
        Console.WriteLine("\n--- Initial State ---");
        Console.WriteLine("NPCs:");
        if (!currentNpcs.Any()) Console.WriteLine("  (None)");
        foreach (var npc in currentNpcs) {
            Console.WriteLine($"  - {npc.Name} (ID: {npc.Id})");
        }
        Console.WriteLine("Active Expeditions:");
        if (!currentExpeditions.Any()) Console.WriteLine("  (None)");
        foreach (var exp in currentExpeditions) {
             string npcNames = string.Join(", ", exp.NpcIds.Select(id => currentNpcs.FirstOrDefault(n => n.Id == id)?.Name ?? "?"));
             Console.WriteLine($"  - ID: {exp.Id.ToString().Substring(0,4)}, NPCs: {npcNames}, Target: {exp.TargetLocationId}, Progress: {exp.Progress * 100:F0}%");
        }
        Console.WriteLine("---------------------");
    }

    /// <summary>
    /// Fetches the current list of NPCs from the API.
    /// </summary>
    static async Task<List<Npc>?> GetNpcsAsync()
    {
        try
        {
            var response = await httpClient.GetAsync("/api/npcs");
            response.EnsureSuccessStatusCode(); // Throw exception for non-2xx status codes
            return await response.Content.ReadFromJsonAsync<List<Npc>>();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"\nError fetching NPCs: {e.Message}");
            Console.WriteLine("(Is the ApiHost running?)");
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine($"\nUnexpected error fetching NPCs: {e.Message}");
            return null;
        }
    }

    static async Task<List<ActiveExpedition>?> GetExpeditionsAsync() {
         try {
            var response = await httpClient.GetAsync("/api/expeditions");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ActiveExpedition>>();
        } catch (Exception e) {
            Console.WriteLine($"\nError fetching expeditions: {e.Message}");
            return null;
        }
    }

     static async Task<List<Location>?> GetLocationsAsync() {
         try {
            var response = await httpClient.GetAsync("/api/locations");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Location>>();
        } catch (Exception e) {
            Console.WriteLine($"\nError fetching locations: {e.Message}");
            return null;
        }
    }

    static async Task<Location?> GetLocationByStaticIdAsync(string staticId) {
         try {
            var response = await httpClient.GetAsync($"/api/locations/byStaticId/{Uri.EscapeDataString(staticId)}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) {
                 Console.WriteLine($"\nLocation with static ID '{staticId}' not found via API.");
                 return null;
            }
            response.EnsureSuccessStatusCode(); // Throw for other errors
            return await response.Content.ReadFromJsonAsync<Location>();
        } catch (Exception e) {
            Console.WriteLine($"\nError fetching location by static ID '{staticId}': {e.Message}");
            return null;
        }
    }

    static async Task StartExpeditionAsync(List<Guid> npcIds, Guid targetLocationId, ExpeditionStance stance) {
         try {
            var request = new { NpcIds = npcIds, TargetLocationId = targetLocationId, Stance = stance };
            var response = await httpClient.PostAsJsonAsync("/api/expeditions", request);
            if (!response.IsSuccessStatusCode) {
                 Console.WriteLine($"\nError starting expedition: {(int)response.StatusCode}");
            } else {
                 Console.WriteLine($"Expedition start request sent.");
            }
        } catch (Exception e) {
            Console.WriteLine($"\nError starting expedition: {e.Message}");
        }
    }
} 