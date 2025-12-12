using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FortiConnect.Services;

// https://stackoverflow.com/questions/38460253/how-to-use-system-windows-forms-in-net-core-class-library
public class ProcessWritterService : IProcessWritterService
{
	public int DelayToShowWindow { get; set; } = 2000;

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

			// textToWrite = ReverseCaseIfKeyboardHasCapsLock(textToWrite); // Looks like this is not required.

			var commandList = SplitSendKeysTextIntoCommands(textToWrite);
			foreach (var command in commandList)
			{
				// SendKeys.SendWait has an issue when sending multiple commands at once:
				// If you try to send an escaped character like "{%}"
				// followed by a character that would require an special key combination like "$" that requires Shift,
				// Sending "{%}$" in one single call would produce "%4" instead of "%$".
				// To solve this issue we separate the text in commands and execute individually.

				SendKeys.SendWait(command);

				// https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys.flush
				// SendKeys.Flush();
			}
		}
	}

	// https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys?view=net-5.0
	public static class WriteKey
	{
		public const string ENTER = "{ENTER}";
		public const string TAB = "{TAB}";

		public const string CAPS_LOCK = "{CAPSLOCK}";
		public const string BACKSPACE = "{BACKSPACE}";
		public const string DELETE = "{DELETE}";
		public const string HELP = "{HELP}";
		public const string INSERT = "{INSERT}";
		public const string NUMLOCK = "{NUMLOCK}";
		public const string PRTSC = "{PRTSC}";
		public const string SCROLLLOCK = "{SCROLLLOCK}";
		public const string BREAK = "{BREAK}";

		public const string PAGE_UP = "{PGUP}";
		public const string PAGE_DOWN = "{PGDN}";
		public const string HOME = "{HOME}";
		public const string END = "{END}";
		public const string ESCAPE = "{ESC}";

		public const string ADD = "{ADD}";
		public const string SUBTRACT = "{SUBTRACT}";
		public const string MULTIPLY = "{MULTIPLY}";
		public const string DIVIDE = "{DIVIDE}";

		public const string F1  = "{F1}";
		public const string F2  = "{F2}";
		public const string F3  = "{F3}";
		public const string F4  = "{F4}";
		public const string F5  = "{F5}";
		public const string F6  = "{F6}";
		public const string F7  = "{F7}";
		public const string F8  = "{F8}";
		public const string F9  = "{F9}";
		public const string F10 = "{F10}";
		public const string F11 = "{F11}";
		public const string F12 = "{F12}";
		public const string F13 = "{F13}";
		public const string F14 = "{F14}";
		public const string F15 = "{F15}";
		public const string F16 = "{F16}";

		public const string ARROW_UP = "{UP}";
		public const string ARROW_DOWN = "{DOWN}";
		public const string ARROW_LEFT = "{LEFT}";
		public const string ARROW_RIGHT = "{RIGHT}";
		
		public const string SHIFT = "+";
		public const string ALT = "%";
		public const string CONTROL = "^";
	}
	
	public string GetKeyTab()
	{
		return WriteKey.TAB;
		//return GetSpecialKey(System.Windows.Forms.Keys.Tab);
	}
	
	public string GetKeyEnter()
	{
		return WriteKey.ENTER;
		//return GetSpecialKey(System.Windows.Forms.Keys.Enter);
	}

	public string GetSpecialKey(System.Windows.Forms.Keys key)
	{
		switch(key) {
			case Keys.Enter : return WriteKey.ENTER;
			case Keys.Tab : return WriteKey.TAB;
			default: throw new NotImplementedException("If more special keys are required, keep adding them to the supported list in source code.");
		}
	}

	/// <summary>
	/// List of characters that have special meaning in SendKeys.
	/// </summary>
	public const string SENDKEYS_SPECIAL_CHARS = "{}()[]~%^+";

	/// <summary>
	/// List of characters that modify the following characters.
	/// </summary>
	public const string SENDKEYS_MODIFIERS = "%^+";
	
	public List<string> SplitSendKeysTextIntoCommands(string text)
	{
		var commandList = new List<string>();
		if (string.IsNullOrEmpty(text)) {
			return commandList;
		}

		var command = "";
		foreach (var c in text)
		{
			command += c;

			var finnishEscaped = command.StartsWith("{") && command.EndsWith("}") && command != "{}";
			var finnishSquareBraquets = command.StartsWith("[") && command.EndsWith("]");

			var isModCommand = SENDKEYS_MODIFIERS.Any(m => command.StartsWith(m));
			var isCommandSingleChar = command.Length == 1;
			var commandWithoutModKeys = command.TrimStart('+', '%', '^');
			var finishModCommandGroup = isModCommand && commandWithoutModKeys.StartsWith("(") && commandWithoutModKeys.EndsWith(")");
			var finishModCommandSingleKey = isModCommand && commandWithoutModKeys.Length == 1 && !SENDKEYS_SPECIAL_CHARS.Contains(commandWithoutModKeys.First());
			var specialCharsWithOneChars = "~";
			var isSpecialSingleChar = isCommandSingleChar && specialCharsWithOneChars.Contains(command.First());
			var isNormalCharacter = isCommandSingleChar && !SENDKEYS_SPECIAL_CHARS.Contains(command.First()); 

			var finishCommand =
				   finnishEscaped
				|| finnishSquareBraquets
				|| finnishSquareBraquets
				|| finishModCommandGroup
				|| finishModCommandSingleKey
				|| isSpecialSingleChar
				|| isNormalCharacter
				;

			if (finishCommand)
			{
				commandList.Add(command);
				command = "";
			}
		}
		if (command != "") {
			commandList.Add(command);
		}
		return commandList;
	}

	public string EscapeLiteralTextToWrite(string literalTextToWrite)
	{
		if (literalTextToWrite == null) {
			return null;
		}

		// https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys.send#remarks
		// https://stackoverflow.com/questions/27452915/system-windows-forms-sendkeys-escape-keywords
		var regexReplacePattern = $"[{RegexEscapeAllSpecialChars(SENDKEYS_SPECIAL_CHARS)}]";
		return Regex.Replace(literalTextToWrite, regexReplacePattern, "{$0}");
	}
	
	/// <summary>
	/// Regex.Escape only escapes the MINIMAL set of characters, so it does NOT escape chars like "]" or "}".
	/// This version escapes all special characters so that the result can be used as a bag of matching single characters inside of a bigger pattern.
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public string RegexEscapeAllSpecialChars(string input)
	{
		// Taken from LoDash v4 and other resources:
		// https://stackoverflow.com/questions/3561493/is-there-a-regexp-escape-function-in-javascript/3561711
		// https://lodash.com/docs#escapeRegExp
		var specialRegexCharacters = @"^$~.*+?()[]{}|-<>\";
		
		var inputParts = input.Select(c => specialRegexCharacters.Contains(c)? ("\\" + c) : c.ToString());
		return string.Join("", inputParts);
	}
	
	public string ReverseCaseIfKeyboardHasCapsLock(string text)
	{
		var isCapsLock = System.Windows.Forms.Control.IsKeyLocked(Keys.CapsLock);
		if (isCapsLock) {
			return ReverseCase(text);
		}
		return text;
	}

	public string ReverseCase(string input)
	{
		// https://stackoverflow.com/questions/3681552/reverse-case-of-all-alphabetic-characters-in-c-sharp-string
		return new string(
		    input.Select(c => char.IsLetter(c) ? (char.IsUpper(c) ?
				char.ToLower(c) : char.ToUpper(c)) : c
			).ToArray());
	}

	#region inter-op
	
	// import the function in your class
	[DllImport ("User32.dll")]
	static extern int SetForegroundWindow(IntPtr point);
	
	// https://stackoverflow.com/questions/38460253/how-to-use-system-windows-forms-in-net-core-class-library
	// <Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">
	// <UseWPF>true</UseWPF>
	// <UseWindowsForms>true</UseWindowsForms>

	// https://stackoverflow.com/questions/825651/how-can-i-send-the-f4-key-to-a-process-in-c
	[DllImport("user32.dll")]
	static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	#endregion

}
