using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models.Raw;

namespace Grabacr07.KanColleWrapper.Models
{
	public class Quest : RawDataWrapper<kcsapi_quest>, IIdentifiable
	{
		public int Id => this.RawData.api_no;

		/// <summary>
		/// 任務のカテゴリ (編成、出撃、演習 など) を取得します。
		/// </summary>
		public QuestCategory Category => (QuestCategory)this.RawData.api_category;

		/// <summary>
		/// 任務の種類 (1 回のみ、デイリー、ウィークリー) を取得します。5 is daily. I don't know why
		/// </summary>
		public QuestType Type => (QuestType)this.RawData.api_type;

		/// <summary>
		/// 任務の状態を取得します。
		/// </summary>
		public QuestState State => (QuestState)this.RawData.api_state;

		/// <summary>
		/// 任務の進捗状況を取得します。
		/// </summary>
		public QuestProgress Progress => (QuestProgress)this.RawData.api_progress_flag;

		/// <summary>
		/// 任務名を取得します。
		/// </summary>
		public string Title => KanColleClient.Current.Translations.GetTranslation(RawData.api_title, TranslationType.QuestTitle, false, this.RawData, RawData.api_no);

		/// <summary>
		/// 任務の詳細を取得します。
		/// </summary>
		public string Detail => KanColleClient.Current.Translations.GetTranslation(RawData.api_detail, TranslationType.QuestDetail, false, this.RawData, RawData.api_no)
									.Replace("<br>", Environment.NewLine);

		/// <summary>
		/// 任務名を取得します。
		/// </summary>
		public string TitleJP => RawData.api_title;

		/// <summary>
		/// 任務の詳細を取得します。
		/// </summary>
		public string DetailJP => RawData.api_detail;

		/// <summary>
		/// 임무 화면에서의 탭 번호
		/// </summary>
		/// 0 - 전체
		/// 9 - 진행중
		/// 1 - 일간
		/// 2 - 주간
		/// 3 - 월간
		/// 4 - 일회성
		/// 5 - 그 외
		public int Tab { get; set; }

		/// <summary>
		/// 임무 화면에서의 페이지 번호
		/// </summary>
		public int Page { get; set; }

		public Quest(kcsapi_quest rawData) : base(rawData) { }

		public void Stop()
		{
			this.RawData.api_state = 1;
			this.RaisePropertyChanged(nameof(this.State));
		}

		public override string ToString()
		{
			return $"ID = {this.Id}, Category = {this.Category}, Title = \"{this.Title}\", Type = {this.Type}, State = {this.State}";
		}
	}
}
