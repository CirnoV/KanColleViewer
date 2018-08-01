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
	/// 함대 대정비
	/// </summary>
	internal class E3 : DefaultTracker
	{
		public override int Id => 503;
		public override QuestType Type => QuestType.Daily;

		public E3()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(5, "입거(수리)")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.RepairStartEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				this.Datas[0].Add(1);
			};
		}
	}
}
