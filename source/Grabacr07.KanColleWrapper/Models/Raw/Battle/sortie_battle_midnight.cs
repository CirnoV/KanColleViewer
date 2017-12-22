namespace Grabacr07.KanColleWrapper.Models.Raw
{
	/// <summary>
	/// 야전
	/// sortie_battle_midnight, sortie_battle_midnight_sp, practice_midnight_battle
	/// </summary>
	public class kcsapi_sortie_battle_midnight : ICommonFirstBattleMembers
	{
		public int api_deck_id { get; set; }
		public int[] api_ship_ke { get; set; }
		public int[] api_ship_lv { get; set; }
		public int[] api_f_nowhps { get; set; }
		public int[] api_f_maxhps { get; set; }
		public int[] api_e_nowhps { get; set; }
		public int[] api_e_maxhps { get; set; }
		public int[][] api_eSlot { get; set; }
		public int[][] api_eKyouka { get; set; }
		public int[][] api_fParam { get; set; }
		public int[][] api_eParam { get; set; }
		public int[] api_formation { get; set; }
		public int[] api_touch_plane { get; set; }
		public int[] api_flare_pos { get; set; }

		public int api_n_support_flag { get; set; }
		public kcsapi_battle_support_info api_n_support_info { get; set; }

		public kcsapi_battle_midnight_hougeki api_hougeki { get; set; }
	}
}
