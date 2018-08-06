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
	/// 대규모 원정작전, 발령!
	/// </summary>
	internal class D4 : DefaultTracker
	{
		public override int Id => 404;
		public override QuestType Type => QuestType.Weekly;

		public D4()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(30, "원정 성공")
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
