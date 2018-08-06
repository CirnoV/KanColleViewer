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
	/// 장비 개발 능력의 정비
	/// </summary>
	internal class F65 : DefaultTracker
	{
		public override int Id => 673;
		public override QuestType Type => QuestType.Daily;

		public F65()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(4, "소구경 주포 장비 폐기")
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

				this.Datas[0].Add(
					args.itemList.Count(x =>
					{
						var y = (homeportSlotitems[x]?.Info.Type ?? SlotItemType.None);
						return y == SlotItemType.小口径主砲;
					})
				);
			};
		}
	}
}
