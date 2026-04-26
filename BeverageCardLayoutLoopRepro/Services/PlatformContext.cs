using Beerbox.App.Pages;
using MauiReactor;
using MauiControls = Microsoft.Maui.Controls;

namespace Beerbox.App.Services;

/// <summary>
/// Production implementation of <see cref="IPlatformContext"/> that delegates to
/// <see cref="MauiControls.Shell.Current"/>, <see cref="DeviceInfo"/>, and
/// <see cref="MainThread"/>.
/// </summary>
internal sealed class PlatformContext : IPlatformContext
{
	// ── Session State ─────────────────────────────────────────────────

	public bool HasShownPaymentMethodProblem { get; set; }

	// ── Platform ───────────────────────────────────────────────────────

	public bool IsAndroid { get; } = DeviceInfo.Current.Platform == DevicePlatform.Android;

	public void BeginInvokeOnMainThread(Action action) => MainThread.BeginInvokeOnMainThread(action);

	// ── Alerts ─────────────────────────────────────────────────────────

	public Task DisplayAlertAsync(string title, string message, string accept) =>
		MauiControls.Shell.Current.DisplayAlertAsync(title, message, accept);

	public Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel) =>
		MauiControls.Shell.Current.DisplayAlertAsync(title, message, accept, cancel);

	// ── Navigation ─────────────────────────────────────────────────────

	public async Task PushAsync<TPage>()
		where TPage : Component, new() => _ = await MauiControls.Shell.Current.Navigation.PushAsync<TPage>();

	public async Task PushAsync<TPage, TProps>(Action<TProps> propsInitializer)
		where TPage : Component, new()
		where TProps : class, new() =>
		_ = await MauiControls.Shell.Current.Navigation.PushAsync<TPage, TProps>(propsInitializer);

	public Task PushModalAsync(MauiControls.Page page) => MauiControls.Shell.Current.Navigation.PushModalAsync(page);

	public Task PopAsync() => MauiControls.Shell.Current.Navigation.PopAsync();

	public Task PopModalAsync(bool animated = true) => MauiControls.Shell.Current.Navigation.PopModalAsync(animated);

	public Task GoToAsync(string route, bool animate = true) => MauiControls.Shell.Current.GoToAsync(route, animate);

	public Task GoToAsync(ShellPage page, bool animate = false, string parameters = "") =>
		MainShell.GoToAsync(page, animate, parameters);

	public void SetFlyoutPresented(bool presented) => MauiControls.Shell.Current.FlyoutIsPresented = presented;
}
