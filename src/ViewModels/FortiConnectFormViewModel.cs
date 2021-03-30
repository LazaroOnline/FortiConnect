using System;
using System.Linq;
using System.Text;
using System.Reactive;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FortiConnect.Services;
using FortiConnect.Models;
using FortiConnect.Utils;
using ReactiveUI;
using Splat;

namespace FortiConnect.ViewModels
{
	public class FortiConnectFormViewModel : ViewModelBase
	{
		private const string URL_LICENSE = "https://opensource.org/licenses/MIT";
		private const string URL_GITHUB = "https://github.com/LazaroOnline/FortiConnect";

		public string VpnUserName   { get; set; }
		public string VpnPassword   { get; set; }

		public string GitVersion   { get; set; } = GitVersionService.GetGitVersionAssemblyInfo().ToString();

		public EmailServerProtocol _emailProtocol;
		public EmailServerProtocol EmailProtocol {
			get => _emailProtocol;
			set => this.RaiseAndSetIfChanged(ref _emailProtocol, value);
		}

		public bool _isPopupVisible;
		public bool IsPopupVisible {
			get => _isPopupVisible;
			set => this.RaiseAndSetIfChanged(ref _isPopupVisible, value);
		}

		public bool _isAboutVisible;
		public bool IsAboutVisible {
			get => _isAboutVisible;
			set => this.RaiseAndSetIfChanged(ref _isAboutVisible, value);
		}
		
		public string _outputMessage;
		public string OutputMessage {
			get => _outputMessage;
			set => this.RaiseAndSetIfChanged(ref _outputMessage, value);
		}
		
		public string _outputMessageTitle = "Error";
		public string OutputMessageTitle {
			get => _outputMessageTitle;
			set => this.RaiseAndSetIfChanged(ref _outputMessageTitle, value);
		}

		public IEnumerable<EmailServerProtocol> EmailProtocolOptions { get; set; }

		public string EmailServer { get; set; }
		public int    EmailPort   { get; set; }

		public string EmailUserName { get; set; }
		public string EmailPassword { get; set; }

		public string _emailVpnCode;
		public string EmailVpnCode {
			get => _emailVpnCode;
			set => this.RaiseAndSetIfChanged(ref _emailVpnCode, value);
		}

		public bool _isPortEnabled;
		public bool IsPortEnabled {
			get => _isPortEnabled;
			set => this.RaiseAndSetIfChanged(ref _isPortEnabled, value);
		}

		public bool _isEnabledCopyEmail;
		public bool IsEnabledCopyEmail {
			get => _isEnabledCopyEmail;
			set => this.RaiseAndSetIfChanged(ref _isEnabledCopyEmail, value);
		}

		public bool _markVpnEmailAsRead;
		public bool MarkVpnEmailAsRead {
			get => _markVpnEmailAsRead;
			set => this.RaiseAndSetIfChanged(ref _markVpnEmailAsRead, value);
		}

		private FortiConnector _fortiConnector { get; set; }
		private AppSettings _appSettings { get; set; }
		private AppSettingsWriter _appSettingsWriter { get; set; }


		// https://avaloniaui.net/docs/controls/button
		public ReactiveCommand<Unit, Unit> OnCopyEmailVpnCodeCommand { get; }
		public ReactiveCommand<Unit, Unit> OnGetEmailVpnCodeCommand { get; }
		public ReactiveCommand<Unit, Unit> OnConnectToVpnCommand { get; }
		public ReactiveCommand<Unit, Unit> OnOpenAboutWindow { get; }
		public ReactiveCommand<Unit, Unit> OnCloseAboutPopup { get; }
		public ReactiveCommand<Unit, Unit> OnCloseMessagePopup { get; }

		// Constructor required by the designer tools.
		public FortiConnectFormViewModel()
			: this(null, null, null) // Call the other constructor
		{ }

