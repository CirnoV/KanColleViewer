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
	/// 운용 장비의 통합 정비
	/// </summary>
	internal class F67 : DefaultTracker
	{
		public override int Id => 675;
		public override QuestType Type => QuestType.Quarterly;

		public F67()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(800, "보크사이트"),
				new TrackingValue(6, "함상전투기 장비 폐기"),
				new TrackingValue(4, "기관총 장비 폐기")
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
					args.itemList.Count(x => (homeportSlotitems[x]?.Info.Type ?? SlotItemType.None) == SlotItemType.艦上戦闘機)
				);
				this.Datas[2].Add(
					args.itemList.Count(x => (homeportSlotitems[x]?.Info.Type ?? SlotItemType.None) == SlotItemType.対空機銃)
				);
			};
		}
	}
}
