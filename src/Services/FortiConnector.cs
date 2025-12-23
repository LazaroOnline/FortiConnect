using System.IO;
using System.Threading;
using System.Diagnostics;

namespace FortiConnect.Services;

public class FortiConnector
{
	public const string DEFAULT_FortiClientExeFullPath = @"C:\Program Files\Fortinet\FortiClient\FortiClient.exe";
	public const string DEFAULT_FortiClientProcessName = @"FortiClient.exe";

	public string FortiClientExeFullPath { get; set; } = DEFAULT_FortiClientExeFullPath;
	public string FortiClientProcessName { get; set; } = DEFAULT_FortiClientProcessName;
	public int DelayToSpawnFortiClientProcess { get; set; } = 2000;
	public int DelayToFetchVpnCodeEmail { get; set; } = 200;

	/// <summary>If set, it will override the keystroke sequence used to set the login password into focus.</summary>
	public string LoginPasswordFocusSequence { get; set; }
	/// <summary>If set, it will override the keystroke sequence used to set the login verification code into focus.</summary>
	public string LoginVerificationFocusSequence { get; set; }

	private readonly IEmailService _emailService;
	private readonly ISendKeysToProcessService _sendKeysToProcessService;

	private bool _emailMarkAsRead { get; set; }

	public FortiConnector(
		 IEmailService emailService
		,ISendKeysToProcessService sendKeysToProcessService
		, bool markVpnEmailsAsRead = false)
	{
		_emailService = emailService ?? Splat.Locator.Current.GetService<IEmailService>();
		_sendKeysToProcessService = sendKeysToProcessService ?? Splat.Locator.Current.GetService<ISendKeysToProcessService>();
		_emailMarkAsRead = markVpnEmailsAsRead;
	}

	public string GetVpnEmailCode(EmailConfig emailConfig)
	{
		var vpnEmailCode =_emailService.GetLastVpnEmailCode(emailConfig, _emailMarkAsRead);
		return vpnEmailCode;
	}

	public string Login(string vpnUserPass, EmailConfig emailConfig, bool markEmailAsRead)
	{
		var process = GetOrCreateFortiClientProcess();

		var loginKeystrokes = GetLoginKeystrokes(vpnUserPass);
		_sendKeysToProcessService.WriteToProcess(process, loginKeystrokes);

		Thread.Sleep(DelayToFetchVpnCodeEmail);
		var vpnEmailCode =_emailService.GetLastVpnEmailCode(emailConfig, markEmailAsRead);
		Console.Beep(100, 300); // Provide some feedback to the user so they know the process stopped trying to get the email.
		var loginConfirmationKeystrokes = GetLoginConfirmationKeystrokes(vpnEmailCode);
		_sendKeysToProcessService.WriteToProcess(process, loginConfirmationKeystrokes);

		return vpnEmailCode;
	}

	public string GetLoginKeystrokes(string vpnUserPass)
	{
		var enter = KeyCode.ENTER;
		var vpnUserPassLiteral = _sendKeysToProcessService.EscapeLiteralTextToWrite(vpnUserPass);
		var passwordFocusSequence = GetLoginPasswordFocusSequence();
		return passwordFocusSequence + vpnUserPassLiteral + enter;
	}

	public string GetLoginPasswordFocusSequence()
	{
		var tab = KeyCode.TAB;

		if (LoginPasswordFocusSequence != null)
			return LoginPasswordFocusSequence;
		else if (IsFortiClientVersionEqualOrUnder(6, 1))
			return tab + tab + tab + tab ;
		else if (IsFortiClientVersionEqualOrAbove(6, 2))
			return tab + tab + tab;
		else if (IsFortiClientVersionEqualOrAbove(6, 4))
			return tab + tab + tab + tab;
		else
			// Unknown combination.
			// This combination works if you focus on the empty area before the VpnName, UserName and Password text-boxes.
			return tab + tab + tab;
	}

