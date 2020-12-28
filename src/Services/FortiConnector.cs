using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FortiConnect.Models;
using Splat;

namespace FortiConnect.Services
{
	public class FortiConnector
	{
		public const string DEFAULT_FortiClientExeFullPath = @"C:\Program Files\Fortinet\FortiClient\FortiClient.exe";
		public const string DEFAULT_FortiClientProcessName = @"FortiClient.exe";

		public string FortiClientExeFullPath { get; set; } = DEFAULT_FortiClientExeFullPath;
		public string FortiClientProcessName { get; set; } = DEFAULT_FortiClientProcessName;
		public int DelayToSpawnFortiClientProcess { get; set; } = 2000;
		private IEmailService _emailService { get; set; }
		private IProcessWritterService _processWritterService { get; set; }
		
		private bool _emailMarkAsRead { get; set; }

		public FortiConnector(
			 IEmailService emailService
			,IProcessWritterService processWritterService
			,bool markVpnEmailsAsRead = false)
		{
			_emailService = emailService ?? Splat.Locator.Current.GetService<IEmailService>();
			_processWritterService = processWritterService ?? Splat.Locator.Current.GetService<IProcessWritterService>();
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

			var tab = _processWritterService.GetKeyTab();
			var enter = _processWritterService.GetKeyEnter();
			var vpnUserPassLiteral = _processWritterService.EscapeLiteralTextToWrite(vpnUserPass);
			_processWritterService.WriteToProcess(process, tab + tab + tab + vpnUserPassLiteral + enter);

			var vpnEmailCode =_emailService.GetLastVpnEmailCode(emailConfig, markEmailAsRead);
			var vpnEmailCodeLiteral = _processWritterService.EscapeLiteralTextToWrite(vpnEmailCode);
			_processWritterService.WriteToProcess(process, vpnEmailCodeLiteral + enter);

			return vpnEmailCode;
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
	}
}
