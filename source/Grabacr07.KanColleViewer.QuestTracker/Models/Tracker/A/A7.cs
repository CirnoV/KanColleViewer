using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleViewer.QuestTracker.Models.Extensions;

namespace Grabacr07.KanColleViewer.QuestTracker.Models.Tracker
{
	/// <summary>
	/// 중순전대를 편성하라!
	/// </summary>
	internal class A7 : NoSerializeOverUnderTracker, ITracker
	{
		private readonly int max_count = 2;
		private int count;

		public event EventHandler ProcessChanged;

		int ITracker.Id => 106;
		public QuestType Type => QuestType.OneTime;
		public bool IsTracking { get; set; }

		private System.EventArgs emptyEventArgs = new System.EventArgs();

		public void RegisterEvent(TrackManager manager)
		{
			manager.HenseiEvent += (sender, args) =>
			{
				if (!IsTracking) return;
				count = 0;

				var homeport = KanColleClient.Current.Homeport;
				foreach (var fleet in homeport.Organization.Fleets)
				{
					var ships = fleet.Value.Ships;

					count = Math.Max(
						count,
						ships.Count(x => x.Info.ShipType.Id == 5).Max(2)
					);
				}

				ProcessChanged?.Invoke(this, emptyEventArgs);
			};
		}

		public void ResetQuest()
		{
			count = 0;
			ProcessChanged?.Invoke(this, emptyEventArgs);
		}

		public int GetProgress()
			=> count * 100 / max_count;

		public string ProgressText => count >= max_count
				? "완료"
				: "중순 " + count.ToString() + " / " + max_count.ToString() + " 척 편성";

		public int[] GetRawDatas() => new int[] { this.count };
		public void SetRawDatas(int[] data) => this.count.Min(0).Max(this.max_count);
	}
}
