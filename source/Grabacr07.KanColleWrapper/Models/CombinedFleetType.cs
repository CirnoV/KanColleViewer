using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grabacr07.KanColleWrapper.Models
{
	public enum CombinedFleetType
	{
		/// <summary>
		/// 연합 없음
		/// </summary>
		None = 0,

		/// <summary>
		/// 기동연합부대
		/// </summary>
		CarrierTaskForce = 1,

		/// <summary>
		/// 수상타격부대
		/// </summary>
		SurfaceTaskForce = 2,

		/// <summary>
		/// 수송연합부대
		/// </summary>
		TransportEscort = 3,
	}
}
