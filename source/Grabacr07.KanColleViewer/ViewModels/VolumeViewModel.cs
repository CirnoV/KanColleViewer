using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Models;
using Livet;
using Livet.EventListeners;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace Grabacr07.KanColleViewer.ViewModels
{
	public class VolumeViewModel : ViewModel
	{
		#region IsMute 変更通知プロパティ

		private bool _IsMute;

		public bool IsMute
		{
			get { return this._IsMute; }
			set
			{
				if (this._IsMute != value)
				{
					this._IsMute = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion


		public VolumeViewModel()
		{
			this.IsMute = IsMuted();
		}

		public void ToggleMute()
		{
			var newValue = !IsMuted();
			try
			{
				var MMDE = new MMDeviceEnumerator();
				var dev = MMDE.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
				var sessions = dev.AudioSessionManager.Sessions;
				var count = sessions.Count;
				for(var i=0; i<count; i++)
				{
					var sess = sessions[i];
					if (sess.GetProcessID != Process.GetCurrentProcess().Id) continue;
					sess.SimpleAudioVolume.Mute = newValue;
				}
				this.IsMute = dev.AudioSessionManager.SimpleAudioVolume.Mute;
			}
			catch
			{
			}
		}
		public static bool IsMuted()
		{
			try
			{
				var MMDE = new MMDeviceEnumerator();
				var dev = MMDE.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
				return dev.AudioSessionManager.SimpleAudioVolume.Mute;
			}
			catch
			{
			}
			return false;
		}
		public bool IsExistSoundDevice()
		{
			try
			{
				MMDeviceEnumerator MMDE = new MMDeviceEnumerator();
				MMDeviceCollection DevCol = MMDE.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
				return DevCol.Count > 0;
			}
			catch
			{
			}
			return false;
		}
	}
}
