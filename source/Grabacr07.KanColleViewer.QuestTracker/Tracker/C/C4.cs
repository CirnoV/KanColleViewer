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
	/// 대규모연습
	/// </summary>
	internal class C4 : DefaultTracker
	{
		public override int Id => 302;
		public override QuestType Type => QuestType.Weekly;

		public C4()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(20, "연습전 승리")
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
