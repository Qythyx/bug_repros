using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeerApp.Core.Models;
using MvvmHelpers.Commands;
using MvvmHelpers.Interfaces;

namespace BeerApp.Core.ViewModels
{
	public class OffersViewModel : PropertyChangeNotifier
	{
		private readonly StateAccessor _accessor;

		public OffersViewModel(Action<Action> mainThreadInvoker) : base()
		{
			_accessor = State.CreateAccessor();
			State = _accessor.State;

			State.PropertyChanged += (sender, e) => {
				if(e.PropertyName == nameof(State.Offers)) {

					// This works fine
					mainThreadInvoker.Invoke(() => {
						ObsOffers.Clear();
						foreach(var offer in State.Offers) {
							ObsOffers.Add(offer);
						}
					});

					// This throws an exception
					//ObsOffers.Clear();
					//foreach(var offer in State.Offers) {
					//	ObsOffers.Add(offer);
					//}

					OnPropertyChanged(nameof(HasOffers));
				}
			};

			Refresh = new AsyncCommand(DoRefresh);
			Initialize();
		}

		public State State { get; }

		private async Task DoRefresh()
		{
			await Task.Run(() => {
				var thread = new Thread(new ThreadStart(() => {
					Thread.Sleep(3000);
					var offers = Enumerable.Range(0, 10)
						.Select(i => new Offer($"Offer {i}"));
					_accessor.SetOffers(offers);
				}));
				thread.Start();
				thread.Join();
			});
			IsRefreshing = false;
		}

		public IAsyncCommand Refresh { get; }

		public bool IsRefreshing
		{
			get => Get(false);
			set => Set(value);
		}

		public bool HasOffers => State.Offers.Count > 0;

		public ObservableCollection<Offer> ObsOffers { get; }
			= new ObservableCollection<Offer>();
	}
}
