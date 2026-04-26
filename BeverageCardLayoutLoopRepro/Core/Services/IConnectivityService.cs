namespace Beerbox.App.Core.Services;

/// <summary>
/// Abstraction over network connectivity monitoring. Enables headless testing
/// by replacing the real MAUI connectivity APIs with a mock.
/// </summary>
public interface IConnectivityService
{
	/// <summary>Whether the device currently has internet access.</summary>
	bool IsConnected { get; }

	/// <summary>
	/// Fires when connectivity changes. The argument is <c>true</c> when connected,
	/// <c>false</c> when disconnected. Always raised on the UI thread.
	/// </summary>
	event Action<bool>? ConnectivityChanged;
}
