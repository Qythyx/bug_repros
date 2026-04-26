using Beerbox.Service.Contracts;
using Beerbox.Service.Contracts.Notifications;

namespace Beerbox.App.Core.Services;

/// <summary>
/// App-specific telemetry service with strongly-typed tracking methods for Beerbox events.
/// Subclass this to provide a concrete telemetry backend.
/// </summary>
public abstract class BeerboxTelemetryService : TelemetryService
{
	#region Enums
	private enum EventName
	{
		AppStart = 0,
		CreateAccount = 1,
		CreateAccountAborted = 2,
		PaymentMethodProblem = 3,
		EditAccount = 4,
		EditAccountAborted = 5,
		Login = 6,
		Logout = 7,
		NotLoggedIn = 8,
		Notification = 9,
		OrderItems = 10,
		PageAppearing = 11,
		Setting = 12,
	}

	public enum AccountInformation
	{
		Address = 0,
		ContactInformation = 1,
		PaymentMethod = 2,
		UntappdToken = 3,
	}

	public enum OrderItemAdjustment
	{
		AddItem = 0,
		DecreaseItem = 1,
		IncreaseItem = 2,
		RemoveItem = 3,
	}

	public enum OrderItemAdjustmentResult
	{
		InsufficientInventory = 0,
		NoInventory = 1,
		Problem = 2,
		Success = 3,
	}

	public enum StartReason
	{
		Manual = 0,
		Notification = 1,
	}

	public enum StartState
	{
		FirstEver = 0,
		FirstForVersion = 1,
		NotFirst = 2,
	}

	public enum LoginState
	{
		Cached = 0,
		InitiateFail = 1,
		InitiateSuccess = 2,
		WaitForVerificationCancelled = 3,
		WaitForVerificationInvalidToken = 4,
		WaitForVerificationSuccess = 5,
	}

	public enum LogoutReason
	{
		Intentional = 0,
		Initialization = 1,
		PermissionsProblem = 2,
	}

	public enum SettingType
	{
		Language = 0,
		Theme = 1,
		Untappd = 2,
	}
	#endregion Enums

	public void TrackAppStart(bool isFirstLaunchEver, bool isFirstLaunchForCurrentVersion, StartReason reason) =>
		Track(
			EventName.AppStart,
			reason,
			isFirstLaunchEver ? StartState.FirstEver
				: isFirstLaunchForCurrentVersion ? StartState.FirstForVersion
				: StartState.NotFirst
		);

	public void TrackCreateAccount(AccountResult result) => Track(EventName.CreateAccount, result);

	public void TrackCreateAccountAborted(string invalidFields) =>
		Track(EventName.CreateAccountAborted, ("InvalidFields", invalidFields));

	public void TrackPaymentMethodProblem(PaymentMethodStatus status) => Track(EventName.PaymentMethodProblem, status);

	public void TrackEditAccount(AccountInformation info, AccountResult result) =>
		Track(EventName.EditAccount, info, result);

	public void TrackEditAccountAborted(AccountInformation info) => Track(EventName.EditAccountAborted, info);

	public void TrackLogin(LoginState state, string? userId)
	{
		Track(EventName.Login, state);
		SetUserId(userId);
	}

	public void TrackNotLoggedIn() => Track(EventName.NotLoggedIn);

	public void TrackLogout(LogoutReason reason)
	{
		if (reason != LogoutReason.Initialization)
		{
			Track(EventName.Logout, reason);
			SetUserId(null);
		}
	}

	public void TrackNotification(bool causedAppToOpen, INotification notification) =>
		Track(EventName.Notification, ("Type", notification.GetType().Name), ("DidLaunch", $"{causedAppToOpen}"));

	public void TrackPageAppearing(string page) => Track(EventName.PageAppearing, ("Page", page));

	public void TrackOrderItems(string offerID, OrderItemAdjustment adjustment, OrderItemAdjustmentResult result) =>
		Track(EventName.OrderItems, ("Offer", offerID), adjustment.ToTuple(), result.ToTuple());

	public void TrackSetting(SettingType type, Enum value) => Track(EventName.Setting, type.ToTuple(), value.ToTuple());

	private void Track(EventName name) => Track(name, Array.Empty<(string, string)>());

	private void Track(EventName name, params Enum[] properties) =>
		Track(name, properties.Select(prop => prop.ToTuple()).ToArray());

	private void Track(EventName name, params (string, string)[] properties) =>
		TrackEvent(name.ToString(), properties.ToDictionary(x => x.Item1, x => x.Item2));
}

internal static class EnumExtensions
{
	internal static (string, string) ToTuple(this Enum e) => (e.GetType().Name, e.ToString());
}
