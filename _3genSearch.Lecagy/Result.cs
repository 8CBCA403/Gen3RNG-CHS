using PokemonStandardLibrary.Gen3;

namespace _3genSearch.Lecagy;

public class Result
{
	public uint Index { get; internal set; }

	public int SlotIndex { get; internal set; }

	public string Method { get; internal set; }

	public Pokemon.Individual Pokemon { get; internal set; }

	public uint InitialSeed { get; internal set; }

	public uint StartingSeed { get; internal set; }

	public uint FinishingSeed { get; internal set; }

	public Result(int slotIndex, Pokemon.Individual poke, uint srtSeed, string method)
	{
		SlotIndex = slotIndex;
		Pokemon = poke;
		StartingSeed = srtSeed;
		Method = method;
	}

	public Result(uint InitialSeed, uint Index, int slotIndex, Pokemon.Individual poke, uint srtSeed, uint finSeed)
	{
		this.InitialSeed = InitialSeed;
		this.Index = Index;
		SlotIndex = slotIndex;
		Pokemon = poke;
		StartingSeed = srtSeed;
		FinishingSeed = finSeed;
	}

	public Result(uint InitialSeed, uint Index, int slotIndex, Pokemon.Individual poke, uint srtSeed, uint finSeed, string method)
	{
		this.InitialSeed = InitialSeed;
		this.Index = Index;
		SlotIndex = slotIndex;
		Method = method;
		Pokemon = poke;
		StartingSeed = srtSeed;
		FinishingSeed = finSeed;
	}
}
