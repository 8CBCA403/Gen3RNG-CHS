using System;
using System.Collections.Generic;
using System.Linq;
using Pokemon3genRNGLibrary;
using PokemonPRNG.LCG32.StandardLCG;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch.Lecagy;

public class SeedFinder
{
	private protected const int BaseIndex = 2;

	private protected GenerateMethod method;

	private protected SlotGenerator getSlot;

	private protected ILvGenerator getLv;

	private protected IIVsGenerator getIVs;

	private protected GBAMap map { get; }

	private protected virtual List<FindWorker> SetUp(uint generatingSeed)
	{
		uint seed = generatingSeed;
		uint targetPID = seed.GetRand() | (seed.GetRand() << 16);
		uint[] targetIVs = getIVs.GenerateIVs(ref seed);
		Action<List<uint>, List<GeneratingSeedResult>, ILvGenerator, int, string> addResult = delegate(List<uint> seedList, List<GeneratingSeedResult> resList, ILvGenerator getLv, int indexShift, string option)
		{
			uint num = seedList[2 + indexShift];
			uint seed2 = num;
			GBASlot gBASlot = getSlot.GenerateSlot(ref seed2);
			uint lv = getLv.GenerateLv(ref seed2, gBASlot.BasicLv, gBASlot.VariableLv);
			Pokemon.Individual individual = gBASlot.Pokemon.GetIndividual(lv, targetIVs, targetPID);
			resList.Add(new GeneratingSeedResult(generatingSeed, num, gBASlot.Index, individual, option)
			{
				Method = method.LegacyName
			});
		};
		return new List<FindWorker>
		{
			new FindWorker(generatingSeed, addResult)
		};
	}

	public virtual List<GeneratingSeedResult> FindEncounterSeed(uint GeneratingSeed)
	{
		List<FindWorker> list = SetUp(GeneratingSeed);
		List<uint> list2 = new List<uint> { GeneratingSeed.PrevSeed() };
		for (int i = 1; i < 10; i++)
		{
			list2.Add(list2[i - 1].PrevSeed());
		}
		List<GeneratingSeedResult> list3 = new List<GeneratingSeedResult>();
		while (list.Any((FindWorker worker) => !worker.isStopped))
		{
			foreach (FindWorker item in list)
			{
				item.DoWork(list2, list3);
			}
			Advance(list2);
		}
		return list3;
	}

	internal SeedFinder(GBAMap map, GenerateMethod method)
	{
		FieldAbility otherAbility = FieldAbility.GetOtherAbility();
		this.map = map;
		this.method = method;
		getSlot = map.GetSlotGenerator(new WildGenerationArgument());
		getLv = otherAbility.lvGenerator;
		getIVs = method;
	}

	private protected static void Advance(List<uint> seedList, int n = 2)
	{
		int num = seedList.Count();
		int i;
		for (i = 0; i < num - n; i++)
		{
			seedList[i] = seedList[i + n];
		}
		for (; i < num; i++)
		{
			seedList[i] = seedList[i].PrevSeed((uint)n);
		}
	}
}
