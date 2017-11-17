using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grabacr07.KanColleWrapper.Models.Raw;
using Grabacr07.KanColleViewer.QuestTracker.Models.Model;

namespace Grabacr07.KanColleViewer.QuestTracker.Models.Extensions
{
	internal static class ModelExtension
	{
		private static readonly FleetDamages defaultValue = new FleetDamages();

		#region 지원함대
		public static FleetDamages GetEnemyDamages(this kcsapi_data_support_info support)
			=> support?.api_support_airatack?.api_stage3?.api_edam?.GetDamages()
				?? support?.api_support_hourai?.api_damage?.GetDamages()
				?? defaultValue;

		public static FleetDamages GetEachFirstEnemyDamages(this kcsapi_data_support_info support)
			=> support?.api_support_airatack?.api_stage3?.api_edam?.GetEachDamages()
			   ?? support?.api_support_hourai?.api_damage?.GetEachDamages()
			   ?? defaultValue;

		public static FleetDamages GetEachSecondEnemyDamages(this kcsapi_data_support_info support)
			=> support?.api_support_airatack?.api_stage3?.api_edam?.GetEachDamages(true)
			   ?? support?.api_support_hourai?.api_damage?.GetEachDamages(true)
			   ?? defaultValue;
		#endregion

		#region 포격전
		public static FleetDamages GetEnemyDamages(this kcsapi_data_hougeki hougeki)
			=> hougeki?.api_damage?.GetEnemyDamages(hougeki.api_df_list, hougeki.api_at_eflag)
				?? defaultValue;

		public static FleetDamages GetEachFirstEnemyDamages(this kcsapi_data_hougeki hougeki)
			=> hougeki?.api_damage?.GetEachEnemyDamages(hougeki.api_df_list, hougeki.api_at_eflag)
			   ?? defaultValue;
		public static FleetDamages GetEachSecondEnemyDamages(this kcsapi_data_hougeki hougeki)
			=> hougeki?.api_damage?.GetEachEnemyDamages(hougeki.api_df_list, hougeki.api_at_eflag, true)
			   ?? defaultValue;
		#endregion

		#region 야전
		public static FleetDamages GetEnemyDamages(this kcsapi_data_midnight_hougeki hougeki)
			=> hougeki?.api_damage?.GetEnemyDamages(hougeki.api_df_list, hougeki.api_at_eflag)
				?? defaultValue;
		#endregion

		#region 항공전
		public static FleetDamages GetEnemyDamages(this kcsapi_data_kouku kouku)
			=> kouku?.api_stage3?.api_edam?.GetDamages()
				?? defaultValue;

		public static FleetDamages GetSecondEnemyDamages(this kcsapi_data_kouku kouku)
			=> kouku?.api_stage3_combined?.api_edam?.GetDamages()
			   ?? defaultValue;
		#endregion

		#region 기지항공대
		public static FleetDamages GetEnemyDamages(this kcsapi_data_airbaseattack[] air_base)
			=> air_base?.Select(x => x?.api_stage3?.api_edam?.GetDamages() ?? defaultValue)
				?.Aggregate((a, b) => a.Add(b)) ?? defaultValue;

		public static FleetDamages GetEachFirstEnemyDamages(this kcsapi_data_airbaseattack[] attacks)
			=> attacks?.Select(x => x?.api_stage3?.api_edam?.GetDamages() ?? defaultValue)
			?.Aggregate((a, b) => a.Add(b)) ?? defaultValue;

		public static FleetDamages GetEachSecondEnemyDamages(this kcsapi_data_airbaseattack[] attacks)
			=> attacks?.Select(x => x?.api_stage3_combined?.api_edam?.GetDamages() ?? defaultValue)
			?.Aggregate((a, b) => a.Add(b)) ?? defaultValue;
		#endregion

		#region 기지항공대 분식
		public static FleetDamages GetEnemyDamages(this kcsapi_data_airbase_injection injection)
			=> injection?.api_stage3?.api_edam?.GetDamages()
			   ?? defaultValue;

