﻿using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleViewer.Models;
using Grabacr07.KanColleViewer.ViewModels.Composition;
using Grabacr07.KanColleViewer.ViewModels.Messages;
using Grabacr07.KanColleWrapper;
using Livet;
using Livet.EventListeners;
using Livet.Messaging;
using Livet.Messaging.Windows;
using MetroRadiance;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Settings2 = Grabacr07.KanColleViewer.Models.Settings;

//Exit Box Add https://github.com/yuyuvn/KanColleViewer/commit/331a1ca5c87032bdafdbd20d9eed6005082d6520
namespace Grabacr07.KanColleViewer.ViewModels
{
	public class MainWindowViewModel : WindowViewModel
	{
		private Mode currentMode;
		private MainContentViewModel mainContent;

		public NavigatorViewModel Navigator { get; private set; }
		public SettingsViewModel Settings { get; private set; }
		#region RefreshNavigator

		private ICommand _RefreshNavigator;
		public ICommand RefreshNavigator
		{
			get { return _RefreshNavigator; }
		}

		#endregion

		#region Mode 変更通知プロパティ

		public Mode Mode
		{
			get { return this.currentMode; }
			set
			{
				this.currentMode = value;
				switch (value)
				{
					case Mode.NotStarted:
						this.Content = StartContentViewModel.Instance;
						this.StatusBar = StartContentViewModel.Instance;
						StatusService.Current.Set(Properties.Resources.StatusBar_NotStarted);
						ThemeService.Current.ChangeAccent(Accent.Purple);
						break;
					case Mode.Started:
						this.Content = this.mainContent ?? (this.mainContent = new MainContentViewModel());
						StatusService.Current.Set(Properties.Resources.StatusBar_Ready);
						ThemeService.Current.ChangeTheme(Theme.Dark);
						ThemeService.Current.ChangeAccent(Accent.Blue);
						break;
					case Mode.InSortie:
						//ThemeService.Current.ChangeAccent(Accent.Orange);
						break;
					case Mode.CriticalCondition:
						ThemeService.Current.ChangeAccent(Accent.Red);
						ThemeService.Current.ChangeTheme(Theme.CriticalRed);
						break;
				}

				this.RaisePropertyChanged();
			}
		}

		#endregion

		#region Content 変更通知プロパティ

		private ViewModel _Content;

