using System;
using System.Text;
using System.Collections.Generic;
using FortiConnect.Models;

namespace FortiConnect.Services
{
	public interface IEmailService
	{
		public string GetLastVpnEmailCode(EmailConfig emailConfig, bool markAsRead);
	}
}
