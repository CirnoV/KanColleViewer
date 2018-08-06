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
	/// 함선의 근대화개수를 실시하자!
	/// </summary>
	internal class G2 : DefaultTracker
	{
		public override int Id => 702;
		public override QuestType Type => QuestType.Daily;

		public G2()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(2, "근대화개수 성공")
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
