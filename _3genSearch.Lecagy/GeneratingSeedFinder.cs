using System.Collections.Generic;
using PokemonPRNG.LCG32.StandardLCG;

namespace _3genSearch.Lecagy;

public static class GeneratingSeedFinder
{
	private static readonly List<uint>[] LOWER;

	private static readonly List<uint>[] LOWER_zure;

	static GeneratingSeedFinder()
	{
		LOWER = new List<uint>[65536];
		LOWER_zure = new List<uint>[65536];
		for (uint num = 0u; num < 65536; num++)
		{
			LOWER[num] = new List<uint>();
			LOWER_zure[num] = new List<uint>();
		}
		for (uint num2 = 0u; num2 < 65536; num2++)
		{
			LOWER[num2.NextSeed() >> 16].Add(num2);
			LOWER_zure[num2.NextSeed(2u) >> 16].Add(num2);
		}
	}

	public static List<uint> FindGeneratingSeed(uint HAB, uint SCD, bool IVInterrupt, bool MiddleInterrupt)
	{
		List<uint> list = new List<uint>();
		List<uint>[] array = (IVInterrupt ? LOWER_zure : LOWER);
		uint num = (SCD - (uint)((IVInterrupt ? 39529 : 20077) * (int)HAB)) & 0xFFFFu;
		list.AddRange(array[num]);
		list.AddRange(array[num ^ 0x8000]);
		List<uint> list2 = new List<uint>();
		uint n = (MiddleInterrupt ? 4u : 3u);
		foreach (uint item in list)
		{
			uint num2 = ((HAB << 16) | item).PrevSeed(n);
			list2.Add(num2);
			list2.Add(num2 ^ 0x80000000u);
		}
		return list2;
	}
}
