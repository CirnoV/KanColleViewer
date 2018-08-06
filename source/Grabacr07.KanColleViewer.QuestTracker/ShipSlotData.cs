using System;
using System.Collections.Generic;
using System.Linq;
using Grabacr07.KanColleWrapper.Models;

namespace Grabacr07.KanColleViewer.QuestTracker.Models
{
	public class ShipSlotData
	{
		public SlotItemInfo Source { get; private set; }

		public bool Equipped => this.Source != null;

		#region 함재기 정보
		public int Maximum { get; set; }
		public int Current { get; set; }
		public int Lost => this.Maximum - this.Current;
		#endregion

		public int Level { get; set; }
		public int Proficiency { get; set; }

		public SlotitemCategoryType CategoryType => (SlotitemCategoryType)this.Source?.RawData.api_type[1];

		public ShipSlotData(SlotItemInfo item, int maximum = -1, int current = -1, int level = 0, int proficiency = 0)
		{
			this.Source = item;
			this.Maximum = maximum;
			this.Current = current;
			this.Level = level;
			this.Proficiency = proficiency;
		}

		public ShipSlotData(ShipSlot slot) : this(slot.Item?.Info, slot.Maximum, slot.Current, slot.Item?.Level ?? 0, slot.Item?.Proficiency ?? 0)
		{
		}
	}
}
