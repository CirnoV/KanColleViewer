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
	/// 잠수함대 출격하라!
	/// </summary>
	internal class Bm2 : DefaultTracker
	{
		public override int Id => 256;
		public override QuestType Type => QuestType.Monthly;

		public Bm2()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(3, "6-1 보스전 S승리")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				if (args.MapWorldId != 6 || args.MapAreaId != 1) return; // 6-1
				if (!args.IsBoss) return; // boss
				if (args.Rank != "S") return;

				this.Datas[0].Add(1);
			};
		}
	}
}
