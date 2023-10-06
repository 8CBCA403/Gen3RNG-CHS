using System;
using System.Collections.Generic;
using Pokemon3genRNGLibrary;
using PokemonPRNG.LCG32.StandardLCG;
using PokemonStandardLibrary;
using PokemonStandardLibrary.CommonExtension;

namespace _3genSearch.Lecagy;

internal class FindWorker_Synchronize : FindWorker
{
	private bool checkSync(uint seed)
	{
		return seed.GetRand(2u) == 0;
	}

	internal override void DoWork(List<uint> seedList, List<GeneratingSeedResult> resList)
	{
		if (!base.isStopped)
		{
			if (checkSync(seedList[0]))
			{
				AddResult(seedList, resList, getLv, 0, "シンクロ(" + ((Nature)targetNature).ToJapanese() + ")");
			}
			if (!checkSync(seedList[1]) && FindWorker.GetNature(seedList[0]) == targetNature)
			{
				AddResult(seedList, resList, getLv, 1, "シンクロ");
			}
			if (GetPID(seedList[1]) % 25 == targetNature)
			{
				base.isStopped = true;
			}
		}
	}

	internal FindWorker_Synchronize(uint generatingSeed, Action<List<uint>, List<GeneratingSeedResult>, ILvGenerator, int, string> addResult)
		: base(generatingSeed, addResult)
	{
	}
}
