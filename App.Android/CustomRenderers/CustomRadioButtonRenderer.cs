using Android.Content;
using BeerApp.App.Droid.CustomRenderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(RadioButton), typeof(CustomRadioButtonRenderer))]
namespace BeerApp.App.Droid.CustomRenderers
{
	public class CustomRadioButtonRenderer : RadioButtonRenderer
	{
		public CustomRadioButtonRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<RadioButton> e)
		{
			base.OnElementChanged(e);

			// The Xamarin Forms RadioButton doesn't respect TextTransform in Android, to force it.
			if(Control != null && e.NewElement.TextTransform == TextTransform.Uppercase) {
				e.NewElement.Content = e.NewElement.Content.ToString().ToUpperInvariant();
			}
		}
	}
}
