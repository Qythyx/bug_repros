using Beerbox.App.Core.Services;
using Beerbox.App.Resources.Localization;
using Beerbox.App.Theming;
using MauiReactor;

namespace Beerbox.App.Components;

public sealed partial class ConnectivityBanner : Component<ConnectivityBanner.MyState>
{
	public sealed record MyState
	{
		public bool IsDisconnected { get; set; }
	}

#pragma warning disable IDE0032 // Cannot use auto property: [Inject] requires a field target
	[Inject]
	private readonly IConnectivityService _connectivity;
#pragma warning restore IDE0032

	/// <summary>
	/// ConnectivityService marshals its event to the main thread via BeginInvokeOnMainThread, which
	/// can deliver after this component has unmounted — leading to SetState on a dead component. The
	/// flag is only read/written on the main thread, so no locking is needed.
	/// </summary>
	private bool _mounted;

	protected override void OnMounted()
	{
		base.OnMounted();
		_mounted = true;
		SetState(s => s.IsDisconnected = !_connectivity.IsConnected);
		_connectivity.ConnectivityChanged += OnConnectivityChanged;
	}

	protected override void OnWillUnmount()
	{
		_mounted = false;
		_connectivity.ConnectivityChanged -= OnConnectivityChanged;
		base.OnWillUnmount();
	}

	public override VisualNode Render() =>
		State.IsDisconnected
			? Border(
					Label(AppResources.NetworkDisconnected)
						.FontSize(AppFonts.FontSizeSmall)
						.TextColor(AppColors.TextAlert.Resolve)
						.HCenter()
				)
				.Padding(new Thickness(AppStyles.Spacing, AppStyles.Spacing / 2))
				.BackgroundColor(AppColors.ControlArea.Resolve)
				.StrokeThickness(0)
				.ID(AutomationIds.Connectivity.Banner)
			: Border().StrokeThickness(0);

	private void OnConnectivityChanged(bool connected)
	{
		if (!_mounted)
		{
			return;
		}
		SetState(s => s.IsDisconnected = !connected);
	}
}
