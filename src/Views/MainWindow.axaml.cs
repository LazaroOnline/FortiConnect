using Avalonia.Markup.Xaml;

namespace FortiConnect.Views;

public partial class MainWindow : Window
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

	protected override void OnClosing(WindowClosingEventArgs e)
	{
		base.OnClosing(e);
		var fortiConnectForm = this.FindControl<FortiConnectForm>("FortiConnectForm");
		var viewModel = (FortiConnectFormViewModel)fortiConnectForm.DataContext;
		viewModel.SaveConfig();
	}

}
