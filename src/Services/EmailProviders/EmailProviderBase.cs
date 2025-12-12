namespace FortiConnect.Services;

public abstract class EmailProviderBase
{
	public const string DEFAULT_EmailSubjectPrefix = "AuthCode: ";

	public string ExtractVpnCodeFromEmailSubject(string emailSubject, string emailSubjectPrefix = DEFAULT_EmailSubjectPrefix)
	{
		return emailSubject?.Replace(emailSubjectPrefix, "").Trim();
	}
	
	public abstract string GetLastVpnEmailCode(EmailConfig emailConfig, bool markAsRead);
}
