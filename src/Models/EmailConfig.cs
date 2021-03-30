using System;
using System.Text;
using System.Collections.Generic;

namespace FortiConnect.Models
{
	public class EmailConfig
	{
		public const int DEFAULT_GetEmailRetryEveryMilliseconds = 3000;
		public const int DEFAULT_GetEmailMaxRetries = 3;
		public const int DEFAULT_PORT_IMAP = 993;
		
		public EmailServerProtocol Protocol { get; set; }
		public string ServerName { get; set; }
		public int ServerPort { get; set; } = DEFAULT_PORT_IMAP;
		public bool UseSsl { get; set; } = true;
		public string UserEmail { get; set; }
		public string UserPassword { get; set; }

		public string EmailSubjectPrefix { get; set; }
		public string InboxSubFolderNameWithVpnEmails { get; set; }

		public int? GetEmailRetryEveryMilliseconds { get; set; } = DEFAULT_GetEmailRetryEveryMilliseconds;
		public int? GetEmailMaxRetries { get; set; } = DEFAULT_GetEmailMaxRetries;
		
	}
}
