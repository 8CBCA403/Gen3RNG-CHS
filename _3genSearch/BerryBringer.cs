using PokemonPRNG.LCG32.StandardLCG;

namespace _3genSearch;

internal class BerryBringer
{
	private readonly string[] berries = new string[10] { "クラボ", "カゴ", "モモン", "チーゴ", "ナナシ", "ヒメリ", "オレン", "キー", "ラム", "オボン" };

	public string GetBerry(ref uint seed)
	{
		return berries[seed.GetRand(10u)];
	}
}
