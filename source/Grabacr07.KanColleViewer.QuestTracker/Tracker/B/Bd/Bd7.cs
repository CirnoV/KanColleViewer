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
	/// 남서제도해역 제해권 획득
	/// </summary>
	internal class Bd7 : DefaultTracker
	{
		public override int Id => 226;
		public override QuestType Type => QuestType.Daily;

		public Bd7()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(5, "2-1 ~ 2-5 보스전 승리")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				if (args.MapWorldId != 2) return; // 2 해역
				if (!args.IsBoss) return; // 보스 노드
				if (!"SAB".Contains(args.Rank)) return; // 승리 랭크

				this.Datas[0].Add(1);
			};
		}
	}
}
