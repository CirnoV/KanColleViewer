using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleViewer.QuestTracker.Extensions;

namespace Grabacr07.KanColleViewer.QuestTracker.Models.Tracker
{
	/// <summary>
	/// 정예 「31구축대」, 철저해역에 돌입하라!
	/// </summary>
	internal class Bq6 : DefaultTracker
	{
		public override int Id => 875;
		public override QuestType Type => QuestType.Quarterly;

		public Bq6()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(2, "나가나미改2 기함, [타카나미改, 오키나미改, 아사시모改] 중 1척 포함 함대로 5-4 보스전 S승리")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
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

				this.Datas[0].Add(1);
			};
		}
	}
}
