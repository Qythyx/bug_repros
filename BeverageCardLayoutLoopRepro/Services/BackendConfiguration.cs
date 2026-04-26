using Beerbox.App.Core.Services;

namespace Beerbox.App.Services;

/// <summary>
/// Resolves backend configuration dynamically from current settings on each access,
/// so environment switches take effect immediately without recreating services.
/// </summary>
/// <param name="settings">The app settings containing environment and URL override.</param>
/// <param name="isAndroid">Whether the current platform is Android (affects localhost URL).</param>
internal sealed class BackendConfiguration(AppSettings settings, bool isAndroid) : IBackendConfiguration
{
	public string BackendUrl =>
		AppConstants.GetBackendUrl(settings.AppEnvironment, settings.ServiceUrlOverride, isAndroid);

	public string AuthKey => AppConstants.GetAuthKey(settings.AppEnvironment);

	public string AppInsightsConnectionString => AppConstants.GetAppInsightsConnectionString(settings.AppEnvironment);
}
