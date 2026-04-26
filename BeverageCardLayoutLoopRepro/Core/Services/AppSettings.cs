using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Beerbox.App.Core.Services.Settings;
using Beerbox.Service.Contracts;
using Beerbox.Service.Contracts.Notifications;

namespace Beerbox.App.Core.Services;

public abstract class AppSettings(Language defaultLanguage)
{
	public event Action<string>? SettingChanged;

	private Language DefaultLanguage { get; } = defaultLanguage;

	/// <summary>The actual language to be shown.</summary>
	public Language Language =>
		LanguageSetting switch
		{
			LanguageSetting.English => Language.English,
			LanguageSetting.Japanese => Language.Japanese,
			_ => DefaultLanguage,
		};

	/// <summary>The language setting the user has chosen.</summary>
	public LanguageSetting LanguageSetting
	{
		get => (LanguageSetting)SafeGet(GetInt, nameof(LanguageSetting), (int)LanguageSetting.System);
		set
		{
			Set((int)value);
			OnSettingChanged(nameof(Language));
		}
	}

	public AppThemeSetting AppTheme
	{
		get => (AppThemeSetting)SafeGet(GetInt, nameof(AppTheme), (int)AppThemeSetting.Unspecified);
		set => Set((int)value);
	}

	public string? LoginInProgressEmailAddress
	{
		get => SafeGet<string?>(GetString, nameof(LoginInProgressEmailAddress), null);
		set => Set(value);
	}

	public DateTime PreviousOrderEndDate
	{
		get => SafeGet(GetDateTime, nameof(PreviousOrderEndDate), DateTime.MaxValue).ToLocalTime();
		set => Set(value);
	}

	public DateTime OrderEndingSoonDate
	{
		get => SafeGet(GetDateTime, nameof(OrderEndingSoonDate), DateTime.MinValue).ToLocalTime();
		set => Set(value);
	}

	/// <summary>The date the "order ending soon" alert was last shown to the user.</summary>
	public DateTime OrderEndingSoonShownDate
	{
		get => SafeGet(GetDateTime, nameof(OrderEndingSoonShownDate), DateTime.MinValue).ToLocalTime();
		set => Set(value);
	}

	public void ResetOrderDates()
	{
		PreviousOrderEndDate = DateTime.MaxValue.ToLocalTime();
		OrderEndingSoonDate = DateTime.MinValue.ToLocalTime();
		OrderEndingSoonShownDate = DateTime.MinValue.ToLocalTime();
	}

	public string? RegisteredNotificationsToken
	{
		get => SafeGet<string?>(GetString, nameof(RegisteredNotificationsToken), null);
		set => Set(value);
	}

	public NotificationTag RegisteredNotificationsTags
	{
		get =>
			SafeGet(
				(name, _) =>
				{
					var json = GetString(name, null);
					return json == null
						? NotificationTag.None
						: JsonSerializer.Deserialize<NotificationTag>(json, SerializerOptions.Default);
				},
				nameof(RegisteredNotificationsTags),
				NotificationTag.None
			);
		set => Set(JsonSerializer.Serialize(value, SerializerOptions.Default));
	}

	public bool HasSeenHowItWorks
	{
		get => SafeGet(GetBoolean, nameof(HasSeenHowItWorks), false);
		set => Set(value);
	}

	public bool HasShownNotLoggedInAlert
	{
		get => SafeGet(GetBoolean, nameof(HasShownNotLoggedInAlert), false);
		set => Set(value);
	}

	#region Test Settings
	public bool ShowTestSettings
	{
		get => SafeGet(GetBoolean, nameof(ShowTestSettings), false);
		set => Set(value);
	}

	public bool ReceiveTestNotifications
	{
		get => SafeGet(GetBoolean, nameof(ReceiveTestNotifications), false);
		set => Set(value);
	}

	private const AppEnvironment DefaultEnvironment =
#if DEBUG
	AppEnvironment.Development;
#else
	AppEnvironment.Production;
#endif

