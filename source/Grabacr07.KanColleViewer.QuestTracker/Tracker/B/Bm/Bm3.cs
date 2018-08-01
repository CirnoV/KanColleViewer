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
	/// 수뢰전대 남서쪽으로!
	/// </summary>
	internal class Bm3 : DefaultTracker
	{
		public override int Id => 257;
		public override QuestType Type => QuestType.Monthly;

		public Bm3()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(1, "경순 기함, 경순 3척 이하, 구축 1척 이상 함대로 1-4 보스전 S승리")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				if (args.MapWorldId != 1 || args.MapAreaId != 4) return; // 1-4
				if (!args.IsBoss) return; // boss
				if (args.Rank != "S") return;

				var fleet = KanColleClient.Current.Homeport.Organization.Fleets.FirstOrDefault(x => x.Value.IsInSortie).Value;

				if (fleet?.Ships[0]?.Info.ShipType.Id != 3) return; // 기함 경순양함 이외
				if (fleet?.Ships.Any(x => x.Info.ShipType.Id != 2 && x.Info.ShipType.Id != 3) ?? false) return; // 구축함, 경순양함 이외 함종
				if (fleet?.Ships.Count(x => x.Info.ShipType.Id == 3) > 3) return; // 경순양함 3척 이상
				if (fleet?.Ships.Count(x => x.Info.ShipType.Id == 2) < 1) return; // 구축함 1척 미만

				this.Datas[0].Add(1);
			};
		}
	}
}
