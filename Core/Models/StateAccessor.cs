using System.Collections.Generic;
using static BeerApp.Core.Models.StateAccessor;

namespace BeerApp.Core.Models
{
	public record StateAccessor(State State, OffersSetter SetOffers)
	{
		public delegate void OffersSetter(IEnumerable<Offer> offers);
	}
}
