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
	/// 군축조약 대응
	/// </summary>
	internal class F9 : DefaultTracker
	{
		public override int Id => 609;
		public override QuestType Type => QuestType.Daily;

		public F9()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(2, "칸무스 해체")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.DestoryShipEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				this.Datas[0].Add(args.shipList.Length);
			};
		}
	}
}
