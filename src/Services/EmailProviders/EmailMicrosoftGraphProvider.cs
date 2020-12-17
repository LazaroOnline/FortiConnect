using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using FortiConnect.Models;

namespace FortiConnect.Services
{
	public class EmailMicrosoftGraphProvider : EmailProviderBase
	{
		public override string GetLastVpnEmailCode(EmailConfig emailConfig, bool markAsRead)
		{
			throw new NotImplementedException();
		}
		
	}
}
