namespace FortiConnect.UnitTests;

public class SendKeysToProcessServiceTest
{
	public const string specialChars = @"""09aAzZ_$$€&@#~`'´,.:;^|\/[](){}<>!?%*-+=";

	[Theory
	// Comment this line to run this test manually.
	//(Skip = "Skip, only execute manually")
	]
	//[InlineData("a", "Notepad")] // Simple key test
	//[InlineData("{invalid}", "Notepad")] // Invalid command > writes as plain text.
	[InlineData($"Start{KeyCode.TAB}AfterTab{KeyCode.ENTER}{KeyCode.ENTER}END.{KeyCode.ARROW_UP}{KeyCode.ARROW_UP}-{KeyCode.ARROW_DOWN}Special chars:{KeyCode.ENTER}{specialChars}", "Notepad")]
	public void IntegrationTest_WriteToProcess_Win32(string text, string processName)
	{
		var virtualKeyboard = new VirtualKeyboard_WindowsApi();
		//var virtualKeyboard = new VirtualKeyboard_HInputSimulator();
		//var virtualKeyboard = new VirtualKeyboard_InputSimulatorStandard();
		//var virtualKeyboard = new VirtualKeyboard_InputSimulatorPlus();

		ISendKeysService sendKeysService = new SendKeysWithWindowsApi(virtualKeyboard);
		IntegrationTest_WriteToProcess(text, processName, sendKeysService);
	}

	[Theory
	// Comment this line to run this test manually. (Would require adding WinForms reference back to the project)
	//(Skip = "Skip, WinForms reference was removed from the project")
	]
	//[InlineData("{invalid}", "Notepad")] // Invalid command > Throws exception (not because we want it, but because WinForms behaves like this).
	[InlineData($"Start+(shiftGroup){KeyCode.TAB}AfterTab{KeyCode.ENTER}{KeyCode.ENTER}END.", "Notepad")]
	public void IntegrationTest_WriteToProcess_WinForms(string text, string processName)
	{
		ISendKeysService sendKeysService = new SendKeysWithWindowsForms();
		IntegrationTest_WriteToProcess(text, processName, sendKeysService);
	}

	public void IntegrationTest_WriteToProcess(string text, string processName, ISendKeysService sendKeysService)
	{
		// Process names are case-insensitive
		// var process = Process.GetProcessesByName(processName).FirstOrDefault();
		var process = FortiConnector.GetExistingProcess(processName);
		var sut = new SendKeysToProcessService(sendKeysService);

		// Act:
		sut.WriteToProcess(process, text);

		Assert.True(true); // Observe manually
	}
}
