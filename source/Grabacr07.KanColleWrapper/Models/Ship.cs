using System;
using System.Collections.Generic;
using System.Linq;
using Grabacr07.KanColleWrapper.Internal;
using Grabacr07.KanColleWrapper.Models.Raw;
using System.Diagnostics;
using System.Threading;

namespace Grabacr07.KanColleWrapper.Models
{
	/// <summary>
	/// 母港に所属している艦娘を表します。
	/// </summary>
	public class Ship : RawDataWrapper<kcsapi_ship2>, IIdentifiable
	{
		private readonly Homeport homeport;

		/// <summary>
		/// この艦娘を識別する ID を取得します。
		/// </summary>
		public int Id => this.RawData.api_id;
		public int FleetId
		{
			get
			{
				try
				{
					foreach (var fleet in homeport.Organization.Fleets)
					{
						foreach (var ship in fleet.Value.Ships)
						{
							if (ship.Id == this.Id) return fleet.Value.Id;
						}
					}
				}
				catch (Exception e)
				{
					Debug.WriteLine(e);
					return -1;
				}
				return -1;
			}
		}
		public int RemodelLevel
		{
			get
			{
				if (this.Info.NextRemodelingLevel.HasValue)
				{
					if (this.Info.NextRemodelingLevel.Value <= this.Level)
						return -1;
					else return this.Info.NextRemodelingLevel.Value;
				}
				else
					return 0;
			}
		}

		public bool NeedBlueprint
		{
			get
			{
				return KanColleClient.Current.Master.RawData.api_mst_shipupgrade
					.FirstOrDefault(x => x.api_current_ship_id == this.Info.Id)
					?.api_drawing_count > 0;
			}
		}
		public bool NeedCatapult
		{
			get
			{
				return KanColleClient.Current.Master.RawData.api_mst_shipupgrade
					.FirstOrDefault(x => x.api_current_ship_id == this.Info.Id)
					?.api_catapult_count > 0;
			}
		}
		public bool NeedReport
		{
			get
			{
				return KanColleClient.Current.Master.RawData.api_mst_shipupgrade
					.FirstOrDefault(x => x.api_current_ship_id == this.Info.Id)
					?.api_report_count > 0;
			}
		}

		public string LvName => "[Lv." + this.Level + "]  " + this.Info.Name;
		public string RepairTimeString
		{
			get
			{
				TimeSpan? Remaining = new TimeSpan(0, 0, 0, 0, (int)this.TimeToRepair.TotalMilliseconds);
				return Remaining.HasValue
					? string.Format("{0:D2}:{1}",
						(int)Remaining.Value.TotalHours,
						Remaining.Value.ToString(@"mm\:ss"))
					: "--:--:--";
			}
		}

		/// <summary>
		/// 艦娘の種類に基づく情報を取得します。
		/// </summary>
		public ShipInfo Info { get; private set; }

		public int SortNumber => this.RawData.api_sortno;

		/// <summary>
		/// 艦娘の現在のレベルを取得します。
		/// </summary>
		public int Level => this.RawData.api_lv;

		/// <summary>
		/// 艦娘がロックされているかどうかを示す値を取得します。
		/// </summary>
		public bool IsLocked => this.RawData.api_locked == 1;

		/// <summary>
		/// 艦娘の現在の累積経験値を取得します。
		/// </summary>
		public int Exp => this.RawData.api_exp.Get(0) ?? 0;

		/// <summary>
		/// この艦娘が次のレベルに上がるために必要な経験値を取得します。
		/// </summary>
		public int ExpForNextLevel => this.RawData.api_exp.Get(1) ?? 0;

		/// <summary>
		/// この艦娘の次の改造までに必要な経験値を取得します。
		/// </summary>
		public int ExpForNextRemodelingLevel => Math.Max(Experience.GetShipExpForSpecifiedLevel(this.Exp, this.Info?.NextRemodelingLevel), 0);

