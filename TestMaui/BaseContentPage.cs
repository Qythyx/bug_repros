namespace TestMaui;

public abstract class BaseContentPage<T> : BaseContentPage
{
	protected BaseContentPage(Func<BaseContentPage, T> bindingContextProvider)
		=> BindingContext = bindingContextProvider.Invoke(this);

	protected T TypedContext => (T)BindingContext;
}

public abstract class BaseContentPage : ContentPage
{
	protected override void OnAppearing()
	{
		base.OnAppearing();
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
	}
}
