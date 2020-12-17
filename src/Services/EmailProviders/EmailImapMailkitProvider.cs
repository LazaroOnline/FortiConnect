using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using MailKit;
using MailKit.Search;
using MailKit.Net.Imap;
using FortiConnect.Models;

namespace FortiConnect.Services
{
	public class EmailImapMailkitProvider : EmailProviderBase
	{
		public override string GetLastVpnEmailCode(EmailConfig emailConfig, bool markAsRead)
		{
			using (var client = GetClient(emailConfig))
			{
				var inbox = client.Inbox; // The InBox folder is always available on all IMAP servers.
				inbox.Open(FolderAccess.ReadOnly);
				// var results = inbox.Search(SearchOptions.All, SearchQuery.Not(SearchQuery.Seen)); // Gets unread emails.
				var results = inbox.Search(SearchOptions.All, SearchQuery.SubjectContains(EmailSubjectPrefix).And(SearchQuery.Not(SearchQuery.Seen)));
				var mostRecentEmailUniqueId = results.UniqueIds.First(); // TODO: test if the First is actually the most recent email.
				var emailSubject = GetEmailSubject(inbox, mostRecentEmailUniqueId);
				if (markAsRead) {
					inbox.AddFlags(mostRecentEmailUniqueId, MessageFlags.Seen, true); // Mark message as read
				}
				client.Disconnect(true);
				var vpnCode = ExtractVpnCodeFromEmailSubject(emailSubject);
				return vpnCode;
			}
		}
		
		// https://stackoverflow.com/questions/7056715/reading-emails-from-gmail-in-c-sharp/19570553#19570553
		public IImapClient GetClient(EmailConfig emailConfig)
		{
			var client = new ImapClient();
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
		
		public string GetEmailMessage(IMailFolder inbox, UniqueId emailUniqueId)
		{
			var message = inbox.GetMessage(emailUniqueId);
			return message.HtmlBody;
		}

		public IEnumerable<string> GetAllMails(EmailConfig emailConfig)
		{
			var messages = new List<string>();
		
			using (var client = GetClient(emailConfig))
			{
				var inbox = client.Inbox; // The Inbox folder is always available on all IMAP servers.
				inbox.Open(FolderAccess.ReadOnly);
				var results = inbox.Search(SearchOptions.All, SearchQuery.NotSeen);
				foreach (var uniqueId in results.UniqueIds)
				{
					var message = inbox.GetMessage(uniqueId);
					messages.Add(message.HtmlBody);
				}
				client.Disconnect(true);
			}
			return messages;
		}
	}
}
