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
	/// 기종 전환 (영식 함전 52형(숙련))
	/// </summary>
	internal class F25 : DefaultTracker
	{
		public override int Id => 628;
		public override QuestType Type => QuestType.Monthly;

		public F25()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(2, "기함에 숙련max 영식함전21형(숙련) 장비하고 영식함전52형 폐기")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.DestoryItemEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				var fleet = KanColleClient.Current.Homeport.Organization.Fleets[1];
				var slotitems = fleet?.Ships[0]?.Slots;

				if (!slotitems.Any(x => x.Item.Info.Id == 96 && x.Item.Proficiency == 7)) return; // 숙련도max 영식 함전 21형(숙련)

				var homeportSlotitems = manager.slotitemTracker.SlotItems;
				this.Datas[0].Add(args.itemList.Count(x => (homeportSlotitems[x]?.Info.Id ?? 0) == 21)); // 영식 함전 52형
			};
		}
	}
}
