using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PokemonPRNG.LCG32.StandardLCG;

namespace _3genSearch;

public static class ArrayExtensions
{
	public static bool And(this CheckBox[] array)
	{
		bool flag = true;
		foreach (CheckBox checkBox in array)
		{
			flag &= checkBox.Checked;
		}
		return flag;
	}

	public static bool Or(this CheckBox[] array)
	{
		bool flag = false;
		foreach (CheckBox checkBox in array)
		{
			flag |= checkBox.Checked;
		}
		return flag;
	}

	public static bool Xor(this CheckBox[] array)
	{
		bool flag = true;
		foreach (CheckBox checkBox in array)
		{
			flag ^= checkBox.Checked;
		}
		return flag;
	}

	internal static void Advance(this List<uint> seedList)
	{
		int num = seedList.Count();
		for (int i = 0; i < num - 1; i++)
		{
			seedList[i] = seedList[i + 1];
		}
		seedList[num - 1] = seedList[num - 1].NextSeed();
	}
}
