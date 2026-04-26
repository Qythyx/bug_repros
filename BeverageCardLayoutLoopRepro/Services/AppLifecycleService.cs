using Beerbox.App.Core.Services;

namespace Beerbox.App.Services;

public sealed class AppLifecycleService(AppSettings settings, DataManager dataManager)
{
	private readonly DataManager _dataManager = dataManager;

	public static bool IsInForeground { get; set; }

	public static void OnSleep() => IsInForeground = false;

	public static void OnResume() => IsInForeground = true;

	private bool _initialized;

	public void Initialize()
	{
		if (_initialized)
		{
			return;
		}

		_initialized = true;
		IsInForeground = true;

		Preferences.Clear();

		var mockDataManager = (MockDataManager)_dataManager;
		var connector = mockDataManager.MockServiceConnector;

		Task.Run(async () =>
			{
				connector.ArtificialLatencyMillis = 0;
				mockDataManager.CreateTestOffers();
				await mockDataManager.CreateTestAccountAsync();
				await mockDataManager.AddOffersToActiveOrderAsync();
				mockDataManager.CreateTestOrderHistory();
				settings.HasSeenHowItWorks = true;
			})
			.GetAwaiter()
			.GetResult();
	}
}
