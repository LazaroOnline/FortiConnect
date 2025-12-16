using FortiConnect.InputSimulation;
using System.Runtime.InteropServices;
using System.Threading;

namespace FortiConnect.Services;

public interface IVirtualKeyboard
{
	// No support here for special keys/language like: "Hi{TAB}{ENTER}Bye", just plain text.
	void Text(string text);
	void KeyPress(VirtualKeyCode key);
}

/*
// https://github.com/HavenDV/H.InputSimulator
// https://github.com/GregsStack/InputSimulatorStandard
public class VirtualKeyboard_InputSimulatorStandard : IVirtualKeyboard
{
	private readonly InputSimulator _sim = new();

	public void Text(string text)
		=> _sim.Keyboard.TextEntry(text);

	public void KeyPress(VirtualKeyCode key)
		=> _sim.Keyboard.KeyPress(key);
}

// https://github.com/michaelnoonan/inputsimulator
// https://github.com/TChatzigiannakis/InputSimulatorPlus
public class VirtualKeyboard_InputSimulatorPlus : IVirtualKeyboard
{
	public void Text(string text)
		=> InputSimulator.SimulateTextEntry(text);

	public void KeyPress(VirtualKeyCode key)
		=> InputSimulator.SimulateKeyDown(key);
}
*/

public class VirtualKeyboard_WindowsApi : IVirtualKeyboard
{
	public void Text(string text)
	{
		if (string.IsNullOrEmpty(text))
			return;

		var inputs = ToInputList(text);
		uint sent = SendInputAndWaitInChunks(inputs);
		FlushKeyboardInput();
	}

	internal IEnumerable<INPUT> ToInputList(string text)
	{
		if (string.IsNullOrEmpty(text))
			return [];

		// 2 INPUTs for each char: key down + key up
		var inputs = new INPUT[text.Length * 2];
		int index = 0;

		foreach (char c in text)
		{
			inputs[index++] = ToInputKeyDown(c);
			inputs[index++] = ToInputKeyUp(c);
		}
		return inputs;
	}
	
	internal bool IsInputKeyUp(INPUT input)
	{
		return input.Type == ((uint)InputType.Keyboard)
			&& Contains(input.Data.Keyboard.Flags, (uint)KeyboardFlag.KeyUp);
	}

	internal bool Contains(uint flag, uint value)
		=> flag == (flag | value);

	internal IEnumerable<INPUT> ToInputKeysDownAndUp(char c)
		=> [ToInputKeyDown(c), ToInputKeyUp(c)];

	internal INPUT ToInputKeyDown(char c)
		=> ToInputKey(c, isKeyDown: true);

	internal INPUT ToInputKeyUp(char c)
		=> ToInputKey(c, isKeyDown: false);

	internal INPUT ToInputKey(char c, bool isKeyDown = true)
	{
		return new INPUT
		{
			Type = (ushort)InputType.Keyboard,
			Data = new MOUSEKEYBDHARDWAREINPUT
			{
				Keyboard = new KEYBDINPUT
				{
					KeyCode = 0,
					Scan = c,
					Flags = GetKeyboardFlags(keyUp: !isKeyDown, useUnicode: true),
					Time = 0,
					ExtraInfo = 0
				}
			}
		};
	}

	public static void FlushKeyboardInput()
	{
		// Force Windows to process the input queue
		SendInput([]);
		System.Diagnostics.Debug.WriteLine($"{nameof(VirtualKeyboard_WindowsApi)}: Flushed");
	}

	public void KeyPress(VirtualKeyCode key)
		=> KeyPressX(key);

	public void KeyPressX(VirtualKeyCode key, bool useScan = false)
	{
		var inputs = new List<INPUT>();

		void Down_WithKeyCode(ushort vk) => inputs.Add(Key_WithKeyCode(vk, false));
		void Up_WithKeyCode(ushort vk) => inputs.Add(Key_WithKeyCode(vk, true));

		void Down_Scan(ushort vk) => inputs.Add(Key_WithScan(vk, false));
		void Up_Scan(ushort vk) => inputs.Add(Key_WithScan(vk, true));

		var k = (ushort)key;
		if (useScan)
		{
			Down_Scan(k);
			Up_Scan(k);
		}
		else
		{
			Down_WithKeyCode(k);
			Up_WithKeyCode(k);
		}
		SendInputAndWait(inputs);
	}
	
	internal static INPUT Key_WithScan(ushort vk, bool up) => new INPUT
	{
		Type = (ushort)InputType.Keyboard,
		Data = new MOUSEKEYBDHARDWAREINPUT {
			Keyboard = {
				Scan = vk,
				Flags = GetKeyboardFlags(up, useUnicode: true)
			}
		}
	};

	internal static INPUT Key_WithKeyCode(ushort vk, bool up) => new INPUT
	{
		Type = (ushort)InputType.Keyboard,
		Data = new MOUSEKEYBDHARDWAREINPUT {
			Keyboard = {
				KeyCode = vk,
				Flags = GetKeyboardFlags(up, useUnicode: false)
			}
		}
	};

	public static ushort GetKeyboardFlags(bool keyUp, bool useUnicode = false)
	{
		if (useUnicode) {
			if (keyUp) {
				return ((ushort)KeyboardFlag.Unicode) | ((ushort)KeyboardFlag.KeyUp);
			} else {
				return (ushort)KeyboardFlag.Unicode;
			}
		}
		else {
			if (keyUp) {
				return (ushort)KeyboardFlag.KeyUp;
			} else {
				return (ushort)KeyboardFlag.KeyDown;
			}
		}
	}

	/// <summary>
	/// Default waiting milliseconds between chunks of key presses.
	/// 
	/// During integration testing, sending a set of characters to the "Notepad.exe" process:
	/// with 10ms it misses sending some characters during the special chars sequence.
	/// with 20ms it works  sending to Notepad.exe process. Use a bit more for safety (ie: 40).
	/// with 30ms it almost works but still sometimes fails to send the character after a space " ".
	/// Use 40ms to be safer.
	/// </summary>
	public const int DefaultWaitMs = 40;

	internal static uint SendInputAndWaitInChunks(IEnumerable<INPUT> inputs, int waitMs = DefaultWaitMs)
	{
		uint sent = 0;
		var inputChunks = SplitInputInChunks(inputs);
		foreach (var inputChunk in inputChunks)
		{
			var sentChunk = SendInputAndWait(inputChunk, waitMs);
			sent += sentChunk;
		}
		return sent;
	}

	internal static IEnumerable<IEnumerable<INPUT>> SplitInputInChunks(IEnumerable<INPUT> inputs, int chunks = 2)
	{
		return inputs.Chunk(chunks);
	}

	internal static uint SendInputAndWait(IEnumerable<INPUT> inputs, int waitMs = DefaultWaitMs)
	{
		var sent = SendInput(inputs);
		if (sent != inputs.Count()) {
			throw new InvalidOperationException($"SendInput failed: {Marshal.GetLastWin32Error()}");
		}
		FlushKeyboardInput();
		Thread.Sleep(waitMs); // Even if you wait between the key down and the key up, it doesn't spam the key, it just writes it once in Notepad.
		System.Diagnostics.Debug.WriteLine($"{nameof(VirtualKeyboard_WindowsApi)}: SendInput {inputs.Count()}");
		return sent;
	}

	internal static uint SendInput(IEnumerable<INPUT> inputs)
		=> SendInput((uint)inputs.Count(), inputs.ToArray(), Marshal.SizeOf<INPUT>());

	[DllImport("user32.dll")]
	internal static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
}