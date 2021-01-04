using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FortiConnect.ViewModels;
using FortiConnect.Views;

namespace FortiConnect
{
	public class App : Application
	{
		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public override void OnFrameworkInitializationCompleted()
		{
			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				desktop.MainWindow = new MainWindow
				{
					DataContext = new FortiConnectFormViewModel(),
				};
			}

			base.OnFrameworkInitializationCompleted();
		}
	}
}
