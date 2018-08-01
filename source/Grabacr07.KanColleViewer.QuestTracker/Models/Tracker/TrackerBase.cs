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
	public class TrackerBase : ITracker
	{
		/// <summary>
		/// Is this quest tracking?
		/// </summary>
		public bool IsTracking { get; set; }

		/// <summary>
		/// Quest Id
		/// </summary>
		public int Id => 0;

		/// <summary>
		/// Quest Type (Daily, Weekly, ...)
		/// </summary>
		public QuestType Type => QuestType.None;

		/// <summary>
		/// Tracking Value Datas
		/// </summary>
		public TrackingValue[] Datas { get; }


		public event EventHandler ProgressChanged;

		/// <summary>
		///  Progress text to display
		/// </summary>
		public string ProgressText =>
			string.Join(
				", ",
				this.Datas.Select(x => string.Format("{0}/{1}", x.Current, x.Maximum))
			);

		/// <summary>
		/// Value using on <see cref="CheckOverUnder"/>, to check 50% progress
		/// </summary>
		protected virtual int Progress50
		{
			get
			{
				if (this.Datas.Length != 1) return 0;
				return (int)Math.Ceiling(this.Datas[0].Maximum * 0.5);
			}
		}
		/// <summary>
		/// Value using on <see cref="CheckOverUnder"/>, to check 80% progress
		/// </summary>
		protected virtual int Progress80
		{
			get
			{
				if (this.Datas.Length != 1) return 0;
				return (int)Math.Ceiling(this.Datas[0].Maximum * 0.8);
			}
		}

		private QuestProgressType lastProgress = QuestProgressType.None;

		public TrackerBase()
		{
			this.Datas = new TrackingValue[0];
		}

		/// <summary>
		/// Initialize event handler
		/// </summary>
		/// <param name="manager"><see cref="TrackManager"/> to register handler</param>
		public void RegisterEvent(TrackManager manager) { }

		/// <summary>
		/// Reset quest tracking data
		/// </summary>
		public void ResetQuest()
		{
			foreach (var Data in this.Datas)
				Data.Clear();
		}

		/// <summary>
		/// Progress as percentage (0~100)
		/// </summary>
		/// <returns></returns>
		public int GetProgress()
		{
			int Maximum = this.Datas.Sum(x => x.Maximum);
			int Value = this.Datas.Sum(x => x.Current);

			if (Maximum == 0) return 0;
			return Value * 100 / Maximum;
		}

		/// <summary>
		/// Serialize tracking data to store
		/// </summary>
		/// <returns></returns>
		public string SerializeData() =>
			string.Join(",", this.Datas.Select(x => x.Serialize()));

		/// <summary>
		/// Deserialize from stored serialized data
		/// </summary>
		/// <param name="data"></param>
		public void DeserializeData(string data)
		{
			var serialized = data.Split(new char[] { ',' });
			if (serialized.Length != this.Datas.Length)
				throw new ArgumentException("Serialized data length not matches", nameof(data));

			this.ResetQuest();
			for (var i = 0; i < serialized.Length; i++)
				this.Datas[i].Deserialize(serialized[i]);
		}

		/// <summary>
		/// Returns value datas to sync with KcaQSync
		/// </summary>
		/// <returns></returns>
		public int[] GetRawDatas() => this.Datas.Select(x => x.Current).ToArray();

		/// <summary>
		/// Set value datas to sync with KcaQSync
		/// </summary>
		/// <param name="data"></param>
		public void SetRawDatas(int[] data)
		{
			if (data.Length != this.Datas.Length)
				throw new ArgumentException("Data length not matches", nameof(data));

			this.ResetQuest();
			for (var i = 0; i < data.Length; i++)
				this.Datas[i].Current = data[i];
		}

		/// <summary>
		/// Adjust tracking value based on In-game progress
		/// </summary>
		/// <param name="progress"></param>
		private void CheckOverUnder(QuestProgressType progress)
		{
			if (this.Datas.Length != 1) return;
			if (lastProgress == progress) return;
			lastProgress = progress;

			int Current = this.Datas[0].Current;
			int Maximum = this.Datas[0].Maximum;
			int cut50 = this.Progress50, cut80 = this.Progress80;

			switch (progress)
			{
				case QuestProgressType.None:
					if (Current >= cut50) Current = cut50 - 1;
					break;
				case QuestProgressType.Progress50:
					if (Current >= cut80) Current = cut80 - 1;
					else if (Current < cut50) Current = cut50;
					break;
				case QuestProgressType.Progress80:
					if (Current < cut80) Current = cut80;
					break;
				case QuestProgressType.Complete:
					Current = Maximum;
					break;
			}
			ProgressChanged?.Invoke(this, System.EventArgs.Empty);
		}
	}
}
