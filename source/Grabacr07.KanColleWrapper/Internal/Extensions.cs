using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nekoxy;

namespace Grabacr07.KanColleWrapper.Internal
{
	internal static class Extensions
	{
		public static string GetResponseAsJson(this Session session)
		{
			// return session.Response.BodyAsString.Replace("svdata=", "");
			var body = session.Response.BodyAsString;
			return body.StartsWith("svdata=")
				? body.Substring(7)
				: body;
		}

		/// <summary>
		/// <see cref="Int32" /> 型の配列に安全にアクセスします。
		/// </summary>
		public static int? Get(this int[] array, int index)
		{
			return array?.Length > index ? (int?)array[index] : null;
		}
	}
}
