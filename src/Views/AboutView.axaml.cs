using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FortiConnect.ViewModels;

namespace FortiConnect.Views
{
	public class AboutView : UserControl
	{
		// https://avaloniaui.net/docs/input/events
		public static readonly RoutedEvent<RoutedEventArgs> ExitViewEvent =
			RoutedEvent.Register<AboutView, RoutedEventArgs>(nameof(ExitView), RoutingStrategies.Bubble);

		public event EventHandler<RoutedEventArgs> ExitView
		{
			add => AddHandler(ExitViewEvent, value);
			remove => RemoveHandler(ExitViewEvent, value);
		}
		
		public AboutView() : this(null)
		{ }

		public AboutView(AboutViewModel viewModel = null)
		{
			this.InitializeComponent();
			var dataContextViewModel = viewModel ?? new AboutViewModel();

			dataContextViewModel.OnCloseView.Subscribe(x => {
				var eventArgs = new RoutedEventArgs { RoutedEvent = ExitViewEvent, Source = this};
				this.RaiseEvent(eventArgs);

				});
			this.DataContext = dataContextViewModel;
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

	}
}
