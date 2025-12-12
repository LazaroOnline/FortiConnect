using MimeKit;
using MailKit;
using MailKit.Search;
using MailKit.Net.Pop3;

namespace FortiConnect.Services;

// https://github.com/jstedfast/MailKit/blob/master/Documentation/Examples/Pop3Examples.cs
public class EmailPop3Provider : EmailProviderBase
{
	public override string GetLastVpnEmailCode(EmailConfig emailConfig, bool markAsRead)
	{
		using (var client = GetClient(emailConfig))
		{
			throw new NotImplementedException();

			// var emails = client.GetMessageUids();
			// var mostRecentEmailUniqueId = ...;
			string emailSubject = ""; // ...
			if (markAsRead) {
				// Mark message as read
			}
			client.Disconnect(true);
			var vpnCode = ExtractVpnCodeFromEmailSubject(emailSubject, emailConfig?.EmailSubjectPrefix);
			return vpnCode;
		}
	}

	public IPop3Client GetClient(EmailConfig emailConfig)
	{
		var client = new Pop3Client();
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
