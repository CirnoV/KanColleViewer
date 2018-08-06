using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlotItemType = Grabacr07.KanColleWrapper.Models.SlotItemType;

using Grabacr07.KanColleWrapper.Models.Raw;

namespace Grabacr07.KanColleViewer.QuestTracker.Models
{
	public class BattleCalculator
	{
		/// <summary>
		/// 전체 전투 기록
		/// </summary>
		public DamageLog[] AllDamageLog => this._AllDamageLog?.ToArray();
		private List<DamageLog> _AllDamageLog { get; set; }

		/// <summary>
		/// 초기화 및 갱신을 한 후에 호출됨
		/// </summary>
		public event EventHandler Updated;

		#region Definitions
		/// <summary>
		/// 전투 페이즈 목록
		/// </summary>
		public enum BattlePhase
		{
			/// <summary>
			/// 정의되지 않은 경우 (초기값 등)
			/// </summary>
			none,

			/// <summary>
			/// 기지항공대 분식 항공전
			/// </summary>
			airbase_injection,

			/// <summary>
			/// 분식 항공전
			/// </summary>
			injection_kouku,

			/// <summary>
			/// 기지항공대 항공전
			/// </summary>
			airbase_attack,

			/// <summary>
			/// 항공전
			/// </summary>
			kouku,

			/// <summary>
			/// 2차 항공전
			/// </summary>
			kouku2,

			/// <summary>
			/// 지원함대
			/// </summary>
			support,

			/// <summary>
			/// 선제 대잠
			/// </summary>
			opening_taisen,

			/// <summary>
			/// 개막 뇌격
			/// </summary>
			opening_atack,

			/// <summary>
			/// 야간 포격전
			/// </summary>
			hougeki,

			/// <summary>
			/// 야간 1차 포격전
			/// </summary>
			n_hougeki1,

			/// <summary>
			/// 야간 2차 포격전
			/// </summary>
			n_hougeki2,

			/// <summary>
			/// 1차 포격전
			/// </summary>
			hougeki1,

			/// <summary>
			/// 2차 포격전
			/// </summary>
			hougeki2,

			/// <summary>
			/// 3차 포격전
			/// </summary>
			hougeki3,

			/// <summary>
			/// 폐막 뇌격전
			/// </summary>
			raigeki
		}

		/// <summary>
		/// 공격 형태
		/// </summary>
		public enum DamageType
		{
			/// <summary>
			/// 없는 값
			/// </summary>
			None = -1,

			/// <summary>
			/// 일반 공격
			/// </summary>
			Normal = 0x00,

			/// <summary>
			/// 레이저 공격 (폐기됨)
			/// </summary>
			Laser = 0x01,

			/// <summary>
			/// 연격
			/// </summary>
			Twice = 0x02,

			/// <summary>
			/// 주포 부포 컷인
			/// </summary>
			MainGun_SubGun_Cutin = 0x03,

			/// <summary>
			/// 주포 전탐 컷인
			/// </summary>
			MainGun_Radar_Cutin = 0x04,

			/// <summary>
			/// 주포 철갑탄 컷인
			/// </summary>
			MainGun_Shell_Cutin = 0x05,

			/// <summary>
			/// 주포 주포 컷인
			/// </summary>
			MainGun_MainGun_Cutin = 0x06,

			/// <summary>
			/// 항공모함 컷인
			/// </summary>
			Airstrike_Cutin = 0x07,

			/// <summary>
			/// 주포 어뢰 컷인
			/// </summary>
			MainGun_Torpedo_Cutin = 0x101,

			/// <summary>
			/// 어뢰 어뢰 컷인
			/// </summary>
			Torpedo_Torpedo_Cutin = 0x102,

			/// <summary>
			/// 주포 주포 부포 컷인
			/// </summary>
			MainGun_MainGun_SubGun_Cutin = 0x103,

			/// <summary>
			/// 주포 주포 주포 컷인
			/// </summary>
			MainGun_MainGun_MainGun_Cutin = 0x104,

			/// <summary>
			/// 주포 어뢰 전탐 컷인 (구축)
			/// </summary>
			MainGun_Torpedo_Radar_Cutin = 0x105,

			/// <summary>
			/// 어뢰 견시원 전탐 컷인 (구축)
			/// </summary>
			Torpedo_Lookouts_Radar_Cutin = 0x106,
		}

		/// <summary>
		/// vs심해연합함대 전투 포격전 종류
		/// </summary>
		public enum EachCombinedPhase
		{
			/// <summary>
			/// 1함대 공격
			/// </summary>
			FirstPhase = 1,

			/// <summary>
			/// 2함대 공격
			/// </summary>
			SecondPhase = 2,

			/// <summary>
			/// 전부 공격
			/// </summary>
			AllPhase = 3
		}

		/// <summary>
		/// 연합함대vs단일함대 전투 포격전 종류
		/// </summary>
		public enum CombinedPhase
		{
			/// <summary>
			/// 첫번째 포격전
			/// </summary>
			FirstPhase = 1,

			/// <summary>
			/// 두번째 포격전
			/// </summary>
			SecondPhase = 2,

			/// <summary>
			/// 세번째 포격전
			/// </summary>
			ThirdPhase = 3
		}

		/// <summary>
		/// 연합함대 종류
		/// </summary>
		public enum CombinedType
		{
			/// <summary>
			/// 연합 아님
			/// </summary>
			None,

			/// <summary>
			/// 공모기동부대, 수송연합부대
			/// </summary>
			Default,

			/// <summary>
			/// 수상타격부대
			/// </summary>
			Water
		}

		/// <summary>
		/// 주거나 받은 데미지 로그
		/// </summary>
		public struct DamageLog
		{
			/// <summary>
			/// 고유 번호
			/// </summary>
			public int Uid { get; }

			/// <summary>
			/// 데미지를 준 로그인지 여부
			/// </summary>
			public bool IsDealt { get; set; }

			/// <summary>
			/// 주거나 받은 데미지 량
			/// </summary>
			public int Damage { get; set; }

			/// <summary>
			/// 소유자의 함선 번호
			/// </summary>
			public int Index { get; set; }

			/// <summary>
			/// 상대 함선 번호
			/// </summary>
			public int Target { get; set; }

			/// <summary>
			/// 주거나 받은 공격의 형식
			/// </summary>
			public DamageType Type { get; set; }

			/// <summary>
			/// 이 데미지가 발생한 페이즈
			/// </summary>
			public BattlePhase Phase { get; set; }

