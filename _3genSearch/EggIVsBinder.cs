using System.Drawing;
using PokemonPRNG.LCG32;
using PokemonStandardLibrary.CommonExtension;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch;

public class EggIVsBinder
{
	private readonly RNGResult<Pokemon.Individual, (int Stat, int Parent)[]> _result;

	private readonly uint _target;

	[DataGridViewRowHeader(66, "F", true, null)]
	public uint Index { get; }

	[DataGridViewRowHeader(51, "ずれ", true, null)]
	public int Gap { get; }

	[DataGridViewRowHeader(25, "H", false, null)]
	public uint IV_H => _result.Content.IVs[0];

	[DataGridViewRowHeader(25, "A", false, null)]
	public uint IV_A => _result.Content.IVs[1];

	[DataGridViewRowHeader(25, "B", false, null)]
	public uint IV_B => _result.Content.IVs[2];

	[DataGridViewRowHeader(25, "C", false, null)]
	public uint IV_C => _result.Content.IVs[3];

	[DataGridViewRowHeader(25, "D", false, null)]
	public uint IV_D => _result.Content.IVs[4];

	[DataGridViewRowHeader(25, "S", false, null)]
	public uint IV_S => _result.Content.IVs[5];

	[DataGridViewRowHeader(67, "Method", false, null)]
	public string Method { get; set; }

	[DataGridViewRowHeader(30, "H", false, null)]
	public uint Stat_H => _result.Content.Stats[0];

	[DataGridViewRowHeader(30, "A", false, null)]
	public uint Stat_A => _result.Content.Stats[1];

	[DataGridViewRowHeader(30, "B", false, null)]
	public uint Stat_B => _result.Content.Stats[2];

	[DataGridViewRowHeader(30, "C", false, null)]
	public uint Stat_C => _result.Content.Stats[3];

	[DataGridViewRowHeader(30, "D", false, null)]
	public uint Stat_D => _result.Content.Stats[4];

	[DataGridViewRowHeader(30, "S", false, null)]
	public uint Stat_S => _result.Content.Stats[5];

	[DataGridViewRowHeader(60, "めざパ", false, null)]
	public string HiddenPower => $"{_result.Content.HiddenPowerType.ToKanji()}{_result.Content.HiddenPower}";

	public string Rand => $"{_result.HeadSeed:X8}";

	internal int HeredityPoint1 => _result.Option[0].Stat;

	internal int HeredityPoint2 => _result.Option[1].Stat;

	internal int HeredityPoint3 => _result.Option[2].Stat;

	internal Color Color1
	{
		get
		{
			if (_result.Option[0].Parent != 0)
			{
				return Color.DodgerBlue;
			}
			return Color.Red;
		}
	}

	internal Color Color2
	{
		get
		{
			if (_result.Option[1].Parent != 0)
			{
				return Color.DodgerBlue;
			}
			return Color.Red;
		}
	}

	internal Color Color3
	{
		get
		{
			if (_result.Option[2].Parent != 0)
			{
				return Color.DodgerBlue;
			}
			return Color.Red;
		}
	}

	public EggIVsBinder(RNGResult<Pokemon.Individual, (int Stat, int Parent)[]> res, uint frame, uint targetFrame, string Method)
	{
		_result = res;
		_target = targetFrame;
		this.Method = Method;
		Index = frame;
		Gap = (int)(frame - _target);
	}
}
