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
	/// 수송선단 호위를 강화하라!
	/// </summary>
	internal class D22 : DefaultTracker
	{
		public override int Id => 424;
		public override QuestType Type => QuestType.Monthly;

		public D22()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(4, "해상호위임무(5) 원정 성공")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.MissionResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;
				if (!args.IsSuccess) return;
				if (args.Name != "海上護衛任務") return;

				this.Datas[0].Add(1);
			};
		}
	}
}
