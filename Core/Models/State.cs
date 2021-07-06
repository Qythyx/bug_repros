using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BeerApp.Core.Models
{
	public class State : PropertyChangeNotifier
	{
		public static StateAccessor CreateAccessor()
		{
			var state = new State();
			return new StateAccessor(state, offers => state.SetOffers(offers));
		}

		private State() => Initialize();

		public ReadOnlyCollection<Offer> Offers
		{
			get => Get(new ReadOnlyCollection<Offer>(Array.Empty<Offer>()));
			private set => Set(value);
		}

		private void SetOffers(IEnumerable<Offer> offers)
			=> Offers = new(offers.ToArray());
	}
}
