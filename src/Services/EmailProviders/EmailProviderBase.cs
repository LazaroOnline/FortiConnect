using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using FortiConnect.Models;

namespace FortiConnect.Services
{
	public abstract class EmailProviderBase
	{
		public string ExtractVpnCodeFromEmailSubject(string emailSubject, string emailSubjectPrefix)
		{
			return emailSubject?.Replace(emailSubjectPrefix, "").Trim();
		}
		
		public abstract string GetLastVpnEmailCode(EmailConfig emailConfig, bool markAsRead);
	}
}
