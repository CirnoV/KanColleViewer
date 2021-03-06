using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.IO;
using System.Text;
using Grabacr07.KanColleViewer.Models;
using Grabacr07.KanColleViewer.Models.Settings;
using Grabacr07.KanColleWrapper;
using MetroTrilithon.Mvvm;

namespace Grabacr07.KanColleViewer.ViewModels.Catalogs
{
	public class ShipCatalogWindowViewModel : WindowViewModel
	{
		private readonly Subject<Unit> updateSource = new Subject<Unit>();
		private readonly Homeport homeport = KanColleClient.Current.Homeport;
		private SallyArea[] sallyAreas;

		public ShipCatalogWindowSettings Settings { get; }

		public ShipCatalogSortWorker SortWorker { get; }
		public IReadOnlyCollection<ShipTypeViewModel> ShipTypes { get; }

		public ShipLevelFilter ShipLevelFilter { get; }
		public ShipLockFilter ShipLockFilter { get; }
		public ShipSpeedFilter ShipSpeedFilter { get; }
		public ShipModernizeFilter ShipModernizeFilter { get; }
		public ShipRemodelingFilter ShipRemodelingFilter { get; }
		public ShipExpeditionFilter ShipExpeditionFilter { get; }
		public ShipSallyAreaFilter ShipSallyAreaFilter { get; }
		public ShipNameSearchFilter ShipNameSearchFilter { get; private set; }
		public ShipDamagedFilter ShipDamagedFilter { get; }
		public ShipConditionFilter ShipConditionFilter { get; }
		public ShipExSlotFilter ShipExSlotFilter { get; }
		public ShipFleetFilter ShipFleetFilter { get; }
		public DaihatsueFilter DaihatsueFilter { get; }

		private string _FleetDisplayData { get; set; }
		public string FleetDisplayData
		{
			get { return this._FleetDisplayData; }
			private set
			{
				if(value != this._FleetDisplayData)
				{
					this._FleetDisplayData = value;
					this.RaisePropertyChanged();
				}
			}
		}

		public bool CheckAllShipTypes
		{
			get { return this.ShipTypes.All(x => x.IsSelected); }
			set
			{
				foreach (var type in this.ShipTypes) type.Set(value);
				this.Update();
			}
		}

		#region Ships 変更通知プロパティ

		private IReadOnlyCollection<ShipViewModel> _Ships;

