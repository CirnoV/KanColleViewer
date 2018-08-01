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
	/// 자원의 재활용
	/// </summary>
	internal class F12 : DefaultTracker
	{
		public override int Id => 613;
		public override QuestType Type => QuestType.Weekly;

		public F12()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(24, "장비 폐기")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.DestoryItemEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				this.Datas[0].Add(1); // 갯수가 아니라 횟수
			};
		}
	}
}
