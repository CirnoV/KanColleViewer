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
	/// 전과확장임무! 「Z작전」 전단작전
	/// </summary>
	internal class Bq2 : NoOverUnderTracker, TrackerBase
	{
		private int progress_2_4;
		private int progress_6_1;
		private int progress_6_3;
		private int progress_6_4;

		public event EventHandler ProcessChanged;

		int TrackerBase.Id => 854;
		public QuestType Type => QuestType.Other;
		public bool IsTracking { get; set; }

		private System.EventArgs emptyEventArgs = new System.EventArgs();

		public void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				if (args.MapWorldId == 2 && args.MapAreaId == 4)
				{
					if (!args.IsBoss) return; // boss
					if (!"SA".Contains(args.Rank)) return;
					progress_2_4 = progress_2_4.Add(1).Max(1);
				}
				else if (args.MapWorldId == 6 && args.MapAreaId == 1)
				{
					if (!args.IsBoss) return; // boss
					if (!"SA".Contains(args.Rank)) return;
					progress_6_1 = progress_6_1.Add(1).Max(1);
				}
				else if (args.MapWorldId == 6 && args.MapAreaId == 3)
				{
					if (!args.IsBoss) return; // boss
					if (!"SA".Contains(args.Rank)) return;
					progress_6_3 = progress_6_3.Add(1).Max(1);
				}
				else if (args.MapWorldId == 6 && args.MapAreaId == 4)
				{
					if (!args.IsBoss) return; // boss
					if (args.Rank != "S") return;
					progress_6_4 = progress_6_4.Add(1).Max(1);
				}

				ProcessChanged?.Invoke(this, emptyEventArgs);
			};
		}

		public void ResetQuest()
		{
			progress_2_4 = progress_6_1 = progress_6_3 = progress_6_4 = 0;
			ProcessChanged?.Invoke(this, emptyEventArgs);
		}

		public int GetProgress()
		{
			return (progress_2_4 + progress_6_1 + progress_6_3 + progress_6_4) * 100 / 4;
		}

		public string ProgressText => (progress_2_4 + progress_6_1 + progress_6_3 + progress_6_4) >= 4
				? "완료"
				: "2-4 보스전 A 승리 이상 " + progress_2_4.ToString() + " / 1, "
				  + "6-1 보스전 A 승리 이상 " + progress_6_1.ToString() + " / 1, "
				  + "6-3 보스전 A 승리 이상 " + progress_6_3.ToString() + " / 1, "
				  + "6-4 보스전 S 승리 " + progress_6_4.ToString() + " / 1";

		public string SerializeData()
		{
			return progress_2_4.ToString() + ","
				+ progress_6_1.ToString() + ","
				+ progress_6_3.ToString() + ","
				+ progress_6_4.ToString();
		}

		public void DeserializeData(string data)
		{
			var part = data.Split(',');
			progress_2_4 = progress_6_1 = progress_6_3 = progress_6_4 = 0;

			if (part.Length != 4) return;

			int.TryParse(part[0], out progress_2_4);
			int.TryParse(part[1], out progress_6_1);
			int.TryParse(part[2], out progress_6_3);
			int.TryParse(part[3], out progress_6_4);
		}
	}
}
