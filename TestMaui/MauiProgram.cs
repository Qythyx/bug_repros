using Microsoft.Maui.Controls.Compatibility.Hosting;

namespace TestMaui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			}
#if IOS
			).ConfigureMauiHandlers(
                handlers => handlers
					.AddHandler(typeof(Shell), typeof(CustomShellRenderer))
#endif
			);

        return builder.Build();
	}
}

