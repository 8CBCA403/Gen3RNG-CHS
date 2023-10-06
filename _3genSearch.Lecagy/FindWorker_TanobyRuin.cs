using System;
using System.Collections.Generic;
using Pokemon3genRNGLibrary;
using PokemonPRNG.LCG32.StandardLCG;

namespace _3genSearch.Lecagy;

internal class FindWorker_TanobyRuin : FindWorker
{
	private readonly string targetForme;

	private readonly Func<uint, string> GetForme;

	private static readonly string[] UnownForms = new string[28]
	{
		"A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
		"K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
		"U", "V", "W", "X", "Y", "Z", "!", "?"
	};

	internal override void DoWork(List<uint> seedList, List<GeneratingSeedResult> resList)
	{
		if (!base.isStopped)
		{
			if (GetForme(seedList[1]) == targetForme)
			{
				AddResult(seedList, resList, getLv, -1, "---");
			}
			if (GetUnownForm(GetPID(seedList[1])) == targetForme)
			{
				base.isStopped = true;
			}
		}
	}

	internal FindWorker_TanobyRuin(uint generatingSeed, SlotGenerator getSlot, Action<List<uint>, List<GeneratingSeedResult>, ILvGenerator, int, string> addResult)
		: base(generatingSeed, addResult)
	{
		GetForme = (uint seed) => getSlot.GenerateSlot(ref seed).Pokemon.Form;
		targetForme = GetUnownForm(GetPID(generatingSeed));
	}

	private static string GetUnownForm(uint PID)
	{
		uint num = (PID & 3u) | ((PID >> 6) & 0xCu) | ((PID >> 12) & 0x30u) | ((PID >> 18) & 0xC0u);
		return UnownForms[num % 28];
	}

	protected override uint GetPID(uint seed)
	{
		return (seed.GetRand() << 16) | seed.GetRand();
	}
}
