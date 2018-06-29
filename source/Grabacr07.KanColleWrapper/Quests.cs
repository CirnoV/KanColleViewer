using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Codeplex.Data;
using Nekoxy;
using Grabacr07.KanColleWrapper.Internal;
using Grabacr07.KanColleWrapper.Models;
using Grabacr07.KanColleWrapper.Models.Raw;
using System.Web;

namespace Grabacr07.KanColleWrapper
{
	public class Quests : Notifier
	{
		private struct TabPage
		{
			public int Tab { get; set; }
			public int Page { get; set; }
		}

		// questPages[tab][page] = Quest
		private readonly Dictionary<int, List<ConcurrentDictionary<int, Quest>>> questPages;
		private static int cur_tab_id = -1;
		private static int last_exec_count = 0;

		#region All 変更通知プロパティ

		private IReadOnlyCollection<Quest> _All;
		public IReadOnlyCollection<Quest> All
		{
			get { return this._All; }
			set
			{
				if (!Equals(this._All, value))
				{
					this._All = value;
					this.RaisePropertyChanged();
					this.RaisePropertyChanged(nameof(this.Current));
				}
			}
		}

		#endregion

		#region Current 変更通知プロパティ

		/// <summary>
		/// 現在遂行中の任務のリストを取得します。未取得の任務がある場合、リスト内に null が含まれることに注意してください。
		/// </summary>
		public IReadOnlyCollection<Quest> Current
		{
			get
			{
				var list = this.All
					.Where(x => x.State != QuestState.None)
					.OrderBy(x => x.Id);
				var placeholder = new Quest[Math.Max(0, list.Count() - last_exec_count)];

				return list
					.Concat(placeholder)
					.ToList();
			}
		}

		#endregion

		#region IsUntaken 変更通知プロパティ

		// 한 번도 퀘스트 화면에 진입하지 않은 경우
		private Dictionary<int, bool> _IsUntaken;
		public Dictionary<int, bool> IsUntaken
		{
			get { return this._IsUntaken; }
			set
			{
				if (this._IsUntaken != value)
				{
					this._IsUntaken = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region IsEmpty 変更通知プロパティ

		// 퀘스트가 하나도 없는 경우
		private Dictionary<int, bool> _IsEmpty;
		public Dictionary<int, bool> IsEmpty
		{
			get { return this._IsEmpty; }
			set
			{
				if (this._IsEmpty != value)
				{
					this._IsEmpty = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion


		internal Quests(KanColleProxy proxy)
		{
			this.questPages = new Dictionary<int, List<ConcurrentDictionary<int, Quest>>>();
			this.IsUntaken = new Dictionary<int, bool>();
			this.IsEmpty = new Dictionary<int, bool>();
			this.All = new List<Quest>();

			proxy.api_get_member_questlist
				// .Where(x => HttpUtility.ParseQueryString(x.Request.BodyAsString)["api_tab_id"] == "0")
				.Select(Serialize)
				.Where(x => x != null)
				.Subscribe(this.Update);

			proxy.ApiSessionSource
				.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_quest/clearitemget")
				.TryParse()
				.Where(x => x.IsSuccess)
				.Subscribe(x =>
				{
					int q_id;
					if (!int.TryParse(x.Request["api_quest_id"], out q_id)) return;

					ClearQuest(q_id);
				});
		}

		private static kcsapi_questlist Serialize(Session session)
		{
			string s_tab_id = HttpUtility.ParseQueryString(session.Request.BodyAsString)["api_tab_id"];
			int tab_id;

			if (!int.TryParse(s_tab_id, out tab_id)) return null;

			/*
			if (!KanColleClient.Current.Settings.QuestOnAnyTabs && tab_id != 0)
				return null;
			*/

			Quests.cur_tab_id = tab_id;

			try
			{
				var djson = DynamicJson.Parse(session.GetResponseAsJson());
				var questlist = new kcsapi_questlist
				{
					api_count = Convert.ToInt32(djson.api_data.api_count),
					api_disp_page = Convert.ToInt32(djson.api_data.api_disp_page),
					api_page_count = Convert.ToInt32(djson.api_data.api_page_count),
					api_exec_count = Convert.ToInt32(djson.api_data.api_exec_count),
				};

				if (djson.api_data.api_list != null)
				{
					var list = new List<kcsapi_quest>();
					var serializer = new DataContractJsonSerializer(typeof(kcsapi_quest));
					foreach (var x in (object[])djson.api_data.api_list)
					{
						try
						{
							list.Add(serializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(x.ToString()))) as kcsapi_quest);
						}
						catch (SerializationException ex)
						{
							// 最後のページで任務数が 5 に満たないとき、api_list が -1 で埋められるというクソ API のせい
							Debug.WriteLine(ex.Message);
						}
					}

					questlist.api_list = list.ToArray();
				}

				return questlist;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return null;
			}
		}

		private void ClearQuest(int q_id)
		{
			Quest temp;

			foreach(var tab in this.questPages)
			{
				foreach (var page in tab.Value) {
					foreach(var key in page.Keys)
					{
						if (page[key] == null) continue;
						if (page[key].Id == q_id)
							page.TryRemove(key, out temp);
					}
				}
			}

			this.All = this.questPages
				.SelectMany(y =>
					y.Value.Where(x => x != null)
						.SelectMany(x => x.Select(kvp => kvp.Value))
						.Distinct(x => x.Id)
						.OrderBy(x => x.Id)
				)
				.ToList();
		}
		private void Update(kcsapi_questlist questlist)
		{
			var tab = Quests.cur_tab_id;
			this.IsUntaken[tab] = false;
			Quests.last_exec_count = questlist.api_exec_count;

			/*
			if(this.last_tab_id != Quests.cur_tab_id)
				this.questPages.Clear();
			*/
			if (!this.questPages.ContainsKey(tab))
				this.questPages[tab] = new List<ConcurrentDictionary<int, Quest>>();

			// キャッシュしてるページの数が、取得したページ数 (api_page_count) より大きいとき
			// 取得したページ数と同じ数になるようにキャッシュしてるページを減らす
			while (this.questPages[tab].Count > questlist.api_page_count)
				this.questPages[tab].RemoveAt(this.questPages.Count - 1);

			// 小さいときは、キャッシュしたページ数と同じ数になるようにページを増やす
			while (this.questPages[tab].Count < questlist.api_page_count)
				this.questPages[tab].Add(null);

			if (questlist.api_list == null)
			{
				this.IsEmpty[tab] = true;
			}
			else
			{
				var page = questlist.api_disp_page - 1;
				if (page >= this.questPages[tab].Count) page = this.questPages[tab].Count - 1;

				this.questPages[tab][page] = new ConcurrentDictionary<int, Quest>();

				this.IsEmpty[tab] = false;

				foreach (var quest in questlist.api_list.Select(x => new Quest(x)))
				{
					quest.Page = page;
					quest.Tab = tab;
					this.questPages[tab][page].AddOrUpdate(quest.Id, quest, (_, __) => quest);
				}

				this.All = this.questPages
					.SelectMany(y =>
						y.Value.Where(x => x != null)
							.SelectMany(x => x.Select(kvp => kvp.Value))
							.Distinct(x => x.Id)
							.OrderBy(x => x.Id)
					)
					.ToList();
			}
		}
	}
}
