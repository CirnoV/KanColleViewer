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
	/// 전선의 항공정찰을 실시하라!
	/// </summary>
	internal class Bq4 : DefaultTracker
	{
		public override int Id => 862;
		public override QuestType Type => QuestType.Quarterly;

		public Bq4()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(2, "수모 1척, 경순 3척 포함 함대로 6-3 보스전 A,S승리")
			};
			this.Attach();
		}
		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				if (args.MapWorldId != 6 || args.MapAreaId != 3) return; // 6-3
				if (!args.IsBoss) return;
				if (!"SA".Contains(args.Rank)) return;

				var fleet = KanColleClient.Current.Homeport.Organization.Fleets.FirstOrDefault(x => x.Value.IsInSortie).Value;
				var ships = fleet?.Ships;

				if (ships.Count(x => x.Info.ShipType.Id == 16) < 1) return; // 수상기모함
				if (ships.Count(x => x.Info.ShipType.Id == 3) < 2) return; // 경순양함

				this.Datas[0].Add(1);
			};
		}
	}
}
