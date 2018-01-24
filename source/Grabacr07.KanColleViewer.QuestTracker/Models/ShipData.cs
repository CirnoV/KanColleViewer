using System;
using System.Collections.Generic;
using System.Linq;
using Grabacr07.KanColleWrapper.Models;

namespace Grabacr07.KanColleViewer.QuestTracker.Models
{
	public class ShipData
	{
		public int Id { get; set; }
		public int MasterId { get; set; }
		public string Name { get; set; }

		public ShipSpeed ShipSpeed { get; set; }
		public int ShipType { get; set; }

		public ShipSituation Situation { get; set; }

		public int MaxHP { get; set; }
		public int NowHP { get; set; }
		public int BeforeNowHP { get; set; }

		public IEnumerable<ShipSlotData> Slots { get; set; }
		public ShipSlotData ExSlot { get; set; }
		public bool IsUsedDamecon { get; set; }

		public LimitedValue HP => new LimitedValue(this.NowHP, this.MaxHP, 0);

		public ShipData()
		{
			this.Name = "？？？";
			this.ShipType = 0;
			this.Situation = ShipSituation.None;
			this.Slots = new ShipSlotData[0];
			this.ShipSpeed = ShipSpeed.Immovable;
		}

		public virtual ShipData Clone()
		{
			return new ShipData
			{
				Id = this.Id,
				MasterId = this.MasterId,
				Name = this.Name,
				ShipSpeed = this.ShipSpeed,
				ShipType = this.ShipType,
				Situation = this.Situation,
				MaxHP = this.MaxHP,
				NowHP = this.NowHP,
				BeforeNowHP = this.BeforeNowHP,
				Slots = this.Slots,
				ExSlot = this.ExSlot,
				IsUsedDamecon = this.IsUsedDamecon
			};
		}
	}

	public class MembersShipData : ShipData
	{
		public MembersShipData()
		{
		}
		public MembersShipData(Ship ship) : this()
		{
			this.Id = ship.Id;
			this.MasterId = ship.Info.Id;

			this.Name = ship.Info.Name;
			this.Situation = ship.Situation;

			this.NowHP = ship.HP.Current;
			this.MaxHP = ship.HP.Maximum;

			this.ShipSpeed = ship.Speed;
			this.ShipType = ship.Info.ShipType.Id;

			this.Slots = ship.Slots
				.Where(s => s != null)
				.Where(s => s.Equipped)
				.Select(s => new ShipSlotData(s))
				.ToArray();
			this.ExSlot =
				ship.ExSlotExists && ship.ExSlot.Equipped
				? new ShipSlotData(ship.ExSlot)
				: null;
		}
	}
	public class MastersShipData : ShipData
	{
		public MastersShipData()
		{
		}

		public MastersShipData(ShipInfo info) : this()
		{
			this.Id = info.Id;
			this.Name = info.Name;
			this.ShipSpeed = info?.Speed ?? ShipSpeed.Immovable;
			this.ShipType = info?.ShipType?.Id ?? 0;
		}
	}
}
