using PokemonPRNG.LCG32;
using PokemonStandardLibrary;
using PokemonStandardLibrary.CommonExtension;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch;

public class EggPIDBinder
{
	public string Initialseed { get; }

	[DataGridViewRowHeader(60, "F", true, null)]
	public uint Index { get; }

	[DataGridViewRowHeader(66, "差分", false, null)]
	public uint Difference { get; }

	[DataGridViewRowHeader(66, "性格値", false, null)]
	public string PID { get; }

	[DataGridViewRowHeader(66, "性格", false, null)]
	public string Nature { get; }

	[DataGridViewRowHeader(100, "特性", false, null)]
	public string Ability { get; }

	[DataGridViewRowHeader(66, "性別", false, "ＭＳ ゴシック")]
	public string Gender { get; }

	public string Rand { get; }

	public bool IsShiny { get; }

	public EggPIDBinder(uint frame, uint initSeed, uint diff, Pokemon.Species pokemon, RNGResult<uint?> res, uint TSV)
	{
		Initialseed = $"{initSeed:X8}";
		Index = frame;
		Difference = diff;
		Rand = $"{res.HeadSeed:X8}";
		if (res.Content.HasValue)
		{
			uint value = res.Content.Value;
			Nature = ((Nature)(value % 25)).ToJapanese() ?? "";
			PID = $"{value:X8}";
			Ability = pokemon.Ability[(int)(value & 1)];
			Gender = CommonFunctions.GetGender(value & 0xFFu, pokemon.GenderRatio).ToSymbol();
			IsShiny = CommonFunctions.GetShinyType((value & 0xFFFFu) ^ (value >> 16), TSV, 8u).IsShiny();
		}
	}
}