		/// <summary>
		/// この艦娘とケッコンカッコカリするために必要な経験値を取得します。
		/// </summary>
		public int ExpForMarrige => Math.Max(Experience.GetShipExpForSpecifiedLevel(this.Exp, 99), 0);

		/// <summary>
		/// この艦娘が最大Lvになるために必要な経験値を取得します。
		/// </summary>
		public int ExpForLevelMax => Experience.GetShipExpForSpecifiedLevel(this.Exp, 165);

		/// <summary>
		/// ExSlot 이 존재하는지 여부
		/// </summary>
		public bool ExSlotExists => this.RawData.api_slot_ex != 0;

		#region HP 変更通知プロパティ

		private LimitedValue _HP;

		/// <summary>
		/// 耐久値を取得します。
		/// </summary>
		public LimitedValue HP
		{
			get { return this._HP; }
			private set
			{
				this._HP = value;
				this.RaisePropertyChanged();

				if (value.IsHeavilyDamage())
				{
					this.Situation |= ShipSituation.HeavilyDamaged;
				}
				else
				{
					this.Situation &= ~ShipSituation.HeavilyDamaged;
				}
			}
		}

		#endregion

		/// <summary>
		/// 함선 속도
		/// </summary>
		public ShipSpeed Speed => ShipSpeedConverter.FromInt32(this.RawData.api_soku);

		#region Fuel 変更通知プロパティ
		private LimitedValue _Fuel;
		public LimitedValue Fuel
		{
			get { return this._Fuel; }
			private set
			{
				this._Fuel = value;
				this.RaisePropertyChanged();
				this.RaisePropertyChanged(nameof(this.UsedFuel));
				this.RaisePropertyChanged(nameof(this.FuelText));
			}
		}
		#endregion

		#region Bull 変更通知プロパティ
		private LimitedValue _Bull;
		public LimitedValue Bull
		{
			get { return this._Bull; }
			private set
			{
				this._Bull = value;
				this.RaisePropertyChanged();
				this.RaisePropertyChanged(nameof(this.UsedBull));
				this.RaisePropertyChanged(nameof(this.BullText));
			}
		}
		#endregion

		#region Bauxite 変更通知プロパティ
		public LimitedValue Bauxite
			=> this.ExSlot != null
				? new LimitedValue(
						(this.Slots.Sum(x => x.Current) + this.ExSlot.Current) * 5,
						(this.Slots.Sum(x => x.Maximum) + this.ExSlot.Maximum) * 5,
						0
					)
				: new LimitedValue(
						this.Slots.Sum(x => x.Current) * 5,
						this.Slots.Sum(x => x.Maximum) * 5,
						0
					);
		#endregion

		#region Firepower 変更通知プロパティ

		private ModernizableStatus _Firepower;

		/// <summary>
		/// 火力ステータス値を取得します。
		/// </summary>
		public ModernizableStatus Firepower
		{
			get { return this._Firepower; }
			private set
			{
				this._Firepower = value;
				this.RaisePropertyChanged();
			}
		}

		#endregion

		#region Torpedo 変更通知プロパティ

		private ModernizableStatus _Torpedo;

		/// <summary>
		/// 雷装ステータス値を取得します。
		/// </summary>
		public ModernizableStatus Torpedo
		{
			get { return this._Torpedo; }
			private set
			{
				this._Torpedo = value;
				this.RaisePropertyChanged();
			}
		}

		#endregion

		#region YasenFp 変更通知プロパティ

		private ModernizableStatus _YasenFp;

		public ModernizableStatus YasenFp
		{
			get { return this._YasenFp; }
			private set
			{
				this._YasenFp = value;
				this.RaisePropertyChanged();
			}
		}

		#endregion

		#region AA 変更通知プロパティ

		private ModernizableStatus _AA;

		/// <summary>
		/// 対空ステータス値を取得します。
		/// </summary>
		public ModernizableStatus AA
		{
			get { return this._AA; }
			private set
			{
				this._AA = value;
				this.RaisePropertyChanged();
			}
		}

		#endregion

		#region Armer 変更通知プロパティ

		private ModernizableStatus _Armer;

