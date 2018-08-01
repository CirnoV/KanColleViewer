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
	/// 해상호위전
	/// </summary>
	internal class Bw5 : DefaultTracker
	{
		public override int Id => 228;
		public override QuestType Type => QuestType.Weekly;

		public Bw5()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(15, "잠수함 격침")
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
						.Where(x => x.Source.ShipType == 13 || x.Source.ShipType == 14)
						.Where(x => x.Source.MaxHP != int.MaxValue && x.Source.NowHP <= 0)
						.Count()
				);
			};
		}
	}
}
