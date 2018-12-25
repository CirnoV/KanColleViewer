using Grabacr07.KanColleWrapper.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Grabacr07.KanColleWrapper
{
	public partial class Translations
	{
		private const string RequestTranslationURL = "http://swaytwig.com/kcvkr/request.php";

		private void RequestTranslation(TranslationType Type, string Source)
		{
			HttpWebRequest request = HttpWebRequest.Create(RequestTranslationURL) as HttpWebRequest;
			WebResponse response;
			Stream writer;

			try
			{
				var postBytes = Encoding.UTF8.GetBytes($"type={(int)Type}&source={WebUtility.UrlEncode(Source)}");
				request.Method = "POST";
				request.ServicePoint.Expect100Continue = false;
				request.ContentType = "application/x-www-form-urlencoded";

				writer = request.GetRequestStream();
				writer.Write(postBytes, 0, postBytes.Length);
				writer.Flush();
				writer.Close();

				request.BeginGetResponse(new AsyncCallback((x) =>
				{
					response = request.EndGetResponse(x);

#if DEBUG
					try {
						var resp = response.GetResponseStream();
						Debug.WriteLine($"Translate requested: {Source}");
						Debug.WriteLine("Translate request result: " + new StreamReader(resp).ReadToEnd());
					}
					catch  { }
#endif

					response.Close();
				}), null);
			}
			catch { }
		}
	}
}
