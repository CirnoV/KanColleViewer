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
	/// 대공기관총 양산
	/// </summary>
	internal class F34 : DefaultTracker
	{
		public override int Id => 638;
		public override QuestType Type => QuestType.Weekly;

		public F34()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(6, "기관총 계열 장비 폐기")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.DestoryItemEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				var master = KanColleClient.Current.Master.SlotItems;
				var homeportSlotitems = manager.slotitemTracker.SlotItems;

				this.Datas[0].Add(
					args.itemList.Count(x => (homeportSlotitems[x]?.Info.Type ?? SlotItemType.None) == SlotItemType.対空機銃)
				);
			};
		}
	}
}
