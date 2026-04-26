namespace Beerbox.App.Services;

/// <summary>
/// Abstraction over MAUI runtime services — alerts, navigation, platform detection,
/// and main thread dispatch. Enables headless testing by replacing the real MAUI
/// environment with a mock.
/// </summary>
public interface IPlatformContext
{
	// ── Session State ─────────────────────────────────────────────────

	/// <summary>Whether the payment method problem alert has already been shown this session.</summary>
	bool HasShownPaymentMethodProblem { get; set; }

	// ── Platform ───────────────────────────────────────────────────────

	/// <summary>Whether the current platform is Android.</summary>
	bool IsAndroid { get; }

	/// <summary>Dispatches an action on the main/UI thread.</summary>
	/// <param name="action">The action to run on the main thread.</param>
	void BeginInvokeOnMainThread(Action action);

	// ── Alerts ─────────────────────────────────────────────────────────

	/// <summary>Displays an alert dialog with a single dismiss button.</summary>
	/// <param name="title">The title of the alert.</param>
	/// <param name="message">The message body of the alert.</param>
	/// <param name="accept">The text for the accept/dismiss button.</param>
	Task DisplayAlertAsync(string title, string message, string accept);

	/// <summary>Displays a confirmation alert dialog with accept and cancel buttons.</summary>
	/// <param name="title">The title of the alert.</param>
	/// <param name="message">The message body of the alert.</param>
	/// <param name="accept">The text for the accept button.</param>
	/// <param name="cancel">The text for the cancel button.</param>
	/// <returns><c>true</c> if the accept button was pressed; otherwise <c>false</c>.</returns>
	Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel);

	// ── Navigation ─────────────────────────────────────────────────────

	/// <summary>Pushes a page onto the navigation stack.</summary>
	/// <typeparam name="TPage">The page type to push.</typeparam>
	Task PushAsync<TPage>()
		where TPage : MauiReactor.Component, new();

	/// <summary>Pushes a page with props onto the navigation stack.</summary>
	/// <typeparam name="TPage">The page type to push.</typeparam>
	/// <typeparam name="TProps">The props type for the page.</typeparam>
	/// <param name="propsInitializer">Action to configure the props.</param>
	Task PushAsync<TPage, TProps>(Action<TProps> propsInitializer)
		where TPage : MauiReactor.Component, new()
		where TProps : class, new();

	/// <summary>Pushes a modal page onto the navigation stack.</summary>
	/// <param name="page">The page to push modally.</param>
	Task PushModalAsync(Page page);

	/// <summary>Pops the top page from the navigation stack.</summary>
	Task PopAsync();

	/// <summary>Pops a modal page from the navigation stack.</summary>
	/// <param name="animated">Whether to animate the transition.</param>
	Task PopModalAsync(bool animated = true);

	/// <summary>Navigates to a shell route.</summary>
	/// <param name="route">The route to navigate to.</param>
	/// <param name="animate">Whether to animate the transition.</param>
	Task GoToAsync(string route, bool animate = true);

	/// <summary>Navigates to a named shell page.</summary>
	/// <param name="page">The shell page to navigate to.</param>
	/// <param name="animate">Whether to animate the transition.</param>
	/// <param name="parameters">Optional query parameters appended to the route.</param>
	Task GoToAsync(ShellPage page, bool animate = false, string parameters = "");

	/// <summary>Sets the flyout presented state.</summary>
	/// <param name="presented">Whether the flyout should be visible.</param>
	void SetFlyoutPresented(bool presented);
}
