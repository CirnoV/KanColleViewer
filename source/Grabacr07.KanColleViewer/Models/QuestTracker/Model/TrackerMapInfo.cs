﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grabacr07.KanColleViewer.Models.QuestTracker.Model
{
	class TrackerMapInfo
	{
		public bool IsFirstCombat { get; set; }
		public int WorldId { get; set; }
		public int MapId { get; set; }
		public int NodeId { get; set; }

		public TrackerMapInfo AfterCombat()
		{
			var prev = new TrackerMapInfo
			{
				IsFirstCombat = this.IsFirstCombat,
				WorldId = this.WorldId,
				MapId = this.MapId,
				NodeId = this.NodeId
			};
			this.IsFirstCombat = false;
			return prev;
		}
		public void Reset(int WorldId, int MapId, int NodeId)
		{
			this.IsFirstCombat = true;
			this.WorldId = WorldId;
			this.MapId = MapId;
			this.NodeId = NodeId;
		}
	}
}
