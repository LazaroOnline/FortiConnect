﻿using System;
using System.Linq;
using System.Text;
using System.Reactive;
using FortiConnect.Models;
using FortiConnect.Services;
using System.Collections.Generic;
using ReactiveUI;
using Splat;

namespace FortiConnect.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		public string VpnUserName   { get; set; }
		public string VpnPassword   { get; set; }
		
		public EmailServerProtocol _emailProtocol;
		public EmailServerProtocol EmailProtocol  {
			get => _emailProtocol;
			set => this.RaiseAndSetIfChanged(ref _emailProtocol, value);
		}
		
		public bool _isPopupVisible;
		public bool IsPopupVisible  {
			get => _isPopupVisible;
			set => this.RaiseAndSetIfChanged(ref _isPopupVisible, value);
		}
		
		public string _outputMessage;
		public string OutputMessage  {
			get => _outputMessage;
			set => this.RaiseAndSetIfChanged(ref _outputMessage, value);
		}
		
		public string _outputMessageTitle = "Error";
		public string OutputMessageTitle  {
			get => _outputMessageTitle;
			set => this.RaiseAndSetIfChanged(ref _outputMessageTitle, value);
		}

		public IEnumerable<EmailServerProtocol> EmailProtocolOptions { get; set; }

		public string EmailServer   { get; set; }
		public int    EmailPort     { get; set; }

		public string EmailUserName { get; set; }
		public string EmailPassword { get; set; }

		public string _emailVpnCode;
		public string EmailVpnCode  {
			get => _emailVpnCode;
			set => this.RaiseAndSetIfChanged(ref _emailVpnCode, value);
		}

		public bool _isPortEnabled;
		public bool IsPortEnabled  {
			get => _isPortEnabled;
			set => this.RaiseAndSetIfChanged(ref _isPortEnabled, value);
		}

		public bool _isEnabledCopyEmail;
		public bool IsEnabledCopyEmail  {
			get => _isEnabledCopyEmail;
			set => this.RaiseAndSetIfChanged(ref _isEnabledCopyEmail, value);
		}

		private FortiConnector _fortiConnector { get; set; }
		private AppSettings _appSettings { get; set; }
		private AppSettingsWriter _appSettingsWriter { get; set; }


		// https://avaloniaui.net/docs/controls/button
		public ReactiveCommand<Unit, Unit> OnConnectToVpnCommand { get; }
		public ReactiveCommand<Unit, Unit> OnGetEmailVpnCodeCommand { get; }
		public ReactiveCommand<Unit, Unit> OnCopyEmailVpnCodeCommand { get; }
		public ReactiveCommand<Unit, Unit> OnCloseMessagePopup { get; }

		// Constructor required by the designer tools.
		public MainWindowViewModel()
			: this(null, null, null) // Call the other constructor
		{ }

		public MainWindowViewModel(
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
			this.EmailProtocolOptions = Enum.GetValues(typeof(EmailServerProtocol)).Cast<EmailServerProtocol>();
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
			};
		}

		public void LoginToVpn()
		{
			var emailConfig = GetEmailConfig();
			this.EmailVpnCode = _fortiConnector.Login(VpnPassword, emailConfig, _appSettings.EmailAccount.MarkVpnEmailAsRead);
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

		public void SaveConfig()
		{
			var newAppSettings = new AppSettings()
			{
				Vpn = new VpnConfig {
					 UserName = this.VpnUserName
					,Password = this.VpnPassword
				},
				EmailServer = new EmailServerConfig {
					 Protocol = this.EmailProtocol
					,Server = this.EmailServer
					,Port = this.EmailPort
				},
				EmailAccount = new EmailAccountConfig {
					 Email = this.EmailUserName
					,Password = this.EmailPassword
					,MarkVpnEmailAsRead = false
				},
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
