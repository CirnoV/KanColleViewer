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
	/// 항모기동부대 서쪽으로!
	/// </summary>
	internal class Bm6 : DefaultTracker
	{
		public override int Id => 264;
		public override QuestType Type => QuestType.Monthly;

		public Bm6()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(1, "구축 2척, 공모 2척 포함 함대로 4-2 보스전 S승리")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				if (args.MapWorldId != 4 || args.MapAreaId != 2) return; // 5 해역
				if (!args.IsBoss) return;
				if (args.Rank != "S") return;

				var fleet = KanColleClient.Current.Homeport.Organization.Fleets.FirstOrDefault(x => x.Value.IsInSortie).Value;

				if (fleet?.Ships.Count(x => x.Info.ShipType.Id == 2) < 2) return; // 구축함 2척 미만
				if (fleet?.Ships.Count(x => new int[] { 7, 11, 18 }.Contains(x.Info.ShipType.Id)) < 2) return; // 공모 2척 미만

				this.Datas[0].Add(1);
			};
		}
	}
}