			/// <summary>
			/// 이 데미지를 받거나 준 후의 현재 함선 정보
			/// </summary>
			public ShipData SourceShip { get; set; }

			/// <summary>
			/// 이 데미지를 받거나 준 후의 상대 함선 정보
			/// </summary>
			public ShipData TargetShip { get; set; }

			/// <summary>
			/// 이 데미지에서 다메콘을 사용했는지 여부
			/// </summary>
			public bool DameconUsed { get; set; }

			public DamageLog(
				bool IsDealt, int Damage, int Index, int Target,
				DamageType Type, BattlePhase Phase, bool DameconUsed,
				ShipData SourceShip, ShipData TargetShip
			)
			{
				this.Uid = new Random().Next();

				this.IsDealt = IsDealt;
				this.Damage = Damage;
				this.Index = Index;
				this.Target = Target;
				this.Type = Type;
				this.Phase = Phase;
				this.DameconUsed = DameconUsed;
				this.SourceShip = SourceShip;
				this.TargetShip = TargetShip;
			}
		}

		/// <summary>
		/// 전투 결과 정보 총합 (받은 데미지, 준 데미지, 페이즈별 데미지 등)
		/// </summary>
		public class ShipBattleInfo
		{
			/// <summary>
			/// 함선 위치
			/// </summary>
			public int Index { get; }

			/// <summary>
			/// 함선 정보
			/// </summary>
			public ShipData Source { get; }

			/// <summary>
			/// 모든 데미지 정보
			/// </summary>
			public List<DamageLog> DamageList { get; private set; }

			/// <summary>
			/// 총 받은 데미지
			/// </summary>
			public int TotalDamaged => this.DamageList.Where(x => !x.IsDealt).Sum(x => x.Damage);

			/// <summary>
			/// 체력 변동 수치
			/// </summary>
			public int HPChangedValue => (this.Source?.BeforeNowHP - Math.Max(0, this.Source?.NowHP ?? 0)) ?? 0;

			/// <summary>
			/// 총 준 데미지
			/// </summary>
			public int TotalDealt => this.DamageList.Where(x => x.IsDealt).Sum(x => x.Damage);

			/// <summary>
			/// <see cref="ShipBattleInfo"/> 객체가 생성된 계산기
			/// </summary>
			private BattleCalculator Parent { get; }

			private ShipBattleInfo(BattleCalculator Parent)
			{
				this.Parent = Parent;
			}

			private ShipBattleInfo(BattleCalculator Parent, ShipBattleInfo source) : this(Parent)
			{
				this.Index = source.Index;
				this.Source = source.Source.Clone();
				this.DamageList = source.DamageList?.ToList();
			}
			public ShipBattleInfo(BattleCalculator Parent, int Index, ShipData Source) : this(Parent)
			{
				this.Index = Index;
				this.Source = Source;
				this.DamageList = new List<DamageLog>();
			}

			public ShipBattleInfo Clone()
			{
				return new ShipBattleInfo(this.Parent, this);
			}

			/// <summary>
			/// 데미지를 받은 경우의 처리
			/// </summary>
			/// <param name="Damage">받은 데미지</param>
			/// <param name="Phase">데미지가 발생한 전투 페이즈</param>
			/// <param name="From">데미지를 준 주체. 함선중에 없거나 알 수 없는 경우 -1</param>
			public void Damaged(decimal Damage, BattlePhase Phase, ShipBattleInfo From, DamageType Type) => this.Damaged((int)Damage, Phase, From, Type);
			private void Damaged(int Damage, BattlePhase Phase, ShipBattleInfo From, DamageType Type)
			{
				var DameconUsed = false;

				this.Source.NowHP -= Damage;
				if (this.Source.NowHP <= 0)
				{
					// 다메콘에 의한 회복 처리. 같은 전투에서 두 번 사용되지 않는다는 전제
					// 다메콘 우선도: 확장 슬롯 -> 인덱스 순
					var dameconState = new
					{
						HasDamecon = HasDamecon(this.Source),
						HasMegami = HasMegami(this.Source)
					};
					this.Source.IsUsedDamecon = (dameconState.HasDamecon || dameconState.HasMegami);

					// 응급수리요원 사용 (체력 20%)
					if (dameconState.HasDamecon)
						this.Source.NowHP = (int)Math.Floor(this.Source.MaxHP * 0.2);
					// 응급수리여신 사용 (체력 100%)
					else if (dameconState.HasMegami)
						this.Source.NowHP = this.Source.MaxHP;

					if (this.Source.IsUsedDamecon)
						DameconUsed = true;
				}

				// 데미지 로그 작성
				var log = new DamageLog
				(
					false, // IsDealt
					Damage,
					this.Index - 1,
					(From?.Index - 1) ?? -1,
					Type,
					Phase,
					DameconUsed,
					this.Source.Clone(),
					From?.Source?.Clone()
				);
				this.DamageList.Add(log);
				this.Parent._AllDamageLog.Add(log);
			}

			/// <summary>
			/// 데미지를 준 경우의 처리
			/// </summary>
			/// <param name="Damage">준 데미지</param>
			/// <param name="Phase">데미지가 발생한 전투 페이즈</param>
			public void Dealt(decimal Damage, BattlePhase Phase, DamageType Type) => this.Dealt((int)Damage, Phase, Type);
			private void Dealt(int Damage, BattlePhase Phase, DamageType Type)
			{
				var log = new DamageLog
				(
					true, // IsDealt
					Damage,
					this.Index - 1,
					-1, // Target
					Type,
					Phase,
					false,
					this.Source.Clone(),
					null
				);

				// 데미지 로그 작성
				this.DamageList.Add(log);
				this.Parent._AllDamageLog.Add(log);
			}


			private bool HasDamecon(ShipData ship)
			{
				if (ship == null) return false;

				var id = ship.ExSlot?.Source?.Id;
				if (id.HasValue) return id.Value == 42;

				return FirstDameconOrNull(ship)?.Source?.Id == 42;
			}
			private bool HasMegami(ShipData ship)
			{
				if (ship == null) return false;

				var id = ship.ExSlot?.Source?.Id;
				if (id.HasValue) return id.Value == 43;

				return FirstDameconOrNull(ship)?.Source?.Id == 43;
			}
			private ShipSlotData FirstDameconOrNull(ShipData ship)
				=> ship?.Slots?.FirstOrDefault(x => x?.Source?.Id == 42 || x?.Source?.Id == 43);
		}

