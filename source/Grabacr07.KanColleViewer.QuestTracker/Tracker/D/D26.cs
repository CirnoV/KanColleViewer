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
	/// 근해에 침입하는 적 잠수함을 제압하라!
	/// </summary>
	internal class D26 : DefaultTracker
	{
		public override int Id => 428;
		public override QuestType Type => QuestType.Quarterly;

		public D26()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(1, "대잠경계임무(4) 원정 성공"),
				new TrackingValue(1, "해협경비행동(A2) 원정 성공"),
				new TrackingValue(1, "장시간대잠경계(A3) 원정 성공"),
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.MissionResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;
				if (!args.IsSuccess) return;

				if (args.Name == "対潜警戒任務")
					this.Datas[0].Add(1);

				else if (args.Name == "海峡警備行動")
					this.Datas[1].Add(1);

				else if (args.Name == "長時間対潜警戒")
					this.Datas[2].Add(1);
			};
		}
	}
}
