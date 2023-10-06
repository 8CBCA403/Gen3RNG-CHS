using System;
using System.Collections.Generic;
using System.Linq;
using Pokemon3genRNGLibrary;
using PokemonPRNG.LCG32.StandardLCG;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch.Lecagy;

internal class EmFeebasSeedFinder : EmSeedFinder
{
	private protected new const int BaseIndex = 2;

	private bool checkFeebas(uint seed)
	{
		return seed.GetRand(100u) < 50;
	}

	public override List<GeneratingSeedResult> FindEncounterSeed(uint GeneratingSeed)
	{
		List<GeneratingSeedResult> list = new List<GeneratingSeedResult>();
		List<FindWorker> list2 = SetUp(GeneratingSeed);
		List<uint> list3 = new List<uint> { GeneratingSeed.PrevSeed() };
		for (int i = 1; i < 10; i++)
		{
			list3.Add(list3[i - 1].PrevSeed());
		}
		List<(uint, string)> list4 = new List<(uint, string)>();
		while (list2.Any((FindWorker worker) => !worker.isStopped))
		{
			foreach (FindWorker item in list2)
			{
				item.DoWork(list3, list);
			}
			SeedFinder.Advance(list3);
		}
		return list;
	}

	private protected override List<FindWorker> SetUp(uint generatingSeed)
	{
		uint seed = generatingSeed;
		uint TargetPID = seed.GetRand() | (seed.GetRand() << 16);
		uint[] TargetIVs = getIVs.GenerateIVs(ref seed);
		Action<List<uint>, List<GeneratingSeedResult>, ILvGenerator, int, string> addResult = delegate(List<uint> seedList, List<GeneratingSeedResult> resList, ILvGenerator getLv, int indexShift, string option)
		{
			uint seed2 = seedList[2 + indexShift];
			if (checkFeebas(seed2))
			{
				uint seed3 = seed2;
				GBASlot gBASlot = getSlot.GenerateSlot(ref seed3);
				uint lv = getLv.GenerateLv(ref seed3, gBASlot.BasicLv, gBASlot.VariableLv);
				Pokemon.Individual individual = gBASlot.Pokemon.GetIndividual(lv, TargetIVs, TargetPID);
				resList.Add(new GeneratingSeedResult(generatingSeed, seed2, gBASlot.Index, individual, option)
				{
					Method = method.LegacyName
				});
			}
			seed2.Back();
			if (!checkFeebas(seed2))
			{
				uint seed4 = seed2;
				GBASlot gBASlot2 = getSlot.GenerateSlot(ref seed4);
				uint lv2 = getLv.GenerateLv(ref seed4, gBASlot2.BasicLv, gBASlot2.VariableLv);
				Pokemon.Individual individual2 = gBASlot2.Pokemon.GetIndividual(lv2, TargetIVs, TargetPID);
				resList.Add(new GeneratingSeedResult(generatingSeed, seed2, gBASlot2.Index, individual2, option)
				{
					Method = method.LegacyName
				});
			}
		};
		return new List<FindWorker>
		{
			new FindWorker(generatingSeed, addResult),
			new FindWorker_Synchronize(generatingSeed, addResult),
			new FindWorker_Pressure(generatingSeed, addResult),
			new FindWorker_CuteCharm(generatingSeed, getSlot, addResult)
		};
	}

	internal EmFeebasSeedFinder(GBAMap map, GenerateMethod method)
		: base(map, method)
	{
		getSlot = map.GetSlotGenerator(new WildGenerationArgument());
	}
}
