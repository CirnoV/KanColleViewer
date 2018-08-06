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
	/// 신형 의장의 지속 연구
	/// </summary>
	internal class F55 : DefaultTracker
	{
		public override int Id => 663;
		public override QuestType Type => QuestType.Quarterly;

		public F55()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(8000, "탄약"),
				new TrackingValue(8000, "강재"),
				new TrackingValue(10, "전탐 장비 폐기")
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

				this.Datas[0].Set(homeport.Materials.Ammunition);
				this.Datas[1].Set(homeport.Materials.Steel);
				this.Datas[2].Add(
					args.itemList.Count(x =>
					{
						var y = (homeportSlotitems[x]?.Info.Type ?? SlotItemType.None);
						return y == SlotItemType.小型電探 || y == SlotItemType.大型電探;
					})
				);
			};
		}
	}
}
