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
	/// 계전 지원 능력의 정비
	/// </summary>
	internal class F69 : DefaultTracker
	{
		public override int Id => 677;
		public override QuestType Type => QuestType.Weekly;

		public F69()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(3600, "강재"),
				new TrackingValue(4, "대구경주포 장비 폐기"),
				new TrackingValue(2, "수상정찰기 장비 폐기"),
				new TrackingValue(3, "어뢰 장비 폐기")
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

				this.Datas[0].Set(homeport.Materials.Steel);
				this.Datas[1].Add(
					args.itemList.Count(x => (homeportSlotitems[x]?.Info.Type ?? SlotItemType.None) == SlotItemType.大口径主砲)
				);
				this.Datas[2].Add(
					args.itemList.Count(x => (homeportSlotitems[x]?.Info.Type ?? SlotItemType.None) == SlotItemType.水上偵察機)
				);
				this.Datas[3].Add(
					args.itemList.Count(x => (homeportSlotitems[x]?.Info.Type ?? SlotItemType.None) == SlotItemType.魚雷)
				);
			};
		}
	}
}
