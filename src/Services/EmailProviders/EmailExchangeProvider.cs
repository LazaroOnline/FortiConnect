using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.Exchange.WebServices.Data;
using FortiConnect.Models;
using FortiConnect.Utils;
using System.Text.RegularExpressions;

namespace FortiConnect.Services
{
	// EWS (Exchange Web Service)
	// OnMobile Microsoft Exchange Server Version: 15.1.1979.6
	// Microsoft Exchange Server 2016 
	// https://support.microsoft.com/en-us/office/determine-the-version-of-microsoft-exchange-server-my-account-connects-to-d427465a-ce3b-42bd-9d83-c7d893d5d334#:~:text=do%20the%20following%3A-,With%20Microsoft%20Outlook%20running%2C%20press%20and%20hold%20CTRL%20while%20you,note%20the%20number%20that%20appears.

	// Requires NuGet package: "Microsoft.Exchange.WebServices"
	// https://www.youtube.com/watch?v=h6hMz4JtHyg
	public class EmailExchangeProvider : EmailProviderBase
	{
		//public override string GetLastVpnEmailCode(EmailConfig emailConfig, bool markAsRead, int maxRetries = 3, int millisecondsBetweenRetries = 3000)
		public override string GetLastVpnEmailCode(EmailConfig emailConfig, bool markAsRead)
		{
			var client = GetClient(emailConfig);
			if (client == null) {
				Console.WriteLine("Not able to log into the email!");
				return null;
			}

			EmailMessage mostRecentVpnEmailMessage = null;
			Retry.Action((r) => {
					mostRecentVpnEmailMessage = GetVpnEmail(client, emailConfig?.EmailSubjectPrefix, emailConfig?.InboxSubFolderNameWithVpnEmails);
				}
				,whileIs: (r) => mostRecentVpnEmailMessage == null
				,maxRetries: 3
				,every: TimeSpan.FromSeconds(3)
			);

			if (mostRecentVpnEmailMessage == null) {
				return null;
			}

			if (markAsRead)
			{
				mostRecentVpnEmailMessage.IsRead = true;
				mostRecentVpnEmailMessage.Update(ConflictResolutionMode.AutoResolve);
				// https://docs.microsoft.com/en-us/exchange/client-developer/exchange-web-services/how-to-process-email-messages-in-batches-by-using-ews-in-exchange
				// client.UpdateItems(new List<EmailMessage> { mostRecentVpnEmailMessage }, folderIdVpn, ConflictResolutionMode.AutoResolve, MessageDisposition.SaveOnly, SendInvitationsOrCancellationsMode.SendToNone);
			}

			var vpnCode = ExtractVpnCodeFromEmailSubject(mostRecentVpnEmailMessage?.Subject, emailConfig?.EmailSubjectPrefix);
			return vpnCode;
		}

		public EmailMessage GetVpnEmail(ExchangeService client, string emailSubjectPrefix, string inboxSubFolderNameWithVpnEmails = null)
		{
			Folder vpnFolder = GetVpnFolder(client, inboxSubFolderNameWithVpnEmails);
			
			// Searching emails into a specific folder:
			// FindItemsResults<Item> emailsInVpnFolder = client.FindItems("MyEmailFolder", subjectFilter, folderView); // This throws an exception, the folder ID is not the folder name but some kind of GUID, throws exception.

			// https://docs.microsoft.com/en-us/exchange/client-developer/exchange-web-services/how-to-use-search-filters-with-ews-in-exchange
			var subjectFilterBySubject = new SearchFilter.ContainsSubstring(ItemSchema.Subject, $"{emailSubjectPrefix}", ContainmentMode.Substring, ComparisonMode.IgnoreCase);
			var searchFilterUnread = new SearchFilter.IsEqualTo(EmailMessageSchema.IsRead, false);
			var searchFilterRecentDate = new SearchFilter.IsGreaterThan(EmailMessageSchema.DateTimeReceived, DateTime.UtcNow.AddMinutes(-30));
			var searchFilterNewVpnCode = new SearchFilter.SearchFilterCollection(LogicalOperator.And, searchFilterUnread, subjectFilterBySubject, searchFilterRecentDate);
			var emailView = new ItemView(pageSize: 5);
			var vpnEmails = vpnFolder.FindItems(searchFilterNewVpnCode, emailView);
			var mostRecentVpnEmailItem = vpnEmails.FirstOrDefault();
			if (mostRecentVpnEmailItem?.Id == null) {
				return null;
			}
			var mostRecentVpnEmailMessage = EmailMessage.Bind(client, mostRecentVpnEmailItem.Id);
			return mostRecentVpnEmailMessage;
		}
		
		public Folder GetVpnFolder(ExchangeService client, string inboxSubFolderNameWithVpnEmails = null)
		{
			var defaultFolder = Folder.Bind(client, WellKnownFolderName.Inbox);
			if (string.IsNullOrEmpty(inboxSubFolderNameWithVpnEmails)) {
				return defaultFolder;
			}
			// https://stackoverflow.com/questions/7912584/exchange-web-service-folderid-for-a-not-well-known-folder-name
			// var folderIdVpn = new FolderId(InboxSubFolderNameWithVpnEmails);

			var folderView = new FolderView(pageSize: 5);
			folderView.Traversal = FolderTraversal.Deep; // Allows search in nested folders.

			var searchFilterVpnFolder = new SearchFilter.IsEqualTo(FolderSchema.DisplayName, inboxSubFolderNameWithVpnEmails);
			var vpnFolders = client.FindFolders(WellKnownFolderName.Inbox, searchFilterVpnFolder, folderView);

			Folder vpnFolder = null;
			if (vpnFolders.TotalCount > 0)
			{
				//var vpnFolderId = vpnFolders.Folders.Single().Id;
				var vpnFolderId = vpnFolders.Folders.FirstOrDefault()?.Id;
				vpnFolder = Folder.Bind(client, vpnFolderId);
			}
			else
			{
				vpnFolder = defaultFolder;
			}
			return vpnFolder;
		}
		
