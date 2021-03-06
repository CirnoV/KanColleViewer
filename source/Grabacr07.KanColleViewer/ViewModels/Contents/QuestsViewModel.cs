using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Properties;
using Grabacr07.KanColleWrapper;
using Livet.EventListeners;

using Grabacr07.KanColleViewer.Models.Settings;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleViewer.Models.QuestTracker;
using Grabacr07.KanColleViewer.ViewModels.Contents.Fleets;

namespace Grabacr07.KanColleViewer.ViewModels.Contents
{
	public class QuestsViewModel : TabItemViewModel
	{
		public override string Name
		{
			get { return Resources.Quests; }
			protected set { throw new NotImplementedException(); }
		}

		private int? _badge = null;

		public QuestViewModel[] Current
			=> this.OriginalQuests
				.Where(x => x.State != QuestState.None)
				.Distinct(x => x.Id)
				.OrderBy(x => x.Id)
				.ToArray();

		#region Quests 変更通知プロパティ

		/// <summary>
		/// Filtered Quest List
		/// </summary>
		private QuestViewModel[] _OriginalQuests;
		private QuestViewModel[] OriginalQuests
		{
			get { return this._OriginalQuests; }
			set
			{
				if (this._OriginalQuests != value)
				{
					this._OriginalQuests = value;
					this.RaisePropertyChanged(nameof(this.Quests));
					this.RaisePropertyChanged(nameof(this.Current));
					this.RaisePropertyChanged(nameof(this.IsEmpty));
					this.RaisePropertyChanged(nameof(this.IsUntaken));
				}
			}
		}

		/// <summary>
		/// Original Quest List
		/// </summary>
		public QuestViewModel[] Quests
		{
			get
			{
				IEnumerable<QuestViewModel> temp;
				var onAnyTabs = IsOnAnyTab;
				var tabId = CurrentTab;
				switch (tabId)
				{
					case 0: // All
						temp = OriginalQuests;
						break;
					case 9: // Current
						temp = OriginalQuests.Where(x => x.State != QuestState.None);
						break;
					case 1: // Daily
						temp = OriginalQuests.Where(x => x.Type == QuestType.Daily);
						break;
					case 2: // Weekly
						temp = OriginalQuests.Where(x => x.Type == QuestType.Weekly);
						break;
					case 3: // Monthly
						temp = OriginalQuests.Where(x => x.Type == QuestType.Monthly);
						break;
					case 4: // Once
						temp = OriginalQuests.Where(x => x.Type == QuestType.OneTime);
						break;
					case 5: // Others
						temp = OriginalQuests.Where(x => x.Type == QuestType.Other);
						break;
					default: // Unknown
						temp = this.OriginalQuests = new QuestViewModel[0];
						break;
				}
				if (QuestCategorySelected.Display != CategoryToColor(QuestCategory.Other))
					temp = temp.Where(x => CategoryToColor(x.Category) == QuestCategorySelected.Display);

				if (!onAnyTabs) tabId = 0;

				if (tabId == 9 && IsOnAnyTab && IsNoTakeOnTab)
				{
					temp = temp
						.Where(x => x.Tab != 9)
						.Where(x => x.State != QuestState.None)
						.OrderBy(x => x.Id)
						.Distinct(x => x.Id);
				}
				else
				{
					temp = temp
						.Where(x => x.Tab == tabId)
						.OrderBy(x => x.Id)
						.Distinct(x => x.Id);
				}

				return ComputeQuestPage(temp.ToArray());
			}
		}

		#endregion

		#region IsUntaken 変更通知プロパティ

		private Dictionary<int, bool> IsUntakenTable;
		public bool IsUntaken
		{
			get
			{
				var tab = !IsOnAnyTab ? 0 : CurrentTab;
				if (CurrentTab == 9 && IsOnAnyTab && IsNoTakeOnTab)
					return this.IsUntakenTable == null
						? true
						: this.IsUntakenTable.Any(x => x.Value);
				else
					return this.IsUntakenTable == null
						? true
						: !this.IsUntakenTable.ContainsKey(tab) || this.IsUntakenTable[tab];
			}
		}

		#endregion

		#region IsEmpty 変更通知プロパティ

