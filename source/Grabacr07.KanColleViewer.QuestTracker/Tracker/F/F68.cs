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
	/// 장비 개발력의 집중 정비
	/// </summary>
	internal class F68 : DefaultTracker
	{
		public override int Id => 676;
		public override QuestType Type => QuestType.Weekly;

		public F68()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(2400, "강재"),
				new TrackingValue(3, "중구경주포 장비 폐기"),
				new TrackingValue(3, "부포 장비 폐기"),
				new TrackingValue(1, "드럼통 장비 폐기")
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
					args.itemList.Count(x => (homeportSlotitems[x]?.Info.Type ?? SlotItemType.None) == SlotItemType.中口径主砲)
				);
				this.Datas[2].Add(
					args.itemList.Count(x => (homeportSlotitems[x]?.Info.Type ?? SlotItemType.None) == SlotItemType.司令部施設)
				);
				this.Datas[3].Add(
					args.itemList.Count(x => homeportSlotitems[x]?.Info.Id == 75)
				);
			};
		}
	}
}
