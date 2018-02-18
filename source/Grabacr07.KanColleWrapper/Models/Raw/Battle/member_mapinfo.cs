using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grabacr07.KanColleWrapper.Models.Raw
{
	public class kcsapi_member_mapinfo
	{
		public int api_id { get; set; }
		public int api_cleared { get; set; }
		public int api_exboss_flag { get; set; }
		public int api_defeat_count { get; set; }
		public kcsapi_eventmap api_eventmap { get; set; }
	}

	public class kcsapi_eventmap
	{
		public int api_now_maphp { get; set; }
		public int api_max_maphp { get; set; }
		public int api_dmg { get; set; }
		public int api_state { get; set; }
		public int? api_selected_rank { get; set; }
		public int api_gauge_type { get; set; }
	}
}
