using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using FortiConnect.Utils;
using FortiConnect.Models;

namespace FortiConnect;

public class AppSettings
{
	public const int DEFAULT_DelayToShowVpnClient = 2000;
	public const int DEFAULT_DelayToSpawnFortiClientProcess = 2000;
	public const int DEFAULT_DelayToFetchVpnCodeEmail = 200;

	//[Required]
	public FortiClientConfig FortiClient { get; set; }
	public VpnConfig Vpn { get; set; }
	public EmailServerConfig EmailServer { get; set; }
	public EmailAccountConfig EmailAccount { get; set; }
	public int? DelayToShowVpnClient { get; set; } = DEFAULT_DelayToShowVpnClient;
	public int? DelayToSpawnFortiClientProcess { get; set; } = DEFAULT_DelayToSpawnFortiClientProcess;
	public int? DelayToFetchVpnCodeEmail { get; set; } = DEFAULT_DelayToFetchVpnCodeEmail;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? GetEmailRetryEveryMilliseconds { get; set; } = EmailConfig.DEFAULT_GetEmailRetryEveryMilliseconds;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public int? GetEmailMaxRetries { get; set; } = EmailConfig.DEFAULT_GetEmailMaxRetries;
}

public class FortiClientConfig
{
	public string ExeFullPath { get; set; }
	public string ProcessName { get; set; }
	public string LoginPasswordFocusSequence { get; set; }
	public string LoginVerificationFocusSequence { get; set; }
}

//[JsonConverter(typeof(VpnConfigConverter))] // This makes attribute here would make an infinite loop.
public class VpnConfig
{
	// FortiClient already stores the UserName.
	public string UserName { get; set; }

	//[JsonIgnore] // This would also prevent reading the property, not just writing it.
	//[JsonConverter(typeof(JsonIgnoreWriteAsNull<string>))] // This can't prevent the property name from being written.
	public string Password { get; set; }
	public string PasswordEncoded {
		get { return Base64Converter.ConvertTextUnicodeToBase64(this.Password); }
		set { this.Password = Base64Converter.ConvertBase64ToTextUnicode(value); }
	}
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? EmailSubjectPrefix { get; set; } = Services.EmailProviderBase.DEFAULT_EmailSubjectPrefix;
}

public class EmailServerConfig
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public EmailServerProtocol Protocol { get; set; } = EmailServerProtocol.Exchange;
	public string Server { get; set; }
	public int Port { get; set; } = EmailConfig.DEFAULT_PORT_IMAP;
}

//[JsonConverter(typeof(EmailAccountConfigConverter))] // This makes attribute here would make an infinite loop.
public class EmailAccountConfig
{
	public const string DEFAULT_InboxSubFolderNameWithVpnEmails = "Vpn";

	public string Email { get; set; }
	//[JsonIgnore] // This would also prevent reading the property, not just writing it.
	//[JsonConverter(typeof(JsonIgnoreWriteAsNull<string>))] // This can't prevent the property name from being written.
	public string Password { get; set; }
	public string PasswordEncoded {
		get { return Base64Converter.ConvertTextUnicodeToBase64(this.Password); }
		set { this.Password = Base64Converter.ConvertBase64ToTextUnicode(value); }
	}
	public bool MarkVpnEmailAsRead { get; set; } = false;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? InboxSubFolderNameWithVpnEmails { get; set; } = DEFAULT_InboxSubFolderNameWithVpnEmails;
}
