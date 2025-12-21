/*
using System.Windows.Automation;

// Requires adding a reference in the project to: UIAutomationClient.dll and  UIAutomationTypes.dll:
// C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0\UIAutomationClient.dll
// C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0\UIAutomationTypes.dll

namespace FortiConnect.Services;

public class FortiClientUIAutomation_UIA : IFortiClientUIAutomation
{
	// This is just an example draft of how UI automation would be using UIA:
	public void Login(string userName, string password)
	{
		string windowName = "FortiClient";
		AutomationElement window = GetWindow(windowName);
		// Username can be stored in the UI already
		if (userName != null) {
			SetTextboxValue(window, "usernameTextbox", userName);
		}
		SetTextboxValue(window, "passwordTextbox", password);
		ClickButton(window, "Connect");
	}

	public static void ClickButton(AutomationElement window, string buttonText)
	{
		AutomationElement button = GetButton(window, buttonText);
		if (button != null && button.TryGetCurrentPattern(InvokePattern.Pattern, out object pattern)) {
			((InvokePattern)pattern).Invoke();
		}
	}

	public static string? GetTextboxValue(AutomationElement window, string textboxName)
	{
		string? text = null;
		var textBox = GetTextboxById(window, textboxName);
		if (textBox.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern)) {
			text = ((ValuePattern)pattern).Current.Value;
		}
		return text;
	}

	public static void SetTextboxValue(AutomationElement window, string textboxName, string newText)
	{
		var textBox = GetTextboxById(window, textboxName);
		if (textBox.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern)) {
			((ValuePattern)pattern).SetValue(newText);
		}
	}

	public static AutomationElement GetButton(AutomationElement window, string buttonText)
		=> GetControlByTypeAndName(window, ControlType.Button, buttonText);

	public static AutomationElement GetTextboxById(AutomationElement window, string id)
		=> GetControlByTypeAndId(window, ControlType.Edit, id);

	public static AutomationElement GetTextboxByName(AutomationElement window, string name)
		=> GetControlByTypeAndName(window, ControlType.Button, name);

	public static AutomationElement GetControlByTypeAndId(AutomationElement window, ControlType controlType, string id)
	{
		var condition = new AndCondition(
			 new PropertyCondition(AutomationElement.ControlTypeProperty, controlType)
			, new PropertyCondition(AutomationElement.AutomationIdProperty, id)
		);
		return window.FindFirst(TreeScope.Descendants, condition);
	}

	public static AutomationElement GetControlByTypeAndName(AutomationElement window, ControlType controlType, string name)
	{
		var condition = new AndCondition(
			new PropertyCondition(AutomationElement.ControlTypeProperty, controlType),
			new PropertyCondition(AutomationElement.NameProperty, name)
		);
		AutomationElement control = window.FindFirst(TreeScope.Descendants, condition);
		return control;
	}

	public static AutomationElement GetWindow(string windowName)
		=> AutomationElement.RootElement.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, windowName));
}
*/
