namespace Grabacr07.KanColleWrapper.Models.Raw
{
	#region 항공전
	// 항공전 / 분식 항공전
	// kcsapi_battle_kouku, Api_Injection_Kouku
	public class kcsapi_battle_kouku
	{
		public int[][] api_plane_from { get; set; }
		public kcsapi_battle_stage1 api_stage1 { get; set; }
		public kcsapi_battle_stage2 api_stage2 { get; set; }
		public kcsapi_battle_stage3 api_stage3 { get; set; }
		public kcsapi_battle_stage3 api_stage3_combined { get; set; }
	}

	public class kcsapi_battle_stage1
	{
		public int api_f_count { get; set; }
		public int api_f_lostcount { get; set; }
		public int api_e_count { get; set; }
		public int api_e_lostcount { get; set; }
		public int api_disp_seiku { get; set; }
		public int[] api_touch_plane { get; set; }
	}

	public class kcsapi_battle_stage2
	{
		public int api_f_count { get; set; }
		public int api_f_lostcount { get; set; }
		public int api_e_count { get; set; }
		public int api_e_lostcount { get; set; }
		public kcsapi_battle_air_fire api_air_fire { get; set; }
	}

	public class kcsapi_battle_air_fire
	{
		public int api_idx { get; set; }
		public int api_kind { get; set; }
		public int[] api_use_items { get; set; }
	}

	public class kcsapi_battle_stage3
	{
		public int?[] api_frai_flag { get; set; }
		public int[] api_erai_flag { get; set; }
		public int?[] api_fbak_flag { get; set; }
		public int[] api_ebak_flag { get; set; }
		public int[] api_fcl_flag { get; set; }
		public int[] api_ecl_flag { get; set; }
		public decimal[] api_fdam { get; set; }
		public decimal[] api_edam { get; set; }
	}
	#endregion

	#region 지원함대
	public class kcsapi_battle_support_info
	{
		public kcsapi_battle_support_airattack api_support_airatack { get; set; }
		public kcsapi_battle_support_hourai api_support_hourai { get; set; }
	}

	public class kcsapi_battle_support_airattack : kcsapi_battle_kouku
	{
		public int api_deck_id { get; set; }
		public int[] api_ship_id { get; set; }
		public int[] api_undressing_flag { get; set; }
		public int[] api_stage_flag { get; set; }
	}

	public class kcsapi_battle_support_hourai
	{
		public int api_deck_id { get; set; }
		public int[] api_ship_id { get; set; }
		public int[] api_undressing_flag { get; set; }
		public int[] api_cl_list { get; set; }
		public decimal[] api_damage { get; set; }
		public decimal[] api_damage_combined { get; set; }
	}
	#endregion

	#region 선제뇌격 / 뇌격전
	public class kcsapi_battle_raigeki
	{
		public int[] api_frai { get; set; }
		public int[] api_erai { get; set; }
		public decimal[] api_fdam { get; set; }
		public decimal[] api_edam { get; set; }
		public decimal[] api_fydam { get; set; }
		public decimal[] api_eydam { get; set; }
		public int[] api_fcl { get; set; }
		public int[] api_ecl { get; set; }
	}
	#endregion

	#region 포격전
	public class kcsapi_battle_hougeki
	{
		public int[] api_at_eflag { get; set; }
		public int[] api_at_list { get; set; }
		public int[] api_at_type { get; set; }
		public int[][] api_df_list { get; set; }
		public object api_si_list { get; set; }
		public object api_cl_list { get; set; }
		public decimal[][] api_damage { get; set; }
	}

	public class kcsapi_battle_midnight_hougeki
	{
		public int[] api_at_eflag { get; set; }
		public int[] api_at_list { get; set; }
		public int[][] api_df_list { get; set; }
		public object[] api_si_list { get; set; }
		public object[] api_cl_list { get; set; }
		public int[] api_sp_list { get; set; }
		public decimal[][] api_damage { get; set; }
	}
	#endregion

	#region 기지항공대
	// 기지항공대 항공전
	public class kcsapi_battle_airbase_attack : kcsapi_battle_kouku
	{
		public int api_base_id { get; set; }
		public int[] api_stage_flag { get; set; }
		public kcsapi_battle_squadron_plane[] api_squadron_plane { get; set; }
	}
	public class kcsapi_battle_squadron_plane
	{
		public int api_mst_id { get; set; }
		public int api_count { get; set; }
	}

	// 기지항공대 분식 항공전
	public class kcsapi_battle_airbase_injection : kcsapi_battle_airbase_attack
	{
	}
	#endregion
}
