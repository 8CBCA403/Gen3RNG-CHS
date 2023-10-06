using System;
using System.Collections.Generic;
using Pokemon3genRNGLibrary;
using PokemonPRNG.LCG32.StandardLCG;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch.Lecagy;

internal class EmRoute119Finder : EmSeedFinder
{
	private protected new const int BaseIndex = 3;

	internal EmRoute119Finder(GBAMap map, GenerateMethod method)
		: base(map, method)
	{
	}

	private protected override List<FindWorker> SetUp(uint generatingSeed)
	{
		uint seed = generatingSeed;
		uint TargetPID = seed.GetRand() | (seed.GetRand() << 16);
		uint[] TargetIVs = getIVs.GenerateIVs(ref seed);
		Action<List<uint>, List<GeneratingSeedResult>, ILvGenerator, int, string> addResult = delegate(List<uint> seedList, List<GeneratingSeedResult> resList, ILvGenerator getLv, int indexShift, string option)
		{
			uint num = seedList[3 + indexShift];
			uint seed2 = num.NextSeed();
			GBASlot gBASlot = getSlot.GenerateSlot(ref seed2);
			uint lv = getLv.GenerateLv(ref seed2, gBASlot.BasicLv, gBASlot.VariableLv);
			Pokemon.Individual individual = gBASlot.Pokemon.GetIndividual(lv, TargetIVs, TargetPID);
			resList.Add(new GeneratingSeedResult(generatingSeed, num, gBASlot.Index, individual, option)
			{
				Method = method.LegacyName
			});
		};
		return new List<FindWorker>
		{
			new FindWorker(generatingSeed, addResult),
			new FindWorker_Synchronize(generatingSeed, addResult),
			new FindWorker_Pressure(generatingSeed, addResult),
			new FindWorker_CuteCharm(generatingSeed, getSlot, addResult)
		};
	}
}
