using System.Text;
using System.Text.RegularExpressions;

namespace FortiConnect.Services;

// Supports special language for special keys like: "Hi {TAB} {ENTER} Bye"
public interface ISendKeysService
{
	void SendKeys(string keys);
	string Escape(string keys);
}

// To use Windows forms you need to add to your csproj:
// <Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">
// <UseWPF>true</UseWPF>
// <UseWindowsForms>true</UseWindowsForms>
// https://stackoverflow.com/questions/38460253/how-to-use-system-windows-forms-in-net-core-class-library
// https://stackoverflow.com/questions/825651/how-can-i-send-the-f4-key-to-a-process-in-c
public class SendKeysWithWindowsForms : ISendKeysService
{
	/// <summary>
	/// List of characters that have special meaning in SendKeys in WindowsForms:
	/// (): key modifier group. For example: "+EC" would send "Shift+E" and "C", with "+(EC)" it sends "Shift+E" and "Shift+C".
	/// []: Brackets have no special meaning to SendKeys, but you must enclose them in braces. In other applications, brackets do have a special meaning that might be significant when dynamic data exchange (DDE) occurs.
	/// ~: ENTER key.
	/// %: ALT key modifier.
	/// ^: CTRL key modifier.
	/// +: SHIFT key modifier.
	/// </summary>
	public const string SENDKEYS_SPECIAL_CHARS = "{}()[]~%^+";

	/// <summary>
	/// List of characters that modify the following characters.
	/// </summary>
	public const string SENDKEYS_MODIFIERS = "%^+";

	public void SendKeys(string keys)
	{
		// textToWrite = ReverseCaseIfKeyboardHasCapsLock(textToWrite); // Looks like this is not required.

		// System.Windows.Forms.SendKeys.SendWait() has an issue when sending multiple commands at once:
		// If you try to send an escaped character like "{%}"
		// followed by a character that would require an special key combination like "$" that requires Shift,
		// Sending "{%}$" in one single call would produce "%4" instead of "%$".
		// To solve this issue we separate the text in commands and execute individually.
		var commandList = SplitSendKeysTextIntoCommands(keys);
		foreach (var command in commandList)
		{
			SendKeysWithWait(command);
			// https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys.flush
			// SendKeys.Flush();
		}
	}

	public void SendKeysWithWait(string keys)
	{
		// "SendKeysWithWindowsForms" is just an example, the actual reference to WinForms is commented out to remove it from this project.
		// System.Windows.Forms.SendKeys.SendWait(keys);
	}

	public string Escape(string literalTextToWrite)
	{
		if (literalTextToWrite == null)
		{
			return null;
		}
		// https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys.send#remarks
		// https://stackoverflow.com/questions/27452915/system-windows-forms-sendkeys-escape-keywords
		var regexReplacePattern = $"[{SENDKEYS_SPECIAL_CHARS.RegexEscapeAllSpecialChars()}]";
		return Regex.Replace(literalTextToWrite, regexReplacePattern, "{$0}");
	}

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

	public string ReverseCaseIfKeyboardHasCapsLock(string text)
	{
		var isCapsLock = KeyboardState.IsCapsLockOn();
		if (isCapsLock)
		{
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
}

public class SendKeysWithWindowsApi : ISendKeysService
{
	public const string SENDKEYS_SPECIAL_CHARS = "{}";

	private readonly IVirtualKeyboard _keyboard;

	public SendKeysWithWindowsApi(IVirtualKeyboard keyboard)
	{
		_keyboard = keyboard;
	}

	public string Escape(string literalTextToWrite)
	{
		if (literalTextToWrite == null) {
			return null;
		}
		var regexReplacePattern = $"[{SENDKEYS_SPECIAL_CHARS.RegexEscapeAllSpecialChars()}]";
		return Regex.Replace(literalTextToWrite, regexReplacePattern, "{$0}");
	}

	public void SendKeys(string input)
	{
		if (string.IsNullOrEmpty(input))
			return;
		var inputList = Parse(input);
		foreach (var inputChunk in inputList ?? []) {
			Send(inputChunk);
		}
	}

	public List<InputChunk> Parse(string input)
	{
		if (string.IsNullOrEmpty(input))
			return [];

		var inputList = new List<InputChunk>();

		var buffer = new StringBuilder();
		for (int i = 0; i < input.Length; i++)
		{
			char c = input[i];
			if (c == '{')
			{
				int end = input.IndexOf('}', i);
				if (end < 0) {
					// Instead of throwing exception and exploding, if a bracket without closing is found, treat it as a normal character.
					// throw new FormatException("Missing closing '}'");
					buffer.Append(c);
					continue;
				}

				inputList.Add(new InputChunkText { Text = buffer.ToString() });
				buffer.Clear();


				string tokenNoBrackets = input.Substring(i + 1, end - i - 1);
				string tokenWithBrackets = input.Substring(i, end - i + 1);
				var inputToken = GetInputFromToken(tokenWithBrackets);
				inputList.Add(inputToken);

				i = end;
			}
			else
			{
				buffer.Append(c);
			}
		}
		inputList.Add(new InputChunkText { Text = buffer.ToString() });
		return inputList;
	}

	public void Send(InputChunk inputChunk)
	{
		if (inputChunk is InputChunkKey inputChunkKey) {
			_keyboard.KeyPress(inputChunkKey.Key);
		}
		else if (inputChunk is InputChunkText inputChunkText && !string.IsNullOrEmpty(inputChunkText.Text)) {
			_keyboard.Text(inputChunkText.Text);
		}
	}

	public InputChunk GetInputFromToken(string token)
	{
		var key = KeyCode.ToVirtualKeyCode(token);
		if (key.HasValue)
		{
			var keyChunkKey = new InputChunkKey { Key = key.Value };
			return keyChunkKey;
		}

		if (token == KeyCode.BRAKET_OPEN) {
			var keyChunkBracket = new InputChunkText { Text = "{" };
			return keyChunkBracket;
		}

		// If the token doesn't match, treat it like plain text.
		// Unlike WinForms.SendKeys, but this avoids unintentional user errors.
		var keyChunkText = new InputChunkText { Text = token };
		return keyChunkText;

		// WinForms SendKeys-compatible:
		// {A} → writes A
		if (token.Length == 1) {
			var keyChunkSingleChar = new InputChunkText { Text = token };
			return keyChunkSingleChar;
		}
	}

	public abstract class InputChunk
	{
		// If this wanted to replicate all the WinForms SendKeys langage modifiers, it would need this:
		// public bool Ctrl = false;
		// public bool Shift = false;
		// public bool Alt = false;
	}

	public class InputChunkText : InputChunk
	{
		public string Text { get; set; } 
	}

	public class InputChunkKey : InputChunk
	{
		public VirtualKeyCode Key { get; set; }
	}

}

public static partial class Extensions
{
	/// <summary>
	/// Regex.Escape only escapes the MINIMAL set of characters, so it does NOT escape chars like "]" or "}".
	/// This version escapes all special characters so that the result can be used as a bag of matching single characters inside of a bigger pattern.
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static string RegexEscapeAllSpecialChars(this string input)
	{
		// Taken from LoDash v4 and other resources:
		// https://stackoverflow.com/questions/3561493/is-there-a-regexp-escape-function-in-javascript/3561711
		// https://lodash.com/docs#escapeRegExp
		var specialRegexCharacters = @"^$~.*+?()[]{}|-<>\";

		var inputParts = input.Select(c => specialRegexCharacters.Contains(c) ? ("\\" + c) : c.ToString());
		return string.Join("", inputParts);
	}
}
