namespace Grabacr07.KanColleWrapper.Models.Raw
{
	/// <summary>
	/// 연합함대 - 수상부대 / 기동부대
	/// combined_battle, combined_battle_water
	/// </summary>
	public class kcsapi_combined_battle : ICommonFirstBattleMembers
	{
		public int api_deck_id { get; set; }
		public int[] api_ship_ke { get; set; }
		public int[] api_ship_lv { get; set; }
		public int[] api_f_nowhps { get; set; }
		public int[] api_f_maxhps { get; set; }
		public int[] api_e_nowhps { get; set; }
		public int[] api_e_maxhps { get; set; }
		public int[] api_f_nowhps_combined { get; set; }
		public int[] api_f_maxhps_combined { get; set; }
		public int api_midnight_flag { get; set; }
		public int[][] api_eSlot { get; set; }
		public int[][] api_eKyouka { get; set; }
		public int[][] api_fParam { get; set; }
		public int[][] api_eParam { get; set; }
		public int[][] api_fParam_combined { get; set; }
		public int[] api_search { get; set; }
		public int[] api_formation { get; set; }

		public kcsapi_battle_airbase_injection api_air_base_injection { get; set; }
		public kcsapi_battle_kouku api_injection_kouku { get; set; }
		public kcsapi_battle_airbase_attack[] api_air_base_attack { get; set; }
		public int[] api_stage_flag { get; set; }
		public kcsapi_battle_kouku api_kouku { get; set; }

		public int api_support_flag { get; set; }
		public kcsapi_battle_support_info api_support_info { get; set; }

		public int api_opening_taisen_flag { get; set; }
		public kcsapi_battle_hougeki api_opening_taisen { get; set; }
		public int api_opening_flag { get; set; }
		public kcsapi_battle_raigeki api_opening_atack { get; set; }

		public int[] api_hourai_flag { get; set; }
		public kcsapi_battle_hougeki api_hougeki1 { get; set; }
		public kcsapi_battle_hougeki api_hougeki2 { get; set; }
		public kcsapi_battle_hougeki api_hougeki3 { get; set; }
		public kcsapi_battle_raigeki api_raigeki { get; set; }
	}
}
