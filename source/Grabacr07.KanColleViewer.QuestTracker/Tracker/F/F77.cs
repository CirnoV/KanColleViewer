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
	/// 대공 병장의 정비 확충
	/// </summary>
	internal class F77 : DefaultTracker
	{
		public override int Id => 686;
		public override QuestType Type => QuestType.Quarterly;

		public F77()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(30, "개발자재"),
				new TrackingValue(900, "강재"),
				new TrackingValue(1, "신형포공병장자재 소지"),
				new TrackingValue(1, "후부키급 기함 개수max 12.7cm 연장포A 改2 1슬롯 장비"),
				new TrackingValue(4, "10cm 연장고각포 장비 폐기"),
				new TrackingValue(1, "94식 고사장치 장비 폐기"),
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.DestoryItemEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				var homeport = KanColleClient.Current.Homeport;
				var master = KanColleClient.Current.Master.SlotItems;
				var homeportSlotitems = manager.slotitemTracker.SlotItems;

				var useitems = KanColleClient.Current.Homeport.Itemyard.UseItems;

				var fleet = KanColleClient.Current.Homeport.Organization.Fleets[1];
				var slotitems = fleet?.Ships[0]?.Slots;

				this.Datas[0].Set(homeport.Materials.DevelopmentMaterials);
				this.Datas[1].Set(homeport.Materials.Bauxite);

				this.Datas[2].Set(useitems.Any(x => x.Value.Id == 75) ? useitems.First(x=>x.Value.Id==75).Value.Count : 0);
				this.Datas[3].Set(slotitems[0].Item.Info.Id == 294 && slotitems[0].Item.Level==10 ? 1 : 0);

				this.Datas[4].Add(
					args.itemList.Count(x => homeportSlotitems[x]?.Info.Id == 3)
				);
				this.Datas[5].Add(
					args.itemList.Count(x => homeportSlotitems[x]?.Info.Id == 121)
				);
			};
		}
	}
}