		private Dictionary<int, bool> IsEmptyTable;
		public bool IsEmpty
		{
			get
			{
				var tab = !IsOnAnyTab ? 0 : CurrentTab;
				if (CurrentTab == 9 && IsOnAnyTab && IsNoTakeOnTab)
					return this.IsEmptyTable == null
						? false
						: this.IsEmptyTable.All(x => x.Value);
				else
					return this.IsEmptyTable == null
						? false
						: this.IsEmptyTable.ContainsKey(tab) && this.IsEmptyTable[tab];
			}
		}

		#endregion

		private int CurrentTab => GetTabIdByKey(SelectedItem.Key);

		private bool IsOnAnyTab => KanColleClient.Current.Settings.QuestOnAnyTabs;
		private bool IsNoTakeOnTab => KanColleClient.Current.Settings.QuestNoTakeOnTab;

		#region QuestCategories 프로퍼티

		public ICollection<QuestCategoryViewModel> QuestCategories { get; }

		#endregion

		#region QuestCategorySelected 변경통지 프로퍼티

		private QuestCategoryViewModel _QuestCategorySelected;
		public QuestCategoryViewModel QuestCategorySelected
		{
			get { return this._QuestCategorySelected; }
			set
			{
				if (this._QuestCategorySelected != value)
				{
					this._QuestCategorySelected = value;
					this.RaisePropertyChanged();
					this.RaisePropertyChanged(nameof(this.Quests));
				}
			}
		}

		#endregion

		public IList<KeyNameTabItemViewModel> TabItems { get; set; }

		private KeyNameTabItemViewModel _SelectedItem;
		public KeyNameTabItemViewModel SelectedItem
		{
			get { return this._SelectedItem; }
			set
			{
				if (this._SelectedItem != value)
				{
					this._SelectedItem = value;
					this.RaisePropertyChanged(nameof(this.Quests));
					this.RaisePropertyChanged(nameof(this.SelectedItem));
					this.RaisePropertyChanged(nameof(this.IsEmpty));
					this.RaisePropertyChanged(nameof(this.IsUntaken));
				}
			}
		}


		private TrackManager questTracker { get; set; }

		public FleetsViewModel Fleets { get; }

		public QuestsViewModel(FleetsViewModel fleets)
		{
			this.TabItems = new List<KeyNameTabItemViewModel>
				{
					new KeyNameTabItemViewModel("All", "전체"),
					new KeyNameTabItemViewModel("Current", "진행중"),
					new KeyNameTabItemViewModel("Daily", "일일"),
					new KeyNameTabItemViewModel("Weekly", "주간"),
					new KeyNameTabItemViewModel("Monthly", "월간"),
					new KeyNameTabItemViewModel("Once", "일회성"),
					new KeyNameTabItemViewModel("Others", "그 외")
				};
			this.SelectedItem = this.TabItems.FirstOrDefault(x => x.Key == "Current");


			var categoryTable = new Dictionary<string, string>
			{
				{ "Composition", "편성" },
				{ "Sortie", "출격" },
				{ "Sortie2", "출격" },
				{ "Practice", "연습" },
				{ "Expeditions", "원정" },
				{ "Supply", "보급" },
				{ "Building", "공창" },
				{ "Remodelling", "근개수" },
			};

			var list = new List<QuestCategoryViewModel>();
			var categories = Enum.GetNames(typeof(QuestCategory));
			list.Add(new QuestCategoryViewModel("All", CategoryToColor(QuestCategory.Other), "전체"));

			foreach (var item in categories)
			{
				var t = (QuestCategory)Enum.Parse(typeof(QuestCategory), item);
				list.Add(new QuestCategoryViewModel(item, CategoryToColor(t), categoryTable.ContainsKey(item) ? categoryTable[item] : ""));
			}
			list = list.Distinct(x => x.Display).ToList();

			this.QuestCategories = list;
			this.QuestCategorySelected = this.QuestCategories.FirstOrDefault();


			var quests = KanColleClient.Current.Homeport.Quests;

			this.CompositeDisposable.Add(new PropertyChangedEventListener(quests)
			{
				{ nameof(quests.All), (sender, args) => this.UpdateQuest(quests) },
			});

			KanColleSettings.QuestOnAnyTabs.Subscribe(x =>
			{
				this.RaisePropertyChanged(nameof(this.Quests));
				this.RaisePropertyChanged(nameof(this.Current));
				this.RaisePropertyChanged(nameof(this.IsEmpty));
				this.RaisePropertyChanged(nameof(this.IsUntaken));
			});
			KanColleSettings.QuestNoTakeOnTab.Subscribe(x =>
			{
				this.RaisePropertyChanged(nameof(this.Quests));
				this.RaisePropertyChanged(nameof(this.Current));
				this.RaisePropertyChanged(nameof(this.IsEmpty));
				this.RaisePropertyChanged(nameof(this.IsUntaken));
			});

			// set Quest Tarcker
			questTracker = new TrackManager();
			questTracker.QuestsEventChanged += (s, e) => this.UpdateQuest(quests);

			KanColleSettings.ShowQuestBadge.ValueChanged += (s, e) => this.UpdateBadge();
			this.UpdateQuest(quests);

			this.Fleets = fleets;
		}

