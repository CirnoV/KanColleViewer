using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models.Raw;

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

		public double AfterFirepower => this.Info.Firepower + this.GetImprovementBonus(false);
		public double AfterTorpedo => this.Info.Torpedo + this.GetImprovementBonus(true);

		internal SlotItem(kcsapi_slotitem rawData)
			: base(rawData)
		{
			this.Info = KanColleClient.Current.Master.SlotItems[this.RawData.api_slotitem_id] ?? SlotItemInfo.Dummy;
		}


		public void Remodel(int level, int masterId)
		{
			this.RawData.api_level = level;
			this.Info = KanColleClient.Current.Master.SlotItems[masterId] ?? SlotItemInfo.Dummy;

			this.RaisePropertyChanged(nameof(this.Info));
			this.RaisePropertyChanged(nameof(this.Level));
		}

		public override string ToString()
		{
			return $"ID = {this.Id}, Name = \"{this.Info.Name}\", Level = {this.Level}, Proficiency = {this.Proficiency}";
		}

		internal double GetImprovementBonusFactor(bool isTorpedo)
		{
			switch (this.Info.IconType)
			{
				case SlotItemIconType.MainCanonLight:
				case SlotItemIconType.MainCanonMedium:
				case SlotItemIconType.SecondaryCanon:
				case SlotItemIconType.HighAngleGun:
				case SlotItemIconType.APShell:
				case SlotItemIconType.AntiAircraftFireDirector:
				case SlotItemIconType.Searchlight:
				case SlotItemIconType.LandingCraft:
				case SlotItemIconType.AmphibiousLandingCraft:
					if (!isTorpedo) return 1;
					break;

				case SlotItemIconType.MainCanonHeavy:
					if (!isTorpedo) return 1.5;
					break;

				case SlotItemIconType.Torpedo:
					if (isTorpedo) return 1.2;
					break;

				case SlotItemIconType.Soner:
				case SlotItemIconType.ASW:
					if (!isTorpedo) return 0.75;
					break;

				case SlotItemIconType.AAGun:
					if (!isTorpedo) return 1;
					return 1.2;
			}
			return 0;
		}
		internal double GetImprovementBonus(bool isTorpedo)
			=> this.GetImprovementBonusFactor(isTorpedo) * Math.Sqrt(this.Level);

		public static SlotItem Dummy { get; } = new SlotItem(new kcsapi_slotitem { api_slotitem_id = -1, });
	}
}
