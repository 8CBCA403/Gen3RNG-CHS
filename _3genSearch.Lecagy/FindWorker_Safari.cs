using System;
using System.Collections.Generic;
using Pokemon3genRNGLibrary;

namespace _3genSearch.Lecagy;

internal class FindWorker_Safari : FindWorker
{
	internal override void DoWork(List<uint> seedList, List<GeneratingSeedResult> resList)
	{
		if (!base.isStopped)
		{
			if (FindWorker.GetNature(seedList[0]) == targetNature)
			{
				AddResult(seedList, resList, getLv, 0, "---");
			}
			if (GetPID(seedList[1]) % 25 == targetNature)
			{
				base.isStopped = true;
			}
		}
	}

	internal FindWorker_Safari(uint generatingSeed, Action<List<uint>, List<GeneratingSeedResult>, ILvGenerator, int, string> addResult)
		: base(generatingSeed, addResult)
	{
	}
}