		public ExchangeService GetClient(EmailConfig emailConfig)
		{
			//EmailExchangeService exchange = new EmailExchangeService();
			ExchangeService exchange = new ExchangeService(ExchangeVersion.Exchange2013_SP1);
			var userName = GetEmailUsername(emailConfig.UserEmail);
			var domain = GetDomain(emailConfig.UserEmail);
			exchange.Credentials = new WebCredentials(userName, emailConfig.UserPassword, domain);

			#if NETFRAMEWORK
			try {
				// AutoDiscover only works in DotNet 4.X, not in DotNet Core:
				exchange.AutodiscoverUrl(emailConfig.UserEmail, (discoverURL) => true);
			} catch(Exception ex) {
			}
			return exchange;
			#endif

			// https://docs.microsoft.com/en-us/exchange/client-developer/exchange-web-services/how-to-set-the-ews-service-url-by-using-the-ews-managed-api
			// https://stackoverflow.com/questions/44359469/connecting-to-exchange-without-autodiscover
			var exchangeUrl = GetDefaultExhangeUrl(emailConfig.UserEmail);
			exchange.Url = new Uri(exchangeUrl);

			return exchange;
		}

		/// <summary>
		/// Returns the default Exchange URL.
		/// Your company may have set up a different URL that is not the default one.
		/// To get your company's Exchange URL use Outlook 2007 or later:
		/// - Hold the Ctrl key and right click on the Outlook Icon in the system tray.
		/// - Select "Test E-mail Auto Configuration" from the menu.
		/// - Type in an email address located on the desired Exchange server.
		/// - Click Test.
		/// - The URL is listed as 'Availability Service URL'.
		/// https://support.neuxpower.com/hc/en-us/articles/202482832-Determining-the-Exchange-Web-Services-EWS-URL
		/// </summary>
		public string GetDefaultExhangeUrl(string email)
		{
			var companyDomain = GetDomain(email);
			return $"https://mail.{companyDomain}/EWS/Exchange.asmx";
		}
		
		public string GetDomain(string email)
		{
			return Regex.Replace(email, @".*@", "");
		}

		public string GetEmailUsername(string email)
		{
			return Regex.Replace(email, @"@.*", "");
		}
		
		#region Examples_NotUsed
		public List<EmailMessage> GetLastEmails(ExchangeService client)
		{
			FindItemsResults<Item> emailItems = client.FindItems(WellKnownFolderName.Inbox, new ItemView(10));
			var emails = emailItems.Select(item => EmailMessage.Bind(client, item.Id)).ToList();
			return emails;
		}

		// http://www.infinitec.de/post/2011/10/05/Setting-the-Homepage-of-an-Exchange-folder-using-the-EWS-Managed-API.aspx
		public static void SetFolderHomePage(IEnumerable<string> pathFragments, string url, ExchangeService service)
		{
			var folderWebviewinfoProperty = new ExtendedPropertyDefinition(14047, MapiPropertyType.Binary);
			var root = Folder.Bind(service, WellKnownFolderName.MsgFolderRoot);
			var targetFolder = root;
			foreach (var fragment in pathFragments)
			{
				var result = service.FindFolders(targetFolder.Id, new SearchFilter.IsEqualTo(FolderSchema.DisplayName, fragment), new FolderView(1));
				if (result.TotalCount == 0)
				{
					throw new InvalidOperationException(string.Format("Folder fragment {0} was not found.", fragment));
				}
				targetFolder = result.Folders[0];
			}
 
			targetFolder.SetExtendedProperty(folderWebviewinfoProperty, EncodeUrl(url));
			targetFolder.Update();
		}

		private static byte[] EncodeUrl(string url)
		{
			var writer = new StringWriter();
			var dataSize = ((ConvertToHex(url).Length / 2) + 2).ToString("X2");
 
			writer.Write("02"); // Version
			writer.Write("00000001"); // Type
			writer.Write("00000001"); // Flags
			writer.Write("00000000000000000000000000000000000000000000000000000000"); // unused
			writer.Write("000000");
			writer.Write(dataSize);
			writer.Write("000000");
			writer.Write(ConvertToHex(url));
			writer.Write("0000");
     
			var buffer = HexStringToByteArray(writer.ToString());
			return buffer;
		}
 
		private static string ConvertToHex(string input)
		{
			return string.Join(string.Empty, input.Select(c => ((int) c).ToString("x2") + "00").ToArray());
		}
 
		private static byte[] HexStringToByteArray(string input)
		{
			return Enumerable
				.Range(0, input.Length/2)
				.Select(index => byte.Parse(input.Substring(index*2, 2), NumberStyles.AllowHexSpecifier)).ToArray();
		}
		#endregion
		
	}
}
