using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models.Raw;
using Grabacr07.KanColleWrapper.Internal;

namespace Grabacr07.KanColleWrapper.Models
{
	public class SlotItem : RawDataWrapper<kcsapi_slotitem>, IIdentifiable
	{
		public int Id => this.RawData.api_id;

		public SlotItemInfo Info { get; private set; }

		public string NameWithLevel => $"{this.Info.Name}{(this.Level >= 1 ? (" " + this.LevelText) : "")}{(this.Proficiency >= 1 ? (" " + this.ProficiencyText) : "")}";

		public int Level => this.RawData.api_level;
		public string LevelText => this.Level >= 10 ? "★max" : this.Level >= 1 ? ("★+" + this.Level) : "";

		public int Proficiency => this.RawData.api_alv;
		public string ProficiencyText => this.Proficiency >= 1 ? (" (숙련도 " + this.Proficiency + ")") : "";

		public bool Locked => this.RawData.api_locked == 1;

		private SlotItemStat _ImprovementStats { get; set; }
		public SlotItemStat ImprovementStats
		{
			get { return this._ImprovementStats; }
			private set
			{
				if (this._ImprovementStats != value)
				{
					this._ImprovementStats = value;
					this.RaisePropertyChanged(nameof(this.ImprovementStats));
				}
			}
		}

		public double ResultFirepower => this.Info.Firepower + this.ImprovementStats.Firepower;
		public double ResultTorpedo => this.Info.Torpedo + this.ImprovementStats.Torpedo;
		public double ResultBomb => this.Info.Bomb + this.ImprovementStats.Bomb;

		internal SlotItem(kcsapi_slotitem rawData)
			: base(rawData)
		{
			this.Info = KanColleClient.Current.Master.SlotItems[this.RawData.api_slotitem_id] ?? SlotItemInfo.Dummy;
			this.ImprovementStats = this.GetImprovementBonus();
		}


		public void Remodel(int level, int masterId)
		{
			this.RawData.api_level = level;
			this.Info = KanColleClient.Current.Master.SlotItems[masterId] ?? SlotItemInfo.Dummy;
			this.ImprovementStats = this.GetImprovementBonus();

			this.RaisePropertyChanged(nameof(this.Info));
			this.RaisePropertyChanged(nameof(this.Level));
		}

		public override string ToString()
		{
			return $"ID = {this.Id}, Name = \"{this.Info.Name}\", Level = {this.Level}, Proficiency = {this.Proficiency}";
		}

		public static SlotItem Dummy { get; } = new SlotItem(new kcsapi_slotitem { api_slotitem_id = -1, });
	}
}
