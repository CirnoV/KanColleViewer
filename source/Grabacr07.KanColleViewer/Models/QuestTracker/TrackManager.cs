using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Linq;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Livet;

using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;

using Grabacr07.KanColleViewer.Models.Settings;
using Grabacr07.KanColleViewer.QuestTracker.Models;
using Grabacr07.KanColleViewer.QuestTracker.Models.Tracker;
using Grabacr07.KanColleViewer.QuestTracker.Models.EventArgs;

namespace Grabacr07.KanColleViewer.Models.QuestTracker
{
	internal class TrackManager
	{
		private string TrackerNamespace => "Grabacr07.KanColleViewer.QuestTracker.Models.Tracker";
		private static DateTime TokyoDateTime => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Tokyo Standard Time");

		private Grabacr07.KanColleViewer.QuestTracker.Models.TrackManager trackManager;
		public event EventHandler QuestsEventChanged
		{
			add { trackManager.QuestsEventChanged += value; }
			remove { trackManager.QuestsEventChanged -= value; }
		}

		public List<TrackerBase> TrackingQuests => trackManager?.TrackingQuests;
		public List<TrackerBase> AllQuests => trackManager?.AllQuests;

		public TrackManager()
		{
			trackManager = new Grabacr07.KanColleViewer.QuestTracker.Models.TrackManager(
				() => KanColleSettings.UseQuestTracker,
				() => GeneralSettings.KcaQSync_Password
			);

			var trackers = trackManager.Assembly.GetTypes()
					.Where(x => (x.Namespace?.StartsWith(TrackerNamespace) ?? false) && typeof(TrackerBase).IsAssignableFrom(x));

			foreach (var tracker in trackers)
			{
				try { trackManager?.trackingAvailable.Add((TrackerBase)Activator.CreateInstance(tracker)); }
				catch { }
			}

			var proxy = KanColleClient.Current.Proxy;
			proxy.ApiSessionSource
				.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_quest/clearitemget")
				.TryParse()
				.Where(x => x.IsSuccess)
				.Subscribe(x =>
				{
					int q_id;
					if (!int.TryParse(x.Request["api_quest_id"], out q_id)) return;
					StopQuest(q_id, true);
				});

			proxy.ApiSessionSource
				.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_quest/stop")
				.TryParse()
				.Where(x => x.IsSuccess)
				.Subscribe(x =>
				{
					int q_id;
					if (!int.TryParse(x.Request["api_quest_id"], out q_id)) return;
					StopQuest(q_id);
				});

			this.QuestsEventChanged += (s, e) => WriteToStorage();
			ReadFromStorage();
			WriteToStorage();

			var quests = KanColleClient.Current.Homeport.Quests;
			quests.PropertyChanged += async (s, e) => {
				if (e.PropertyName == nameof(quests.All))
					await Task.Run(() => ProcessQuests());
			};
		}

		private void StopQuest(int quest, bool Cleared = false)
		{
			var tracker = trackManager?.trackingAvailable.FirstOrDefault(t => t.Id == quest);
			if (tracker == default(TrackerBase)) return; // 추적할 수 없는 임무

			tracker.IsTracking = false;
			if (Cleared) tracker.ResetQuest();

			trackManager?.RefreshTrackers();
			WriteToStorage();
		}

