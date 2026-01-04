using System.Text;
using System.Text.RegularExpressions;

namespace FortiConnect.Services;

// Supports special language for special keys like: "Hi {TAB} {ENTER} Bye"
public interface ISendKeysService
{
	void SendKeys(string keys);
	string? Escape(string keys);
}

// To use Windows forms you need to add to your csproj in a PropertyGroup:
// <UseWindowsForms>true</UseWindowsForms>
// and add "-windows" to the TargetFramework, example:
// <TargetFramework>net10.0-windows</TargetFramework>
public class SendKeysWithWindowsForms : ISendKeysService
{

	public void SendKeys(string keys)
	{
		// textToWrite = ReverseCaseIfKeyboardHasCapsLock(textToWrite); // Looks like this is not required.

		// System.Windows.Forms.SendKeys.SendWait() has an issue when sending multiple commands at once:
		// If you try to send an escaped character like "{%}"
		// followed by a character that would require an special key combination like "$" that requires Shift,
		// Sending "{%}$" in one single call would produce "%4" instead of "%$".
		// To solve this issue we separate the text in commands and execute individually.
		var commandList = SendKeysLang.SplitSendKeysTextIntoCommands(keys);
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

	public string? Escape(string literalTextToWrite)
	{
		return SendKeysLang.EscapeLiteralTextToWrite(literalTextToWrite);
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

	public string? Escape(string literalTextToWrite)
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
