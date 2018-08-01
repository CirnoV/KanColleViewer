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
	/// 신 조함 건조 지령
	/// </summary>
	internal class F6 : DefaultTracker
	{
		public override int Id => 606;
		public override QuestType Type => QuestType.Daily;

		public F6()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(1, "칸무스 건조")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.CreateShipEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				this.Datas[0].Add(1);
			};
		}
	}
}
