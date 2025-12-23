using System.Runtime.InteropServices;

namespace FortiConnect.Services;

public interface IKeyboardState
{
	bool IsCapsLock();
}

// Alternative to WindowsForms using Win32 (doesn't require a reference to WinForms).
// Avalonia decided not to support "CapsLock" state by design:
// https://github.com/AvaloniaUI/Avalonia/issues/2422
//var isCapsLock = Avalonia.Input.Key.CapsLock   Avalonia.Input.KeyStates.Toggled
public class KeyboardState : IKeyboardState
{
	[DllImport("user32.dll")]
	private static extern short GetKeyState(int nVirtKey);

	private const int VK_CAPITAL = 0x14;

	public bool IsCapsLock() => KeyboardState.IsCapsLockOn();

	public static bool IsCapsLockOn()
	{
		// System.Windows.Forms.Control.IsKeyLocked(System.Windows.Forms.Keys.CapsLock);
		return (GetKeyState(VK_CAPITAL) & 0x0001) != 0;
	}

}
