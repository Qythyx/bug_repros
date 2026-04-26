using MauiReactor;

namespace Beerbox.App;

internal static class AutomationIdExtensions
{
	internal static T ID<T>(this T control, AutoId<T> id)
		where T : VisualNode, MauiReactor.IElement => control.AutomationId(id);
}