		private int GetTabIdByKey(string Key)
		{
			switch (Key)
			{
				case "Current":
					return 9;
				case "Daily":
					return 1;
				case "Weekly":
					return 2;
				case "Monthly":
					return 3;
				case "Once":
					return 4;
				case "Others":
					return 5;
				case "All":
				default:
					return 0;
			}
		}

		private void UpdateBadge()
		{
			if (KanColleSettings.ShowQuestBadge)
			{
				this.Badge = _badge == 0 ? null : (int?)_badge;
			}
			else this.Badge = null;
		}

		private void UpdateQuest(Quests quests)
		{
			var viewList = quests.All
				.Select(x => new QuestViewModel(x))
				.Distinct(x => x.TabId)
				.ToList();

			try
			{ // QuestTracker 어디서 문제가 생길지 모름
				questTracker.AllQuests
					.ForEach(x =>
					{
						var y = viewList.Where(z => z.Id == x.Id);
						foreach (var z in y)
						{
							z.QuestProgressValue = x.GetProgress();
							z.QuestProgressText = x.ProgressText;
						}
					});
			}
			catch { }

			this.OriginalQuests = viewList.ToArray();

			var tab = CurrentTab;
			if (IsOnAnyTab) tab = 0;
			this.IsEmptyTable = quests.IsEmpty;
			this.IsUntakenTable = quests.IsUntaken;

			// Quests 멤버는 필터 적용된걸 get으로 반환해서 문제가 있음
			_badge = OriginalQuests
				.Where(x => x.QuestProgressValue == 100)
				.Distinct(x => x.Id)
				.Count();

			this.UpdateBadge();
			this.RaisePropertyChanged(nameof(this.Quests));
		}
		private QuestViewModel[] ComputeQuestPage(QuestViewModel[] inp)
		{
			if (inp.Length == 0) return inp;

			inp = inp
				.Select(x => { x.LastOnPage = false; return x; })
				.OrderBy(x => x.Page)
				.ThenBy(x => x.Id)
				.ToArray();

			int[] pages = inp.Select(x => x.Page)
				.Distinct()
				.ToArray();

			if (!(CurrentTab == 9 && IsOnAnyTab && IsNoTakeOnTab))
			{
				foreach (var page in pages)
					inp.Where(x => x.Page == page)
						.Last().LastOnPage = true;
			}

			return inp.ToArray();
		}

		private string CategoryToColor(QuestCategory category)
		{
			switch (category)
			{
				case QuestCategory.Composition:
					return "#FF2A7D46";
				case QuestCategory.Sortie:
				case QuestCategory.Sortie2:
					return "#FFB53B36";
				case QuestCategory.Practice:
					return "#FF8DC660";
				case QuestCategory.Expeditions:
					return "#FF3BA09D";
				case QuestCategory.Supply:
					return "#FFB2932E";
				case QuestCategory.Building:
					return "#FF64443B";
				case QuestCategory.Remodelling:
					return "#FFA987BA";
			}
			return "#FF808080";
		}
	}

	public class QuestCategoryViewModel : Livet.ViewModel
	{
		public string Key { get; }
		public string Display { get; }
		public string DisplayText { get; }

		public QuestCategoryViewModel(string Key, string Display, string DisplayText = "")
		{
			this.Key = Key;
			this.Display = Display;
			this.DisplayText = DisplayText;
		}
	}
}
