namespace TestMaui;

public class SizeLinkedImage : Image
{
    private double _widthToHeightAspectRatio = double.NaN;

    public View HeightTarget
    {
        get => (View)GetValue(HeightTargetProperty);
        set
        {
            SetValue(HeightTargetProperty, value);
            value.SizeChanged += Value_SizeChanged;
        }
    }

    public View WidthTarget
    {
        get => (View)GetValue(WidthTargetProperty);
        set
        {
            SetValue(WidthTargetProperty, value);
            value.SizeChanged += Value_SizeChanged;
        }
    }

    private void Value_SizeChanged(object? sender, EventArgs e)
    {
        SetSize();
    }

    public static readonly BindableProperty HeightTargetProperty = BindableProperty.Create(
        nameof(HeightTarget),
        typeof(View),
        typeof(SizeLinkedImage),
        null,
        BindingMode.OneTime,
        null,
        (bindable, oldValue, newValue) => ((SizeLinkedImage)bindable).HeightTarget = (View)newValue
    );

    public static readonly BindableProperty WidthTargetProperty = BindableProperty.Create(
        nameof(WidthTarget),
        typeof(View),
        typeof(SizeLinkedImage),
        null,
        BindingMode.OneTime,
        null,
        (bindable, oldValue, newValue) => ((SizeLinkedImage)bindable).WidthTarget = (View)newValue
    );

    private void SetSize()
    {
        if (!double.IsNaN(_widthToHeightAspectRatio))
        {
            if (HeightTarget is not null)
            {
                HeightRequest = HeightTarget.Height;
                WidthRequest = HeightTarget.Height * _widthToHeightAspectRatio;
            }
            if (WidthTarget is not null)
            {
                HeightRequest = WidthTarget.Width / _widthToHeightAspectRatio;
                WidthRequest = WidthTarget.Width;
            }
        }
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (double.IsNaN(_widthToHeightAspectRatio))
        {
            _widthToHeightAspectRatio = Width / Height;
        }
#if ANDROID
        // Why is this required for Android? It should not be.
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(100);
            SetSize();
        });
#else
		SetSize();
#endif
    }
}
