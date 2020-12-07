using System;
using System.Text;
using System.Collections.Generic;
using FortiConnect.Models;

namespace FortiConnect.Services
{
	public class EmailService : IEmailService
	{
		public string GetLastVpnEmailCode(EmailConfig emailConfig, bool markAsRead)
		{
			var emailProvider = GetEmailProvider(emailConfig.Protocol);
			return emailProvider.GetLastVpnEmailCode(emailConfig, markAsRead);
		}
		
		public EmailProviderBase GetEmailProvider(EmailServerProtocol protocol)
		{
			switch(protocol) {
				default:
				case EmailServerProtocol.Exchange: return new EmailExchangeProvider();    break; // Splat.Locator.Current.GetService<EmailExchangeProvider>();
				case EmailServerProtocol.Imap:     return new EmailImapMailkitProvider(); break; // Splat.Locator.Current.GetService<EmailImapMailkitProvider>();
				case EmailServerProtocol.Pop3:     return new EmailImapMailkitProvider(); break; // Splat.Locator.Current.GetService<EmailImapMailkitProvider>();
				case EmailServerProtocol.Smtp:     return new EmailImapMailkitProvider(); break; // Splat.Locator.Current.GetService<EmailImapMailkitProvider>();
			}
		}
	}
}