		/// <summary>
		/// 전투 정보만 모아놓은 클래스
		/// </summary>
		private class CommonBattleData
		{
			public kcsapi_battle_airbase_injection api_air_base_injection { get; set; }
			public kcsapi_battle_airbase_attack[] api_air_base_attack { get; set; }
			public kcsapi_battle_kouku api_injection_kouku { get; set; }
			public kcsapi_battle_kouku api_kouku { get; set; }
			public kcsapi_battle_kouku api_kouku2 { get; set; }
			public kcsapi_battle_support_info api_support_info { get; set; }
			public kcsapi_battle_support_info api_n_support_info { get; set; }
			public kcsapi_battle_hougeki api_opening_taisen { get; set; }
			public kcsapi_battle_raigeki api_opening_atack { get; set; }
			public kcsapi_battle_midnight_hougeki api_hougeki { get; set; }
			public kcsapi_battle_hougeki api_n_hougeki1 { get; set; }
			public kcsapi_battle_hougeki api_n_hougeki2 { get; set; }
			public kcsapi_battle_hougeki api_hougeki1 { get; set; }
			public kcsapi_battle_hougeki api_hougeki2 { get; set; }
			public kcsapi_battle_hougeki api_hougeki3 { get; set; }
			public kcsapi_battle_raigeki api_raigeki { get; set; }

			public int[] api_active_deck { get; set; }
		}

		/// <summary>
		/// 연합함대 전투 목록
		/// </summary>
		private static ApiTypes[] CombinedBattles = new ApiTypes[]
		{
			ApiTypes.combined_battle_ec, // 아군 연합
			ApiTypes.combined_battle_each, // 아군 적 연합
			ApiTypes.combined_battle_each_water, // 아군 적 연합 (수상)

			ApiTypes.combined_battle_midnight_ec, // 연합 야전
		};
		#endregion

		#region Variables
		/// <summary>
		/// 아군 1함대 함선 정보
		/// </summary>
		public ShipBattleInfo[] AliasFirstShips { get; private set; }

		/// <summary>
		/// 아군 2함대 함선 정보
		/// </summary>
		public ShipBattleInfo[] AliasSecondShips { get; private set; }

		/// <summary>
		/// 적군 1함대 함선 정보
		/// </summary>
		public ShipBattleInfo[] EnemyFirstShips { get; private set; }

		/// <summary>
		/// 적군 2함대 함선 정보
		/// </summary>
		public ShipBattleInfo[] EnemySecondShips { get; private set; }

		/// <summary>
		/// 아군 연합함대 여부
		/// </summary>
		public bool IsCombined => this.AliasSecondShips?.Length > 0;

		/// <summary>
		/// 적군 연합함대 여부
		/// </summary>
		public bool IsEnemyCombined => this.EnemySecondShips?.Length > 0;

		/// <summary>
		/// 아군 1함대 MVP
		/// </summary>
		public int MVP_First { get; private set; } = 0;

		/// <summary>
		/// 아군 2함대 MVP
		/// </summary>
		public int MVP_Second { get; private set; } = 0;
		#endregion

		public BattleCalculator()
		{
			this._AllDamageLog = new List<DamageLog>();
		}

		/// <summary>
		/// 같은 값을 가지는 새 객체를 반환
		/// </summary>
		/// <returns></returns>
		public BattleCalculator Clone()
		{
			return new BattleCalculator
			{
				AliasFirstShips = this.AliasFirstShips?.Select(x => x?.Clone()).ToArray(),
				AliasSecondShips = this.AliasSecondShips?.Select(x => x?.Clone()).ToArray(),
				EnemyFirstShips = this.EnemyFirstShips?.Select(x => x?.Clone()).ToArray(),
				EnemySecondShips = this.EnemySecondShips?.Select(x => x?.Clone()).ToArray(),

				MVP_First = this.MVP_First,
				MVP_Second = this.MVP_Second,

				_AllDamageLog = this._AllDamageLog.ToList()
			};
		}

		/// <summary>
		/// 초기화
		/// </summary>
		public BattleCalculator Initialize(FleetData aliasFirst, FleetData aliasSecond, FleetData enemyFirst, FleetData enemySecond)
		{
			var aliasShipsFirst = aliasFirst.Ships.ToArray();
			var enemyShipsFirst = enemyFirst.Ships.ToArray();

			// 아군함대
			if (aliasSecond == null || aliasSecond.Ships.Count() == 0)
			{
				// 단일함대
				this.AliasFirstShips = new ShipBattleInfo[7];
				this.AliasSecondShips = null;

				for (var i = 0; i < aliasShipsFirst.Length; i++)
					this.AliasFirstShips[i] = new ShipBattleInfo(this, i + 1, aliasShipsFirst[i]);
			}
			else
			{
				var aliasShipsSecond = aliasSecond.Ships.ToArray();

				// 연합함대
				this.AliasFirstShips = new ShipBattleInfo[7];
				this.AliasSecondShips = new ShipBattleInfo[7];

				for (var i = 0; i < aliasShipsFirst.Length; i++)
					this.AliasFirstShips[i] = new ShipBattleInfo(this, i + 1, aliasShipsFirst[i]);

				for (var i = 0; i < aliasShipsSecond.Length; i++)
					this.AliasSecondShips[i] = new ShipBattleInfo(this, 6 + i + 1, aliasShipsSecond[i]);
			}

			// 적군함대
			if (enemySecond == null || enemySecond.Ships.Count() == 0)
			{
				// 단일함대
				this.EnemyFirstShips = new ShipBattleInfo[7];
				this.EnemySecondShips = null;

				for (var i = 0; i < enemyShipsFirst.Length; i++)
					this.EnemyFirstShips[i] = new ShipBattleInfo(this, i + 1, enemyShipsFirst[i]);
			}
			else
			{
				var enemyShipsSecond = enemySecond.Ships.ToArray();

				// 연합함대
				this.EnemyFirstShips = new ShipBattleInfo[7];
				this.EnemySecondShips = new ShipBattleInfo[7];

				for (var i = 0; i < enemyShipsFirst.Length; i++)
					this.EnemyFirstShips[i] = new ShipBattleInfo(this, i + 1, enemyShipsFirst[i]);

				for (var i = 0; i < enemyShipsSecond.Length; i++)
					this.EnemySecondShips[i] = new ShipBattleInfo(this, 6 + i + 1, enemyShipsSecond[i]);
			}

			// 전체 전투 기록 초기화
			this._AllDamageLog.Clear();

			// Event
			Updated?.Invoke(this, System.EventArgs.Empty);

			/// Chaining
			return this;
		}

