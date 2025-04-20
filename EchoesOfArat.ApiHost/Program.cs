using EchoesOfArat.Core.Models;
using EchoesOfArat.Core.Persistence;
using EchoesOfArat.Core.Services;
using EchoesOfArat.Core.Data;
using EchoesOfArat.ApiHost.Models; // Using for DTOs
using Microsoft.Extensions.Logging;

namespace EchoesOfArat.ApiHost;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // --- Logging Configuration ---
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        // --- Service Registration (Dependency Injection) ---

        // Add Swagger services
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Echoes of Arat API",
                Version = "v1",
                Description = "API for managing the Echoes of Arat game simulation backend."
            });
        });

        // Register static data (load once, make available)
        var staticItems = GameDataLoader.LoadItems();
        var staticLocations = GameDataLoader.LoadLocations();
        builder.Services.AddSingleton(staticItems);
        builder.Services.AddSingleton(staticLocations);

        // Register the persistence layer
        builder.Services.AddSingleton<IGameStateRepository, JsonGameStateRepository>();

        // Register WorldState as a Singleton - it will be initialized after building
        builder.Services.AddSingleton<WorldState>();

        // Register Core Services (they will get WorldState, ILogger etc. injected)
        builder.Services.AddSingleton<NpcService>();
        builder.Services.AddSingleton<TimeService>();
        builder.Services.AddSingleton<ExplorationService>();
        builder.Services.AddSingleton<FactionService>();
        // TODO: Register other services

        // --- Add CORS services ---
        builder.Services.AddCors();

        // --- Build the App ---
        var app = builder.Build();

        // --- Initialize Game State (After Build, Before Run) ---
        await InitializeGameStateAsync(app.Services);

        // --- Middleware --- 
        app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

        // Enable Swagger middleware
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Echoes of Arat API V1");
            });
        }

        // --- Minimal API Endpoints --- 
        ConfigureApiEndpoints(app);

        // --- Run Configuration ---
        var listeningUrl = "http://localhost:5123";
        var hostLogger = app.Services.GetRequiredService<ILogger<Program>>();
        hostLogger.LogInformation("API Host configured. Attempting to listen on {Url}", listeningUrl);
        app.Urls.Add(listeningUrl);

        try
        {
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            hostLogger.LogCritical(ex, "API Host failed to start.");
        }
    }

    private static async Task InitializeGameStateAsync(IServiceProvider services)
    {
        // Scope needed to resolve scoped/transient services if any, but singletons are fine
        using (var scope = services.CreateScope())
        {
            var sp = scope.ServiceProvider;
            var logger = sp.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Initializing game state after app build...");

            var worldState = sp.GetRequiredService<WorldState>();
            var repository = sp.GetRequiredService<IGameStateRepository>();
            var loadedLocations = sp.GetRequiredService<Dictionary<string, Location>>(); 

            var loadedSave = await repository.LoadAsync();

            if (loadedSave != null)
            {
                logger.LogInformation("Loaded existing save game state.");
                worldState.CurrentGameTime = loadedSave.CurrentGameTime;
                worldState.Npcs = loadedSave.Npcs ?? new();
                worldState.DiscoveredLocations = loadedSave.DiscoveredLocations ?? new();
                worldState.Factions = loadedSave.Factions ?? new();
                worldState.ActiveExpeditions = loadedSave.ActiveExpeditions ?? new();
            }
            else
            {
                logger.LogInformation("No save game found, initializing new world state.");
                var baseCamp = loadedLocations.Values.FirstOrDefault(loc => loc.StaticId == "base_camp");
                Guid startingLocationId = baseCamp?.Id ?? Guid.Empty;

                worldState.AddNpc(new Npc(Guid.NewGuid(), "Bek the Guard", startingLocationId));

                if (baseCamp != null)
                {
                    worldState.AddDiscoveredLocation(baseCamp);
                }

                foreach (FactionType fType in Enum.GetValues(typeof(FactionType)))
                {
                    if (fType != FactionType.None)
                    {
                        worldState.UpdateFaction(new Faction(fType, fType.ToString()));
                    }
                }
            }
            logger.LogInformation("Game state initialization complete.");
        }
    }

    private static void ConfigureApiEndpoints(WebApplication app)
    {
        var apiLogger = app.Services.GetRequiredService<ILogger<Program>>();

        app.MapGet("/", () => Results.Redirect("/swagger"));

        // == NPC Endpoints ==
        app.MapGet("/api/npcs", (NpcService npcService) =>
        {
            apiLogger.LogInformation("Endpoint '/api/npcs' accessed.");
            return Results.Ok(npcService.GetAllNpcs());
        });

        app.MapGet("/api/npcs/{id}", (Guid id, NpcService npcService) =>
        {
            apiLogger.LogInformation("Endpoint '/api/npcs/{NpcId}' accessed.", id);
            var npc = npcService.GetNpcById(id);
            return npc != null ? Results.Ok(npc) : Results.NotFound();
        });

        // == Game State Endpoints ==
        app.MapPost("/api/gamestate/save", async (IGameStateRepository repo, WorldState worldState, SaveGameRequest? request) =>
        {
            var slot = request?.SlotName ?? "default";
            apiLogger.LogInformation("Endpoint '/api/gamestate/save' accessed for slot: {Slot}", slot);
            try { await repo.SaveAsync(worldState, slot); return Results.Ok($"Game Saved to slot '{slot}'."); }
            catch (Exception ex) { apiLogger.LogError(ex, "Failed to save game to slot: {Slot}", slot); return Results.StatusCode(StatusCodes.Status500InternalServerError); }
        });

        app.MapGet("/api/gamestate/load", async (string? slotName, IGameStateRepository repo) =>
        {
            var slot = slotName ?? "default";
            apiLogger.LogInformation("Endpoint '/api/gamestate/load' accessed for slot: {Slot}", slot);
            try { var loadedState = await repo.LoadAsync(slot); return loadedState != null ? Results.Ok(loadedState) : Results.NotFound($"Save slot '{slot}' not found."); }
            catch (Exception ex) { apiLogger.LogError(ex, "Failed to load game from slot: {Slot}", slot); return Results.StatusCode(StatusCodes.Status500InternalServerError); }
        });

        app.MapGet("/api/gamestate/slots", async (IGameStateRepository repo) =>
        {
            apiLogger.LogInformation("Endpoint '/api/gamestate/slots' accessed.");
            var slots = await repo.ListSaveSlotsAsync();
            return Results.Ok(slots);
        });

        // == Time Endpoint ==
        app.MapPost("/api/gametime/advance", (TimeService timeService, WorldState worldState) =>
        {
            apiLogger.LogInformation("Endpoint '/api/gametime/advance' accessed.");
            timeService.AdvanceTurn();
            return Results.Ok(new { CurrentGameTime = worldState.CurrentGameTime });
        });

        // == Expedition Endpoints ==
        app.MapPost("/api/expeditions", (StartExpeditionRequest request, ExplorationService explorationService) =>
        {
            apiLogger.LogInformation("Endpoint '/api/expeditions' POST accessed: {@Request}", request);
            try { var success = explorationService.StartExpedition(request.NpcIds, request.TargetLocationId, request.Stance); return success ? Results.Accepted() : Results.BadRequest("Failed to start expedition (validation failed?)."); }
            catch (Exception ex) { apiLogger.LogError(ex, "Error starting expedition for request: {@Request}", request); return Results.StatusCode(StatusCodes.Status500InternalServerError); }
        });

        app.MapGet("/api/expeditions", (ExplorationService explorationService) =>
        {
            apiLogger.LogInformation("Endpoint '/api/expeditions' GET accessed.");
            return Results.Ok(explorationService.GetActiveExpeditions());
        });

        // == Location Endpoint ==
        app.MapGet("/api/locations", (WorldState worldState) =>
        {
            apiLogger.LogInformation("Endpoint '/api/locations' accessed.");
            return Results.Ok(worldState.DiscoveredLocations.Values);
        });

        // == Faction Endpoint ==
        app.MapGet("/api/factions", (FactionService factionService) =>
        {
            apiLogger.LogInformation("Endpoint '/api/factions' accessed.");
            return Results.Ok(factionService.GetAllFactions());
        });
    }
} 