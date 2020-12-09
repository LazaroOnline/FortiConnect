using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Splat;
using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Logging.Serilog;
using FortiConnect.ViewModels;

namespace FortiConnect
{
    public class ConsoleApp : IHostedService
	{
		private readonly IHostApplicationLifetime _applicationLifetime;
		private ILogger<App> _logger;
		private MainWindowViewModel _mainWindowViewModel;
		
		public ConsoleApp(
			IHostApplicationLifetime applicationLifetime,
			ILogger<App> logger,
			MainWindowViewModel mainWindowViewModel
		) {
			_applicationLifetime = applicationLifetime;
			_logger = logger;
			_mainWindowViewModel = mainWindowViewModel;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			try
            {
				await Execute();
				Environment.ExitCode = 0;
			}
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Unexpected error exception!");
                Environment.ExitCode = 1;
            }
			_applicationLifetime.StopApplication();
		}
		
		public async Task Execute()
		{
			_logger.LogDebug($"Starting...");
			//await _mainWindowViewModel.Main();
			
			var emailVpnCode = _mainWindowViewModel.GetEmailVpnCode();
			Console.WriteLine($"Email VPN Code: {emailVpnCode}");

			_logger.LogDebug($"App finished running.");
		}
		
		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
