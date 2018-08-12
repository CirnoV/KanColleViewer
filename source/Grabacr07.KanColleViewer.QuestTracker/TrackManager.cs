using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Reactive.Linq;
using WebUtility = System.Net.WebUtility;
using _EventArgs = System.EventArgs;

using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;

using Grabacr07.KanColleViewer.QuestTracker.Models;
using Grabacr07.KanColleViewer.QuestTracker.Models.Tracker;
using Grabacr07.KanColleViewer.QuestTracker.Models.EventArgs;

using DynamicJson = Codeplex.Data.DynamicJson;

#region Alias
using practice_battle = Grabacr07.KanColleWrapper.Models.Raw.kcsapi_sortie_battle;
using battle_midnight_sp_midnight = Grabacr07.KanColleWrapper.Models.Raw.kcsapi_sortie_battle_midnight;
using practice_midnight_battle = Grabacr07.KanColleWrapper.Models.Raw.kcsapi_sortie_battle_midnight;
using sortie_ld_airbattle = Grabacr07.KanColleWrapper.Models.Raw.kcsapi_sortie_airbattle;
using combined_battle_battle_water = Grabacr07.KanColleWrapper.Models.Raw.kcsapi_combined_battle;
using combined_battle_ld_airbattle = Grabacr07.KanColleWrapper.Models.Raw.kcsapi_combined_airbattle;
using combined_battle_sp_midnight = Grabacr07.KanColleWrapper.Models.Raw.kcsapi_combined_battle_midnight;
#endregion

namespace Grabacr07.KanColleViewer.QuestTracker.Models
{
	public class TrackManager
	{
		public Assembly Assembly => Assembly.GetExecutingAssembly();

		public ObservableCollection<TrackerBase> trackingAvailable
		{
			get; private set;
		} = new ObservableCollection<TrackerBase>();

		public List<TrackerBase> TrackingQuests => trackingAvailable.Where(x => x.IsTracking).ToList();
		public List<TrackerBase> AllQuests => trackingAvailable.ToList();

		internal event EventHandler<BattleResultEventArgs> BattleResultEvent;
		internal event EventHandler<MissionResultEventArgs> MissionResultEvent;
		internal event EventHandler<PracticeResultEventArgs> PracticeResultEvent;
		internal event EventHandler RepairStartEvent;
		internal event EventHandler ChargeEvent;
		internal event EventHandler<BaseEventArgs> CreateItemEvent;
		internal event EventHandler CreateShipEvent;
		internal event EventHandler<DestroyShipEventArgs> DestoryShipEvent;
		internal event EventHandler<DestroyItemEventArgs> DestoryItemEvent;
		internal event EventHandler<BaseEventArgs> PowerUpEvent;
		internal event EventHandler<BaseEventArgs> ReModelEvent;
		internal event EventHandler HenseiEvent;
		internal event EventHandler EquipEvent;

		internal SlotItemTracker slotitemTracker { get; }

		public event EventHandler QuestsEventChanged;

		private BattleCalculator battleCalculator { get; set; }

		private Func<bool> PreprocessCheck { get; }
		private void Preprocess(Action action)
		{
			if ((!PreprocessCheck?.Invoke()) ?? false) return;

			try { action(); }
			catch { }
		}

