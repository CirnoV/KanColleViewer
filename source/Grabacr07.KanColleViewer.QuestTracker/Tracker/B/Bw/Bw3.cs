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
	/// 해상통상파괴작전
	/// </summary>
	internal class Bw3 : DefaultTracker
	{
		public override int Id => 213;
		public override QuestType Type => QuestType.Weekly;

		public Bw3()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(20, "보급함 격침")
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
