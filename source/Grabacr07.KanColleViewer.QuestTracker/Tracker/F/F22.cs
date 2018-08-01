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
	/// 정예 함전대의 신 편성 (영전21형 숙련, 월간)
	/// </summary>
	internal class F22 : DefaultTracker
	{
		public override int Id => 626;
		public override QuestType Type => QuestType.Monthly;

		public F22()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(2, "호쇼 기함, 숙련max 영식함전21형 장비하고 영식함전21형 폐기"),
				new TrackingValue(1, "96식함전 폐기")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.DestoryItemEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				var flagshipTable = new int[]
				{
					89,  // 鳳翔
					285, // 鳳翔改
				};

				var fleet = KanColleClient.Current.Homeport.Organization.Fleets[1];
				if (!flagshipTable.Any(x => x == (fleet?.Ships[0]?.Info.Id ?? 0))) return; // 호쇼 비서함

				var slotitems = fleet?.Ships[0]?.Slots;
				if (!slotitems.Any(x => x.Item.Info.Id == 20 && x.Item.Proficiency == 7)) return; // 숙련도max 영식함전21형

				var homeportSlotitems = manager.slotitemTracker.SlotItems;
				this.Datas[0].Add(args.itemList.Count(x => (homeportSlotitems[x]?.Info.Id ?? 0) == 20)); // 영식함전21형
				this.Datas[1].Add(args.itemList.Count(x => (homeportSlotitems[x]?.Info.Id ?? 0) == 19)); // 96식함전
			};
		}
	}
}