		public TrackManager(Func<bool> PreprocessCheck, Func<string> KcaQSync_Pass)
		{
			this.PreprocessCheck = PreprocessCheck;

			var homeport = KanColleClient.Current.Homeport;
			var proxy = KanColleClient.Current.Proxy;
			var MapInfo = new TrackerMapInfo();

			battleCalculator = new BattleCalculator();
			slotitemTracker = new SlotItemTracker(homeport, proxy);

			// 동기화
			{
				var pass = KcaQSync_Pass?.Invoke();
				if (!string.IsNullOrEmpty(pass))
				{
					var json = DynamicJson.Serialize(new {
						userid = KanColleClient.Current.Homeport.Admiral.RawData.api_member_id,
						pass = pass
					});
					HTTPRequest.PostAsync(
						"http://kcaqsync.swaytwig.com/api/read",
						"data=" + WebUtility.UrlEncode(RSA.Encrypt(json)),
						y =>
						{
							var result = DynamicJson.Parse(y);
							ApplySyncData(
								result.data.ToString() as string,
								Convert.ToInt64(result.timestamp.ToString() as string)
							);
						}
					);
				}
			}
			this.QuestsEventChanged += (s, e) =>
			{
				var enc36 = new Func<int, string>(input =>
				{
					var table = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ=";
					var output = "";
					do
					{
						output = table[input % 36] + output;
						input /= 36;
					} while (input > 0);
					return output;
				});
				var enc37 = new Func<int, string>(input =>
				{
					var table = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ=";
					var output = "";
					do
					{
						var pos = Math.Min(table.Length - 1, input);
						output += table[pos];
						input -= pos;
					} while (input > 0);
					return output;
				});

				var pass = KcaQSync_Pass?.Invoke();
				if (string.IsNullOrEmpty(pass)) return;

				var data = "";
				foreach (var item in this.trackingAvailable)
				{
					if (item.GetType() == typeof(DefaultTracker)) continue;
					if (item.GetRawDatas().All(x => x == 0) && !item.IsTracking) continue;

					var content =
						(item.IsTracking ? "1" : "0")
						+ enc36(item.Id).PadLeft(2, '0')
						+ string.Join("", item.GetRawDatas().Select(enc37));

					data += enc37(content.Length) + content;
				}

				var json = DynamicJson.Serialize(new
				{
					userid = KanColleClient.Current.Homeport.Admiral.RawData.api_member_id,
					pass = pass,
					data = data
				});
				HTTPRequest.PostAsync(
					"http://kcaqsync.swaytwig.com/api/write",
					"data=" + WebUtility.UrlEncode(RSA.Encrypt(json)),
					y => { }
				);
			};

			// 편성 변경
			homeport.Organization.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(homeport.Organization.Fleets))
				{
					var fleets = homeport.Organization.Fleets.Select(x => x.Value);
					foreach (var x in fleets)
						x.State.Updated += (_, _2) => Preprocess(() => HenseiEvent?.Invoke(this, _EventArgs.Empty));
				}
			};
			// 장비 변경
			proxy.api_req_kaisou_slot_exchange_index.TryParse<kcsapi_slot_exchange_index>()
				.Subscribe(x => Preprocess(() => EquipEvent?.Invoke(this, _EventArgs.Empty)));
			proxy.api_req_kaisou_slot_deprive.TryParse<kcsapi_slot_deprive>()
				.Subscribe(x => Preprocess(() => EquipEvent?.Invoke(this, _EventArgs.Empty)));

			// 근대화 개수
			proxy.api_req_kaisou_powerup.TryParse<kcsapi_powerup>()
				.Subscribe(x => Preprocess(() => PowerUpEvent?.Invoke(this, new BaseEventArgs(x.Data.api_powerup_flag != 0))));

			// 개수공창 개수
			proxy.api_req_kousyou_remodel_slot.TryParse<kcsapi_remodel_slot>()
				.Subscribe(x => Preprocess(() => ReModelEvent?.Invoke(this, new BaseEventArgs(x.Data.api_remodel_flag != 0))));

			// 폐기
			slotitemTracker.DestroyItemEvent += (s, e) => Preprocess(() => DestoryItemEvent?.Invoke(this, e));

			// 해체
			proxy.api_req_kousyou_destroyship.TryParse<kcsapi_destroyship>()
				.Subscribe(x => Preprocess(() => DestoryShipEvent?.Invoke(this, new DestroyShipEventArgs(x.Request, x.Data))));

			// 건조
			proxy.api_req_kousyou_createship.TryParse<kcsapi_createship>()
				.Subscribe(x => Preprocess(() => CreateShipEvent?.Invoke(this, _EventArgs.Empty)));

