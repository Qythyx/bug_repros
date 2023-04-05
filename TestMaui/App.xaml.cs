namespace TestMaui;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
        MainPage = new ShellMenu();
	}

    protected override void OnResume()
    {
        base.OnResume();
    }
}
