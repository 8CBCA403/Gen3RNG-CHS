using PokemonPRNG.LCG32;
using PokemonStandardLibrary;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch;

internal class ShinyCriteria : ICriteria<Pokemon.Individual>
{
	private readonly ShinyType shinyType;

	private readonly uint tsv;

	public bool CheckConditions(Pokemon.Individual item)
	{
		return (item.GetShinyType(tsv) & shinyType) != 0;
	}

	public ShinyCriteria(uint tsv, ShinyType shinyType)
	{
		this.shinyType = shinyType;
		this.tsv = tsv;
	}
}
