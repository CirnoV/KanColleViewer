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
	public class DefaultTracker : TrackerBase
	{
		public override int Id => 0;
		public override QuestType Type => QuestType.None;
		public override TrackingValue[] Datas { get; protected set; }

		/// <summary>
		///  Progress text to display
		/// </summary>
		public override string ProgressText =>
			this.GetProgress() == 100
				? "완료"
				: string.Join(
					", ",
					this.Datas.Select(x => string.Format("{0} {1}/{2}", x.Name, x.Current, x.Maximum)) ?? new string[0]
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

		public DefaultTracker()
		{
			this.Datas = new TrackingValue[0];
		}

		protected void Attach()
		{
			foreach (var data in this.Datas)
				data.ValueChanged += (s, e) => this.RaiseProgressChanged();
		}

		public override void RegisterEvent(TrackManager manager)
			=> throw new NotImplementedException();

		/// <summary>
		/// Reset quest tracking data
		/// </summary>
		public override void ResetQuest()
		{
			foreach (var Data in this.Datas)
				Data.Clear();
		}

		/// <summary>
		/// Progress as percentage (0~100)
		/// </summary>
		/// <returns></returns>
		public override int GetProgress()
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
		public override string SerializeData() =>
			string.Join(",", this.Datas.Select(x => x.Serialize()));

		/// <summary>
		/// Deserialize from stored serialized data
		/// </summary>
		/// <param name="data"></param>
		public override void DeserializeData(string data)
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
		public override int[] GetRawDatas()
			=> this.Datas.Select(x => x.Current).ToArray();

		/// <summary>
		/// Set value datas to sync with KcaQSync
		/// </summary>
		/// <param name="data"></param>
		public override void SetRawDatas(int[] data)
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

			this.Datas[0].Current = Current;
		}

		public override void UpdateState(QuestProgressType State)
			=> CheckOverUnder(State);
	}
}
