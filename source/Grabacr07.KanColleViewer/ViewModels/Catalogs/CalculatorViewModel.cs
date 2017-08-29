using Grabacr07.KanColleViewer.Models;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Livet.EventListeners;
using MetroTrilithon.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Grabacr07.KanColleViewer.ViewModels.Catalogs
{
	public class CalculatorViewModel : WindowViewModel
	{
		/// <summary>
		/// Completely experience table from 1 to 165. Each line = 20 levels
		/// </summary>
		public static int[] ExpTable = new int[] { 0, 0, 100, 300, 600, 1000, 1500, 2100, 2800, 3600, 4500, 5500, 6600, 7800, 9100, 10500, 12000, 13600, 15300, 17100, 19000,
			21000, 23100, 25300, 27600, 30000, 32500, 35100, 37800, 40600, 43500, 46500, 49600, 52800, 56100, 59500, 63000, 66600, 70300, 74100, 78000,
			82000, 86100, 90300, 94600, 99000, 103500, 108100, 112800, 117600, 122500, 127500, 132700, 138100, 143700, 149500, 155500, 161700, 168100, 174700, 181500,
			188500, 195800, 203400, 211300, 219500, 228000, 236800, 245900, 255300, 265000, 275000, 285400, 296200, 307400, 319000, 331000, 343400, 356200, 369400, 383000,
			397000, 411500, 426500, 442000, 458000, 474500, 491500, 509000, 527000, 545500, 564500, 584500, 606500, 631500, 661500, 701500, 761500, 851500, 1000000, 1000000,
			1010000, 1011000, 1013000, 1016000, 1020000, 1025000, 1031000, 1038000, 1046000, 1055000, 1065000, 1077000, 1091000, 1107000, 1125000, 1145000, 1168000, 1194000, 1223000, 1255000,
			1290000, 1329000, 1372000, 1419000, 1470000, 1525000, 1584000, 1647000, 1714000, 1785000, 1860000, 1940000, 2025000, 2115000, 2210000, 2310000, 2415000, 2525000, 2640000, 2760000,
			2887000, 3021000, 3162000, 3310000, 3465000, 3628000, 3799000, 3978000, 4165000, 4360000, 4564000, 4777000, 4999000, 5230000, 5470000, 5720000, 5780000, 5860000, 5970000, 6120000,
			6320000, 6580000, 6910000, 7320000, 7820000 };

		/// <summary>
		/// Sea exp table. Cannot be used properly in xaml without dumb workarounds.
		/// </summary>
		public static Dictionary<string, int> SeaExpTable = new Dictionary<string, int>
		{
			{"1-1", 30}, {"1-2", 50}, {"1-3", 80}, {"1-4", 100}, {"1-5", 150},
			{"2-1", 120}, {"2-2", 150}, {"2-3", 200},{"2-4", 300},{"2-5", 250},
			{"3-1", 310}, {"3-2", 320}, {"3-3", 330}, {"3-4", 350},{"3-5",400},
			{"4-1", 310}, {"4-2", 320}, {"4-3", 330}, {"4-4", 340},
			{"5-1", 360}, {"5-2", 380}, {"5-3", 400}, {"5-4", 420}, {"5-5", 450},
			{"6-1", 380}, {"6-2", 420}
		};
		public IEnumerable<string> SeaList => CalculatorViewModel.SeaExpTable.Keys.ToList();

		public string[] ResultRanks { get; } = new string[] { "S", "A", "B", "C", "D", "E" };
		public IEnumerable<string> ResultList => this.ResultRanks.ToList();

		public LBASActionTypeViewModel[] LandBasedType { get; } = new LBASActionTypeViewModel[]
		{
			new LBASActionTypeViewModel(LBASActionType.Attack, "출격"),
			new LBASActionTypeViewModel(LBASActionType.Defence, "방공"),
		};
		public IEnumerable<LBASActionTypeViewModel> LandBasedTypeList => this.LandBasedType.ToList();


		private readonly Subject<Unit> UpdateSourceShipList = new Subject<Unit>();
		private readonly Homeport homeport = KanColleClient.Current.Homeport;

		#region TabItems 변경통지 프로퍼티
		private string[] _TabItems;
		public string[] TabItems
		{
			get { return this._TabItems; }
			set
			{
				if (this._TabItems != value)
				{
					this._TabItems = value;
					this.RaisePropertyChanged();
				}
			}
		}

		private string _SelectedTab;
		public string SelectedTab
		{
			get { return this._SelectedTab; }
			set
			{
				if (this._SelectedTab != value)
				{
					this._SelectedTab = value;
					this.RaisePropertyChanged();
					this.RaisePropertyChanged("SelectedTabIdx");
					this.UpdateCalculator();
				}
			}
		}

		public int SelectedTabIdx => this.SelectedTab == null
			? 0
			: (this.TabItems?.ToList().IndexOf(this.SelectedTab) ?? 0);
		#endregion

		#region Ships 変更通知プロパティ
		private IReadOnlyCollection<ShipViewModel> _Ships;
		public IReadOnlyCollection<ShipViewModel> Ships
		{
			get { return this._Ships; }
			set
			{
				if (this._Ships != value)
				{
					this._Ships = value;
					this.RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region IsReloading 変更通知プロパティ
		private int _Reloading;
		public int Reloading
		{
			get { return this._Reloading; }
			set
			{
				if (this._Reloading != value)
				{
					this._Reloading = value;
					this.RaisePropertyChanged();
					this.RaisePropertyChanged(nameof(IsReloading));
				}
			}
		}

		public bool IsReloading => this.Reloading > 0;
		#endregion


		#region CurrentShip 変更通知プロパティ
		private Ship _CurrentShip;
		public Ship CurrentShip
		{
			get { return this._CurrentShip; }
			set
			{
				if (this._CurrentShip != value)
				{
					this._CurrentShip = value;
					if (value != null)
					{
						this.CurrentLevel = this.CurrentShip.Level;
						this.TargetLevel = Math.Min(this.CurrentShip.Level + 1, 165);
						if (this.CurrentShip.Info.NextRemodelingLevel.HasValue)
						{
							if (this.CurrentShip.Info.NextRemodelingLevel.Value > this.CurrentLevel)
							{
								this.RemodelLv = this.CurrentShip.Info.NextRemodelingLevel.Value;
								this.TargetLevel = RemodelLv;
							}
						}
						this.CurrentExp = this.CurrentShip.Exp;
						this.UpdateCalculator();
						this.RaisePropertyChanged();
					}
				}
			}
		}
		#endregion

		#region CurrentLevel 変更通知プロパティ
		private int _CurrentLevel;
		public int CurrentLevel
		{
			get { return this._CurrentLevel; }
			set
			{
				if (this._CurrentLevel != value && value >= 1 && value <= 165)
				{
					this._CurrentLevel = value;
					this.CurrentExp = ExpTable[value];
					this.TargetLevel = Math.Max(this.TargetLevel, Math.Min(value + 1, 165));
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		#endregion

		#region TargetLevel 変更通知プロパティ
		private int _TargetLevel;
		public int TargetLevel
		{
			get { return this._TargetLevel; }
			set
			{
				if (this._TargetLevel != value && value >= 1 && value <= 165)
				{
					this._TargetLevel = value;
					this.TargetExp = ExpTable[value];
					this.CurrentLevel = Math.Min(this.CurrentLevel, Math.Max(value - 1, 1));
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		#endregion

		#region SelectedSea 変更通知プロパティ
		private string _SelectedSea;
		public string SelectedSea
		{
			get { return this._SelectedSea; }
			set
			{
				if (_SelectedSea != value)
				{
					this._SelectedSea = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		#endregion

		#region SelectedResult 変更通知プロパティ
		private string _SelectedResult;
		public string SelectedResult
		{
			get { return this._SelectedResult; }
			set
			{
				if (this._SelectedResult != value)
				{
					this._SelectedResult = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		#endregion

		#region IsFlagship 変更通知プロパティ
		private bool _IsFlagship;
		public bool IsFlagship
		{
			get { return this._IsFlagship; }
			set
			{
				if (this._IsFlagship != value)
				{
					this._IsFlagship = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		#endregion

		#region IsMVP 変更通知プロパティ
		private bool _IsMVP;
		public bool IsMVP
		{
			get { return this._IsMVP; }
			set
			{
				if (this._IsMVP != value)
				{
					this._IsMVP = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		#endregion

		#region CurrentExp 変更通知プロパティ
		private int _CurrentExp;
		public int CurrentExp
		{
			get { return this._CurrentExp; }
			set
			{
				if (this._CurrentExp != value)
				{
					this._CurrentExp = value;
					this.RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region TargetExp 変更通知プロパティ
		private int _TargetExp;
		public int TargetExp
		{
			get { return this._TargetExp; }
			set
			{
				if (this._TargetExp != value)
				{
					this._TargetExp = value;
					this.RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region RemodelLv 変更通知プロパティ
		private int _RemodelLv;
		public int RemodelLv
		{
			get { return this._RemodelLv; }
			set
			{
				if (this._RemodelLv != value)
				{
					this._RemodelLv = value;
					this.RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region SortieExp 変更通知プロパティ
		private int _SortieExp;
		public int SortieExp
		{
			get { return this._SortieExp; }
			set
			{
				if (this._SortieExp != value)
				{
					this._SortieExp = value;
					this.RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region RemainingExp 変更通知プロパティ
		private int _RemainingExp;
		public int RemainingExp
		{
			get { return this._RemainingExp; }
			set
			{
				if (this._RemainingExp != value)
				{
					this._RemainingExp = value;
					this.RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region RunCount 変更通知プロパティ
		private int _RunCount;
		public int RunCount
		{
			get { return this._RunCount; }
			set
			{
				if (this._RunCount != value)
				{
					this._RunCount = value;
					this.RaisePropertyChanged();
				}
			}
		}
		#endregion


		#region 연습전 경험치 프로퍼티
		private int _Training_FlagshipExp;
		private int _Training_FlagshipMvpExp;
		private int _Training_AccshipExp;
		private int _Training_AccshipMvpExp;
		private int _Training_TrainingCruiser_Bonus;

		private int _Training_Flagship_Lv;
		private int _Training_Secondship_Lv;
		private bool _Training_Secondship;

		public int Training_FlagshipExp
		{
			get { return this._Training_FlagshipExp; }
			set
			{
				if (this._Training_FlagshipExp != value)
				{
					this._Training_FlagshipExp = value;
					this.RaisePropertyChanged();
				}
			}
		}
		public int Training_FlagshipMvpExp
		{
			get { return this._Training_FlagshipMvpExp; }
			set
			{
				if (this._Training_FlagshipMvpExp != value)
				{
					this._Training_FlagshipMvpExp = value;
					this.RaisePropertyChanged();
				}
			}
		}
		public int Training_AccshipExp
		{
			get { return this._Training_AccshipExp; }
			set
			{
				if (this._Training_AccshipExp != value)
				{
					this._Training_AccshipExp = value;
					this.RaisePropertyChanged();
				}
			}
		}
		public int Training_AccshipMvpExp
		{
			get { return this._Training_AccshipMvpExp; }
			set
			{
				if (this._Training_AccshipMvpExp != value)
				{
					this._Training_AccshipMvpExp = value;
					this.RaisePropertyChanged();
				}
			}
		}
		public int Training_TrainingCruiser_Bonus
		{
			get { return this._Training_TrainingCruiser_Bonus; }
			set
			{
				if (this._Training_TrainingCruiser_Bonus != value)
				{
					this._Training_TrainingCruiser_Bonus = value;
					this.RaisePropertyChanged();
				}
			}
		}

		public int Training_Flagship_Lv
		{
			get { return this._Training_Flagship_Lv; }
			set
			{
				if (this._Training_Flagship_Lv != value)
				{
					this._Training_Flagship_Lv = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		public int Training_Secondship_Lv
		{
			get { return this._Training_Secondship_Lv; }
			set
			{
				if (this._Training_Secondship_Lv != value)
				{
					this._Training_Secondship_Lv = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		public bool Training_Secondship
		{
			get { return this._Training_Secondship; }
			set
			{
				if (this._Training_Secondship != value)
				{
					this._Training_Secondship = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}

		private string _SelectedExpResult;
		public string SelectedExpResult
		{
			get { return this._SelectedExpResult; }
			set
			{
				if (this._SelectedExpResult != value)
				{
					this._SelectedExpResult = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		#endregion


		#region LBAS_Slots 변경통지 프로퍼티
		public ICollection<SlotItemInfoViewModel> _LBAS_Slots;
		public ICollection<SlotItemInfoViewModel> LBAS_Slots
		{
			get { return this._LBAS_Slots; }
			private set
			{
				if (this._LBAS_Slots != value)
				{
					this._LBAS_Slots = value;
					this.RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region SelectedLandBasedType 変更通知プロパティ
		private LBASActionType _SelectedLandBasedType;
		public LBASActionType SelectedLandBasedType
		{
			get { return this._SelectedLandBasedType; }
			set
			{
				if (this._SelectedLandBasedType != value)
				{
					this._SelectedLandBasedType = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		#endregion

		#region LBAS_AirSuperiorityPotential 変更通知プロパティ
		private double _LBAS_AirSuperiorityPotential;
		public double LBAS_AirSuperiorityPotential
		{
			get { return this._LBAS_AirSuperiorityPotential; }
			set
			{
				if (this._LBAS_AirSuperiorityPotential != value)
				{
					this._LBAS_AirSuperiorityPotential = value;
					this.RaisePropertyChanged();
					this.RaisePropertyChanged(nameof(LBAS_AirSuperiorityPotentialText));
				}
			}
		}

		public string LBAS_AirSuperiorityPotentialText => this.LBAS_AirSuperiorityPotential.ToString("0.#");
		#endregion

		#region LBAS_AttackPower 変更通知プロパティ
		private double _LBAS_AttackPower;
		public double LBAS_AttackPower
		{
			get { return this._LBAS_AttackPower; }
			set
			{
				if (this._LBAS_AttackPower != value)
				{
					this._LBAS_AttackPower = value;
					this.RaisePropertyChanged();
					this.RaisePropertyChanged(nameof(LBAS_AttackPowerText));
				}
			}
		}
		public string LBAS_AttackPowerText => this.LBAS_AttackPower.ToString("0.#");
		#endregion
		#region LBAS_AttackPower_vsLandBased 変更通知プロパティ
		private double _LBAS_AttackPower_vsLandBased;
		public double LBAS_AttackPower_vsLandBased
		{
			get { return this._LBAS_AttackPower_vsLandBased; }
			set
			{
				if (this._LBAS_AttackPower_vsLandBased != value)
				{
					this._LBAS_AttackPower_vsLandBased = value;
					this.RaisePropertyChanged();
					this.RaisePropertyChanged(nameof(LBAS_AttackPower_vsLandBasedText));
				}
			}
		}
		public string LBAS_AttackPower_vsLandBasedText => this.LBAS_AttackPower_vsLandBased.ToString("0.#");
		#endregion
		#region LBAS_AttackPower_JetPhase 変更通知プロパティ
		private double _LBAS_AttackPower_JetPhase;
		public double LBAS_AttackPower_JetPhase
		{
			get { return this._LBAS_AttackPower_JetPhase; }
			set
			{
				if (this._LBAS_AttackPower_JetPhase != value)
				{
					this._LBAS_AttackPower_JetPhase = value;
					this.RaisePropertyChanged();
					this.RaisePropertyChanged(nameof(LBAS_AttackPower_JetPhaseText));
				}
			}
		}
		public string LBAS_AttackPower_JetPhaseText => this.LBAS_AttackPower_JetPhase.ToString("0.#");
		#endregion

		#region LBAS_AttackPower_vsAbyssalAircraft 変更通知プロパティ
		private double _LBAS_AttackPower_vsAbyssalAircraft;
		public double LBAS_AttackPower_vsAbyssalAircraft
		{
			get { return this._LBAS_AttackPower_vsAbyssalAircraft; }
			set
			{
				if (this._LBAS_AttackPower_vsAbyssalAircraft != value)
				{
					this._LBAS_AttackPower_vsAbyssalAircraft = value;
					this.RaisePropertyChanged();
					this.RaisePropertyChanged(nameof(LBAS_AttackPower_vsAbyssalAircraftText));
				}
			}
		}
		public string LBAS_AttackPower_vsAbyssalAircraftText => this.LBAS_AttackPower_vsAbyssalAircraft.ToString("0.#");
		#endregion
		#region LBAS_AttackPower_vsArtilleryImp 変更通知プロパティ
		private double _LBAS_AttackPower_vsArtilleryImp;
		public double LBAS_AttackPower_vsArtilleryImp
		{
			get { return this._LBAS_AttackPower_vsArtilleryImp; }
			set
			{
				if (this._LBAS_AttackPower_vsArtilleryImp != value)
				{
					this._LBAS_AttackPower_vsArtilleryImp = value;
					this.RaisePropertyChanged();
					this.RaisePropertyChanged(nameof(LBAS_AttackPower_vsArtilleryImpText));
				}
			}
		}
		public string LBAS_AttackPower_vsArtilleryImpText => this.LBAS_AttackPower_vsArtilleryImp.ToString("0.#");
		#endregion
		#region LBAS_AttackPower_vsIsolatedIsland 変更通知プロパティ
		private double _LBAS_AttackPower_vsIsolatedIsland;
		public double LBAS_AttackPower_vsIsolatedIsland
		{
			get { return this._LBAS_AttackPower_vsIsolatedIsland; }
			set
			{
				if (this._LBAS_AttackPower_vsIsolatedIsland != value)
				{
					this._LBAS_AttackPower_vsIsolatedIsland = value;
					this.RaisePropertyChanged();
					this.RaisePropertyChanged(nameof(LBAS_AttackPower_vsIsolatedIslandText));
				}
			}
		}
		public string LBAS_AttackPower_vsIsolatedIslandText => this.LBAS_AttackPower_vsIsolatedIsland.ToString("0.#");
		#endregion
		#region LBAS_AttackPower_vsSupplyDepot 変更通知プロパティ
		private double _LBAS_AttackPower_vsSupplyDepot;
		public double LBAS_AttackPower_vsSupplyDepot
		{
			get { return this._LBAS_AttackPower_vsSupplyDepot; }
			set
			{
				if (this._LBAS_AttackPower_vsSupplyDepot != value)
				{
					this._LBAS_AttackPower_vsSupplyDepot = value;
					this.RaisePropertyChanged();
					this.RaisePropertyChanged(nameof(LBAS_AttackPower_vsSupplyDepotText));
				}
			}
		}
		public string LBAS_AttackPower_vsSupplyDepotText => this.LBAS_AttackPower_vsSupplyDepot.ToString("0.#");
		#endregion

		#region LBAS_Distance 変更通知プロパティ
		private int _LBAS_Distance;
		public int LBAS_Distance
		{
			get { return this._LBAS_Distance; }
			set
			{
				if (this._LBAS_Distance != value)
				{
					this._LBAS_Distance = value;
					this.RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region LBAS_Slot1 変更通知プロパティ
		private SlotItemInfoViewModel _LBAS_Slot1;
		public SlotItemInfoViewModel LBAS_Slot1
		{
			get { return this._LBAS_Slot1; }
			set
			{
				if (this._LBAS_Slot1 != value)
				{
					this._LBAS_Slot1 = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		#endregion
		#region LBAS_Slot2 変更通知プロパティ
		private SlotItemInfoViewModel _LBAS_Slot2;
		public SlotItemInfoViewModel LBAS_Slot2
		{
			get { return this._LBAS_Slot2; }
			set
			{
				if (this._LBAS_Slot2 != value)
				{
					this._LBAS_Slot2 = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		#endregion
		#region LBAS_Slot3 変更通知プロパティ
		private SlotItemInfoViewModel _LBAS_Slot3;
		public SlotItemInfoViewModel LBAS_Slot3
		{
			get { return this._LBAS_Slot3; }
			set
			{
				if (this._LBAS_Slot3 != value)
				{
					this._LBAS_Slot3 = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		#endregion
		#region LBAS_Slot4 変更通知プロパティ
		private SlotItemInfoViewModel _LBAS_Slot4;
		public SlotItemInfoViewModel LBAS_Slot4
		{
			get { return this._LBAS_Slot4; }
			set
			{
				if (this._LBAS_Slot4 != value)
				{
					this._LBAS_Slot4 = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		#endregion

		#region LBAS Level Values + Proficiency Values
		public LevelValueViewModel[] LBAS_Levels { get; }
		public ProficiencyValueViewModel[] LBAS_Proficiencies { get; }

		private LevelValueViewModel[] LBAS_LevelValues { get; }
			= new LevelValueViewModel[]
			{
				new LevelValueViewModel(0),
				new LevelValueViewModel(0),
				new LevelValueViewModel(0),
				new LevelValueViewModel(0),
			};
		private ProficiencyValueViewModel[] LBAS_ProficiencyValues { get; }
			= new ProficiencyValueViewModel[]
			{
				new ProficiencyValueViewModel(7),
				new ProficiencyValueViewModel(7),
				new ProficiencyValueViewModel(7),
				new ProficiencyValueViewModel(7),
			};

		#region Levels
		public LevelValueViewModel LBAS_Slot1_Level
		{
			get { return this.LBAS_LevelValues[0]; }
			set
			{
				if (this.LBAS_LevelValues[0] != value)
				{
					this.LBAS_LevelValues[0] = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		public LevelValueViewModel LBAS_Slot2_Level
		{
			get { return this.LBAS_LevelValues[1]; }
			set
			{
				if (this.LBAS_LevelValues[1] != value)
				{
					this.LBAS_LevelValues[1] = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		public LevelValueViewModel LBAS_Slot3_Level
		{
			get { return this.LBAS_LevelValues[2]; }
			set
			{
				if (this.LBAS_LevelValues[2] != value)
				{
					this.LBAS_LevelValues[2] = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		public LevelValueViewModel LBAS_Slot4_Level
		{
			get { return this.LBAS_LevelValues[3]; }
			set
			{
				if (this.LBAS_LevelValues[3] != value)
				{
					this.LBAS_LevelValues[3] = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		#endregion
		#region Proficiencys
		public ProficiencyValueViewModel LBAS_Slot1_Proficiency
		{
			get { return this.LBAS_ProficiencyValues[0]; }
			set
			{
				if (this.LBAS_ProficiencyValues[0] != value)
				{
					this.LBAS_ProficiencyValues[0] = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		public ProficiencyValueViewModel LBAS_Slot2_Proficiency
		{
			get { return this.LBAS_ProficiencyValues[1]; }
			set
			{
				if (this.LBAS_ProficiencyValues[1] != value)
				{
					this.LBAS_ProficiencyValues[1] = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		public ProficiencyValueViewModel LBAS_Slot3_Proficiency
		{
			get { return this.LBAS_ProficiencyValues[2]; }
			set
			{
				if (this.LBAS_ProficiencyValues[2] != value)
				{
					this.LBAS_ProficiencyValues[2] = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		public ProficiencyValueViewModel LBAS_Slot4_Proficiency
		{
			get { return this.LBAS_ProficiencyValues[3]; }
			set
			{
				if (this.LBAS_ProficiencyValues[3] != value)
				{
					this.LBAS_ProficiencyValues[3] = value;
					this.RaisePropertyChanged();
					this.UpdateCalculator();
				}
			}
		}
		#endregion
		#endregion

		public CalculatorViewModel()
		{
			this.Title = "수치 계산기";
			this.TabItems = new string[]
			{
				"목표 경험치",
				"연습전 경험치",
				"기항대 계산기"
			};
			this.SelectedTab = this.TabItems.FirstOrDefault();

			#region Update Sources
			this.UpdateSourceShipList
				.Do(_ => this.Reloading++)
				.Throttle(TimeSpan.FromMilliseconds(7.0))
				.Do(_ => this.UpdateShipList())
				.Do(_ => this.Reloading--)
				.Subscribe(_ => this.UpdateCalculator());
			this.CompositeDisposable.Add(this.UpdateSourceShipList);

			this.CompositeDisposable.Add(new PropertyChangedEventListener(this.homeport)
			{
				{ () => this.homeport.Organization, (_, __) => {
					this.CompositeDisposable.Add(new PropertyChangedEventListener(this.homeport.Organization)
					{
						{ () => this.homeport.Organization.Ships, (sender, args) => this.RequestUpdateShipList() },
					});
				} }
			});
			#endregion

			#region LBAS slot list
			this.LBAS_Slots =
				new SlotItemInfoViewModel[] { new SlotItemInfoViewModel(0, null) }
				.Concat(
					KanColleClient.Current.Master.SlotItems
						.Select(x => x.Value)
						.Where(x => x.IsNumerable)
						.OrderBy(x => x.IconType)
						.ThenBy(x => x.Id)
						.Select((x, i) => new SlotItemInfoViewModel(x.Id, x))
				)
				.ToArray();

			this.LBAS_Slot1 = LBAS_Slots.FirstOrDefault();
			this.LBAS_Slot2 = LBAS_Slots.FirstOrDefault();
			this.LBAS_Slot3 = LBAS_Slots.FirstOrDefault();
			this.LBAS_Slot4 = LBAS_Slots.FirstOrDefault();
			#endregion
			#region LBAS level & proficiency list
			LBAS_Levels = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
				.Select(x => new LevelValueViewModel(x))
				.ToArray();
			LBAS_Slot1_Level = LBAS_Slot2_Level = LBAS_Slot3_Level = LBAS_Slot4_Level
				= LBAS_Levels.FirstOrDefault();

			LBAS_Proficiencies = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }
				.Select(x => new ProficiencyValueViewModel(x))
				.ToArray();
			LBAS_Slot1_Proficiency = LBAS_Slot2_Proficiency = LBAS_Slot3_Proficiency = LBAS_Slot4_Proficiency
				= LBAS_Proficiencies.LastOrDefault();
			#endregion

			SelectedSea = SeaExpTable.Keys.FirstOrDefault();
			SelectedResult = ResultRanks.FirstOrDefault();
			SelectedExpResult = ResultRanks.FirstOrDefault();

			SelectedLandBasedType = LandBasedType.FirstOrDefault().Value;

			this.RequestUpdateShipList();
		}
		public void RequestUpdateShipList() => this.UpdateSourceShipList.OnNext(Unit.Default);

		/// <summary>
		/// Update ship list for calculator
		/// </summary>
		private void UpdateShipList()
		{
			var list = this.homeport.Organization.Ships.Values;

			this.Ships = list.OrderByDescending(x => x.Exp)
				.ThenBy(x => x.Id)
				.Select((x, i) => new ShipViewModel(i + 1, x, null))
				.ToList();
		}


		/// <summary>
		/// Update Calculator Display Values
		/// </summary>
		public void UpdateCalculator()
		{
			switch (this.SelectedTabIdx)
			{
				case 0:
					CalculateRemainingExp();
					break;

				case 1:
					CalculateTrainingExp();
					break;

				case 2:
					CalculateLandBased();
					break;
			}
		}

		/// <summary>
		/// Calculates experience with given parameters.
		/// Requires levels and experience to work with.
		/// </summary>
		private void CalculateRemainingExp()
		{
			if (this.TargetLevel < this.CurrentLevel || this.TargetExp < this.CurrentExp ||
				this.SelectedResult == null || this.SelectedSea == null)
				return;

			var RankTable = new Dictionary<string, double>
				{
					{"S", 1.2 },
					{"A", 1.0 },
					{"B", 1.0 },
					{"C", 0.8 },
					{"D", 0.7 },
					{"E", 0.5 }
				};

			// Lawl at that this inline conditional.
			double Multiplier = (this.IsFlagship ? 1.5 : 1) * (this.IsMVP ? 2 : 1) * RankTable[this.SelectedResult];

			this.SortieExp = (int)Math.Round(SeaExpTable[this.SelectedSea] * Multiplier, 0, MidpointRounding.AwayFromZero);
			this.RemainingExp = this.TargetExp - this.CurrentExp;
			this.RunCount = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(this.RemainingExp) / Convert.ToDecimal(this.SortieExp)));
		}

		/// <summary>
		/// Calculates expected exp for training with given parameters.
		/// </summary>
		private void CalculateTrainingExp()
		{
			if (this.SelectedExpResult == null)
				return;

			var RankTable = new Dictionary<string, double>
				{
					{"S", 1.2 },
					{"A", 1.0 },
					{"B", 1.0 },
					{"C", 0.64 },
					{"D", 0.56 },
					{"E", 0.4 }
				};
			var rankRate = RankTable[this.SelectedExpResult];
			var flagshipRate = 1.5;
			var mvpRate = 2.0;
			var tRate = 0.0;
			var tExp = 0;

			int baseExp = ExpTable[this.Training_Flagship_Lv] / 100;
			if (this.Training_Secondship) baseExp += ExpTable[this.Training_Secondship_Lv] / 300;

			if (baseExp > 500) baseExp = 500 + (int)Math.Floor(Math.Sqrt(baseExp - 500));
			baseExp = (int)(baseExp * rankRate);

			var FirstFleet = this.homeport.Organization.Fleets.FirstOrDefault().Value.Ships;
			var tShip = FirstFleet.Where(x => x.Info.ShipType.Id == 21).ToArray(); // 21 is Training Cruiser

			if (tShip.Length > 0)
			{
				if (tShip.Length == 1 && FirstFleet.FirstOrDefault().Info.ShipType.Id == 21)
					tRate = GetTrainingBonus(tShip[0].Level, 0); // Flagship only
				else if (tShip.Length == 1 && FirstFleet.FirstOrDefault().Info.ShipType.Id != 21)
					tRate = GetTrainingBonus(tShip[0].Level, 1); // One at Accompanies
				else if (tShip.Length > 1 && FirstFleet.FirstOrDefault().Info.ShipType.Id == 21)
					tRate = GetTrainingBonus(FirstFleet.FirstOrDefault().Level, 2); // Flagship and Accompany
				else if (tShip.Length > 1 && FirstFleet.FirstOrDefault().Info.ShipType.Id != 21)
					tRate = GetTrainingBonus(Math.Max(tShip[0].Level, tShip[1].Level), 3); // Two at Accompanies
			}
			tExp = (int)Math.Floor(tRate * baseExp / 100);

			this.Training_FlagshipExp = (int)(baseExp * flagshipRate);
			this.Training_FlagshipMvpExp = (int)(baseExp * flagshipRate * mvpRate);
			this.Training_AccshipExp = (int)(baseExp);
			this.Training_AccshipMvpExp = (int)(baseExp * mvpRate);
			this.Training_TrainingCruiser_Bonus = tExp;
		}

		/// <summary>
		/// Calculates training ship's bonus exp rate
		/// </summary>
		private double GetTrainingBonus(int tLevel, int rateType)
		{
			var rateIdx = 0;
			if (tLevel <= 9) rateIdx = 0;
			else if (tLevel <= 29) rateIdx = 1;
			else if (tLevel <= 59) rateIdx = 2;
			else if (tLevel <= 99) rateIdx = 3;
			else rateIdx = 4;

			double[][] rateTable = new double[][]
			{
				new double[] {5,  8,  12, 15, 20},  // Flagship only
				new double[] {3,  5,  7,  10, 15},  // One at Accompanies
				new double[] {10, 13, 16, 20, 25},  // Flagship and Accompany
				new double[] {4,  6,  8,  12, 17.5} // Two at Accompanies
			};
			return rateTable[rateType][rateIdx];
		}

		/// <summary>
		/// Calculates land-base aerial support's air superiority potential
		/// </summary>
		private void CalculateLandBased()
		{
			var items = new SlotItemInfoViewModel[]
				{
					new SlotItemInfoViewModel(this.LBAS_Slot1, 0),
					new SlotItemInfoViewModel(this.LBAS_Slot2, 1),
					new SlotItemInfoViewModel(this.LBAS_Slot3, 2),
					new SlotItemInfoViewModel(this.LBAS_Slot4, 3),
				}
				.Where(x => x.Display != null)
				.ToArray();


			LBAS_AttackPower = 0;
			LBAS_AirSuperiorityPotential = 0;
			LBAS_Distance = 0;
			if (items.Count() == 0) return;

			#region Calculate Distance
			LBAS_Distance = LBASCalculateDistance(items);
			#endregion

			#region Attack Power Calc
			LBAS_AttackPower
				= LBASCaltulateDamage(LBASDamageType.SurfaceVessel, false, false, LBASEnemyType.Default, items);
			LBAS_AttackPower_vsLandBased
				= LBASCaltulateDamage(LBASDamageType.LandBased, false, false, LBASEnemyType.Default, items);
			LBAS_AttackPower_JetPhase
				= LBASCaltulateDamage(LBASDamageType.JetPowered, false, false, LBASEnemyType.Default, items);

			LBAS_AttackPower_vsAbyssalAircraft
				= LBASCaltulateDamage(LBASDamageType.SurfaceVessel, false, false, LBASEnemyType.AbyssalAircraftPrincess, items);
			LBAS_AttackPower_vsArtilleryImp
				= LBASCaltulateDamage(LBASDamageType.LandBased, false, false, LBASEnemyType.ArtilleryImp, items);
			LBAS_AttackPower_vsIsolatedIsland
				= LBASCaltulateDamage(LBASDamageType.LandBased, false, false, LBASEnemyType.IsolatedIslandPrincess, items);
			LBAS_AttackPower_vsSupplyDepot
				= LBASCaltulateDamage(LBASDamageType.LandBased, false, false, LBASEnemyType.SupplyDepotPrincess, items);
			#endregion

			#region AA Calc
			LBAS_AirSuperiorityPotential = LBASCalculateAA(this.SelectedLandBasedType, items);
			#endregion
		}

		private int LBASCalculateDistance(IEnumerable<SlotItemInfoViewModel> slotitems)
		{
			var distanceBonus = new Dictionary<int, int[]>()
			{
				{ 138, new int[] { 3, 3, 3, 3, 3, 3, 3, 3 } }, // 이식대정
				{ 178, new int[] { 3, 3, 2, 2, 2, 2, 1, 1 } }, // 카탈리나
				{ 151, new int[] { 2, 2, 2, 2, 1, 1, 0, 0 } }, // 시제케이운
				{  54, new int[] { 2, 2, 2, 2, 1, 1, 0, 0 } }, // 사이운
				{  25, new int[] { 2, 2, 2, 1, 1, 0, 0, 0 } }, // 영식수상정찰기
				{  59, new int[] { 2, 2, 2, 1, 1, 0, 0, 0 } }, // 영식수상관측기
				{  61, new int[] { 2, 1, 1, 0, 0, 0, 0, 0 } }, // 2식함상정찰기
			};

			int distance = slotitems.Min(x => x.Info.Distance);

			if (slotitems.Any(x => distanceBonus.ContainsKey(x.Info.Id)))
			{
				var dist = Math.Max(0, Math.Min(7, distance - 2));
				distance += slotitems
					.Where(x => distanceBonus.ContainsKey(x.Info.Id))
					.Max(x => distanceBonus[x.Info.Id][dist]);
			}

			return distance;
		}
		private double LBASCaltulateDamage(LBASDamageType calcType, bool isCritical, bool isContacted, LBASEnemyType enemyType, IEnumerable<SlotItemInfoViewModel> slotitems)
		{
			var jet_powered = new SlotItemType[]
			{
				SlotItemType.噴式攻撃機,
				SlotItemType.噴式戦闘爆撃機,
			};
			var attackers = new SlotItemType[]
			{
				SlotItemType.艦上攻撃機,
				SlotItemType.艦上爆撃機,
				SlotItemType.噴式攻撃機,
				SlotItemType.噴式戦闘爆撃機,
				SlotItemType.陸上攻撃機,
				SlotItemType.水上爆撃機
			};
			var items = slotitems.Where(x => attackers.Contains(x.Info.Type)).ToArray();

			var typeMultiplier = new Func<SlotItemType, double>
			(
				type => (type == SlotItemType.艦上攻撃機 || type == SlotItemType.艦上爆撃機 || type == SlotItemType.水上爆撃機)
					? 1.0
					: type == SlotItemType.陸上攻撃機
						? 0.8
						: (type == SlotItemType.噴式戦闘爆撃機 || type == SlotItemType.噴式攻撃機)
							? 0.7071
							: 0
			);
			var proficiencyMultiplier = new Func<SlotItemType, int, double>
			(
				(type, proficiency) => type == SlotItemType.陸上攻撃機
					? 1.0
					: 1.0 + 0.2 * (proficiency / 7)
			);
			var calcCap = new Func<double, double, double>
			(
				(source, cap) =>
					source <= cap
						? source
						: cap + Math.Sqrt(source - cap)
			);

			switch (calcType)
			{
				case LBASDamageType.SurfaceVessel:
					return items
						.Sum(item =>
						{
							var proficiency = LBAS_ProficiencyValues[item.Index].Value;
							double damage = 0;

							// 최종공격력 = [[기본공격력] x 크리티컬보정 x 숙련도보정] x 촉접보정 x 육공보정 x 육공특효
							// 기본공격력 = 종별배율 x {(뇌격 or 폭격) x √(1.8 x 탑재량) + 25}
							// [] 반올림
							// 종별: 함공,함폭,수폭 = 1.0 / 육공 = 0.8 / 제트기 = 0.7071
							// 숙련도 보정: 육공 = 1.0(숙련 무관) / 육공이외 = 1.0~1.2 (숙련변동)
							// 육공 보정: 육공 = 1.8 / 이외 = 1.0

							var isLBAS = (item.Info.Type == SlotItemType.陸上攻撃機);
							var dmg_src = (item.Info.Type == SlotItemType.陸上攻撃機 || item.Info.Type == SlotItemType.艦上攻撃機)
								? item.Info.Torpedo
								: item.Info.Bomb;

							var carries = 18; // 고정
							var criticalMultiplier = isCritical ? 1.5 : 1.0;
							var contactMultiplier = isContacted ? 1.0 : 1.0; // 촉접 함재기마다 다름. 어차피 안쓰니 패스
							var LBAMultiplier = isLBAS ? 1.8 : 1.0;
							var LBASpecialMultiplier = enemyType == LBASEnemyType.AbyssalAircraftPrincess
								? 3.3 // 공모서희
								: 1.0; // 해당 없음

							damage = typeMultiplier(item.Info.Type) * (dmg_src * Math.Sqrt(1.8 * carries) + 25); // 기본 공격력
							damage = Math.Floor(Math.Floor(damage) * criticalMultiplier * proficiencyMultiplier(item.Info.Type, proficiency))
										* contactMultiplier * LBAMultiplier * LBASpecialMultiplier;

							return damage;
						});

				case LBASDamageType.LandBased:
					return items
						.Sum(item =>
						{
							var proficiency = LBAS_ProficiencyValues[item.Index].Value;
							double damage = 0;

							// 최종공격력 = [[[<기본공격력 x 기지항공특효(포대/이도서희)> x 폭격특효(집적지) + 기지항공특효(집적지)]
							//    x 폭격특효(포대/이도서희)] x 크리티컬 보정 x 숙련도 보정] x 촉접보정 x 육공보정
							// 기본공격력 = 종별배율 x {(뇌격 or 폭격) x √(1.8 x 탑재량) + 25}

							var isLBAS = (item.Info.Type == SlotItemType.陸上攻撃機);
							var isBomber = (item.Info.Type == SlotItemType.艦上爆撃機) || isLBAS; // 함폭 or 육공. 수폭, 분폭 제외

							var dmg_src = (item.Info.Type == SlotItemType.陸上攻撃機 || item.Info.Type == SlotItemType.艦上攻撃機)
								? item.Info.Torpedo
								: item.Info.Bomb;

							var carries = 18; // 고정
							var criticalMultiplier = isCritical ? 1.5 : 1.0;
							var contactMultiplier = isContacted ? 1.0 : 1.0; // 촉접 함재기마다 다름. 어차피 안쓰니 패스
							var LBAMultiplier = isLBAS ? 1.8 : 1.0;
							var LBASpecialMultiplier = enemyType == LBASEnemyType.AbyssalAircraftPrincess
								? 3.3 // 공모서희
								: 1.0; // 해당 없음

							// 포대소귀/이도서희 기지항공 특효 (곱셈)
							var enemyBonus1 = enemyType == LBASEnemyType.ArtilleryImp
								? 1.6
								: enemyType == LBASEnemyType.IsolatedIslandPrincess
									? 1.18
									: 1;
							// 집적지서희 기지항공 특효 (덧셈)
							var enemyBonus2 = enemyType == LBASEnemyType.SupplyDepotPrincess
								? 100
								: 0;

							// 포대소귀/이도서희 폭격 특효
							var bombBonus1 = enemyType == LBASEnemyType.ArtilleryImp
								? 1.55
								: enemyType == LBASEnemyType.IsolatedIslandPrincess
									? 1.7
									: 1;
							// 집적지서희 폭격 특효
							var bombBonus2 = enemyType == LBASEnemyType.SupplyDepotPrincess
								? 2.1
								: 1.0;

							if (!isBomber) // 폭격기가 아니면 폭격 특효 없음
								bombBonus1 = bombBonus2 = 1;

							damage = typeMultiplier(item.Info.Type) * (dmg_src * Math.Sqrt(1.8 * carries) + 25); // 기본 공격력
							damage = Math.Floor(calcCap(damage * enemyBonus1, 150) * bombBonus1 + enemyBonus2);
							damage = Math.Floor(damage * bombBonus2);
							damage = Math.Floor(damage * criticalMultiplier * proficiencyMultiplier(item.Info.Type, proficiency));
							damage *= contactMultiplier * LBAMultiplier;

							return damage;
						});

				case LBASDamageType.JetPowered:
					return items
						.Where(x => jet_powered.Contains(x.Info.Type))
						.Sum(item =>
						{
							double damage = 0;

							var dmg_src = item.Info.Bomb;

							var carries = 18; // 고정
							var criticalMultiplier = isCritical ? 1.5 : 1.0;

							damage = Math.Floor(dmg_src * Math.Sqrt(1.8 * carries) + 25);
							damage = Math.Floor(damage * criticalMultiplier);

							return damage;
						});
			}
			return 0;
		}
		private double LBASCalculateAA(LBASActionType actionType, IEnumerable<SlotItemInfoViewModel> slotitems)
		{
			var proficiencies = new Dictionary<int, Proficiency>()
			{
				{ 0, new Proficiency(  0,   9,  0,  0) },
				{ 1, new Proficiency( 10,  24,  0,  0) },
				{ 2, new Proficiency( 25,  39,  2,  1) },
				{ 3, new Proficiency( 40,  54,  5,  1) },
				{ 4, new Proficiency( 55,  69,  9,  1) },
				{ 5, new Proficiency( 70,  84, 14,  3) },
				{ 6, new Proficiency( 85,  99, 14,  3) },
				{ 7, new Proficiency(100, 120, 22,  6) },
			};

			var aa_sum = slotitems
				.Sum(item =>
				{
					var proficiencyData = proficiencies[LBAS_ProficiencyValues[item.Index].Value];
					var level = LBAS_LevelValues[item.Index].Value;

					double aa = item.Info.AA;
					double interception = item.Info.Evade;
					double anti_bomber = item.Info.Hit;

					if (item.Info.Type != SlotItemType.局地戦闘機)
					{
						interception = 0;
						anti_bomber = 0;
					}

					var aa_level_able = new SlotItemType[]
						{
							SlotItemType.艦上戦闘機,
							SlotItemType.水上戦闘機,
							SlotItemType.噴式戦闘機,
							SlotItemType.噴式戦闘爆撃機,
							SlotItemType.局地戦闘機,
						};
					var aa_level_fighterbombers = new int[]
					{
						60, // 62형 폭전
						219, // 63형 폭전
						154, // 62형 폭전 (이와이)
					};

					if (aa_level_able.Contains(item.Info.Type))
						aa += level * 0.2;
					else if (aa_level_fighterbombers.Contains(item.Info.Id))
						aa += level * 0.25;

					double proficiency = (item.Info.Type == SlotItemType.水上爆撃機)
						? proficiencyData.SeaplaneBomberBonus
						: (item.Info.Type == SlotItemType.艦上戦闘機 || item.Info.Type == SlotItemType.水上戦闘機 || item.Info.Type == SlotItemType.局地戦闘機)
							? proficiencyData.FighterBonus
							: 0;
					proficiency += Math.Sqrt(0.1 * proficiencyData.GetInternalValue(AirSuperiorityCalculationOptions.InternalProficiencyMaxValue));

					/*
					 * 출격 식 : [(대공 + 1.5 x 영격) x √(탑재량) + 숙련도보정]
					 * 
					 * 방공 식: [(중대 방공 합) x 정찰기보정]
					 * 중대 방공: [(대공 + 영격 + 대폭 x 2) x √(탑재량) + 숙련도보정]
					 * 
					 * 숙련도 보정: (숙련도 값)+√12
					 * ※ 12 = 내부 숙련도 (0~120) / 10, 항상 최대치로 가정
					 */

					switch (actionType)
					{
						case LBASActionType.Attack:
							return Math.Floor((aa + 1.5 * interception) * Math.Sqrt(18) + proficiency);

						case LBASActionType.Defence:
							return Math.Floor((aa + interception + anti_bomber * 2) * Math.Sqrt(18) + proficiency);

						default:
							return 0;
					}
				});

			#region Bonus rate calculate when Air Defence Mode
			if (actionType == LBASActionType.Defence)
			{
				var bonusRate = 1.0;
				if (slotitems.Any(x => x.Info.Type == SlotItemType.艦上偵察機)) // 함상정찰기
				{
					var viewrange = slotitems
						.Where(x => x.Info.Type == SlotItemType.艦上偵察機)
						.Max(x => x.Info.ViewRange);

					if (viewrange <= 7)
						bonusRate = 1.2;
					else if (viewrange == 8)
						bonusRate = 1.25; // Maybe?
					else
						bonusRate = 1.3;
				}

				// 수상정찰기 or 대형비행정
				else if (slotitems.Any(x => x.Info.Type == SlotItemType.水上偵察機 || x.Info.Type == SlotItemType.大型飛行艇))
				{
					var viewrange = slotitems
						.Where(x => x.Info.Type == SlotItemType.水上偵察機 || x.Info.Type == SlotItemType.大型飛行艇)
						.Max(x => x.Info.ViewRange);

					if (viewrange <= 7)
						bonusRate = 1.1;
					else if (viewrange == 8)
						bonusRate = 1.13;
					else
						bonusRate = 1.16;
				}

				aa_sum = Math.Floor(aa_sum * bonusRate);
			}
			#endregion

			return aa_sum;
		}

		private enum LBASDamageType
		{
			SurfaceVessel, // 수상함
			LandBased, // 육상기지
			JetPowered, // 분식
		}
		private enum LBASEnemyType
		{
			Default, // 해당 없음
			AbyssalAircraftPrincess, // 공모서희
			ArtilleryImp, // 포대소귀
			IsolatedIslandPrincess, // 이도서희
			SupplyDepotPrincess, // 집적지서희
		}
		private class Proficiency
		{
			private int internalMinValue { get; }
			private int internalMaxValue { get; }

			public int FighterBonus { get; }
			public int SeaplaneBomberBonus { get; }

			public Proficiency(int internalMin, int internalMax, int fighterBonus, int seaplaneBomberBonus)
			{
				this.internalMinValue = internalMin;
				this.internalMaxValue = internalMax;
				this.FighterBonus = fighterBonus;
				this.SeaplaneBomberBonus = seaplaneBomberBonus;
			}

			/// <summary>
			/// 内部熟練度値を取得します。
			/// </summary>
			public int GetInternalValue(AirSuperiorityCalculationOptions options)
			{
				if (options.HasFlag(AirSuperiorityCalculationOptions.InternalProficiencyMinValue)) return this.internalMinValue;
				if (options.HasFlag(AirSuperiorityCalculationOptions.InternalProficiencyMaxValue)) return this.internalMaxValue;
				return (this.internalMaxValue + this.internalMinValue) / 2; // <- めっちゃ適当
			}
		}
	}

	public enum LBASActionType
	{
		Attack, // 출격
		Defence // 방공
	}

	public class SlotItemInfoViewModel : Livet.ViewModel
	{
		public int Key { get; }
		public SlotItemInfo Display { get; }
		public int Index { get; }

		public SlotItemInfo Info => this.Display;

		public SlotItemInfoViewModel(int Key, SlotItemInfo Display, int Index = -1)
		{
			this.Key = Key;
			this.Display = Display;
			this.Index = Index;
		}
		public SlotItemInfoViewModel(SlotItemInfoViewModel origin, int Index = -1)
			: this(origin.Key, origin.Display, Index == -1 ? origin.Index : Index)
		{ }
	}
	public class LevelValueViewModel : Livet.ViewModel
	{
		public int Value { get; }
		public LevelValueViewModel(int Value)
		{
			this.Value = Value < 0
				? 0
				: Value > 10
					? 10
					: Value;
		}
	}
	public class ProficiencyValueViewModel : Livet.ViewModel
	{
		public int Value { get; }
		public ProficiencyValueViewModel(int Value)
		{
			this.Value = Value < 0
				? 0
				: Value > 7
					? 7
					: Value;
		}
	}
	public class LBASActionTypeViewModel  : Livet.ViewModel
	{
		public LBASActionType Value { get; }
		public string Display { get; }

		public LBASActionTypeViewModel(LBASActionType Action, string Display)
		{
			this.Value = Action;
			this.Display = Display;
		}
	}
}
