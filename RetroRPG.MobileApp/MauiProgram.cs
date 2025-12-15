using Microsoft.Extensions.Logging;
using RetroRPG.Core.Data;
using RetroRPG.Core.Services;

namespace RetroRPG.MobileApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Register database service (single instance)
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "retrorpg.db");
        builder.Services.AddSingleton(new LiteDatabaseService(dbPath));

        // Register repositories (they will share the database)
        builder.Services.AddSingleton<CharacterRepository>();
        builder.Services.AddSingleton<GameStateRepository>();

        // Register AI service
        builder.Services.AddHttpClient<IAIGameMasterService, AIGameMasterService>((sp, client) =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true // For local dev
        });

        builder.Services.AddScoped<IAIGameMasterService>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
            // Default to local Ollama instance - user can configure this
            var apiEndpoint = "http://localhost:11434/v1/chat/completions";
            var modelName = "llama3";
            return new AIGameMasterService(httpClient, apiEndpoint, modelName);
        });

        // Register game engine
        builder.Services.AddScoped<GameEngine>();

        return builder.Build();
    }
}