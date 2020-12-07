using System;
using System.Text;
using System.Collections.Generic;

namespace FortiConnect.Models
{
	public class EmailConfig
	{
		public EmailServerProtocol Protocol { get; set; }
		public string ServerName { get; set; }
		public int ServerPort { get; set; } = 993;
		public bool UseSsl { get; set; } = true;
		public string UserEmail { get; set; }
		public string UserPassword { get; set; }
	}
}
