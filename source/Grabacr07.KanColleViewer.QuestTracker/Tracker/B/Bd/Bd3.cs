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
	/// 적 함대 10회 요격
	/// </summary>
	internal class Bd3 : DefaultTracker
	{
		public override int Id => 210;
		public override QuestType Type => QuestType.Daily;

		public Bd3()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(10, "전투")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				this.Datas[0].Add(1);
			};
		}
	}
}
