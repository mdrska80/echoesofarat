# Game Architecture: EchoesOfArat (Working Title)

## 1. Overview

This document outlines the proposed initial architecture for "EchoesOfArat," a multiplatform management and exploration simulation game based on the concepts described in `docs/game_concept.md`. The focus is on a robust, scalable design using **Tauri** to package a **Vue.js** frontend which communicates with a **C# backend** running as a sidecar process. This approach leverages existing skills in Vue.js and C# while benefiting from Tauri's performance and resource efficiency.

## 2. Core Goals & Constraints

*   **Multiplatform:** Target Windows, macOS, and Linux desktops.
*   **Language Preference:** Leverage existing Vue.js (frontend) and C# (backend) knowledge.
*   **Game Type:** Management/Simulation with heavy UI focus (NPCs, base, exploration logs, atlas). Requires a capable UI framework and a strong backend for simulation.
*   **Performance/Resources:** Aim for a relatively lightweight application bundle and reasonable resource usage, favoring Tauri over Electron by using native webviews.
*   **Complexity:** High degree of simulation complexity for NPCs (stats, skills, traits, AI, relationships) and world state (factions, locations, events) handled centrally in the C# backend.

## 3. Proposed Technology Stack

*   **Desktop App Framework:** **Tauri**
    *   *Rationale:* Creates lightweight, secure, cross-platform desktop applications using web frontends. Uses the OS's native webview (WebView2 on Windows, WebKit on macOS/Linux) resulting in smaller bundles and lower RAM usage compared to Electron. Manages the C# backend process lifecycle via the sidecar feature.
*   **Frontend Framework:** **Vue.js** (v3 recommended with Composition API) + **TypeScript** (recommended for type safety) + **Pinia** (for state management)
    *   *Rationale:* Leverages user familiarity. Powerful and flexible for building complex, reactive user interfaces. Rich ecosystem and tooling (Vite recommended for build tooling). TypeScript enhances maintainability. Pinia is the current standard for Vue state management.
*   **Backend Language/Framework:** **C#** with **.NET** (latest LTS recommended)
    *   *Rationale:* Leverages user familiarity. Excellent for building complex, performant simulation logic. Strong typing, rich standard library.
    *   **Implementation:** An **ASP.NET Core Minimal API** application compiled as a self-contained executable. Runs a lightweight Kestrel web server listening *only* on `localhost` (e.g., `http://localhost:5123`).
*   **Communication Protocol:** **Local HTTP Requests** (Frontend <-> Backend)
    *   *Rationale:* Simple, well-understood standard for request/response communication. Vue's `fetch` or libraries like `axios` can easily call the C# backend's Minimal API endpoints. WebSockets could be added later if real-time push from backend to frontend is needed (e.g., for background event notifications).
*   **Data Storage:**
    *   **Save Games:** Managed entirely by the C# backend. Serialized C# objects (e.g., `WorldState`) using `System.Text.Json`, stored in a standard user data location (e.g., `Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)/YourGameName/savegame.json`).
    *   **Game Data (Items, Skills, Locations, etc.):** JSON files read by the C# backend at startup. These files define the static data of the game world. Managed via C# classes/records representing the structure of these JSON files.

## 4. Detailed Architecture Breakdown

The application consists of three distinct parts managed by Tauri:

### 4.1. Backend (C# Sidecar Process - The Simulation Brain)

This standalone .NET application holds the authoritative game state and executes all simulation logic. It runs headlessly in the background, managed by Tauri.

#### 4.1.1. Core Principles

*   **Separation of Concerns:** The core simulation logic (`EchoesOfArat.Core`) is kept separate from the web hosting layer (`EchoesOfArat.ApiHost`).
*   **State Management:** The authoritative game state (`WorldState`) resides entirely within this backend process.
*   **Testability:** The `EchoesOfArat.Core` library should be highly unit-testable without needing the web host.
*   **Communication:** Exposes functionality via a local HTTP API for the frontend.

#### 4.1.2. Project Structure & Key Components (`EchoesOfArat.Core` - Class Library)

This project contains the heart of the game simulation.

