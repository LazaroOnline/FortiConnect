using Microsoft.Extensions.Configuration;
using ReactiveUI.Avalonia;
using Splat;

namespace FortiConnect;

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
	[STAThread]
	public static void Main(string[] args)
	{
		Console.WriteLine($"Starting {nameof(FortiConnect)} app...");
		AppDomain.CurrentDomain.UnhandledException += (s, e) => {
			var ex = e.ExceptionObject as Exception;
			FileLogger.Log($"Crashed App error:{Environment.NewLine}{ex}");
		};
		TaskScheduler.UnobservedTaskException += (s, e) => {
			FileLogger.Log($"Crashed Task error:{Environment.NewLine}{e.Exception}");
			e.SetObserved();
		};

		var configBuilder = new ConfigurationBuilder()
			.SetBasePath(AppContext.BaseDirectory)
			.AddJsonFile(APPSETTINGS_FILENAME, optional: true)
			.AddJsonFile(APPSETTINGS_LOCAL_FILENAME, optional: true)
			.AddJsonFile(APPSETTINGS_AUTOSAVE_FILENAME, optional: true)
			.AddCommandLine(args);
		var config = configBuilder.Build();
		
		// Dependency Injection.
		RegisterServices(config);

		if (args.Any(arg => IsCommandArgument(arg, AppCommand.GetEmailVpnCode))) {
			var viewModel = Splat.Locator.Current.GetService<FortiConnectFormViewModel>();
			var emailVpnCode = viewModel.GetEmailVpnCode();
			Console.WriteLine($"Email VPN Code: {emailVpnCode}");
			return;
		}
		
		if (args.Any(arg => IsCommandArgument(arg, AppCommand.LoginToVpn))) {
			var viewModel = Splat.Locator.Current.GetService<FortiConnectFormViewModel>();
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
			.WithInterFont()
			.LogToTrace()
			.UseReactiveUI();

	// https://www.reactiveui.net/docs/handbook/dependency-inversion/
	// https://dev.to/ingvarx/avaloniaui-dependency-injection-4aka
	// Example: https://github.com/rbmkio/radish/blob/master/src/Rbmk.Radish/Program.cs
	// Other ways of DI: https://github.com/egramtel/egram.tel/blob/master/src/Tel.Egram/Program.cs
	public static void RegisterServices(IConfiguration config)
	{
		RegisterServices(Locator.CurrentMutable, Locator.Current, config);
	}

	public static void RegisterServices(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver, IConfiguration config)
	{
		var appSettings = config.Get<AppSettings>() ?? new AppSettings();

		// Splat.Locator.CurrentMutable.Register<EmailService>(() => new EmailExchangeService());

		services.Register<AppSettings>(() => config.Get<AppSettings>());
		services.Register<AppSettingsWriter>(() => new AppSettingsWriter());
		services.Register<IEmailService>(() => new EmailService());
		services.Register<IKeyboardState>(() => new KeyboardState());
		services.Register<IVirtualKeyboard>(() => new VirtualKeyboard_WindowsApi());
		//services.Register<ISendKeysService>(() => new SendKeysWithWindowsForms());
		services.Register<ISendKeysService>(() => new SendKeysWithWindowsApi(resolver.GetService<IVirtualKeyboard>()));
		services.Register<ISendKeysToProcessService>(() => {
			var sendKeysService = resolver.GetService<ISendKeysService>();
			return new SendKeysToProcessService(sendKeysService) {
				DelayToShowWindow = appSettings?.DelayToShowVpnClient ?? AppSettings.DEFAULT_DelayToShowVpnClient
			};
		});

		services.RegisterLazySingleton<FortiConnector>(() => {
			var emailService = resolver.GetService<IEmailService>();
			var sendKeysToProcessService = resolver.GetService<ISendKeysToProcessService>();
			var fortiConnector = new FortiConnector(emailService, sendKeysToProcessService, appSettings.EmailAccount?.MarkVpnEmailAsRead ?? false);
			fortiConnector.LoginPasswordFocusSequence = appSettings?.FortiClient?.LoginPasswordFocusSequence;
			fortiConnector.LoginVerificationFocusSequence = appSettings?.FortiClient?.LoginVerificationFocusSequence;
			if (!string.IsNullOrWhiteSpace(appSettings?.FortiClient?.ExeFullPath)) {
				fortiConnector.FortiClientExeFullPath = appSettings.FortiClient.ExeFullPath;
			}
			if (!string.IsNullOrWhiteSpace(appSettings?.FortiClient?.ProcessName)) {
				fortiConnector.FortiClientProcessName = appSettings.FortiClient.ProcessName;
			}
			if (appSettings?.DelayToSpawnFortiClientProcess != null) {
				fortiConnector.DelayToSpawnFortiClientProcess = appSettings.DelayToSpawnFortiClientProcess.Value;
			}
			if (appSettings?.DelayToFetchVpnCodeEmail != null) {
				fortiConnector.DelayToFetchVpnCodeEmail = appSettings.DelayToFetchVpnCodeEmail.Value;
			}
			return fortiConnector;
		});
		
		services.RegisterLazySingleton<FortiConnectFormViewModel>(() => {
			var fortiConnectFormViewModel = new FortiConnectFormViewModel();
			return fortiConnectFormViewModel;
		});
	}

	public static bool IsCommandArgument(string arg, AppCommand command)
	{
		var commandName = command.ToString().ToLower();
		var argName = arg.ToLower().TrimStart('-', '/', '\\').Trim();
		return argName == commandName;
	}
}
