using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

namespace FortiConnect.Views;

public partial class FortiConnectForm : UserControl
{
	public FortiConnectForm()
	{
		this.InitializeComponent();
		//this.OnClosing += Closing;
		SubscribeToCapsLockEvent();
	}

	private void SubscribeToCapsLockEvent()
	{
		// TODO: find a better reactive way to listen to the CapsLock event.
		
		var connectToVpnButton = this.FindControl<Button>("ConnectToVpnButton");
		connectToVpnButton.GotFocus += (obj, ev) => SetCapsLockWarning();
		connectToVpnButton.LostFocus += (obj, ev) => SetCapsLockWarning();
		this.GotFocus += (obj, ev) => SetCapsLockWarning();
		this.LostFocus += (obj, ev) => SetCapsLockWarning();
		this.KeyDown += (obj, ev) => SetCapsLockWarning();
		this.PointerMoved += (obj, ev) => SetCapsLockWarning();
		SetCapsLockWarning();
	}

	private void SetCapsLockWarning()
	{
		// TODO: get isCapsLock from Avalonia framework once it is supported: 
		// https://github.com/AvaloniaUI/Avalonia/issues/2422
		//var isCapsLock = Avalonia.Input.Key.CapsLock   Avalonia.Input.KeyStates.Toggled

		var isCapsLock = System.Windows.Forms.Control.IsKeyLocked(System.Windows.Forms.Keys.CapsLock);
		var capsLockWarning = this.FindControl<Control>("CapsLockWarning");
		// System.Diagnostics.Debug.WriteLine("IsCapsLock: " + isCapsLock);
		capsLockWarning.IsVisible = isCapsLock;
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
	
	// TODO: try to bind "OnCloseAboutPopup" directly from XAML instead from this code-behind:
	public void AboutViewExitHandler(object sender, RoutedEventArgs e)
	{
		var viewModel = (FortiConnectFormViewModel)this.DataContext;
		viewModel.IsAboutVisible = false;
		e.Handled=true;
	}

}