*   **`Models/` (or `Domain/`)**: Contains the core data structures (POCOs or Records).

    *   **`Npc.cs` (Example Record)**
        ```csharp
        // Example structure - properties will expand based on game_concept.md
        public record Npc(
            Guid Id,
            string Name,
            Stats BaseStats,
            Dictionary<SkillType, int> Skills,
            List<TraitType> Traits,
            Guid CurrentLocationId,
            Inventory Inventory,
            // Add Loyalty, Morale, Goals, CurrentActivity, etc.
        );
        ```

    *   **`Item.cs` (Example Record)**
        ```csharp
        public record Item(
            string Id, // Use string IDs for items defined in JSON
            string Name,
            string Description,
            ItemType Type,
            double Weight,
            // Add properties for effects, durability, etc.
        );
        ```

    *   **`Inventory.cs` (Example Class)**
        ```csharp
        public class Inventory
        {
            public double MaxCapacity { get; init; }
            public List<Item> Items { get; private set; } = new();
            public double CurrentWeight => Items.Sum(item => item.Weight);

            public bool TryAddItem(Item item)
            {
                if (CurrentWeight + item.Weight <= MaxCapacity)
                {
                    Items.Add(item);
                    return true;
                }
                return false;
            }
            // Methods for removing items, checking space etc.
        }
        ```

    *   **`WorldState.cs` (Example Class)**
        ```csharp
        public class WorldState
        {
            public DateTime CurrentGameTime { get; set; }
            public List<Npc> Npcs { get; set; } = new();
            public Dictionary<Guid, Location> DiscoveredLocations { get; set; } = new();
            public Dictionary<FactionType, Faction> Factions { get; set; } = new();
            public List<ActiveExpedition> ActiveExpeditions { get; set; } = new();
            // Add Base Management state, Event Queues etc.
        }
        ```

    *   Other models: `Location.cs`, `Faction.cs`, `SkillType.cs` (enum), `TraitType.cs` (enum), `ItemType.cs` (enum), `ActiveExpedition.cs`, `Stats.cs` etc.

*   **`Services/` (or `Managers/`)**: Classes containing business logic, operating on the models.

    *   **`TimeService.cs`**
        ```csharp
        public class TimeService
        {
            private readonly WorldState _worldState; // Injected or managed
            // Inject other needed services (Exploration, NPC needs etc.)

            public TimeService(WorldState worldState /*, other services */) 
            {
                 _worldState = worldState;
            }

            public void AdvanceTurn()
            {
                _worldState.CurrentGameTime = _worldState.CurrentGameTime.AddHours(1); // Example granularity
                // Trigger NPC need updates
                // Trigger expedition progress checks
                // Trigger random events?
                Console.WriteLine($"Game time advanced to: {_worldState.CurrentGameTime}"); // Example Log
            }
        }
        ```

    *   **`NpcService.cs`**
        ```csharp
        public class NpcService
        {
            private readonly WorldState _worldState;

             public NpcService(WorldState worldState) 
             {
                 _worldState = worldState;
             }

            public Npc? GetNpcById(Guid id) => _worldState.Npcs.FirstOrDefault(n => n.Id == id);
            public IEnumerable<Npc> GetAllNpcs() => _worldState.Npcs;
            // Methods for NPC AI ticks, need updates, skill checks, etc.
        }
        ```

    *   **`ExplorationService.cs`**
        ```csharp
        public class ExplorationService
        {
            private readonly WorldState _worldState;

            public ExplorationService(WorldState worldState) 
            {
                 _worldState = worldState;
            }

            public bool StartExpedition(List<Guid> npcIds, Guid targetLocationId, ExpeditionStance stance)
            {
                // Validate NPCs, location, etc.
                // Create ActiveExpedition record
                // Update NPC statuses
                Console.WriteLine($"Starting expedition for NPCs: {string.Join(",", npcIds)} to {targetLocationId}"); // Example Log
                return true; // Or return result object
            }

            public void UpdateExpeditionProgress(ActiveExpedition expedition)
            {
                // Calculate progress based on time, NPCs, stance
                // Trigger encounters
            }
        }
        ```

    *   Other services: `InventoryService`, `FactionService`, `EncounterService`, `BaseManagementService`.

*   **`Persistence/`**: Handles saving and loading the game state.

    *   **`IGameStateRepository.cs` (Interface)**
        ```csharp
        public interface IGameStateRepository
        {
            Task SaveAsync(WorldState worldState, string slotName = "default");
            Task<WorldState?> LoadAsync(string slotName = "default");
            Task<List<string>> ListSaveSlotsAsync();
        }
        ```

    *   **`JsonGameStateRepository.cs` (Implementation)**
        ```csharp
        using System.Text.Json; // Add this using directive

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
                catch(Exception ex)
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
                                        .Where(name => name != null)
                                        .ToList();
                     _logger.LogInformation("Found save slots: {Slots}", string.Join(", ", files!));
                     return Task.FromResult(files!); // Non-null asserted due to Where clause
                 }
                 catch (Exception ex)
                 {
                      _logger.LogError(ex, "Failed to list save slots in {SavePath}", _saveGameDirectory);
                      return Task.FromResult(new List<string>()); // Return empty list on error
                 }
             }
        }
        ```

