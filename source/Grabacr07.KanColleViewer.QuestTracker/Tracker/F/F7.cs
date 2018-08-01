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
	/// 장비 개발 집중강화
	/// </summary>
	internal class F7 : DefaultTracker
	{
		public override int Id => 607;
		public override QuestType Type => QuestType.Daily;

		public F7()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(3, "장비 개발")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.CreateItemEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				this.Datas[0].Add(1);
			};
		}
	}
}
