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
	/// 수상반격부대 돌입하라!
	/// </summary>
	internal class Bm7 : DefaultTracker
	{
		public override int Id => 266;
		public override QuestType Type => QuestType.Monthly;

		public Bm7()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(1, "구축 기함, 중순 1척, 경순 1척, 구축 4척 함대로 2-5 보스전 S승리")
			};
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				if (args.MapWorldId != 2 || args.MapAreaId != 5) return; // 2-5
				if (!args.IsBoss) return; // boss
				if (args.Rank != "S") return;

				var fleet = KanColleClient.Current.Homeport.Organization.Fleets.FirstOrDefault(x => x.Value.IsInSortie).Value;

				if (fleet?.Ships[0]?.Info.ShipType.Id != 2) return; // 기함 구축함 이외
				if (fleet?.Ships.Count(x => x.Info.ShipType.Id == 2) != 4) return; // 구축함 4척 이외
				if (fleet?.Ships.Count(x => x.Info.ShipType.Id == 3) != 1) return; // 경순양함 1척 이외
				if (fleet?.Ships.Count(x => x.Info.ShipType.Id == 5) != 1) return; // 중순양함 1척 이외

				this.Datas[0].Add(1);
			};
		}
	}
}