		public IReadOnlyCollection<ShipViewModel> Ships
		{
			get { return this._Ships; }
			set
			{
				if (this._Ships != value)
				{
					this._Ships = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region IsOpenFilterSettings 変更通知プロパティ

		private bool _IsOpenFilterSettings;

		public bool IsOpenFilterSettings
		{
			get { return this._IsOpenFilterSettings; }
			set
			{
				if (this._IsOpenFilterSettings != value)
				{
					this._IsOpenFilterSettings = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region IsOpenSortSettings 変更通知プロパティ

		private bool _IsOpenSortSettings;

		public bool IsOpenSortSettings
		{
			get { return this._IsOpenSortSettings; }
			set
			{
				if (this._IsOpenSortSettings != value)
				{
					this._IsOpenSortSettings = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region IsReloading 変更通知プロパティ

		private bool _IsReloading;

		public bool IsReloading
		{
			get { return this._IsReloading; }
			set
			{
				if (this._IsReloading != value)
				{
					this._IsReloading = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		public ShipCatalogWindowViewModel()
		{
			this.Title = "소유 칸무스 목록";
			this.IsOpenFilterSettings = true;
			this.Settings = new ShipCatalogWindowSettings();

			this.SortWorker = new ShipCatalogSortWorker();

			this.ShipTypes = KanColleClient.Current.Master.ShipTypes
				.Where(kvp => !(kvp.Value.Id == 15 && kvp.Value.RawData.api_name == "補給艦")) // おそらく敵艦用と思われる補給艦を除外
				.Select(kvp => new ShipTypeViewModel(kvp.Value)
				{
					IsSelected = true,
					SelectionChangedAction = () => this.Update()
				})
				.ToList();

			this.ShipLevelFilter = new ShipLevelFilter(this.Update);
			this.ShipLockFilter = new ShipLockFilter(this.Update);
			this.ShipSpeedFilter = new ShipSpeedFilter(this.Update);
			this.ShipModernizeFilter = new ShipModernizeFilter(this.Update);
			this.ShipRemodelingFilter = new ShipRemodelingFilter(this.Update);
			this.ShipExpeditionFilter = new ShipExpeditionFilter(this.Update);
			this.ShipSallyAreaFilter = new ShipSallyAreaFilter(this.Update);
			this.ShipNameSearchFilter = new ShipNameSearchFilter(this.Update);
			this.ShipDamagedFilter = new ShipDamagedFilter(this.Update);
			this.ShipConditionFilter = new ShipConditionFilter(this.Update);
			this.ShipExSlotFilter = new ShipExSlotFilter(this.Update);
			this.ShipFleetFilter = new ShipFleetFilter(this.Update);
			this.DaihatsueFilter = new DaihatsueFilter(this.Update);

			this.updateSource
				.Do(_ => this.IsReloading = true)
				.SelectMany(_ => this.GetSallyAreaAsync())
				.SelectMany(x => this.UpdateAsync(x))
				.Do(_ => this.IsReloading = false)
				.Subscribe()
				.AddTo(this);

			this.homeport.Organization
				.Subscribe(nameof(Organization.Ships), this.Update)
				.AddTo(this);
		}

		public void Update()
		{
			this.ShipExpeditionFilter.SetFleets(this.homeport.Organization.Fleets);

			this.RaisePropertyChanged(nameof(this.CheckAllShipTypes));
			this.updateSource.OnNext(Unit.Default);

			this.FleetDisplayData = this.FleetDisplayCode();
		}

		private IObservable<Unit> UpdateAsync(SallyArea[] areas)
		{
			return Observable.Start(() =>
			{
				try
				{
					var list = this.homeport.Organization.Ships
						.Select(kvp => kvp.Value)
						.Where(x => this.ShipTypes.Where(t => t.IsSelected).Any(t => x.Info.ShipType.Id == t.Id))
						.Where(this.ShipLevelFilter.Predicate)
						.Where(this.ShipLockFilter.Predicate)
						.Where(this.ShipSpeedFilter.Predicate)
						.Where(this.ShipModernizeFilter.Predicate)
						.Where(this.ShipRemodelingFilter.Predicate)
						.Where(this.ShipExpeditionFilter.Predicate)
						.Where(this.ShipSallyAreaFilter.Predicate)
						.Where(this.ShipNameSearchFilter.Predicate)
						.Where(this.ShipDamagedFilter.Predicate)
						.Where(this.ShipConditionFilter.Predicate)
						.Where(this.ShipExSlotFilter.Predicate)
						.Where(this.ShipFleetFilter.Predicate)
						.Where(this.DaihatsueFilter.Predicate)
						.ToArray();

					this.Ships = this.SortWorker.Sort(list)
						.Select((x, i) => new ShipViewModel(i + 1, x, areas.FirstOrDefault(y => y.Area == x.SallyArea)))
						.ToList();
				}
				catch { }
			});
		}

		private IObservable<SallyArea[]> GetSallyAreaAsync()
		{
			return this.sallyAreas == null
				? SallyArea.GetAsync()
					.ToObservable()
					.Do(x =>
					{
						// これはひどい
						this.sallyAreas = x;
						this.ShipSallyAreaFilter.SetSallyArea(x);
					})
				: Observable.Return(this.sallyAreas);
		}

		public void SetShipType(int[] ids)
		{
			foreach (var type in this.ShipTypes) type.Set(ids.Any(id => type.Id == id));
			this.Update();
		}

		public string FleetDisplayCode()
		{
			var output = new StringBuilder(".2");

			var temp = this.homeport.Organization.Ships.Values.ToArray();
			var trees = new List<int[]>();
			while (temp.Length > 0)
			{
				var tree = new List<int>();
				var ship = temp.First();

				// Find root
				var master = KanColleClient.Current.Master;
				var root = ship.Info;

				while (master.Ships.Any(x => x.Value.RawData.api_aftershipid == root.Id.ToString()))
					root = master.Ships.FirstOrDefault(x => x.Value.RawData.api_aftershipid == root.Id.ToString()).Value;

				do
				{
					tree.Add(root.Id);
					root = master.Ships.FirstOrDefault(x => x.Value.Id.ToString() == root.RawData.api_aftershipid).Value;
				} while (root != null && !tree.Contains(root.Id));

				trees.Add(tree.ToArray());
				temp = temp.Where(x => !tree.Contains(x.Info.Id)).ToArray();
			}

			temp = this.homeport.Organization.Ships.Values.ToArray();
			var groups = temp.GroupBy(x => trees.FindIndex(y => y.Contains(x.Info.Id)));
			foreach (var group in groups)
			{
				var tree = trees[group.Key];
				var ships = new List<string>();

				foreach (var ship in group)
					ships.Add($"{ship.Level}.{Array.IndexOf(tree, ship.Info.Id) + 1}");

				output.AppendFormat("|{0}:", tree.First());
				output.Append(string.Join(",", ships));
			}
			return output.ToString();
		}

		public void Sort(SortableColumn column)
		{
			this.SortWorker.SetFirst(column);
			this.Update();
		}
	}
}
