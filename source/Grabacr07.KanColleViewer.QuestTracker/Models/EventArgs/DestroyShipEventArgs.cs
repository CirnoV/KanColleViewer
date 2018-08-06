using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Specialized;
using Grabacr07.KanColleViewer.QuestTracker.Models;
using Grabacr07.KanColleViewer.QuestTracker.Extensions;
using Grabacr07.KanColleWrapper.Models.Raw;

namespace Grabacr07.KanColleViewer.QuestTracker.Models.EventArgs
{
	internal class DestroyShipEventArgs
	{
		public int[] shipList { get; set; }

		public DestroyShipEventArgs(NameValueCollection request, kcsapi_destroyship data)
		{
			shipList = request["api_ship_id"]
				.Split(',')
				.Select(int.Parse)
				.ToArray();
		}
	}
}
