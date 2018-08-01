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
	/// 아호작전
	/// </summary>
	internal class Bw1 : DefaultTracker
	{
		public override int Id => 214;
		public override QuestType Type => QuestType.Weekly;

		public Bw1()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(36, "출격"),
				new TrackingValue(6, "S승리"),
				new TrackingValue(24, "보스전"),
				new TrackingValue(12, "보스전 승리")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				// 출격
				if (args.IsFirstCombat)
					this.Datas[0].Add(1);

				// S 승리
				if (args.Rank == "S")
					this.Datas[1].Add(1);

				// 보스전
				if (args.IsBoss)
				{
					this.Datas[2].Add(1);

					// 보스전 승리
					if ("SAB".Contains(args.Rank))
						this.Datas[3].Add(1);
				}
			};
		}
	}
}
