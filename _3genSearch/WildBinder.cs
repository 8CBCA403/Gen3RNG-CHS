using PokemonPRNG.LCG32;
using PokemonStandardLibrary.CommonExtension;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch;

internal class WildBinder
{
	private readonly RNGResult<Pokemon.Individual, uint> _result;

	[DataGridViewRowHeader(80, "初期seed", false, null)]
	public string InitialSeed { get; }

	[DataGridViewRowHeader(66, "F", true, null)]
	public uint Frame { get; }

	[DataGridViewRowHeader(51, "ずれ", true, null)]
	public int Gap { get; }

	[DataGridViewRowHeader(66, "seed", false, null)]
	public string Seed => $"{_result.HeadSeed:X8}";

	[DataGridViewRowHeader(67, "ポケモン", true, null)]
	public string Name => $"{_result.Content.Name:X8}";

	[DataGridViewRowHeader(42, "Lv", false, null)]
	public uint Lv => _result.Content.Lv;

	[DataGridViewRowHeader(66, "性格値", false, null)]
	public string PID => $"{_result.Content.PID:X8}";

	[DataGridViewRowHeader(66, "性格", false, null)]
	public string Nature => _result.Content.Nature.ToJapanese();

	[DataGridViewRowHeader(27, "H", false, null)]
	public uint IVs_H => _result.Content.IVs[0];

	[DataGridViewRowHeader(27, "A", false, null)]
	public uint IVs_A => _result.Content.IVs[1];

	[DataGridViewRowHeader(27, "B", false, null)]
	public uint IVs_B => _result.Content.IVs[2];

	[DataGridViewRowHeader(27, "C", false, null)]
	public uint IVs_C => _result.Content.IVs[3];

	[DataGridViewRowHeader(27, "D", false, null)]
	public uint IVs_D => _result.Content.IVs[4];

	[DataGridViewRowHeader(27, "S", false, null)]
	public uint IVs_S => _result.Content.IVs[5];

	[DataGridViewRowHeader(66, "性別", false, "ＭＳ ゴシック")]
	public string Gender => _result.Content.Gender.ToSymbol();

	[DataGridViewRowHeader(100, "特性", true, null)]
	public string Ability => _result.Content.Ability;

	[DataGridViewRowHeader(30, "H", false, null)]
	public uint Stats_H => _result.Content.Stats[0];

	[DataGridViewRowHeader(30, "A", false, null)]
	public uint Stats_A => _result.Content.Stats[1];

	[DataGridViewRowHeader(30, "B", false, null)]
	public uint Stats_B => _result.Content.Stats[2];

	[DataGridViewRowHeader(30, "C", false, null)]
	public uint Stats_C => _result.Content.Stats[3];

	[DataGridViewRowHeader(30, "D", false, null)]
	public uint Stats_D => _result.Content.Stats[4];

	[DataGridViewRowHeader(30, "S", false, null)]
	public uint Stats_S => _result.Content.Stats[5];

	[DataGridViewRowHeader(60, "めざパ", false, null)]
	public string HiddenPower => $"{_result.Content.HiddenPowerType.ToKanji()}{_result.Content.HiddenPower}";

	[DataGridViewRowHeader(30, "#", false, null)]
	public uint Recalc => _result.Option;

	public bool IsShiny { get; }

	public WildBinder(uint initSeed, uint frame, uint targetFrame, RNGResult<Pokemon.Individual, uint> result, uint tsv)
	{
		string text = $"{initSeed:X4}";
		InitialSeed = text;
		_result = result;
		Frame = frame;
		Gap = (int)(frame - targetFrame);
		IsShiny = _result.Content.GetShinyType(tsv).IsShiny();
	}
}
