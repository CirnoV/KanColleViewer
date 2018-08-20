using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleViewer.Models;
using Grabacr07.KanColleViewer.ViewModels;
using MetroRadiance.Interop;
using MetroTrilithon.UI.Controls;
using Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT;
using IViewObject = Grabacr07.KanColleViewer.Win32.IViewObject;
using IServiceProvider = Grabacr07.KanColleViewer.Win32.IServiceProvider;
//using WebBrowser = System.Windows.Controls.WebBrowser;
using WebView = Microsoft.Toolkit.Win32.UI.Controls.WPF.WebView;
using KanColleSettings = Grabacr07.KanColleViewer.Models.Settings.KanColleSettings;

namespace Grabacr07.KanColleViewer.Views.Controls
{
	[ContentProperty(nameof(WebView))]
	[TemplatePart(Name = PART_ContentHost, Type = typeof(ScrollViewer))]
	public class KanColleHost : Control
	{
		private const string PART_ContentHost = "PART_ContentHost";

		public static Size KanColleSize { get; } = new Size(1200.0, 720.0);
		public static Size InitialSize { get; } = new Size(1200.0, 720.0);

		static KanColleHost()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(KanColleHost), new FrameworkPropertyMetadata(typeof(KanColleHost)));
		}

		private ScrollViewer scrollViewer;
		private bool styleSheetApplied;
		private Dpi? systemDpi;
		private bool firstLoaded;

		#region WebView 依存関係プロパティ

		public WebView WebView
		{
			get { return (WebView)this.GetValue(WebViewProperty); }
			set { this.SetValue(WebViewProperty, value); }
		}

		public static readonly DependencyProperty WebViewProperty =
			DependencyProperty.Register(nameof(WebView), typeof(WebView), typeof(KanColleHost), new UIPropertyMetadata(null, WebViewPropertyChangedCallback));

		private static void WebViewPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var instance = (KanColleHost)d;
			var newBrowser = (WebView)e.NewValue;
			var oldBrowser = (WebView)e.OldValue;

			if (oldBrowser != null)
			{
				oldBrowser.NavigationCompleted -= instance.HandleNavigationCompleted;
				oldBrowser.DOMContentLoaded -= instance.HandleDOMContentLoaded;
				oldBrowser.NewWindowRequested -= instance.HandleNewWindowRequested;
			}
			if (newBrowser != null)
			{
				newBrowser.NavigationCompleted += instance.HandleNavigationCompleted;
				newBrowser.DOMContentLoaded += instance.HandleDOMContentLoaded;
				newBrowser.NewWindowRequested += instance.HandleNewWindowRequested;
			}
			if (instance.scrollViewer != null)
			{
				instance.scrollViewer.Content = newBrowser;
			}

			WebBrowserHelper.SetAllowWebBrowserDrop(newBrowser, false);
		}

		#endregion

		#region ZoomFactor 依存関係プロパティ

		/// <summary>
		/// ブラウザーのズーム倍率を取得または設定します。
		/// </summary>
		public double ZoomFactor
		{
			get { return (double)this.GetValue(ZoomFactorProperty); }
			set { this.SetValue(ZoomFactorProperty, value); }
		}

		/// <summary>
		/// <see cref="ZoomFactor"/> 依存関係プロパティを識別します。
		/// </summary>
		public static readonly DependencyProperty ZoomFactorProperty =
			DependencyProperty.Register(nameof(ZoomFactor), typeof(double), typeof(KanColleHost), new UIPropertyMetadata(1.0, ZoomFactorChangedCallback));

		private static void ZoomFactorChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var instance = (KanColleHost)d;

			instance.Update();
		}

		#endregion

		#region UserStyleSheet 依存関係プロパティ

		/// <summary>
		/// ユーザー スタイル シートを取得または設定します。
		/// </summary>
		public string UserStyleSheet
		{
			get { return (string)this.GetValue(UserStyleSheetProperty); }
			set { this.SetValue(UserStyleSheetProperty, value); }
		}

		/// <summary>
		/// <see cref="UserStyleSheet"/> 依存関係プロパティを識別します。
		/// </summary>
		public static readonly DependencyProperty UserStyleSheetProperty =
			DependencyProperty.Register(nameof(UserStyleSheet), typeof(string), typeof(KanColleHost), new UIPropertyMetadata(string.Empty, UserStyleSheetChangedCallback));

		private static void UserStyleSheetChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var instance = (KanColleHost)d;

			instance.ApplyStyleSheet();
		}

		#endregion

		public event EventHandler<Size> OwnerSizeChangeRequested;

		public KanColleHost()
		{
			this.Loaded += (sender, args) => this.Update();
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.scrollViewer = this.GetTemplateChild(PART_ContentHost) as ScrollViewer;
			if (this.scrollViewer != null)
			{
				this.scrollViewer.Content = this.WebView;
			}
		}

		public void Update()
		{
			if (this.WebView == null) return;
			this.WebView.IsPrivateNetworkClientServerCapabilityEnabled = true;
			this.WebView.IsScriptNotifyAllowed = true;

			var dpi = this.systemDpi ?? (this.systemDpi = this.GetSystemDpi()) ?? Dpi.Default;
			var zoomFactor = dpi.ScaleX + (this.ZoomFactor - 1.0);
			var percentage = (int)(zoomFactor * 100);

			ApplyZoomFactor(this, percentage);

			var size = this.styleSheetApplied ? KanColleSize : InitialSize;
			size = new Size(
				(size.Width * (zoomFactor / dpi.ScaleX)) / dpi.ScaleX,
				(size.Height * (zoomFactor / dpi.ScaleY)) / dpi.ScaleY);
			this.WebView.Width = size.Width;
			this.WebView.Height = size.Height;

			this.OwnerSizeChangeRequested?.Invoke(this, size);
		}

		private static void ApplyZoomFactor(KanColleHost target, int zoomFactor)
		{
			if (!target.firstLoaded) return;

			if (zoomFactor < 10 || zoomFactor > 1000)
			{
				StatusService.Current.Notify(string.Format(Properties.Resources.ZoomAction_OutOfRange, zoomFactor));
				return;
			}

			try
			{
				// https://gist.github.com/chgeuer/3514112
				Func<WebView, double, bool> zoom = (wv, zoomPercentage) =>
				{
					var script = string.Format(
						"(function () {{ document.styleSheets[0]['rules'][0].style['zoom'] = {0}; }})();", zoomPercentage);

					try
					{
						var result = wv.InvokeScript("eval", new string[] { script });
						return true;
					}
					catch (Exception ex)
					{
						var notReady = (uint)ex.HResult == 0x80020101;
						if (notReady) return false;
						throw;
					}
				};

				var provider = target.WebView;
				if (provider == null) return;

				zoom(provider, zoomFactor / 100.0);
			}
			catch (Exception) when (Application.Instance.State == ApplicationState.Startup)
			{
				// about:blank だから仕方ない
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				StatusService.Current.Notify(string.Format(Properties.Resources.ZoomAction_ZoomFailed, ex.Message));
			}
		}

		private void HandleNavigationCompleted(object sender, WebViewControlNavigationCompletedEventArgs e)
		{
			if (e.Uri.AbsoluteUri == KanColleViewer.Properties.Settings.Default.KanColleUrl.AbsoluteUri)
			{
				this.firstLoaded = true;

				this.ApplyStyleSheet();
				this.Update();
			}
		}
		private void HandleDOMContentLoaded(object sender, WebViewControlDOMContentLoadedEventArgs e)
		{
			this.ApplyStyleSheet();
			this.Update();
		}
		private void HandleNewWindowRequested(object sender, WebViewControlNewWindowRequestedEventArgs e)
		{
			e.Handled = true;

			var window = new BrowserWindow { DataContext = new NavigatorViewModel(), };
			window.Show();
			window.WebBrowser.Navigate(e.Uri);
		}

		private void ApplyStyleSheet()
		{
			if (!this.firstLoaded) return;

			try
			{
				var script = string.Format(
					"var x=document.createElement('style');x.type='text/css';x.innerHTML='{0}';document.body.appendChild(x);",
					this.UserStyleSheet.Replace("'", "\\'").Replace("\r", "").Replace("\n", "\\n")
				);
				WebView.InvokeScript("eval", new string[] { script });
				this.styleSheetApplied = true;
			}
			catch (Exception) when (Application.Instance.State == ApplicationState.Startup)
			{
				// about:blank だから仕方ない
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				StatusService.Current.Notify("failed to apply css: " + ex.Message);
			}
		}
	}
}
