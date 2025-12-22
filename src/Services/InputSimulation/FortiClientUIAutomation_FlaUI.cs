/*
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

namespace FortiConnect.Services;

// Requires "FlaUI.UIA3" NuGet package.

public class FortiClientUIAutomation_FlaUI : IFortiClientUIAutomation
{
	// This is just an example draft of how UI automation would be using FlaUI:
	public void Login(string userName, string password)
	{
		var app = FlaUI.Core.Application.Attach("FortiClient");
		using var automation = new UIA3Automation();
		var window = app.GetMainWindow(automation);
		var usernameTextbox = window.FindFirstDescendant(cf => cf.ByText("usernameTextbox"))?.AsTextBox();
		var passwordTextbox = window.FindFirstDescendant(cf => cf.ByText("passwordTextbox"))?.AsTextBox();
		usernameTextbox.Text = userName;
		passwordTextbox.Text = password;
		var connectButton = window.FindFirstDescendant(cf => cf.ByText("connect"))?.AsButton();
		connectButton?.Invoke();
	}
}
*/
