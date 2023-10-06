using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Pokemon3genRNGLibrary;

namespace _3genSearch;

internal static class Common
{
	internal static readonly GenerateMethod[] methodList = new GenerateMethod[3]
	{
		StandardIVsGenerator.GetInstance(),
		PriorInterruptIVsGenerator.GetInstance(),
		MiddleInterruptedIVsGenerator.GetInstance()
	};

	internal static readonly PokeBlock[] pokeBlockList = new PokeBlock[6]
	{
		PokeBlock.Plain,
		PokeBlock.RedPokeBlock,
		PokeBlock.YellowPokeBlock,
		PokeBlock.BluePokeBlock,
		PokeBlock.GreenPokeBlock,
		PokeBlock.PinkPokeBlock
	};

	private static readonly Dictionary<EncounterType, uint[]> encounterRate = new Dictionary<EncounterType, uint[]>
	{
		{
			EncounterType.Grass,
			new uint[12]
			{
				20u, 20u, 10u, 10u, 10u, 10u, 5u, 5u, 4u, 4u,
				1u, 1u
			}
		},
		{
			EncounterType.Surf,
			new uint[5] { 60u, 30u, 5u, 4u, 1u }
		},
		{
			EncounterType.OldRod,
			new uint[2] { 70u, 30u }
		},
		{
			EncounterType.GoodRod,
			new uint[3] { 60u, 20u, 20u }
		},
		{
			EncounterType.SuperRod,
			new uint[5] { 40u, 40u, 15u, 4u, 1u }
		},
		{
			EncounterType.RockSmash,
			new uint[5] { 60u, 30u, 5u, 4u, 1u }
		}
	};

	internal static Rom[] RomList { get; } = new Rom[5]
	{
		Rom.Ruby,
		Rom.Sapphire,
		Rom.Em,
		Rom.FireRed,
		Rom.LeafGreen
	};


	internal static void UpdateTable(this DataGridView dgv, GBAMap SelectedMap, EncounterType SelectedEncounterType)
	{
		dgv.Rows.Clear();
		dgv.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "");
		dgv.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "");
		dgv.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "");
		IReadOnlyList<GBASlot> encounterTable = SelectedMap.EncounterTable;
		for (int i = 0; i < encounterTable.Count; i++)
		{
			uint[] array = encounterRate[SelectedEncounterType];
			GBASlot gBASlot = encounterTable[i];
			dgv[i, 0].Value = $"{i} ({array[i]}%)";
			dgv[i, 1].Value = gBASlot.Pokemon.GetDefaultName();
			dgv[i, 2].Value = ((gBASlot.VariableLv == 1) ? $"Lv.{gBASlot.BasicLv}" : $"Lv.{gBASlot.BasicLv} - {gBASlot.BasicLv + gBASlot.VariableLv - 1}");
		}
		dgv.Rows[0].DefaultCellStyle.BackColor = Color.FromArgb(221, 221, 221);
		dgv.ClearSelection();
	}
}
