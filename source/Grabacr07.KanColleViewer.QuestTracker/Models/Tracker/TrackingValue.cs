using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grabacr07.KanColleViewer.QuestTracker.Models.Tracker
{
	public class TrackingValue
	{
		/// <summary>
		/// Quest part's maximum count
		/// </summary>
		public int Maximum { get; set; }

		/// <summary>
		/// Quest part's current value
		/// </summary>
		public int Current { get; set; }

		/// <summary>
		/// Reset part progress (Set <see cref="Current"/> to 0)
		/// </summary>
		public void Clear()
		{
			this.Current = 0;
		}

		/// <summary>
		/// Serialize <see cref="Current"/> and <see cref="Maximum"/>
		/// </summary>
		/// <returns></returns>
		public string Serialize()
			=> string.Format("{0}:{1}", this.Maximum, this.Current);

		/// <summary>
		/// Load from serialized with <see cref="Serialize"/>
		/// </summary>
		/// <param name="Serialized"></param>
		public void Deserialize(string Serialized)
		{
			if (Serialized.Count(x => x == ':') != 1)
				throw new ArgumentException("Invalid argument", nameof(Serialized));

			int _Maximum, _Value;
			var parts = Serialized.Split(new char[] { ':' });
			if (!int.TryParse(parts[0], out _Maximum))
				throw new ArgumentException("Cannot convert to int", nameof(Serialized));

			if (!int.TryParse(parts[0], out _Value))
				throw new ArgumentException("Cannot convert to int", nameof(Serialized));

			this.Maximum = _Maximum;
			this.Current = _Value;
		}
	}
}
