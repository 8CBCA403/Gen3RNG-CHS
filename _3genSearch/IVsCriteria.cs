using PokemonPRNG.LCG32;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch;

internal class IVsCriteria : ICriteria<Pokemon.Individual>
{
	private readonly uint[] min;

	private readonly uint[] max;

	public bool CheckConditions(Pokemon.Individual individual)
	{
		for (int i = 0; i < 6; i++)
		{
			if (individual.IVs[i] < min[i] || max[i] < individual.IVs[i])
			{
				return false;
			}
		}
		return true;
	}

	public IVsCriteria(uint[] min, uint[] max)
	{
		this.min = min;
		this.max = max;
	}
}
