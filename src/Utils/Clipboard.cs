using Avalonia.Threading;

namespace FortiConnect.Utils;

public static class Clipboard
{
	public static async Task SetTextAsync(string text, Control? control = null)
	{
		await Dispatcher.UIThread.Invoke(() =>
		{
			var controlForClipboard = control ?? new Window();
			return TopLevel.GetTopLevel(controlForClipboard)?.Clipboard?.SetTextAsync(text) ?? Task.CompletedTask;
		});
	}
}