		/// <summary>
		/// 전투 결과를 계산
		/// </summary>
		/// <param name="type">전투 형식</param>
		/// <param name="data">전투 데이터</param>
		/// <returns></returns>
		private BattleCalculator Update(ApiTypes type, CommonBattleData data)
		{
			var isCombined = CombinedBattles.Contains(type);

			// 기지항공대 분식 항공전
			this.UpdateAirBattle(data.api_air_base_injection, BattlePhase.airbase_injection);

			// 기지항공대 항공전
			if (data.api_air_base_attack != null)
			{
				foreach (var _data in data.api_air_base_attack)
					this.UpdateAirBattle(_data, BattlePhase.airbase_attack);
			}

			// 분식 항공전
			this.UpdateAirBattle(data.api_injection_kouku, BattlePhase.injection_kouku);

			// 항공전
			this.UpdateAirBattle(data.api_kouku, BattlePhase.kouku);

			// 항공전 (2차)
			this.UpdateAirBattle(data.api_kouku2, BattlePhase.kouku2);

			// 지원함대 (주간, 야간)
			this.UpdateSupport(data.api_support_info, BattlePhase.support);
			this.UpdateSupport(data.api_n_support_info, BattlePhase.support);

			// 선제 대잠
			this.UpdateHougeki(data.api_opening_taisen, BattlePhase.opening_taisen, EachCombinedPhase.AllPhase, CombinedType.None);

			// 개막 뇌격
			this.UpdateTorpedo(data.api_opening_atack, BattlePhase.opening_atack);

			// 포격전
			switch (type)
			{
				case ApiTypes.sortie_battle_midnight:
				case ApiTypes.sortie_battle_midnight_sp:
				case ApiTypes.practice_midnight_battle:
					this.UpdateMidnight(data.api_hougeki, BattlePhase.hougeki);
					break;

				case ApiTypes.sortie_battle:
				case ApiTypes.practice_battle:
					this.UpdateHougeki(data.api_hougeki1, BattlePhase.hougeki1);
					this.UpdateHougeki(data.api_hougeki2, BattlePhase.hougeki2);
					this.UpdateHougeki(data.api_hougeki3, BattlePhase.hougeki3);
					break;

				case ApiTypes.combined_battle:
				case ApiTypes.combined_battle_water:
				{
					var combined_type =
						type == ApiTypes.combined_battle ? CombinedType.Default
							: type == ApiTypes.combined_battle_water ? CombinedType.Water
							: CombinedType.None;

					this.UpdateHougeki(data.api_hougeki1, BattlePhase.hougeki1, CombinedPhase.FirstPhase, combined_type);
					this.UpdateHougeki(data.api_hougeki2, BattlePhase.hougeki2, CombinedPhase.SecondPhase, combined_type);
					this.UpdateHougeki(data.api_hougeki3, BattlePhase.hougeki3, CombinedPhase.ThirdPhase, combined_type);
					break;
				}

				case ApiTypes.combined_battle_ec:
				case ApiTypes.combined_battle_each:
				case ApiTypes.combined_battle_each_water:
				{
					var combined_type =
						type == ApiTypes.combined_battle_each ? CombinedType.Default
							: type == ApiTypes.combined_battle_each_water ? CombinedType.Water
							: CombinedType.None;

					EachCombinedPhase phase1, phase2, phase3;

					if (combined_type == CombinedType.Default)
					{
						phase1 = EachCombinedPhase.FirstPhase;
						phase2 = EachCombinedPhase.SecondPhase;
						phase3 = EachCombinedPhase.AllPhase;
					}
					else if (combined_type == CombinedType.Water)
					{
						phase1 = EachCombinedPhase.FirstPhase;
						phase2 = EachCombinedPhase.AllPhase;
						phase3 = EachCombinedPhase.SecondPhase;
					}
					else
					{
						phase1 = EachCombinedPhase.SecondPhase;
						phase2 = EachCombinedPhase.FirstPhase;
						phase3 = EachCombinedPhase.AllPhase;
					}

					this.UpdateHougeki(data.api_hougeki1, BattlePhase.hougeki1, phase1, combined_type);
					this.UpdateHougeki(data.api_hougeki2, BattlePhase.hougeki2, phase2, combined_type);
					this.UpdateHougeki(data.api_hougeki3, BattlePhase.hougeki3, phase3, combined_type);
					break;
				}

				case ApiTypes.combined_battle_midnight:
				case ApiTypes.combined_battle_midnight_sp:
					this.UpdateMidnight(data.api_hougeki, BattlePhase.hougeki);
					break;

				case ApiTypes.combined_battle_midnight_ec:
					this.UpdateMidnight(data.api_hougeki, BattlePhase.hougeki, data.api_active_deck[1]);
					break;
			}

			// 폐막 뇌격
			this.UpdateTorpedo(data.api_raigeki, BattlePhase.raigeki);


			// MVP 산출
			this.MVP_First = this.AliasFirstShips
				?.Where(x => x != null)
				?.OrderByDescending(x => x.TotalDealt)
				?.FirstOrDefault()
				?.Index ?? 0;

			this.MVP_Second = this.AliasSecondShips
				?.Where(x => x != null)
				?.OrderByDescending(x => x.TotalDealt)
				?.FirstOrDefault()
				?.Index ?? 0;

			// Event
			Updated?.Invoke(this, System.EventArgs.Empty);

			// Chaining
			return this;
		}

