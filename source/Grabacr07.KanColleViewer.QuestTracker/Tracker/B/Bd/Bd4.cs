using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleViewer.QuestTracker.Extensions;

namespace Grabacr07.KanColleViewer.QuestTracker.Models.Tracker
{
	/// <summary>
	/// 적 공모 3척 격침
	/// </summary>
	internal class Bd4 : DefaultTracker
	{
		public override int Id => 211;
		public override QuestType Type => QuestType.Daily;

		public Bd4()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(3, "정규공모/경공모 격침")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				this.Datas[0].Add(
					args.EnemyShips
						.Where(x => x.Source.ShipType == 7 || x.Source.ShipType == 11 || x.Source.ShipType == 18)
						.Where(x => x.Source.MaxHP != int.MaxValue && x.Source.NowHP <= 0)
						.Count()
				);
			};
		}
	}
}
