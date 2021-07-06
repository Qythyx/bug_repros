using System;
using Xamarin.Forms;

namespace BeerApp.App.Forms.Views
{
	public abstract class BaseContentPage<T> : ContentPage
	{
		protected BaseContentPage(Func<ContentPage, T> bindingContextProvider)
			=> BindingContext = bindingContextProvider.Invoke(this);

		protected T TypedContext => (T)BindingContext;
	}
}