		/// <summary>
		/// 전투 결과를 계산 (주간->야간전 심해연합)
		/// </summary>
		/// <param name="type">전투 형식</param>
		/// <param name="data">전투 데이터</param>
		/// <returns></returns>
		private BattleCalculator UpdateECNightToDay(ApiTypes type, CommonBattleData data)
		{
			// 지원함대 (야간)
			this.UpdateSupport(data.api_n_support_info, BattlePhase.support);

			// 야간 포격전
			this.UpdateHougeki(data.api_n_hougeki1, BattlePhase.n_hougeki1, EachCombinedPhase.AllPhase, CombinedType.None);
			this.UpdateHougeki(data.api_n_hougeki2, BattlePhase.n_hougeki2, EachCombinedPhase.AllPhase, CombinedType.None);

			// ** 주간전 전환 ** //

			// 기지항공대 분식 항공전
			this.UpdateAirBattle(data.api_air_base_injection, BattlePhase.airbase_injection);

			// 기지항공대 항공전
			if (data.api_air_base_attack != null)
			{
				foreach (var _data in data.api_air_base_attack)
					this.UpdateAirBattle(_data, BattlePhase.airbase_attack);
			}

			// 분식 항공전
			this.UpdateAirBattle(data.api_injection_kouku, BattlePhase.injection_kouku);

			// 항공전
			this.UpdateAirBattle(data.api_kouku, BattlePhase.kouku);

			// 항공전 (2차)
			this.UpdateAirBattle(data.api_kouku2, BattlePhase.kouku2);

			// 지원함대 (주간)
			this.UpdateSupport(data.api_support_info, BattlePhase.support);

			// 선제 대잠
			this.UpdateHougeki(data.api_opening_taisen, BattlePhase.opening_taisen);

			// 개막 뇌격
			this.UpdateTorpedo(data.api_opening_atack, BattlePhase.opening_atack);

			// 주간 포격전
			this.UpdateHougeki(data.api_hougeki1, BattlePhase.hougeki1);
			this.UpdateHougeki(data.api_hougeki2, BattlePhase.hougeki2);
			this.UpdateHougeki(data.api_hougeki3, BattlePhase.hougeki3);

			// 폐막 뇌격
			this.UpdateTorpedo(data.api_raigeki, BattlePhase.raigeki);


			// MVP 산출
			this.MVP_First = this.AliasFirstShips
				?.OrderByDescending(x => x.TotalDealt)
				?.FirstOrDefault()
				?.Index ?? 0;

			this.MVP_Second = this.AliasSecondShips
				?.OrderByDescending(x => x.TotalDealt)
				?.FirstOrDefault()
				?.Index ?? 0;

			// Event
			Updated?.Invoke(this, System.EventArgs.Empty);

			// Chaining
			return this;
		}

		/// <summary>
		/// 항공전 관련 계산
		/// </summary>
		private void UpdateAirBattle(kcsapi_battle_kouku data, BattlePhase phase)
		{
			if (data == null) return;

			if (data.api_stage3 != null)
			{
				var api_fdam = data.api_stage3.api_fdam;
				if (api_fdam != null)
				{
					for (int i = 0; i < api_fdam.Length; i++)
					{
						if (IsCombined && i >= 6)
							AliasSecondShips[i - 6]?.Damaged(api_fdam[i], phase, null, DamageType.Normal);
						else
							AliasFirstShips[i]?.Damaged(api_fdam[i], phase, null, DamageType.Normal);
					}
				}

				var api_edam = data.api_stage3.api_edam;
				if (api_edam != null)
				{
					for (int i = 0; i < api_edam.Length; i++)
					{
						if (IsEnemyCombined && i >= 6)
							EnemySecondShips[i - 6]?.Damaged(api_edam[i], phase, null, DamageType.Normal);
						else
							EnemyFirstShips[i]?.Damaged(api_edam[i], phase, null, DamageType.Normal);
					}
				}
			}
			if (data.api_stage3_combined != null)
			{
				var api_fdam = data.api_stage3_combined.api_fdam;
				if (api_fdam != null)
				{
					for (int i = 0; i < api_fdam.Length; i++)
					{
						if (IsCombined && i >= 6)
							AliasSecondShips[i - 6]?.Damaged(api_fdam[i], phase, null, DamageType.Normal);
						else
							AliasFirstShips[i]?.Damaged(api_fdam[i], phase, null, DamageType.Normal);
					}
				}

				var api_edam = data.api_stage3_combined.api_edam;
				if (api_edam != null)
				{
					for (int i = 0; i < api_edam.Length; i++)
					{
						if (IsEnemyCombined && i >= 6)
							EnemySecondShips[i - 6]?.Damaged(api_edam[i], phase, null, DamageType.Normal);
						else
							EnemyFirstShips[i]?.Damaged(api_edam[i], phase, null, DamageType.Normal);
					}
				}
			}
		}

		/// <summary>
		/// 지원함대 관련 계산 (_combined 에 값이 들어와도 문제 없도록)
		/// </summary>
		private void UpdateSupport(kcsapi_battle_support_info data, BattlePhase phase)
		{
			if (data == null) return;

			decimal[] damages = null;
			decimal[] damages_combined = null;

			if (data.api_support_airatack != null)
			{
				damages = data.api_support_airatack.api_stage3?.api_edam;
				damages_combined = data.api_support_airatack.api_stage3_combined?.api_edam;
			}
			else if (data.api_support_hourai != null)
			{
				damages = data.api_support_hourai.api_damage;
				damages_combined = data.api_support_hourai.api_damage_combined;
			}

			if (damages != null)
			{
				for (int i = 0; i < damages.Length; i++)
				{
					if (IsEnemyCombined && i >= 6)
						EnemySecondShips[i - 6]?.Damaged(damages[i], phase, null, DamageType.Normal);
					else
						EnemyFirstShips[i]?.Damaged(damages[i], phase, null, DamageType.Normal);
				}
			}
			if (damages_combined != null)
			{
				for (int i = 0; i < damages_combined.Length; i++)
					EnemySecondShips[i]?.Damaged(damages_combined[i], phase, null, DamageType.Normal);
			}
		}

		/// <summary>
		/// 포격전, 선제대잠 관련 계산 (아군단일, 심해단일)
		/// </summary>
		private void UpdateHougeki(kcsapi_battle_hougeki data, BattlePhase phase)
		{
			if (data == null) return;

			if (data.api_df_list != null)
			{
				var at_eflag = data.api_at_eflag;
				var at_list = data.api_at_list;
				var at_type = data.api_at_type;
				var df_list = data.api_df_list;
				var df_damage = data.api_damage;

				for (int i = 0; i < df_list.Length; i++)
				{
					var eflag = at_eflag[i];
					var type = at_type[i];

					var from = at_list[i];
					var target = df_list[i];
					var target_dmg = df_damage[i];

					for (int j = 0; j < target.Length; j++)
					{
						var target_idx = target[j];
						if (target_idx < 0) continue;

						// 아군이 공격 (적군이 맞음)
						if (eflag == 0)
						{
							EnemyFirstShips[target_idx]?.Damaged(target_dmg[j], phase, AliasFirstShips[from], (DamageType)type);
							AliasFirstShips[from]?.Dealt(target_dmg[j], phase, (DamageType)type);
						}

						// 적군이 공격 (아군이 맞음)
						else
						{
							AliasFirstShips[target_idx]?.Damaged(target_dmg[j], phase, EnemyFirstShips[from], (DamageType)type);
							EnemyFirstShips[from]?.Dealt(target_dmg[j], phase, (DamageType)type);
						}
					}
				}
			}
		}

