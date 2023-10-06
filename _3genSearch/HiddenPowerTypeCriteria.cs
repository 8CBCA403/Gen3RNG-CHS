using PokemonPRNG.LCG32;
using PokemonStandardLibrary;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch;

internal class HiddenPowerTypeCriteria : ICriteria<Pokemon.Individual>
{
	private readonly PokeType targetType;

	public bool CheckConditions(Pokemon.Individual item)
	{
		return (item.HiddenPowerType & targetType) != 0;
	}

	public HiddenPowerTypeCriteria(params PokeType[] pokeTypes)
	{
		PokeType pokeType = PokeType.None;
		foreach (PokeType pokeType2 in pokeTypes)
		{
			pokeType |= pokeType2;
		}
		targetType = pokeType;
	}
}
