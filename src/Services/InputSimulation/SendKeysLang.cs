using System.Text.RegularExpressions;

namespace FortiConnect.Services;

/// <summary>
/// Language of the WindowsForms "SendKeys" text format:
/// https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys.send#remarks
/// </summary>
public class SendKeysLang
{
	public static List<string> SplitSendKeysTextIntoCommands(string text)
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

			var isModCommand = GetKeyModifierList().Any(m => command.StartsWith(m));
			var isCommandSingleChar = command.Length == 1;
			var commandWithoutModKeys = command.TrimStart('+', '%', '^');
			var finishModCommandGroup = isModCommand && commandWithoutModKeys.StartsWith("(") && commandWithoutModKeys.EndsWith(")");
			var finishModCommandSingleKey = isModCommand && commandWithoutModKeys.Length == 1 && !GetSpecialMeaningCharList().Contains(commandWithoutModKeys.First());
			var specialCharsWithOneChars = "~";
			var isSpecialSingleChar = isCommandSingleChar && specialCharsWithOneChars.Contains(command.First());
			var isNormalCharacter = isCommandSingleChar && !GetSpecialMeaningCharList().Contains(command.First()); 

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

	public static string? EscapeLiteralTextToWrite(string? literalTextToWrite)
	{
		if (literalTextToWrite == null) {
			return null;
		}

		// https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys.send#remarks
		// https://stackoverflow.com/questions/27452915/system-windows-forms-sendkeys-escape-keywords
		var regexReplacePattern = $"[{GetSpecialMeaningCharList().RegexEscapeAllSpecialChars()}]";
		return Regex.Replace(literalTextToWrite, regexReplacePattern, "{$0}");
	}

	public static bool IsKnownSpecialKey(string? command)
	{
		var allSpecialKeyCommands = GetAllSpecialKeyCommands();
		var isKnownCommand = allSpecialKeyCommands.Any(k => k.Equals(command, StringComparison.InvariantCultureIgnoreCase));
		return isKnownCommand;
	}

	private static List<string>? AllSpecialKeyCommands = null;

	public static List<string> GetAllSpecialKeyCommands()
	{
		if (AllSpecialKeyCommands != null) { return AllSpecialKeyCommands; }
		var specialKeyValues = Enum.GetValues<SpecialKey>().Select(v => v.GetValue()).ToList();
		AllSpecialKeyCommands = specialKeyValues;
		return specialKeyValues;
	}

	private static string? KeyModifierList = null;

	/// <summary>String containing all the KeyModifier characters.</summary>
	public static string GetKeyModifierList()
	{
		if (KeyModifierList != null) { return KeyModifierList; }
		var keyModifierValues = Enum.GetValues<KeyModifier>().Select(v => v.GetValue());
		var result = string.Join("", keyModifierValues);
		KeyModifierList = result;
		return result;
	}

	private static string? SpecialMeaningCharList = null;

	/// <summary>List of characters that have special meaning in SendKeys.</summary>
	/// <returns>All SpecialMeaningChar and KeyModifier enum values."{}()[]~%^+"</returns>
	public static string GetSpecialMeaningCharList()
	{
		if (SpecialMeaningCharList != null) { return SpecialMeaningCharList; }
		var specialMeaningCharValues = Enum.GetValues<SpecialMeaningChar>().Select(v => v.GetValue());
		var keyModifierValues = Enum.GetValues<KeyModifier>().Select(v => v.GetValue());
		var result = string.Join("", specialMeaningCharValues.Concat(keyModifierValues));
		SpecialMeaningCharList = result;
		return result;
	}

	/// <summary>List of characters that has special meaning (and must be escaped), apart from KeyModifiers.</summary>
	public enum SpecialMeaningChar
	{
		// Curly-braces {} are used to specify special keys. Example: {ENTER}.
		 SpecialCharOpen = '{'
		,SpecialCharClose = '}'
		// Parenthesis () are used for key-groups for modifiers (Ctrl, Shift, Alt). Example: ^(cv).
		,KeyModifierGroupOpen = '('
		,KeyModifierGroupClose = ')'
		// Brackets ([ ]) have no special meaning to SendKeys, but you must enclose them in braces. In other applications, brackets do have a special meaning that might be significant when dynamic data exchange (DDE) occurs.
		,SquareBracketOpen = '['
		,SquareBracketClose = ']'
		,Enter = '~' // Alternative character for the {Enter} key.
	}

	/// <summary>List of characters that modify the following character in the sequence.</summary>
	public enum KeyModifier
	{
		SHIFT = '+',
		ALT = '%',
		CONTROL = '^'
	}

	// The names in this enum list must be the same as the code value required by WindowsForms, example: ENTER > {ENTER}.
	// https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys?view=net-5.0
	public enum SpecialKey
	{
		 ENTER
		,TAB
		,CAPSLOCK
		,BACKSPACE
		,DELETE
		,HELP
		,INSERT
		,NUMLOCK
		,PRTSC
		,SCROLLLOCK
		,BREAK
		,PGUP
		,PGDN
		,HOME
		,END
		,ESC
		,ADD
		,SUBTRACT
		,MULTIPLY
		,DIVIDE
		,F1
		,F2
		,F3
		,F4
		,F5
		,F6
		,F7
		,F8
		,F9
		,F10
		,F11
		,F12
		,F13
		,F14
		,F15
		,F16
		,UP
		,DOWN
		,LEFT
		,RIGHT
	}
}

public static class KeyExtensions
{
	public static string GetValue(this SendKeysLang.KeyModifier keyModifier) => ((char)keyModifier).ToString();

	public static string GetValue(this SendKeysLang.SpecialMeaningChar specialMeaningChar) => ((char)specialMeaningChar).ToString();

	public static string GetValue(this SendKeysLang.SpecialKey specialKey) => "{" + specialKey + "}";
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
