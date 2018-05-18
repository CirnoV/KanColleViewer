using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grabacr07.KanColleWrapper.Models
{
	/// <summary>
	/// 上限のある値を表します。
	/// </summary>
	public struct LimitedValue
	{
		/// <summary>
		/// 現在値を取得します。
		/// </summary>
		public int Current { get; }

		/// <summary>
		/// 最大値を取得します。
		/// </summary>
		public int Maximum { get; }

		/// <summary>
		/// 最小値を取得します。
		/// </summary>
		public int Minimum { get; }

		/// <summary>
		/// Previous <see cref="Current"/> value.
		/// </summary>
		public int Previous { get; }

		public LimitedValue(int current, int maximum, int minimum, int? previous = null)
			: this()
		{
			this.Current = current;
			this.Maximum = maximum;
			this.Minimum = minimum;
			this.Previous = previous.HasValue ? previous.Value : current;
		}

		public LimitedValue Update(int current)
		{
			return new LimitedValue(current, this.Maximum, this.Minimum, this.Current);
		}
	}

	/// <summary>
	/// 上限のある値を表します。
	/// </summary>
	public struct LimitedValue<T> where T : struct
	{
		/// <summary>
		/// 現在値を取得します。
		/// </summary>
		public T Current { get; private set; }

		/// <summary>
		/// 最大値を取得します。
		/// </summary>
		public T Maximum { get; private set; }

		/// <summary>
		/// 最小値を取得します。
		/// </summary>
		public T Minimum { get; private set; }

		/// <summary>
		/// Previous <see cref="Current"/> value.
		/// </summary>
		public T Previous { get; private set; }

		public LimitedValue(T current, T maximum, T minimum, T? previous)
			: this()
		{
			this.Current = current;
			this.Maximum = maximum;
			this.Minimum = minimum;
			this.Previous = previous.HasValue ? previous.Value : current;
		}
	}
}
