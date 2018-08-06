using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace Grabacr07.KanColleViewer.QuestTracker.Models
{
	class HTTPRequest
	{
		public static Stream Request(string URL)
		{
			WebRequest request = HttpWebRequest.Create(URL);
			WebResponse response;

			try
			{
				response = request.GetResponse();
				return response.GetResponseStream();
			}
			catch
			{
				return null;
			}
		}
		public static Stream RequestAsync(string URL, Action<Stream> Callback)
		{
			WebRequest request = HttpWebRequest.Create(URL);
			WebResponse response;

			try
			{
				request.BeginGetResponse((x) => {
					response = request.EndGetResponse(x);
					Callback?.Invoke(response.GetResponseStream());

					response.Close();
				}, null);
				return null;
			}
			catch
			{
				return null;
			}
		}

		public static Stream Post(string URL, string Post)
		{
			HttpWebRequest request = HttpWebRequest.Create(URL) as HttpWebRequest;
			WebResponse response;
			Stream writer;

			try
			{
				var postBytes = Encoding.UTF8.GetBytes(Post);
				request.Method = "POST";
				request.ServicePoint.Expect100Continue = false;
				request.ContentType = "application/x-www-form-urlencoded";

				writer = request.GetRequestStream();
				writer.Write(postBytes, 0, postBytes.Length);
				writer.Flush();
				writer.Close();

				response = request.GetResponse();
				return response.GetResponseStream();
			}
			catch
			{
				return null;
			}
		}
		public static void PostAsync(string URL, string Post, Action<Stream> Callback)
		{
			HttpWebRequest request = HttpWebRequest.Create(URL) as HttpWebRequest;
			WebResponse response;
			Stream writer;

			try
			{
				var postBytes = Encoding.UTF8.GetBytes(Post);
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
					Callback?.Invoke(response.GetResponseStream());

					response.Close();
				}), null);
			}
			catch { }
		}
	}
}
