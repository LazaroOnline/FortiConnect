using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace FortiConnect.Services;

public interface ISendKeysToProcessService
{
	public void WriteToProcess(Process process, string textToWrite);
	public string EscapeLiteralTextToWrite(string literalTextToWrite);
}

public class SendKeysToProcessService : ISendKeysToProcessService
{
	private readonly ISendKeysService _SendKeysService;
	public int DelayToShowWindow { get; set; } = 2000;

	public SendKeysToProcessService(ISendKeysService sendKeysService)
	{
		_SendKeysService = sendKeysService;
	}

	public void WriteToProcess(Process process, string textToWrite)
	{
		this.WriteToProcess(process, textToWrite, null);
	}

	public void WriteToProcess(Process process, string textToWrite, int? delayToShowWindow = null)
	{
		if (process != null)
		{
			IntPtr windowHandle = process.MainWindowHandle;
			ShowWindow(windowHandle, 1);
			//Thread.Sleep(delayToShowWindow);
			SetForegroundWindow(windowHandle);
			Thread.Sleep(delayToShowWindow ?? this.DelayToShowWindow);
			_SendKeysService.SendKeys(textToWrite);
		}
	}

	public string EscapeLiteralTextToWrite(string literalTextToWrite)
	{
		return _SendKeysService.Escape(literalTextToWrite);
	}

	#region inter-op
	
	// import the function in your class
	[DllImport ("User32.dll")]
	static extern int SetForegroundWindow(IntPtr point);
	
	[DllImport("user32.dll")]
	static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	#endregion
}
