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
	/// 공창 환경의 정비
	/// </summary>
	internal class F66 : DefaultTracker
	{
		public override int Id => 674;
		public override QuestType Type => QuestType.Daily;

		public F66()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(300, "강재"),
				new TrackingValue(3, "기관총 장비 폐기")
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
					args.itemList.Count(x => (homeportSlotitems[x]?.Info.Type ?? SlotItemType.None) == SlotItemType.対空機銃)
				);
			};
		}
	}
}