*   **`Data/`**: Responsible for loading static game definitions (items, base locations, etc.) from embedded resources or files at startup.
    ```csharp
    using System.Text.Json; // Add this using directive

    public static class GameDataLoader
    {
        // Inject logger if needed, or use static logging
        // Consider making this non-static and injecting via DI if complex init is needed

        public static List<Item> LoadItems(string filePath = "Data/items.json")
        {
            try
            {
                 Console.WriteLine($"Loading items from: {Path.GetFullPath(filePath)}"); // Log loading path
                 string json = File.ReadAllText(filePath);
                 var items = JsonSerializer.Deserialize<List<Item>>(json);
                 Console.WriteLine($"Loaded {items?.Count ?? 0} items."); // Log count
                 return items ?? new List<Item>();
            }
            catch (Exception ex)
            {
                 Console.WriteLine($"Error loading items from {filePath}: {ex.Message}"); // Log error
                 // Decide how to handle: return empty list, throw, default items?
                 return new List<Item>();
            }
        }
        // Methods for loading skills, locations, factions etc. from their respective JSON files
    }
    ```

#### 4.1.3. API Hosting & Interaction (`EchoesOfArat.ApiHost` - ASP.NET Core Minimal API)

This project acts as the communication bridge between the core simulation and the frontend.

*   **`Program.cs` - Key Configurations:**
    *   **Dependency Injection:** Register `Core` services. Singletons are often suitable for services managing the main `WorldState` or loading static data. Use `AddLogging()` for ILogger support. **Crucially, manage the `WorldState` instance carefully.**
        ```csharp
        var builder = WebApplication.CreateBuilder(args);

        // --- Logging ---
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(); // Log to console (visible in Tauri sidecar logs)
        // Add other providers like Debug, File etc. if needed

        // --- Configuration ---
        // Load appsettings.json etc. if needed for ports or other config

        // --- Dependency Injection ---
        builder.Services.AddSingleton<IGameStateRepository, JsonGameStateRepository>(); // Register persistence

        // Load static game data (example items)
        // Ensure Data/items.json exists and is copied to output directory
        // Or use embedded resources
        var staticItems = GameDataLoader.LoadItems();
        builder.Services.AddSingleton(staticItems);

        // ** WorldState Management Strategy **
        // Option A: Load/Create on Startup (Simplest for single save slot)
        // Resolve repository *during* setup to load initial state
        var initialRepo = new JsonGameStateRepository(builder.Services.BuildServiceProvider().GetRequiredService<ILogger<JsonGameStateRepository>>());
        var initialWorldState = await initialRepo.LoadAsync() ?? new WorldState(); // Load default or create new
        builder.Services.AddSingleton(initialWorldState); // Register the instance

        // Option B: Service to manage loading/saving (More flexible for multiple slots)
        // builder.Services.AddSingleton<GameStateService>(); // This service would internally hold/manage WorldState

        // Register services that operate on the WorldState
        // Ensure their constructors take WorldState (and other services like ILogger)
        builder.Services.AddSingleton<TimeService>();
        builder.Services.AddSingleton<NpcService>();
        builder.Services.AddSingleton<ExplorationService>();
        // ... other services

        var app = builder.Build();

        // --- Middleware ---
        // Configure CORS for development (less critical once inside Tauri, but good practice)
        app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()); // Adjust policy as needed


        // --- Minimal API Endpoints ---
        // ... (See examples below)

        // --- Run ---
        // Ensure Kestrel listens only on localhost
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        string listeningUrl = "http://localhost:5123"; // Use configuration later
        logger.LogInformation("API Host starting, listening on {Url}", listeningUrl);
        app.Urls.Add(listeningUrl); // Explicitly set URL
        await app.RunAsync();

        ```
    *   **Kestrel Configuration:** Ensure it listens only locally (set via `app.Urls.Add` or `UseUrls`).
    *   **Logging:** Basic console logging configured via `builder.Logging`.

