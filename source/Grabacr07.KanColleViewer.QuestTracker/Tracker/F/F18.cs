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
	/// 장비의 개수강화
	/// </summary>
	internal class F18 : DefaultTracker
	{
		public override int Id => 619;
		public override QuestType Type => QuestType.Daily;

		public F18()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(1, "장비 개수 강화")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.ReModelEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				this.Datas[0].Add(1);
			};
		}
	}
}
