using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Livet;

using Grabacr07.KanColleWrapper.Models.Raw;

namespace Grabacr07.KanColleWrapper.Models
{
	public class ExpeditionResult : ViewModel
	{
		#region Result, ResultText
		private int _Result { get; set; }
		public int Result
		{
			get { return this._Result; }
			set
			{
				if (this._Result != value)
				{
					this._Result = value;
					this.RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region Fuel
		private int _Fuel { get; set; }
		public int Fuel
		{
			get { return this._Fuel; }
			set
			{
				if (this._Fuel != value)
				{
					this._Fuel = value;
					this.RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region Ammo
		private int _Ammo { get; set; }
		public int Ammo
		{
			get { return this._Ammo; }
			set
			{
				if (this._Ammo != value)
				{
					this._Ammo = value;
					this.RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region Steel
		private int _Steel { get; set; }
		public int Steel
		{
			get { return this._Steel; }
			set
			{
				if (this._Steel != value)
				{
					this._Steel = value;
					this.RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region Bauxite
		private int _Bauxite { get; set; }
		public int Bauxite
		{
			get { return this._Bauxite; }
			set
			{
				if (this._Bauxite != value)
				{
					this._Bauxite = value;
					this.RaisePropertyChanged();
				}
			}
		}
		#endregion

		#region Items
		private ExpeditionResultItem[] _Items { get; set; }
		public ExpeditionResultItem[] Items
		{
			get { return this._Items; }
			set
			{
				if (this._Items != value)
				{
					this._Items = value;
					this.RaisePropertyChanged();
				}
			}
		}
		#endregion

		public ExpeditionResult()
		{
		}

		public ExpeditionResult(kcsapi_mission_result mission)
			=> this.Update(mission);

		public void Update(kcsapi_mission_result mission)
		{
			this.Result = mission.api_clear_result;

			this.Fuel = mission.api_get_material[0];
			this.Ammo = mission.api_get_material[1];
			this.Steel = mission.api_get_material[2];
			this.Bauxite = mission.api_get_material[3];

			var list = new List<ExpeditionResultItem>();
			if (mission.api_get_item1 != null) list.Add(new ExpeditionResultItem(mission.api_get_item1, mission.api_useitem_flag[0]));
			if (mission.api_get_item2 != null) list.Add(new ExpeditionResultItem(mission.api_get_item1, mission.api_useitem_flag[1]));

			this.Items = list.ToArray();

			/*
			 * mission.api_useitem_flag[i]
			 * 1: Repair bucket
			 * 2: Instant construction
			 * 3: Development material
			 * 4: BASED ON [id] property
			 * 5: Furniture coin
			 */
		}
	}

	public class ExpeditionResultItem : ViewModel
	{
		private kcsapi_mission_result_item source { get; }
		public int Kind { get; }

		public int Id => this.source.api_useitem_id;
		public string Name
		{
			get
			{
				switch (this.Kind)
				{
					case 1:
						return "고속수복재";
					case 2:
						return "고속건조재";
					case 3:
						return "개발자재";
					case 5:
						return "가구코인";

					case 4:
						return KanColleClient.Current.Translations.GetTranslation(
							KanColleClient.Current.Master.UseItems[this.Id]?.Name,
							TranslationType.Useitems,
							false
						);

					default:
						return "???";
				}
			}
		}
		public int Count => this.source.api_useitem_count;

		internal ExpeditionResultItem(kcsapi_mission_result_item item, int kind)
		{
			this.source = item;
			this.Kind = kind;
		}
	}
}
