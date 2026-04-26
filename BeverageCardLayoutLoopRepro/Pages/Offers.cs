using System.Collections.ObjectModel;
using System.Globalization;
using Beerbox.App.Components;
using Beerbox.App.Core.Extensions;
using Beerbox.App.Core.Models;
using Beerbox.App.Core.Services;
using Beerbox.App.Resources.Localization;
using Beerbox.App.Theming;
using MauiReactor;

namespace Beerbox.App.Pages;

public sealed class Offers : Base<Offers.MyState>
{
	public sealed record MyState
	{
		public bool IsRefreshing { get; set; }
		public string? OfferIdToShow { get; set; }
		public bool ShowQuantitySelector { get; set; }
		public bool ShowAllOffers { get; set; }
		public int SelectedIndex { get; set; }
	}

	protected override string PageTitle => AppResources.OffersTitle;
	protected override ShellPage? TitleBarPage => ShellPage.Offers;

	private Offer[] _offers = [];
	private readonly ObservableCollection<Offer> _filteredOffers = [];
	private bool _hasRefreshedFirstTime;

	protected override void OnPageMounted()
	{
		PlatformContext.BeginInvokeOnMainThread(() =>
		{
			SetState(s => s.IsRefreshing = true);
			_ = LoadOffersAsync();
		});

		_ = Task.Run(async () =>
		{
			await Task.Delay(500);
			PlatformContext.BeginInvokeOnMainThread(NotifyPageAppearing);
		});
	}

	private void RebuildOffers()
	{
		static int IndexOfById(ObservableCollection<Offer> collection, string id, int startIndex)
		{
			for (var i = startIndex; i < collection.Count; i++)
			{
				if (collection[i].ID == id)
				{
					return i;
				}
			}
			return -1;
		}

		_offers = [.. DataManager.Offers];

		var newFiltered = _offers
			.Where(o => State.ShowAllOffers || !Settings.DisabledBeverageKinds.Contains(o.Beverage.Kind))
			.ToArray();
		if (!newFiltered.SequenceEqual(_filteredOffers))
		{
			var newById = newFiltered.ToDictionary(o => o.ID);

			// Phase 1: Remove or replace items (iterate backward to preserve indices)
			for (var i = _filteredOffers.Count - 1; i >= 0; i--)
			{
				if (newById.TryGetValue(_filteredOffers[i].ID, out var updated))
				{
					if (_filteredOffers[i] != updated)
					{
						_filteredOffers[i] = updated;
					}
				}
				else
				{
					_filteredOffers.RemoveAt(i);
				}
			}

			// Phase 2: Insert and move items to match the target order
			for (var i = 0; i < newFiltered.Length; i++)
			{
				if (i < _filteredOffers.Count)
				{
					if (_filteredOffers[i].ID != newFiltered[i].ID)
					{
						var oldIndex = IndexOfById(_filteredOffers, newFiltered[i].ID, i + 1);
						if (oldIndex >= 0)
						{
							_filteredOffers.Move(oldIndex, i);
						}
						else
						{
							_filteredOffers.Insert(i, newFiltered[i]);
						}
					}
				}
				else
				{
					_filteredOffers.Add(newFiltered[i]);
				}
			}

			// Only invalidate when _filteredOffers changed
			Invalidate();
		}
	}

