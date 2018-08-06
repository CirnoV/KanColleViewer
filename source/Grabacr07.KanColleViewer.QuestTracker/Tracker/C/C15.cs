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
	/// 보급함「이라코」돕기
	/// </summary>
	internal class C16 : DefaultTracker
	{
		public override int Id => 318;
		public override QuestType Type => QuestType.Monthly;

		public C16()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(20, "경순 2척 포함 함대로 연습전 승리"),
				new TrackingValue(2, "기함 전투식량 장비")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.PracticeResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;
				if (!args.IsSuccess) return;

				var fleet = KanColleClient.Current.Homeport.Organization.Fleets.FirstOrDefault(x => x.Value.IsInSortie).Value;
				if (fleet?.Ships.Count(x => x.Info.ShipType.Id == 3) < 2) return; // 경순양함 2척 미만

				this.Datas[0].Add(1);
			};
			manager.EquipEvent += (sender, args) =>
			{
				var flagship = KanColleClient.Current.Homeport.Organization.Fleets[1].Ships.FirstOrDefault();
				var count = flagship.Slots.Where(x => x != null && x.Equipped).Count(x => x.Item.Info.Id == 145);
				count += flagship.ExSlotExists && flagship.ExSlot.Equipped && flagship.ExSlot.Item.Info.Id == 145
					? 1 : 0;

				this.Datas[1].Set(count);
			};
		}
	}
}
