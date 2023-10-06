using System;
using System.Collections.Generic;
using Pokemon3genRNGLibrary;
using PokemonPRNG.LCG32.StandardLCG;

namespace _3genSearch.Lecagy;

internal class FindWorker
{
	private protected uint generatingSeed;

	private protected uint targetNature;

	private protected ILvGenerator getLv;

	private protected Action<List<uint>, List<GeneratingSeedResult>, ILvGenerator, int, string> AddResult;

	internal bool isStopped { get; private protected set; }

	internal virtual void DoWork(List<uint> seedList, List<GeneratingSeedResult> resList)
	{
		if (!isStopped)
		{
			if (GetNature(seedList[0]) == targetNature)
			{
				AddResult(seedList, resList, getLv, 0, "---");
			}
			if (GetPID(seedList[1]) % 25 == targetNature)
			{
				isStopped = true;
			}
		}
	}

	internal FindWorker(uint generatingSeed, Action<List<uint>, List<GeneratingSeedResult>, ILvGenerator, int, string> addResult)
	{
		this.generatingSeed = generatingSeed;
		AddResult = addResult;
		targetNature = GetPID(generatingSeed) % 25;
		getLv = FieldAbility.GetOtherAbility().lvGenerator;
	}

	protected static uint GetNature(uint seed)
	{
		return seed.GetRand(25u);
	}

	protected virtual uint GetPID(uint seed)
	{
		return seed.GetRand() | (seed.GetRand() << 16);
	}
}
