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
	/// 함대 PX 축제
	/// </summary>
	internal class E4 : DefaultTracker
	{
		public override int Id => 504;
		public override QuestType Type => QuestType.Daily;

		public E4()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(15, "보급")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.ChargeEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				this.Datas[0].Add(1);
			};
		}
	}
}
