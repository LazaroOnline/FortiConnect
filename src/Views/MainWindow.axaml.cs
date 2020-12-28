using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FortiConnect.ViewModels;
using Avalonia.Interactivity;

namespace FortiConnect.Views
{
	public class MainWindow : Window
	{

		public MainWindow()
		{
			InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
			//this.OnClosing += Closing;
			
			var vpnPasswordTextBox = this.FindControl<TextBox>("VpnPassword");
			var toggleShowVpnPasswordButton = this.FindControl<Button>("ToggleShowVpnPassword");
			var vpnPasswordHiddenChar = vpnPasswordTextBox.PasswordChar;
			toggleShowVpnPasswordButton.Click += (o, e) => {
				var isPasswordHidden = vpnPasswordTextBox.PasswordChar == vpnPasswordHiddenChar;
				if (isPasswordHidden)
					vpnPasswordTextBox.PasswordChar = '\0' ;
				else
					vpnPasswordTextBox.PasswordChar = vpnPasswordHiddenChar;
			};
			
			var emailPasswordTextBox = this.FindControl<TextBox>("EmailPassword");
			var toggleShowEmailPasswordButton = this.FindControl<Button>("ToggleShowEmailPassword");
			var emailPasswordHiddenChar = emailPasswordTextBox.PasswordChar;
			toggleShowEmailPasswordButton.Click += (o, e) => {
				var isPasswordHidden = emailPasswordTextBox.PasswordChar == emailPasswordHiddenChar;
				if (isPasswordHidden)
					emailPasswordTextBox.PasswordChar = '\0' ;
				else
					emailPasswordTextBox.PasswordChar = emailPasswordHiddenChar;
			};
			
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			var viewModel = (MainWindowViewModel)this.DataContext;
			viewModel.SaveConfig();
		}

		// TODO: try to bind "OnCloseAboutPopup" directly from XAML instead from this code-behind:
		public void AboutViewExitHandler(object sender, RoutedEventArgs e)
		{
			var viewModel = (MainWindowViewModel)this.DataContext;
			viewModel.IsAboutVisible = false;
			e.Handled=true;
		}

	}
}