	public string GetLoginConfirmationKeystrokes(string vpnEmailCode)
	{
		var enter = KeyCode.ENTER;
		var vpnEmailCodeLiteral = _sendKeysToProcessService.EscapeLiteralTextToWrite(vpnEmailCode);
		var loginVerificationFocusSequence = GetLoginVerificationFocusSequence();
		return loginVerificationFocusSequence + vpnEmailCodeLiteral + enter;
	}

	public string GetLoginVerificationFocusSequence()
	{
		if (LoginVerificationFocusSequence != null)
			return LoginVerificationFocusSequence;
		else
			// Usually the FortiClient UI focus already goes automatically
			// to the email verification code text-box after entering the password.
			return "";
	}

	public Process GetOrCreateFortiClientProcess()
	{
		var process = GetExistingFortiClientProcess();
		if (process == null) {
			process = StartFortiClientProcess();
		}
		return process;
	}

	public Process GetExistingFortiClientProcess()
	{
		return GetExistingProcess(FortiClientProcessName);
	}

	public static Process GetExistingProcess(string processName)
	{
		var processes = Process.GetProcessesByName(processName);
		if (!processes.Any())
		{
			var processNameWithoutExtension = Path.GetFileNameWithoutExtension(processName);
			processes = Process.GetProcessesByName(processNameWithoutExtension);
		}
		return processes.FirstOrDefault();
	}

	public Process StartFortiClientProcess(int? delayToSpawnFortiClientProcess = null)
	{
		Process fortiClientProcess = new Process();
		fortiClientProcess.StartInfo.FileName = FortiClientExeFullPath;
		fortiClientProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(FortiClientExeFullPath);
		fortiClientProcess.Start();

		try {
			fortiClientProcess.WaitForInputIdle(); // Wait is required before sending keys.
		}
		catch(Exception ex) {
			Console.WriteLine("Handled exception during process.WaitForInputIdle: " + ex);
		}
		// FortiClient spawns another 2 processes to render its UI, taking longer in a separate process.
		Thread.Sleep(delayToSpawnFortiClientProcess ?? DelayToSpawnFortiClientProcess);
		
		return fortiClientProcess;
	}

	public FileVersionInfo GetFortiClientFileVersionInfo()
	{
		return System.Diagnostics.FileVersionInfo.GetVersionInfo(FortiClientExeFullPath);
	}

	// See version list at: https://docs.fortinet.com/product/forticlient/
	public string GetFortiClientVersion()
	{
		var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(FortiClientExeFullPath);
		return versionInfo.ProductVersion;
	}

	//public bool IsFortiClient_v6_2() => GetFortiClientVersion().StartsWith("6.2");

	public bool IsFortiClientVersionEqual(int majorVersion, int? minorVersion = null, int? buildVersion = null, int? privateVersion = null)
	{
		var fortiVersion = GetFortiClientFileVersionInfo();

		var majorVersionMatch = fortiVersion.ProductMajorPart == majorVersion;
		var minorVersionMatch = minorVersion == null || fortiVersion.ProductMinorPart == minorVersion;
		var buildVersionMatch = buildVersion == null || fortiVersion.ProductBuildPart == buildVersion;
		var privateVersionMatch = privateVersion == null || fortiVersion.ProductPrivatePart == privateVersion;

		return majorVersionMatch
			&& minorVersionMatch
			&& buildVersionMatch
			&& privateVersionMatch;
	}

	public bool IsFortiClientVersionEqualOrAbove(int majorVersion, int minorVersion = 0)
	{
		var fortiVersion = GetFortiClientFileVersionInfo();
		return fortiVersion.ProductMajorPart > majorVersion
			|| (fortiVersion.ProductMajorPart == majorVersion
			 && fortiVersion.ProductMinorPart >= minorVersion
			);
	}

	public bool IsFortiClientVersionEqualOrUnder(int majorVersion, int minorVersion = 0)
	{
		var fortiVersion = GetFortiClientFileVersionInfo();
		return fortiVersion.ProductMajorPart < majorVersion
			|| (fortiVersion.ProductMajorPart == majorVersion
			 && fortiVersion.ProductMinorPart <= minorVersion
			);
	}
}