			// 개발
			slotitemTracker.CreateItemEvent += (s, e) => Preprocess(() => CreateItemEvent?.Invoke(this, e));

			// 보급
			proxy.api_req_hokyu_charge.TryParse<kcsapi_charge>()
				.Subscribe(x => Preprocess(() => ChargeEvent?.Invoke(this, _EventArgs.Empty)));

			// 입거
			proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_nyukyo/start")
				.Subscribe(x => Preprocess(() => RepairStartEvent?.Invoke(this, _EventArgs.Empty)));

			// 원정
			proxy.api_req_mission_result.TryParse<kcsapi_mission_result>()
				.Subscribe(x => Preprocess(() => MissionResultEvent?.Invoke(this, new MissionResultEventArgs(x.Data))));

			// 출격 (시작, 진격)
			proxy.api_req_map_start.TryParse<kcsapi_map_start_next>()
				.Subscribe(x => Preprocess(() => MapInfo.Reset(x.Data.api_maparea_id, x.Data.api_mapinfo_no, x.Data.api_no, x.Data.api_event_id == 5)));
			proxy.api_req_map_next.TryParse<kcsapi_map_start_next>()
				.Subscribe(x => Preprocess(() => MapInfo.Next(x.Data.api_maparea_id, x.Data.api_mapinfo_no, x.Data.api_no, x.Data.api_event_id == 5)));

			#region 전투

			#region 통상 - 주간전 / 연습 - 주간전
			proxy.api_req_sortie_battle
				.TryParse<kcsapi_sortie_battle>().Subscribe(x => this.Update(x.Data, ApiTypes.sortie_battle));

			proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_practice/battle")
				.TryParse<practice_battle>().Subscribe(x => this.Update(x.Data, ApiTypes.practice_battle));
			#endregion

			#region 통상 - 야전 / 통상 - 개막야전 / 연습 - 야전
			proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_battle_midnight/battle")
				.TryParse<kcsapi_sortie_battle_midnight>().Subscribe(x => this.Update(x.Data, ApiTypes.sortie_battle_midnight));

			proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_battle_midnight/sp_midnight")
				.TryParse<battle_midnight_sp_midnight>().Subscribe(x => this.Update(x.Data, ApiTypes.sortie_battle_midnight_sp));

			proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_practice/midnight_battle")
				.TryParse<practice_midnight_battle>().Subscribe(x => this.Update(x.Data, ApiTypes.practice_midnight_battle));
			#endregion

			#region 항공전 - 주간전 / 공습전 - 주간전
			proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_sortie/airbattle")
				.TryParse<kcsapi_sortie_airbattle>().Subscribe(x => this.Update(x.Data, ApiTypes.sortie_airbattle));

			proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_sortie/ld_airbattle")
				.TryParse<sortie_ld_airbattle>().Subscribe(x => this.Update(x.Data, ApiTypes.sortie_ld_airbattle));
			#endregion

			#region 연합함대 - 주간전
			proxy.api_req_combined_battle_battle
				.TryParse<kcsapi_combined_battle>().Subscribe(x => this.Update(x.Data, ApiTypes.combined_battle));

			proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_combined_battle/battle_water")
				.TryParse<combined_battle_battle_water>().Subscribe(x => this.Update(x.Data, ApiTypes.combined_battle_water));
			#endregion

			#region 연합vs연합 - 주간전
			proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_combined_battle/ec_battle")
				.TryParse<kcsapi_combined_battle_each>().Subscribe(x => this.Update(x.Data, ApiTypes.combined_battle_ec));

			proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_combined_battle/each_battle")
				.TryParse<kcsapi_combined_battle_each>().Subscribe(x => this.Update(x.Data, ApiTypes.combined_battle_each));

			proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_combined_battle/each_battle_water")
				.TryParse<kcsapi_combined_battle_each>().Subscribe(x => this.Update(x.Data, ApiTypes.combined_battle_each_water));
			#endregion

			#region 연합함대 - 항공전 / 공습전
			proxy.api_req_combined_battle_airbattle
				.TryParse<kcsapi_combined_airbattle>().Subscribe(x => this.Update(x.Data, ApiTypes.combined_airbattle));

			proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_combined_battle/ld_airbattle")
				.TryParse<combined_battle_ld_airbattle>().Subscribe(x => this.Update(x.Data, ApiTypes.combined_ld_airbattle));
			#endregion

			#region 연합함대 - 야전
			proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_combined_battle/midnight_battle")
				.TryParse<kcsapi_combined_battle_midnight>().Subscribe(x => this.Update(x.Data, ApiTypes.combined_battle_midnight));

			proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_combined_battle/sp_midnight")
				.TryParse<combined_battle_sp_midnight>().Subscribe(x => this.Update(x.Data, ApiTypes.combined_battle_midnight_sp));
			#endregion

			#region vs 연합 - 야전
			proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_combined_battle/ec_midnight_battle")
				.TryParse<kcsapi_combined_battle_midnight_ec>().Subscribe(x => this.Update(x.Data, ApiTypes.combined_battle_midnight_ec));
			#endregion

			#region vs 연합 - 야전>주간전
			proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_combined_battle/ec_night_to_day")
				.TryParse<kcsapi_combined_battle_ec_nighttoday>().Subscribe(x => this.Update(x.Data, ApiTypes.combined_battle_ec_nighttoday));
			#endregion

			#endregion

			// 전투 종료 (연합함대 포함)
			proxy.api_req_sortie_battleresult.TryParse<kcsapi_battle_result>()
				.Subscribe(x => Preprocess(() => BattleResultEvent?.Invoke(this,
				new BattleResultEventArgs(
					MapInfo.AfterCombat(),
					battleCalculator.EnemyFirstShips,
					battleCalculator.EnemySecondShips,
					x.Data
				)
			)));
			proxy.api_req_combined_battle_battleresult.TryParse<kcsapi_battle_result>()
				.Subscribe(x => Preprocess(() => BattleResultEvent?.Invoke(this,
				new BattleResultEventArgs(
					MapInfo.AfterCombat(),
					battleCalculator.EnemyFirstShips,
					battleCalculator.EnemySecondShips,
					x.Data
				)
			)));

			// 연습전 종료
			proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_practice/battle_result")
				.TryParse<kcsapi_battle_result>().Subscribe(x =>
					Preprocess(() => PracticeResultEvent?.Invoke(this, new PracticeResultEventArgs(x.Data)))
				);


