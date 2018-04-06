using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grabacr07.KanColleWrapper.Models;

namespace Grabacr07.KanColleWrapper.Internal
{
	internal static class SlotItemExtensions
	{
		/// <summary>
		/// 개수에 의한 추가 수치를 계산합니다.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		internal static SlotItemStat GetImprovementBonus(this SlotItem item)
		{
			var output = new SlotItemStat();
			var id = item.Info.Id;
			var sqLevel = Math.Sqrt(item.Level);
			var level = item.Level;

			switch (item.Info.IconType)
			{
				// 전투기 계열 (대공 x0.2) - 함상전투기, 수상전투기, 국지전투기, 육군전투기
				case SlotItemIconType.Fighter:
				case SlotItemIconType.SeaplaneFighter:
				case SlotItemIconType.InterceptorFighter:
				case SlotItemIconType.LandBasedFighter:
					output.AA = level * 0.2;
					break;

				// 폭격기 (대공 x0.25) - 아직까진 개수 되는 것이 폭전밖에 없으므로 대충 작성
				case SlotItemIconType.DiveBomber:
					output.AA = level * 0.25;
					break;

				// 정찰기 (색적 √x1.2)
				case SlotItemIconType.ReconPlane:
					output.LoS = sqLevel * 1.2;
					break;

				// 소구경 주포, 중구경 주포 (화력, 명중 √x1.0)
				case SlotItemIconType.MainCanonLight:
				case SlotItemIconType.MainCanonMedium:
					output.Firepower = sqLevel * 1.0;
					output.Hit = sqLevel * 1.0;
					break;

				// 대구경 주포 (화력 √x1.5, 야전화력 √x1.0, 명중 √x1.0)
				case SlotItemIconType.MainCanonHeavy:
					output.Firepower = sqLevel * 1.5;
					output.NightFirepower = sqLevel * 1.0;
					output.Hit = sqLevel * 1.0;
					break;

				// 부포 (화력, 명중 √x1.0) - 노란색 아이콘
				case SlotItemIconType.SecondaryCanon:
					// 15.5cm 삼연장포(부포), 15.5cm 삼연장부포改
					// 화력 x0.3, 야전 화력 √x1.0
					if (new int[] { 12, 234 }.Contains(id))
					{
						output.Firepower = level * 0.3;
						output.NightFirepower = sqLevel * 1.0;
						output.Hit = sqLevel * 1.0;
					}
					// 그 외 부포
					else
					{
						// 화력, 명중 √x1.0
						output.Firepower = sqLevel * 1.0;
						output.Hit = sqLevel * 1.0;
					}
					break;

				// 고각포 (화력, 명중 √x1.0, 대공 √x0.7, 함대방공 √x3.0)
				case SlotItemIconType.HighAngleGun:
					// 12.7cm 연장고각포, 8cm 고각포, 8cm 고각포改+증설기총
					// 화력 x0.2, 야전화력 √x1.0, 대공 √x0.7, 함대방공 √x2.0
					if (new int[] { 10, 66, 220 }.Contains(id))
					{
						output.Firepower = level * 0.2;
						output.NightFirepower = sqLevel * 1.0;
						output.AA = sqLevel * 0.7;
						output.FleetAA = sqLevel * 2.0;
					}
					// 그 외 고각포
					else
					{
						output.Firepower = sqLevel * 1.0;
						output.Hit = sqLevel * 1.0;
						output.AA = sqLevel * 0.7;
						output.FleetAA = sqLevel * 3.0;
					}
					break;

				// 어뢰 (뇌장 √x1.2, 야전 화력 √x1.0, 뇌격 명중 √x2.0)
				case SlotItemIconType.Torpedo:
					output.Torpedo = sqLevel * 1.2;
					output.NightFirepower = sqLevel * 1.0;
					output.TorpedoHit = sqLevel * 2.0;
					break;

				// 전파탐신기
				// - 대공전탐:   색적 √x1.25, 명중 √x1.0, 함대방공 √x1.5
				// - 대수상전탐: 색적 √x1.0, 명중 √x2.0
				// - 잠수함전탐: ???
				case SlotItemIconType.Rader:
					// 잠수함전탐
					// ???
					if (new int[] { 210, 211 }.Contains(id))
					{
					}
					// 대공전탐
					// 색적 √x1.25, 명중 √x1.0, 함대방공 √x1.5
					else if (new int[] { 27, 30, 32, 89, 106, 124, 142, 278, 279 }.Contains(id))
					{
						output.LoS = sqLevel * 1.25;
						output.Hit = sqLevel * 1.0;
						output.FleetAA = sqLevel * 1.5;
					}
					// 대수상전탐
					// 색적 √x1.0, 명중 √x2.0
					else
					{
						output.LoS = sqLevel * 1.0;
						output.Hit = sqLevel * 2.0;
					}
					break;

				// 소나 (화력 √x0.75, 대잠 √x6÷9, 대잠 명중 √x1.3, 뇌격 명중 √x1.5)
				case SlotItemIconType.Soner:
					output.Firepower = sqLevel * 0.75;
					output.ASW = sqLevel * 6.0 / 9.0;
					output.ASWHit = sqLevel * 1.3;
					output.TorpedoHit = sqLevel * 1.5;
					break;

				// 폭뢰/폭뢰투사기
				// - 폭뢰:       대잠 √x6÷9
				// - 폭뢰투사기: 대잠 √x6÷9, 화력 √x0.75
				case SlotItemIconType.ASW:
					// 폭뢰
					// 대잠 √x6÷9
					if (new int[] { 226, 227 }.Contains(id))
					{
						output.ASW = sqLevel * 6.0 / 9.0;
					}
					// 폭뢰투사기
					// 대잠 √x6÷9, 화력 √x0.75
					else
					{
						output.Firepower = sqLevel * 0.75;
						output.ASW = sqLevel * 6.0 / 9.0;
					}
					break;

				// 철갑탄 (화력, 명중 √x1.0)
				case SlotItemIconType.APShell:
					output.Firepower = sqLevel * 1.0;
					output.Hit = sqLevel * 1.0;
					break;

				// 대공기총 (대공 √x0.7, 화력 √x1.0, 뇌장 √x1.2, 뇌장 명중 ???)
				case SlotItemIconType.AAGun:
					output.Firepower = sqLevel * 1.0;
					output.AA = sqLevel * 0.7;
					output.Torpedo = sqLevel * 1.2;
					break;

				// 고사장치 (화력 √x1.0, 명중 √x1.0, 대공 √x0.7, 함대방공 √x2.0)
				case SlotItemIconType.AntiAircraftFireDirector:
					output.Firepower = sqLevel * 1.0;
					output.Hit = sqLevel * 1.0;
					output.AA = sqLevel * 0.7;
					output.FleetAA = sqLevel * 2.0;
					break;

				// 기관부강화 (회피 √x1.5)
				case SlotItemIconType.EngineImprovement:
					output.Evade = sqLevel * 1.5;
					break;

				// 증설벌지
				// - 중형벌지 : 장갑 x0.2
				// - 대형벌지 : 장갑 x0.3
				case SlotItemIconType.AntiTorpedoBulge:
					// 중형벌지
					// 장갑 x0.2
					if (new int[] { 72, 203 }.Contains(id))
					{
						output.Armor = level * 0.2;
					}
					// 대형벌지
					// 장갑 x0.3
					else
					{
						output.Armor = level * 0.3;
					}
					break;

				// 탐조등 (화력 √x1.0, 피탄확률 ???, 적 컷인 확률 ???)
				case SlotItemIconType.Searchlight:
					output.Firepower = sqLevel * 1.0;
					break;

				// 수상폭격기 (폭장 x0.2, 색적 √x1.15)
				// 수상정찰기 (색적 √x1.2)
				case SlotItemIconType.ReconSeaplane:
					// 수상폭격기
					// 폭장 x0.2, 색적 √x1.15
					if (new int[] { 26, 79, 237 }.Contains(id))
					{
						output.Bomb = level * 0.2;
						output.LoS = sqLevel * 1.15;
					}
					// 수상정찰기
					// 색적 √x1.2
					else
					{
						output.LoS = sqLevel * 1.2;
					}
					break;

				// 상륙정, 내화정 (화력 √x1.0)
				// - 대발동정:         원정 보수 x0.05, 포대 특효 x0.04
				// - 대발동정(육전대): 원정 보수 x0.02, 포대 특효 x0.044
				// - 특대발동정:       원정 보수 x0.05
				// - 2식 내화정:       원정 보수 x0.01, 포대 특효 x0.08
				case SlotItemIconType.LandingCraft:
				case SlotItemIconType.AmphibiousLandingCraft:
					output.Firepower = sqLevel * 1.0;

					// 대발동정
					// 원정 보수 x0.05, 포대 특효 x0.04
					if (id == 68)
					{
						output.ExpeditionBonus = level * 0.05;
						output.TurrentEfficacy = level * 0.04;
					}
					// 대발동정(육전대)
					// 원정 보수 x0.02, 포대 특효 x0.044
					else if (id == 166)
					{
						output.ExpeditionBonus = level * 0.02;
						output.TurrentEfficacy = level * 0.044;
					}
					// 특대발동정
					// 원정 보수 x0.05
					else if (id == 193)
					{
						output.ExpeditionBonus = level * 0.05;
					}
					// 2식 내화정
					// 원정 보수 x0.01, 포대 특효 x0.08
					else if (id == 167)
					{
						output.ExpeditionBonus = level * 0.01;
						output.TurrentEfficacy = level * 0.08;
					}
					break;
			}
			return output;
		}
	}
}