		private void ProcessQuests()
		{
			var quests = KanColleClient.Current.Homeport.Quests;
			if (quests.All == null || quests.All.Count == 0) return;

			foreach (var quest in quests.All)
			{
				var tracker = trackManager?.trackingAvailable.FirstOrDefault(t => t.Id == quest.Id);
				if (tracker == null) continue; // 추적할 수 없는 임무

				try
				{
					// 만료된 경우 (임무가 갱신되었다던가)
					if (!IsTrackingAvailable(tracker.Type, tracker.LastUpdated))
						tracker.ResetQuest();

					switch (quest.State)
					{
						case QuestState.None:
							tracker.IsTracking = false;
							break;

						case QuestState.TakeOn:
							tracker.IsTracking = true;
							break;

						case QuestState.Accomplished:
							tracker.IsTracking = true;
							break;
					}

					tracker.LastUpdated = TokyoDateTime;
				}
				catch { }
			}

			trackManager?.RefreshTrackers();
			CallCheckOverUnder(
				quests.All.Select(x => new IdProgressPair
				{
					Id = x.Id,
					Progress = x.Progress,
					State = x.State
				}).ToArray()
			);
			WriteToStorage();
		}
		private bool IsTrackingAvailable(QuestType type, DateTime time)
		{
			// 임무는 오전 5시, UTC+4 기준 0시에 갱신됨
			// 일광절약 없는 아랍 에미레이트 연합 표준시 (ar-AE) => UTC+4

			if (time == DateTime.MinValue)
				return false;

			var no = TrackManager.TokyoDateTime.AddHours(-5);
			time = time.AddHours(-5);

			switch (type)
			{
				case QuestType.OneTime:
					return true;

				case QuestType.Daily:
					return (time.Date == no.Date);

				case QuestType.Weekly:
					var cal = CultureInfo.CreateSpecificCulture("ar-AE").Calendar;
					var w_time = cal.GetWeekOfYear(time, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
					var w_now = cal.GetWeekOfYear(no, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

					return (w_time == w_now) && (time.Year == no.Year);

				case QuestType.Monthly:
					return (time.Month == no.Month) && (time.Year == no.Year);

				case QuestType.Quarterly:
					return ((time.Month / 4) == (no.Month / 4)) && (time.Year == no.Year);

				default:
					return false;
			}
		}

		private void WriteToStorage()
		{
			var baseDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
			var list = new List<StorageData>();

			foreach (var tracker in trackManager?.trackingAvailable)
			{
				var item = new StorageData();
				var dateTime = tracker.LastUpdated;

				try
				{
					if (tracker.GetProgress() == 0 || dateTime == DateTime.MinValue) continue;

					item.Id = tracker.Id;
					item.TrackTime = dateTime;
					item.Type = tracker.Type;
					item.Serialized = tracker.SerializeData();
					list.Add(item);
				}
				catch { }
			}

			string path = Path.Combine(baseDir, "TrackingQuest.csv");
			try
			{
				using (FileStream fs = new FileStream(path, FileMode.Create))
				{
					foreach (var item in list)
					{
						try {
							CSV.Write(
								fs,
								item.Id, item.TrackTime, item.Type,
								item.Serialized,
								KanColleClient.Current.Homeport.Quests.All
									.FirstOrDefault(x => x.Id == item.Id)
									.Title
							);
						}
						catch { }
					}

					fs.Flush();
				}
			}
			catch { }
		}
		private void ReadFromStorage()
		{
			var baseDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			string path = Path.Combine(baseDir, "TrackingQuest.csv");
			if (!File.Exists(path)) return;

			try
			{
				using (FileStream fs = new FileStream(path, FileMode.Open))
				{
					while (fs.Position < fs.Length)
					{
						string[] data = CSV.Read(fs);

						try
						{
							int Id;
							DateTime trackTime;
							string QuestTypeText;
							QuestType QuestType;
							string Serialized;

							if (!int.TryParse(data[0], out Id)) continue;
							DateTime.TryParse(data[1], out trackTime);
							QuestTypeText = data[2];
							Enum.TryParse<QuestType>(QuestTypeText, out QuestType);
							Serialized = data[3];

							if (!(trackManager?.trackingAvailable.Any(x => x.Id == Id) ?? false)) continue;
							if (IsTrackingAvailable(QuestType, trackTime))
							{
								var tracker = trackManager?.trackingAvailable.Where(x => x.Id == Id).First();

								// tracker.IsTracking = true;
								tracker.LastUpdated = trackTime;
								tracker.DeserializeData(Serialized);
							}
						}
						catch { }
					}
				}
			}
			catch { }
		}

		public void CallCheckOverUnder(IdProgressPair[] questList)
		{
			foreach (var x in questList)
			{
				var z = this.TrackingQuests.FirstOrDefault(y => y.Id == x.Id);
				if (z == null) continue;

				z.UpdateState(x.State == QuestState.Accomplished ? QuestProgressType.Complete : (QuestProgressType)x.Progress);
			}
		}
	}
}
