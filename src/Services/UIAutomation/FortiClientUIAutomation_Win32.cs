using System.Text;
using System.Runtime.InteropServices;

namespace FortiConnect.Services;

public interface IFortiClientUIAutomation
{
	void Login(string userName, string password);
}

public class FortiClientUIAutomation_Win32 : IFortiClientUIAutomation
{
	// This is just an example, FortiClient app doesn't work with these APIs,
	// but if it id, it would be something like this:
	public void Login(string userName, string password)
	{
		string windowTitle = "FortiClient";
		IntPtr hWnd = FindWindow(null, windowTitle);
		// Username can be stored in the UI already
		if (userName != null) {
			SetControlStringByReplace(hWnd, "Edit", "usernameTextbox", userName);
		}
		SetControlStringByReplace(hWnd, "Edit", "passwordTextbox", password);
		ControlMouseClick(hWnd, "Button", "connect");
	}

	public IntPtr? SearchChildControlExample(IntPtr hWnd, string text, IntPtr? lParam = null)
	{
		IntPtr? control = null;
		EnumChildWindows(hWnd, (IntPtr childControl, IntPtr lParam) => {
			// Return "false" if you find your element to stop the enumeration, return true to continue.
			var sb = new StringBuilder();
			GetClassName(hWnd, sb, 256);
			if (!sb.ToString().Contains(text)) {
				return true;
			}
			control = childControl;
			return false;
		}, lParam ?? IntPtr.Zero);
		return control;
	}

	public static void SetControlStringByReplace(IntPtr hWnd, string controlName, string controlText, string newText)
		=> SetControlStringByReplace(GetControl(hWnd, controlName, controlText), newText);

	public static void SetControlStringByReplace(IntPtr hEdit, string newText)
	{
		ControlSelectAllText(hEdit);
		ControlReplaceSelectedText(hEdit, newText);
	}

	/// <summary>
	/// It is better to use the "SetControlStringByReplace()" version instead since it is more reliable.
	/// </summary>
	public static void SetControlString(IntPtr hWnd, string controlName, string controlText, string newText)
		=> SetControlString(GetControl(hWnd, controlName, controlText), newText);

	public static void SetControlString(IntPtr hEdit, string newText)
		=> SendMessage(hEdit, WM_SETTEXT, IntPtr.Zero, newText);

	public static void ControlSelectAllText(IntPtr hEdit)
		=> SendMessage(hEdit, EM_SETSEL, (IntPtr)0, (IntPtr)(-1));

	public static void ControlReplaceSelectedText_v0(IntPtr hEdit, string newText)
	{
		IntPtr textPtr = Marshal.StringToHGlobalAuto(newText);
		SendMessage(hEdit, EM_REPLACESEL, IntPtr.Zero, textPtr);
		Marshal.FreeHGlobal(textPtr);
	}

	public static void ControlReplaceSelectedText(IntPtr hEdit, string newText)
		=> SendMessage(hEdit, EM_REPLACESEL, IntPtr.Zero, newText);

	public static void ControlMouseClick(IntPtr hWnd, string controlName, string? text = null)
		=> ControlMouseClick(GetControl(hWnd, controlName, text));

	public static void ControlMouseClickWithUpAndDown(IntPtr hWnd, string controlName, string? text = null)
		=> ControlMouseClickWithUpAndDown(GetControl(hWnd, controlName, text));

	public static void ControlMouseClickWithUpAndDown(IntPtr hControl) {
		ControlMouseClickDown(hControl);
		ControlMouseClickUp(hControl);
	}

	public static void ControlMouseClickDown(IntPtr hControl)
		=> SendMessage(hControl, WM_LBUTTONDOWN, IntPtr.Zero, IntPtr.Zero);

	public static void ControlMouseClickUp(IntPtr hControl)
		=> SendMessage(hControl, WM_LBUTTONUP, IntPtr.Zero, IntPtr.Zero);

	public static void ControlMouseClick(IntPtr hControl)
		=> SendMessage(hControl, BM_CLICK, IntPtr.Zero, IntPtr.Zero);

	public static IntPtr GetControl(IntPtr hWnd, string controlName, string? text = null)
	{
		IntPtr hEdit = FindWindowEx(hWnd, IntPtr.Zero, controlName, text);
		return hEdit;
	}

	const int WM_CHAR = 0x102;
	const int WM_PASTE = 0x0302;
	const int EM_SETSEL = 0x00B1;
	const int WM_SETTEXT = 0x000C;
	const int EM_REPLACESEL = 0x00C2;
	const int BM_CLICK = 0x00F5;
	const int WM_LBUTTONDOWN = 0x0201;
	const int WM_LBUTTONUP = 0x0202;

	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr FindWindowEx(
		IntPtr hwndParent,
		IntPtr hwndChildAfter,
		string lpszClass,
		string lpszWindow);

	delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

	[DllImport("user32.dll")]
	static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

	[DllImport("user32.dll")]
	static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
}