	public AppEnvironment AppEnvironment
	{
		get => (AppEnvironment)SafeGet(GetInt, nameof(AppEnvironment), (int)DefaultEnvironment);
		set => Set((int)value);
	}

	public string? ServiceUrlOverride
	{
		get => SafeGet<string?>(GetString, nameof(ServiceUrlOverride), null);
		set => Set(value);
	}
	#endregion Test Settings

	/// <summary>
	/// The minimum app version required to run. This is a runtime-only property
	/// (not persisted to Preferences); persistence is handled by <c>AppConfigCache</c>.
	/// </summary>
	public Version MinimumAppVersionRequired { get; set; } = new(1, 0, 0, 0);

	public UntappdSetting UntappdSetting
	{
		get => (UntappdSetting)SafeGet(GetInt, nameof(UntappdSetting), (int)UntappdSetting.GlobalRating);
		set => Set((int)value);
	}

	/// <summary>
	/// The set of beverage kinds that are currently disabled in the filter.
	/// Empty by default, meaning all kinds are enabled — so newly added kinds
	/// are automatically included.
	/// </summary>
	[AllowNull]
	public ReadOnlySet<BeverageKind> DisabledBeverageKinds
	{
		get =>
			field ??= (SafeGet<string?>(GetString, nameof(DisabledBeverageKinds), "") ?? "")
				.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.Select(item => (BeverageKind?)(Enum.TryParse<BeverageKind>(item, out var kind) ? kind : null))
				.Where(item => item is not null)
				.Cast<BeverageKind>()
				.ToHashSet()
				.AsReadOnly();
		private set;
	}

	/// <summary>
	/// Enables or disables a specific beverage kind in the filter.
	/// </summary>
	/// <param name="kind">The beverage kind to toggle.</param>
	/// <param name="enabled">Whether the kind should be enabled.</param>
	public void SetBeverageKindEnabled(BeverageKind kind, bool enabled)
	{
		var disabled = DisabledBeverageKinds.ToHashSet();
		_ = enabled ? disabled.Remove(kind) : disabled.Add(kind);
		Set(string.Join(",", disabled), nameof(DisabledBeverageKinds));
		DisabledBeverageKinds = null;
	}

	/// <summary>
	/// Returns the enabled beverage kinds as combined <see cref="NotificationTag"/> flags
	/// for notification hub registration.
	/// </summary>
	public NotificationTag GetBeverageKindNotificationTags() =>
		Enum.GetValues<BeverageKind>()
			.Where(k => !DisabledBeverageKinds.Contains(k))
			.Aggregate(NotificationTag.None, (combined, kind) => combined |= kind.NotificationTag);

	private void OnSettingChanged([CallerMemberName] string propertyName = "") => SettingChanged?.Invoke(propertyName);

	private static T SafeGet<T>(Func<string, T, T> getter, string name, T defaultValue)
	{
		try
		{
			return getter(name, defaultValue);
		}
		catch
		{
			return defaultValue;
		}
	}

	protected abstract bool GetBoolean(string name, bool defaultValue);

	private void Set<T>(T value, [CallerMemberName] string name = "")
	{
		switch (typeof(T))
		{
			case Type t when t == typeof(bool):
				SetBoolean(name, (bool)(object)value!);
				break;
			case Type t when t == typeof(DateTime):
				SetDateTime(name, (DateTime)(object)value!);
				break;
			case Type t when t == typeof(int):
				SetInt(name, (int)(object)value!);
				break;
			case Type t when t == typeof(string):
				SetString(name, (string?)(object)value!);
				break;
			default:
				throw new NotSupportedException($"Type {typeof(T)} is not supported by Set<T>.");
		}
		OnSettingChanged(name);
	}

	protected abstract void SetBoolean(string name, bool value);

	protected abstract DateTime GetDateTime(string name, DateTime defaultValue);
	protected abstract void SetDateTime(string name, DateTime value);

	protected abstract int GetInt(string name, int defaultValue);
	protected abstract void SetInt(string name, int value);

	protected abstract string? GetString(string name, string? defaultValue);
	protected abstract void SetString(string name, string? value);
}