		/// <summary>
		/// 포격전, 선제대잠 관련 계산 (아군연합, 심해단일)
		/// </summary>
		private void UpdateHougeki(kcsapi_battle_hougeki data, BattlePhase phase, CombinedPhase combinedPhase, CombinedType combinedType = CombinedType.Default)
		{
			if (data == null) return;
			if (combinedType == CombinedType.None) return;

			if (data.api_df_list != null)
			{
				var at_eflag = data.api_at_eflag;
				var at_list = data.api_at_list;
				var at_type = data.api_at_type;
				var df_list = data.api_df_list;
				var df_damage = data.api_damage;

				for (int i = 0; i < df_list.Length; i++)
				{
					var eflag = at_eflag[i];
					var type = at_type[i];

					var from = at_list[i];
					var target = df_list[i];
					var target_dmg = df_damage[i];

					for (int j = 0; j < target.Length; j++)
					{
						int target_idx = target[j];
						int damage_value = (int)target_dmg[j];

						if (target_idx < 0) continue;

						if (combinedType == CombinedType.Default)
						{
							if (combinedPhase == CombinedPhase.FirstPhase)
							{
								// 아군이 공격 (적군이 맞음)
								if (eflag == 0)
								{
									ShipBattleInfo _from = (IsCombined && from >= 6) ? AliasSecondShips[from - 6] : AliasFirstShips[from];
									_from?.Dealt(target_dmg[j], phase, (DamageType)type);

									EnemyFirstShips[target_idx]?.Damaged(target_dmg[j], phase, _from, (DamageType)type);

									if (IsCombined && from >= 6)
										AliasSecondShips[from - 6]?.Dealt(target_dmg[j], phase, (DamageType)type);
									else
										AliasFirstShips[from]?.Dealt(target_dmg[j], phase, (DamageType)type);
								}

								// 적군이 공격 (아군이 맞음)
								else
								{
									ShipBattleInfo _from = (IsEnemyCombined && from >= 6) ? EnemySecondShips[from - 6] : EnemyFirstShips[from];
									_from?.Dealt(target_dmg[j], phase, (DamageType)type);

									AliasSecondShips[target_idx - 6]?.Damaged(target_dmg[j], phase, _from, (DamageType)type);
								}
							}
							else
							{
								// 아군이 공격 (적군이 맞음)
								if (eflag == 0)
								{
									ShipBattleInfo _from = (IsCombined && from >= 6) ? AliasSecondShips[from - 6] : AliasFirstShips[from];
									_from?.Dealt(target_dmg[j], phase, (DamageType)type);

									EnemyFirstShips[target_idx]?.Damaged(target_dmg[j], phase, _from, (DamageType)type);
								}

								// 적군이 공격 (아군이 맞음)
								else
								{
									ShipBattleInfo _from = (IsEnemyCombined && from >= 6) ? EnemySecondShips[from - 6] : EnemyFirstShips[from];
									_from?.Dealt(target_dmg[j], phase, (DamageType)type);

									AliasFirstShips[target_idx]?.Damaged(target_dmg[j], phase, _from, (DamageType)type);
								}
							}
						}
						else if (combinedType == CombinedType.Water)
						{
							if (combinedPhase == CombinedPhase.ThirdPhase)
							{
								// 아군이 공격 (적군이 맞음)
								if (eflag == 0)
								{
									ShipBattleInfo _from = (IsCombined && from >= 6) ? AliasSecondShips[from - 6] : AliasFirstShips[from];
									_from?.Dealt(target_dmg[j], phase, (DamageType)type);

									EnemyFirstShips[target_idx]?.Damaged(target_dmg[j], phase, _from, (DamageType)type);
								}

								// 적군이 공격 (아군이 맞음)
								else
								{
									ShipBattleInfo _from = (IsEnemyCombined && from >= 6) ? EnemySecondShips[from - 6] : EnemyFirstShips[from];
									_from?.Dealt(target_dmg[j], phase, (DamageType)type);

									AliasSecondShips[target_idx - 6]?.Damaged(target_dmg[j], phase, _from, (DamageType)type);
								}
							}
							else
							{
								// 아군이 공격 (적군이 맞음)
								if (eflag == 0)
								{
									ShipBattleInfo _from = (IsCombined && from >= 6) ? AliasSecondShips[from - 6] : AliasFirstShips[from];
									_from?.Dealt(target_dmg[j], phase, (DamageType)type);

									EnemyFirstShips[target_idx]?.Damaged(target_dmg[j], phase, _from, (DamageType)type);
								}

								// 적군이 공격 (아군이 맞음)
								else
								{
									ShipBattleInfo _from = (IsEnemyCombined && from >= 6) ? EnemySecondShips[from - 6] : EnemyFirstShips[from];
									_from?.Dealt(target_dmg[j], phase, (DamageType)type);

									AliasFirstShips[target_idx]?.Damaged(target_dmg[j], phase, _from, (DamageType)type);
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// 포격전, 선제대잠 관련 계산 (심해연합)
		/// </summary>
		private void UpdateHougeki(kcsapi_battle_hougeki data, BattlePhase phase, EachCombinedPhase combinedPhase = EachCombinedPhase.AllPhase, CombinedType combinedType = CombinedType.None)
		{
			if (data == null) return;

			if (data.api_df_list != null)
			{
				var at_eflag = data.api_at_eflag;
				var at_list = data.api_at_list;
				var at_type = data.api_at_type;
				var df_list = data.api_df_list;
				var df_damage = data.api_damage;

				for (int i = 0; i < df_list.Length; i++)
				{
					var eflag = at_eflag[i];
					var type = at_type[i];

					var from = at_list[i];
					var target = df_list[i];
					var target_dmg = df_damage[i];

					for (int j = 0; j < target.Length; j++)
					{
						int target_idx = target[j];
						int damage_value = (int)target_dmg[j];

						if (target_idx < 0) continue;

						if (combinedPhase == EachCombinedPhase.FirstPhase)
						{
							// 아군이 공격 (적군이 맞음)
							if (eflag == 0)
							{
								ShipBattleInfo _from = (IsCombined && from >= 6) ? AliasSecondShips[from - 6] : AliasFirstShips[from];
								_from?.Dealt(target_dmg[j], phase, (DamageType)type);

								EnemyFirstShips[target_idx]?.Damaged(target_dmg[j], phase, _from, (DamageType)type);
							}

							// 적군이 공격 (아군이 맞음)
							else
							{
								ShipBattleInfo _from = (IsEnemyCombined && from >= 6) ? EnemySecondShips[from - 6] : EnemyFirstShips[from];
								_from?.Dealt(target_dmg[j], phase, (DamageType)type);

								AliasFirstShips[target_idx]?.Damaged(target_dmg[j], phase, _from, (DamageType)type);
							}
						}
						else if (combinedPhase == EachCombinedPhase.SecondPhase)
						{
							// 아군이 공격 (적군이 맞음)
							if (eflag == 0)
							{
								ShipBattleInfo _from = (IsCombined && from >= 6) ? AliasSecondShips[from - 6] : AliasFirstShips[from];
								_from?.Dealt(target_dmg[j], phase, (DamageType)type);

								EnemySecondShips[target_idx - 6]?.Damaged(target_dmg[j], phase, _from, (DamageType)type);
							}

							// 적군이 공격 (아군이 맞음)
							else
							{
								ShipBattleInfo _from = (IsEnemyCombined && from >= 6) ? EnemySecondShips[from - 6] : EnemyFirstShips[from];
								_from?.Dealt(target_dmg[j], phase, (DamageType)type);

								if (combinedType == CombinedType.Default || combinedType == CombinedType.Water)
									AliasSecondShips[target_idx - 6]?.Damaged(target_dmg[j], phase, _from, (DamageType)type);
								else
									AliasFirstShips[target_idx]?.Damaged(target_dmg[j], phase, _from, (DamageType)type);
							}
						}
						else if (combinedPhase == EachCombinedPhase.AllPhase)
						{
							// 아군이 공격 (적군이 맞음)
							if (eflag == 0)
							{
								ShipBattleInfo _from = (IsCombined && from >= 6) ? AliasSecondShips[from - 6] : AliasFirstShips[from];
								_from?.Dealt(target_dmg[j], phase, (DamageType)type);

								if (IsEnemyCombined && target_idx >= 6)
									EnemySecondShips[target_idx - 6]?.Damaged(target_dmg[j], phase, _from, (DamageType)type);
								else
									EnemyFirstShips[target_idx]?.Damaged(target_dmg[j], phase, _from, (DamageType)type);
							}

							// 적군이 공격 (아군이 맞음)
							else
							{
								ShipBattleInfo _from = (IsEnemyCombined && from >= 6) ? EnemySecondShips[from - 6] : EnemyFirstShips[from];
								_from?.Dealt(target_dmg[j], phase, (DamageType)type);

								if (IsCombined && target_idx >= 6)
									AliasSecondShips[target_idx - 6]?.Damaged(target_dmg[j], phase, _from, (DamageType)type);
								else
									AliasFirstShips[target_idx]?.Damaged(target_dmg[j], phase, _from, (DamageType)type);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// 야간 포격전 관련 계산
		/// </summary>
		private void UpdateMidnight(kcsapi_battle_midnight_hougeki data, BattlePhase phase, int enemy_deck = 1)
		{
			if (data == null) return;

			if (data.api_df_list != null)
			{
				var at_eflag = data.api_at_eflag;
				var at_list = data.api_at_list;
				var sp_list = data.api_sp_list;
				var df_list = data.api_df_list;
				var df_damage = data.api_damage;

				for (int i = 0; i < df_list.Length; i++)
				{
					var eflag = at_eflag[i];
					var type = sp_list[i];

					var from = at_list[i];
					var target = df_list[i];
					var target_dmg = df_damage[i];

					for (int j = 0; j < target.Length; j++)
					{
						var target_idx = target[j];
						if (target_idx < 0) continue;

						// 아군이 공격 (적군이 맞음)
						if (eflag == 0)
						{
							ShipBattleInfo _from = (IsCombined && from >= 6) ? AliasSecondShips[from - 6] : AliasFirstShips[from];
							_from?.Dealt(target_dmg[j], phase, DamageTypeFromSP(type));

							if (enemy_deck == 1)
								EnemyFirstShips[target_idx]?.Damaged(target_dmg[j], phase, _from, DamageTypeFromSP(type));
							else
								EnemySecondShips[target_idx - 6]?.Damaged(target_dmg[j], phase, _from, DamageTypeFromSP(type));
						}

						// 적군이 공격 (아군이 맞음)
						else
						{
							ShipBattleInfo _from = (enemy_deck != 1) ? EnemySecondShips[from - 6] : EnemyFirstShips[from];
							_from?.Dealt(target_dmg[j], phase, DamageTypeFromSP(type));

							if (IsCombined && target_idx >= 6)
								AliasSecondShips[target_idx - 6]?.Damaged(target_dmg[j], phase, _from, DamageTypeFromSP(type));
							else
								AliasFirstShips[target_idx]?.Damaged(target_dmg[j], phase, _from, DamageTypeFromSP(type));
						}
					}
				}
			}
		}

		/// <summary>
		/// 개막 뇌격전, 폐막 뇌격 관련 계산
		/// </summary>
		private void UpdateTorpedo(kcsapi_battle_raigeki data, BattlePhase phase)
		{
			if (data == null) return;

			var fydam = data.api_fydam; // 아군이 준 데미지
			var eydam = data.api_eydam; // 적군이 준 데미지
			var frai = data.api_frai; // 아군이 누구 때렸는지
			var erai = data.api_erai; // 적군이 누구 때렸는지

			for (int i = 0; i < frai.Length; i++)
			{
				var idx = frai[i];
				if (idx < 0) continue;

				var _from = (IsCombined && i >= 6 ? AliasSecondShips[i - 6] : AliasFirstShips[i]);
				_from?.Dealt(fydam[i], phase, DamageType.Normal);

				if (IsEnemyCombined && idx >= 6)
					EnemySecondShips[idx - 6]?.Damaged(fydam[i], phase, _from, DamageType.Normal);
				else
					EnemyFirstShips[idx]?.Damaged(fydam[i], phase, _from, DamageType.Normal);
			}
			for (int i = 0; i < erai.Length; i++)
			{
				var idx = erai[i];
				if (idx < 0) continue;

				var _from = (IsEnemyCombined && i >= 6 ? EnemySecondShips[i - 6] : EnemyFirstShips[i]);
				_from?.Dealt(eydam[i], phase, DamageType.Normal);

				if (IsCombined && idx >= 6)
					AliasSecondShips[idx - 6]?.Damaged(eydam[i], phase, _from, DamageType.Normal);
				else
					AliasFirstShips[idx]?.Damaged(eydam[i], phase, _from, DamageType.Normal);
			}
		}

		/// <summary>
		/// sp_list 의 값을 DamageType 으로 변환
		/// </summary>
		private DamageType DamageTypeFromSP(int sp)
		{
			switch (sp)
			{
				case 0:
					return DamageType.Normal;
				case 1:
					return DamageType.Twice;
				case 2:
					return DamageType.MainGun_Torpedo_Cutin;
				case 3:
					return DamageType.Torpedo_Torpedo_Cutin;
				case 4:
					return DamageType.MainGun_MainGun_SubGun_Cutin;
				case 5:
					return DamageType.MainGun_MainGun_MainGun_Cutin;
				case 6:
					return DamageType.Airstrike_Cutin;
				case 7:
					return DamageType.MainGun_Torpedo_Radar_Cutin;
				case 8:
					return DamageType.Torpedo_Lookouts_Radar_Cutin;

				default:
					return DamageType.None;
			}
		}

		#region Raw Update functions
		public void Update(kcsapi_sortie_battle data, ApiTypes_Sortie type)
		{
			this.Update(
				(ApiTypes)type,
				new CommonBattleData
				{
					api_air_base_injection = data.api_air_base_injection,
					api_air_base_attack = data.api_air_base_attack,
					api_injection_kouku = data.api_injection_kouku,
					api_kouku = data.api_kouku,

					api_support_info = data.api_support_info,

					api_opening_taisen = data.api_opening_taisen,
					api_opening_atack = data.api_opening_atack,
					api_hougeki1 = data.api_hougeki1,
					api_hougeki2 = data.api_hougeki2,
					api_hougeki3 = data.api_hougeki3,
					api_raigeki = data.api_raigeki
				}
			);
		}
		public void Update(kcsapi_sortie_battle_midnight data, ApiTypes_SortieMidnight type)
		{
			this.Update(
				(ApiTypes)type,
				new CommonBattleData
				{
					api_n_support_info = data.api_n_support_info,
					api_hougeki = data.api_hougeki
				}
			);
		}
		public void Update(kcsapi_sortie_airbattle data, ApiTypes_SortieAirBattle type)
		{
			this.Update(
				(ApiTypes)type,
				new CommonBattleData
				{
					api_air_base_injection = data.api_air_base_injection,
					api_injection_kouku = data.api_injection_kouku,
					api_air_base_attack = data.api_air_base_attack,
					api_kouku = data.api_kouku,
					api_kouku2 = data.api_kouku2
				}
			);
		}

		public void Update(kcsapi_combined_battle data, ApiTypes_CombinedBattle type)
		{
			this.Update(
				(ApiTypes)type,
				new CommonBattleData
				{
					api_air_base_injection = data.api_air_base_injection,
					api_injection_kouku = data.api_injection_kouku,
					api_air_base_attack = data.api_air_base_attack,
					api_kouku = data.api_kouku,

					api_support_info = data.api_support_info,

					api_opening_taisen = data.api_opening_taisen,
					api_opening_atack = data.api_opening_atack,
					api_hougeki1 = data.api_hougeki1,
					api_hougeki2 = data.api_hougeki2,
					api_hougeki3 = data.api_hougeki3,
					api_raigeki = data.api_raigeki
				});
		}
		public void Update(kcsapi_combined_battle_each data, ApiTypes_CombinedBattleEC type)
		{
			this.Update(
				(ApiTypes)type,
				new CommonBattleData
				{
					api_air_base_injection = data.api_air_base_injection,
					api_injection_kouku = data.api_injection_kouku,
					api_air_base_attack = data.api_air_base_attack,
					api_kouku = data.api_kouku,

					api_support_info = data.api_support_info,

					api_opening_taisen = data.api_opening_taisen,
					api_opening_atack = data.api_opening_atack,
					api_hougeki1 = data.api_hougeki1,
					api_hougeki2 = data.api_hougeki2,
					api_hougeki3 = data.api_hougeki3,
					api_raigeki = data.api_raigeki
				}
			);
		}
		public void Update(kcsapi_combined_airbattle data, ApiTypes_CombinedAirBattle type)
		{
			this.Update(
				(ApiTypes)type,
				new CommonBattleData
				{
					api_air_base_injection = data.api_air_base_injection,
					api_injection_kouku = data.api_injection_kouku,
					api_air_base_attack = data.api_air_base_attack,
					api_kouku = data.api_kouku,
					api_kouku2 = data.api_kouku2
				}
			);
		}
		public void Update(kcsapi_combined_battle_midnight data, ApiTypes_CombinedMidnight type)
		{
			this.Update(
				(ApiTypes)type,
				new CommonBattleData
				{
					api_n_support_info = data.api_n_support_info,
					api_hougeki = data.api_hougeki
				}
			);
		}
		public void Update(kcsapi_combined_battle_midnight_ec data)
		{
			this.Update(
				ApiTypes.combined_battle_midnight_ec,
				new CommonBattleData
				{
					api_active_deck = data.api_active_deck,

					api_n_support_info = data.api_n_support_info,
					api_hougeki = data.api_hougeki
				}
			);
		}
		public void Update(kcsapi_combined_battle_ec_nighttoday data)
		{
			this.Update(
				ApiTypes.combined_battle_ec_nighttoday,
				new CommonBattleData
				{
					api_n_support_info = data.api_n_support_info,
					api_n_hougeki1 = data.api_n_hougeki1,
					api_n_hougeki2 = data.api_n_hougeki2,

					api_air_base_injection = data.api_air_base_injection,
					api_injection_kouku = data.api_injection_kouku,
					api_air_base_attack = data.api_air_base_attack,
					api_kouku = data.api_kouku,

					api_support_info = data.api_support_info,

					api_opening_taisen = data.api_opening_taisen,
					api_opening_atack = data.api_opening_atack,
					api_hougeki1 = data.api_hougeki1,
					api_hougeki2 = data.api_hougeki2,
					api_hougeki3 = data.api_hougeki3,
					api_raigeki = data.api_raigeki
				}
			);
		}
		#endregion
	}
}
