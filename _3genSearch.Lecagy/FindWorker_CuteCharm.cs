using System;
using System.Collections.Generic;
using System.Linq;
using Pokemon3genRNGLibrary;
using PokemonPRNG.LCG32.StandardLCG;
using PokemonStandardLibrary;
using PokemonStandardLibrary.CommonExtension;

namespace _3genSearch.Lecagy;

internal class FindWorker_CuteCharm : FindWorker
{
	private readonly uint targetGV;

	private bool OverBorder;

	private int hantei;

	private readonly Func<uint, GenderRatio> GetGenderRatio;

	private readonly Dictionary<GenderRatio, bool> Border = new Dictionary<GenderRatio, bool>
	{
		[GenderRatio.MaleOnly] = true,
		[GenderRatio.M7F1] = false,
		[GenderRatio.M3F1] = false,
		[GenderRatio.M1F1] = false,
		[GenderRatio.M1F3] = false,
		[GenderRatio.FemaleOnly] = true,
		[GenderRatio.Genderless] = true
	};

	private bool checkCuteCharm(uint seed)
	{
		return seed.GetRand(3u) != 0;
	}

	internal override void DoWork(List<uint> seedList, List<GeneratingSeedResult> resList)
	{
		if (!base.isStopped)
		{
			GenderRatio genderRatio = GetGenderRatio(seedList[3]);
			Gender gender = GetGender(targetGV, genderRatio);
			if (checkCuteCharm(seedList[hantei]) && !Border[genderRatio] && FindWorker.GetNature(seedList[0]) == targetNature)
			{
				AddResult(seedList, resList, getLv, 1, "メロメロボディ(" + gender.Reverse().ToSymbol() + ")");
			}
			if (!checkCuteCharm(seedList[hantei]) && !OverBorder && FindWorker.GetNature(seedList[0]) == targetNature)
			{
				AddResult(seedList, resList, getLv, 1, "メロメロボディ");
			}
			uint pID = GetPID(seedList[1]);
			if (pID % 25 == targetNature)
			{
				OverBorder = true;
				Border[GenderRatio.M7F1] |= GetGender(pID & 0xFFu, GenderRatio.M7F1) == GetGender(targetGV, GenderRatio.M7F1);
				Border[GenderRatio.M3F1] |= GetGender(pID & 0xFFu, GenderRatio.M3F1) == GetGender(targetGV, GenderRatio.M3F1);
				Border[GenderRatio.M1F1] |= GetGender(pID & 0xFFu, GenderRatio.M1F1) == GetGender(targetGV, GenderRatio.M1F1);
				Border[GenderRatio.M1F3] |= GetGender(pID & 0xFFu, GenderRatio.M1F3) == GetGender(targetGV, GenderRatio.M1F3);
			}
			base.isStopped |= Border.All((KeyValuePair<GenderRatio, bool> _) => _.Value);
		}
	}

	internal FindWorker_CuteCharm(uint generatingSeed, SlotGenerator getSlot, Action<List<uint>, List<GeneratingSeedResult>, ILvGenerator, int, string> addResult, int hantei = 1)
		: base(generatingSeed, addResult)
	{
		GetGenderRatio = (uint seed) => getSlot.GenerateSlot(ref seed).Pokemon.GenderRatio;
		targetGV = GetPID(generatingSeed) & 0xFFu;
		this.hantei = hantei;
	}

	private Gender GetGender(uint gvalue, GenderRatio ratio)
	{
		if (ratio == GenderRatio.Genderless)
		{
			return Gender.Genderless;
		}
		if (gvalue >= (uint)ratio)
		{
			return Gender.Male;
		}
		return Gender.Female;
	}
}
