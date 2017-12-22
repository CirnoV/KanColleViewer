namespace Grabacr07.KanColleWrapper.Models.Raw
{
	public class kcsapi_map_start_next
	{
		public int api_rashin_flg { get; set; }
		public int api_rashin_id { get; set; }
		public int api_maparea_id { get; set; }
		public int api_mapinfo_no { get; set; }
		public int api_no { get; set; }
		public int api_color_no { get; set; }
		public int api_event_id { get; set; }
		public int api_event_kind { get; set; }
		public int api_next { get; set; }
		public int api_bosscell_no { get; set; }
		public int api_bosscomp { get; set; }
		public kcsapi_sortie_airsearch api_airsearch { get; set; }
		public kcsapi_eventmap api_eventmap { get; set; }
		public int api_from_no { get; set; }
		public kcsapi_sortie_distance_data[] api_distance_data { get; set; }
		// 以下next
		public int api_comment_kind { get; set; }
		public int api_production_kind { get; set; }
		public kcsapi_sortie_enemy api_enemy { get; set; }
		public kcsapi_sortie_happening api_happening { get; set; }
		public kcsapi_sortie_itemget[] api_itemget { get; set; }
		public kcsapi_select_route api_select_route { get; set; }
		public int api_ration_flag { get; set; }
		public kcsapi_sortie_itemget api_itemget_eo_comment { get; set; }

		public int? api_m1 { get; set; }
	}

	public class kcsapi_sortie_airsearch
	{
		public int api_plane_type { get; set; }
		public int api_result { get; set; }
	}

	public class kcsapi_sortie_distance_data
	{
		public int api_mapcell_id { get; set; }
		public int api_distance { get; set; }
	}

	public class kcsapi_sortie_enemy
	{
		//public int api_enemy_id { get; set; }
		public int api_result { get; set; }
		public string api_result_str { get; set; }
	}

	public class kcsapi_sortie_happening
	{
		public int api_type { get; set; }
		public int api_count { get; set; }
		public int api_usemst { get; set; }
		public int api_mst_id { get; set; }
		public int api_icon_id { get; set; }
		public int api_dentan { get; set; }
	}

	public class kcsapi_sortie_itemget
	{
		public int api_getcount { get; set; }
		public int api_icon_id { get; set; }
		public int api_id { get; set; }
		public string api_name { get; set; }
		public int api_usemst { get; set; }
	}
	public class kcsapi_select_route
	{
		public int[] api_select_cells { get; set; }
	}

}
