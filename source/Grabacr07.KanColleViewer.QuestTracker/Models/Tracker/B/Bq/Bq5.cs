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
	/// 북방해역 경계를 실시하라!
	/// </summary>
	internal class Bq5 : NoOverUnderTracker, ITracker
	{
		private int progress_3_1;
		private int progress_3_2;
		private int progress_3_3;

		public event EventHandler ProcessChanged;

		int ITracker.Id => 873;
		public QuestType Type => QuestType.Other;
		public bool IsTracking { get; set; }

		private System.EventArgs emptyEventArgs = new System.EventArgs();

		public void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				var fleet = KanColleClient.Current.Homeport.Organization.Fleets.FirstOrDefault(x => x.Value.IsInSortie).Value;
				var ships = fleet?.Ships;
				if (ships.Count(x => x.Info.ShipType.Id == 3) < 1) return; // 경순양함

				if (args.MapWorldId == 3 && args.MapAreaId == 1)
				{
					if (!args.IsBoss) return; // boss
					if (!"SA".Contains(args.Rank)) return;
					progress_3_1 = progress_3_1.Add(1).Max(1);
				}
				else if (args.MapWorldId == 3 && args.MapAreaId == 2)
				{
					if (!args.IsBoss) return; // boss
					if (!"SA".Contains(args.Rank)) return;
					progress_3_2 = progress_3_2.Add(1).Max(1);
				}
				else if (args.MapWorldId == 3 && args.MapAreaId == 3)
				{
					if (!args.IsBoss) return; // boss
					if (!"SA".Contains(args.Rank)) return;
					progress_3_3 = progress_3_3.Add(1).Max(1);
				}

				ProcessChanged?.Invoke(this, emptyEventArgs);
			};
		}

		public void ResetQuest()
		{
			progress_3_1 = progress_3_2 = progress_3_3 = 0;
			ProcessChanged?.Invoke(this, emptyEventArgs);
		}

		public int GetProgress()
		{
			return (progress_3_1 + progress_3_2 + progress_3_3) * 100 / 3;
		}

		public string GetProgressText()
		{
			return (progress_3_1 + progress_3_2 + progress_3_3) >= 3
				? "완료"
				: "경순양함 1척 포함 함대로 3-1 보스전 A 승리 이상 " + progress_3_1.ToString() + " / 1, "
				  + "3-2 보스전 A 승리 이상 " + progress_3_2.ToString() + " / 1, "
				  + "3-3 보스전 A 승리 이상 " + progress_3_3.ToString() + " / 1";
		}

		public string SerializeData()
		{
			return progress_3_1.ToString() + ","
				+ progress_3_2.ToString() + ","
				+ progress_3_3.ToString();
		}

		public void DeserializeData(string data)
		{
			var part = data.Split(',');
			progress_3_1 = progress_3_2 = progress_3_3 = 0;

			if (part.Length != 3) return;

			int.TryParse(part[0], out progress_3_1);
			int.TryParse(part[1], out progress_3_2);
			int.TryParse(part[2], out progress_3_3);
		}
	}
}
