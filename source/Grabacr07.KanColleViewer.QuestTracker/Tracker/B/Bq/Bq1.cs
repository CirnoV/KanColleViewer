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
	/// 오키노시마 해역 영격전
	/// </summary>
	internal class Bq1 : DefaultTracker
	{
		public override int Id => 822;
		public override QuestType Type => QuestType.Quarterly;

		public Bq1()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(2, "2-4 보스전 S승리")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				if (args.MapWorldId != 2 || args.MapAreaId != 4) return; // 2-4
				if (!args.IsBoss) return; // boss
				if (args.Rank != "S") return;

				this.Datas[0].Add(1);
			};
		}
	}
}
