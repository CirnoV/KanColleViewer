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
	/// 주력 육공의 조달
	/// </summary>
	internal class F39 : DefaultTracker
	{
		public override int Id => 643;
		public override QuestType Type => QuestType.Other; // 계절

		public F39()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(2, "96식육공 소지"),
				new TrackingValue(1, "97식함공 소지"),
				new TrackingValue(2,"영식함전21형 폐기")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.DestoryItemEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				var homeport = KanColleClient.Current.Homeport;
				var slotitems = homeport.Itemyard.SlotItems.Select(x => x.Value).ToArray();
				slotitems = slotitems.Where(x => homeport.Organization.Ships.Any(y => !y.Value.Slots.Select(z => z.Item.Id).Contains(x.Id)))
					.Where(x => x.RawData.api_locked == 0).ToArray(); // 장비중이지 않고 잠기지 않은 장비들

				this.Datas[0].Set(slotitems.Count(x => x.Info.Id == 168)); // 96식 육공
				this.Datas[1].Set(slotitems.Count(x => x.Info.Id == 16)); // 97식 함공
				this.Datas[2].Add(args.itemList.Count(x => x == 20)); // 영식함전 21형
			};
			manager.EquipEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				var homeport = KanColleClient.Current.Homeport;
				var slotitems = homeport.Itemyard.SlotItems.Select(x => x.Value).ToArray();
				slotitems = slotitems.Where(x => homeport.Organization.Ships.Any(y => !y.Value.Slots.Select(z => z.Item.Id).Contains(x.Id)))
					.Where(x => x.RawData.api_locked == 0).ToArray(); // 장비중이지 않고 잠기지 않은 장비들

				this.Datas[0].Set(slotitems.Count(x => x.Info.Id == 168)); // 96식 육공
				this.Datas[1].Set(slotitems.Count(x => x.Info.Id == 16)); // 97식 함공
			};
		}
	}
}
