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
	/// 해상수송로의 안전확보에 힘쓰자!
	/// </summary>
	internal class Bw10 : DefaultTracker
	{
		public override int Id => 261;
		public override QuestType Type => QuestType.Weekly;

		public Bw10()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(3, "1-5 보스전 A,S승리")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				if (args.MapWorldId != 1 || args.MapAreaId != 5) return; // 1-5
				if (!args.IsBoss) return; // boss
				if (!"SA".Contains(args.Rank)) return;

				this.Datas[0].Add(1);
			};
		}
	}
}
