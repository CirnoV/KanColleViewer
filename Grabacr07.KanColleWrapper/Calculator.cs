﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Internal;
using Grabacr07.KanColleWrapper.Models;
using System.Diagnostics;

namespace Grabacr07.KanColleWrapper
{
	internal static class Calculator
	{
		/// <summary>
		/// 装備と搭載数を指定して、スロット単位の制空能力を計算します。
		/// </summary>
		/// <param name="slotItem">対空能力を持つ装備。</param>
		/// <param name="onslot">搭載数。</param>
		/// <returns></returns>
		public static int CalcAirSuperiorityPotential(this SlotItem slotItem, int onslot)
		{
			if (slotItem != null && slotItem.Info.IsAirSuperiorityFighter)
			{
				return (int)(slotItem.Info.AA * Math.Sqrt(onslot));
			}

			return 0;
		}

		/// <summary>
		/// 指定した艦の制空能力を計算します。
		/// </summary>
		public static int CalcAirSuperiorityPotential(this Ship ship)
		{
			return ship.Slots.Select(x => x.Item.CalcAirSuperiorityPotential(x.Current)).Sum();
		}


		/// <summary>
		/// 艦隊の索敵値を計算します。
		/// </summary>
		/// <returns></returns>
		public static int CalcFleetViewRange(this Fleet fleet, ViewRangeCalcLogic logic)
		{
			if (fleet == null || fleet.Ships.Length == 0) return 0;

			if (logic == ViewRangeCalcLogic.Type1)
			{
				return fleet.Ships.Sum(x => x.ViewRange);
			}
			if (logic == ViewRangeCalcLogic.Type2)
			{
				// http://wikiwiki.jp/kancolle/?%C6%EE%C0%BE%BD%F4%C5%E7%B3%A4%B0%E8#area5
				// [索敵装備と装備例] によって示されている計算式
				// stype=7 が偵察機 (2 倍する索敵値)、stype=8 が電探
				int spotter = 0;
				try
				{
					spotter = fleet.Ships.SelectMany(
					x => x.Slots
						.Where(s => s.Equipped)
						.Where(s => s.Item.Info.RawData.api_type.Get(1) == 7)
						.Select(s => s.Item.Info.RawData.api_saku)
					).Sum();
				}
				catch (Exception e)
				{
					Debug.WriteLine(e);
				}
				int radar = 0;
				try
				{
					radar = fleet.Ships.SelectMany(
					x => x.Slots
						.Where(s => s.Equipped)
						.Where(s => s.Item.Info.RawData.api_type.Get(1) == 8)
						.Select(s => s.Item.Info.RawData.api_saku)
					).Sum();
				}
				catch (Exception e)
				{
					Debug.WriteLine(e);
				}


				return (spotter * 2) + radar + (int)Math.Sqrt(fleet.Ships.Sum(x => x.ViewRange) - spotter - radar);
			}

			return 0;
		}
	}


	/// <summary>
	/// 索敵値の計算に使用するロジックの種類を示す識別します。
	/// </summary>
	public enum ViewRangeCalcLogic
	{
		/// <summary>
		/// 単純な索敵合計値。
		/// </summary>
		Type1,

		/// <summary>
		/// (偵察機 × 2) + (電探) + √(装備込みの艦隊索敵値合計 - 偵察機 - 電探)。
		/// </summary>
		Type2,
	}
}