		public FortiConnectFormViewModel(
			  FortiConnector fortiConnector
			 ,AppSettings appSettings
			 ,AppSettingsWriter appSettingsWriter
		)
		{
			this.WhenAnyValue(x => x.EmailProtocol).Subscribe(x => {
				 this.IsPortEnabled = EmailProtocol != EmailServerProtocol.Exchange;
			});
			this.WhenAnyValue(x => x.EmailVpnCode).Subscribe(x => {
				 this.IsEnabledCopyEmail = !string.IsNullOrWhiteSpace(EmailVpnCode);
			});

			OnConnectToVpnCommand = ReactiveCommand.Create(() => {
				TryAction(LoginToVpn);
			});
			OnGetEmailVpnCodeCommand = ReactiveCommand.Create(() => {
				TryAction(GetEmailVpnCodeAndCopyToClipboard);
			});
			OnCopyEmailVpnCodeCommand = ReactiveCommand.Create(() => {
				TryAction(CopyEmailVpnCode);
			});
			OnOpenAboutWindow = ReactiveCommand.Create(() => {
				this.IsAboutVisible = true;
			});
			OnCloseAboutPopup = ReactiveCommand.Create(() => {
				this.IsAboutVisible = false;
			});
			OnCloseMessagePopup = ReactiveCommand.Create(() => {
				this.IsPopupVisible = false;
				this.OutputMessage = "";
			});

			_fortiConnector = fortiConnector ?? Splat.Locator.Current.GetService<FortiConnector>();
			_appSettings = appSettings ?? Splat.Locator.Current.GetService<AppSettings>();
			_appSettingsWriter = appSettingsWriter ?? Splat.Locator.Current.GetService<AppSettingsWriter>();
			this.VpnUserName = _appSettings?.Vpn?.UserName;
			this.VpnPassword = _appSettings?.Vpn?.Password;
			this.EmailUserName = _appSettings?.EmailAccount?.Email;
			this.EmailPassword = _appSettings?.EmailAccount?.Password;
			this.EmailProtocol = _appSettings?.EmailServer?.Protocol ?? EmailServerProtocol.Exchange;
			this.EmailServer = _appSettings?.EmailServer?.Server;
			this.EmailPort = _appSettings?.EmailServer?.Port ?? 993;
			this.MarkVpnEmailAsRead = _appSettings?.EmailAccount?.MarkVpnEmailAsRead ?? true;
			this.EmailProtocolOptions = Enum.GetValues(typeof(EmailServerProtocol)).Cast<EmailServerProtocol>()
				.Where(p => p != EmailServerProtocol.MictrosoftGraph && p != EmailServerProtocol.Mapi); // Exclude not implemented options.
		}

		public EmailConfig GetEmailConfig()
		{
			return new EmailConfig{
				 Protocol = EmailProtocol
				,ServerName = EmailServer
				,ServerPort = EmailPort
				,UserEmail = EmailUserName
				,UserPassword = EmailPassword
				,UseSsl = true
				,EmailSubjectPrefix = _appSettings?.Vpn?.EmailSubjectPrefix
				,InboxSubFolderNameWithVpnEmails = _appSettings?.EmailAccount?.InboxSubFolderNameWithVpnEmails
				,GetEmailRetryEveryMilliseconds = _appSettings?.GetEmailRetryEveryMilliseconds
				,GetEmailMaxRetries = _appSettings?.GetEmailMaxRetries
			};
		}

		public void LoginToVpn()
		{
			var emailConfig = GetEmailConfig();
			this.EmailVpnCode = _fortiConnector.Login(VpnPassword, emailConfig, MarkVpnEmailAsRead);
		}

		public void GetEmailVpnCodeAndCopyToClipboard()
		{
			GetEmailVpnCode();
			CopyEmailVpnCode();
		}
		
		public string GetEmailVpnCode()
		{
			var emailConfig = GetEmailConfig();
			this.EmailVpnCode = _fortiConnector.GetVpnEmailCode(emailConfig);
			return EmailVpnCode;
		}

		public void CopyEmailVpnCode()
		{
			Avalonia.Application.Current.Clipboard.SetTextAsync(EmailVpnCode);
			//System.Windows.Forms.Clipboard.SetText(EmailVpnCode); // WinForms is Windows only, and it requires the Program.Main method to be [STAThread].
		}

		public void OpenLinkLicense()
		{
			Util.OpenUrl(URL_LICENSE);
		}

		public void OpenLinkGitHub()
		{
			Util.OpenUrl(URL_GITHUB);
		}

		public void SaveConfig()
		{
			var newAppSettings = new AppSettings()
			{
				Vpn = new VpnConfig {
					 UserName = this.VpnUserName
					,Password = this.VpnPassword
					,EmailSubjectPrefix = null
				},
				EmailServer = new EmailServerConfig {
					 Protocol = this.EmailProtocol
					,Server = this.EmailServer
					,Port = this.EmailPort
				},
				EmailAccount = new EmailAccountConfig {
					 Email = this.EmailUserName
					,Password = this.EmailPassword
					,MarkVpnEmailAsRead = this.MarkVpnEmailAsRead
					,InboxSubFolderNameWithVpnEmails = null
				}
				,DelayToShowVpnClient = null
				,DelayToSpawnFortiClientProcess = null
				,DelayToFetchVpnCodeEmail = null
				,GetEmailRetryEveryMilliseconds = null
				,GetEmailMaxRetries = null
			};
			_appSettingsWriter.Save(newAppSettings, fileName: Program.APPSETTINGS_AUTOSAVE_FILENAME);
		}

		public T TryAction<T>(Func<T> function)
		{
			try
			{
				return function();
			}
			catch(Exception ex)
			{
				this.OutputMessage = $"{ex.GetType()}: {ex}";
				this.IsPopupVisible = true;
			}
			return default(T);
		}

		public void TryAction(Action action)
		{
			try
			{
				action();
			}
			catch(Exception ex)
			{
				this.OutputMessage = $"{ex.GetType()}: {ex}";
				this.IsPopupVisible = true;
			}
		}
	}
}
