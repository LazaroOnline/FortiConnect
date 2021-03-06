﻿using System;
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
				case EmailServerProtocol.Exchange:        return new EmailExchangeProvider();       // Splat.Locator.Current.GetService<EmailExchangeProvider>();
				case EmailServerProtocol.Imap:            return new EmailImapMailkitProvider();    // Splat.Locator.Current.GetService<EmailImapMailkitProvider>();
				case EmailServerProtocol.MictrosoftGraph: return new EmailMicrosoftGraphProvider(); // Splat.Locator.Current.GetService<EmailMicrosoftGraphProvider>();
			//	case EmailServerProtocol.Pop3:            return new EmailPop3Provider();           // Splat.Locator.Current.GetService<EmailPop3Provider>();
			}
		}
	}
}
