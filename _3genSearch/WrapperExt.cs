using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch;

internal static class WrapperExt
{
	public static uint? Seed(this TextBox textBox)
	{
		if (!Regex.IsMatch(textBox.Text, "[^0-9a-fA-F]") && textBox.Text.Length != 0)
		{
			return Convert.ToUInt32(textBox.Text, 16);
		}
		return null;
	}

	public static uint UValue(this NumericUpDown numericUpDown)
	{
		return (uint)numericUpDown.Value;
	}

	public static Pokemon.Species ToPokemon(this string name)
	{
		if (!Pokemon.TryGetPokemon(name, out var pokemon))
		{
			return null;
		}
		return pokemon;
	}
}
