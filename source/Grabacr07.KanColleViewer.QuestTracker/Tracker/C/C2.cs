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
	/// "연습"으로 훈련도 향상!
	/// </summary>
	internal class C2 : DefaultTracker
	{
		public override int Id => 303;
		public override QuestType Type => QuestType.Daily;

		public C2()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(3, "연습전")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.PracticeResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				this.Datas[0].Add(1);
			};
		}
	}
}
