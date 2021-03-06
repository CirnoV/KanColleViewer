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
	/// 남방쥐수송을 계속 실시하라!
	/// </summary>
	internal class D11 : DefaultTracker
	{
		public override int Id => 411;
		public override QuestType Type => QuestType.Weekly;

		public D11()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(6, "도쿄급행/도쿄급행(弐) 원정 성공")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.MissionResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;
				if (!args.IsSuccess) return;
				if (!args.Name.Contains("東京急行")) return;

				this.Datas[0].Add(1);
			};
		}
	}
}
