using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grabacr07.KanColleViewer.QuestTracker.Models.Model;
using Grabacr07.KanColleViewer.QuestTracker.Models.Extensions;

using Grabacr07.KanColleWrapper.Models.Raw;
using ShipBattleInfo = Grabacr07.KanColleViewer.QuestTracker.Models.Model.BattleCalculator.ShipBattleInfo;

namespace Grabacr07.KanColleViewer.QuestTracker.Models.EventArgs
{
	internal class BattleResultEventArgs
	{
		public string EnemyName { get; set; }
		public ShipBattleInfo[] EnemyShips { get; set; }

		public int MapWorldId { get; set; }
		public int MapAreaId { get; set; }
		public int MapNodeId { get; set; }

		public bool IsFirstCombat { get; set; }
		public bool IsBoss { get; set; }
		public string Rank { get; set; }

		public BattleResultEventArgs(TrackerMapInfo MapInfo, ShipBattleInfo[] enemyFirstShips, ShipBattleInfo[] enemSecondShips, kcsapi_battle_result data)
		{
			var enemyShips = new ShipBattleInfo[0];
			if (enemyFirstShips != null)
				enemyShips = enemyShips.Concat(enemyFirstShips.Where(x => x != null)).ToArray();
			if (enemSecondShips != null)
				enemyShips = enemyShips.Concat(enemSecondShips.Where(x => x != null)).ToArray();

			IsFirstCombat = MapInfo.IsFirstCombat;
			MapWorldId = MapInfo.WorldId;
			MapAreaId = MapInfo.MapId;
			MapNodeId = MapInfo.NodeId;
			IsBoss = MapInfo.IsBoss;
			EnemyName = data.api_enemy_info.api_deck_name;
			EnemyShips = enemyShips;
			Rank = data.api_win_rank;
		}
	}

	internal class PracticeResultEventArgs : BaseEventArgs
	{
		public PracticeResultEventArgs(kcsapi_battle_result data) : base("SAB".Contains(data.api_win_rank))
		{
		}
	}
}
