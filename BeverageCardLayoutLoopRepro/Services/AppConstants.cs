using Beerbox.App.Core.Services;

namespace Beerbox.App.Services;

internal static class AppConstants
{
    public static string GetBackendUrl(
        AppEnvironment environment,
        string? serviceUrlOverride,
        bool isAndroid
    ) =>
        environment switch
        {
            AppEnvironment.Development
            or AppEnvironment.Devtunnel when !string.IsNullOrWhiteSpace(serviceUrlOverride) =>
                serviceUrlOverride.Trim(),
            AppEnvironment.Development => isAndroid
                ? "http://10.0.2.2:7071"
                : "http://localhost:7071",
            AppEnvironment.Devtunnel => "https://3t6kp5l2-8080.asse.devtunnels.ms",
            AppEnvironment.Staging => "https://beerbox-staging.azurewebsites.net",
            AppEnvironment.Production => "https://beerplease.azurewebsites.net",
            _ => throw new ArgumentException($"Unknown environment: {environment}"),
        };

    // Real keys redacted — the repro always runs in mock mode and never hits the backend.
#pragma warning disable IDE0060 // Remove unused parameter
    public static string GetAuthKey(AppEnvironment environment) => string.Empty;

    public static string GetAppInsightsConnectionString(AppEnvironment environment) => string.Empty;
#pragma warning restore IDE0060
}
