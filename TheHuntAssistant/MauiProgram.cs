using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Plugin.Maui.Audio;
using System.Diagnostics;
using TheHuntAssistant.Services;

namespace TheHuntAssistant;

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
        builder.Services.AddMudServices();
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:7120") });

        builder.Services.AddSingleton<AuthorizationService>();
        builder.Services.AddSingleton<FileService>();
        builder.Services.AddSingleton<ClipService>();
        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<SoundPlayerService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        return app;
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        LogException(exception);
    }

    private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        LogException(e.Exception);
        e.SetObserved(); // Prevents the app from crashing due to unobserved task exceptions
    }

    private static void LogException(Exception ex)
    {
        if (ex != null)
        {
            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TheHuntAssistant\\error_log.txt");
            var logMessage = $"{DateTime.Now}: {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}";

            File.AppendAllText(logPath, logMessage); // Append the error to the log file

            Debug.WriteLine(logMessage); // Optionally log to output window
        }
    }
}