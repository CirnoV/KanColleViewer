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
	/// 주력 함상전투기의 갱신
	/// </summary>
	internal class F70 : DefaultTracker
	{
		public override int Id => 678;
		public override QuestType Type => QuestType.Quarterly;

		public F70()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(4000, "보크사이트"),
				new TrackingValue(2, "기함에 영식함전52형 장비"),
				new TrackingValue(5, "영식함전21형 장비 폐기"),
				new TrackingValue(3, "96식함전 장비 폐기")
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

				var fleet = KanColleClient.Current.Homeport.Organization.Fleets[1];
				var slotitems = fleet?.Ships[0]?.Slots;

				this.Datas[0].Set(homeport.Materials.Bauxite);
				this.Datas[1].Set(slotitems.Count(x => x.Item.Info.Id == 21));

				this.Datas[2].Add(
					args.itemList.Count(x => homeportSlotitems[x]?.Info.Id == 20)
				);
				this.Datas[3].Add(
					args.itemList.Count(x => homeportSlotitems[x]?.Info.Id == 19)
				);
			};
		}
	}
}
