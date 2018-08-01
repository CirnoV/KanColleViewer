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
	/// 원정을 3회 성공시켜라!
	/// </summary>
	internal class D2 : DefaultTracker
	{
		public override int Id => 402;
		public override QuestType Type => QuestType.Daily;

		public D2()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(3, "원정 성공")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.MissionResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;
				if (!args.IsSuccess) return;

				this.Datas[0].Add(1);
			};
		}
	}
}
