using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using FortiConnect.Models;

namespace FortiConnect.Services
{
	public abstract class EmailProviderBase
	{
		public string EmailSubjectPrefix { get; set; } = "AuthCode: ";
		public string InboxSubFolderNameWithVpnEmails { get; set; } = "Vpn"; // "Inbox/Accounts/Vpn"

		public string ExtractVpnCodeFromEmailSubject(string emailSubject)
		{
			return emailSubject?.Replace(EmailSubjectPrefix, "").Trim();
		}
		
		public abstract string GetLastVpnEmailCode(EmailConfig emailConfig, bool markAsRead);
	}
}
