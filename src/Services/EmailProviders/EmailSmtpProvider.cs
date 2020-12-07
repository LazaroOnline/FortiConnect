using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using MimeKit;
using MailKit;
using MailKit.Search;
using MailKit.Net.Smtp;
using FortiConnect.Models;

namespace FortiConnect.Services
{
	public class EmailSmtpProvider : EmailProviderBase
	{
		public override string GetLastVpnEmailCode(EmailConfig emailConfig, bool markAsRead)
		{
			using (var client = GetClient(emailConfig))
			{
				throw new NotImplementedException();

				// var emails = client.();
				// var mostRecentEmailUniqueId = ...;
				string emailSubject = ""; // ...
				if (markAsRead) {
					// Mark message as read
				}
				client.Disconnect(true);
				var vpnCode = ExtractVpnCodeFromEmailSubject(emailSubject);
				return vpnCode;
			}
		}

		public ISmtpClient GetClient(EmailConfig emailConfig)
		{
			var client = new SmtpClient();
			client.Connect(emailConfig.ServerName, emailConfig.ServerPort, emailConfig.UseSsl);
			client.AuthenticationMechanisms.Remove("XOAUTH2"); // Note: since we don't have an OAuth2 token, disable the XOAUTH2 authentication mechanism.
			client.Authenticate(emailConfig.UserEmail, emailConfig.UserPassword);
			return client;
		}
		
		public string GetEmailSubject(IMailFolder inbox, UniqueId emailUniqueId)
		{
			var message = inbox.GetMessage(emailUniqueId);
			return message.Subject;
		}
	}
}