	protected override VisualNode RenderContent()
	{
		RebuildOffers();

		var hasOffers = _offers.Length > 0;
		var filteredCount = _offers.Count(o => !Settings.DisabledBeverageKinds.Contains(o.Beverage.Kind));
		var hasFilteredOffers = filteredCount > 0;

		// Resolve notification deep-link: find the target offer's index
		if (State.OfferIdToShow is { } renderOfferId)
		{
			var offerAndIndex = _filteredOffers.Select((o, i) => (o, i)).FirstOrDefault(x => x.o.ID == renderOfferId);
			if (offerAndIndex != default && offerAndIndex.i >= 0 && offerAndIndex.i < _filteredOffers.Count)
			{
				SetState(s =>
				{
					s.SelectedIndex = offerAndIndex.i;
					s.OfferIdToShow = null;
				});
			}
		}

		var currentOffer =
			State.SelectedIndex >= 0 && State.SelectedIndex < _filteredOffers.Count
				? _filteredOffers[State.SelectedIndex]
				: null;

		return AbsoluteLayout(
				hasOffers ? null : Image().ThemeBackground().AbsoluteLayoutFill(),
				RefreshView(
						Grid(
								"*,Auto",
								"*",
								hasFilteredOffers
									? new PanCarouselView()
										.ID(AutomationIds.Offers.Carousel)
										.ItemsSource(
											_filteredOffers,
											offer =>
												new OfferCard()
													.Offer(offer)
													.QuantityInOrder(GetQuantityInOrder(offer))
													.BackendSettings(DataManager.BackendSettings)
													.OnFabTapped(() => SetState(s => s.ShowQuantitySelector = true))
													.FilteredCount(filteredCount)
													.TotalCount(_offers.Length)
													.ShowingAll(State.ShowAllOffers)
													.OnFilterChipTapped(HandleFilterChipTapped)
										)
										.SelectedIndex(State.SelectedIndex)
										.OnItemAppeared(HandleItemAppeared)
										.GridRow(0)
									: null,
								hasFilteredOffers
									? new CarouselIndicator()
										.ItemCount(_filteredOffers.Count)
										.SelectedIndex(State.SelectedIndex)
										.Size(AppFonts.FontSizeTiny)
										.SelectedColor(AppColors.BodyText.Resolve)
										.UnselectedColor(AppColors.BodyTextDimmed.Resolve)
										.DotSpacing(AppFonts.FontSizeSmall)
										.OnPositionSelected(i => SetState(s => s.SelectedIndex = i))
										.ViewMargin(AppStyles.SpacingExtra * 2, 0)
										.GridRow(1)
									: null,
								// Empty state: all offers filtered out (but offers exist)
								hasOffers && !hasFilteredOffers
									? VStack(
											Label(
													AppResources.OffersHiddenByFilters.Replace(
														"__COUNT__",
														_offers.Length.ToString(CultureInfo.InvariantCulture)
													)
												)
												.HorizontalTextAlignment(TextAlignment.Center),
											Label(
													AppResources.OffersShowAll,
													TapGestureRecognizer(() => SetState(s => s.ShowAllOffers = true))
												)
												.HorizontalTextAlignment(TextAlignment.Center)
												.TextDecorations(TextDecorations.Underline)
												.TextColor(AppColors.Primary.Resolve),
											Image().Source(AppImages.AppLogo).HeightRequest(AppStyles.LogoSize)
										)
										.BackgroundColor(AppColors.SemiTransparent.Resolve)
										.Padding(AppStyles.SpacingExtra)
										.Spacing(AppStyles.SpacingExtra)
										.VCenter()
										.ID(AutomationIds.Offers.FilteredEmpty)
									: null,
								!hasOffers
									? VStack(
											Label(AppResources.OffersNoCurrent1)
												.HorizontalTextAlignment(TextAlignment.Center),
											Label(AppResources.OffersNoCurrent2)
												.HorizontalTextAlignment(TextAlignment.Center),
											Image().Source(AppImages.AppLogo).HeightRequest(AppStyles.LogoSize)
										)
										.ID(AutomationIds.Offers.Empty)
										.BackgroundColor(AppColors.SemiTransparent.Resolve)
										.Padding(AppStyles.SpacingExtra)
										.Spacing(AppStyles.SpacingExtra)
										.VCenter()
										.InputTransparent(true)
									: null
							)
							.RowSpacing(AppStyles.Spacing / 2)
					)
					.IsRefreshing(State.IsRefreshing)
					.OnRefreshing(HandleRefresh)
					.RefreshColor(hasOffers ? AppColors.Primary.Resolve : AppColors.BodyText.Resolve)
					.AbsoluteLayoutFill(),
				State.ShowQuantitySelector && hasFilteredOffers && currentOffer != null
					? new QuantitySelector()
						.Offer(currentOffer)
						.QuantityInOrder(GetQuantityInOrder(currentOffer))
						.OnClose(() => SetState(s => s.ShowQuantitySelector = false))
					: null
			)
			.Set(Layout.SafeAreaEdgesProperty, SafeAreaEdges.None);
	}

