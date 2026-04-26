using Beerbox.App.Core.Services;
using Beerbox.Service.Contracts;
using Beerbox.Service.Contracts.Database.Documents;

namespace Beerbox.App.Core.Models;

/// <summary>
/// Pure condition evaluation for offer state alerts. Determines which (if any) alert
/// should be shown based on current order, account, and settings state.
/// </summary>
public static class OfferStateCheckers
{
	public enum CheckerKind
	{
		None = 0,
		OrderRecentlyClosed = 1,
		NotLoggedIn = 2,
		PaymentMethodProblem = 3,
		OrderEndingSoon = 4,
	}

	/// <summary>
	/// Evaluates offer state conditions and returns the first active checker.
	/// Checkers are evaluated in priority order; only the first match is returned.
	/// </summary>
	/// <param name="order">The current active order.</param>
	/// <param name="account">The current account.</param>
	/// <param name="settings">The app settings.</param>
	/// <param name="backendSettings">The backend/server settings.</param>
	/// <param name="hasShownPaymentMethodProblem">Whether the payment problem alert has already been shown this session.</param>
	public static CheckerKind GetActiveChecker(
		Order order,
		Account account,
		AppSettings settings,
		Settings backendSettings,
		bool hasShownPaymentMethodProblem
	)
	{
		// Order recently closed
		if (!order.IsTemporary && (settings.PreviousOrderEndDate < order.EndDate || order.EndDate < DateTime.Now))
		{
			return CheckerKind.OrderRecentlyClosed;
		}

		// Not logged in
		if (!settings.HasShownNotLoggedInAlert && account.IsTemporary)
		{
			return CheckerKind.NotLoggedIn;
		}

		// Payment method problems
		if (
			!account.IsTemporary
			&& !hasShownPaymentMethodProblem
			&& account.PaymentMethod.Status != PaymentMethodStatus.OK
		)
		{
			return CheckerKind.PaymentMethodProblem;
		}

		// Order ending soon
		return
			!account.IsTemporary
			&& order.EndDate.Date != settings.OrderEndingSoonDate.Date
			&& settings.OrderEndingSoonShownDate.Date != DateTime.Today
			&& order.EndDate > DateTime.Now
			&& order.EndDate.Subtract(DateTime.Now).TotalHours < backendSettings.OrderEndingSoonHours
			? CheckerKind.OrderEndingSoon
			: CheckerKind.None;
	}
}
