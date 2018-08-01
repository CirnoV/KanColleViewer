using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleViewer.QuestTracker.Models.Extensions;
using Grabacr07.KanColleViewer.QuestTracker.Models.Model;

namespace Grabacr07.KanColleViewer.QuestTracker.Models.Tracker
{
	/// <summary>
	/// 정예 「31구축대」, 철저해역에 돌입하라!
	/// </summary>
	internal class Bq6 : TrackerBase
	{
		private QuestProgressType lastProgress = QuestProgressType.None;
		private readonly int max_count = 2;
		private int count;

		public event EventHandler ProcessChanged;

		int TrackerBase.Id => 875;
		public QuestType Type => QuestType.Weekly;
		public bool IsTracking { get; set; }

		private System.EventArgs emptyEventArgs = new System.EventArgs();

		public void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				if (args.MapWorldId != 5 || args.MapAreaId != 4) return; // 5-4
				if (!args.IsBoss) return;
				if (args.Rank != "S") return;

				var fleet = KanColleClient.Current.Homeport.Organization.Fleets.FirstOrDefault(x => x.Value.IsInSortie).Value;
				var ships = fleet?.Ships;
				var shipIds = ships.Select(x => x.Info.Id);

				if (shipIds.FirstOrDefault() != 543) return; // 나가나미改2 기함
				if (!shipIds.Contains(345) && !shipIds.Contains(359) && !shipIds.Contains(344)) return; // 타카나미改, 오키나미改, 아사시모改 포함

				if (ships.Count(x => x.Info.ShipType.Id == 16) < 1) return; // 수상기모함
				if (ships.Count(x => x.Info.ShipType.Id == 3) < 2) return; // 경순양함

				count = count.Add(1).Max(max_count);

				ProcessChanged?.Invoke(this, emptyEventArgs);
			};
		}

		public void ResetQuest()
		{
			count = 0;
			ProcessChanged?.Invoke(this, emptyEventArgs);
		}

		public int GetProgress()
		{
			return count * 100 / max_count;
		}

		public string ProgressText => count >= max_count ? "완료" : "나가나미改2 기함, [타카나미改, 오키나미改, 아사시모改] 중 한 척 포함한 함대로 5-4 보스전 S승 " + count.ToString() + " / " + max_count.ToString();

		public string SerializeData()
		{
			return count.ToString();
		}

		public void DeserializeData(string data)
		{
			count = 0;
			int.TryParse(data, out count);
		}

		public void CheckOverUnder(QuestProgressType progress)
		{
			if (lastProgress == progress) return;
			lastProgress = progress;

			int cut50 = (int)Math.Ceiling(max_count * 0.5);
			int cut80 = (int)Math.Ceiling(max_count * 0.8);

			switch (progress)
			{
				case QuestProgressType.None:
					if (count >= cut50) count = cut50 - 1;
					break;
				case QuestProgressType.Progress50:
					if (count >= cut80) count = cut80 - 1;
					else if (count < cut50) count = cut50;
					break;
				case QuestProgressType.Progress80:
					if (count < cut80) count = cut80;
					break;
				case QuestProgressType.Complete:
					count = max_count;
					break;
			}
			ProcessChanged?.Invoke(this, emptyEventArgs);
		}
	}
}
