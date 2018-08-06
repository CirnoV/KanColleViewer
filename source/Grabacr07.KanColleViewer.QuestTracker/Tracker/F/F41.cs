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
	/// 해상보급 물자의 조달
	/// </summary>
	internal class F41 : DefaultTracker
	{
		public override int Id => 645;
		public override QuestType Type => QuestType.Monthly;

		public F41()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(750, "연료"),
				new TrackingValue(750, "탄약"),
				new TrackingValue(2, "드럼통 소지"),
				new TrackingValue(1, "91식 철갑탄 소지"),
				new TrackingValue(1, "삼식탄 폐기")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.EquipEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				var homeport = KanColleClient.Current.Homeport;
				var slotitems = homeport.Itemyard.SlotItems.Select(x => x.Value).ToArray();
				slotitems = slotitems.Where(x => homeport.Organization.Ships.Any(y => !y.Value.Slots.Select(z => z.Item.Id).Contains(x.Id)))
					.Where(x => x.RawData.api_locked == 0).ToArray(); // 장비중이지 않고 잠기지 않은 장비들

				this.Datas[0].Set(homeport.Materials.Fuel);
				this.Datas[1].Set(homeport.Materials.Ammunition);
				this.Datas[2].Set(slotitems.Count(x => x.Info.Id == 75)); // 드럼통
				this.Datas[3].Set(slotitems.Count(x => x.Info.Id == 36)); // 91식 철갑탄
			};
			manager.DestoryItemEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				var homeport = KanColleClient.Current.Homeport;
				var slotitems = homeport.Itemyard.SlotItems.Select(x => x.Value).ToArray();
				slotitems = slotitems.Where(x => homeport.Organization.Ships.Any(y => !y.Value.Slots.Select(z => z.Item.Id).Contains(x.Id)))
					.Where(x => x.RawData.api_locked == 0).ToArray(); // 장비중이지 않고 잠기지 않은 장비들

				this.Datas[0].Set(homeport.Materials.Fuel);
				this.Datas[1].Set(homeport.Materials.Ammunition);
				this.Datas[2].Set(slotitems.Count(x => x.Info.Id == 75)); // 드럼통
				this.Datas[3].Set(slotitems.Count(x => x.Info.Id == 36)); // 91식 철갑탄

				var master = KanColleClient.Current.Master.SlotItems;
				var homeportSlotitems = manager.slotitemTracker.SlotItems;
				this.Datas[4].Add( // 삼식탄
					args.itemList.Count(x => homeportSlotitems[x]?.Info.Id == 35)
				);
			};
		}
	}
}
