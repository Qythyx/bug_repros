using Beerbox.App.Core.Services;

namespace Beerbox.App.Services;

/// <summary>
/// Mock implementation of <see cref="IConnectivityService"/> that reads its state from
/// the <see cref="MockEnvironmentVars.IsConnected"/> environment variable.
/// Defaults to connected when the variable is absent.
/// </summary>
public sealed class MockConnectivityService : IConnectivityService
{
	public bool IsConnected { get; } = true;

#pragma warning disable CS0067 // Mock intentionally never fires connectivity changes
	public event Action<bool>? ConnectivityChanged;
#pragma warning restore CS0067
}
