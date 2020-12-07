using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FortiConnect.Services
{
	// This class is a test to try to find a better way of writing the login into FortiClient UI.
	// https://www.codeproject.com/Questions/316567/Send-Keys-to-background
	public class SendKeysToApp
	{
		delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

		[DllImport("user32.dll")]
		static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

		[DllImport("user32.dll")]
		static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll")]
		static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll")]
		static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

		const uint WM_CHAR = 0x102;
		const uint WM_PASTE = 0x0302;

		static IntPtr m_hWndEdit = IntPtr.Zero;

		static void WriteToWindow(string windowName = "FortiClient")
		{
			// Requires the user to have the app's window open, even if it is not focused.
			IntPtr hWnd = FindWindow(windowName, null);
			IntPtr hWndEdit = IntPtr.Zero;
			EnumChildWindows(hWnd, EnumChildWindowsCallback, hWndEdit);

			if (m_hWndEdit != null) // edit window found?
			{
				// Now you could use different messages to send text (WM_CHAR, WM_KEYDOWN, ...)
				// I decided to use the clipboard:
				Clipboard.SetText("this is magic!"); // Copy some text to the clipboard
				SendMessage(m_hWndEdit, WM_PASTE, 0, 0); // paste it to the target window
			}
		}

		static bool EnumChildWindowsCallback(IntPtr hWnd, IntPtr lParam)
		{
			// Search for notepads edit window - if we find it "false" is returned (means stop enumerating windows)

			var sb = new StringBuilder();
			GetClassName(hWnd, sb, 256);
			if (!sb.ToString().Contains("Edit"))
			{
				return true;
			}

			m_hWndEdit = hWnd; // Store the handle to notepads edit window (this is the window we want to send the messages to)
			return false;
		}
	}
}
