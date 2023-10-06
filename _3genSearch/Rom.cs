using System;
using System.Collections.Generic;
using System.Linq;
using Pokemon3genRNGLibrary;
using Pokemon3genRNGLibrary.MapData;

namespace _3genSearch;

internal class Rom
{
	private Dictionary<EncounterType, List<GBAMap>> mapList;

	private List<StationarySymbol> stationarySymbolList;

	public static readonly Rom Ruby = new Rom
	{
		mapList = new Dictionary<EncounterType, List<GBAMap>>
		{
			{
				EncounterType.Grass,
				RubyMapData.Field.SelectMap().ToList()
			},
			{
				EncounterType.Surf,
				RubyMapData.Surf.SelectMap().ToList()
			},
			{
				EncounterType.OldRod,
				RubyMapData.OldRod.SelectMap().ToList()
			},
			{
				EncounterType.GoodRod,
				RubyMapData.GoodRod.SelectMap().ToList()
			},
			{
				EncounterType.SuperRod,
				RubyMapData.SuperRod.SelectMap().ToList()
			},
			{
				EncounterType.RockSmash,
				RubyMapData.RockSmash.SelectMap().ToList()
			}
		},
		stationarySymbolList = new List<StationarySymbol>
		{
			new StationarySymbol("イベント", new GBASlot(-1, "キモリ", 5u)),
			new StationarySymbol("イベント", new GBASlot(-1, "アチャモ", 5u)),
			new StationarySymbol("イベント", new GBASlot(-1, "ミズゴロウ", 5u)),
			new StationarySymbol("イベント(卵)", new GBASlot(-1, "ソーナノ", 5u)),
			new StationarySymbol("イベント", new GBASlot(-1, "ポワルン", 25u)),
			new StationarySymbol("イベント", new GBASlot(-1, "ダンバル", 5u)),
			new StationarySymbol("イベント(化石)", new GBASlot(-1, "リリーラ", 20u)),
			new StationarySymbol("イベント(化石)", new GBASlot(-1, "アノプス", 20u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "カクレオン", 30u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "ビリリダマ", 25u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "マルマイン", 30u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "レジロック", 40u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "レジアイス", 40u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "レジスチル", 40u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "ラティアス", 50u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "グラードン", 45u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "レックウザ", 70u)),
			new StationarySymbol("徘徊", new GBASlot(-1, "ラティオス", 40u))
		}
	};

	public static readonly Rom Sapphire = new Rom
	{
		mapList = new Dictionary<EncounterType, List<GBAMap>>
		{
			{
				EncounterType.Grass,
				SapphireMapData.Field.SelectMap().ToList()
			},
			{
				EncounterType.Surf,
				SapphireMapData.Surf.SelectMap().ToList()
			},
			{
				EncounterType.OldRod,
				SapphireMapData.OldRod.SelectMap().ToList()
			},
			{
				EncounterType.GoodRod,
				SapphireMapData.GoodRod.SelectMap().ToList()
			},
			{
				EncounterType.SuperRod,
				SapphireMapData.SuperRod.SelectMap().ToList()
			},
			{
				EncounterType.RockSmash,
				SapphireMapData.RockSmash.SelectMap().ToList()
			}
		},
		stationarySymbolList = new List<StationarySymbol>
		{
			new StationarySymbol("イベント", new GBASlot(-1, "キモリ", 5u)),
			new StationarySymbol("イベント", new GBASlot(-1, "アチャモ", 5u)),
			new StationarySymbol("イベント", new GBASlot(-1, "ミズゴロウ", 5u)),
			new StationarySymbol("イベント(卵)", new GBASlot(-1, "ソーナノ", 5u)),
			new StationarySymbol("イベント", new GBASlot(-1, "ポワルン", 25u)),
			new StationarySymbol("イベント", new GBASlot(-1, "ダンバル", 5u)),
			new StationarySymbol("イベント(化石)", new GBASlot(-1, "リリーラ", 20u)),
			new StationarySymbol("イベント(化石)", new GBASlot(-1, "アノプス", 20u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "カクレオン", 30u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "ビリリダマ", 25u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "マルマイン", 30u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "レジロック", 40u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "レジアイス", 40u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "レジスチル", 40u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "ラティオス", 50u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "カイオーガ", 45u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "レックウザ", 70u)),
			new StationarySymbol("徘徊", new GBASlot(-1, "ラティアス", 40u))
		}
	};

