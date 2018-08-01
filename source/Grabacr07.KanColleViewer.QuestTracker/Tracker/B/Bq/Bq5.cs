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
	/// 북방해역 경계를 실시하라!
	/// </summary>
	internal class Bq5 : DefaultTracker
	{
		public override int Id => 873;
		public override QuestType Type => QuestType.Quarterly;

		public Bq5()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(1, "3-1 보스전 A,S승리"),
				new TrackingValue(1, "3-2 보스전 A,S승리"),
				new TrackingValue(1, "3-3 보스전 A,S승리")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				var fleet = KanColleClient.Current.Homeport.Organization.Fleets.FirstOrDefault(x => x.Value.IsInSortie).Value;
				var ships = fleet?.Ships;
				if (ships.Count(x => x.Info.ShipType.Id == 3) < 1) return; // 경순양함

				if (!args.IsBoss) return; // boss
				if (!"SA".Contains(args.Rank)) return;

				if (args.MapWorldId == 3 && args.MapAreaId == 1)
					this.Datas[0].Add(1);

				else if (args.MapWorldId == 3 && args.MapAreaId == 2)
					this.Datas[1].Add(1);

				else if (args.MapWorldId == 3 && args.MapAreaId == 3)
					this.Datas[2].Add(1);
			};
		}
	}
}
