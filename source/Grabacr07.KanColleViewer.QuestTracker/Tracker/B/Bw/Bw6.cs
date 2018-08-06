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
	/// 적 동방함대 격멸
	/// </summary>
	internal class Bw6 : DefaultTracker
	{
		public override int Id => 229;
		public override QuestType Type => QuestType.Weekly;

		public Bw6()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(12, "4-1 ~ 4-5 보스전 승리")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				if (args.MapWorldId != 4) return; // 4 해역
				if (!args.IsBoss) return; // boss
				if (!"SAB".Contains(args.Rank)) return;

				this.Datas[0].Add(1);
			};
		}
	}
}
