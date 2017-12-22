namespace Grabacr07.KanColleWrapper.Models.Raw
{
	/// <summary>
	/// vs 심해연합함대 - 야전
	/// </summary>
	public class kcsapi_combined_battle_midnight_ec : ICommonEachBattleMembers
	{
		public int[] api_active_deck { get; set; }
		public int api_deck_id { get; set; }
		public int[] api_ship_ke { get; set; }
		public int[] api_ship_ke_combined { get; set; }
		public int[] api_ship_lv { get; set; }
		public int[] api_ship_lv_combined { get; set; }
		public int[] api_f_nowhps { get; set; }
		public int[] api_f_maxhps { get; set; }
		public int[] api_e_nowhps { get; set; }
		public int[] api_e_maxhps { get; set; }
		public int[] api_f_nowhps_combined { get; set; }
		public int[] api_f_maxhps_combined { get; set; }
		public int[] api_e_nowhps_combined { get; set; }
		public int[] api_e_maxhps_combined { get; set; }
		public int[][] api_eSlot { get; set; }
		public int[][] api_eSlot_combined { get; set; }
		public int[][] api_eKyouka { get; set; }
		public int[][] api_fParam { get; set; }
		public int[][] api_eParam { get; set; }
		public int[][] api_fParam_combined { get; set; }
		public int[][] api_eParam_combined { get; set; }
		public int[] api_touch_plane { get; set; }
		public int[] api_flare_pos { get; set; }

		public int api_n_support_flag { get; set; }
		public kcsapi_battle_support_info api_n_support_info { get; set; }

		public kcsapi_battle_midnight_hougeki api_hougeki { get; set; }
	}

}
