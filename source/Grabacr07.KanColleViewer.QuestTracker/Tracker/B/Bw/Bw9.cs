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
	/// 남방해역 산호제도 앞바다의 제공권을 쥐어라!
	/// </summary>
	internal class Bw9 : DefaultTracker
	{
		public override int Id => 243;
		public override QuestType Type => QuestType.Weekly;

		public Bw9()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(2, "5-2 보스전 S승리")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				if (args.MapWorldId != 5 || args.MapAreaId != 2) return; // 5-2
				if (!args.IsBoss) return; // boss
				if (args.Rank != "S") return;

				this.Datas[0].Add(1);
			};
		}
	}
}
