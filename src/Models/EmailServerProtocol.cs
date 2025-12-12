namespace FortiConnect.Models;

/// <summary>
/// Protocols for READING emails.
/// https://contactgenie.info/understanding-email-protocols-pop-imap-mapi-eas/
/// </summary>
public enum EmailServerProtocol
{
	/// <summary> Default port:  25. This is Microsoft's protocol for Outlook. </summary>
	Exchange,

	/// <summary> Default port: 143 / 993 with SSL. IMAP is the most used standard, but it may not be accessible. </summary>
	Imap,
	
	/// <summary> Microsoft API for Office 365, Outlook.com and some hybrid on-premise Microsoft Exchange Servers 2016. </summary>
	MictrosoftGraph,

	/// <summary> Default port: 135 (plus other dynamic ports). Messaging Application Programming Interface, created by Microsoft and used in old versions of Outlook. EMAPI is the current protocol, while SMAPI is an old subset that was removed since Exchange 2003. </summary>
	Mapi,

	/// <summary> Default port: 110 / 995 with SSL. POP3 protocol deletes the received emails from the server. </summary> Pop3,

	/// <summary> Default port:  25 / 465 with SSL / 587 with TLS. SMTP is to send emails only, not to receive them! </summary> Smtp,
}
