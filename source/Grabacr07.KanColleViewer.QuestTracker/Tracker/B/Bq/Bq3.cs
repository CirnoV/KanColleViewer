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
	/// 강행수송함대, 발묘!
	/// </summary>
	internal class Bq3 : DefaultTracker
	{
		public override int Id => 861;
		public override QuestType Type => QuestType.Quarterly;

		public Bq3()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(2, "항공전함/보급함 2척 포함 함대로 1-6 완주")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				if (args.MapWorldId != 1 || args.MapAreaId != 6) return; // 1-6
				if (args.MapNodeId != 14 && args.MapNodeId != 17) return; // 1-6-N node

				var fleet = KanColleClient.Current.Homeport.Organization.Fleets.FirstOrDefault(x => x.Value.IsInSortie).Value;
				var ships = fleet?.Ships;

				var require = ships.Count(x => x.Info.ShipType.Id == 10)
					+ ships.Count(x => x.Info.ShipType.Id == 22);

				if (require < 2) return; // 항공전함/보급함 수
				this.Datas[0].Add(1);
			};
		}
	}
}
