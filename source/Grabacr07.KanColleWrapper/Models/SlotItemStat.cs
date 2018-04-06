using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grabacr07.KanColleWrapper.Models
{
	public class SlotItemStat
	{
		/// <summary>
		/// 화력 수치
		/// </summary>
		public double Firepower { get; set; }

		/// <summary>
		/// 야전 화력 수치
		/// </summary>
		public double NightFirepower { get; set; }

		/// <summary>
		/// 명중 수치
		/// </summary>
		public double Hit { get; set; }

		/// <summary>
		/// 뇌장 수치
		/// </summary>
		public double Torpedo { get; set; }

		/// <summary>
		/// 뇌격 명중 수치
		/// </summary>
		public double TorpedoHit { get; set; }

		/// <summary>
		/// 대공 수치
		/// </summary>
		public double AA { get; set; }

		/// <summary>
		/// 함대 방공 수치
		/// </summary>
		public double FleetAA { get; set; }

		/// <summary>
		/// 색적 수치
		/// </summary>
		public double LoS { get; set; }

		/// <summary>
		/// 대잠 수치
		/// </summary>
		public double ASW { get; set; }

		/// <summary>
		/// 대잠 명중 수치
		/// </summary>
		public double ASWHit { get; set; }

		/// <summary>
		/// 장갑 수치
		/// </summary>
		public double Armor { get; set; }

		/// <summary>
		/// 회피 수치
		/// </summary>
		public double Evade { get; set; }

		/// <summary>
		/// 폭장 수치
		/// </summary>
		public double Bomb { get; set; }

		/// <summary>
		/// 원정 보너스
		/// </summary>
		public double ExpeditionBonus { get; set; }

		/// <summary>
		/// 포대 특효 수치
		/// </summary>
		public double TurrentEfficacy { get; set; }

		#region 알려지지 않은 수치
		/// <summary>
		/// 피격 확률
		/// </summary>
		public double DamageChance { get; set; }

		/// <summary>
		/// 적 컷인 확률
		/// </summary>
		public double EnemyCutInChance { get; set; }
		#endregion
	}
}
