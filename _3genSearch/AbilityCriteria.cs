using PokemonPRNG.LCG32;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch;

internal class AbilityCriteria : ICriteria<Pokemon.Individual>
{
	private readonly string ability;

	public bool CheckConditions(Pokemon.Individual individual)
	{
		return ability == individual.Ability;
	}

	public AbilityCriteria(string ability)
	{
		this.ability = ability;
	}
}
