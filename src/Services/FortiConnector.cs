using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
//using System.Windows.Forms;
using FortiConnect.Models;
using Splat;

namespace FortiConnect.Services
{
	public class FortiConnector
	{
		public string FortiClientExeFullPath { get; set; } = @"C:\Program Files\Fortinet\FortiClient\FortiClient.exe";
		public string FortiClientProcessName { get; set; } = @"FortiClient.exe";
		private IEmailService _emailService { get; set; }
		private bool _emailMarkAsRead { get; set; }

		public FortiConnector(IEmailService emailService, bool markVpnEmailsAsRead = false)
		{
			_emailService = emailService ?? Splat.Locator.Current.GetService<IEmailService>();
			_emailMarkAsRead = markVpnEmailsAsRead;
		}

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

		// https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys?view=net-5.0
		public static class Keys
		{
			public const string ENTER = "{ENTER}";
			public const string TAB = "{TAB}";
		}

		public string GetVpnEmailCode(EmailConfig emailConfig)
		{
			var vpnEmailCode =_emailService.GetLastVpnEmailCode(emailConfig, _emailMarkAsRead);
			return vpnEmailCode;
		}

		public string Login(string vpnUserPass, EmailConfig emailConfig, bool markEmailAsRead)
		{
			//var vpnEmailCodeTest =_emailService.GetLastVpnEmailCode(emailConfig); return;  // TODO: delete this testing line.

			var process = GetOrCreateFortiClientProcess();
			WriteToProcess(process, Keys.TAB + Keys.TAB + Keys.TAB + vpnUserPass + Keys.ENTER);
			var vpnEmailCode =_emailService.GetLastVpnEmailCode(emailConfig, markEmailAsRead);
			WriteToProcess(process, Keys.TAB + vpnEmailCode + Keys.ENTER);
			return vpnEmailCode;
		}

		public void WriteToProcess(Process process, string textToWrite)
		{
			if (process != null)
			{
				IntPtr windowHandle = process.MainWindowHandle;
				ShowWindow(windowHandle, 1);
				Thread.Sleep(2000);
				SetForegroundWindow(windowHandle);
				Thread.Sleep(2000);
				// https://stackoverflow.com/questions/38460253/how-to-use-system-windows-forms-in-net-core-class-library
				//SendKeys.SendWait(textToWrite);
			}
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
			var processes = Process.GetProcessesByName(FortiClientProcessName);
			if (!processes.Any()) {
				var processNameWithoutExtension = Path.GetFileNameWithoutExtension(FortiClientProcessName);
				processes = Process.GetProcessesByName(processNameWithoutExtension);
			}
			return processes.FirstOrDefault();
		}

		public Process StartFortiClientProcess()
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
			Thread.Sleep(2000); // FortiClient spawns another 2 processes to render its UI, taking longer in a separate process.

			return fortiClientProcess;
		}
	}
}
