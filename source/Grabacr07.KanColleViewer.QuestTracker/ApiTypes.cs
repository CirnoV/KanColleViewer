using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grabacr07.KanColleViewer.QuestTracker.Models
{
	/// <summary>
	/// 전투 API 전체 목록
	/// </summary>
	public enum ApiTypes
	{
		sortie_battle,
		practice_battle,

		sortie_battle_midnight,
		sortie_battle_midnight_sp,
		practice_midnight_battle,

		sortie_airbattle,
		sortie_ld_airbattle,

		sortie_nighttoday,

		combined_battle,
		combined_battle_water,

		combined_battle_ec,
		combined_battle_each,
		combined_battle_each_water,

		combined_airbattle,
		combined_ld_airbattle,

		combined_battle_midnight,
		combined_battle_midnight_sp,
		combined_battle_midnight_ec,

		combined_battle_ec_nighttoday
	}

	/// <summary>
	/// sortie_battle 의 동형 API
	/// </summary>
	public enum ApiTypes_Sortie
	{
		sortie_battle = ApiTypes.sortie_battle,
		practice_battle = ApiTypes.practice_battle
	}

	/// <summary>
	/// sortie_battle_midnight 의 동형 API
	/// </summary>
	public enum ApiTypes_SortieMidnight
	{
		sortie_battle_midnight = ApiTypes.sortie_battle_midnight,
		sortie_battle_midnight_sp = ApiTypes.sortie_battle_midnight_sp,
		practice_midnight_battle = ApiTypes.practice_midnight_battle
	}

	/// <summary>
	/// sortie_airbattle 의 동형 API
	/// </summary>
	public enum ApiTypes_SortieAirBattle
	{
		sortie_airbattle = ApiTypes.sortie_airbattle,
		sortie_ld_airbattle = ApiTypes.sortie_ld_airbattle
	}

	/// <summary>
	/// combined_battle 의 동형 API
	/// </summary>
	public enum ApiTypes_CombinedBattle
	{
		combined_battle = ApiTypes.combined_battle,
		combined_battle_water = ApiTypes.combined_battle_water
	}

	/// <summary>
	/// combined_battle_ec 의 동형 API
	/// </summary>
	public enum ApiTypes_CombinedBattleEC
	{
		combined_battle_ec = ApiTypes.combined_battle_ec,
		combined_battle_each = ApiTypes.combined_battle_each,
		combined_battle_each_water = ApiTypes.combined_battle_each_water
	}

	/// <summary>
	/// combined_airbattle 의 동형 API
	/// </summary>
	public enum ApiTypes_CombinedAirBattle
	{
		combined_airbattle = ApiTypes.combined_airbattle,
		combined_ld_airbattle = ApiTypes.combined_ld_airbattle
	}

	/// <summary>
	/// combined_battle_midnight 의 동형 API
	/// </summary>
	public enum ApiTypes_CombinedMidnight
	{
		combined_battle_midnight = ApiTypes.combined_battle_midnight,
		combined_battle_midnight_sp = ApiTypes.combined_battle_midnight_sp,
		combined_battle_midnight_ec = ApiTypes.combined_battle_midnight_ec
	}
}