		public ViewModel Content
		{
			get { return this._Content; }
			set
			{
				if (this._Content != value)
				{
					this._Content = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region StatusMessage 変更通知プロパティ

		private string _StatusMessage;

		public string StatusMessage
		{
			get { return this._StatusMessage; }
			set
			{
				if (this._StatusMessage != value)
				{
					this._StatusMessage = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region StatusBar 変更通知プロパティ

		private ViewModel _StatusBar;

		public ViewModel StatusBar
		{
			get { return this._StatusBar; }
			set
			{
				if (this._StatusBar != value)
				{
					this._StatusBar = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region TopMost 変更通知プロパティ

		public bool TopMost
		{
			get { return Models.Settings.Current.TopMost; }
			set
			{
				if (Models.Settings.Current.TopMost != value)
				{
					Models.Settings.Current.TopMost = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region CanClose 変更通知プロパティ

		private bool _CanClose;

		public bool CanClose
		{
			get { return this._CanClose; }
			set
			{
				if (this._CanClose != value)
				{
					this._CanClose = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region Items 変更通知プロパティ

		private List<ToolViewModel> _Tools;

		public List<ToolViewModel> Tools
		{
			get { return this._Tools; }
			set
			{
				if (this._Tools != value)
				{
					this._Tools = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region VerticalBar 変更通知プロパティ

		private Visibility _VerticalBar;

		public Visibility VerticalBar
		{
			get { return this._VerticalBar; }
			set
			{
				if (this._VerticalBar != value)
				{
					this._VerticalBar = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region HorizontalBar 変更通知プロパティ

		private Visibility _HorizontalBar;

		public Visibility HorizontalBar
		{
			get { return this._HorizontalBar; }
			set
			{
				if (this._HorizontalBar != value)
				{
					this._HorizontalBar = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion


		public MainWindowViewModel()
		{
			if (Settings2.Current.Orientation == OrientationType.Vertical)
			{
				this.VerticalBar = Visibility.Visible;
				this.HorizontalBar = Visibility.Collapsed;
			}
			else
			{
				this.VerticalBar = Visibility.Collapsed;
				this.HorizontalBar = Visibility.Visible;
			}

			Settings2.Current.VerticalWindow += () =>
			{
				this.VerticalBar = Visibility.Visible;
				this.HorizontalBar = Visibility.Collapsed;
			};
			Settings2.Current.HorizontalWindow += () =>
			{
				this.VerticalBar = Visibility.Collapsed;
				this.HorizontalBar = Visibility.Visible;
			};

			this.Title = App.ProductInfo.Title;
			this.Navigator = new NavigatorViewModel();
			this.Settings = new SettingsViewModel();

			this.CompositeDisposable.Add(new PropertyChangedEventListener(StatusService.Current)
			{
				{ () => StatusService.Current.Message, (sender, args) => this.StatusMessage = StatusService.Current.Message },
			});
			this.CompositeDisposable.Add(new PropertyChangedEventListener(KanColleClient.Current)
			{
				{ () => KanColleClient.Current.IsStarted, (sender, args) => this.UpdateMode() },
				{ () => KanColleClient.Current.IsInSortie, (sender, args) => this.UpdateMode() },
			});

			UpdateCloseConfirm();
			this.CompositeDisposable.Add(new PropertyChangedEventListener(Settings2.Current) 
			{
				{ "CloseConfirm", (sender, args) => UpdateCloseConfirm() }, 
				{ "CloseConfirmOnlyInSortie", (sender, args) => UpdateCloseConfirm() }, 
			});

			this.UpdateMode();

			this.Tools = new List<ToolViewModel>(PluginHost.Instance.Tools.Select(x => new ToolViewModel(x)));


			_RefreshNavigator = new RelayCommand(Navigator.ReNavigate);
		}

		public void TakeScreenshot()
		{
			var path = Helper.CreateScreenshotFilePath();
			var message = new ScreenshotMessage("Screenshot/Save") { Path = path, };

			this.Messenger.Raise(message);

			var notify = message.Response.IsSuccess
				? Properties.Resources.Screenshot_Saved + Path.GetFileName(path)
				: Properties.Resources.Screenshot_Failed + message.Response.Exception.Message;
			StatusService.Current.Notify(notify);
		}


		/// <summary>
		/// メイン ウィンドウをアクティブ化することを試みます。
		/// </summary>
		public void Activate()
		{
			this.Messenger.Raise(new WindowActionMessage(WindowAction.Active, "Window/Activate"));
		}


		private void UpdateMode()
		{
			this.Mode = KanColleClient.Current.IsStarted
				? KanColleClient.Current.IsInSortie
					? Mode.InSortie
					: Mode.Started
				: Mode.NotStarted;
			UpdateCloseConfirm();
		}

		private void UpdateCloseConfirm()
		{
			this.CanClose = !Settings2.Current.CloseConfirm;
			if (Settings2.Current.CloseConfirmOnlyInSortie)
			{
				if (this.Mode != Mode.InSortie) this.CanClose = true;
			}
		}
		public void Closing()
		{
			if (!this.CanClose)
			{
				var message = new TransitionMessage(this, "Show/ExitDialog");
				this.Messenger.Raise(message);
			}
		}
		public void Close()
		{
			this.CanClose = true;
			var message = new TransitionMessage(this, "Close");
			this.Messenger.Raise(message);
		}
		public void ShowRefreshPopup()
		{
			if (Settings2.Current.RefreshConfirm && (!Settings2.Current.RefreshConfirmOnlyInSortie || !KanColleClient.Current.IsInSortie))
			{
				KanColleViewer.Views.MainWindow.Current.RefreshNavigator();
				return;
			}
			var window = new RefreshPopupViewModel();
			var message = new TransitionMessage(window, "Show/RefreshPopup");
			this.Messenger.RaiseAsync(message);

		}
	}
}
