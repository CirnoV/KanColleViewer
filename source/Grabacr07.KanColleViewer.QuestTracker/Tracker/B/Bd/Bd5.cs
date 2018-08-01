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
	/// 적 보급함 3척 격침
	/// </summary>
	internal class Bd5 : DefaultTracker
	{
		public override int Id => 218;
		public override QuestType Type => QuestType.Daily;

		public Bd5()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(3, "보급함 격침")
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
						.Where(x => x.Source.ShipType == 15)
						.Where(x => x.Source.MaxHP != int.MaxValue && x.Source.NowHP <= 0)
						.Count()
				);
			};
		}
	}
}
