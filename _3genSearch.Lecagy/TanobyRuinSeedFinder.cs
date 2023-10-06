using System;
using System.Collections.Generic;
using Pokemon3genRNGLibrary;
using PokemonPRNG.LCG32.StandardLCG;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch.Lecagy;

internal class TanobyRuinSeedFinder : SeedFinder
{
	private protected override List<FindWorker> SetUp(uint generatingSeed)
	{
		uint seed = generatingSeed;
		uint TargetPID = (seed.GetRand() << 16) | seed.GetRand();
		uint[] TargetIVs = getIVs.GenerateIVs(ref seed);
		Action<List<uint>, List<GeneratingSeedResult>, ILvGenerator, int, string> addResult = delegate(List<uint> seedList, List<GeneratingSeedResult> resList, ILvGenerator getLv, int indexShift, string option)
		{
			uint num = seedList[2 + indexShift];
			uint seed2 = num;
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
			new FindWorker_TanobyRuin(generatingSeed, base.map.GetSlotGenerator(new WildGenerationArgument()), addResult)
		};
	}

	internal TanobyRuinSeedFinder(GBAMap map, GenerateMethod method)
		: base(map, method)
	{
	}
}
