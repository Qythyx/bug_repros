using Android.Content;
using BeerApp.App.Droid.CustomRenderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Button), typeof(CustomButtonRenderer))]
namespace BeerApp.App.Droid.CustomRenderers
{
	public class CustomButtonRenderer : ButtonRenderer
	{
		public CustomButtonRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			base.OnElementChanged(e);

			if(Control != null) {
				Control.SetSingleLine(true);
				// The Xamarin Forms Button doesn't respect TextTransform in Android, to force it.
				if(e.NewElement.TextTransform == TextTransform.Uppercase) {
					e.NewElement.Text = e.NewElement.Text.ToUpperInvariant();
				}
				Control.SetTextColor(e.NewElement.TextColor.ToAndroid());
				Control.HorizontalScrollBarEnabled = false;
				Control.VerticalScrollBarEnabled = false;
			}
		}
	}
}