		public static FleetDamages GetSecondEnemyDamages(this kcsapi_data_airbase_injection injection)
			=> injection?.api_stage3_combined?.api_edam?.GetDamages()
			   ?? defaultValue;
		#endregion

		#region 선제뇌격 / 뇌격전
		public static FleetDamages GetEnemyDamages(this kcsapi_data_raigeki raigeki)
			=> raigeki?.api_edam?.GetDamages()
				?? defaultValue;

		public static FleetDamages GetEachFirstEnemyDamages(this kcsapi_data_raigeki raigeki)
			=> raigeki?.api_edam?.GetEachDamages()
			   ?? defaultValue;

		public static FleetDamages GetEachSecondEnemyDamages(this kcsapi_data_raigeki raigeki)
			=> raigeki?.api_edam?.GetEachDamages(true)
			   ?? defaultValue;
		#endregion


		public static IEnumerable<T> GetFriendData<T>(this IEnumerable<T> source, int origin = 0)
			=> source.Skip(origin).Take(6);

		public static IEnumerable<T> GetEnemyData<T>(this IEnumerable<T> source, int origin = 0)
			=> source.Skip(origin + 6).Take(6);

		public static IEnumerable<T> GetEachFriendData<T>(this IEnumerable<T> source, int origin = 0)
			=> source.Skip(origin).Take(6);

		public static IEnumerable<T> GetEachEnemyData<T>(this IEnumerable<T> source, int origin = 0)
			=> source.Skip(origin + 12).Take(6);


		public static FleetDamages GetDamages(this decimal[] damages)
			=> damages
				.GetFriendData() //敵味方共通
				.Select(Convert.ToInt32)
				.ToArray()
				.ToFleetDamages();

		public static FleetDamages GetEachDamages(this decimal[] damages, bool IsSecond = false)
			=> damages
				.GetFriendData(IsSecond ? 6 : 0) //敵味方共通
				.Select(Convert.ToInt32)
				.ToArray()
				.ToFleetDamages();

		public static FleetDamages GetEnemyDamages(this object[] damages, object[] df_list, int[] eflag)
			=> damages
				// .ToIntArray()
				.ToIntArray2()
				.ToSortedDamages(df_list.ToIntArray2(), eflag, 0) // 적군이 쏘는게 아니라 아군이 쏘는거니까 0
																  // .GetEnemyData(0)
				.ToFleetDamages();

		public static FleetDamages GetEachEnemyDamages(this object[] damages, object[] df_list, int[] at_eflag, bool IsSecond = false)
			=> damages
				.ToIntArray2()
				.ToSortedDamages(df_list.ToIntArray2(), at_eflag, 0)
				.ToFleetDamages();

		private static int[] ToIntArray(this object[] damages)
			=> damages
				.Where(x => x is Array)
				.Select(x => ((Array)x).Cast<object>())
				.SelectMany(x => x.Select(Convert.ToInt32))
				.ToArray();

		private static int[][] ToIntArray2(this object[] damages)
			=> damages
				.Where(x => x is Array)
				.Select(x => ((Array)x).Cast<object>())
				.Select(x => x.Select(Convert.ToInt32).ToArray())
				.ToArray();

		/// <summary>
		/// フラット化したapi_damageとapi_df_listを元に
		/// 自軍6隻＋敵軍6隻の長さ12のダメージ合計配列を作成
		/// </summary>
		/// <param name="damages">api_damage</param>
		/// <param name="dfList">api_df_list</param>
		/// <returns></returns>
		private static int[] ToSortedDamages(this int[][] damages, int[][] dfList, int[] _eflag, int target)
		{
			var zip = damages
				.Zip(
					dfList,
					(_da, _df)
						=> _da.Zip(
							_df,
							(da, df) => new { df, da }
						)
				)
				.Zip(_eflag, (data, eflag) => new { data, eflag });

			var ret = new int[6];
			foreach (var da in zip.Where(x => x.eflag == target))
				foreach (var d in da.data)
					ret[d.df] += d.da;

			return ret;
		}
	}
}
