namespace TestMaui;

public partial class MainPage : BaseContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Shell.SetForegroundColor(this, Colors.Red);
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;
        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        TheImage.WidthRequest = 50 + 200 * Random.Shared.NextDouble();
        TheImage.HeightRequest = 50 + 200 * Random.Shared.NextDouble();

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}


