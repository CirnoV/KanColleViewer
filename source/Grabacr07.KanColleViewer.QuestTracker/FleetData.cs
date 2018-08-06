using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models;

namespace Grabacr07.KanColleViewer.QuestTracker.Models
{
	public class FleetData
	{
		public FleetType FleetType { get; set; }
		public string Name { get; set; }
		public string AttackGauge { get; set; }
		public bool IsCritical { get; set; }
		public IEnumerable<ShipData> Ships { get; set; }
		public Formation Formation { get; set; }

		public FleetData() : this(new ShipData[0], Formation.없음, "", FleetType.EnemyFirst)
		{
		}

		public FleetData(IEnumerable<ShipData> ships, Formation formation, string name, FleetType type)
		{
			this.Ships = ships;
			this.Formation = formation;
			this.Name = name;
			this.FleetType = type;

			if (type == FleetType.EnemyFirst || type == FleetType.EnemySecond) return;
			this.IsCritical = this.Ships?
				.Any(x => (x.NowHP / (double)x.MaxHP <= 0.25) && (x.NowHP / (double)x.MaxHP > 0))
				?? false;

			//this._AirSuperiorityPotential = this._Ships
			//	.SelectMany(s => s.Slots)
			//	.Where(s => s.Source.IsAirSuperiorityFighter)
			//	.Sum(s => (int)(s.AA * Math.Sqrt(s.Current)))
			//	;
		}

		public FleetData Clone()
		{
			return new FleetData
			{
				FleetType = this.FleetType,
				Name = this.Name,
				AttackGauge = this.AttackGauge,
				IsCritical = this.IsCritical,
				Ships = this.Ships.Select(x => x.Clone()).ToArray(),
				Formation = this.Formation
			};
		}
	}

	public static class FleetDataExtensions
	{
		/// <summary>
		/// Actionを使用して値を設定
		/// Zipするので要素数が少ない方に合わせられる
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="source"></param>
		/// <param name="values"></param>
		/// <param name="setter"></param>
		public static void SetValues<TSource, TValue>(
			this IEnumerable<TSource> source,
			IEnumerable<TValue> values,
			Action<TSource, TValue> setter)
		{
			source.Zip(values, (s, v) => new { s, v })
				.ToList()
				.ForEach(x => setter(x.s, x.v));
		}
		public static bool CriticalCheck(this FleetData fleet)
		{
			if (fleet.Ships
					.Where(x => !x.Situation.HasFlag(ShipSituation.DamageControlled))
					.Where(x => !x.Situation.HasFlag(ShipSituation.Evacuation))
					.Where(x => !x.Situation.HasFlag(ShipSituation.Tow))
					.Any(x => x.Situation.HasFlag(ShipSituation.HeavilyDamaged)))
				return true;
			else return false;
		}
	}
}
