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
	public interface ITracker
	{
		/// <summary>
		/// Is this quest tracking?
		/// </summary>
		bool IsTracking { get; set; }

		/// <summary>
		/// Quest Id
		/// </summary>
		int Id { get; }

		/// <summary>
		/// Quest Type (Daily, Weekly, ...)
		/// </summary>
		QuestType Type { get; }

		/// <summary>
		/// Tracking Value Datas
		/// </summary>
		TrackingValue[] Datas { get; }

		/// <summary>
		/// Fires when progress has changed
		/// </summary>
		event EventHandler ProgressChanged;

		/// <summary>
		///  Progress text to display
		/// </summary>
		string ProgressText { get; }

		/// <summary>
		/// Initialize event handler
		/// </summary>
		/// <param name="manager"><see cref="TrackManager"/> to register handler</param>
		void RegisterEvent(TrackManager manager);

		/// <summary>
		/// Reset quest tracking data
		/// </summary>
		void ResetQuest();

		/// <summary>
		/// Progress as percentage (0~100)
		/// </summary>
		/// <returns></returns>
		int GetProgress();

		/// <summary>
		/// Serialize tracking data to store
		/// </summary>
		/// <returns></returns>
		string SerializeData();

		/// <summary>
		/// Deserialize from stored serialized data
		/// </summary>
		/// <param name="data"></param>
		void DeserializeData(string data);

		/// <summary>
		/// Returns value datas to sync with KcaQSync
		/// </summary>
		/// <returns></returns>
		int[] GetRawDatas();

		/// <summary>
		/// Set value datas to sync with KcaQSync
		/// </summary>
		/// <param name="data"></param>
		void SetRawDatas(int[] data);
	}
}
