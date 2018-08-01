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
	/// 근대화개수를 진행하여 군비를 갖춰라!
	/// </summary>
	internal class G3 : DefaultTracker
	{
		public override int Id => 703;
		public override QuestType Type => QuestType.Weekly;

		public G3()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(15, "근대화개수 성공")
			};
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.PowerUpEvent += (sender, args) =>
			{
				if (!IsTracking) return;
				if (!args.IsSuccess) return;

				this.Datas[0].Add(1);
			};
		}
	}
}
