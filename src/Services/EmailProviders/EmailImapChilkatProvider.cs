using System;
using System.Text;
using System.Collections.Generic;
using FortiConnect.Models;

namespace FortiConnect.Services
{
	/*
	public class EmailImapChilkatProvider : EmailServiceBase
	{
		public override string GetLastVpnEmailCode(EmailConfig emailConfig, bool markAsRead)
		{
			var imap = GetClient(emailConfig);
			bool success = imap.ExpungeAndClose();

			imap.SelectMailbox("Inbox");

			Chilkat.MessageSet messageSet = imap.Search(EmailSubjectPrefix, true);
			Chilkat.EmailBundle emailBundle = imap.FetchBundle(messageSet);
			// _log.Info($"Total Emails: {emailBundle.MessageCount}");

			Chilkat.Email mostRecentVpnEmail = emailBundle?.GetEmail(0);
			if (markAsRead)
			{
				// TODO: mark as read instead of deleting:
				imap.SetMailFlag(mostRecentVpnEmail, "Deleted", 1);
			}
			imap.Disconnect();
			var vpnCode = ExtractVpnCodeFromEmailSubject(mostRecentVpnEmail?.Subject);
			return vpnCode;
		}

		public Chilkat.Imap GetClient(EmailConfig emailConfig)
		{
			var imap = new Chilkat.Imap();
			var success = imap.UnlockComponent("YOUR_CHILKAT_PRODUCT_KEY_HERE");
			imap.Ssl = emailConfig.UseSsl;
			imap.Port = emailConfig.ServerPort;

			success = imap.Connect(emailConfig.ServerName);
			success = imap.Login(emailConfig.UserEmail, emailConfig.UserPassword);
			//_log.Info(imap.LastErrorText);
			return imap;
		}
	}
	*/
}
