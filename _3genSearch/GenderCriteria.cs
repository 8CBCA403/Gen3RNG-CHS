using PokemonPRNG.LCG32;
using PokemonStandardLibrary;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch;

internal class GenderCriteria : ICriteria<Pokemon.Individual>
{
	private readonly Gender gender;

	public bool CheckConditions(Pokemon.Individual individual)
	{
		return gender == individual.Gender;
	}

	public GenderCriteria(Gender gender)
	{
		this.gender = gender;
	}
}
