using System;
using System.Linq;
using System.Text;
using System.Reactive;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FortiConnect.Services;
using FortiConnect.Models;
using ReactiveUI;
using Splat;

namespace FortiConnect.ViewModels
{
	public class AboutViewModel : ViewModelBase
	{
		private const string URL_LICENSE = "https://opensource.org/licenses/MIT";
		private const string URL_GITHUB = "https://github.com/LazaroOnline/FortiConnect";

		public string GitVersion   { get; set; } = GitVersionService.GetGitVersionAssemblyInfo().ToString();
		
		public ReactiveCommand<Unit, Unit> OnOpenAboutWindow { get; }
		public ReactiveCommand<Unit, Unit> OnCloseView { get; }

		public AboutViewModel()
		{
			OnCloseView = ReactiveCommand.Create(() => { });
		}
		
		public void OpenLinkLicense()
		{
			Utils.OpenUrl(URL_LICENSE);
		}

		public void OpenLinkGitHub()
		{
			Utils.OpenUrl(URL_GITHUB);
		}

	}
}
