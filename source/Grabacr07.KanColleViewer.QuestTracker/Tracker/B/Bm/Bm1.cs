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
	/// 제5전대 출격하라!
	/// </summary>
	internal class Bm1 : DefaultTracker
	{
		public override int Id => 249;
		public override QuestType Type => QuestType.Monthly;

		public Bm1()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(1, "묘코,나치,하구로 포함 함대로 2-5 보스전 S승리")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				var shipList = new int[]
				{
					62,  // 妙高
					265, // 妙高改
					319, // 妙高改二
					63,  // 那智
					266, // 那智改
					192, // 那智改二
					65,  // 羽黒
					268, // 羽黒改
					194, // 羽黒改二
				};

				if (args.MapWorldId != 2 || args.MapAreaId != 5) return; // 2-5
				if (!args.IsBoss) return; // boss
				if (args.Rank != "S") return;

				var fleet = KanColleClient.Current.Homeport.Organization.Fleets.FirstOrDefault(x => x.Value.IsInSortie).Value;
				if (fleet?.Ships.Count(x => shipList.Contains(x.Info?.Id ?? 0)) != 3) return; // 묘코, 나치, 하구로 없음

				this.Datas[0].Add(1);
			};
		}
	}
}
