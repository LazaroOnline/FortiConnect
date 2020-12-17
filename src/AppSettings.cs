﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FortiConnect.Models;

namespace FortiConnect
{
	public class AppSettings
	{
        //[Required]
		public FortiClientConfig FortiClient { get; set; }
		public VpnConfig Vpn { get; set; }
		public EmailServerConfig EmailServer { get; set; }
		public EmailAccountConfig EmailAccount { get; set; }
		public int DelayToShowVpnClient { get; set; } = 2000;
		public int DelayToSpawnFortiClientProcess { get; set; } = 2000;
	}
	
	public class FortiClientConfig
	{
		public string ExeFullPath { get; set; }
		public string ProcessName { get; set; }
	}
	
	public class VpnConfig
	{
		public string UserName { get; set; }
		public string Password { get; set; }
	}
	
	public class EmailServerConfig
	{
		public EmailServerProtocol Protocol { get; set; } = EmailServerProtocol.Exchange;
		public string Server { get; set; }
		public int Port { get; set; } = 993;
	}

	public class EmailAccountConfig
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public bool MarkVpnEmailAsRead { get; set; } = false;
	}
	
}
