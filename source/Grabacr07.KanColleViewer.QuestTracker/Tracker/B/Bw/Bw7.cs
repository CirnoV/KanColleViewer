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
	/// 적 북방함대 주력 격멸
	/// </summary>
	internal class Bw7 : DefaultTracker
	{
		public override int Id => 241;
		public override QuestType Type => QuestType.Weekly;

		public Bw7()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(5, "3-3 ~ 3-5 보스전 승리")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				if (args.MapWorldId != 3 || (args.MapAreaId != 3 && args.MapAreaId != 4 && args.MapAreaId != 5)) return; // 3-3 3-4 3-5
				if (!args.IsBoss) return; // boss
				if (!"SAB".Contains(args.Rank)) return;

				this.Datas[0].Add(1);
			};
		}
	}
}
