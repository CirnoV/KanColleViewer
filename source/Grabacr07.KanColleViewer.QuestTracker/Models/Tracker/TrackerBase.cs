using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleViewer.QuestTracker;
using Grabacr07.KanColleViewer.QuestTracker.Models;

namespace Grabacr07.KanColleViewer.QuestTracker.Models.Tracker
{
	public delegate void ProgressChangedHandler();

	public abstract class TrackerBase
	{
		/// <summary>
		/// Is this quest tracking?
		/// </summary>
		public bool IsTracking { get; set; }

		/// <summary>
		/// Quest Id
		/// </summary>
		public abstract int Id { get; }

		/// <summary>
		/// Quest Type (Daily, Weekly, ...)
		/// </summary>
		public abstract QuestType Type { get; }

		/// <summary>
		/// Tracking Value Datas
		/// </summary>
		public abstract TrackingValue[] Datas { get; protected set; }

		/// <summary>
		/// Fires when progress has changed
		/// </summary>
		public event ProgressChangedHandler ProgressChanged;

		protected void RaiseProgressChanged()
			=> this.ProgressChanged?.Invoke();

		/// <summary>
		///  Progress text to display
		/// </summary>
		public abstract string ProgressText { get; }

		/// <summary>
		/// Initialize event handler
		/// </summary>
		/// <param name="manager"><see cref="TrackManager"/> to register handler</param>
		public abstract void RegisterEvent(TrackManager manager);

		/// <summary>
		/// Reset quest tracking data
		/// </summary>
		public abstract void ResetQuest();

		/// <summary>
		/// Progress as percentage (0~100)
		/// </summary>
		/// <returns></returns>
		public abstract int GetProgress();

		/// <summary>
		/// Serialize tracking data to store
		/// </summary>
		/// <returns></returns>
		public abstract string SerializeData();

		/// <summary>
		/// Deserialize from stored serialized data
		/// </summary>
		/// <param name="data"></param>
		public abstract void DeserializeData(string data);

		/// <summary>
		/// Returns value datas to sync with KcaQSync
		/// </summary>
		/// <returns></returns>
		public abstract int[] GetRawDatas();

		/// <summary>
		/// Set value datas to sync with KcaQSync
		/// </summary>
		/// <param name="data"></param>
		public abstract void SetRawDatas(int[] data);
	}
}