*   **Minimal API Endpoint Examples:** Define routes cleanly. Use `Results` static class for standard HTTP responses. Inject services directly into handlers.
    ```csharp
    // Get all NPCs (ensure Npc record is serializable)
    app.MapGet("/api/npcs", (NpcService npcService) => Results.Ok(npcService.GetAllNpcs()));

    // Get specific NPC by ID
    app.MapGet("/api/npcs/{id}", (Guid id, NpcService npcService) =>
    {
        var npc = npcService.GetNpcById(id);
        return npc != null ? Results.Ok(npc) : Results.NotFound($"NPC with ID {id} not found.");
    });

    // Start an expedition (using a request body DTO)
    app.MapPost("/api/expeditions", (StartExpeditionRequest request, ExplorationService explorationService, ILogger<Program> logger) =>
    {
        try
        {
             logger.LogInformation("Received request to start expedition: {@Request}", request);
             var success = explorationService.StartExpedition(request.NpcIds, request.TargetLocationId, request.Stance);
             return success
                ? Results.Accepted($"/api/expeditions/{Guid.NewGuid()}") // Return dummy path or actual ID if generated
                : Results.BadRequest("Failed to start expedition (e.g., invalid input).");
        }
        catch (Exception ex)
        {
             logger.LogError(ex, "Error starting expedition for request: {@Request}", request);
             return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    });

    // Advance game time by one turn
    app.MapPost("/api/gametime/advance", (TimeService timeService, WorldState worldState, ILogger<Program> logger) => // Inject WorldState to get current time?
    {
        logger.LogInformation("Advancing game time from {CurrentTime}", worldState.CurrentGameTime);
        timeService.AdvanceTurn();
        // Return minimal confirmation or potentially key updated state parts
        return Results.Ok(new { NewTime = worldState.CurrentGameTime });
    });

    // Save game state to a specific slot (or default)
    app.MapPost("/api/gamestate/save", async (SaveGameRequest request, IGameStateRepository repo, WorldState worldState, ILogger<Program> logger) =>
    {
        // Injecting WorldState here assumes Singleton strategy from DI setup
        var slot = request?.SlotName ?? "default";
        logger.LogInformation("Saving game state to slot: {Slot}", slot);
        try
        {
             await repo.SaveAsync(worldState, slot);
             return Results.Ok($"Game Saved to slot '{slot}'.");
        }
        catch(Exception ex)
        {
             logger.LogError(ex, "Failed to save game to slot: {Slot}", slot);
             return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    });

    // Load game state from a specific slot (or default)
    // NOTE: Loading needs a strategy to REPLACE the current WorldState singleton instance
    // This is non-trivial and might require a dedicated GameStateService or restarting the backend.
    // A simpler approach for now might be: Load returns data, frontend requests a "restart"
    app.MapGet("/api/gamestate/load", async (string? slotName, IGameStateRepository repo, ILogger<Program> logger) =>
    {
        var slot = slotName ?? "default";
        logger.LogInformation("Loading game state from slot: {Slot}", slot);
        try
        {
             var loadedState = await repo.LoadAsync(slot);
             return loadedState != null
                ? Results.Ok(loadedState) // Frontend handles applying this state
                : Results.NotFound($"Save slot '{slot}' not found.");
        }
        catch(Exception ex)
        {
             logger.LogError(ex, "Failed to load game from slot: {Slot}", slot);
             return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    });

    // List available save slots
    app.MapGet("/api/gamestate/slots", async (IGameStateRepository repo) => {
        var slots = await repo.ListSaveSlotsAsync();
        return Results.Ok(slots);
    });


    // --- DTO Examples ---
    public record StartExpeditionRequest(List<Guid> NpcIds, Guid TargetLocationId, ExpeditionStance Stance);
    public record SaveGameRequest(string? SlotName);
    // Define enums like ExpeditionStance in the Core project if shared
    public enum ExpeditionStance { Cautious, Aggressive, Research, Gather } // Example
    ```

#### 4.1.4. Build Configuration

*   The `EchoesOfArat.ApiHost.csproj` file needs specific properties for self-contained deployment:
    ```xml
    <PropertyGroup>
      <OutputType>Exe</OutputType>
      <TargetFramework>net8.0</TargetFramework> <!-- Or latest LTS -->
      <Nullable>enable</Nullable>
      <ImplicitUsings>enable</ImplicitUsings>
      <!-- Crucial for Tauri sidecar bundling -->
      <PublishSingleFile>true</PublishSingleFile>
      <SelfContained>true</SelfContained>
      <!-- Specify target runtime ID (RID) e.g., win-x64, osx-x64, linux-x64 -->
      <!-- This needs to be configured per-platform for Tauri build -->
      <RuntimeIdentifier>win-x64</RuntimeIdentifier>
      <!-- Include native libs if needed (e.g., for globalization) -->
      <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
      <!-- Optional: Trimming can reduce size but requires testing -->
      <!-- <PublishTrimmed>true</PublishTrimmed> -->
      <!-- Optional: Reduce size further -->
      <DebugType>None</DebugType>
      <DebugSymbols>false</DebugSymbols>
    </PropertyGroup>

    <!-- Ensure static data files are copied -->
    <ItemGroup>
      <Content Include="Data\\**">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
      </Content>
    </ItemGroup>
    ```
