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
	/// "연습"으로 다른 제독 압도!
	/// </summary>
	internal class C3 : DefaultTracker
	{
		public override int Id => 304;
		public override QuestType Type => QuestType.Daily;

		public C3()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(5, "연습전 승리")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.PracticeResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;
				if (!args.IsSuccess) return;

				this.Datas[0].Add(1);
			};
		}
	}
}
