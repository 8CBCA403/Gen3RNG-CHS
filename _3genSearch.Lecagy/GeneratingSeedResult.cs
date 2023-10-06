using PokemonStandardLibrary;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch.Lecagy;

public class GeneratingSeedResult : Result
{
	public uint GeneratingSeed { get; internal set; }

	public string Option { get; internal set; }

	public Gender CuteCharmGender { get; internal set; }

	internal GeneratingSeedResult(uint genseed, uint srtseed, int slotIndex, Pokemon.Individual poke, string opt)
		: base(0u, 0u, slotIndex, poke, srtseed, 0u)
	{
		GeneratingSeed = genseed;
		Option = opt;
	}

	internal GeneratingSeedResult(uint genseed, uint srtseed, int slotIndex, Pokemon.Individual poke, string opt, Gender ccgender)
		: base(0u, 0u, slotIndex, poke, srtseed, 0u)
	{
		GeneratingSeed = genseed;
		Option = opt;
		CuteCharmGender = ccgender;
	}
}
