using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grabacr07.KanColleViewer.QuestTracker.Models.Model
{
	class FleetDamages
	{
		public int Ship1 { get; set; }
		public int Ship2 { get; set; }
		public int Ship3 { get; set; }
		public int Ship4 { get; set; }
		public int Ship5 { get; set; }
		public int Ship6 { get; set; }

		public int[] ToArray()
		{
			return new[] { Ship1, Ship2, Ship3, Ship4, Ship5, Ship6 };
		}

		public FleetDamages Add(FleetDamages value)
		{
			return Parse(new[]
			{
				this.Ship1 + value.Ship1,
				this.Ship2 + value.Ship2,
				this.Ship3 + value.Ship3,
				this.Ship4 + value.Ship4,
				this.Ship5 + value.Ship5,
				this.Ship6 + value.Ship6,
			});
		}

		public static FleetDamages Parse(IEnumerable<int> damages)
		{
			if (damages == null) throw new ArgumentNullException();

			var arr = damages.ToArray();
			var dat = new int[6];
			for (var i = 0; i < arr.Length; i++)
				dat[i] = arr[i];
			/*
			if (arr.Length != 6)
				throw new ArgumentException("艦隊ダメージ配列の長さは6である必要があります。");
			*/

			return new FleetDamages
			{
				Ship1 = dat[0],
				Ship2 = dat[1],
				Ship3 = dat[2],
				Ship4 = dat[3],
				Ship5 = dat[4],
				Ship6 = dat[5]
			};
		}
	}

	internal static class FleetDamagesExtensions
	{
		public static FleetDamages ToFleetDamages(this IEnumerable<int> damages)
		{
			return FleetDamages.Parse(damages);
		}
	}
}
