namespace FortiConnect.Services;

public interface IEmailService
{
	public string GetLastVpnEmailCode(EmailConfig emailConfig, bool markAsRead);
}
