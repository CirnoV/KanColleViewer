using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleViewer.QuestTracker.Extensions;

namespace Grabacr07.KanColleViewer.QuestTracker.Models.Tracker
{
	/// <summary>
	/// 숙련승무원 양성
	/// </summary>
	internal class F35 : DefaultTracker
	{
		public override int Id => 637;
		public override QuestType Type => QuestType.Quarterly; // 계절

		public F35()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(1, "호쇼 기함, 숙련max 개수max 96식함전 장비"),
				new TrackingValue(2, "훈장2개 소지")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			EventHandler handler = (sender, args) =>
			{
				if (!IsTracking) return;

				var flagshipTable = new int[]
				{
					89,  // 鳳翔
					285, // 鳳翔改
				};

				this.Datas[0].Set(0);
				this.Datas[1].Set(0);

				var fleet = KanColleClient.Current.Homeport.Organization.Fleets[1];
				var items = KanColleClient.Current.Homeport.Itemyard.UseItems;

				// 호쇼 기함
				if (flagshipTable.Any(x => x == (fleet?.Ships[0]?.Info.Id ?? 0)))
				{
					var slotitems = fleet?.Ships[0]?.Slots;

					// 숙련도max, 개수max 96식함전
					Datas[0].Set(slotitems.Any(x => x.Item.Info.Id == 19 && x.Item.Level == 10 && x.Item.Proficiency == 7) ? 1 : 0);
				}

				// 훈장
				if (items.Any(x => x.Value.Id == 57))
					this.Datas[1].Set(items.First(x => x.Value.Id == 57).Value.Count);
			};
			manager.HenseiEvent += handler;
			manager.EquipEvent += handler;
		}
	}
}
