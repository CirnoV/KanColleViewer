using System.Linq;
using Grabacr07.KanColleWrapper;

namespace Grabacr07.KanColleWrapper.Models.Raw
{
	public interface ICommonBattleMembers
	{
		int[] api_ship_ke { get; set; }
		int[] api_ship_lv { get; set; }
		int[] api_f_nowhps { get; set; }
		int[] api_f_maxhps { get; set; }
		int[] api_e_nowhps { get; set; }
		int[] api_e_maxhps { get; set; }
		int[][] api_eSlot { get; set; }
		int[][] api_eKyouka { get; set; }
		int[][] api_fParam { get; set; }
		int[][] api_eParam { get; set; }
	}
}
