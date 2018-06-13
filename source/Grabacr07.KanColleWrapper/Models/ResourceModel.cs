using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grabacr07.KanColleWrapper.Models
{
	public class ResourceModel
	{
		public DateTime Date;

		public int Fuel;
		public int Ammo;
		public int Steel;
		public int Bauxite;

		public int RepairBucket;
		public int InstantConstruction;
		public int DevelopmentMaterial;
		public int ImprovementMaterial;

		public void Clear()
		{
			this.Fuel = this.Ammo = this.Steel = this.Bauxite = 0;
			this.RepairBucket = this.InstantConstruction = this.DevelopmentMaterial = this.ImprovementMaterial = 0;
		}

		#region Operator overrides
		public static ResourceModel operator +(ResourceModel r1, ResourceModel r2)
		{
			return new ResourceModel
			{
				Fuel = r1.Fuel+r2.Fuel,
				Ammo = r1.Ammo+r2.Ammo,
				Steel = r1.Steel+r2.Steel,
				Bauxite = r1.Bauxite+r2.Bauxite,

				RepairBucket = r1.RepairBucket + r2.RepairBucket,
				InstantConstruction = r1.InstantConstruction + r2.InstantConstruction,
				DevelopmentMaterial = r1.DevelopmentMaterial + r2.DevelopmentMaterial,
				ImprovementMaterial = r1.ImprovementMaterial + r2.ImprovementMaterial,

				Date = r1.Date>r2.Date?r1.Date:r2.Date
			};
		}
		public static ResourceModel operator -(ResourceModel r1, ResourceModel r2)
		{
			return new ResourceModel
			{
				Fuel = r1.Fuel - r2.Fuel,
				Ammo = r1.Ammo - r2.Ammo,
				Steel = r1.Steel - r2.Steel,
				Bauxite = r1.Bauxite - r2.Bauxite,

				RepairBucket = r1.RepairBucket - r2.RepairBucket,
				InstantConstruction = r1.InstantConstruction - r2.InstantConstruction,
				DevelopmentMaterial = r1.DevelopmentMaterial - r2.DevelopmentMaterial,
				ImprovementMaterial = r1.ImprovementMaterial - r2.ImprovementMaterial,

				Date = r1.Date > r2.Date ? r1.Date : r2.Date
			};
		}
		public static ResourceModel operator *(ResourceModel r, int m)
		{
			return new ResourceModel
			{
				Fuel = r.Fuel * m,
				Ammo = r.Ammo * m,
				Steel = r.Steel * m,
				Bauxite = r.Bauxite * m,

				RepairBucket = r.RepairBucket * m,
				InstantConstruction = r.InstantConstruction * m,
				DevelopmentMaterial = r.DevelopmentMaterial * m,
				ImprovementMaterial = r.ImprovementMaterial * m,

				Date = r.Date
			};
		}
		public static ResourceModel operator /(ResourceModel r, int m)
		{
			return new ResourceModel
			{
				Fuel = r.Fuel / m,
				Ammo = r.Ammo / m,
				Steel = r.Steel / m,
				Bauxite = r.Bauxite / m,

				RepairBucket = r.RepairBucket / m,
				InstantConstruction = r.InstantConstruction / m,
				DevelopmentMaterial = r.DevelopmentMaterial / m,
				ImprovementMaterial = r.ImprovementMaterial / m,

				Date = r.Date
			};
		}
		#endregion

		public override string ToString()
		{
			return string.Format(
				"Date={0}, Fuel={1}, Ammo={2}, Steel={3}, Bauxite={4}, RepairBucket={5}, InstantConstruction={6}, DevelopmentMaterial={7}, ImprovementMaterial={8}",
				(object)this.Date, (object)this.Fuel, (object)this.Ammo, (object)this.Steel, (object)this.Bauxite,
				(object)this.RepairBucket, (object)this.InstantConstruction, (object)this.DevelopmentMaterial, (object)this.ImprovementMaterial
			);
		}
	}
}