		/// <summary>
		/// 装甲ステータス値を取得します。
		/// </summary>
		public ModernizableStatus Armer
		{
			get { return this._Armer; }
			private set
			{
				this._Armer = value;
				this.RaisePropertyChanged();
			}
		}

		#endregion

		#region ASW 変更通知プロパティ

		private ModernizableStatus _ASW;

		/// <summary>
		/// 対潜ステータス値を取得します。
		/// </summary>
		public ModernizableStatus ASW
		{
			get { return this._ASW; }
			private set
			{
				this._ASW = value;
				this.RaisePropertyChanged();
				this.RaisePropertyChanged(nameof(this.OpeningASW));
			}
		}

		#endregion

		#region Luck 変更通知プロパティ

		private ModernizableStatus _Luck;

		/// <summary>
		/// 運のステータス値を取得します。
		/// </summary>
		public ModernizableStatus Luck
		{
			get { return this._Luck; }
			private set
			{
				this._Luck = value;
				this.RaisePropertyChanged();
			}
		}

		#endregion

		#region Slots 変更通知プロパティ

		private ShipSlot[] _Slots;

		/// <summary>
		/// この艦娘の装備スロットを取得します。装備していないスロットには <see cref="ShipSlot.Equipped"/> が false のオブジェクトが割り当てられます (null を返しません)。
		/// </summary>
		public ShipSlot[] Slots
		{
			get { return this._Slots; }
			set
			{
				if (this._Slots != value)
				{
					this._Slots = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region EquippedSlots 変更通知プロパティ

		private ShipSlot[] _EquippedSlots;

		/// <summary>
		/// <see cref="Slots"/> と <see cref="ExSlot"/> のなかで <see cref="ShipSlot.Equipped"/> が true (空でないスロット) を列挙します。
		/// </summary>
		public ShipSlot[] EquippedItems
		{
			get { return this._EquippedSlots; }
			set
			{
				if (this._EquippedSlots != value)
				{
					this._EquippedSlots = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region ExSlot 変更通知プロパティ

		private ShipSlot _ExSlot;

		public ShipSlot ExSlot
		{
			get { return this._ExSlot; }
			set
			{
				if (this._ExSlot != value)
				{
					this._ExSlot = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region TimeToRepair 変更通知プロパティ

		private TimeSpan _TimeToRepair;

		public TimeSpan TimeToRepair
		{
			get { return this._TimeToRepair; }
			set
			{
				if (this._TimeToRepair != value)
				{
					this._TimeToRepair = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region UsedFuel UsedBull UsedBauxite / FuelText BullText
		public int UsedFuel => (int)((this.Level <= 99 ? 1.0f : 0.85f) * (this.Fuel.Maximum - this.Fuel.Current));
		public int UsedBull => (int)((this.Level <= 99 ? 1.0f : 0.85f) * (this.Bull.Maximum - this.Bull.Current));
		public int UsedBauxite => this.Bauxite.Maximum - this.Bauxite.Current;

		public string FuelText
		{
			get
			{
				var perc = decimal.Ceiling((decimal)this.Fuel.Current / this.Fuel.Maximum * 10);
				if (perc >= 8) return "패널티 없음";
				else if (perc >= 4) return "명중·회피 감소";
				else if (perc >= 1) return "명중·회피 격감";
				else return "명중·회피 최소";
			}
		}
		public string BullText
		{
			get
			{
				var perc = decimal.Ceiling((decimal)this.Bull.Current / this.Bull.Maximum * 10);
				if (perc >= 5) return "패널티 없음";
				else if (perc >= 4) return "80% 데미지";
				else if (perc >= 3) return "60% 데미지";
				else if (perc >= 2) return "40% 데미지";
				else if (perc >= 1) return "20% 데미지";
				else return "지근탄";
			}
		}
		#endregion

		/// <summary>
		/// 装備によるボーナスを含めた索敵ステータス値を取得します。
		/// </summary>
		public int ViewRange => this.RawData.api_sakuteki.Get(0) ?? 0;

		/// <summary>
		/// 火力・雷装・対空・装甲のすべてのステータス値が最大値に達しているかどうかを示す値を取得します。
		/// </summary>
		public bool IsMaxModernized => this.Firepower.IsMax && this.Torpedo.IsMax && this.AA.IsMax && this.Armer.IsMax;

		private readonly int[] DaihatsuEquipableShips = new int[] { 490, 463, 548, 464, 470, 469, 435, 489, 498, 434, 199, 418, 147, 200, 487, 547, 478, 488, 541 };
		private readonly int[] DaihatsuUnequipableShips = new int[] { 491, 445 };

		public bool DaihatsuEquipable =>
			(DaihatsuEquipableShips.Contains(this.Info.Id)) || // 리스트에 포함되어있거나
			(!DaihatsuUnequipableShips.Contains(this.Info.Id) && this.Info.ShipType.RawData.api_equip_type?._24 == 1);
			// 제외 리스트에 없으면서 장착 가능

		/// <summary>
		/// 現在のコンディション値を取得します。
		/// </summary>
		public int Condition => this.RawData.api_cond;

		/// <summary>
		/// コンディションの種類を示す <see cref="ConditionType" /> 値を取得します。
		/// </summary>
		public ConditionType ConditionType => ConditionTypeHelper.ToConditionType(this.RawData.api_cond);

		/// <summary>
		/// この艦が出撃した海域を識別する整数値を取得します。
		/// </summary>
		public int SallyArea => this.RawData.api_sally_area;

		#region Status 変更通知プロパティ

		private ShipSituation situation;

		public ShipSituation Situation
		{
			get { return this.situation; }
			set
			{
				if (this.situation != value)
				{
					this.situation = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region IntStatus 変更通知プロパティ

		private int _IntStatus;

		public int IntStatus
		{
			get { return this._IntStatus; }
			set
			{
				if (this._IntStatus != value)
				{
					this._IntStatus = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		public int SlotsASW => this.EquippedItems.Sum(x => x.Item.Info.ASW);
		public int SumASW => this.ASW.Current + this.SlotsASW;

		public int SumFirepower => this.Firepower.Current + this.EquippedItems.Sum(x => x.Item.Info.Firepower);
		public int SumTorpedo => this.Torpedo.Current + this.EquippedItems.Sum(x => x.Item.Info.Torpedo);
		public int SumAA => this.AA.Current + this.EquippedItems.Sum(x => x.Item.Info.AA);
		public int SumLOS => this.ViewRange;// + this.EquippedItems.Sum(x => x.Item.Info.ViewRange);
		public int SumArmor => this.Armer.Current + this.EquippedItems.Sum(x => x.Item.Info.Armer);
		public int SumEvade => this.RawData.api_kaihi[0] + this.EquippedItems.Sum(x => x.Item.Info.Evade);
		public int SumCarry => this.Slots.Sum(x => x.Maximum);

		#region Cap Calc
		public int DmgCap_Day => this.CalcDamageCap(1.0, 180, false);
		public bool DmgCap_DayOver => this.DmgCap_Day >= 180;

		public int DmgCap_Day_HeadOn => this.CalcDamageCap(0.8, 180, false);
		public bool DmgCap_Day_HeadOnOver => this.DmgCap_Day_HeadOn >= 180;

		public int DmgCap_Night => this.CalcDamageCap(1.0, 300, false);
		public bool DmgCap_NightOver => this.DmgCap_Night >= 300;

		public int DmgCap_Night_HeadOn => this.CalcDamageCap(0.8, 300, false);
		public bool DmgCap_Night_HeadOnOver => this.DmgCap_Night_HeadOn >= 300;

		//

		public int DmgCap_Combined_Day => this.CalcDamageCap(1.0, 180, true);
		public bool DmgCap_Combined_DayOver => this.DmgCap_Combined_Day >= 180;

		public int DmgCap_Combined_Day_HeadOn => this.CalcDamageCap(0.8, 180, true);
		public bool DmgCap_Combined_Day_HeadOnOver => this.DmgCap_Combined_Day_HeadOn >= 180;

		public int DmgCap_Combined_Night => this.CalcDamageCap(1.0, 300, true);
		public bool DmgCap_Combined_NightOver => this.DmgCap_Combined_Night >= 300;

		public int DmgCap_Combined_Night_HeadOn => this.CalcDamageCap(0.8, 300, true);
		public bool DmgCap_Combined_Night_HeadOnOver => this.DmgCap_Combined_Night_HeadOn >= 300;

		//

		public int DmgCap_Support => this.CalcDamageCap(1.0, 150, false, true);
		public bool DmgCap_SupportOver => this.DmgCap_Support >= 150;

		public int DmgCap_Support_HeadOn => this.CalcDamageCap(0.8, 150, false, true);
		public bool DmgCap_Support_HeadOnOver => this.DmgCap_Support_HeadOn >= 150;
		#endregion

		public int Range => this.Info.RawData.api_leng;

		// 선제대잠 가능 여부
		public bool OpeningASW
			=> (this.Info.Id == 141 || this.Info.Id == 478) ? true // 이스즈改2 or 타츠타改2
				: this.Info.ShipType.Id == 1 ? SumASW >= 60 // 해방함
				: this.Info.ShipType.Id == 7 && this.Speed == ShipSpeed.Slow ? SumASW >= 65 // 저속 경공모
				: SumASW >= 100;

		internal Ship(Homeport parent, kcsapi_ship2 rawData)
			: base(rawData)
		{
			this.homeport = parent;
			this.Update(rawData);
		}

		internal void Update(kcsapi_ship2 rawData)
		{
			this.UpdateRawData(rawData);

			this.Info = KanColleClient.Current.Master.Ships[rawData.api_ship_id] ?? ShipInfo.Dummy;
			this.HP = new LimitedValue(this.RawData.api_nowhp, this.RawData.api_maxhp, 0);
			this.Fuel = new LimitedValue(this.RawData.api_fuel, this.Info.RawData.api_fuel_max, 0);
			this.Bull = new LimitedValue(this.RawData.api_bull, this.Info.RawData.api_bull_max, 0);
			this.ASW = new ModernizableStatus(new int[] { 0, this.RawData.api_taisen[1] }, this.RawData.api_taisen[0]);

			double temp = (double)this.HP.Current / (double)this.HP.Maximum;

			if (temp <= 0.25) IntStatus = 3;
			else if (temp <= 0.5) IntStatus = 2;
			else if (temp <= 0.75) IntStatus = 1;
			else IntStatus = 0;

			if (this.RawData.api_kyouka.Length >= 5)
			{
				this.Firepower = new ModernizableStatus(this.Info.RawData.api_houg, this.RawData.api_kyouka[0]);
				this.Torpedo = new ModernizableStatus(this.Info.RawData.api_raig, this.RawData.api_kyouka[1]);
				this.YasenFp = new ModernizableStatus(
					new int[] {
						this.Info.RawData.api_houg[0] + this.Info.RawData.api_raig[0],
						this.Info.RawData.api_houg[1] + this.Info.RawData.api_raig[1]},
					this.RawData.api_kyouka[0] + this.RawData.api_kyouka[1]);
				this.AA = new ModernizableStatus(this.Info.RawData.api_tyku, this.RawData.api_kyouka[2]);
				this.Armer = new ModernizableStatus(this.Info.RawData.api_souk, this.RawData.api_kyouka[3]);
				this.Luck = new ModernizableStatus(this.Info.RawData.api_luck, this.RawData.api_kyouka[4]);
			}

			this.TimeToRepair = TimeSpan.FromMilliseconds(this.RawData.api_ndock_time);
			this.UpdateSlots();
		}

		public void UpdateSlots()
		{
			this.Slots = this.RawData.api_slot
				.Select(id => this.homeport.Itemyard.SlotItems[id])
				.Select((t, i) => new ShipSlot(this, t, this.Info.RawData.api_maxeq.Get(i) ?? 0, this.RawData.api_onslot.Get(i) ?? 0))
				.ToArray();
			this.ExSlot = new ShipSlot(this, this.homeport.Itemyard.SlotItems[this.RawData.api_slot_ex], 0, 0);
			this.EquippedItems = this.EnumerateAllEquippedItems().ToArray();

			if (this.EquippedItems.Any(x => x.Item.Info.Type == SlotItemType.応急修理要員))
			{
				this.Situation |= ShipSituation.DamageControlled;
			}
			else
			{
				this.Situation &= ~ShipSituation.DamageControlled;
			}

			//장비의 대잠 수치 제외
			this.ASW = this.ASW.Update(this.ASW.Upgraded - this.Slots.Sum(slot => slot.Item.Info.ASW));
		}


		internal void Charge(int fuel, int bull, int[] onslot)
		{
			this.Fuel = this.Fuel.Update(fuel);
			this.Bull = this.Bull.Update(bull);
			for (var i = 0; i < this.Slots.Length; i++) this.Slots[i].Current = onslot.Get(i) ?? 0;
		}

		internal void Repair()
		{
			var max = this.HP.Maximum;
			this.HP = this.HP.Update(max);
		}

		public override string ToString()
		{
			return $"ID = {this.Id}, Name = \"{this.Info.Name}\", ShipType = \"{this.Info.ShipType.Name}\", Level = {this.Level}";
		}


		private IEnumerable<ShipSlot> EnumerateAllEquippedItems()
		{
			foreach (var slot in this.Slots.Where(x => x.Equipped)) yield return slot;
			if (this.ExSlot.Equipped) yield return this.ExSlot;
		}

		internal static int CalcCombinedFleetFactor(CombinedFleetType type, int fleetId, bool abyssalCombined)
		{
			if (type == CombinedFleetType.None || fleetId >= 3)
				return 0;

			if (abyssalCombined)
			{
				if (fleetId == 2)
				{
					if (type == CombinedFleetType.None)
						return 5;
					else
						return -5;
				}

				switch (type)
				{
					case CombinedFleetType.SurfaceTaskForce:
						return 2;

					case CombinedFleetType.CarrierTaskForce:
						return 2;

					case CombinedFleetType.TransportEscort:
						return -5;

					default:
						return 5;
				}
			}
			else
			{
				switch (type)
				{
					case CombinedFleetType.SurfaceTaskForce:
						if (fleetId == 1)
							return 10;
						else
							return -5;

					case CombinedFleetType.CarrierTaskForce:
						if (fleetId == 1)
							return 2;
						else
							return 10;
					case CombinedFleetType.TransportEscort:
						if (fleetId == 1)
							return -5;
						else
							return 10;
				}
			}
			return 0;
		}
		internal int CalcDamageCap(double formationMultiply, int cap, bool abyssalCombined, bool forSupport = false)
		{
			var combinedFleetFactor = CalcCombinedFleetFactor(homeport.Organization.CombinedType, this.FleetId, abyssalCombined);
			double power = 0;

			var shipType = this.Info.ShipType.Id;
			// 항모계열
			if (shipType == 7 || shipType == 11 || shipType == 18)
			{
				power =
					(int)
					(
						(
							this.Firepower.Current
							+ this.EquippedItems.Sum(x => x.Item.ResultFirepower)
							+ this.Torpedo.Current
							+ this.EquippedItems.Sum(x => x.Item.ResultTorpedo)
							+ this.EquippedItems.Sum(x => (int)(x.Item.ResultBomb * 1.3))
							+ (forSupport ? -1 : combinedFleetFactor)
						)
						* 1.5
					)
					+ 55;
			}
			// 그 외 함선
			else
			{
				power = this.Firepower.Current
					+ this.EquippedItems.Sum(x => x.Item.ResultFirepower)
					+ (forSupport ? -1 : combinedFleetFactor)
					+ 5;
			}

			power *= formationMultiply;
			if (power > cap) // soft cap
				power = cap + Math.Sqrt(power - cap);

			return (int)Math.Floor(power);
		}
	}
}