	public static readonly Rom FireRed = new Rom
	{
		mapList = new Dictionary<EncounterType, List<GBAMap>>
		{
			{
				EncounterType.Grass,
				FRMapData.Field.SelectMap().ToList()
			},
			{
				EncounterType.Surf,
				FRMapData.Surf.SelectMap().ToList()
			},
			{
				EncounterType.OldRod,
				FRMapData.OldRod.SelectMap().ToList()
			},
			{
				EncounterType.GoodRod,
				FRMapData.GoodRod.SelectMap().ToList()
			},
			{
				EncounterType.SuperRod,
				FRMapData.SuperRod.SelectMap().ToList()
			},
			{
				EncounterType.RockSmash,
				FRMapData.RockSmash.SelectMap().ToList()
			}
		},
		stationarySymbolList = new List<StationarySymbol>
		{
			new StationarySymbol("イベント", new GBASlot(-1, "フシギダネ", 5u)),
			new StationarySymbol("イベント", new GBASlot(-1, "ヒトカゲ", 5u)),
			new StationarySymbol("イベント", new GBASlot(-1, "ゼニガメ", 5u)),
			new StationarySymbol("イベント", new GBASlot(-1, "コイキング", 5u)),
			new StationarySymbol("イベント(景品)", new GBASlot(-1, "ピッピ", 8u)),
			new StationarySymbol("イベント(景品)", new GBASlot(-1, "ケーシィ", 9u)),
			new StationarySymbol("イベント(景品)", new GBASlot(-1, "ミニリュウ", 18u)),
			new StationarySymbol("イベント(景品)", new GBASlot(-1, "ストライク", 25u)),
			new StationarySymbol("イベント(景品)", new GBASlot(-1, "ポリゴン", 26u)),
			new StationarySymbol("イベント(化石)", new GBASlot(-1, "オムナイト", 5u)),
			new StationarySymbol("イベント(化石)", new GBASlot(-1, "カブト", 5u)),
			new StationarySymbol("イベント(化石)", new GBASlot(-1, "プテラ", 5u)),
			new StationarySymbol("イベント", new GBASlot(-1, "イーブイ", 25u)),
			new StationarySymbol("イベント", new GBASlot(-1, "サワムラー", 25u)),
			new StationarySymbol("イベント", new GBASlot(-1, "エビワラー", 25u)),
			new StationarySymbol("イベント", new GBASlot(-1, "ラプラス", 25u)),
			new StationarySymbol("イベント(卵)", new GBASlot(-1, "トゲピー", 5u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "スリーパー", 30u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "マルマイン", 34u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "カビゴン", 30u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "フリーザー", 50u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "サンダー", 50u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "ファイヤー", 50u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "ミュウツー", 70u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "ルギア", 70u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "ホウオウ", 70u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "デオキシス#アタック", 30u)),
			new StationarySymbol("徘徊", new GBASlot(-1, "ライコウ", 50u)),
			new StationarySymbol("徘徊", new GBASlot(-1, "エンテイ", 50u)),
			new StationarySymbol("徘徊", new GBASlot(-1, "スイクン", 50u))
		}
	};

	public static readonly Rom LeafGreen = new Rom
	{
		mapList = new Dictionary<EncounterType, List<GBAMap>>
		{
			{
				EncounterType.Grass,
				LGMapData.Field.SelectMap().ToList()
			},
			{
				EncounterType.Surf,
				LGMapData.Surf.SelectMap().ToList()
			},
			{
				EncounterType.OldRod,
				LGMapData.OldRod.SelectMap().ToList()
			},
			{
				EncounterType.GoodRod,
				LGMapData.GoodRod.SelectMap().ToList()
			},
			{
				EncounterType.SuperRod,
				LGMapData.SuperRod.SelectMap().ToList()
			},
			{
				EncounterType.RockSmash,
				LGMapData.RockSmash.SelectMap().ToList()
			}
		},
		stationarySymbolList = new List<StationarySymbol>
		{
			new StationarySymbol("イベント", new GBASlot(-1, "フシギダネ", 5u)),
			new StationarySymbol("イベント", new GBASlot(-1, "ヒトカゲ", 5u)),
			new StationarySymbol("イベント", new GBASlot(-1, "ゼニガメ", 5u)),
			new StationarySymbol("イベント", new GBASlot(-1, "コイキング", 5u)),
			new StationarySymbol("イベント(景品)", new GBASlot(-1, "ケーシィ", 7u)),
			new StationarySymbol("イベント(景品)", new GBASlot(-1, "ピッピ", 12u)),
			new StationarySymbol("イベント(景品)", new GBASlot(-1, "カイロス", 18u)),
			new StationarySymbol("イベント(景品)", new GBASlot(-1, "ミニリュウ", 24u)),
			new StationarySymbol("イベント(景品)", new GBASlot(-1, "ポリゴン", 18u)),
			new StationarySymbol("イベント(化石)", new GBASlot(-1, "オムナイト", 5u)),
			new StationarySymbol("イベント(化石)", new GBASlot(-1, "カブト", 5u)),
			new StationarySymbol("イベント(化石)", new GBASlot(-1, "プテラ", 5u)),
			new StationarySymbol("イベント", new GBASlot(-1, "イーブイ", 25u)),
			new StationarySymbol("イベント", new GBASlot(-1, "サワムラー", 25u)),
			new StationarySymbol("イベント", new GBASlot(-1, "エビワラー", 25u)),
			new StationarySymbol("イベント", new GBASlot(-1, "ラプラス", 25u)),
			new StationarySymbol("イベント(卵)", new GBASlot(-1, "トゲピー", 5u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "スリーパー", 30u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "マルマイン", 34u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "カビゴン", 30u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "フリーザー", 50u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "サンダー", 50u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "ファイヤー", 50u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "ミュウツー", 70u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "ルギア", 70u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "ホウオウ", 70u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "デオキシス#ディフェンス", 30u)),
			new StationarySymbol("徘徊", new GBASlot(-1, "ライコウ", 50u)),
			new StationarySymbol("徘徊", new GBASlot(-1, "エンテイ", 50u)),
			new StationarySymbol("徘徊", new GBASlot(-1, "スイクン", 50u))
		}
	};

