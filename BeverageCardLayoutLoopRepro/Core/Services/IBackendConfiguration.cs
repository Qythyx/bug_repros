namespace Beerbox.App.Core.Services;

/// <summary>
/// Provides environment-dependent backend configuration that is resolved dynamically
/// on each access, allowing the app to switch environments without recreating services.
/// </summary>
public interface IBackendConfiguration
{
	/// <summary>The backend API URL for the current environment.</summary>
	string BackendUrl { get; }

	/// <summary>The Azure Functions auth key for the current environment.</summary>
	string AuthKey { get; }

	/// <summary>The App Insights connection string for the current environment.</summary>
	string AppInsightsConnectionString { get; }
}
