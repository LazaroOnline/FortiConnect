using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FortiConnect.ViewModels;

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
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			var fortiConnectForm = this.FindControl<FortiConnectForm>("FortiConnectForm");
			var viewModel = (FortiConnectFormViewModel)fortiConnectForm.DataContext;
			viewModel.SaveConfig();
		}

	}
}
