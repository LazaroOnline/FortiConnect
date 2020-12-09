using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Splat;
using ReactiveUI;
using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Logging.Serilog;
using FortiConnect;
using FortiConnect.Models;
using FortiConnect.ViewModels;
using FortiConnect.Services;
using System.Runtime.InteropServices;

namespace FortiConnect
{
	public class Program
	{
		public enum AppCommand
		{
			GetEmailVpnCode,
			LoginToVpn,
		}

		public const string APPSETTINGS_FILENAME = "AppSettings.json";
		public const string APPSETTINGS_LOCAL_FILENAME = "AppSettings.local.json";
		public const string APPSETTINGS_AUTOSAVE_FILENAME = "AppSettings.AutoSave.json";

		// Initialization code. Don't use any Avalonia, third-party APIs or any
		// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
		// yet and stuff might break.
		// [STAThread]
		public static async Task Main(string[] args)
		{
			
			IntPtr h = GetConsoleWindow();
			ShowWindow(h, 0);

			Console.WriteLine("Starting FortiConnect...");
			var configBuilder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile(APPSETTINGS_FILENAME, optional: true)
				.AddJsonFile(APPSETTINGS_LOCAL_FILENAME, optional: true)
				.AddJsonFile(APPSETTINGS_AUTOSAVE_FILENAME, optional: true)
				.AddCommandLine(args);
			var config = configBuilder.Build();
			
			// Dependency Injection.
			RegisterServices(config);


			if (args.Any(arg => IsCommandArgument(arg, AppCommand.GetEmailVpnCode))) {


				// switch here to show or not show console window
				// ShowWindow(h, 1);
				
				var program = new Program();
				var host = new Microsoft.Extensions.Hosting.HostBuilder()
					.ConfigureAppConfiguration((hostContext, configApp) =>
					{
					})
					.ConfigureLogging((hostContext, logging) =>
					{
						logging
							.AddConfiguration(hostContext.Configuration.GetSection("Logging"))
							//.SetMinimumLevel(LogLevel.Trace)
							.AddDebug()
							//.AddNLog(Configuration)
						;
					})
					.ConfigureServices((hostContext, services) =>
					{
						/*
						*/
						//services.AddTransient<MainWindowViewModel>();
						services.AddTransient<IEmailService, EmailService>();
						services.AddTransient<MainWindowViewModel>((x) => Splat.Locator.Current.GetService<MainWindowViewModel>());
						services.AddHostedService<ConsoleApp>();
						//services.RegisterServicesForDependencyInjection();
					})
					.UseConsoleLifetime();

				await host.Build().RunAsync();

				return;
			}
			
			if (args.Any(arg => IsCommandArgument(arg, AppCommand.LoginToVpn))) {
				var viewModel = Splat.Locator.Current.GetService<MainWindowViewModel>();
				viewModel.LoginToVpn();
				Console.WriteLine($"Done.");
				return;
			}

			BuildAvaloniaApp()
			.StartWithClassicDesktopLifetime(args);
		}

		// Avalonia configuration, don't remove; also used by visual designer.
		public static AppBuilder BuildAvaloniaApp()
			=> AppBuilder.Configure<App>()
				.UsePlatformDetect()
				.LogToDebug()
				.UseReactiveUI();

		
		// https://www.reactiveui.net/docs/handbook/dependency-inversion/
		// https://dev.to/ingvarx/avaloniaui-dependency-injection-4aka
		// Example: https://github.com/rbmkio/radish/blob/master/src/Rbmk.Radish/Program.cs
		// Other ways of DI: https://github.com/egramtel/egram.tel/blob/master/src/Tel.Egram/Program.cs
		public static void RegisterServices(IConfiguration config)
		{
			RegisterServices(Locator.CurrentMutable, Locator.Current, config);
		}
		
		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("kernel32.dll")]
		static extern IntPtr GetConsoleWindow();

		public static void RegisterServices(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver, IConfiguration config)
		{
			var appSettings = config.Get<AppSettings>();

			// Splat.Locator.CurrentMutable.Register<EmailService>(() => new EmailExchangeService());

			services.Register<AppSettings>(() => config.Get<AppSettings>());
			services.Register<AppSettingsWriter>(() => new AppSettingsWriter());
			services.Register<IEmailService>(() => new EmailService());

			services.RegisterLazySingleton<FortiConnector>(() => {
				var emailService = resolver.GetService<IEmailService>();
				var fortiConnector = new FortiConnector(emailService, appSettings.EmailAccount?.MarkVpnEmailAsRead ?? false);
				if (!string.IsNullOrWhiteSpace(appSettings.FortiClient.ExeFullPath)) {
					fortiConnector.FortiClientExeFullPath = appSettings.FortiClient.ExeFullPath;
				}
				if (!string.IsNullOrWhiteSpace(appSettings.FortiClient.ProcessName)) {
					fortiConnector.FortiClientProcessName = appSettings.FortiClient.ProcessName;
				}
				return fortiConnector;
			});
			
			services.RegisterLazySingleton<MainWindowViewModel>(() => {
				var mainWindowViewModel = new MainWindowViewModel();
				return mainWindowViewModel;
			});

			
		}

		public static bool IsCommandArgument(string arg, AppCommand command)
		{
			var commandName = command.ToString().ToLower();
			var argName = arg.ToLower().TrimStart('-', '/', '\\').Trim();
			return argName == commandName;
		}
	}
}
