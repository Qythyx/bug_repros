using Beerbox.App.Core.Services;

namespace Beerbox.App.Services;

public class Vibrator : IVibrator
{
	public void GenerateSelectionVibration() => HapticFeedback.Default.Perform(HapticFeedbackType.Click);

	public void GenerateSuccessVibration() => Vibration.Default.Vibrate(500);
}