*   **Build Automation:** A script (e.g., PowerShell, Bash, Cake) is highly recommended to:
    1.  Build the C# backend using `dotnet publish` for *each* required `RuntimeIdentifier` (win-x64, osx-x64, linux-x64).
    2.  Specify the correct output path for the published executable.
    3.  Copy the correct platform-specific executable to the location Tauri expects (referenced in `tauri.conf.json` under `externalBin`). This often involves copying it into the `src-tauri/binaries` directory or similar, *before* running `tauri build`.
    4.  Run `tauri build`.

### 4.2. Frontend (Vue.js Application - The User Interface)

This standard web application runs within the Tauri webview, providing the visual representation and user interaction layer.

*   **Project Structure (`src/` - Example using Vite):**
    *   `main.ts`: Initializes Vue, Pinia, Router.
    *   `App.vue`: Root component.
    *   `components/`: Reusable UI pieces (`NpcCard.vue`, `InventoryGrid.vue`, `ActionLog.vue`).
    *   `views/`: Major screen layouts (`MainGameView.vue`, `AtlasView.vue`).
    *   `stores/`: Pinia modules (`gameState.ts`, `npcStore.ts`, `inventoryStore.ts`). Stores hold frontend state, fetch data via API service, and provide actions triggered by UI events.
    *   `services/` or `api/`: Communication logic (`apiClient.ts`). Configures `axios` or `fetch`, defines functions for calling backend endpoints, handles base URL and JSON formatting.
    *   `router/`: Vue Router configuration (if needed).
    *   `assets/`: Static files (images, CSS).
*   **State Management (Pinia):** Stores reflect the *view's interpretation* of the backend state. Actions call the `apiClient`, and upon successful response, update the store state, triggering reactive UI updates via Vue's system.
*   **Communication:** Strictly interacts with the backend via HTTP calls made through the `apiClient` service.

### 4.3. Tauri Application (The Wrapper & Orchestrator)

This layer, primarily configured via `tauri.conf.json`, packages the frontend and manages the backend sidecar.

*   **Core Functionality:**
    *   Uses the OS's native webview to render the Vue.js frontend (efficient resource usage).
    *   Manages the lifecycle of the C# backend process (starts on launch, terminates on exit).
    *   Provides APIs for interacting with the OS (e.g., file dialogs, system tray), though these might not be heavily needed if most logic is in the backend.
*   **`tauri.conf.json` Configuration:**
    *   `build`: `distDir` (path to Vue build output), `devPath` (URL for `tauri dev`).
    *   `package`: `productName`, `version`.
    *   `tauri`:
        *   `bundle`:
            *   `identifier`: A unique reverse-domain identifier for your app (e.g., `com.yourcompany.echoesofarat`).
            *   `active`: Must be `true`.
            *   **`externalBin`**: An array listing the relative path(s) to your compiled C# backend executable(s). Example: `["../EchoesOfArat.ApiHost/bin/Release/netX.0/win-x64/publish/EchoesOfArat.ApiHost.exe"]`. You'll need to adjust paths and potentially use build scripts to copy the correct OS-specific executable here before Tauri bundles it.
        *   `allowlist`: `shell: { sidecar: true }` (required to enable sidecar execution). Other APIs (like `fs`, `dialog`) enabled as needed.
        *   `windows`: Initial window configuration (title, size, etc.).
*   **Build Process:**
    *   `npm run tauri dev`: For development, runs Vue dev server and Tauri app, hot-reloading frontend changes. Manages sidecar.
    *   `npm run tauri build`: Creates production build - bundles Vue app, copies C# sidecar executable, packages into native installer/bundle.

### 4.4. Communication Flow (Local HTTP)

Frontend actions trigger API calls to the backend:

