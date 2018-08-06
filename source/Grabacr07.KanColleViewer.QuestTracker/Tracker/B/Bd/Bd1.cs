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
	/// 적 함대 격파!
	/// </summary>
	internal class Bd1 : DefaultTracker
	{
		public override int Id => 201;
		public override QuestType Type => QuestType.Daily;

		public Bd1()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(1, "전투 승리")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;
				if (!"SAB".Contains(args.Rank)) return; // 승리 랭크

				this.Datas[0].Add(1);
			};
		}
	}
}