			// Register all trackers
			trackingAvailable = new ObservableCollection<TrackerBase>(trackingAvailable.OrderBy(x => x.Id));
			trackingAvailable.CollectionChanged += (sender, e) =>
			{
				if (e.Action != NotifyCollectionChangedAction.Add)
					return;

				foreach (TrackerBase tracker in e.NewItems)
				{
					tracker.RegisterEvent(this);
					tracker.ResetQuest();
					tracker.ProgressChanged += () =>
					 {
						 try
						 {
							 QuestsEventChanged?.Invoke(this, _EventArgs.Empty);
						 }
						 catch { }
					 };
				}
			};
		}

		private bool ApplySyncData(string data, long timestamp)
		{
			var regex = new Regex("^[01][0-9A-Z=]+$", RegexOptions.Compiled);
			var table = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ=";

			var length = data.Length;
			var pos = 0;

			var list = new List<SyncData>();
			while (pos < length)
			{
				var size = table.IndexOf(data[pos++]);
				var q = data.Substring(pos, size);

				if (size < 4) return false;
				if (q.Length != size) return false;
				if (!regex.IsMatch(q)) return false;

				int[] tmp = null;
				var id = SyncData.Decode(q.Substring(1, 2));
				if (id == 215) // 로호
				{
					tmp = new int[] { 0 };
				}

				list.Add(new SyncData
				{
					Active = SyncData.Decode(q[0].ToString()) > 0,
					Id = id,
					Count = SyncData.DecodeList(q.Substring(3), tmp)
				});
			}

			foreach(var item in list)
			{
				var tracker = this.trackingAvailable.FirstOrDefault(x => x.Id == item.Id);
				if (tracker == null) continue;

				tracker.LastUpdated = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp).ToLocalTime();
				tracker.IsTracking = item.Active; // Needed?
				tracker.SetRawDatas(item.Count);
			}
			return true;
		}

		public void RefreshTrackers()
		{
			Preprocess(() => HenseiEvent?.Invoke(this, _EventArgs.Empty));
			Preprocess(() => EquipEvent?.Invoke(this, _EventArgs.Empty));
			QuestsEventChanged?.Invoke(this, _EventArgs.Empty);
		}


		#region Update From Battle SvData

		FleetData AliasFirst { get; set; }
		FleetData AliasSecond { get; set; }
		FleetData EnemyFirst { get; set; }
		FleetData EnemySecond { get; set; }

		private void ClearBattleInfo()
		{
			// Reset enemy fleet
			this.EnemyFirst = new FleetData();
			this.EnemySecond = new FleetData();
			if (this.AliasFirst != null) this.AliasFirst.Formation = Formation.없음;
		}
		private void UpdateFleets(int api_deck_id, ICommonBattleMembers data, int[] api_formation = null)
		{
			// 아군 정보 모항 기준으로 갱신
			this.UpdateFriendFleets(api_deck_id);

			this.EnemyFirst = new FleetData(
				data.ToMastersShipDataArray(),
				this.EnemyFirst?.Formation ?? Formation.없음,
				this.EnemyFirst?.Name ?? "",
				FleetType.EnemyFirst
			);

			// 제 2함대는 없음
			this.EnemySecond = new FleetData(
				new MembersShipData[0],
				Formation.없음,
				"",
				FleetType.EnemySecond
			);

			// 진형과 전투형태 존재하면
			if (api_formation != null)
			{
				if (this.AliasFirst != null)
					this.AliasFirst.Formation = (Formation)api_formation[0];

				if (this.EnemyFirst != null)
					this.EnemyFirst.Formation = (Formation)api_formation[1];
			}
		}
		private void UpdateFleetsCombinedEnemy(int api_deck_id, ICommonEachBattleMembers data, int[] api_formation = null)
		{
			// 아군 정보 모항 기준으로 갱신
			this.UpdateFriendFleets(api_deck_id);

			this.EnemyFirst = new FleetData(
				data.ToMastersShipDataArray(),
				this.EnemyFirst?.Formation ?? Formation.없음,
				this.EnemyFirst?.Name ?? "",
				FleetType.EnemyFirst
			);

			this.EnemySecond = new FleetData(
				data.ToMastersSecondShipDataArray(),
				this.EnemySecond?.Formation ?? Formation.없음,
				this.EnemySecond?.Name ?? "",
				FleetType.EnemySecond
			);

			// 진형과 전투형태 존재하면
			if (api_formation != null)
			{
				if (this.AliasFirst != null)
					this.AliasFirst.Formation = (Formation)api_formation[0];

				if (this.EnemyFirst != null)
					this.EnemyFirst.Formation = (Formation)api_formation[1];
			}
		}
		private void UpdateFriendFleets(int deckID)
		{
			var organization = KanColleClient.Current.Homeport.Organization;

			this.AliasFirst = new FleetData(
				organization.Fleets[deckID].Ships.Select(s => new MembersShipData(s)).ToArray(),
				this.AliasFirst?.Formation ?? Formation.없음,
				organization.Fleets[deckID].Name,
				FleetType.AliasFirst
			);
			this.AliasSecond = new FleetData(
				organization.Combined && deckID == 1
					? organization.Fleets[2].Ships.Select(s => new MembersShipData(s)).ToArray()
					: new MembersShipData[0],
				this.AliasSecond?.Formation ?? Formation.없음,
				organization.Fleets[2].Name,
				FleetType.AliasSecond
			);
		}
		private void UpdateMaxHP(int[] api_f_maxhps, int[] api_e_maxhps, int[] api_f_maxhps_combined = null, int[] api_e_maxhps_combined = null)
		{
			this.AliasFirst.Ships.SetValues(api_f_maxhps, (s, v) => s.MaxHP = v);
			if (api_f_maxhps_combined != null)
				this.AliasSecond.Ships.SetValues(api_f_maxhps_combined, (s, v) => s.MaxHP = v);

			this.EnemyFirst.Ships.SetValues(api_e_maxhps, (s, v) => s.MaxHP = v);
			if (api_e_maxhps_combined != null)
				this.EnemySecond.Ships.SetValues(api_e_maxhps_combined, (s, v) => s.MaxHP = v);
		}
		private void UpdateBefHP(int[] api_f_nowhps, int[] api_e_nowhps, int[] api_f_nowhps_combined = null, int[] api_e_nowhps_combined = null)
		{
			this.AliasFirst.Ships.SetValues(api_f_nowhps, (s, v) => s.BeforeNowHP = v);
			if (api_f_nowhps_combined != null)
				this.AliasSecond.Ships.SetValues(api_f_nowhps_combined, (s, v) => s.BeforeNowHP = v);

			this.EnemyFirst.Ships.SetValues(api_e_nowhps, (s, v) => s.BeforeNowHP = v);
			if (api_e_nowhps_combined != null)
				this.EnemySecond.Ships.SetValues(api_e_nowhps_combined, (s, v) => s.BeforeNowHP = v);
		}
		private void UpdateNowHP(int[] api_f_nowhps, int[] api_e_nowhps, int[] api_f_nowhps_combined = null, int[] api_e_nowhps_combined = null)
		{
			this.AliasFirst.Ships.SetValues(api_f_nowhps, (s, v) => s.NowHP = v);
			if (api_f_nowhps_combined != null)
				this.AliasSecond.Ships.SetValues(api_f_nowhps_combined, (s, v) => s.NowHP = v);

			this.EnemyFirst.Ships.SetValues(api_e_nowhps, (s, v) => s.NowHP = v);
			if (api_e_nowhps_combined != null)
				this.EnemySecond.Ships.SetValues(api_e_nowhps_combined, (s, v) => s.NowHP = v);
		}

		private void Update(kcsapi_sortie_battle data, ApiTypes apiType)
		{
			if (apiType == ApiTypes.practice_battle)
				this.ClearBattleInfo();

			// 적, 아군 함대 정보 갱신
			this.UpdateFleets(data.api_deck_id, data, data.api_formation);

			// 체력 갱신
			this.UpdateMaxHP(data.api_f_maxhps, data.api_e_maxhps);
			this.UpdateBefHP(data.api_f_nowhps, data.api_e_nowhps);
			this.UpdateNowHP(data.api_f_nowhps, data.api_e_nowhps);

			// 전투 계산
			battleCalculator
				.Initialize(
					this.AliasFirst, this.AliasSecond,
					this.EnemyFirst, this.EnemySecond
				)
				.Update(data, (ApiTypes_Sortie)apiType);


		}
		private void Update(kcsapi_sortie_battle_midnight data, ApiTypes apiType)
		{
			if (apiType == ApiTypes.sortie_battle_midnight_sp)
			{
				// 적, 아군 함대 정보 갱신
				this.UpdateFleets(data.api_deck_id, data, data.api_formation);

				// 체력 갱신
				this.UpdateMaxHP(data.api_f_maxhps, data.api_e_maxhps);
				this.UpdateBefHP(data.api_f_nowhps, data.api_e_nowhps);
			}

			// 체력 갱신
			this.UpdateNowHP(data.api_f_nowhps, data.api_e_nowhps);

			// 전투 계산
			if (apiType == ApiTypes.sortie_battle_midnight_sp)
				battleCalculator
					.Initialize(
						this.AliasFirst, this.AliasSecond,
						this.EnemyFirst, this.EnemySecond
					);

			battleCalculator.Update(data, (ApiTypes_SortieMidnight)apiType);
		}
		private void Update(kcsapi_sortie_airbattle data, ApiTypes apiType)
		{
			// 적, 아군 함대 정보 갱신
			this.UpdateFleets(data.api_deck_id, data, data.api_formation);

			// 체력 갱신
			this.UpdateMaxHP(data.api_f_maxhps, data.api_e_maxhps);
			this.UpdateBefHP(data.api_f_nowhps, data.api_e_nowhps);
			this.UpdateNowHP(data.api_f_nowhps, data.api_e_nowhps);

			// 전투 계산
			battleCalculator
				.Initialize(
					this.AliasFirst, this.AliasSecond,
					this.EnemyFirst, this.EnemySecond
				)
				.Update(data, (ApiTypes_SortieAirBattle)apiType);
		}

		private void Update(kcsapi_combined_battle data, ApiTypes apiType)
		{
			// 적, 아군 함대 정보 갱신
			this.UpdateFleets(data.api_deck_id, data, data.api_formation);

			// 체력 갱신
			this.UpdateMaxHP(data.api_f_maxhps, data.api_e_maxhps, data.api_f_maxhps_combined);
			this.UpdateBefHP(data.api_f_nowhps, data.api_e_nowhps, data.api_f_nowhps_combined);
			this.UpdateNowHP(data.api_f_nowhps, data.api_e_nowhps, data.api_f_nowhps_combined);

			// 전투 계산
			battleCalculator
				.Initialize(
					this.AliasFirst, this.AliasSecond,
					this.EnemyFirst, this.EnemySecond
				)
				.Update(data, (ApiTypes_CombinedBattle)apiType);
		}
		private void Update(kcsapi_combined_battle_each data, ApiTypes apiType)
		{
			// 적, 아군 함대 정보 갱신
			this.UpdateFleetsCombinedEnemy(data.api_deck_id, data, data.api_formation);

			// 체력 갱신
			this.UpdateMaxHP(data.api_f_maxhps, data.api_e_maxhps, data.api_f_maxhps_combined, data.api_e_maxhps_combined);
			this.UpdateBefHP(data.api_f_nowhps, data.api_e_nowhps, data.api_f_nowhps_combined, data.api_e_nowhps_combined);
			this.UpdateNowHP(data.api_f_nowhps, data.api_e_nowhps, data.api_f_nowhps_combined, data.api_e_nowhps_combined);

			// 전투 계산
			battleCalculator
				.Initialize(
					this.AliasFirst, this.AliasSecond,
					this.EnemyFirst, this.EnemySecond
				)
				.Update(data, (ApiTypes_CombinedBattleEC)apiType);
		}
		private void Update(kcsapi_combined_airbattle data, ApiTypes apiType)
		{
			// 적, 아군 함대 정보 갱신
			this.UpdateFleets(data.api_deck_id, data, data.api_formation);

			// 체력 갱신
			this.UpdateMaxHP(data.api_f_maxhps, data.api_e_maxhps, data.api_f_maxhps_combined);
			this.UpdateBefHP(data.api_f_nowhps, data.api_e_nowhps, data.api_f_nowhps_combined);
			this.UpdateNowHP(data.api_f_nowhps, data.api_e_nowhps, data.api_f_nowhps_combined);

			// 전투 계산
			battleCalculator
				.Initialize(
					this.AliasFirst, this.AliasSecond,
					this.EnemyFirst, this.EnemySecond
				)
				.Update(data, (ApiTypes_CombinedAirBattle)apiType);
		}

		private void Update(kcsapi_combined_battle_midnight data, ApiTypes apiType)
		{
			if (apiType == ApiTypes.combined_battle_midnight_sp)
			{
				// 적, 아군 함대 정보 갱신
				this.UpdateFleets(data.api_deck_id, data, data.api_formation);

				// 체력 갱신
				this.UpdateMaxHP(data.api_f_maxhps, data.api_e_maxhps, data.api_f_maxhps_combined);
				this.UpdateBefHP(data.api_f_nowhps, data.api_e_nowhps, data.api_f_nowhps_combined);
			}

			// 체력 갱신
			this.UpdateNowHP(data.api_f_nowhps, data.api_e_nowhps, data.api_f_nowhps_combined);

			if (apiType == ApiTypes.combined_battle_midnight_sp)
				battleCalculator
					.Initialize(
						this.AliasFirst, this.AliasSecond,
						this.EnemyFirst, this.EnemySecond
					);

			battleCalculator.Update(data, (ApiTypes_CombinedMidnight)apiType);
		}
		private void Update(kcsapi_combined_battle_midnight_ec data, ApiTypes apiType)
		{
			// 체력 갱신
			this.UpdateNowHP(data.api_f_nowhps, data.api_e_nowhps, data.api_f_nowhps_combined, data.api_e_nowhps_combined);

			// 전투 계산
			battleCalculator.Update(data);
		}
		private void Update(kcsapi_combined_battle_ec_nighttoday data, ApiTypes apiType)
		{
			// 적, 아군 함대 정보 갱신
			this.UpdateFleetsCombinedEnemy(data.api_deck_id, data);

			// 체력 갱신
			this.UpdateMaxHP(data.api_f_maxhps, data.api_e_maxhps, data.api_f_maxhps_combined, data.api_e_maxhps_combined);
			this.UpdateBefHP(data.api_f_nowhps, data.api_e_nowhps, data.api_f_nowhps_combined, data.api_e_nowhps_combined);
			this.UpdateNowHP(data.api_f_nowhps, data.api_e_nowhps, data.api_f_nowhps_combined, data.api_e_nowhps_combined);

			// 전투 계산
			battleCalculator.Update(data);
		}

		#endregion
	}

	public static class CommonBattleMembersExtensions
	{
		public static MastersShipData[] ToMastersShipDataArray(this ICommonBattleMembers data)
		{
			var master = KanColleClient.Current.Master;
			return data.api_ship_ke
				.Where(x => x != -1)
				.Select((x, i) => new MastersShipData(master.Ships[x])
				{
					Slots = data.api_eSlot[i]
						.Where(s => 0 < s)
						.Select(s => master.SlotItems[s])
						.Select(s => new ShipSlotData(s))
						.ToArray(),
				})
				.ToArray();
		}
		public static MastersShipData[] ToMastersShipDataArray(this ICommonEachBattleMembers data)
		{
			var master = KanColleClient.Current.Master;
			return data.api_ship_ke
				.Where(x => x != -1)
				.Select((x, i) => new MastersShipData(master.Ships[x])
				{
					Slots = data.api_eSlot[i]
						.Where(s => 0 < s)
						.Select(s => master.SlotItems[s])
						.Select(s => new ShipSlotData(s))
						.ToArray(),
				})
				.ToArray();
		}
		public static MastersShipData[] ToMastersSecondShipDataArray(this ICommonEachBattleMembers data)
		{
			var master = KanColleClient.Current.Master;
			return data.api_ship_ke_combined
				.Where(x => x != -1)
				.Select((x, i) => new MastersShipData(master.Ships[x])
				{
					Slots = data.api_eSlot_combined[i]
						.Where(s => 0 < s)
						.Select(s => master.SlotItems[s])
						.Select(s => new ShipSlotData(s))
						.ToArray(),
				})
				.ToArray();
		}
	}
}
