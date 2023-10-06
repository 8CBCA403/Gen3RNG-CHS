using PokemonPRNG.LCG32;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch;

internal class HiddenPowerPowerCriteria : ICriteria<Pokemon.Individual>
{
	private readonly uint minPower;

	public bool CheckConditions(Pokemon.Individual item)
	{
		return minPower <= item.HiddenPower;
	}

	public HiddenPowerPowerCriteria(uint min)
	{
		minPower = min;
	}
}
