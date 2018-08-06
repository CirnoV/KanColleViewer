using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grabacr07.KanColleViewer.QuestTracker.Models
{
	internal class SyncData
	{
		public bool Active { get; set; }
		public int Id { get; set; }
		public int[] Count { get; set; }


		/// <summary>
		/// Encoding & Decoding table
		/// </summary>
		private static readonly string EncodingTable = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ=";

		/// <summary>
		/// Encode value
		/// </summary>
		/// <param name="input">Value to encode</param>
		/// <returns></returns>
		public static string Encode(int input)
		{
			var output = "";
			while (input >= 0)
			{
				var pos = Math.Min(EncodingTable.Length - 1, input);

				output += EncodingTable[pos];
				input -= pos;
				if (input == 0 && pos != 36) break;
			}
			return output;
		}

		/// <summary>
		/// Decode single value
		/// </summary>
		/// <param name="input">Encoded data</param>
		/// <returns></returns>
		public static int Decode(string input)
		{
			var _length = input.Length;
			var _output = 0;
			for (var i = 0; i < _length; i++)
			{
				if (EncodingTable.IndexOf(input[i]) < 0) return 0;
				_output += EncodingTable.IndexOf(input[i]) * (int)Math.Pow(36, _length - i - 1);
			}
			return _output;
		}

		/// <summary>
		/// Decode counter list
		/// </summary>
		/// <param name="input">Encoded counter data</param>
		/// <param name="ExtIndexes">Equal or Over 36 index list</param>
		/// <returns></returns>
		public static int[] DecodeList(string input, int[] ExtIndexes = null)
		{
			var _length = input.Length;
			var _output = new List<int>();
			var _idx = 0;
			for (var i = 0; i < _length; i++)
			{
				if (EncodingTable.IndexOf(input[i]) < 0) return null;
				if (ExtIndexes != null && ExtIndexes.Contains(_idx++))
					_output.Add(SyncData.Decode(input.Substring(i++, 2)));
				else
					_output.Add(SyncData.Decode(input.Substring(i, 1)));
			}
			return _output.ToArray();
		}
	}
}
