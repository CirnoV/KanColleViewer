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
	/// 신편성「미카와 함대」, 철저해협으로 돌입하라!
	/// </summary>
	internal class Bq7 : DefaultTracker
	{
		public override int Id => 888;
		public override QuestType Type => QuestType.Quarterly;

		public Bq7()
		{
			this.Datas = new TrackingValue[]
			{
				new TrackingValue(1, "[쵸카이, 아오바, 키누가사, 카코, 후루타카, 텐류, 유바리] 중 4척 포함 함대로 5-1 보스전 S승리"),
				new TrackingValue(1, "5-3 보스전 S승리"),
				new TrackingValue(1, "5-4 보스전 S승리")
			};
			this.Attach();
		}

		public override void RegisterEvent(TrackManager manager)
		{
			manager.BattleResultEvent += (sender, args) =>
			{
				if (!IsTracking) return;

				var shipList = new int[]
				{
					69, // 鳥海
					272, // 鳥海改
					427, // 鳥海改二
					61, // 青葉
					264, // 青葉改
					123, // 衣笠
					295, // 衣笠改
					142, // 衣笠改二
					60, // 加古
					263, // 加古改
					417, // 加古改二
					59, // 古鷹
					262, // 古鷹改
					416, // 古鷹改二
					51, // 天龍
					213, // 天龍改
					477, // 天龍改二
					115, // 夕張
					293, // 夕張改
				};

				var fleet = KanColleClient.Current.Homeport.Organization.Fleets.FirstOrDefault(x => x.Value.IsInSortie).Value;
				if (fleet?.Ships.Count(x => shipList.Contains(x.Info?.Id ?? 0)) < 4) return; // 함선 포함 안됨

				if (!args.IsBoss) return;
				if (args.Rank != "S") return;

				if (args.MapWorldId == 5 && args.MapAreaId == 1) // 5-1
					this.Datas[0].Add(1);

				else if (args.MapWorldId == 5 && args.MapAreaId == 3) // 5-3
					this.Datas[1].Add(1);

				else if (args.MapWorldId == 5 && args.MapAreaId == 4) // 5-4
					this.Datas[2].Add(1);
			};
		}
	}
}
