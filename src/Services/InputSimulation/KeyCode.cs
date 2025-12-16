namespace FortiConnect.Services;

// https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys?view=net-5.0
public static class KeyCode
{
	public const string BRAKET_OPEN = "{{}";

	public const string ENTER      = "{ENTER}";
	public const string TAB        = "{TAB}";

	public const string CAPS_LOCK  = "{CAPSLOCK}";
	public const string BACKSPACE  = "{BACKSPACE}";
	public const string BS         = "{BS}";   // Alternative to BACKSPACE
	public const string BKSP       = "{BKSP}"; // Alternative to BACKSPACE
	public const string DELETE     = "{DELETE}";
	public const string DEL        = "{DEL}";  // Alternative to DELETE
	public const string HELP       = "{HELP}";
	public const string INSERT     = "{INSERT}";
	public const string INS        = "{INS}";  // Alternative to INSERT
	public const string NUMLOCK    = "{NUMLOCK}";
	public const string PRTSC      = "{PRTSC}";
	public const string SCROLLLOCK = "{SCROLLLOCK}";
	public const string BREAK      = "{BREAK}";

	public const string PAGE_UP    = "{PGUP}";
	public const string PAGE_DOWN  = "{PGDN}";
	public const string HOME       = "{HOME}";
	public const string END        = "{END}";
	public const string ESCAPE     = "{ESC}";

	public const string ADD        = "{ADD}";
	public const string SUBTRACT   = "{SUBTRACT}";
	public const string MULTIPLY   = "{MULTIPLY}";
	public const string DIVIDE     = "{DIVIDE}";

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

	public const string ARROW_UP    = "{UP}";
	public const string ARROW_DOWN  = "{DOWN}";
	public const string ARROW_LEFT  = "{LEFT}";
	public const string ARROW_RIGHT = "{RIGHT}";

	public const string SHIFT   = "+";
	public const string ALT     = "%";
	public const string CONTROL = "^";
	
	public static readonly Dictionary<string, VirtualKeyCode> CodeToKey =
		new(StringComparer.OrdinalIgnoreCase) {
		 [ENTER      ]  = VirtualKeyCode.RETURN
		,[TAB        ]  = VirtualKeyCode.TAB

		,[CAPS_LOCK  ] = VirtualKeyCode.CAPSLOCK
		,[BACKSPACE  ] = VirtualKeyCode.BACKSPACE
		,[BKSP       ] = VirtualKeyCode.BACKSPACE
		,[BS         ] = VirtualKeyCode.BACKSPACE
		,[DELETE     ] = VirtualKeyCode.DELETE
		,[DEL        ] = VirtualKeyCode.DELETE
		,[HELP       ] = VirtualKeyCode.HELP
		,[INSERT     ] = VirtualKeyCode.INSERT
		,[INS        ] = VirtualKeyCode.INSERT
		,[NUMLOCK    ] = VirtualKeyCode.NUMLOCK
		,[PRTSC      ] = VirtualKeyCode.PRTSC
		,[SCROLLLOCK ] = VirtualKeyCode.SCROLL
		,[BREAK      ] = VirtualKeyCode.BREAK

		,[PAGE_UP    ] = VirtualKeyCode.PAGE_UP
		,[PAGE_DOWN  ] = VirtualKeyCode.PAGE_DOWN
		,[HOME       ] = VirtualKeyCode.HOME
		,[END        ] = VirtualKeyCode.END
		,[ESCAPE     ] = VirtualKeyCode.ESCAPE

		,[ADD        ] = VirtualKeyCode.ADD
		,[SUBTRACT   ] = VirtualKeyCode.SUBTRACT
		,[MULTIPLY   ] = VirtualKeyCode.MULTIPLY
		,[DIVIDE     ] = VirtualKeyCode.DIVIDE

		,[F1         ] = VirtualKeyCode.F1
		,[F2         ] = VirtualKeyCode.F2
		,[F3         ] = VirtualKeyCode.F3
		,[F4         ] = VirtualKeyCode.F4
		,[F5         ] = VirtualKeyCode.F5
		,[F6         ] = VirtualKeyCode.F6
		,[F7         ] = VirtualKeyCode.F7
		,[F8         ] = VirtualKeyCode.F8
		,[F9         ] = VirtualKeyCode.F9
		,[F10        ] = VirtualKeyCode.F10
		,[F11        ] = VirtualKeyCode.F11
		,[F12        ] = VirtualKeyCode.F12
		,[F13        ] = VirtualKeyCode.F13
		,[F14        ] = VirtualKeyCode.F14
		,[F15        ] = VirtualKeyCode.F15
		,[F16        ] = VirtualKeyCode.F16

		,[ARROW_RIGHT] = VirtualKeyCode.RIGHT
		,[ARROW_LEFT ] = VirtualKeyCode.LEFT
		,[ARROW_UP   ] = VirtualKeyCode.UP
		,[ARROW_DOWN ] = VirtualKeyCode.DOWN
	};

	public static VirtualKeyCode? ToVirtualKeyCode(string keyCode)
	{
		if (CodeToKey.TryGetValue(keyCode, out var code)) {
			return code; 
		}
		return null;
	}

	public static string GetKeyTab()
	{
		return KeyCode.TAB;
		//return GetKeyCode(Avalonia.Input.Key.Tab);
	}

	public static string GetKeyEnter()
	{
		return KeyCode.ENTER;
		//return GetKeyCode(Avalonia.Input.Key.Enter);
	}

	public static string GetKeyCode(Avalonia.Input.Key key)
	{
		switch (key)
		{
			case Avalonia.Input.Key.Enter: return KeyCode.ENTER;
			case Avalonia.Input.Key.Tab: return KeyCode.TAB;
			default: throw new NotImplementedException("If more special keys are required, keep adding them to the supported list in source code.");
		}
	}
}

public enum VirtualKeyCode //: UInt16
{
	SHIFT = 0x10,
	CONTROL = 0x11,
	MENU = 0x12, // Alt key

	RETURN = 0x0D,
	TAB = 0x09,

	CAPSLOCK = 0x14,
	BACKSPACE = 0x08,
	DELETE = 0x2E,
	HELP = 0x2F,
	INSERT = 0x2D,
	NUMLOCK = 0x90,
	PRTSC = 0x2A, // Print-Screen
	SCROLL = 0x91,
	BREAK = 0x03,

	PAGE_UP = 0x21,
	PAGE_DOWN = 0x22,
	HOME = 0x24,
	END = 0x23,
	ESCAPE = 0x1B,


	ADD = 0x6B,
	SUBTRACT = 0x6D,
	MULTIPLY = 0x6A,
	DIVIDE = 0x6F,

	F1 = 0x70,
	F2 = 0x71,
	F3 = 0x72,
	F4 = 0x73,
	F5 = 0x74,
	F6 = 0x75,
	F7 = 0x76,
	F8 = 0x77,
	F9 = 0x78,
	F10 = 0x79,
	F11 = 0x7A,
	F12 = 0x7B,
	F13 = 0x7C,
	F14 = 0x7D,
	F15 = 0x7E,
	F16 = 0x7F,

	RIGHT = 0x27,
	LEFT = 0x25,
	UP = 0x26,
	DOWN = 0x28,

	SPACE = 0x20

	// ...
	// More at: https://github.com/michaelnoonan/inputsimulator/blob/master/WindowsInput/Native/VirtualKeyCode.cs
}
