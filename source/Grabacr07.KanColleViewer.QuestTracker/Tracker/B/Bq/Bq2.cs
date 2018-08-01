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
	/// 전과확장임무! 「Z작전」 전단작전
	/// </summary>
	internal class Bq2 : DefaultTracker
	{
		public override int Id => 854;
		public override QuestType Type => QuestType.Quarterly;

		public Bq2()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(1, "2-4 보스전 A,S승리"),
				new TrackingValue(1, "6-1 보스전 A,S승리"),
				new TrackingValue(1, "6-3 보스전 A,S승리"),
				new TrackingValue(1, "6-4 보스전 S승리")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				if (args.MapWorldId == 2 && args.MapAreaId == 4)
				{
					if (!args.IsBoss) return; // boss
					if (!"SA".Contains(args.Rank)) return;

					this.Datas[0].Add(1);
				}
				else if (args.MapWorldId == 6 && args.MapAreaId == 1)
				{
					if (!args.IsBoss) return; // boss
					if (!"SA".Contains(args.Rank)) return;

					this.Datas[1].Add(1);
				}
				else if (args.MapWorldId == 6 && args.MapAreaId == 3)
				{
					if (!args.IsBoss) return; // boss
					if (!"SA".Contains(args.Rank)) return;

					this.Datas[2].Add(1);
				}
				else if (args.MapWorldId == 6 && args.MapAreaId == 4)
				{
					if (!args.IsBoss) return; // boss
					if (args.Rank != "S") return;

					this.Datas[3].Add(1);
				}
			};
		}
	}
}
