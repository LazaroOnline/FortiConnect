using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.ComponentModel;
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
	}
}