1.  **Vue Component Event:** (e.g., Button click) -> Calls Pinia store action.
2.  **Pinia Store Action:** -> Calls `apiClient` function (e.g., `apiClient.equipItem(npcId, itemId)`).
3.  **`apiClient` (Vue Service):** -> Sends HTTP request (`POST http://localhost:5123/api/npcs/{npcId}/equip`) with JSON payload.
4.  **Kestrel (C# Backend):** Receives request.
5.  **ASP.NET Core Minimal API:** Routes request to endpoint handler.
6.  **Endpoint Handler (C#):** -> Calls appropriate service method in `EchoesOfArat.Core` (e.g., `InventoryService.EquipItem(npcId, itemId)`).
7.  **Core Service (C#):** Executes game logic, updates internal `WorldState`, potentially saves state via `IGameStateRepository`.
8.  **Endpoint Handler (C#):** Returns HTTP response (e.g., `200 OK` with updated NPC data, or `400 Bad Request` if invalid).
9.  **`apiClient` (Vue Service):** Receives response.
10. **Pinia Store Action:** Updates store state based on response.
11. **Vue Component:** Reactively updates UI based on changed store state.

## 5. Key Modules & Systems (Initial Focus Areas)

*   **Backend (C#):**
    1.  **Domain Models:** Define core C# classes/records for `Npc`, `Item`, `Inventory`, basic `Stats`.
    2.  **Persistence:** Implement basic `JsonGameStateRepository` for saving/loading a simple `WorldState`.
    3.  **API Host:** Setup Minimal API project, configure Kestrel, create initial endpoints (e.g., `GET /api/npcs`, `POST /api/gamestate/save`, `POST /api/gamestate/load`).
    4.  **Build:** Configure self-contained executable publishing.
*   **Frontend (Vue.js):**
    1.  **Setup:** Initialize project (Vite), add TypeScript, Pinia.
    2.  **API Service:** Create `apiClient.ts` to call initial backend endpoints.
    3.  **Stores:** Setup basic Pinia stores (`npcStore`, `gameStateStore`) with actions to fetch/update data via `apiClient`.
    4.  **Components:** Create simple components to display NPC list and details fetched from the store.
*   **Tauri:**
    1.  **Setup:** Initialize Tauri project (`npm create tauri-app@latest`).
    2.  **Configuration:** Edit `tauri.conf.json`: set `distDir`, configure `externalBin` pointing to the *built* C# executable location, enable `shell.sidecar` allowlist. Test `tauri dev` and `tauri build`.

## 6. Considerations & Future Work

*   **API Design:** Crucial interface. Define clearly, version early. Consider OpenAPI/Swagger for documentation.
*   **Error Handling:** Implement robustly on both ends (network errors, validation errors, simulation errors). Consistent error response format from backend.
*   **State Synchronization:** Primarily pull-based initially (frontend refetches after actions). Consider WebSockets later if real-time backend pushes are needed (e.g., background event notifications).
*   **Performance:** Monitor C# backend response times. Use `async/await` properly for I/O (saving state). Complex simulation steps might need optimization. Localhost HTTP latency is low but not zero.
*   **Security:** Backend listening only on localhost is key. Tauri's model is generally secure. Validate inputs on the backend.
*   **Debugging:** Requires separate debugging workflows: browser devtools (attachable to Tauri webview) for Vue.js, and standard .NET debugger (Visual Studio/Code) attached to the running C# sidecar process.
*   **Testing:** Unit tests for C# Core services are essential. Integration tests for API endpoints. Component/E2E tests for Vue frontend.
*   **Build Automation:** Scripts needed to reliably build the C# backend for each OS and place the executable where Tauri's `externalBin` expects it before running `tauri build`.

*   **API Design:** Carefully design the API contract between the frontend and backend. Consider versioning.
*   **Error Handling:** Implement robust error handling on both frontend and backend (e.g., network failures, invalid actions).
*   **State Synchronization:** Decide on the strategy for keeping frontend state synchronized (e.g., refetch data after actions, backend pushing updates via WebSockets if needed later).
*   **Performance:** While Tauri/Webview is efficient, ensure the C# backend simulation doesn't block responses for too long. Complex calculations might need asynchronous handling (`async/await` in C# endpoints). The communication latency (even on localhost) needs consideration for very rapid interactions.
*   **Security:** Tauri defaults are generally secure. Ensure the C# backend only listens on localhost and potentially implement some basic validation if needed.
*   **Debugging:** Need tools/workflows to debug both the Vue.js frontend (browser devtools attached to webview) and the C# backend (standard .NET debugging).
*   **Testing:** Implement unit tests for the C# backend logic and potentially UI component tests for Vue.js. 