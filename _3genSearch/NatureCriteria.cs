using System.Collections.Generic;
using PokemonPRNG.LCG32;
using PokemonStandardLibrary;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch;

internal class NatureCriteria : ICriteria<Pokemon.Individual>
{
	private readonly HashSet<Nature> natures;

	public bool CheckConditions(Pokemon.Individual individual)
	{
		return natures.Contains(individual.Nature);
	}

	public NatureCriteria(Nature[] natures)
	{
		this.natures = new HashSet<Nature>(natures);
	}
}