	private void HandleItemAppeared(PanCardView.EventArgs.ItemAppearedEventArgs args)
	{
		var index = args.Index;
		if (index >= 0 && index < _filteredOffers.Count && index != State.SelectedIndex)
		{
			SetState(s => s.SelectedIndex = index);
		}
	}

	private int GetQuantityInOrder(Offer offer) =>
		DataManager.Order.Items.FirstOrDefault(i => i.Offer.ID == offer.ID)?.Quantity ?? 0;

	#region Data loading
	protected override void OnPageAppearing() => RebuildOffers();

	private void NotifyPageAppearing()
	{
		_hasRefreshedFirstTime = true;
		_ = HandleOfferStateCheckers();
	}

	private async void HandleRefresh() =>
		// During the initial load (before OnAppearing), IsRefreshing=true fires this
		// event. Skip alert checks (HandleOfferStateCheckers) at that point because the
		// page is not yet in the visual tree and DisplayAlertAsync would fail silently.
		await (_hasRefreshedFirstTime ? RefreshAsync() : LoadOffersAsync());

	private async Task LoadOffersAsync()
	{
		await DataManager.GetActiveOffersAsync().HandleException(Telemetry);
		PlatformContext.BeginInvokeOnMainThread(() => SetState(s => s.IsRefreshing = false));
	}

	private async Task RefreshAsync()
	{
		var stateCheckTask = HandleOfferStateCheckers();
		await LoadOffersAsync();
		await stateCheckTask;
	}

	private void HandleFilterChipTapped() => SetState(s => s.ShowAllOffers = !s.ShowAllOffers);

	#endregion Data loading

	#region Alert handlers

	private async Task HandleOfferStateCheckers()
	{
		try
		{
			var checker = OfferStateCheckers.GetActiveChecker(
				DataManager.Order,
				DataManager.Account,
				Settings,
				DataManager.BackendSettings,
				PlatformContext.HasShownPaymentMethodProblem
			);

			switch (checker)
			{
				case OfferStateCheckers.CheckerKind.OrderRecentlyClosed:
					Settings.PreviousOrderEndDate = DataManager.Order.EndDate;
					_ = await DataManager.RefreshActiveOrderAsync();
					await HandleOrderRecentlyClosed();
					break;

				case OfferStateCheckers.CheckerKind.NotLoggedIn:
					Settings.HasShownNotLoggedInAlert = true;
					Telemetry.TrackNotLoggedIn();
					await HandleNotLoggedIn();
					break;

				case OfferStateCheckers.CheckerKind.PaymentMethodProblem:
					PlatformContext.HasShownPaymentMethodProblem = true;
					Telemetry.TrackPaymentMethodProblem(DataManager.Account.PaymentMethod.Status);
					await HandlePaymentMethodProblem();
					break;

				case OfferStateCheckers.CheckerKind.OrderEndingSoon:
					Settings.OrderEndingSoonDate = DataManager.Order.EndDate;
					Settings.OrderEndingSoonShownDate = DateTime.Now;
					await HandleOrderEndingSoon();
					break;
			}
		}
		catch (Exception ex)
		{
			Telemetry.TrackException(ex);
		}
	}

	private async Task HandleNotLoggedIn()
	{
		var fixNow = await PlatformContext.DisplayAlertAsync(
			AppResources.OfferNotLoggedInTitle,
			AppResources.OfferNotLoggedInDescription,
			AppResources.OfferNotLoggedInLoginNow,
			AppResources.OfferNotLoggedInLoginLater
		);
		if (fixNow)
		{
			await PlatformContext.GoToAsync(ShellPage.Login);
		}
	}

	private static Task HandlePaymentMethodProblem() => Task.CompletedTask;

	private async Task HandleOrderRecentlyClosed() =>
		await PlatformContext.DisplayAlertAsync(
			AppResources.OrderRecentlyClosedTitle,
			AppResources.OrderRecentlyClosedDescription,
			AppResources.ButtonAccept
		);

	private async Task HandleOrderEndingSoon()
	{
		var text = AppResources.OrderEndingSoonDescription.Replace(
			"__CLOSE_DATETIME__",
			DataManager.Order.EndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
		);
		await PlatformContext.DisplayAlertAsync(AppResources.OrderEndingSoonTitle, text, AppResources.ButtonAccept);
	}

	#endregion Alert handlers
}
