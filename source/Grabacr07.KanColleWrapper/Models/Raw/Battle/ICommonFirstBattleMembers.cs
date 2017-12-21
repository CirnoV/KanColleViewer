using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grabacr07.KanColleWrapper.Models.Raw
{
	interface ICommonFirstBattleMembers : ICommonBattleMembers
	{
		int[] api_formation { get; set; }
	}
}
