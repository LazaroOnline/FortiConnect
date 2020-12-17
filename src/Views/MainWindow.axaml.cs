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
