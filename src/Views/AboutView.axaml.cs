using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace FortiConnect.Views;

public partial class AboutView : UserControl
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

		dataContextViewModel.OnCloseView.Subscribe(x => CloseDialog());
		this.DataContext = dataContextViewModel;
		InputElement.KeyDownEvent.AddClassHandler<TopLevel>(OnKeyDownParent, handledEventsToo: true);
	}

	public void CloseDialog()
	{
		var eventArgs = new RoutedEventArgs { RoutedEvent = ExitViewEvent, Source = this };
		this.RaiseEvent(eventArgs);
	}

	protected void OnKeyDownParent(object sender, KeyEventArgs e)
	{
		OnKeyDown(e);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.Key == Key.Escape && this.IsVisible)
		{
			CloseDialog();
			e.Handled = true;
		}
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}

}
