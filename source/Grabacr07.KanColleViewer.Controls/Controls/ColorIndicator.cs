using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Grabacr07.KanColleWrapper.Models;

namespace Grabacr07.KanColleViewer.Controls
{
	/// <summary>
	/// 
	/// </summary>
	[TemplatePart(Name = "PART_PreviousIndicator", Type = typeof(FrameworkElement))]
	public class ColorIndicator : ProgressBar
	{
		static ColorIndicator()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(ColorIndicator),
				new FrameworkPropertyMetadata(typeof(ColorIndicator)));
		}

		#region LimitedValue 依存関係プロパティ
		public LimitedValue LimitedValue
		{
			get { return (LimitedValue)this.GetValue(LimitedValueProperty); }
			set { this.SetValue(LimitedValueProperty, value); }
		}
		public static readonly DependencyProperty LimitedValueProperty =
			DependencyProperty.Register(nameof(LimitedValue), typeof(LimitedValue), typeof(ColorIndicator), new UIPropertyMetadata(new LimitedValue(), LimitedValuePropertyChangedCallback));

		private static void LimitedValuePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var source = (ColorIndicator)d;
			var value = (LimitedValue)e.NewValue;

			source.ChangeColor(value);
		}
		#endregion

		#region Previous 종속성 프로퍼티
		public double Previous
		{
			get { return (double)this.GetValue(PreviousProperty); }
			set { this.SetValue(PreviousProperty, value); }
		}
		public static readonly DependencyProperty PreviousProperty =
			DependencyProperty.Register(nameof(Previous), typeof(double), typeof(ColorIndicator), new UIPropertyMetadata(0.0));
		#endregion

		#region Columns 종속성 프로퍼티
		public int Columns
		{
			get { return (int)this.GetValue(ColumnsProperty); }
			set { this.SetValue(ColumnsProperty, value); }
		}
		public static readonly DependencyProperty ColumnsProperty =
			DependencyProperty.Register(nameof(Columns), typeof(int), typeof(ColorIndicator), new UIPropertyMetadata(4));
		#endregion

		#region FullColor 종속성 프로퍼티
		public Color FullColor
		{
			get { return (Color)this.GetValue(FullColorProperty); }
			set { this.SetValue(FullColorProperty, value); }
		}
		public static readonly DependencyProperty FullColorProperty =
			DependencyProperty.Register(nameof(FullColor), typeof(Color), typeof(ColorIndicator), new UIPropertyMetadata(Color.FromRgb(40, 144, 16)));
		#endregion

		#region PreviousIndicator process
		private FrameworkElement _track, _previousindicator;
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (_track != null)
				_track.SizeChanged -= OnTrackSizeChanged;

			_track = GetTemplateChild("PART_Track") as FrameworkElement;
			_previousindicator = GetTemplateChild("PART_PreviousIndicator") as FrameworkElement;

			if (_track != null)
				_track.SizeChanged += OnTrackSizeChanged;
		}
		private void OnTrackSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (_track != null && _previousindicator != null)
			{
				double min = Minimum;
				double max = Maximum;
				double val = Previous;

				// When indeterminate or maximum == minimum, have the indicator stretch the 
				// whole length of track
				double percent = IsIndeterminate || max <= min ? 1.0 : (val - min) / (max - min);
				_previousindicator.Width = percent * _track.ActualWidth;
			}
		}
		#endregion

		/// <summary>
		/// Calls after new <see cref="LimitedValue"/> given.
		/// </summary>
		/// <param name="value"></param>
		private void ChangeColor(LimitedValue value)
		{
			this.Maximum = value.Maximum;
			this.Minimum = value.Minimum;
			this.Value = value.Current;
			this.Previous = value.Previous < this.Minimum ? this.Minimum : (value.Previous > this.Maximum ? this.Maximum : value.Previous);

			Color color;
			var percentage = value.Maximum == 0 ? 0.0 : value.Current / (double)value.Maximum;

			// 0.25 以下のとき、「大破」
			if (percentage <= 0.25) color = Color.FromRgb(255, 32, 32);

			// 0.5 以下のとき、「中破」
			else if (percentage <= 0.5) color = Color.FromRgb(240, 128, 32);

			// 0.75 以下のとき、「小破」
			else if (percentage <= 0.75) color = Color.FromRgb(240, 240, 0);

			// 0.75 より大きいとき、「小破未満」
			else if (percentage < 1.0) color = Color.FromRgb(64, 200, 32);

			// 1 (100%)
			// else color = Color.FromRgb(40, 160, 240); // Blue one
			// else color = Color.FromRgb(40, 144, 16); // Deep green one
			else color = FullColor;

			this.Foreground = new SolidColorBrush(color);
		}
	}
}
