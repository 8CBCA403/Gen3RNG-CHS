using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace _3genSearch;

internal static class Util
{
	public static uint GetValueDec(TextBox textBox)
	{
		return Convert.ToUInt32(textBox.Text, 10);
	}

	public static uint GetValueDec(NumericUpDown numericUpDown)
	{
		return (uint)(numericUpDown?.Value ?? 0m);
	}

	public static uint GetValueAsUint32(this NumericUpDown numericUpDown)
	{
		return (uint)(numericUpDown?.Value ?? 0m);
	}

	public static uint GetValueHex(TextBox textBox)
	{
		return Convert.ToUInt32(textBox.Text, 16);
	}

	public static uint GetRTCSeed(uint Minute)
	{
		return 1440 * (Minute / 1440 + 1) + 60 * Convert.ToUInt32($"{Minute / 60 % 24}", 16) + Convert.ToUInt32($"{Minute % 60}", 16);
	}

	public static List<uint> GetHexList(TextBox seedBox)
	{
		List<uint> list = new List<uint>();
		List<string> list2 = seedBox.Text.Replace(Environment.NewLine, " ").Split().Distinct()
			.ToList();
		foreach (string item in list2)
		{
			if (item != "" && !Regex.IsMatch(item, "[^0-9a-fA-F]{1,8}"))
			{
				list.Add(Convert.ToUInt32(item, 16));
			}
		}
		return list;
	}

	public static (uint, uint) GetRange(NumericUpDown targetBox, NumericUpDown rangeBox)
	{
		uint valueDec = GetValueDec(targetBox);
		uint valueDec2 = GetValueDec(rangeBox);
		uint item = ((valueDec >= valueDec2) ? (valueDec - valueDec2) : 0u);
		uint item2 = (((uint)(-1 - (int)valueDec) > valueDec2) ? (valueDec + valueDec2) : uint.MaxValue);
		return (item, item2);
	}

	public static string RegaxReplace(this string arg1, string pattern, string replacement)
	{
		return Regex.Replace(arg1, pattern, replacement);
	}

	public static uint ToUint(this string arg)
	{
		if (!uint.TryParse(arg, out var result))
		{
			return 0u;
		}
		return result;
	}
}
