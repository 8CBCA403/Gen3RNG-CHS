using System.Linq;
using PokemonPRNG.LCG32;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch;

internal class StatsCriteria : ICriteria<Pokemon.Individual>
{
	private readonly int[] indexes;

	private readonly uint[] targetStats;

	public bool CheckConditions(Pokemon.Individual item)
	{
		return indexes.All((int _) => targetStats[_] == item.Stats[_]);
	}

	public StatsCriteria(uint[] targetStats)
	{
		indexes = (from _ in Enumerable.Range(0, 6)
			where targetStats[_] != 0
			select _).ToArray();
		this.targetStats = targetStats.ToArray();
	}
}
