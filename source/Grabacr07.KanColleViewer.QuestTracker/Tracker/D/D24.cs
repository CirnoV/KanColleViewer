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
	/// 해상통상항로의 경계를 엄격히 하라!
	/// </summary>
	internal class D24 : DefaultTracker
	{
		public override int Id => 426;
		public override QuestType Type => QuestType.Quarterly;

		public D24()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(1, "경비임무(3) 원정 성공"),
				new TrackingValue(1, "대잠경계임무(4) 원정 성공"),
				new TrackingValue(1, "해상호위임무(5) 원정 성공"),
				new TrackingValue(1, "강행정찰임무(10) 원정 성공"),
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.MissionResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;
				if (!args.IsSuccess) return;

				if (args.Name == "警備任務")
					this.Datas[0].Add(1);

				else if (args.Name == "対潜警戒任務")
					this.Datas[1].Add(1);

				else if (args.Name == "海上護衛任務")
					this.Datas[2].Add(1);

				else if (args.Name == "強行偵察任務")
					this.Datas[3].Add(1);
			};
		}
	}
}
