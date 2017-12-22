using System.Linq;
using Grabacr07.KanColleWrapper;

namespace Grabacr07.KanColleWrapper.Models.Raw
{
	public interface ICommonEachBattleMembers : ICommonBattleMembers
	{
		int[] api_ship_ke_combined { get; set; }
		int[] api_ship_lv_combined { get; set; }
		int[] api_e_nowhps_combined { get; set; }
		int[] api_e_maxhps_combined { get; set; }
		int[][] api_eSlot_combined { get; set; }
		int[][] api_eParam_combined { get; set; }
	}
}