	public static Rom Em { get; } = new Rom
	{
		mapList = new Dictionary<EncounterType, List<GBAMap>>
		{
			{
				EncounterType.Grass,
				EmMapData.Field.SelectMap().ToList()
			},
			{
				EncounterType.Surf,
				EmMapData.Surf.SelectMap().ToList()
			},
			{
				EncounterType.OldRod,
				EmMapData.OldRod.SelectMap().ToList()
			},
			{
				EncounterType.GoodRod,
				EmMapData.GoodRod.SelectMap().ToList()
			},
			{
				EncounterType.SuperRod,
				EmMapData.SuperRod.SelectMap().ToList()
			},
			{
				EncounterType.RockSmash,
				EmMapData.RockSmash.SelectMap().ToList()
			}
		},
		stationarySymbolList = new List<StationarySymbol>
		{
			new StationarySymbol("イベント", new GBASlot(-1, "キモリ", 5u)),
			new StationarySymbol("イベント", new GBASlot(-1, "アチャモ", 5u)),
			new StationarySymbol("イベント", new GBASlot(-1, "ミズゴロウ", 5u)),
			new StationarySymbol("イベント(卵)", new GBASlot(-1, "ソーナノ", 5u)),
			new StationarySymbol("イベント", new GBASlot(-1, "ポワルン", 25u)),
			new StationarySymbol("イベント", new GBASlot(-1, "ダンバル", 5u)),
			new StationarySymbol("イベント(化石)", new GBASlot(-1, "リリーラ", 20u)),
			new StationarySymbol("イベント(化石)", new GBASlot(-1, "アノプス", 20u)),
			new StationarySymbol("イベント", new GBASlot(-1, "チコリータ", 5u)),
			new StationarySymbol("イベント", new GBASlot(-1, "ヒノアラシ", 5u)),
			new StationarySymbol("イベント", new GBASlot(-1, "ワニノコ", 5u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "ビリリダマ", 25u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "マルマイン", 30u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "ウソッキー", 40u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "カクレオン", 30u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "レジロック", 40u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "レジアイス", 40u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "レジスチル", 40u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "ラティアス", 50u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "ラティオス", 50u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "グラードン", 70u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "カイオーガ", 70u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "レックウザ", 70u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "ミュウ", 30u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "ルギア", 70u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "ホウオウ", 70u)),
			new StationarySymbol("シンボル", new GBASlot(-1, "デオキシス#スピード", 30u)),
			new StationarySymbol("徘徊", new GBASlot(-1, "ラティオス", 40u)),
			new StationarySymbol("徘徊", new GBASlot(-1, "ラティアス", 40u))
		}
	};


	public GBAMap GetMap(EncounterType encounterType, int index)
	{
		return mapList[encounterType][index];
	}

	public List<string> GetMapNameList(EncounterType encounterType)
	{
		return mapList[encounterType].Select((GBAMap _) => _.MapName).ToList();
	}

	private Rom()
	{
	}

	public StationarySymbol GetStationarySymbol(int index)
	{
		if (stationarySymbolList.Count <= index)
		{
			throw new IndexOutOfRangeException();
		}
		return stationarySymbolList[index];
	}

	public List<GBASlot> GetStationarySymbolList()
	{
		return stationarySymbolList.Select((StationarySymbol _) => _.GetSymbol()).ToList();
	}

	public List<string> GetStationarySymbolLabelList()
	{
		return stationarySymbolList.Select((StationarySymbol _) => _.GetLabel()).ToList();
	}
}
