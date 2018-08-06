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
	internal class F72 : DefaultTracker
	{
		public override int Id => 680;
		public override QuestType Type => QuestType.Quarterly;

		public F72()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(1500, "보크사이트"),
				new TrackingValue(4, "기관총 장비 폐기"),
				new TrackingValue(4, "전탐 장비 폐기")
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

				this.Datas[0].Set(homeport.Materials.Bauxite);
				this.Datas[1].Add(
					args.itemList.Count(x => (homeportSlotitems[x]?.Info.Type ?? SlotItemType.None) == SlotItemType.対空機銃)
				);
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
