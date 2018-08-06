using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models.Raw;

namespace Grabacr07.KanColleWrapper.Models
{
	[DebuggerDisplay("[{Id}] {Title} - {Detail}")]
	public class Mission : RawDataWrapper<kcsapi_mission>, IIdentifiable
	{
		public int Id { get; }

		public string JPTitle { get; }
		public string JPDetail { get; }

		public string DisplayId
		{
			get
			{
				var DisplayID = KanColleClient.Current.Translations.GetExpeditionData("DisplayID", this.Id);
				return string.IsNullOrEmpty(DisplayID) ? this.Id.ToString() : DisplayID;
			}
		}

		public string Title => KanColleClient.Current.Translations.GetTranslation(this.JPTitle, TranslationType.ExpeditionTitle, false, this.RawData, this.Id);
		public string Detail => KanColleClient.Current.Translations.GetTranslation(this.JPDetail, TranslationType.ExpeditionDetail, false, this.RawData, this.Id);

		public Mission(kcsapi_mission mission)
			: base(mission)
		{
			this.Id = mission.api_id;

			this.JPTitle = mission.api_name;
			this.JPDetail = mission.api_details;
		}
	}
}
