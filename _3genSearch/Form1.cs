using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using _3genSearch.Omake;
using Pokemon3genRNGLibrary;
using Pokemon3genRNGLibrary.InitialSeed;
using PokemonPRNG.LCG32;
using PokemonPRNG.LCG32.StandardLCG;
using PokemonStandardLibrary;
using PokemonStandardLibrary.CommonExtension;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch;

public class Form1 : Form
{
	private class CalcEggPIDParam
	{
		internal bool OnlyShiny;

		internal bool CheckNature;

		internal bool CheckAbility;

		internal bool CheckGender;

		internal Nature[] TargetNatureList;

		internal string TargetAbility;

		internal string TargetGender;

		internal uint TSV;

		internal uint MinDiff;

		internal uint MaxDiff;

		internal bool ListMode;

		internal Pokemon.Species Species;

		internal bool Check(RNGResult<uint?, uint> res)
		{
			if (ListMode)
			{
				return true;
			}
			if (!res.Content.HasValue)
			{
				return false;
			}
			uint value = res.Content.Value;
			if (CheckAbility && Species.Ability[(int)(value & 1)] != TargetAbility)
			{
				return false;
			}
			if (CheckGender && CommonFunctions.GetGender(value & 0xFFu, Species.GenderRatio).ToSymbol() != TargetGender)
			{
				return false;
			}
			if (OnlyShiny && !CommonFunctions.GetShinyType((value & 0xFFFFu) ^ (value >> 16), TSV, 8u).IsShiny())
			{
				return false;
			}
			if (CheckNature && !TargetNatureList.Contains((Nature)(value % 25)))
			{
				return false;
			}
			return true;
		}
	}

	private class CalcEggIVsParam
	{
		internal uint[] LowestIVs;

		internal uint[] HighestIVs;

		internal uint[] TargetStats;

		internal PokeType TargetHiddenPowerType;

		internal uint LowestHiddenPower;

		internal bool ListMode;

		internal bool ForIVs;

		internal bool ForStats;

		internal bool CheckHP;

		internal bool Check(RNGResult<Pokemon.Individual, (int Stat, int Parent)[]> res)
		{
			if (ListMode)
			{
				return true;
			}
			if (ForIVs && !res.Content.IVs.Select((uint iv, int idx) => (iv, idx)).All<(uint, int)>(((uint iv, int idx) _) => LowestIVs[_.idx] <= _.iv && _.iv <= HighestIVs[_.idx]))
			{
				return false;
			}
			if (ForStats && !res.Content.Stats.Select((uint st, int idx) => (st, idx)).All<(uint, int)>(((uint st, int idx) _) => TargetStats[_.idx] == _.st))
			{
				return false;
			}
			if (CheckHP && res.Content.HiddenPowerType != TargetHiddenPowerType)
			{
				return false;
			}
			if (res.Content.HiddenPower < LowestHiddenPower)
			{
				return false;
			}
			return true;
		}
	}

	private class CalcIDParam
	{
		internal uint TargetTID;

		internal uint TargetSID;

		internal bool ExistTargetTID;

		internal bool ExistTargetSID;

		internal List<uint> PIDList;

		internal bool OnlyShiny;

		internal bool ListMode;

		internal CalcIDParam()
		{
			ListMode = true;
		}

		internal CalcIDParam(uint TargetTID, uint TargetSID, bool ExistTargetTID, bool ExistTargetSID, bool OnlyShiny, List<uint> PIDList)
		{
			this.TargetTID = TargetTID;
			this.TargetSID = TargetSID;
			this.ExistTargetTID = ExistTargetTID;
			this.ExistTargetSID = ExistTargetSID;
			this.OnlyShiny = OnlyShiny;
			this.PIDList = PIDList;
			ListMode = false;
		}
	}

	private class WildCalcParam
	{
		internal uint TSV;

		internal string TargetPokemon;

		internal string TargetAbility;

		internal string TargetGender;

		internal uint TargetLv;

		internal uint[] LowestIVs;

		internal uint[] HighestIVs;

		internal uint[] TargetStats;

		internal Nature[] TargetNatureList;

		internal PokeType TargetHiddenPowerType;

		internal uint LowestHiddenPower;

		internal bool OnlyShiny;

		internal bool ListMode;

		internal bool ForIVs;

		internal bool ForStats;

		internal bool CheckNature;

		internal bool CheckLv;

		internal bool CheckPokemon;

		internal bool CheckAbility;

		internal bool CheckGender;

		internal bool CheckHP;

		internal bool Check(RNGResult<Pokemon.Individual, uint> res)
		{
			if (res.Content == null)
			{
				return false;
			}
			if (ListMode)
			{
				return true;
			}
			if (CheckLv && res.Content.Lv != TargetLv)
			{
				return false;
			}
			if (CheckPokemon && res.Content.Species.GetDefaultName() != TargetPokemon)
			{
				return false;
			}
			if (CheckAbility && res.Content.Ability != TargetAbility)
			{
				return false;
			}
			if (CheckGender && res.Content.Gender.ToSymbol() != TargetGender)
			{
				return false;
			}
			if (OnlyShiny && !res.Content.GetShinyType(TSV).IsShiny())
			{
				return false;
			}
			if (ForIVs && !res.Content.IVs.Select((uint iv, int idx) => (iv, idx)).All<(uint, int)>(((uint iv, int idx) _) => LowestIVs[_.idx] <= _.iv && _.iv <= HighestIVs[_.idx]))
			{
				return false;
			}
			if (ForStats && !res.Content.Stats.Select((uint st, int idx) => (st, idx)).All<(uint, int)>(((uint st, int idx) _) => TargetStats[_.idx] == _.st))
			{
				return false;
			}
			if (CheckNature && !TargetNatureList.Any((Nature _) => _ == res.Content.Nature))
			{
				return false;
			}
			if (CheckHP && res.Content.HiddenPowerType != TargetHiddenPowerType)
			{
				return false;
			}
			if (res.Content.HiddenPower < LowestHiddenPower)
			{
				return false;
			}
			return true;
		}
	}

	private readonly RadioButton[] RomButtons_back;

	private readonly RadioButton[] EncounterTypeButtons_back;

	private readonly SearchTab EggPIDTab;

	private readonly DataGridViewWrapper<EggPIDBinder> _eggPIDDGV;

	private readonly SearchTab EggIVsTab;

	private readonly DataGridViewWrapper<EggIVsBinder> _eggIVsDGV;

	private List<Pokemon.Species> HatchablePokemonList;

	private string[] natures = new string[25]
	{
		"がんばりや", "さみしがり", "ゆうかん", "いじっぱり", "やんちゃ", "ずぶとい", "すなお", "のんき", "わんぱく", "のうてんき",
		"おくびょう", "せっかち", "まじめ", "ようき", "むじゃき", "ひかえめ", "おっとり", "れいせい", "てれや", "うっかりや",
		"おだやか", "おとなしい", "なまいき", "しんちょう", "きまぐれ"
	};

	private Dictionary<string, Nature> Natures = new Dictionary<string, Nature>();

	private string[] natures_jp = new string[25]
	{
		"がんばりや", "さみしがり", "ゆうかん", "いじっぱり", "やんちゃ", "ずぶとい", "すなお", "のんき", "わんぱく", "のうてんき",
		"おくびょう", "せっかち", "まじめ", "ようき", "むじゃき", "ひかえめ", "おっとり", "れいせい", "てれや", "うっかりや",
		"おだやか", "おとなしい", "なまいき", "しんちょう", "きまぐれ"
	};

	private string[] natures_en = new string[25]
	{
		"Hardy", "Lonely", "Brave", "Adamant", "Naughty", "Bold", "Docile", "Relaxed", "Impish", "Lax",
		"Timid", "Hasty", "Serious", "Jolly", "Naive", "Modest", "Mild", "Quiet", "Bashful", "Rash",
		"Calm", "Gentle", "Sassy", "Careful", "Quirky"
	};

	private string[] natures_ge = new string[25]
	{
		"Robust", "Solo", "Mutig", "Hart", "Frech", "Kühn", "Sanft", "Locker", "Pfiffig", "Lasch",
		"Scheu", "Hastig", "Ernst", "Froh", "Niav", "Mäßig", "Mild", "Ruhig", "Zaghaft", "Hitzig",
		"Still", "Zart", "Forsch", "Sacht", "Kauzig"
	};

	private string[] natures_fr = new string[25]
	{
		"Hardi", "Solo", "Brave", "Rigide", "Mauvais", "Assuré", "Docile", "Relax", "Malin", "Lâche",
		"Timide", "Présseé", "Sérieux", "Jovial", "Naif", "Modeste", "Doux", "Discret", "Pudique", "Foufou",
		"Calme", "Gentil", "Malpoli", "Prudent", "Bizarre"
	};

	private string[] natures_ita = new string[25]
	{
		"Ardita", "Schiva", "Audace", "Decisa", "Birbona", "Sicura", "Docile", "Placida", "Scaltra", "Fiacca",
		"Timida", "Lesta", "Seria", "Allegra", "Ingenua", "Modesta", "Mite", "Quieta", "Ritrosa", "Ardente",
		"Calma", "Gentile", "Vivace", "Cauta", "Furba"
	};

	private string[] natures_spa = new string[25]
	{
		"Fuerte", "Huraña", "Audaz", "Firme", "Pícara", "Osada", "Dócil", "Plácida", "Agitada", "Floja",
		"Miedosa", "Activa", "Seria", "Alegre", "Ingenua", "Modesta", "Afable", "Mansa", "Tímida", "Alocada",
		"Serena", "Amable", "Grosera", "Cauta", "Rara"
	};

	private object[,] nature_table = new object[26, 2]
	{
		{ 25, "指定なし" },
		{ 3, "いじっぱり" },
		{ 19, "うっかりや" },
		{ 10, "おくびょう" },
		{ 20, "おだやか" },
		{ 16, "おっとり" },
		{ 21, "おとなしい" },
		{ 0, "がんばりや" },
		{ 24, "きまぐれ" },
		{ 1, "さみしがり" },
		{ 23, "しんちょう" },
		{ 6, "すなお" },
		{ 5, "ずぶとい" },
		{ 11, "せっかち" },
		{ 18, "てれや" },
		{ 22, "なまいき" },
		{ 9, "のうてんき" },
		{ 7, "のんき" },
		{ 15, "ひかえめ" },
		{ 12, "まじめ" },
		{ 14, "むじゃき" },
		{ 4, "やんちゃ" },
		{ 2, "ゆうかん" },
		{ 13, "ようき" },
		{ 17, "れいせい" },
		{ 8, "わんぱく" }
	};

	private object[,] nature_table_jp = new object[26, 2]
	{
		{ 25, "指定なし" },
		{ 3, "いじっぱり" },
		{ 19, "うっかりや" },
		{ 10, "おくびょう" },
		{ 20, "おだやか" },
		{ 16, "おっとり" },
		{ 21, "おとなしい" },
		{ 0, "がんばりや" },
		{ 24, "きまぐれ" },
		{ 1, "さみしがり" },
		{ 23, "しんちょう" },
		{ 6, "すなお" },
		{ 5, "ずぶとい" },
		{ 11, "せっかち" },
		{ 18, "てれや" },
		{ 22, "なまいき" },
		{ 9, "のうてんき" },
		{ 7, "のんき" },
		{ 15, "ひかえめ" },
		{ 12, "まじめ" },
		{ 14, "むじゃき" },
		{ 4, "やんちゃ" },
		{ 2, "ゆうかん" },
		{ 13, "ようき" },
		{ 17, "れいせい" },
		{ 8, "わんぱく" }
	};

	private object[,] nature_table_en = new object[26, 2]
	{
		{ 25, "指定なし" },
		{ 3, "Adamant" },
		{ 18, "Bashful" },
		{ 5, "Bold" },
		{ 2, "Brave" },
		{ 20, "Calm" },
		{ 23, "Careful" },
		{ 6, "Docile" },
		{ 21, "Gentle" },
		{ 0, "Hardy" },
		{ 11, "Hasty" },
		{ 8, "Impish" },
		{ 13, "Jolly" },
		{ 9, "Lax" },
		{ 1, "Lonely" },
		{ 16, "Mild" },
		{ 15, "Modest" },
		{ 14, "Naive" },
		{ 4, "Naughty" },
		{ 17, "Quiet" },
		{ 24, "Quirky" },
		{ 19, "Rash" },
		{ 7, "Relaxed" },
		{ 22, "Sassy" },
		{ 12, "Serious" },
		{ 10, "Timid" }
	};

	private object[,] nature_table_ge = new object[26, 2]
	{
		{ 25, "指定なし" },
		{ 12, "Ernst" },
		{ 22, "Forsch" },
		{ 4, "Frech" },
		{ 13, "Froh" },
		{ 3, "Hart" },
		{ 11, "Hastig" },
		{ 19, "Hitzig" },
		{ 24, "Kauzig" },
		{ 5, "Kühn" },
		{ 9, "Lasch" },
		{ 7, "Locker" },
		{ 15, "Mäßig" },
		{ 16, "Mild" },
		{ 2, "Mutig" },
		{ 14, "Niav" },
		{ 8, "Pfiffig" },
		{ 0, "Robust" },
		{ 17, "Ruhig" },
		{ 23, "Sacht" },
		{ 6, "Sanft" },
		{ 10, "Scheu" },
		{ 1, "Solo" },
		{ 20, "Still" },
		{ 18, "Zaghaft" },
		{ 21, "Zart" }
	};

	private object[,] nature_table_fr = new object[26, 2]
	{
		{ 25, "指定なし" },
		{ 5, "Assuré" },
		{ 24, "Bizarre" },
		{ 2, "Brave" },
		{ 20, "Calme" },
		{ 17, "Discret" },
		{ 6, "Docile" },
		{ 16, "Doux" },
		{ 19, "Foufou" },
		{ 21, "Gentil" },
		{ 0, "Hardi" },
		{ 13, "Jovial" },
		{ 9, "Lâche" },
		{ 8, "Malin" },
		{ 22, "Malpoli" },
		{ 4, "Mauvais" },
		{ 15, "Modeste" },
		{ 14, "Naif" },
		{ 11, "Présseé" },
		{ 23, "Prudent" },
		{ 18, "Pudique" },
		{ 7, "Relax" },
		{ 3, "Rigide" },
		{ 12, "Sérieux" },
		{ 1, "Solo" },
		{ 10, "Timide" }
	};

	private object[,] nature_table_ita = new object[26, 2]
	{
		{ 25, "指定なし" },
		{ 13, "Allegra" },
		{ 19, "Ardente" },
		{ 0, "Ardita" },
		{ 2, "Audace" },
		{ 4, "Birbona" },
		{ 20, "Calma" },
		{ 23, "Cauta" },
		{ 3, "Decisa" },
		{ 6, "Docile" },
		{ 9, "Fiacca" },
		{ 24, "Furba" },
		{ 21, "Gentile" },
		{ 14, "Ingenua" },
		{ 11, "Lesta" },
		{ 16, "Mite" },
		{ 15, "Modesta" },
		{ 7, "Placida" },
		{ 17, "Quieta" },
		{ 18, "Ritrosa" },
		{ 8, "Scaltra" },
		{ 1, "Schiva" },
		{ 12, "Seria" },
		{ 5, "Sicura" },
		{ 10, "Timida" },
		{ 22, "Vivace" }
	};

	private object[,] nature_table_spa = new object[26, 2]
	{
		{ 25, "指定なし" },
		{ 11, "Activa" },
		{ 16, "Afable" },
		{ 8, "Agitada" },
		{ 13, "Alegre" },
		{ 19, "Alocada" },
		{ 21, "Amable" },
		{ 2, "Audaz" },
		{ 23, "Cauta" },
		{ 6, "Dócil" },
		{ 3, "Firme" },
		{ 9, "Floja" },
		{ 0, "Fuerte" },
		{ 22, "Grosera" },
		{ 1, "Huraña" },
		{ 14, "Ingenua" },
		{ 17, "Mansa" },
		{ 10, "Miedosa" },
		{ 15, "Modesta" },
		{ 5, "Osada" },
		{ 4, "Pícara" },
		{ 7, "Plácida" },
		{ 24, "Rara" },
		{ 20, "Serena" },
		{ 12, "Seria" },
		{ 18, "Tímida" }
	};

	private CalcInitialSeedForm subform1;

	private IVsCalcBackForm subform2;

	private CalcSIDForm subform3;

	private CalcSVForm subform4;

	private FindPaintSeedForm subform5;

	private FindInitialSeedForm subform6;

	private IContainer components;

	private ContextMenuStrip contextMenuStrip1;

	private ToolStripMenuItem copyToolStripMenuItem;

	private ToolStripMenuItem SelectAllToolStripMenuItem;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;

	private Label label68;

	private Label label67;

	private TextBox textBox35;

	private Label label66;

	private TextBox textBox34;

	private Label label65;

	private TextBox textBox33;

	private Label label64;

	private TextBox textBox32;

	private Label label63;

	private TextBox textBox31;

	private Label label62;

	private TextBox textBox30;

	private Label label61;

	private TextBox textBox29;

	private Label label60;

	private TextBox textBox28;

	private Label label59;

	private TextBox textBox27;

	private Label label58;

	private TextBox textBox26;

	private Label label57;

	private TextBox textBox25;

	private Label label56;

	private TextBox textBox24;

	private Label label55;

	private Label label54;

	private ComboBox comboBox6;

	private TextBox textBox23;

	private Label label53;

	private Label label26;

	private Label label25;

	private Label label24;

	private Label label23;

	private Label label22;

	private ComboBox comboBox5;

	private Label label21;

	private Label label20;

	private ComboBox comboBox4;

	private Label label19;

	private TextBox textBox22;

	private TextBox textBox21;

	private TextBox textBox20;

	private TextBox textBox19;

	private TextBox textBox18;

	private TextBox textBox17;

	private CheckBox checkBox6;

	private CheckBox checkBox5;

	private CheckBox checkBox4;

	private CheckBox checkBox3;

	private TextBox textBox16;

	private TextBox textBox15;

	private Label label18;

	private Label label17;

	private TextBox textBox14;

	private TextBox textBox13;

	private Label label16;

	private Label label15;

	private Label label14;

	private Button button1;

	private Label label13;

	private RadioButton radioButton6;

	private TextBox textBox12;

	private RadioButton radioButton5;

	private TextBox textBox11;

	private Label label12;

	private TextBox textBox10;

	private Label label11;

	private RadioButton radioButton4;

	private TextBox textBox9;

	private CheckBox checkBox2;

	private Label label8;

	private Label label7;

	private TextBox textBox8;

	private TextBox textBox7;

	private RadioButton radioButton7;

	private RadioButton radioButton8;

	private RadioButton radioButton9;

	private RadioButton radioButton10;

	private RadioButton radioButton11;

	private ComboBox comboBox7;

	private TextBox textBox60;

	private Label label102;

	private Label label101;

	private Label label100;

	private Label label99;

	private Label label98;

	private Label label97;

	private ComboBox comboBox8;

	private Label label96;

	private Label label95;

	private ComboBox comboBox3;

	private Label label94;

	private TextBox textBox59;

	private TextBox textBox58;

	private TextBox textBox57;

	private TextBox textBox56;

	private TextBox textBox55;

	private TextBox textBox54;

	private CheckBox checkBox7;

	private TextBox textBox53;

	private TextBox textBox52;

	private Label label93;

	private Label label92;

	private TextBox textBox51;

	private TextBox textBox50;

	private Label label91;

	private Label label90;

	private Label label89;

	private Button button4;

	private Label label88;

	private RadioButton radioButton3;

	private TextBox textBox49;

	private RadioButton radioButton2;

	private TextBox textBox48;

	private Label label87;

	private TextBox textBox47;

	private Label label86;

	private RadioButton radioButton1;

	private TextBox textBox46;

	private CheckBox checkBox8;

	private Label label85;

	private Label label84;

	private TextBox textBox45;

	private TextBox textBox44;

	private ComboBox comboBox2;

	private Label label83;

	private Label label82;

	private TextBox textBox43;

	private Label label81;

	private TextBox textBox42;

	private Label label80;

	private TextBox textBox41;

	private Label label79;

	private TextBox textBox40;

	private Label label78;

	private TextBox textBox39;

	private Label label77;

	private TextBox textBox38;

	private Label label76;

	private TextBox textBox37;

	private Label label75;

	private TextBox textBox36;

	private Label label74;

	private TextBox textBox6;

	private Label label73;

	private TextBox textBox5;

	private Label label72;

	private TextBox textBox4;

	private Label label71;

	private TextBox textBox1;

	private Label label70;

	private Label label69;

	private ComboBox comboBox1;

	private CheckBox checkBox1;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn29;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn28;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn27;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn26;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn25;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn24;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn23;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn22;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn21;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn20;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn19;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn18;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn17;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn16;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn15;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn14;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn13;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn12;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;

	private ContextMenuStrip contextMenuStrip2;

	private ToolStripMenuItem copyToolStripMenuItem2;

	private ToolStripMenuItem SelectAllToolStripMenuItem2;

	private ContextMenuStrip contextMenuStrip3;

	private ToolStripMenuItem copyToolStripMenuItem3;

	private ToolStripMenuItem SelectAllToolStripMenuItem3;

	private ContextMenuStrip contextMenuStrip4;

	private ToolStripMenuItem copyToolStripMenuItem4;

	private ToolStripMenuItem SelectAllToolStripMenuItem4;

	private ContextMenuStrip contextMenuStrip5;

	private ToolStripMenuItem copyToolStripMenuItem5;

	private ToolStripMenuItem SelectAllToolStripMenuItem5;

	private TabPage TabPage_Egg;

	private TabControl tabControl2;

	private TabPage TabPage_EggPID;

	private GroupBox es_groupBox2;

	private RadioButton calibration_value3;

	private Label label155;

	private RadioButton calibration_value1;

	private RadioButton calibration_value2;

	private Button Cancel_EggPID;

	private Button Search_EggPID;

	private DataGridView es_dataGridView;

	private TabPage TabPage_EggIVs;

	private Button ek_cancel;

	private Button ek_listup;

	private CheckBox Method1;

	private CheckBox Method2;

	private CheckBox Method3;

	private GroupBox ek_groupBox1;

	private Label label199;

	private Label label201;

	private DataGridView ek_dataGridView;

	private Button ek_start;

	private TabPage TabPage_ID;

	private GroupBox ID_groupBox2;

	private Button ID_cancel;

	private Button ID_listup;

	private Button ID_start;

	private DataGridView ID_dataGridView;

	private TabPage TabPage_wild;

	private Button y_cancel;

	private DataGridView dataGridView_table;

	private DataGridViewTextBoxColumn Column22;

	private DataGridViewTextBoxColumn Column23;

	private DataGridViewTextBoxColumn Column24;

	private DataGridViewTextBoxColumn Column25;

	private DataGridViewTextBoxColumn Column26;

	private DataGridViewTextBoxColumn Column27;

	private DataGridViewTextBoxColumn Column28;

	private DataGridViewTextBoxColumn Column29;

	private DataGridViewTextBoxColumn Column30;

	private DataGridViewTextBoxColumn Column31;

	private DataGridViewTextBoxColumn Column32;

	private DataGridViewTextBoxColumn Column33;

	private GroupBox groupBox8;

	private RadioButton Encounter_OldRod;

	private RadioButton Encounter_GoodRod;

	private RadioButton Encounter_SuperRod;

	private RadioButton Encounter_RockSmash;

	private RadioButton Encounter_Surf;

	private RadioButton Encounter_Grass;

	private GroupBox groupBox7;

	private RadioButton ROM_FR;

	private RadioButton ROM_Em;

	private RadioButton ROM_R;

	private Button y_listup;

	private Button y_start;

	private GroupBox groupBox9;

	private ComboBox MapBox;

	private DataGridView y_dataGridView;

	private TabPage TabPage_stationary;

	private Button k_cancel;

	private Button k_listup;

	private Button k_start;

	private DataGridView k_dataGridView;

	private TabControl tabControl1;

	private GroupBox k_groupBox2;

	private RadioButton k_search2;

	private CheckBox k_shiny;

	private Label label6;

	private Label label27;

	private Label label29;

	private NumericUpDown k_mezapaPower;

	private RadioButton k_search1;

	private Label label31;

	private ComboBox k_pokedex;

	private Button NatureButton_stationary;

	private ComboBox k_mezapaType;

	private Label label33;

	private NumericUpDown k_Lv;

	private Label label36;

	private GroupBox k_groupBox1;

	private Label label44;

	private NumericUpDown k_RSmax;

	private NumericUpDown k_RSmin;

	private ComboBox Method_stationary;

	private TextBox k_Initialseed3;

	private RadioButton ForMultipleSeed_stationary;

	private Label label3;

	private RadioButton ForRTC_stationary;

	private TextBox k_Initialseed1;

	private RadioButton ForSimpleSeed_stationary;

	private Label label1;

	private Button SetFrameButton_stationary;

	private Label label52;

	private Label label50;

	private Label label51;

	private Label label40;

	private Label label39;

	private Label label2;

	private GroupBox y_groupBox2;

	private NumericUpDown y_stats6;

	private NumericUpDown y_stats5;

	private NumericUpDown y_stats4;

	private NumericUpDown y_stats3;

	private NumericUpDown y_stats2;

	private NumericUpDown y_stats1;

	private RadioButton y_search2;

	private Label label43;

	private Label label45;

	private RadioButton y_search1;

	private Label label47;

	private Label label48;

	private Label label103;

	private Label label105;

	private ComboBox y_pokedex;

	private Button NatureButton_wild;

	private Label label107;

	private Label label108;

	private NumericUpDown y_Lv;

	private NumericUpDown y_IVup6;

	private NumericUpDown y_IVlow1;

	private Label label109;

	private NumericUpDown y_IVlow2;

	private NumericUpDown y_IVlow3;

	private NumericUpDown y_IVup5;

	private NumericUpDown y_IVlow4;

	private Label label110;

	private NumericUpDown y_IVlow5;

	private Label label111;

	private NumericUpDown y_IVlow6;

	private NumericUpDown y_IVup4;

	private Label label112;

	private Label label113;

	private NumericUpDown y_IVup1;

	private NumericUpDown y_IVup3;

	private Label label114;

	private Label label115;

	private NumericUpDown y_IVup2;

	private GroupBox y_groupBox1;

	private Label label116;

	private Label label117;

	private NumericUpDown y_RSmax;

	private NumericUpDown y_RSmin;

	private ComboBox Method_wild;

	private TextBox y_Initialseed3;

	private RadioButton ForMultipleSeed_wild;

	private Label label118;

	private RadioButton ForRTC_wild;

	private TextBox y_Initialseed1;

	private RadioButton ForSimpleSeed_wild;

	private Label label119;

	private Label label121;

	private Label label123;

	private Button y_table_display;

	private CheckBox y_shiny;

	private ComboBox y_sex;

	private Label label46;

	private Label label49;

	private ComboBox y_ability;

	private Label label104;

	private Label label125;

	private Label label126;

	private NumericUpDown y_mezapaPower;

	private Label label127;

	private ComboBox y_mezapaType;

	private ComboBox SyncNature_wild;

	private ComboBox FieldAbility_wild;

	private GroupBox y_groupBox3;

	private Label label106;

	private CheckBox y_check_LvEnable;

	private TabPage TabPage_other;

	private GroupBox groupBox4;

	private Button L_button;

	private ComboBox Language;

	private Button poyo;

	private GroupBox ID_groupBox1;

	private Label label129;

	private NumericUpDown ID_RSmax;

	private NumericUpDown ID_RSmin;

	private Label label130;

	private RadioButton IDInitialseed2;

	private TextBox ID_Initialseed1;

	private RadioButton IDInitialseed1;

	private Label label131;

	private Label label133;

	private Label label135;

	private CheckBox ID_shiny;

	private TextBox ID_PID;

	private GroupBox es_groupBox1;

	private Label label138;

	private Label label145;

	private Label label142;

	private ComboBox es_pokedex;

	private ComboBox es_sex;

	private Label label144;

	private ComboBox es_ability;

	private Label label147;

	private Label label140;

	private Label label141;

	private ComboBox EverstoneNature_EggPID;

	private CheckBox EverstoneCheck;

	private CheckBox OnlyShiny_EggPID;

	private GroupBox ek_groupBox3;

	private NumericUpDown ek_stats6;

	private NumericUpDown ek_stats5;

	private NumericUpDown ek_stats3;

	private NumericUpDown ek_stats4;

	private NumericUpDown ek_stats2;

	private NumericUpDown ek_stats1;

	private Label label156;

	private NumericUpDown ek_mezapaPower;

	private Label label157;

	private ComboBox ek_mezapaType;

	private RadioButton ek_search2;

	private Label label158;

	private Label label159;

	private RadioButton ek_search1;

	private Label label162;

	private Label label164;

	private Label label165;

	private Label label166;

	private ComboBox ek_pokedex;

	private Label label167;

	private Label label168;

	private NumericUpDown ek_IVup6;

	private NumericUpDown ek_IVlow1;

	private Label label169;

	private NumericUpDown ek_IVlow2;

	private ComboBox ek_nature;

	private NumericUpDown ek_IVlow3;

	private NumericUpDown ek_IVup5;

	private NumericUpDown ek_IVlow4;

	private Label label170;

	private NumericUpDown ek_IVlow5;

	private Label label171;

	private NumericUpDown ek_IVlow6;

	private NumericUpDown ek_IVup4;

	private Label label172;

	private Label label173;

	private NumericUpDown ek_IVup1;

	private NumericUpDown ek_IVup3;

	private Label label174;

	private Label label175;

	private NumericUpDown ek_IVup2;

	private NumericUpDown ek_Lv;

	private GroupBox ek_groupBox2;

	private Label label160;

	private Label label163;

	private Label label177;

	private NumericUpDown pre_parent6;

	private Label label178;

	private NumericUpDown pre_parent5;

	private Label label179;

	private NumericUpDown pre_parent4;

	private Label label153;

	private NumericUpDown pre_parent3;

	private Label label152;

	private NumericUpDown pre_parent2;

	private NumericUpDown post_parent6;

	private NumericUpDown post_parent5;

	private NumericUpDown post_parent4;

	private NumericUpDown post_parent3;

	private NumericUpDown post_parent2;

	private NumericUpDown post_parent1;

	private NumericUpDown pre_parent1;

	private Label label150;

	private NumericUpDown FirstFrame_stationary;

	private NumericUpDown FrameRange_stationary;

	private NumericUpDown TargetFrame_stationary;

	private NumericUpDown LastFrame_stationary;

	private NumericUpDown FrameRange_wild;

	private NumericUpDown TargetFrame_wild;

	private NumericUpDown LastFrame_wild;

	private NumericUpDown FirstFrame_wild;

	private Button SetFrameButton_wild;

	private Label label180;

	private Label label181;

	private Label label182;

	private NumericUpDown DiffMax;

	private NumericUpDown DiffMin;

	private Label label122;

	private NumericUpDown LastFrame_EggPID;

	private NumericUpDown FirstFrame_EggPID;

	private Label label120;

	private NumericUpDown TID_stationary;

	private NumericUpDown SID_stationary;

	private NumericUpDown SID_EggPID;

	private NumericUpDown TID_EggPID;

	private NumericUpDown FrameRange_EggIVs;

	private NumericUpDown TargetFrame_EggIVs;

	private NumericUpDown LastFrame_EggIVs;

	private NumericUpDown FirstFrame_EggIVs;

	private Button SetFrameButton_EggIVs;

	private Label label124;

	private Label label139;

	private Label label146;

	private NumericUpDown FrameRange_ID;

	private NumericUpDown TargetFrame_ID;

	private NumericUpDown LastFrame_ID;

	private NumericUpDown FirstFrame_ID;

	private Button SetFrameButton_ID;

	private Label label132;

	private Label label134;

	private Label label136;

	private NumericUpDown SID_ID;

	private CheckBox CheckSID;

	private CheckBox CheckTID;

	private NumericUpDown TID_ID;

	private Label label137;

	private Label label128;

	private NumericUpDown SID_wild;

	private NumericUpDown TID_wild;

	private NaturePanel NaturePanel_stationary;

	private NaturePanel NaturePanel_EggPID;

	private NaturePanel NaturePanel_wild;

	private ComboBox CCGender_wild;

	private CheckBox RSFLRoamingCheck;

	private Label label_pb;

	private ComboBox PokeBlockBox;

	private TabPage TabPage_backword;

	private DataGridView dataGridView1_back;

	private Label DataCount_back;

	private Label label154;

	private Label label161;

	private Label label176;

	private Label label183;

	private Label label184;

	private Label label185;

	private NumericUpDown Smax_back;

	private NumericUpDown Hmin_back;

	private Label label186;

	private NumericUpDown Amin_back;

	private NumericUpDown Bmin_back;

	private NumericUpDown Dmax_back;

	private NumericUpDown Cmin_back;

	private Label label187;

	private NumericUpDown Dmin_back;

	private NumericUpDown Smin_back;

	private NumericUpDown Cmax_back;

	private Label label188;

	private Label label189;

	private NumericUpDown Hmax_back;

	private NumericUpDown Bmax_back;

	private Label label190;

	private Label label191;

	private NumericUpDown Amax_back;

	private ComboBox Map_back;

	private Button CalcButton_back;

	private RadioButton Stationary_back;

	private GroupBox groupBox2;

	private RadioButton Wild_back;

	private Panel panel1;

	private Label label30;

	private Label label42;

	private NumericUpDown k_stats1;

	private NumericUpDown k_IVup3;

	private NumericUpDown k_stats5;

	private NumericUpDown k_IVup1;

	private NumericUpDown k_stats6;

	private Label label38;

	private NumericUpDown k_stats4;

	private Label label37;

	private NumericUpDown k_stats3;

	private NumericUpDown k_IVup4;

	private NumericUpDown k_stats2;

	private NumericUpDown k_IVlow6;

	private NumericUpDown k_IVlow5;

	private Label label35;

	private Label label4;

	private NumericUpDown k_IVlow4;

	private Label label5;

	private NumericUpDown k_IVup5;

	private NumericUpDown k_IVlow3;

	private NumericUpDown k_IVlow2;

	private Label label9;

	private Label label34;

	private Label label10;

	private NumericUpDown k_IVlow1;

	private NumericUpDown k_IVup6;

	private Label label28;

	private NumericUpDown k_IVup2;

	private Label label41;

	private GroupBox groupBox5;

	private RadioButton FR_stationary;

	private RadioButton Em_stationary;

	private RadioButton Ruby_stationary;

	private CheckBox CheckAppearing_wild;

	private ComboBox AbilityBox_stationary;

	private Label label192;

	private ComboBox GenderBox_stationary;

	private Label label32;

	private ContextMenuStrip contextMenuStrip6;

	private ToolStripMenuItem toolStripMenuItem1;

	private ToolStripMenuItem toolStripMenuItem2;

	private CheckBox OFUDA_wild;

	private CheckBox WhiteFlute_wild;

	private CheckBox BlackFlute_wild;

	private CheckBox RidingBicycle_wild;

	private GroupBox groupBox6;

	private Panel panel2;

	private CheckBox Method1_back;

	private CheckBox Method2_back;

	private CheckBox Method3_back;

	private GroupBox groupBox10;

	private RadioButton OldRod_back;

	private RadioButton GoodRod_back;

	private RadioButton SuperRod_back;

	private RadioButton RockSmash_back;

	private RadioButton Surf_back;

	private RadioButton Grass_back;

	private DataGridView EncounterTable_back;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn30;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn57;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn66;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn67;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn68;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn69;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn70;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn71;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn72;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn73;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn74;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn75;

	private GroupBox groupBox11;

	private RadioButton FR_back;

	private RadioButton Em_back;

	private RadioButton Ruby_back;

	private NumericUpDown SID_back;

	private NumericUpDown TID_back;

	private CheckBox OnlyShiny_back;

	private Label label194;

	private Label label196;

	private Label label197;

	private NumericUpDown HiddenPowerPower_back;

	private Label label198;

	private ComboBox HiddenPowerType_back;

	private Button NatureButton_back;

	private Label label200;

	private NaturePanel NaturePanel_back;

	private DataGridViewTextBoxColumn StartSeedColumn_back;

	private DataGridViewTextBoxColumn GenerateSeedColumn_back;

	private DataGridViewTextBoxColumn FieldAbilityColumn_back;

	private DataGridViewTextBoxColumn MethodColumn_back;

	private DataGridViewTextBoxColumn SlotColumn_back;

	private DataGridViewTextBoxColumn LvColumn_back;

	private DataGridViewTextBoxColumn PIDColumn_back;

	private DataGridViewTextBoxColumn NatureColumn_back;

	private DataGridViewTextBoxColumn HColumn_back;

	private DataGridViewTextBoxColumn AColumn_back;

	private DataGridViewTextBoxColumn BColumn_back;

	private DataGridViewTextBoxColumn CColumn_back;

	private DataGridViewTextBoxColumn DColumn_back;

	private DataGridViewTextBoxColumn SColumn_back;

	private DataGridViewTextBoxColumn GenderColumn_back;

	private DataGridViewTextBoxColumn AbilityColumn_back;

	private DataGridViewTextBoxColumn HiddenPowerColumn_back;

	private GroupBox groupBox1;

	private Button button3;

	private GroupBox groupBox3;

	private Button button2;

	private GroupBox groupBox13;

	private Button button5;

	private GroupBox groupBox14;

	private Button button6;

	private RadioButton ROM_LG;

	private RadioButton ROM_S;

	private RadioButton LG_back;

	private RadioButton Sapphire_back;

	private RadioButton LG_stationary;

	private RadioButton Sapphire_stationary;

	private NumericUpDown MinLvBox_back;

	private CheckBox checkLv_back;

	private CheckBox checkSpecies_back;

	private ComboBox PokemonBox_back;

	private Label label148;

	private NumericUpDown MaxLvBox_back;

	private GroupBox groupBox12;

	private Button button7;

	private GroupBox groupBox15;

	private Button button8;

	private Panel PaintPanel_wild;

	private Label label149;

	private NumericUpDown PaintSeedMaxFrameBox_wild;

	private NumericUpDown PaintSeedMinFrameBox_wild;

	private Label label143;

	private Label label151;

	private Panel PaintPanel_stationary;

	private Label label193;

	private NumericUpDown PaintSeedMaxFrameBox_stationary;

	private NumericUpDown PaintSeedMinFrameBox_stationary;

	private Label label195;

	private readonly SearchTab IDTab;

	private readonly DataGridViewWrapper<IDBinder> _idDGV;

	private readonly SearchTab StationaryTab;

	private readonly DataGridViewWrapper<StationaryBinder> _stationaryDGV;

	private Rom SelectedRom_stationary = Rom.Ruby;

	private readonly SearchTab _wildTab;

	private readonly DataGridViewWrapper<WildBinder> _wildDGV;

	private Rom SelectedRom_wild;

	private EncounterType SelectedEncounterType;

	private Rom SelectedRom_backword => Common.RomList[RomButtons_back.Select((RadioButton btn, int idx) => (btn, idx)).ToList().Find(((RadioButton btn, int idx) _) => _.btn.Checked)
		.Item2];

	private EncounterType SelectedEncounterType_back => (EncounterType)EncounterTypeButtons_back.Select((RadioButton btn, int idx) => (btn, idx)).ToList().Find(((RadioButton btn, int idx) _) => _.btn.Checked)
		.Item2;

	private GBAMap SelectedMap_backword => SelectedRom_backword.GetMap(SelectedEncounterType_back, Map_back.SelectedIndex);

	private IEnumerable<GenerateMethod> SelectedMethodList_backword
	{
		get
		{
			if (Method1_back.Checked)
			{
				yield return Common.methodList[0];
			}
			if (Method2_back.Checked)
			{
				yield return Common.methodList[1];
			}
			if (Method3_back.Checked)
			{
				yield return Common.methodList[2];
			}
		}
	}

	private IEnumerable<(uint H, uint A, uint B, uint C, uint D, uint S)> SelectedIVs
	{
		get
		{
			new List<(uint, uint)>();
			for (uint H = Util.GetValueDec(Hmin_back); H <= Util.GetValueDec(Hmax_back); H++)
			{
				for (uint A = Util.GetValueDec(Amin_back); A <= Util.GetValueDec(Amax_back); A++)
				{
					for (uint B = Util.GetValueDec(Bmin_back); B <= Util.GetValueDec(Bmax_back); B++)
					{
						for (uint C = Util.GetValueDec(Cmin_back); C <= Util.GetValueDec(Cmax_back); C++)
						{
							for (uint D = Util.GetValueDec(Dmin_back); D <= Util.GetValueDec(Dmax_back); D++)
							{
								for (uint S = Util.GetValueDec(Smin_back); S <= Util.GetValueDec(Smax_back); S++)
								{
									yield return (H, A, B, C, D, S);
								}
							}
						}
					}
				}
			}
		}
	}

	private PokeType SelectedHPType
	{
		get
		{
			if (HiddenPowerType_back.SelectedIndex != 0)
			{
				return HiddenPowerType_back.Text.KanjiToPokeType();
			}
			return PokeType.None;
		}
	}

	private uint[] SelectedPreParentIVs => new uint[6]
	{
		Util.GetValueDec(pre_parent1),
		Util.GetValueDec(pre_parent2),
		Util.GetValueDec(pre_parent3),
		Util.GetValueDec(pre_parent4),
		Util.GetValueDec(pre_parent5),
		Util.GetValueDec(pre_parent6)
	};

	private uint[] SelectedPostParentIVs => new uint[6]
	{
		Util.GetValueDec(post_parent1),
		Util.GetValueDec(post_parent2),
		Util.GetValueDec(post_parent3),
		Util.GetValueDec(post_parent4),
		Util.GetValueDec(post_parent5),
		Util.GetValueDec(post_parent6)
	};

	private IEnumerable<(string label, GenerateMethod method)> SelectedEggMethodList
	{
		get
		{
			if (Method1.Checked)
			{
				yield return ("Method1", StandardIVsGenerator.GetInstance());
			}
			if (Method2.Checked)
			{
				yield return ("Method2", MiddleInterruptedIVsGenerator.GetInstance());
			}
			if (Method3.Checked)
			{
				yield return ("Method3", PosteriorInterruptedIVsGenerator.GetInstance());
			}
		}
	}

	private Nature SelectedEverstoneNature
	{
		get
		{
			if (!EverstoneCheck.Checked)
			{
				return Nature.other;
			}
			return (Nature)EverstoneNature_EggPID.SelectedIndex;
		}
	}

	private AISHOU SelectedCompatibility
	{
		get
		{
			if (!calibration_value1.Checked)
			{
				if (!calibration_value2.Checked)
				{
					return AISHOU.YOSASOU;
				}
				return AISHOU.SOKOSOKO;
			}
			return AISHOU.ANMARI;
		}
	}

	private Pokemon.Species SelectedPokemon_back => HatchablePokemonList[es_pokedex.SelectedIndex];

	private GenerateMethod SelectedMethod_stationary => Common.methodList[Method_stationary.SelectedIndex];

	private GBAMap SelectedMap => SelectedRom_wild.GetMap(SelectedEncounterType, MapBox.SelectedIndex);

	private GenerateMethod SelectedMethod_wild => Common.methodList[Method_wild.SelectedIndex];

	private FieldAbility SelectedAbility => FieldAbility_wild.Text switch
	{
		"シンクロ" => FieldAbility.GetSynchronize((Nature)SyncNature_wild.SelectedIndex), 
		"プレッシャー" => FieldAbility.GetPressure(), 
		"メロボ" => FieldAbility.GetCuteCharm((Gender)CCGender_wild.SelectedIndex), 
		"せいでんき" => FieldAbility.GetStatic(), 
		"じりょく" => FieldAbility.GetMagnetPull(), 
		"あくしゅう" => FieldAbility.GetStench(), 
		"はっこう" => FieldAbility.GetIlluminate(), 
		_ => FieldAbility.GetOtherAbility(), 
	};

	private PokeBlock SelectedPokeBlock
	{
		get
		{
			if (!PokeBlockBox.Visible)
			{
				return PokeBlock.Plain;
			}
			return Common.pokeBlockList[PokeBlockBox.SelectedIndex];
		}
	}

	private void CalcButton_back_click(object sender, EventArgs e)
	{
		if (Hmin_back.Value > Hmax_back.Value)
		{
			ShowCaution("Hの個体値が 下限 ＞上限 になっています。");
			return;
		}
		if (Amin_back.Value > Amax_back.Value)
		{
			ShowCaution("Aの個体値が 下限 ＞上限 になっています。");
			return;
		}
		if (Bmin_back.Value > Bmax_back.Value)
		{
			ShowCaution("Bの個体値が 下限 ＞上限 になっています。");
			return;
		}
		if (Cmin_back.Value > Cmax_back.Value)
		{
			ShowCaution("Cの個体値が 下限 ＞上限 になっています。");
			return;
		}
		if (Dmin_back.Value > Dmax_back.Value)
		{
			ShowCaution("Dの個体値が 下限 ＞上限 になっています。");
			return;
		}
		if (Smin_back.Value > Smax_back.Value)
		{
			ShowCaution("Sの個体値が 下限 ＞上限 になっています。");
			return;
		}
		dataGridView1_back.ClearSelection();
		dataGridView1_back.Rows.Clear();
		if (Stationary_back.Checked)
		{
			SlotColumn_back.Visible = false;
			FieldAbilityColumn_back.Visible = false;
			LvColumn_back.Visible = false;
			AbilityColumn_back.Visible = false;
			GenderColumn_back.Visible = false;
			Calc_Backword_stationary();
		}
		else
		{
			SlotColumn_back.Visible = true;
			FieldAbilityColumn_back.Visible = true;
			LvColumn_back.Visible = true;
			AbilityColumn_back.Visible = true;
			GenderColumn_back.Visible = true;
			Calc_Backword_wild();
		}
	}

	private void Calc_Backword_stationary()
	{
		IEnumerable<GenerateMethod> selectedMethodList_backword = SelectedMethodList_backword;
		int num = 0;
		IEnumerable<(uint, uint, uint, uint, uint, uint)> selectedIVs = SelectedIVs;
		uint tSV = Util.GetValueDec(TID_back) ^ Util.GetValueDec(SID_back);
		PokeType selectedHPType = SelectedHPType;
		uint valueDec = Util.GetValueDec(HiddenPowerPower_back);
		var (source, flag) = NaturePanel_back.GetNatureList();
		foreach (GenerateMethod method in selectedMethodList_backword)
		{
			IEnumerable<uint> source2 = SelectedIVs.SelectMany(((uint H, uint A, uint B, uint C, uint D, uint S) _) => SeedFinder.FindGeneratingSeed(_.H, _.A, _.B, _.C, _.D, _.S, method.LegacyName == "Method4", method.LegacyName == "Method2"));
			StationaryGenerator generator = SelectedRom_backword.GetStationarySymbol(0).CreateGenerator(method);
			foreach (RNGResult<Pokemon.Individual, uint> item in source2.Select((uint _) => generator.Generate(_)))
			{
				if ((!OnlyShiny_back.Checked || item.Content.GetShinyType(tSV).IsShiny()) && (selectedHPType == PokeType.None || item.Content.HiddenPowerType == selectedHPType) && (!flag || source.Contains(item.Content.Nature)) && item.Content.HiddenPower >= valueDec)
				{
					DataGridViewRow dataGridViewRow = new DataGridViewRow();
					dataGridViewRow.CreateCells(dataGridView1_back);
					dataGridViewRow.SetValues($"{item.HeadSeed:X8}", $"{item.HeadSeed:X8}", "", method.LegacyName, "", "", $"{item.Content.PID:X8}", item.Content.Nature.ToJapanese() ?? "", item.Content.IVs[0], item.Content.IVs[1], item.Content.IVs[2], item.Content.IVs[3], item.Content.IVs[4], item.Content.IVs[5], item.Content.Gender.ToSymbol(), item.Content.Ability, $"{item.Content.HiddenPowerType.ToKanji()}{item.Content.HiddenPower}");
					dataGridView1_back.Rows.Add(dataGridViewRow);
					num++;
				}
			}
		}
		DataCount_back.Text = $"データ数:{num}";
	}

	private void Calc_Backword_wild()
	{
		IEnumerable<GenerateMethod> selectedMethodList_backword = SelectedMethodList_backword;
		int num = 0;
		IEnumerable<(uint, uint, uint, uint, uint, uint)> selectedIVs = SelectedIVs;
		uint tSV = Util.GetValueDec(TID_back) ^ Util.GetValueDec(SID_back);
		PokeType selectedHPType = SelectedHPType;
		uint valueDec = Util.GetValueDec(HiddenPowerPower_back);
		var (source, flag) = NaturePanel_back.GetNatureList();
		foreach (GenerateMethod method in selectedMethodList_backword)
		{
			IEnumerable<CalcBackResult> enumerable = SelectedIVs.SelectMany(((uint H, uint A, uint B, uint C, uint D, uint S) _) => SelectedMap_backword.FindGeneratingSeed(_.H, _.A, _.B, _.C, _.D, _.S, method.LegacyName == "Method4", method.LegacyName == "Method2"));
			foreach (CalcBackResult item in enumerable)
			{
				if ((!OnlyShiny_back.Checked || item.Individual.GetShinyType(tSV).IsShiny()) && (!checkSpecies_back.Checked || !(item.Individual.Species.GetDefaultName() != PokemonBox_back.Text)) && (!checkLv_back.Enabled || !checkLv_back.Checked || !((decimal)item.Individual.Lv < MinLvBox_back.Value)) && (!checkLv_back.Enabled || !checkLv_back.Checked || !((decimal)item.Individual.Lv > MaxLvBox_back.Value)) && (!flag || source.Contains(item.Individual.Nature)) && (selectedHPType == PokeType.None || item.Individual.HiddenPowerType == selectedHPType) && item.Individual.HiddenPower >= valueDec)
				{
					DataGridViewRow dataGridViewRow = new DataGridViewRow();
					dataGridViewRow.CreateCells(dataGridView1_back);
					dataGridViewRow.SetValues($"{item.Seed:X8}", $"{item.Key:X8}", item.Condition, item.Method ?? "", item.Individual.Species.GetDefaultName() ?? "", item.Individual.Lv, $"{item.Individual.PID:X8}", item.Individual.Nature.ToJapanese() ?? "", item.Individual.IVs[0], item.Individual.IVs[1], item.Individual.IVs[2], item.Individual.IVs[3], item.Individual.IVs[4], item.Individual.IVs[5], item.Individual.Gender.ToSymbol(), item.Individual.Ability, $"{item.Individual.HiddenPowerType.ToKanji()}{item.Individual.HiddenPower}");
					dataGridView1_back.Rows.Add(dataGridViewRow);
					num++;
				}
			}
		}
		DataCount_back.Text = $"データ数:{num}";
	}

	private void UpdateMapBox_back()
	{
		Map_back.Items.Clear();
		ComboBox.ObjectCollection items = Map_back.Items;
		object[] items2 = SelectedRom_backword.GetMapNameList(SelectedEncounterType_back).ToArray();
		items.AddRange(items2);
		Map_back.SelectedIndex = 0;
		EncounterTable_back.UpdateTable(SelectedMap_backword, SelectedEncounterType_back);
	}

	private void EncounterType_back_ChackedChanged(object sender, EventArgs e)
	{
		UpdateMapBox_back();
	}

	private void Rom_back_CheckedChanged(object sender, EventArgs e)
	{
		UpdateMapBox_back();
	}

	private void Method1_back_Click(object sender, EventArgs e)
	{
		Method1_back.Checked |= !Method2_back.Checked && !Method3_back.Checked;
	}

	private void Method2_back_Click(object sender, EventArgs e)
	{
		Method2_back.Checked |= !Method1_back.Checked && !Method3_back.Checked;
	}

	private void Method3_back_Click(object sender, EventArgs e)
	{
		Method3_back.Checked |= !Method1_back.Checked && !Method2_back.Checked;
	}

	private void Map_back_SelectedIndexChanged(object sender, EventArgs e)
	{
		EncounterTable_back.UpdateTable(SelectedMap_backword, SelectedEncounterType_back);
		IReadOnlyList<GBASlot> encounterTable = SelectedMap_backword.EncounterTable;
		string[] array = encounterTable.Select((GBASlot _) => _.Pokemon.GetDefaultName()).Distinct().ToArray();
		PokemonBox_back.Items.Clear();
		if (Map_back.Text.Contains("ヒンバス"))
		{
			PokemonBox_back.Items.Add("ヒンバス");
		}
		ComboBox.ObjectCollection items = PokemonBox_back.Items;
		object[] items2 = array;
		items.AddRange(items2);
		PokemonBox_back.SelectedIndex = 0;
	}

	private void NatureButton_back_Click(object sender, EventArgs e)
	{
		NaturePanel naturePanel_back = NaturePanel_back;
		naturePanel_back.Visible = !naturePanel_back.Visible;
	}

	private void PokemonBox_back_SelectedIndexChanged(object sender, EventArgs e)
	{
		decimal minimum;
		if (PokemonBox_back.Text == "ヒンバス")
		{
			NumericUpDown minLvBox_back = MinLvBox_back;
			minimum = (MaxLvBox_back.Minimum = 20m);
			minLvBox_back.Minimum = minimum;
			NumericUpDown minLvBox_back2 = MinLvBox_back;
			minimum = (MaxLvBox_back.Maximum = 25m);
			minLvBox_back2.Maximum = minimum;
			MinLvBox_back.Value = 20m;
			MaxLvBox_back.Value = 25m;
			return;
		}
		IReadOnlyList<GBASlot> encounterTable = SelectedMap_backword.EncounterTable;
		uint num3 = encounterTable.Where((GBASlot _) => _.Pokemon.GetDefaultName() == PokemonBox_back.Text).Min((GBASlot _) => _.BasicLv);
		uint num4 = encounterTable.Where((GBASlot _) => _.Pokemon.GetDefaultName() == PokemonBox_back.Text).Max((GBASlot _) => _.BasicLv + _.VariableLv - 1);
		NumericUpDown minLvBox_back3 = MinLvBox_back;
		minimum = (MaxLvBox_back.Minimum = num3);
		minLvBox_back3.Minimum = minimum;
		NumericUpDown minLvBox_back4 = MinLvBox_back;
		minimum = (MaxLvBox_back.Maximum = num4);
		minLvBox_back4.Maximum = minimum;
		MinLvBox_back.Value = num3;
		MaxLvBox_back.Value = num4;
	}

	private void CheckSpecies_back_CheckedChanged(object sender, EventArgs e)
	{
		checkLv_back.Enabled = checkSpecies_back.Checked;
		PokemonBox_back.Enabled = checkSpecies_back.Checked;
		NumericUpDown minLvBox_back = MinLvBox_back;
		bool enabled = (MaxLvBox_back.Enabled = checkLv_back.Enabled && checkLv_back.Checked);
		minLvBox_back.Enabled = enabled;
	}

	private void CheckLv_back_CheckedChanged(object sender, EventArgs e)
	{
		NumericUpDown minLvBox_back = MinLvBox_back;
		bool enabled = (MaxLvBox_back.Enabled = checkLv_back.Enabled && checkLv_back.Checked);
		minLvBox_back.Enabled = enabled;
	}

	private async void Search_EggPID_Click(object sender, EventArgs e)
	{
		if (Util.GetValueDec(FirstFrame_EggPID) > Util.GetValueDec(LastFrame_EggPID))
		{
			ShowCaution("Fが 下限 ＞上限 になっています。");
			return;
		}
		if (Util.GetValueDec(DiffMin) > Util.GetValueDec(DiffMax))
		{
			ShowCaution("差分が 下限 ＞上限 になっています。");
			return;
		}
		es_dataGridView.ClearSelection();
		Search_EggPID.Enabled = false;
		CalcEggPIDParam calcEggPIDParam = new CalcEggPIDParam
		{
			ListMode = false,
			MinDiff = Util.GetValueDec(DiffMin),
			MaxDiff = Util.GetValueDec(DiffMax),
			Species = SelectedPokemon_back
		};
		calcEggPIDParam.OnlyShiny = OnlyShiny_EggPID.Checked;
		calcEggPIDParam.TSV = Util.GetValueDec(TID_EggPID) ^ Util.GetValueDec(SID_EggPID);
		(calcEggPIDParam.TargetNatureList, calcEggPIDParam.CheckNature) = NaturePanel_EggPID.GetNatureList();
		if (calcEggPIDParam.CheckAbility = es_ability.SelectedIndex != 0)
		{
			calcEggPIDParam.TargetAbility = es_ability.Text;
		}
		if (calcEggPIDParam.CheckGender = es_sex.SelectedIndex != 0)
		{
			calcEggPIDParam.TargetGender = es_sex.Text;
		}
		Cancel_EggPID.Enabled = true;
		EggPIDTab.SearchWorker.isBusy = true;
		try
		{
			using (EggPIDTab.SearchWorker.newSource())
			{
				List<EggPIDBinder> data = await CalcEggPID(ProphaseEggGenerator.CreateInstance(SelectedCompatibility, SelectedEverstoneNature), EggPIDTab.GetParam(ListMode: false), calcEggPIDParam);
				_eggPIDDGV.SetData(data);
			}
		}
		catch
		{
		}
		EggPIDTab.SearchWorker.isBusy = false;
		es_dataGridView.CurrentCell = null;
		Cancel_EggPID.Enabled = false;
		Search_EggPID.Enabled = true;
	}

	private void Cancel_EggPID_Click(object sender, EventArgs e)
	{
		if (EggPIDTab.SearchWorker.isBusy)
		{
			EggPIDTab.SearchWorker.Cancel();
		}
	}

	private async void Search_EggIVs_Click(object sender, EventArgs e)
	{
		if (Util.GetValueDec(FirstFrame_EggIVs) > Util.GetValueDec(LastFrame_EggIVs))
		{
			ShowCaution("Fが 下限 ＞上限 になっています。");
			return;
		}
		if (Util.GetValueDec(ek_IVlow1) > Util.GetValueDec(ek_IVup1))
		{
			ShowCaution("Hの個体値が 下限 ＞上限 になっています。");
			return;
		}
		if (Util.GetValueDec(ek_IVlow2) > Util.GetValueDec(ek_IVup2))
		{
			ShowCaution("Aの個体値が 下限 ＞上限 になっています。");
			return;
		}
		if (Util.GetValueDec(ek_IVlow3) > Util.GetValueDec(ek_IVup3))
		{
			ShowCaution("Bの個体値が 下限 ＞上限 になっています。");
			return;
		}
		if (Util.GetValueDec(ek_IVlow4) > Util.GetValueDec(ek_IVup4))
		{
			ShowCaution("Cの個体値が 下限 ＞上限 になっています。");
			return;
		}
		if (Util.GetValueDec(ek_IVlow5) > Util.GetValueDec(ek_IVup5))
		{
			ShowCaution("Dの個体値が 下限 ＞上限 になっています。");
			return;
		}
		if (Util.GetValueDec(ek_IVlow6) > Util.GetValueDec(ek_IVup6))
		{
			ShowCaution("Sの個体値が 下限 ＞上限 になっています。");
			return;
		}
		IEnumerable<(string, GenerateMethod)> selectedEggMethodList = SelectedEggMethodList;
		if (selectedEggMethodList.Count() == 0)
		{
			ShowCaution("Methodが選択されていません。");
			return;
		}
		ek_dataGridView.ClearSelection();
		Button button = ek_start;
		bool enabled = (ek_listup.Enabled = false);
		button.Enabled = enabled;
		Pokemon.Species poke = HatchablePokemonList[ek_pokedex.SelectedIndex];
		uint pid = (uint)ek_nature.SelectedIndex;
		uint[] pre = SelectedPreParentIVs;
		uint[] post = SelectedPostParentIVs;
		IEnumerable<(string, EggGenerator)> generators = selectedEggMethodList.Select<(string, GenerateMethod), (string, EggGenerator)>(((string label, GenerateMethod method) _) => (_.label, EggGenerator.CreateInstance(poke, pid, pre, post, _.method)));
		CalcEggIVsParam calcEggIVsParam = new CalcEggIVsParam();
		if (calcEggIVsParam.ForIVs = ek_search1.Checked)
		{
			calcEggIVsParam.LowestIVs = new uint[6]
			{
				Util.GetValueDec(ek_IVlow1),
				Util.GetValueDec(ek_IVlow2),
				Util.GetValueDec(ek_IVlow3),
				Util.GetValueDec(ek_IVlow4),
				Util.GetValueDec(ek_IVlow5),
				Util.GetValueDec(ek_IVlow6)
			};
			calcEggIVsParam.HighestIVs = new uint[6]
			{
				Util.GetValueDec(ek_IVup1),
				Util.GetValueDec(ek_IVup2),
				Util.GetValueDec(ek_IVup3),
				Util.GetValueDec(ek_IVup4),
				Util.GetValueDec(ek_IVup5),
				Util.GetValueDec(ek_IVup6)
			};
		}
		if (calcEggIVsParam.ForStats = ek_search2.Checked)
		{
			calcEggIVsParam.TargetStats = new uint[6]
			{
				Util.GetValueDec(ek_stats1),
				Util.GetValueDec(ek_stats2),
				Util.GetValueDec(ek_stats3),
				Util.GetValueDec(ek_stats4),
				Util.GetValueDec(ek_stats5),
				Util.GetValueDec(ek_stats6)
			};
		}
		if (calcEggIVsParam.CheckHP = ek_mezapaType.SelectedIndex != 0)
		{
			calcEggIVsParam.TargetHiddenPowerType = ek_mezapaType.Text.KanjiToPokeType();
		}
		calcEggIVsParam.LowestHiddenPower = Util.GetValueDec(ek_mezapaPower);
		ek_cancel.Enabled = true;
		EggIVsTab.SearchWorker.isBusy = true;
		try
		{
			using (EggIVsTab.SearchWorker.newSource())
			{
				List<EggIVsBinder> data = await CalcEggIVs(generators, EggIVsTab.GetParam(ListMode: false), calcEggIVsParam);
				_eggIVsDGV.SetData(data);
			}
		}
		catch
		{
		}
		EggIVsTab.SearchWorker.isBusy = false;
		ek_cancel.Enabled = false;
		ek_dataGridView.CurrentCell = null;
		ek_start.Enabled = true;
		ek_listup.Enabled = true;
	}

	private async void ListUp_EggIVs_Click(object sender, EventArgs e)
	{
		if (Util.GetValueDec(FirstFrame_EggIVs) > Util.GetValueDec(LastFrame_EggIVs))
		{
			ShowCaution("Fが 下限 ＞上限 になっています。");
			return;
		}
		IEnumerable<(string, GenerateMethod)> selectedEggMethodList = SelectedEggMethodList;
		if (selectedEggMethodList.Count() == 0)
		{
			ShowCaution("Methodが選択されていません。");
			return;
		}
		ek_dataGridView.ClearSelection();
		ek_start.Enabled = false;
		ek_listup.Enabled = false;
		Pokemon.Species poke = HatchablePokemonList[ek_pokedex.SelectedIndex];
		uint pid = (uint)ek_nature.SelectedIndex;
		uint[] pre = SelectedPreParentIVs;
		uint[] post = SelectedPostParentIVs;
		IEnumerable<(string, EggGenerator)> generators = selectedEggMethodList.Select<(string, GenerateMethod), (string, EggGenerator)>(((string label, GenerateMethod method) _) => (_.label, EggGenerator.CreateInstance(poke, pid, pre, post, _.method)));
		CalcEggIVsParam param = new CalcEggIVsParam
		{
			ListMode = true
		};
		ek_cancel.Enabled = true;
		EggIVsTab.SearchWorker.isBusy = true;
		try
		{
			using (EggIVsTab.SearchWorker.newSource())
			{
				List<EggIVsBinder> data = await CalcEggIVs(generators, EggIVsTab.GetParam(ListMode: true), param);
				_eggIVsDGV.SetData(data);
			}
		}
		catch
		{
		}
		EggIVsTab.SearchWorker.isBusy = false;
		ek_cancel.Enabled = false;
		ek_dataGridView.CurrentCell = null;
		ek_start.Enabled = true;
		ek_listup.Enabled = true;
	}

	private void Cancel_EggIVs_Click(object sender, EventArgs e)
	{
		if (EggIVsTab.SearchWorker.isBusy)
		{
			EggIVsTab.SearchWorker.Cancel();
		}
	}

	private Task<List<EggPIDBinder>> CalcEggPID(ProphaseEggGenerator generator, CalcParam param, CalcEggPIDParam param2)
	{
		return Task.Run(delegate
		{
			List<EggPIDBinder> list = new List<EggPIDBinder>();
			uint seed = 0u.NextSeed(param.FirstFrame + param2.MinDiff);
			ulong num = param.FirstFrame;
			while (num <= param.LastFrame)
			{
				if (param.token.IsCancellationRequested)
				{
					param.token.ThrowIfCancellationRequested();
				}
				uint seed2 = seed;
				uint num2 = param2.MinDiff;
				while (num2 < param2.MaxDiff)
				{
					RNGResult<uint?, uint> res = generator.Generate(seed2, (uint)(num & 0xFFFF));
					if (param2.Check(res))
					{
						EggPIDBinder item = new EggPIDBinder((uint)num, 0u, num2, param2.Species, res, param2.TSV);
						list.Add(item);
					}
					num2++;
					seed2.Advance();
				}
				num++;
				seed.Advance();
			}
			return list;
		});
	}

	private Task<List<EggIVsBinder>> CalcEggIVs(IEnumerable<(string, EggGenerator)> generators, CalcParam param, CalcEggIVsParam param2)
	{
		return Task.Run(delegate
		{
			List<EggIVsBinder> list = new List<EggIVsBinder>();
			uint seed = 0u.NextSeed(param.FirstFrame);
			ulong num = param.FirstFrame;
			while (num <= param.LastFrame)
			{
				if (param.token.IsCancellationRequested)
				{
					param.token.ThrowIfCancellationRequested();
				}
				foreach (var generator in generators)
				{
					string item = generator.Item1;
					EggGenerator item2 = generator.Item2;
					RNGResult<Pokemon.Individual, (int, int)[]> res = item2.Generate(seed);
					if (param2.Check(res))
					{
						EggIVsBinder item3 = new EggIVsBinder(res, (uint)num, param.TargetFrame, item);
						list.Add(item3);
					}
				}
				num++;
				seed.Advance();
			}
			return list;
		});
	}

	private void es_pokedex_SelectedIndexChanged(object sender, EventArgs e)
	{
		UpdateSex(es_sex, Pokemon.GetPokemon(es_pokedex.Text));
		UpdateAbility(es_ability, Pokemon.GetPokemon(es_pokedex.Text));
	}

	private void Ek_pokedex_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (ek_pokedex.SelectedIndex != -1)
		{
			UpdateStatBox_egg();
		}
	}

	private void UpdateStatBox_egg()
	{
		uint[] ivs = new uint[6] { 31u, 31u, 31u, 31u, 31u, 31u };
		IReadOnlyList<uint> stats = Pokemon.GetPokemon(ek_pokedex.Text).GetIndividual(Util.GetValueDec(ek_Lv), ivs, (uint)ek_nature.SelectedIndex).Stats;
		ek_stats1.Value = stats[0];
		ek_stats2.Value = stats[1];
		ek_stats3.Value = stats[2];
		ek_stats4.Value = stats[3];
		ek_stats5.Value = stats[4];
		ek_stats6.Value = stats[5];
	}

	private void SetFrameButton_EggIVs_Click(object sender, EventArgs e)
	{
		NumericUpDown firstFrame_EggIVs = FirstFrame_EggIVs;
		NumericUpDown lastFrame_EggIVs = LastFrame_EggIVs;
		(uint, uint) range = Util.GetRange(TargetFrame_EggIVs, FrameRange_EggIVs);
		decimal num = range.Item1;
		decimal num2 = range.Item2;
		decimal num4 = (firstFrame_EggIVs.Value = num);
		num4 = (lastFrame_EggIVs.Value = num2);
	}

	private void ek_search1_CheckedChanged(object sender, EventArgs e)
	{
		NumericUpDown numericUpDown = ek_IVlow1;
		NumericUpDown numericUpDown2 = ek_IVlow2;
		NumericUpDown numericUpDown3 = ek_IVlow3;
		NumericUpDown numericUpDown4 = ek_IVlow4;
		NumericUpDown numericUpDown5 = ek_IVlow5;
		bool flag = (ek_IVlow6.Visible = ek_search1.Checked);
		bool flag3 = (numericUpDown5.Visible = flag);
		bool flag5 = (numericUpDown4.Visible = flag3);
		bool flag7 = (numericUpDown3.Visible = flag5);
		bool visible = (numericUpDown2.Visible = flag7);
		numericUpDown.Visible = visible;
		NumericUpDown numericUpDown6 = ek_IVup1;
		NumericUpDown numericUpDown7 = ek_IVup2;
		NumericUpDown numericUpDown8 = ek_IVup3;
		NumericUpDown numericUpDown9 = ek_IVup4;
		NumericUpDown numericUpDown10 = ek_IVup5;
		flag = (ek_IVup6.Visible = ek_search1.Checked);
		flag3 = (numericUpDown10.Visible = flag);
		flag5 = (numericUpDown9.Visible = flag3);
		flag7 = (numericUpDown8.Visible = flag5);
		visible = (numericUpDown7.Visible = flag7);
		numericUpDown6.Visible = visible;
		NumericUpDown numericUpDown11 = ek_stats1;
		NumericUpDown numericUpDown12 = ek_stats2;
		NumericUpDown numericUpDown13 = ek_stats3;
		NumericUpDown numericUpDown14 = ek_stats4;
		NumericUpDown numericUpDown15 = ek_stats5;
		flag = (ek_stats6.Visible = !ek_search1.Checked);
		flag3 = (numericUpDown15.Visible = flag);
		flag5 = (numericUpDown14.Visible = flag3);
		flag7 = (numericUpDown13.Visible = flag5);
		visible = (numericUpDown12.Visible = flag7);
		numericUpDown11.Visible = visible;
	}

	private void Everstone_CheckStateChanged(object sender, EventArgs e)
	{
	}

	private void Method1_Click(object sender, EventArgs e)
	{
		Method1.Checked |= !Method2.Checked && !Method3.Checked;
	}

	private void Method2_Click(object sender, EventArgs e)
	{
		Method2.Checked |= !Method1.Checked && !Method3.Checked;
	}

	private void Method3_Click(object sender, EventArgs e)
	{
		Method3.Checked |= !Method1.Checked && !Method2.Checked;
	}

	public Form1()
	{
		InitializeComponent();
		for (int i = 0; i < 25; i++)
		{
			Natures.Add(natures[i], (Nature)i);
		}
		StationaryTab = new SearchTab
		{
			InitialSeedBox = k_Initialseed1,
			RTCBox = new NumericUpDown[2] { k_RSmin, k_RSmax },
			MultipleInitialSeedBox = k_Initialseed3,
			InitialSeedButtons = new RadioButton[3] { ForSimpleSeed_stationary, ForRTC_stationary, ForMultipleSeed_stationary },
			FrameBox = new NumericUpDown[2] { FirstFrame_stationary, LastFrame_stationary },
			TargetFrameBox = TargetFrame_stationary,
			FrameRangeBox = FrameRange_stationary,
			DataGridView = k_dataGridView,
			CalcButton = k_start
		};
		_wildTab = new SearchTab
		{
			InitialSeedBox = y_Initialseed1,
			RTCBox = new NumericUpDown[2] { y_RSmin, y_RSmax },
			MultipleInitialSeedBox = y_Initialseed3,
			InitialSeedButtons = new RadioButton[3] { ForSimpleSeed_wild, ForRTC_wild, ForMultipleSeed_wild },
			FrameBox = new NumericUpDown[2] { FirstFrame_wild, LastFrame_wild },
			TargetFrameBox = TargetFrame_wild,
			FrameRangeBox = FrameRange_wild,
			DataGridView = y_dataGridView,
			CalcButton = y_start
		};
		_wildDGV = new DataGridViewWrapper<WildBinder>(y_dataGridView);
		EggPIDTab = new SearchTab
		{
			FrameBox = new NumericUpDown[2] { FirstFrame_EggPID, LastFrame_EggPID },
			DataGridView = es_dataGridView,
			CalcButton = Search_EggPID
		};
		_eggPIDDGV = new DataGridViewWrapper<EggPIDBinder>(es_dataGridView);
		EggIVsTab = new SearchTab
		{
			FrameBox = new NumericUpDown[2] { FirstFrame_EggIVs, LastFrame_EggIVs },
			TargetFrameBox = TargetFrame_EggIVs,
			FrameRangeBox = FrameRange_EggIVs,
			DataGridView = ek_dataGridView,
			CalcButton = ek_start
		};
		_eggIVsDGV = new DataGridViewWrapper<EggIVsBinder>(ek_dataGridView);
		IDTab = new SearchTab
		{
			InitialSeedBox = ID_Initialseed1,
			RTCBox = new NumericUpDown[2] { ID_RSmin, ID_RSmax },
			InitialSeedButtons = new RadioButton[2] { IDInitialseed1, IDInitialseed2 },
			FrameBox = new NumericUpDown[2] { FirstFrame_ID, LastFrame_ID },
			TargetFrameBox = TargetFrame_ID,
			FrameRangeBox = FrameRange_ID,
			DataGridView = ID_dataGridView,
			CalcButton = ID_start
		};
		_idDGV = new DataGridViewWrapper<IDBinder>(ID_dataGridView);
		RomButtons_back = new RadioButton[5] { Ruby_back, Sapphire_back, Em_back, FR_back, LG_back };
		EncounterTypeButtons_back = new RadioButton[6] { Grass_back, Surf_back, OldRod_back, GoodRod_back, SuperRod_back, RockSmash_back };
		SelectedRom_stationary = Rom.Ruby;
		SelectedRom_wild = Rom.Ruby;
		SelectedEncounterType = EncounterType.Grass;
		_stationaryDGV = new DataGridViewWrapper<StationaryBinder>(k_dataGridView);
	}

	private void Form1_Load(object sender, EventArgs e)
	{
		k_dataGridView.DefaultCellStyle.Font = new Font("Consolas", 9f);
		y_dataGridView.DefaultCellStyle.Font = new Font("Consolas", 9f);
		ID_dataGridView.DefaultCellStyle.Font = new Font("Consolas", 9f);
		es_dataGridView.DefaultCellStyle.Font = new Font("Consolas", 9f);
		ek_dataGridView.DefaultCellStyle.Font = new Font("Consolas", 9f);
		dataGridView1_back.DefaultCellStyle.Font = new Font("Consolas", 9f);
		GenderColumn_back.DefaultCellStyle.Font = new Font("ＭＳ ゴシック", 9f);
		Type typeFromHandle = typeof(DataGridView);
		PropertyInfo property = typeFromHandle.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
		property.SetValue(k_dataGridView, true, null);
		property.SetValue(y_dataGridView, true, null);
		property.SetValue(ID_dataGridView, true, null);
		property.SetValue(es_dataGridView, true, null);
		property.SetValue(ek_dataGridView, true, null);
		property.SetValue(dataGridView1_back, true, null);
		Method_stationary.SelectedIndex = 0;
		Method_wild.SelectedIndex = 0;
		FieldAbility_wild.SelectedIndex = 0;
		y_sex.SelectedIndex = 0;
		UpdateMapBox();
		UpdateMapBox_back();
		k_IVlow1.Visible = true;
		k_IVlow2.Visible = true;
		k_IVlow3.Visible = true;
		k_IVlow4.Visible = true;
		k_IVlow5.Visible = true;
		k_IVlow6.Visible = true;
		k_IVup1.Visible = true;
		k_IVup2.Visible = true;
		k_IVup3.Visible = true;
		k_IVup4.Visible = true;
		k_IVup5.Visible = true;
		k_IVup6.Visible = true;
		k_stats1.Visible = false;
		k_stats2.Visible = false;
		k_stats3.Visible = false;
		k_stats4.Visible = false;
		k_stats5.Visible = false;
		k_stats6.Visible = false;
		y_IVlow1.Visible = true;
		y_IVlow2.Visible = true;
		y_IVlow3.Visible = true;
		y_IVlow4.Visible = true;
		y_IVlow5.Visible = true;
		y_IVlow6.Visible = true;
		y_IVup1.Visible = true;
		y_IVup2.Visible = true;
		y_IVup3.Visible = true;
		y_IVup4.Visible = true;
		y_IVup5.Visible = true;
		y_IVup6.Visible = true;
		y_stats1.Visible = false;
		y_stats2.Visible = false;
		y_stats3.Visible = false;
		y_stats4.Visible = false;
		y_stats5.Visible = false;
		y_stats6.Visible = false;
		ek_IVlow1.Visible = true;
		ek_IVlow2.Visible = true;
		ek_IVlow3.Visible = true;
		ek_IVlow4.Visible = true;
		ek_IVlow5.Visible = true;
		ek_IVlow6.Visible = true;
		ek_IVup1.Visible = true;
		ek_IVup2.Visible = true;
		ek_IVup3.Visible = true;
		ek_IVup4.Visible = true;
		ek_IVup5.Visible = true;
		ek_IVup6.Visible = true;
		ek_stats1.Visible = false;
		ek_stats2.Visible = false;
		ek_stats3.Visible = false;
		ek_stats4.Visible = false;
		ek_stats5.Visible = false;
		ek_stats6.Visible = false;
		k_Lv.Enabled = true;
		y_Lv.Enabled = false;
		string[] array = new string[25]
		{
			"がんばりや", "さみしがり", "ゆうかん", "いじっぱり", "やんちゃ", "ずぶとい", "すなお", "のんき", "わんぱく", "のうてんき",
			"おくびょう", "せっかち", "まじめ", "ようき", "むじゃき", "ひかえめ", "おっとり", "れいせい", "てれや", "うっかりや",
			"おだやか", "おとなしい", "なまいき", "しんちょう", "きまぐれ"
		};
		ComboBox.ObjectCollection items = SyncNature_wild.Items;
		object[] items2 = array;
		items.AddRange(items2);
		ComboBox.ObjectCollection items3 = EverstoneNature_EggPID.Items;
		items2 = array;
		items3.AddRange(items2);
		ComboBox.ObjectCollection items4 = ek_nature.Items;
		items2 = array;
		items4.AddRange(items2);
		SyncNature_wild.SelectedIndex = 0;
		EverstoneNature_EggPID.SelectedIndex = 0;
		ek_nature.SelectedIndex = 3;
		HatchablePokemonList = (from _ in Pokemon.GetUniquePokemonList()
			where _.IsHatchable
			select _).ToList();
		string[] array2 = HatchablePokemonList.Select((Pokemon.Species _) => _.Name).ToArray();
		ComboBox.ObjectCollection items5 = es_pokedex.Items;
		items2 = array2;
		items5.AddRange(items2);
		ComboBox.ObjectCollection items6 = ek_pokedex.Items;
		items2 = array2;
		items6.AddRange(items2);
		es_pokedex.SelectedIndex = 0;
		ek_pokedex.SelectedIndex = 0;
		string[] array3 = new string[17]
		{
			"指定なし", "闘", "飛", "毒", "地", "岩", "虫", "霊", "鋼", "炎",
			"水", "草", "電", "超", "氷", "龍", "悪"
		};
		ComboBox.ObjectCollection items7 = k_mezapaType.Items;
		items2 = array3;
		items7.AddRange(items2);
		ComboBox.ObjectCollection items8 = y_mezapaType.Items;
		items2 = array3;
		items8.AddRange(items2);
		ComboBox.ObjectCollection items9 = ek_mezapaType.Items;
		items2 = array3;
		items9.AddRange(items2);
		ComboBox.ObjectCollection items10 = HiddenPowerType_back.Items;
		items2 = array3;
		items10.AddRange(items2);
		k_mezapaType.SelectedIndex = 0;
		y_mezapaType.SelectedIndex = 0;
		ek_mezapaType.SelectedIndex = 0;
		HiddenPowerType_back.SelectedIndex = 0;
		Language.SelectedIndex = 0;
		UpdateSymbolList();
		UpdateStatBox_stationary();
		UpdateStatBox_egg();
		PokeBlockBox.SelectedIndex = 0;
		CCGender_wild.SelectedIndex = 0;
		AddEvent(TabPage_stationary, "NatureButton_stationary");
		AddEvent(TabPage_wild, "NatureButton_wild");
		AddEvent(TabPage_Egg, "NatureButton_EggPID");
		AddEvent(TabPage_backword, "NatureButton_back");
	}

	private void AddEvent(Control control, string buttonName)
	{
		if (control.GetType().Equals(typeof(NaturePanel)))
		{
			return;
		}
		if (control.Controls.Count != 0)
		{
			foreach (Control control2 in control.Controls)
			{
				AddEvent(control2, buttonName);
			}
		}
		if (control.Name != buttonName)
		{
			control.MouseClick += UnvisiblizeNaturePanel;
		}
	}

	private void UnvisiblizeNaturePanel(object sender, MouseEventArgs e)
	{
		NaturePanel_stationary.Visible = false;
		NaturePanel_wild.Visible = false;
		NaturePanel_back.Visible = false;
	}

	private void tabControl1_Click(object sender, EventArgs e)
	{
		dataGridView_table.CurrentCell = null;
		EncounterTable_back.CurrentCell = null;
	}

	private void TextBox_SelectText(object sender, EventArgs e)
	{
		((TextBox)sender).SelectAll();
	}

	private void NumericUpDown_SelectValue(object sender, EventArgs e)
	{
		((NumericUpDown)sender).Select(0, ((NumericUpDown)sender).Text.Length);
	}

	private void InitialSeedBox_Check(object sender, CancelEventArgs e)
	{
		TextBox textBox = sender as TextBox;
		if (textBox.Text == "" || Regex.IsMatch(textBox.Text, "[^0-9a-fA-F]+$"))
		{
			textBox.Undo();
		}
	}

	private void NumericUpDown_Check(object sender, CancelEventArgs e)
	{
		NumericUpDown numericUpDown = sender as NumericUpDown;
		Control control = numericUpDown;
		if (!(numericUpDown.Text == "") || !(control is NumericUpDown))
		{
			return;
		}
		foreach (Control control2 in ((NumericUpDown)control).Controls)
		{
			if (control2 is TextBox)
			{
				((TextBox)control2).Undo();
				break;
			}
		}
	}

	private void RadioButton_Checked(object sender, EventArgs e)
	{
	}

	private void SelectAllToolStripMenuItem_Click(object sender, EventArgs e)
	{
		k_dataGridView.SelectAll();
	}

	private void SelectAllToolStripMenuItem2_Click(object sender, EventArgs e)
	{
		y_dataGridView.SelectAll();
	}

	private void SelectAllToolStripMenuItem3_Click(object sender, EventArgs e)
	{
		ID_dataGridView.SelectAll();
	}

	private void SelectAllToolStripMenuItem4_Click(object sender, EventArgs e)
	{
		es_dataGridView.SelectAll();
	}

	private void SelectAllToolStripMenuItem5_Click(object sender, EventArgs e)
	{
		ek_dataGridView.SelectAll();
	}

	private void SelectAllToolStripMenuItem6_Click(object sender, EventArgs e)
	{
		dataGridView1_back.SelectAll();
	}

	private void copyToolStripMenuItem_Click(object sender, EventArgs e)
	{
		try
		{
			Clipboard.SetDataObject(k_dataGridView.GetClipboardContent());
		}
		catch (ArgumentNullException)
		{
			MessageBox.Show("選択されていません");
		}
	}

	private void copyToolStripMenuItem2_Click(object sender, EventArgs e)
	{
		try
		{
			Clipboard.SetDataObject(y_dataGridView.GetClipboardContent());
		}
		catch (ArgumentNullException)
		{
			MessageBox.Show("選択されていません");
		}
	}

	private void copyToolStripMenuItem3_Click(object sender, EventArgs e)
	{
		try
		{
			Clipboard.SetDataObject(ID_dataGridView.GetClipboardContent());
		}
		catch (ArgumentNullException)
		{
			MessageBox.Show("選択されていません");
		}
	}

	private void copyToolStripMenuItem4_Click(object sender, EventArgs e)
	{
		try
		{
			Clipboard.SetDataObject(es_dataGridView.GetClipboardContent());
		}
		catch (ArgumentNullException)
		{
			MessageBox.Show("選択されていません");
		}
	}

	private void copyToolStripMenuItem5_Click(object sender, EventArgs e)
	{
		try
		{
			Clipboard.SetDataObject(ek_dataGridView.GetClipboardContent());
		}
		catch (ArgumentNullException)
		{
			MessageBox.Show("選択されていません");
		}
	}

	private void copyToolStripMenuItem6_Click(object sender, EventArgs e)
	{
		try
		{
			Clipboard.SetDataObject(dataGridView1_back.GetClipboardContent());
		}
		catch (ArgumentNullException)
		{
			MessageBox.Show("選択されていません");
		}
	}

	private void Language_button_Click(object sender, EventArgs e)
	{
		SyncNature_wild.Items.Clear();
		EverstoneNature_EggPID.Items.Clear();
		ek_nature.Items.Clear();
		if (Language.Text == "日本語")
		{
			for (int i = 0; i < nature_table.GetLength(0); i++)
			{
				if (i > 0)
				{
					SyncNature_wild.Items.Add(nature_table_jp[i, 1]);
					EverstoneNature_EggPID.Items.Add(nature_table_jp[i, 1]);
					ek_nature.Items.Add(nature_table_jp[i, 1]);
					natures[i - 1] = natures_jp[i - 1];
				}
				nature_table[i, 0] = nature_table_jp[i, 0];
				nature_table[i, 1] = nature_table_jp[i, 1];
			}
			NatureButton_stationary.Enabled = true;
			NatureButton_wild.Enabled = true;
		}
		else
		{
			NatureButton_stationary.Enabled = false;
			NatureButton_wild.Enabled = false;
		}
		if (Language.Text == "英語")
		{
			for (int j = 0; j < nature_table.GetLength(0); j++)
			{
				if (j > 0)
				{
					SyncNature_wild.Items.Add(nature_table_en[j, 1]);
					EverstoneNature_EggPID.Items.Add(nature_table_en[j, 1]);
					ek_nature.Items.Add(nature_table_en[j, 1]);
					natures[j - 1] = natures_en[j - 1];
				}
				nature_table[j, 0] = nature_table_en[j, 0];
				nature_table[j, 1] = nature_table_en[j, 1];
			}
		}
		if (Language.Text == "ドイツ語")
		{
			for (int k = 0; k < nature_table.GetLength(0); k++)
			{
				if (k > 0)
				{
					SyncNature_wild.Items.Add(nature_table_ge[k, 1]);
					EverstoneNature_EggPID.Items.Add(nature_table_ge[k, 1]);
					ek_nature.Items.Add(nature_table_ge[k, 1]);
					natures[k - 1] = natures_ge[k - 1];
				}
				nature_table[k, 0] = nature_table_ge[k, 0];
				nature_table[k, 1] = nature_table_ge[k, 1];
			}
		}
		if (Language.Text == "フランス語")
		{
			for (int l = 0; l < nature_table.GetLength(0); l++)
			{
				if (l > 0)
				{
					SyncNature_wild.Items.Add(nature_table_fr[l, 1]);
					EverstoneNature_EggPID.Items.Add(nature_table_fr[l, 1]);
					ek_nature.Items.Add(nature_table_fr[l, 1]);
					natures[l - 1] = natures_fr[l - 1];
				}
				nature_table[l, 0] = nature_table_fr[l, 0];
				nature_table[l, 1] = nature_table_fr[l, 1];
			}
		}
		if (Language.Text == "イタリア語")
		{
			for (int m = 0; m < nature_table.GetLength(0); m++)
			{
				if (m > 0)
				{
					SyncNature_wild.Items.Add(nature_table_ita[m, 1]);
					EverstoneNature_EggPID.Items.Add(nature_table_ita[m, 1]);
					ek_nature.Items.Add(nature_table_ita[m, 1]);
					natures[m - 1] = natures_ita[m - 1];
				}
				nature_table[m, 0] = nature_table_ita[m, 0];
				nature_table[m, 1] = nature_table_ita[m, 1];
			}
		}
		if (Language.Text == "スペイン語")
		{
			for (int n = 0; n < nature_table.GetLength(0); n++)
			{
				if (n > 0)
				{
					SyncNature_wild.Items.Add(nature_table_spa[n, 1]);
					EverstoneNature_EggPID.Items.Add(nature_table_spa[n, 1]);
					ek_nature.Items.Add(nature_table_spa[n, 1]);
					natures[n - 1] = natures_spa[n - 1];
				}
				nature_table[n, 0] = nature_table_spa[n, 0];
				nature_table[n, 1] = nature_table_spa[n, 1];
			}
		}
		SyncNature_wild.SelectedIndex = 0;
		EverstoneNature_EggPID.SelectedIndex = 0;
		ek_nature.SelectedIndex = 0;
	}

	private void poyo_Click(object sender, EventArgs e)
	{
		if (File.Exists("Data/HuiFontP29.ttf"))
		{
			k_dataGridView.DefaultCellStyle.Font = new Font("ふい字", 9f);
			y_dataGridView.DefaultCellStyle.Font = new Font("ふい字", 9f);
			ID_dataGridView.DefaultCellStyle.Font = new Font("ふい字", 9f);
			es_dataGridView.DefaultCellStyle.Font = new Font("ふい字", 9f);
			ek_dataGridView.DefaultCellStyle.Font = new Font("ふい字", 9f);
		}
		else if (File.Exists("Data/HuiFont29.ttf"))
		{
			k_dataGridView.DefaultCellStyle.Font = new Font("ふい字Ｐ", 9f);
			y_dataGridView.DefaultCellStyle.Font = new Font("ふい字Ｐ", 9f);
			ID_dataGridView.DefaultCellStyle.Font = new Font("ふい字Ｐ", 9f);
			es_dataGridView.DefaultCellStyle.Font = new Font("ふい字Ｐ", 9f);
			ek_dataGridView.DefaultCellStyle.Font = new Font("ふい字Ｐ", 9f);
		}
		else
		{
			MessageBox.Show("例のファイルは存在しません。最新版を入れて下さい。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}
	}

	private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
	{
		StationaryBinder stationaryBinder = k_dataGridView.Rows[e.RowIndex].DataBoundItem as StationaryBinder;
		if (stationaryBinder.IsShiny)
		{
			e.CellStyle.BackColor = Color.LightCyan;
		}
	}

	private void dataGridView_MouseDown(object sender, MouseEventArgs e)
	{
		DataGridView dataGridView = sender as DataGridView;
		DataGridView.HitTestInfo hitTestInfo = dataGridView.HitTest(e.X, e.Y);
		if (hitTestInfo.Type == DataGridViewHitTestType.ColumnHeader)
		{
			dataGridView.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
		}
		else if (hitTestInfo.Type == DataGridViewHitTestType.RowHeader)
		{
			dataGridView.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
		}
	}

	private void Y_dataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
	{
		WildBinder wildBinder = y_dataGridView.Rows[e.RowIndex].DataBoundItem as WildBinder;
		if (wildBinder.IsShiny)
		{
			e.CellStyle.BackColor = Color.LightCyan;
		}
	}

	private void Es_dataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
	{
		EggPIDBinder eggPIDBinder = es_dataGridView.Rows[e.RowIndex].DataBoundItem as EggPIDBinder;
		if (eggPIDBinder.IsShiny)
		{
			e.CellStyle.BackColor = Color.LightCyan;
		}
	}

	private void Ek_dataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
	{
		if (e.ColumnIndex >= 2 && e.ColumnIndex <= 7)
		{
			EggIVsBinder eggIVsBinder = ek_dataGridView.Rows[e.RowIndex].DataBoundItem as EggIVsBinder;
			if (e.ColumnIndex - 2 == eggIVsBinder.HeredityPoint1)
			{
				e.CellStyle.ForeColor = eggIVsBinder.Color1;
			}
			if (e.ColumnIndex - 2 == eggIVsBinder.HeredityPoint2)
			{
				e.CellStyle.ForeColor = eggIVsBinder.Color2;
			}
			if (e.ColumnIndex - 2 == eggIVsBinder.HeredityPoint3)
			{
				e.CellStyle.ForeColor = eggIVsBinder.Color3;
			}
		}
	}

	private void StationaryDGVRowBindingSource_ListChanged(object sender, ListChangedEventArgs e)
	{
	}

	private void Button2_Click(object sender, EventArgs e)
	{
		if (subform1 == null || subform1.IsDisposed)
		{
			subform1 = new CalcInitialSeedForm();
			subform1.Show();
		}
		subform1.WindowState = FormWindowState.Normal;
		subform1.Activate();
	}

	private void Button3_Click(object sender, EventArgs e)
	{
		if (subform2 == null || subform2.IsDisposed)
		{
			subform2 = new IVsCalcBackForm();
			subform2.Show();
		}
		subform2.WindowState = FormWindowState.Normal;
		subform2.Activate();
	}

	private void Button5_Click(object sender, EventArgs e)
	{
		if (subform3 == null || subform3.IsDisposed)
		{
			subform3 = new CalcSIDForm();
			subform3.Show();
		}
		subform3.WindowState = FormWindowState.Normal;
		subform3.Activate();
	}

	private void Button6_Click(object sender, EventArgs e)
	{
		if (subform4 == null || subform4.IsDisposed)
		{
			subform4 = new CalcSVForm();
			subform4.Show();
		}
		subform4.WindowState = FormWindowState.Normal;
		subform4.Activate();
	}

	private void button7_Click(object sender, EventArgs e)
	{
		if (subform5 == null || subform5.IsDisposed)
		{
			subform5 = new FindPaintSeedForm();
			subform5.Show();
		}
		subform5.WindowState = FormWindowState.Normal;
		subform5.Activate();
	}

	private void button8_Click(object sender, EventArgs e)
	{
		if (subform6 == null || subform6.IsDisposed)
		{
			subform6 = new FindInitialSeedForm();
			subform6.Show();
		}
		subform6.WindowState = FormWindowState.Normal;
		subform6.Activate();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(_3genSearch.Form1));
		this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.SelectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.copyToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
		this.SelectAllToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
		this.contextMenuStrip3 = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.copyToolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
		this.SelectAllToolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
		this.contextMenuStrip4 = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.copyToolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
		this.SelectAllToolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
		this.contextMenuStrip5 = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.copyToolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
		this.SelectAllToolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
		this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.label68 = new System.Windows.Forms.Label();
		this.label67 = new System.Windows.Forms.Label();
		this.textBox35 = new System.Windows.Forms.TextBox();
		this.label66 = new System.Windows.Forms.Label();
		this.textBox34 = new System.Windows.Forms.TextBox();
		this.label65 = new System.Windows.Forms.Label();
		this.textBox33 = new System.Windows.Forms.TextBox();
		this.label64 = new System.Windows.Forms.Label();
		this.textBox32 = new System.Windows.Forms.TextBox();
		this.label63 = new System.Windows.Forms.Label();
		this.textBox31 = new System.Windows.Forms.TextBox();
		this.label62 = new System.Windows.Forms.Label();
		this.textBox30 = new System.Windows.Forms.TextBox();
		this.label61 = new System.Windows.Forms.Label();
		this.textBox29 = new System.Windows.Forms.TextBox();
		this.label60 = new System.Windows.Forms.Label();
		this.textBox28 = new System.Windows.Forms.TextBox();
		this.label59 = new System.Windows.Forms.Label();
		this.textBox27 = new System.Windows.Forms.TextBox();
		this.label58 = new System.Windows.Forms.Label();
		this.textBox26 = new System.Windows.Forms.TextBox();
		this.label57 = new System.Windows.Forms.Label();
		this.textBox25 = new System.Windows.Forms.TextBox();
		this.label56 = new System.Windows.Forms.Label();
		this.textBox24 = new System.Windows.Forms.TextBox();
		this.label55 = new System.Windows.Forms.Label();
		this.label54 = new System.Windows.Forms.Label();
		this.comboBox6 = new System.Windows.Forms.ComboBox();
		this.textBox23 = new System.Windows.Forms.TextBox();
		this.label53 = new System.Windows.Forms.Label();
		this.label26 = new System.Windows.Forms.Label();
		this.label25 = new System.Windows.Forms.Label();
		this.label24 = new System.Windows.Forms.Label();
		this.label23 = new System.Windows.Forms.Label();
		this.label22 = new System.Windows.Forms.Label();
		this.comboBox5 = new System.Windows.Forms.ComboBox();
		this.label21 = new System.Windows.Forms.Label();
		this.label20 = new System.Windows.Forms.Label();
		this.comboBox4 = new System.Windows.Forms.ComboBox();
		this.label19 = new System.Windows.Forms.Label();
		this.textBox22 = new System.Windows.Forms.TextBox();
		this.textBox21 = new System.Windows.Forms.TextBox();
		this.textBox20 = new System.Windows.Forms.TextBox();
		this.textBox19 = new System.Windows.Forms.TextBox();
		this.textBox18 = new System.Windows.Forms.TextBox();
		this.textBox17 = new System.Windows.Forms.TextBox();
		this.checkBox6 = new System.Windows.Forms.CheckBox();
		this.checkBox5 = new System.Windows.Forms.CheckBox();
		this.checkBox4 = new System.Windows.Forms.CheckBox();
		this.checkBox3 = new System.Windows.Forms.CheckBox();
		this.textBox16 = new System.Windows.Forms.TextBox();
		this.textBox15 = new System.Windows.Forms.TextBox();
		this.label18 = new System.Windows.Forms.Label();
		this.label17 = new System.Windows.Forms.Label();
		this.textBox14 = new System.Windows.Forms.TextBox();
		this.textBox13 = new System.Windows.Forms.TextBox();
		this.label16 = new System.Windows.Forms.Label();
		this.label15 = new System.Windows.Forms.Label();
		this.label14 = new System.Windows.Forms.Label();
		this.button1 = new System.Windows.Forms.Button();
		this.label13 = new System.Windows.Forms.Label();
		this.radioButton6 = new System.Windows.Forms.RadioButton();
		this.textBox12 = new System.Windows.Forms.TextBox();
		this.radioButton5 = new System.Windows.Forms.RadioButton();
		this.textBox11 = new System.Windows.Forms.TextBox();
		this.label12 = new System.Windows.Forms.Label();
		this.textBox10 = new System.Windows.Forms.TextBox();
		this.label11 = new System.Windows.Forms.Label();
		this.radioButton4 = new System.Windows.Forms.RadioButton();
		this.textBox9 = new System.Windows.Forms.TextBox();
		this.checkBox2 = new System.Windows.Forms.CheckBox();
		this.label8 = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.textBox8 = new System.Windows.Forms.TextBox();
		this.textBox7 = new System.Windows.Forms.TextBox();
		this.radioButton7 = new System.Windows.Forms.RadioButton();
		this.radioButton8 = new System.Windows.Forms.RadioButton();
		this.radioButton9 = new System.Windows.Forms.RadioButton();
		this.radioButton10 = new System.Windows.Forms.RadioButton();
		this.radioButton11 = new System.Windows.Forms.RadioButton();
		this.comboBox7 = new System.Windows.Forms.ComboBox();
		this.textBox60 = new System.Windows.Forms.TextBox();
		this.label102 = new System.Windows.Forms.Label();
		this.label101 = new System.Windows.Forms.Label();
		this.label100 = new System.Windows.Forms.Label();
		this.label99 = new System.Windows.Forms.Label();
		this.label98 = new System.Windows.Forms.Label();
		this.label97 = new System.Windows.Forms.Label();
		this.comboBox8 = new System.Windows.Forms.ComboBox();
		this.label96 = new System.Windows.Forms.Label();
		this.label95 = new System.Windows.Forms.Label();
		this.comboBox3 = new System.Windows.Forms.ComboBox();
		this.label94 = new System.Windows.Forms.Label();
		this.textBox59 = new System.Windows.Forms.TextBox();
		this.textBox58 = new System.Windows.Forms.TextBox();
		this.textBox57 = new System.Windows.Forms.TextBox();
		this.textBox56 = new System.Windows.Forms.TextBox();
		this.textBox55 = new System.Windows.Forms.TextBox();
		this.textBox54 = new System.Windows.Forms.TextBox();
		this.checkBox7 = new System.Windows.Forms.CheckBox();
		this.textBox53 = new System.Windows.Forms.TextBox();
		this.textBox52 = new System.Windows.Forms.TextBox();
		this.label93 = new System.Windows.Forms.Label();
		this.label92 = new System.Windows.Forms.Label();
		this.textBox51 = new System.Windows.Forms.TextBox();
		this.textBox50 = new System.Windows.Forms.TextBox();
		this.label91 = new System.Windows.Forms.Label();
		this.label90 = new System.Windows.Forms.Label();
		this.label89 = new System.Windows.Forms.Label();
		this.button4 = new System.Windows.Forms.Button();
		this.label88 = new System.Windows.Forms.Label();
		this.radioButton3 = new System.Windows.Forms.RadioButton();
		this.textBox49 = new System.Windows.Forms.TextBox();
		this.radioButton2 = new System.Windows.Forms.RadioButton();
		this.textBox48 = new System.Windows.Forms.TextBox();
		this.label87 = new System.Windows.Forms.Label();
		this.textBox47 = new System.Windows.Forms.TextBox();
		this.label86 = new System.Windows.Forms.Label();
		this.radioButton1 = new System.Windows.Forms.RadioButton();
		this.textBox46 = new System.Windows.Forms.TextBox();
		this.checkBox8 = new System.Windows.Forms.CheckBox();
		this.label85 = new System.Windows.Forms.Label();
		this.label84 = new System.Windows.Forms.Label();
		this.textBox45 = new System.Windows.Forms.TextBox();
		this.textBox44 = new System.Windows.Forms.TextBox();
		this.comboBox2 = new System.Windows.Forms.ComboBox();
		this.label83 = new System.Windows.Forms.Label();
		this.label82 = new System.Windows.Forms.Label();
		this.textBox43 = new System.Windows.Forms.TextBox();
		this.label81 = new System.Windows.Forms.Label();
		this.textBox42 = new System.Windows.Forms.TextBox();
		this.label80 = new System.Windows.Forms.Label();
		this.textBox41 = new System.Windows.Forms.TextBox();
		this.label79 = new System.Windows.Forms.Label();
		this.textBox40 = new System.Windows.Forms.TextBox();
		this.label78 = new System.Windows.Forms.Label();
		this.textBox39 = new System.Windows.Forms.TextBox();
		this.label77 = new System.Windows.Forms.Label();
		this.textBox38 = new System.Windows.Forms.TextBox();
		this.label76 = new System.Windows.Forms.Label();
		this.textBox37 = new System.Windows.Forms.TextBox();
		this.label75 = new System.Windows.Forms.Label();
		this.textBox36 = new System.Windows.Forms.TextBox();
		this.label74 = new System.Windows.Forms.Label();
		this.textBox6 = new System.Windows.Forms.TextBox();
		this.label73 = new System.Windows.Forms.Label();
		this.textBox5 = new System.Windows.Forms.TextBox();
		this.label72 = new System.Windows.Forms.Label();
		this.textBox4 = new System.Windows.Forms.TextBox();
		this.label71 = new System.Windows.Forms.Label();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.label70 = new System.Windows.Forms.Label();
		this.label69 = new System.Windows.Forms.Label();
		this.comboBox1 = new System.Windows.Forms.ComboBox();
		this.checkBox1 = new System.Windows.Forms.CheckBox();
		this.dataGridViewTextBoxColumn29 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn28 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn27 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn26 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn25 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn24 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn23 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn22 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn21 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn20 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn19 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn18 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn17 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn16 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn15 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn14 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.TabPage_Egg = new System.Windows.Forms.TabPage();
		this.tabControl2 = new System.Windows.Forms.TabControl();
		this.TabPage_EggPID = new System.Windows.Forms.TabPage();
		this.es_groupBox2 = new System.Windows.Forms.GroupBox();
		this.label143 = new System.Windows.Forms.Label();
		this.SID_EggPID = new System.Windows.Forms.NumericUpDown();
		this.TID_EggPID = new System.Windows.Forms.NumericUpDown();
		this.OnlyShiny_EggPID = new System.Windows.Forms.CheckBox();
		this.label142 = new System.Windows.Forms.Label();
		this.es_pokedex = new System.Windows.Forms.ComboBox();
		this.label140 = new System.Windows.Forms.Label();
		this.es_sex = new System.Windows.Forms.ComboBox();
		this.label144 = new System.Windows.Forms.Label();
		this.label147 = new System.Windows.Forms.Label();
		this.label141 = new System.Windows.Forms.Label();
		this.es_ability = new System.Windows.Forms.ComboBox();
		this.es_groupBox1 = new System.Windows.Forms.GroupBox();
		this.DiffMax = new System.Windows.Forms.NumericUpDown();
		this.Cancel_EggPID = new System.Windows.Forms.Button();
		this.DiffMin = new System.Windows.Forms.NumericUpDown();
		this.Search_EggPID = new System.Windows.Forms.Button();
		this.label122 = new System.Windows.Forms.Label();
		this.EverstoneNature_EggPID = new System.Windows.Forms.ComboBox();
		this.LastFrame_EggPID = new System.Windows.Forms.NumericUpDown();
		this.FirstFrame_EggPID = new System.Windows.Forms.NumericUpDown();
		this.label120 = new System.Windows.Forms.Label();
		this.calibration_value3 = new System.Windows.Forms.RadioButton();
		this.EverstoneCheck = new System.Windows.Forms.CheckBox();
		this.label138 = new System.Windows.Forms.Label();
		this.calibration_value1 = new System.Windows.Forms.RadioButton();
		this.label145 = new System.Windows.Forms.Label();
		this.calibration_value2 = new System.Windows.Forms.RadioButton();
		this.label155 = new System.Windows.Forms.Label();
		this.es_dataGridView = new System.Windows.Forms.DataGridView();
		this.TabPage_EggIVs = new System.Windows.Forms.TabPage();
		this.ek_groupBox3 = new System.Windows.Forms.GroupBox();
		this.ek_stats6 = new System.Windows.Forms.NumericUpDown();
		this.ek_stats5 = new System.Windows.Forms.NumericUpDown();
		this.ek_stats4 = new System.Windows.Forms.NumericUpDown();
		this.ek_stats3 = new System.Windows.Forms.NumericUpDown();
		this.ek_stats2 = new System.Windows.Forms.NumericUpDown();
		this.ek_stats1 = new System.Windows.Forms.NumericUpDown();
		this.label156 = new System.Windows.Forms.Label();
		this.ek_mezapaPower = new System.Windows.Forms.NumericUpDown();
		this.label157 = new System.Windows.Forms.Label();
		this.ek_mezapaType = new System.Windows.Forms.ComboBox();
		this.ek_search2 = new System.Windows.Forms.RadioButton();
		this.label158 = new System.Windows.Forms.Label();
		this.label159 = new System.Windows.Forms.Label();
		this.ek_search1 = new System.Windows.Forms.RadioButton();
		this.label162 = new System.Windows.Forms.Label();
		this.label164 = new System.Windows.Forms.Label();
		this.label165 = new System.Windows.Forms.Label();
		this.label166 = new System.Windows.Forms.Label();
		this.ek_pokedex = new System.Windows.Forms.ComboBox();
		this.label167 = new System.Windows.Forms.Label();
		this.label168 = new System.Windows.Forms.Label();
		this.ek_IVup6 = new System.Windows.Forms.NumericUpDown();
		this.ek_IVlow1 = new System.Windows.Forms.NumericUpDown();
		this.label169 = new System.Windows.Forms.Label();
		this.ek_IVlow2 = new System.Windows.Forms.NumericUpDown();
		this.ek_nature = new System.Windows.Forms.ComboBox();
		this.ek_IVlow3 = new System.Windows.Forms.NumericUpDown();
		this.ek_IVup5 = new System.Windows.Forms.NumericUpDown();
		this.ek_IVlow4 = new System.Windows.Forms.NumericUpDown();
		this.label170 = new System.Windows.Forms.Label();
		this.ek_IVlow5 = new System.Windows.Forms.NumericUpDown();
		this.label171 = new System.Windows.Forms.Label();
		this.ek_IVlow6 = new System.Windows.Forms.NumericUpDown();
		this.ek_IVup4 = new System.Windows.Forms.NumericUpDown();
		this.label172 = new System.Windows.Forms.Label();
		this.label173 = new System.Windows.Forms.Label();
		this.ek_IVup1 = new System.Windows.Forms.NumericUpDown();
		this.ek_IVup3 = new System.Windows.Forms.NumericUpDown();
		this.label174 = new System.Windows.Forms.Label();
		this.label175 = new System.Windows.Forms.Label();
		this.ek_IVup2 = new System.Windows.Forms.NumericUpDown();
		this.ek_Lv = new System.Windows.Forms.NumericUpDown();
		this.ek_groupBox2 = new System.Windows.Forms.GroupBox();
		this.FrameRange_EggIVs = new System.Windows.Forms.NumericUpDown();
		this.TargetFrame_EggIVs = new System.Windows.Forms.NumericUpDown();
		this.LastFrame_EggIVs = new System.Windows.Forms.NumericUpDown();
		this.FirstFrame_EggIVs = new System.Windows.Forms.NumericUpDown();
		this.SetFrameButton_EggIVs = new System.Windows.Forms.Button();
		this.label124 = new System.Windows.Forms.Label();
		this.label139 = new System.Windows.Forms.Label();
		this.label146 = new System.Windows.Forms.Label();
		this.label160 = new System.Windows.Forms.Label();
		this.Method1 = new System.Windows.Forms.CheckBox();
		this.Method2 = new System.Windows.Forms.CheckBox();
		this.label163 = new System.Windows.Forms.Label();
		this.Method3 = new System.Windows.Forms.CheckBox();
		this.ek_cancel = new System.Windows.Forms.Button();
		this.ek_listup = new System.Windows.Forms.Button();
		this.ek_groupBox1 = new System.Windows.Forms.GroupBox();
		this.label177 = new System.Windows.Forms.Label();
		this.pre_parent6 = new System.Windows.Forms.NumericUpDown();
		this.label178 = new System.Windows.Forms.Label();
		this.pre_parent5 = new System.Windows.Forms.NumericUpDown();
		this.label179 = new System.Windows.Forms.Label();
		this.pre_parent4 = new System.Windows.Forms.NumericUpDown();
		this.label153 = new System.Windows.Forms.Label();
		this.pre_parent3 = new System.Windows.Forms.NumericUpDown();
		this.label152 = new System.Windows.Forms.Label();
		this.pre_parent2 = new System.Windows.Forms.NumericUpDown();
		this.post_parent6 = new System.Windows.Forms.NumericUpDown();
		this.post_parent5 = new System.Windows.Forms.NumericUpDown();
		this.post_parent4 = new System.Windows.Forms.NumericUpDown();
		this.post_parent3 = new System.Windows.Forms.NumericUpDown();
		this.post_parent2 = new System.Windows.Forms.NumericUpDown();
		this.post_parent1 = new System.Windows.Forms.NumericUpDown();
		this.label199 = new System.Windows.Forms.Label();
		this.label201 = new System.Windows.Forms.Label();
		this.pre_parent1 = new System.Windows.Forms.NumericUpDown();
		this.label150 = new System.Windows.Forms.Label();
		this.ek_dataGridView = new System.Windows.Forms.DataGridView();
		this.ek_start = new System.Windows.Forms.Button();
		this.TabPage_ID = new System.Windows.Forms.TabPage();
		this.ID_groupBox1 = new System.Windows.Forms.GroupBox();
		this.FrameRange_ID = new System.Windows.Forms.NumericUpDown();
		this.TargetFrame_ID = new System.Windows.Forms.NumericUpDown();
		this.LastFrame_ID = new System.Windows.Forms.NumericUpDown();
		this.FirstFrame_ID = new System.Windows.Forms.NumericUpDown();
		this.SetFrameButton_ID = new System.Windows.Forms.Button();
		this.label132 = new System.Windows.Forms.Label();
		this.label134 = new System.Windows.Forms.Label();
		this.label136 = new System.Windows.Forms.Label();
		this.label129 = new System.Windows.Forms.Label();
		this.ID_RSmax = new System.Windows.Forms.NumericUpDown();
		this.ID_RSmin = new System.Windows.Forms.NumericUpDown();
		this.label130 = new System.Windows.Forms.Label();
		this.IDInitialseed2 = new System.Windows.Forms.RadioButton();
		this.ID_Initialseed1 = new System.Windows.Forms.TextBox();
		this.IDInitialseed1 = new System.Windows.Forms.RadioButton();
		this.label131 = new System.Windows.Forms.Label();
		this.label133 = new System.Windows.Forms.Label();
		this.label135 = new System.Windows.Forms.Label();
		this.ID_groupBox2 = new System.Windows.Forms.GroupBox();
		this.label137 = new System.Windows.Forms.Label();
		this.label128 = new System.Windows.Forms.Label();
		this.CheckSID = new System.Windows.Forms.CheckBox();
		this.CheckTID = new System.Windows.Forms.CheckBox();
		this.TID_ID = new System.Windows.Forms.NumericUpDown();
		this.SID_ID = new System.Windows.Forms.NumericUpDown();
		this.ID_PID = new System.Windows.Forms.TextBox();
		this.ID_shiny = new System.Windows.Forms.CheckBox();
		this.ID_cancel = new System.Windows.Forms.Button();
		this.ID_listup = new System.Windows.Forms.Button();
		this.ID_start = new System.Windows.Forms.Button();
		this.ID_dataGridView = new System.Windows.Forms.DataGridView();
		this.TabPage_wild = new System.Windows.Forms.TabPage();
		this.panel2 = new System.Windows.Forms.Panel();
		this.CheckAppearing_wild = new System.Windows.Forms.CheckBox();
		this.groupBox6 = new System.Windows.Forms.GroupBox();
		this.RidingBicycle_wild = new System.Windows.Forms.CheckBox();
		this.BlackFlute_wild = new System.Windows.Forms.CheckBox();
		this.OFUDA_wild = new System.Windows.Forms.CheckBox();
		this.WhiteFlute_wild = new System.Windows.Forms.CheckBox();
		this.y_dataGridView = new System.Windows.Forms.DataGridView();
		this.y_table_display = new System.Windows.Forms.Button();
		this.y_groupBox2 = new System.Windows.Forms.GroupBox();
		this.SID_wild = new System.Windows.Forms.NumericUpDown();
		this.TID_wild = new System.Windows.Forms.NumericUpDown();
		this.y_stats1 = new System.Windows.Forms.NumericUpDown();
		this.y_stats6 = new System.Windows.Forms.NumericUpDown();
		this.y_stats5 = new System.Windows.Forms.NumericUpDown();
		this.y_stats3 = new System.Windows.Forms.NumericUpDown();
		this.y_stats4 = new System.Windows.Forms.NumericUpDown();
		this.y_stats2 = new System.Windows.Forms.NumericUpDown();
		this.y_check_LvEnable = new System.Windows.Forms.CheckBox();
		this.y_shiny = new System.Windows.Forms.CheckBox();
		this.y_sex = new System.Windows.Forms.ComboBox();
		this.label46 = new System.Windows.Forms.Label();
		this.label49 = new System.Windows.Forms.Label();
		this.y_ability = new System.Windows.Forms.ComboBox();
		this.label104 = new System.Windows.Forms.Label();
		this.label125 = new System.Windows.Forms.Label();
		this.label126 = new System.Windows.Forms.Label();
		this.y_mezapaPower = new System.Windows.Forms.NumericUpDown();
		this.label127 = new System.Windows.Forms.Label();
		this.y_mezapaType = new System.Windows.Forms.ComboBox();
		this.y_search2 = new System.Windows.Forms.RadioButton();
		this.label43 = new System.Windows.Forms.Label();
		this.label45 = new System.Windows.Forms.Label();
		this.y_search1 = new System.Windows.Forms.RadioButton();
		this.label47 = new System.Windows.Forms.Label();
		this.label48 = new System.Windows.Forms.Label();
		this.label103 = new System.Windows.Forms.Label();
		this.label105 = new System.Windows.Forms.Label();
		this.y_pokedex = new System.Windows.Forms.ComboBox();
		this.NatureButton_wild = new System.Windows.Forms.Button();
		this.label107 = new System.Windows.Forms.Label();
		this.label108 = new System.Windows.Forms.Label();
		this.y_IVup6 = new System.Windows.Forms.NumericUpDown();
		this.y_IVlow1 = new System.Windows.Forms.NumericUpDown();
		this.label109 = new System.Windows.Forms.Label();
		this.y_IVlow2 = new System.Windows.Forms.NumericUpDown();
		this.y_IVlow3 = new System.Windows.Forms.NumericUpDown();
		this.y_IVup5 = new System.Windows.Forms.NumericUpDown();
		this.y_IVlow4 = new System.Windows.Forms.NumericUpDown();
		this.label110 = new System.Windows.Forms.Label();
		this.y_IVlow5 = new System.Windows.Forms.NumericUpDown();
		this.label111 = new System.Windows.Forms.Label();
		this.y_IVlow6 = new System.Windows.Forms.NumericUpDown();
		this.y_IVup4 = new System.Windows.Forms.NumericUpDown();
		this.label112 = new System.Windows.Forms.Label();
		this.label113 = new System.Windows.Forms.Label();
		this.y_IVup1 = new System.Windows.Forms.NumericUpDown();
		this.y_IVup3 = new System.Windows.Forms.NumericUpDown();
		this.label114 = new System.Windows.Forms.Label();
		this.label115 = new System.Windows.Forms.Label();
		this.y_IVup2 = new System.Windows.Forms.NumericUpDown();
		this.y_Lv = new System.Windows.Forms.NumericUpDown();
		this.y_groupBox1 = new System.Windows.Forms.GroupBox();
		this.PaintPanel_wild = new System.Windows.Forms.Panel();
		this.label151 = new System.Windows.Forms.Label();
		this.PaintSeedMaxFrameBox_wild = new System.Windows.Forms.NumericUpDown();
		this.PaintSeedMinFrameBox_wild = new System.Windows.Forms.NumericUpDown();
		this.label149 = new System.Windows.Forms.Label();
		this.FrameRange_wild = new System.Windows.Forms.NumericUpDown();
		this.TargetFrame_wild = new System.Windows.Forms.NumericUpDown();
		this.LastFrame_wild = new System.Windows.Forms.NumericUpDown();
		this.FirstFrame_wild = new System.Windows.Forms.NumericUpDown();
		this.SetFrameButton_wild = new System.Windows.Forms.Button();
		this.label180 = new System.Windows.Forms.Label();
		this.label181 = new System.Windows.Forms.Label();
		this.label182 = new System.Windows.Forms.Label();
		this.label116 = new System.Windows.Forms.Label();
		this.label117 = new System.Windows.Forms.Label();
		this.y_RSmax = new System.Windows.Forms.NumericUpDown();
		this.y_RSmin = new System.Windows.Forms.NumericUpDown();
		this.Method_wild = new System.Windows.Forms.ComboBox();
		this.y_Initialseed3 = new System.Windows.Forms.TextBox();
		this.ForMultipleSeed_wild = new System.Windows.Forms.RadioButton();
		this.label118 = new System.Windows.Forms.Label();
		this.ForRTC_wild = new System.Windows.Forms.RadioButton();
		this.y_Initialseed1 = new System.Windows.Forms.TextBox();
		this.ForSimpleSeed_wild = new System.Windows.Forms.RadioButton();
		this.label119 = new System.Windows.Forms.Label();
		this.label121 = new System.Windows.Forms.Label();
		this.label123 = new System.Windows.Forms.Label();
		this.y_cancel = new System.Windows.Forms.Button();
		this.y_listup = new System.Windows.Forms.Button();
		this.y_start = new System.Windows.Forms.Button();
		this.groupBox8 = new System.Windows.Forms.GroupBox();
		this.Encounter_OldRod = new System.Windows.Forms.RadioButton();
		this.Encounter_GoodRod = new System.Windows.Forms.RadioButton();
		this.Encounter_SuperRod = new System.Windows.Forms.RadioButton();
		this.Encounter_RockSmash = new System.Windows.Forms.RadioButton();
		this.Encounter_Surf = new System.Windows.Forms.RadioButton();
		this.Encounter_Grass = new System.Windows.Forms.RadioButton();
		this.dataGridView_table = new System.Windows.Forms.DataGridView();
		this.Column22 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Column23 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Column24 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Column25 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Column26 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Column27 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Column28 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Column29 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Column30 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Column31 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Column32 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Column33 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.groupBox7 = new System.Windows.Forms.GroupBox();
		this.ROM_LG = new System.Windows.Forms.RadioButton();
		this.ROM_S = new System.Windows.Forms.RadioButton();
		this.ROM_FR = new System.Windows.Forms.RadioButton();
		this.ROM_Em = new System.Windows.Forms.RadioButton();
		this.ROM_R = new System.Windows.Forms.RadioButton();
		this.groupBox9 = new System.Windows.Forms.GroupBox();
		this.MapBox = new System.Windows.Forms.ComboBox();
		this.y_groupBox3 = new System.Windows.Forms.GroupBox();
		this.label_pb = new System.Windows.Forms.Label();
		this.PokeBlockBox = new System.Windows.Forms.ComboBox();
		this.CCGender_wild = new System.Windows.Forms.ComboBox();
		this.label106 = new System.Windows.Forms.Label();
		this.SyncNature_wild = new System.Windows.Forms.ComboBox();
		this.FieldAbility_wild = new System.Windows.Forms.ComboBox();
		this.TabPage_stationary = new System.Windows.Forms.TabPage();
		this.groupBox5 = new System.Windows.Forms.GroupBox();
		this.LG_stationary = new System.Windows.Forms.RadioButton();
		this.FR_stationary = new System.Windows.Forms.RadioButton();
		this.Sapphire_stationary = new System.Windows.Forms.RadioButton();
		this.Em_stationary = new System.Windows.Forms.RadioButton();
		this.Ruby_stationary = new System.Windows.Forms.RadioButton();
		this.k_groupBox2 = new System.Windows.Forms.GroupBox();
		this.AbilityBox_stationary = new System.Windows.Forms.ComboBox();
		this.label192 = new System.Windows.Forms.Label();
		this.GenderBox_stationary = new System.Windows.Forms.ComboBox();
		this.label32 = new System.Windows.Forms.Label();
		this.panel1 = new System.Windows.Forms.Panel();
		this.label30 = new System.Windows.Forms.Label();
		this.k_stats1 = new System.Windows.Forms.NumericUpDown();
		this.k_stats5 = new System.Windows.Forms.NumericUpDown();
		this.k_IVup1 = new System.Windows.Forms.NumericUpDown();
		this.k_stats6 = new System.Windows.Forms.NumericUpDown();
		this.k_stats4 = new System.Windows.Forms.NumericUpDown();
		this.label37 = new System.Windows.Forms.Label();
		this.k_stats3 = new System.Windows.Forms.NumericUpDown();
		this.k_IVup4 = new System.Windows.Forms.NumericUpDown();
		this.k_stats2 = new System.Windows.Forms.NumericUpDown();
		this.k_IVlow6 = new System.Windows.Forms.NumericUpDown();
		this.k_IVlow5 = new System.Windows.Forms.NumericUpDown();
		this.label35 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.k_IVlow4 = new System.Windows.Forms.NumericUpDown();
		this.label5 = new System.Windows.Forms.Label();
		this.k_IVup5 = new System.Windows.Forms.NumericUpDown();
		this.k_IVlow3 = new System.Windows.Forms.NumericUpDown();
		this.k_IVlow2 = new System.Windows.Forms.NumericUpDown();
		this.label9 = new System.Windows.Forms.Label();
		this.label34 = new System.Windows.Forms.Label();
		this.label10 = new System.Windows.Forms.Label();
		this.k_IVlow1 = new System.Windows.Forms.NumericUpDown();
		this.k_IVup6 = new System.Windows.Forms.NumericUpDown();
		this.label28 = new System.Windows.Forms.Label();
		this.k_IVup2 = new System.Windows.Forms.NumericUpDown();
		this.label41 = new System.Windows.Forms.Label();
		this.k_IVup3 = new System.Windows.Forms.NumericUpDown();
		this.label42 = new System.Windows.Forms.Label();
		this.label38 = new System.Windows.Forms.Label();
		this.RSFLRoamingCheck = new System.Windows.Forms.CheckBox();
		this.SID_stationary = new System.Windows.Forms.NumericUpDown();
		this.TID_stationary = new System.Windows.Forms.NumericUpDown();
		this.k_search2 = new System.Windows.Forms.RadioButton();
		this.k_shiny = new System.Windows.Forms.CheckBox();
		this.label6 = new System.Windows.Forms.Label();
		this.k_search1 = new System.Windows.Forms.RadioButton();
		this.label27 = new System.Windows.Forms.Label();
		this.label29 = new System.Windows.Forms.Label();
		this.k_mezapaPower = new System.Windows.Forms.NumericUpDown();
		this.label31 = new System.Windows.Forms.Label();
		this.k_pokedex = new System.Windows.Forms.ComboBox();
		this.NatureButton_stationary = new System.Windows.Forms.Button();
		this.k_mezapaType = new System.Windows.Forms.ComboBox();
		this.label33 = new System.Windows.Forms.Label();
		this.k_Lv = new System.Windows.Forms.NumericUpDown();
		this.label36 = new System.Windows.Forms.Label();
		this.k_groupBox1 = new System.Windows.Forms.GroupBox();
		this.FrameRange_stationary = new System.Windows.Forms.NumericUpDown();
		this.TargetFrame_stationary = new System.Windows.Forms.NumericUpDown();
		this.LastFrame_stationary = new System.Windows.Forms.NumericUpDown();
		this.FirstFrame_stationary = new System.Windows.Forms.NumericUpDown();
		this.label2 = new System.Windows.Forms.Label();
		this.label44 = new System.Windows.Forms.Label();
		this.k_RSmax = new System.Windows.Forms.NumericUpDown();
		this.k_RSmin = new System.Windows.Forms.NumericUpDown();
		this.Method_stationary = new System.Windows.Forms.ComboBox();
		this.k_Initialseed3 = new System.Windows.Forms.TextBox();
		this.ForMultipleSeed_stationary = new System.Windows.Forms.RadioButton();
		this.label3 = new System.Windows.Forms.Label();
		this.ForRTC_stationary = new System.Windows.Forms.RadioButton();
		this.k_Initialseed1 = new System.Windows.Forms.TextBox();
		this.ForSimpleSeed_stationary = new System.Windows.Forms.RadioButton();
		this.label1 = new System.Windows.Forms.Label();
		this.SetFrameButton_stationary = new System.Windows.Forms.Button();
		this.label52 = new System.Windows.Forms.Label();
		this.label50 = new System.Windows.Forms.Label();
		this.label51 = new System.Windows.Forms.Label();
		this.label40 = new System.Windows.Forms.Label();
		this.label39 = new System.Windows.Forms.Label();
		this.k_dataGridView = new System.Windows.Forms.DataGridView();
		this.k_start = new System.Windows.Forms.Button();
		this.k_cancel = new System.Windows.Forms.Button();
		this.k_listup = new System.Windows.Forms.Button();
		this.tabControl1 = new System.Windows.Forms.TabControl();
		this.TabPage_backword = new System.Windows.Forms.TabPage();
		this.label148 = new System.Windows.Forms.Label();
		this.MaxLvBox_back = new System.Windows.Forms.NumericUpDown();
		this.MinLvBox_back = new System.Windows.Forms.NumericUpDown();
		this.checkLv_back = new System.Windows.Forms.CheckBox();
		this.checkSpecies_back = new System.Windows.Forms.CheckBox();
		this.PokemonBox_back = new System.Windows.Forms.ComboBox();
		this.EncounterTable_back = new System.Windows.Forms.DataGridView();
		this.dataGridViewTextBoxColumn30 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn57 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn66 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn67 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn68 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn69 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn70 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn71 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn72 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn73 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn74 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn75 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Map_back = new System.Windows.Forms.ComboBox();
		this.groupBox10 = new System.Windows.Forms.GroupBox();
		this.OldRod_back = new System.Windows.Forms.RadioButton();
		this.GoodRod_back = new System.Windows.Forms.RadioButton();
		this.SuperRod_back = new System.Windows.Forms.RadioButton();
		this.RockSmash_back = new System.Windows.Forms.RadioButton();
		this.Surf_back = new System.Windows.Forms.RadioButton();
		this.Grass_back = new System.Windows.Forms.RadioButton();
		this.groupBox11 = new System.Windows.Forms.GroupBox();
		this.LG_back = new System.Windows.Forms.RadioButton();
		this.Sapphire_back = new System.Windows.Forms.RadioButton();
		this.FR_back = new System.Windows.Forms.RadioButton();
		this.Em_back = new System.Windows.Forms.RadioButton();
		this.Ruby_back = new System.Windows.Forms.RadioButton();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.SID_back = new System.Windows.Forms.NumericUpDown();
		this.Method1_back = new System.Windows.Forms.CheckBox();
		this.label194 = new System.Windows.Forms.Label();
		this.TID_back = new System.Windows.Forms.NumericUpDown();
		this.Method2_back = new System.Windows.Forms.CheckBox();
		this.OnlyShiny_back = new System.Windows.Forms.CheckBox();
		this.Method3_back = new System.Windows.Forms.CheckBox();
		this.label197 = new System.Windows.Forms.Label();
		this.label196 = new System.Windows.Forms.Label();
		this.HiddenPowerPower_back = new System.Windows.Forms.NumericUpDown();
		this.Amax_back = new System.Windows.Forms.NumericUpDown();
		this.label198 = new System.Windows.Forms.Label();
		this.label191 = new System.Windows.Forms.Label();
		this.HiddenPowerType_back = new System.Windows.Forms.ComboBox();
		this.label190 = new System.Windows.Forms.Label();
		this.Bmax_back = new System.Windows.Forms.NumericUpDown();
		this.NatureButton_back = new System.Windows.Forms.Button();
		this.Hmax_back = new System.Windows.Forms.NumericUpDown();
		this.label200 = new System.Windows.Forms.Label();
		this.label189 = new System.Windows.Forms.Label();
		this.label154 = new System.Windows.Forms.Label();
		this.label188 = new System.Windows.Forms.Label();
		this.label161 = new System.Windows.Forms.Label();
		this.Cmax_back = new System.Windows.Forms.NumericUpDown();
		this.label176 = new System.Windows.Forms.Label();
		this.Smin_back = new System.Windows.Forms.NumericUpDown();
		this.label183 = new System.Windows.Forms.Label();
		this.Dmin_back = new System.Windows.Forms.NumericUpDown();
		this.label184 = new System.Windows.Forms.Label();
		this.label187 = new System.Windows.Forms.Label();
		this.label185 = new System.Windows.Forms.Label();
		this.Cmin_back = new System.Windows.Forms.NumericUpDown();
		this.Smax_back = new System.Windows.Forms.NumericUpDown();
		this.Dmax_back = new System.Windows.Forms.NumericUpDown();
		this.Hmin_back = new System.Windows.Forms.NumericUpDown();
		this.Bmin_back = new System.Windows.Forms.NumericUpDown();
		this.label186 = new System.Windows.Forms.Label();
		this.Amin_back = new System.Windows.Forms.NumericUpDown();
		this.dataGridView1_back = new System.Windows.Forms.DataGridView();
		this.StartSeedColumn_back = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.GenerateSeedColumn_back = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.FieldAbilityColumn_back = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.MethodColumn_back = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.SlotColumn_back = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.LvColumn_back = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.PIDColumn_back = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.NatureColumn_back = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.HColumn_back = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.AColumn_back = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.BColumn_back = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.CColumn_back = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.DColumn_back = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.SColumn_back = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.GenderColumn_back = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.AbilityColumn_back = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.HiddenPowerColumn_back = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.contextMenuStrip6 = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
		this.CalcButton_back = new System.Windows.Forms.Button();
		this.DataCount_back = new System.Windows.Forms.Label();
		this.Wild_back = new System.Windows.Forms.RadioButton();
		this.Stationary_back = new System.Windows.Forms.RadioButton();
		this.TabPage_other = new System.Windows.Forms.TabPage();
		this.groupBox15 = new System.Windows.Forms.GroupBox();
		this.button8 = new System.Windows.Forms.Button();
		this.groupBox12 = new System.Windows.Forms.GroupBox();
		this.button7 = new System.Windows.Forms.Button();
		this.groupBox14 = new System.Windows.Forms.GroupBox();
		this.button6 = new System.Windows.Forms.Button();
		this.groupBox13 = new System.Windows.Forms.GroupBox();
		this.button5 = new System.Windows.Forms.Button();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.button3 = new System.Windows.Forms.Button();
		this.groupBox3 = new System.Windows.Forms.GroupBox();
		this.button2 = new System.Windows.Forms.Button();
		this.poyo = new System.Windows.Forms.Button();
		this.groupBox4 = new System.Windows.Forms.GroupBox();
		this.L_button = new System.Windows.Forms.Button();
		this.Language = new System.Windows.Forms.ComboBox();
		this.PaintPanel_stationary = new System.Windows.Forms.Panel();
		this.label193 = new System.Windows.Forms.Label();
		this.PaintSeedMaxFrameBox_stationary = new System.Windows.Forms.NumericUpDown();
		this.PaintSeedMinFrameBox_stationary = new System.Windows.Forms.NumericUpDown();
		this.label195 = new System.Windows.Forms.Label();
		this.NaturePanel_stationary = new _3genSearch.NaturePanel();
		this.NaturePanel_wild = new _3genSearch.NaturePanel();
		this.NaturePanel_EggPID = new _3genSearch.NaturePanel();
		this.NaturePanel_back = new _3genSearch.NaturePanel();
		this.contextMenuStrip1.SuspendLayout();
		this.contextMenuStrip2.SuspendLayout();
		this.contextMenuStrip3.SuspendLayout();
		this.contextMenuStrip4.SuspendLayout();
		this.contextMenuStrip5.SuspendLayout();
		this.TabPage_Egg.SuspendLayout();
		this.tabControl2.SuspendLayout();
		this.TabPage_EggPID.SuspendLayout();
		this.es_groupBox2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.SID_EggPID).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TID_EggPID).BeginInit();
		this.es_groupBox1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.DiffMax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DiffMin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.LastFrame_EggPID).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.FirstFrame_EggPID).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.es_dataGridView).BeginInit();
		this.TabPage_EggIVs.SuspendLayout();
		this.ek_groupBox3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.ek_stats6).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_stats5).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_stats4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_stats3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_stats2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_stats1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_mezapaPower).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVup6).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVlow1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVlow2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVlow3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVup5).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVlow4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVlow5).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVlow6).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVup4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVup1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVup3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVup2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_Lv).BeginInit();
		this.ek_groupBox2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.FrameRange_EggIVs).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TargetFrame_EggIVs).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.LastFrame_EggIVs).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.FirstFrame_EggIVs).BeginInit();
		this.ek_groupBox1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pre_parent6).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pre_parent5).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pre_parent4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pre_parent3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pre_parent2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.post_parent6).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.post_parent5).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.post_parent4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.post_parent3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.post_parent2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.post_parent1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pre_parent1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ek_dataGridView).BeginInit();
		this.TabPage_ID.SuspendLayout();
		this.ID_groupBox1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.FrameRange_ID).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TargetFrame_ID).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.LastFrame_ID).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.FirstFrame_ID).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ID_RSmax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ID_RSmin).BeginInit();
		this.ID_groupBox2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.TID_ID).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SID_ID).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ID_dataGridView).BeginInit();
		this.TabPage_wild.SuspendLayout();
		this.panel2.SuspendLayout();
		this.groupBox6.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.y_dataGridView).BeginInit();
		this.y_groupBox2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.SID_wild).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TID_wild).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_stats1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_stats6).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_stats5).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_stats3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_stats4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_stats2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_mezapaPower).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVup6).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVlow1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVlow2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVlow3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVup5).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVlow4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVlow5).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVlow6).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVup4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVup1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVup3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVup2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_Lv).BeginInit();
		this.y_groupBox1.SuspendLayout();
		this.PaintPanel_wild.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.PaintSeedMaxFrameBox_wild).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.PaintSeedMinFrameBox_wild).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.FrameRange_wild).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TargetFrame_wild).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.LastFrame_wild).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.FirstFrame_wild).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_RSmax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.y_RSmin).BeginInit();
		this.groupBox8.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dataGridView_table).BeginInit();
		this.groupBox7.SuspendLayout();
		this.groupBox9.SuspendLayout();
		this.y_groupBox3.SuspendLayout();
		this.TabPage_stationary.SuspendLayout();
		this.groupBox5.SuspendLayout();
		this.k_groupBox2.SuspendLayout();
		this.panel1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.k_stats1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_stats5).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVup1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_stats6).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_stats4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_stats3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVup4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_stats2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVlow6).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVlow5).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVlow4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVup5).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVlow3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVlow2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVlow1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVup6).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVup2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVup3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SID_stationary).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TID_stationary).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_mezapaPower).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_Lv).BeginInit();
		this.k_groupBox1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.FrameRange_stationary).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TargetFrame_stationary).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.LastFrame_stationary).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.FirstFrame_stationary).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_RSmax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_RSmin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.k_dataGridView).BeginInit();
		this.tabControl1.SuspendLayout();
		this.TabPage_backword.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.MaxLvBox_back).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.MinLvBox_back).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.EncounterTable_back).BeginInit();
		this.groupBox10.SuspendLayout();
		this.groupBox11.SuspendLayout();
		this.groupBox2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.SID_back).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TID_back).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.HiddenPowerPower_back).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Amax_back).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Bmax_back).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Hmax_back).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Cmax_back).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Smin_back).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Dmin_back).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Cmin_back).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Smax_back).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Dmax_back).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Hmin_back).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Bmin_back).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Amin_back).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dataGridView1_back).BeginInit();
		this.contextMenuStrip6.SuspendLayout();
		this.TabPage_other.SuspendLayout();
		this.groupBox15.SuspendLayout();
		this.groupBox12.SuspendLayout();
		this.groupBox14.SuspendLayout();
		this.groupBox13.SuspendLayout();
		this.groupBox1.SuspendLayout();
		this.groupBox3.SuspendLayout();
		this.groupBox4.SuspendLayout();
		this.PaintPanel_stationary.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.PaintSeedMaxFrameBox_stationary).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.PaintSeedMinFrameBox_stationary).BeginInit();
		base.SuspendLayout();
		this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
		this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.copyToolStripMenuItem, this.SelectAllToolStripMenuItem });
		this.contextMenuStrip1.Name = "contextMenuStrip1";
		this.contextMenuStrip1.Size = new System.Drawing.Size(120, 48);
		this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
		this.copyToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
		this.copyToolStripMenuItem.Text = "コピー";
		this.copyToolStripMenuItem.Click += new System.EventHandler(copyToolStripMenuItem_Click);
		this.SelectAllToolStripMenuItem.Name = "SelectAllToolStripMenuItem";
		this.SelectAllToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
		this.SelectAllToolStripMenuItem.Text = "全て選択";
		this.SelectAllToolStripMenuItem.Click += new System.EventHandler(SelectAllToolStripMenuItem_Click);
		this.contextMenuStrip2.ImageScalingSize = new System.Drawing.Size(32, 32);
		this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.copyToolStripMenuItem2, this.SelectAllToolStripMenuItem2 });
		this.contextMenuStrip2.Name = "contextMenuStrip1";
		this.contextMenuStrip2.Size = new System.Drawing.Size(120, 48);
		this.copyToolStripMenuItem2.Name = "copyToolStripMenuItem2";
		this.copyToolStripMenuItem2.Size = new System.Drawing.Size(119, 22);
		this.copyToolStripMenuItem2.Text = "コピー";
		this.copyToolStripMenuItem2.Click += new System.EventHandler(copyToolStripMenuItem2_Click);
		this.SelectAllToolStripMenuItem2.Name = "SelectAllToolStripMenuItem2";
		this.SelectAllToolStripMenuItem2.Size = new System.Drawing.Size(119, 22);
		this.SelectAllToolStripMenuItem2.Text = "全て選択";
		this.SelectAllToolStripMenuItem2.Click += new System.EventHandler(SelectAllToolStripMenuItem2_Click);
		this.contextMenuStrip3.ImageScalingSize = new System.Drawing.Size(32, 32);
		this.contextMenuStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.copyToolStripMenuItem3, this.SelectAllToolStripMenuItem3 });
		this.contextMenuStrip3.Name = "contextMenuStrip1";
		this.contextMenuStrip3.Size = new System.Drawing.Size(120, 48);
		this.copyToolStripMenuItem3.Name = "copyToolStripMenuItem3";
		this.copyToolStripMenuItem3.Size = new System.Drawing.Size(119, 22);
		this.copyToolStripMenuItem3.Text = "コピー";
		this.copyToolStripMenuItem3.Click += new System.EventHandler(copyToolStripMenuItem3_Click);
		this.SelectAllToolStripMenuItem3.Name = "SelectAllToolStripMenuItem3";
		this.SelectAllToolStripMenuItem3.Size = new System.Drawing.Size(119, 22);
		this.SelectAllToolStripMenuItem3.Text = "全て選択";
		this.SelectAllToolStripMenuItem3.Click += new System.EventHandler(SelectAllToolStripMenuItem3_Click);
		this.contextMenuStrip4.ImageScalingSize = new System.Drawing.Size(32, 32);
		this.contextMenuStrip4.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.copyToolStripMenuItem4, this.SelectAllToolStripMenuItem4 });
		this.contextMenuStrip4.Name = "contextMenuStrip1";
		this.contextMenuStrip4.Size = new System.Drawing.Size(120, 48);
		this.copyToolStripMenuItem4.Name = "copyToolStripMenuItem4";
		this.copyToolStripMenuItem4.Size = new System.Drawing.Size(119, 22);
		this.copyToolStripMenuItem4.Text = "コピー";
		this.copyToolStripMenuItem4.Click += new System.EventHandler(copyToolStripMenuItem4_Click);
		this.SelectAllToolStripMenuItem4.Name = "SelectAllToolStripMenuItem4";
		this.SelectAllToolStripMenuItem4.Size = new System.Drawing.Size(119, 22);
		this.SelectAllToolStripMenuItem4.Text = "全て選択";
		this.SelectAllToolStripMenuItem4.Click += new System.EventHandler(SelectAllToolStripMenuItem4_Click);
		this.contextMenuStrip5.ImageScalingSize = new System.Drawing.Size(32, 32);
		this.contextMenuStrip5.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.copyToolStripMenuItem5, this.SelectAllToolStripMenuItem5 });
		this.contextMenuStrip5.Name = "contextMenuStrip1";
		this.contextMenuStrip5.Size = new System.Drawing.Size(120, 48);
		this.copyToolStripMenuItem5.Name = "copyToolStripMenuItem5";
		this.copyToolStripMenuItem5.Size = new System.Drawing.Size(119, 22);
		this.copyToolStripMenuItem5.Text = "コピー";
		this.copyToolStripMenuItem5.Click += new System.EventHandler(copyToolStripMenuItem5_Click);
		this.SelectAllToolStripMenuItem5.Name = "SelectAllToolStripMenuItem5";
		this.SelectAllToolStripMenuItem5.Size = new System.Drawing.Size(119, 22);
		this.SelectAllToolStripMenuItem5.Text = "全て選択";
		this.SelectAllToolStripMenuItem5.Click += new System.EventHandler(SelectAllToolStripMenuItem5_Click);
		this.dataGridViewTextBoxColumn9.HeaderText = "S";
		this.dataGridViewTextBoxColumn9.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
		this.dataGridViewTextBoxColumn9.Width = 27;
		this.dataGridViewTextBoxColumn8.HeaderText = "D";
		this.dataGridViewTextBoxColumn8.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
		this.dataGridViewTextBoxColumn8.Width = 27;
		this.dataGridViewTextBoxColumn7.HeaderText = "C";
		this.dataGridViewTextBoxColumn7.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
		this.dataGridViewTextBoxColumn7.Width = 27;
		this.dataGridViewTextBoxColumn6.HeaderText = "B";
		this.dataGridViewTextBoxColumn6.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
		this.dataGridViewTextBoxColumn6.Width = 27;
		this.dataGridViewTextBoxColumn5.HeaderText = "A";
		this.dataGridViewTextBoxColumn5.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
		this.dataGridViewTextBoxColumn5.Width = 27;
		this.dataGridViewTextBoxColumn4.HeaderText = "H";
		this.dataGridViewTextBoxColumn4.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
		this.dataGridViewTextBoxColumn4.Width = 27;
		this.dataGridViewTextBoxColumn3.HeaderText = "性格";
		this.dataGridViewTextBoxColumn3.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
		this.dataGridViewTextBoxColumn3.Width = 77;
		this.dataGridViewTextBoxColumn2.HeaderText = "乱数値";
		this.dataGridViewTextBoxColumn2.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
		this.dataGridViewTextBoxColumn2.Width = 77;
		this.dataGridViewTextBoxColumn1.HeaderText = "F";
		this.dataGridViewTextBoxColumn1.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
		this.dataGridViewTextBoxColumn1.Width = 50;
		this.label68.AutoSize = true;
		this.label68.Location = new System.Drawing.Point(6, 59);
		this.label68.Name = "label68";
		this.label68.Size = new System.Drawing.Size(29, 12);
		this.label68.TabIndex = 35;
		this.label67.AutoSize = true;
		this.label67.Location = new System.Drawing.Point(7, 89);
		this.label67.Name = "label67";
		this.label67.Size = new System.Drawing.Size(29, 12);
		this.label67.TabIndex = 36;
		this.textBox35.Location = new System.Drawing.Point(61, 56);
		this.textBox35.Name = "textBox35";
		this.textBox35.Size = new System.Drawing.Size(23, 19);
		this.textBox35.TabIndex = 42;
		this.label66.AutoSize = true;
		this.label66.Location = new System.Drawing.Point(42, 59);
		this.label66.Name = "label66";
		this.label66.Size = new System.Drawing.Size(13, 12);
		this.label66.TabIndex = 38;
		this.textBox34.Location = new System.Drawing.Point(109, 56);
		this.textBox34.Name = "textBox34";
		this.textBox34.Size = new System.Drawing.Size(23, 19);
		this.textBox34.TabIndex = 43;
		this.label65.AutoSize = true;
		this.label65.Location = new System.Drawing.Point(90, 59);
		this.label65.Name = "label65";
		this.label65.Size = new System.Drawing.Size(13, 12);
		this.label65.TabIndex = 40;
		this.textBox33.Location = new System.Drawing.Point(157, 56);
		this.textBox33.Name = "textBox33";
		this.textBox33.Size = new System.Drawing.Size(23, 19);
		this.textBox33.TabIndex = 44;
		this.label64.AutoSize = true;
		this.label64.Location = new System.Drawing.Point(138, 59);
		this.label64.Name = "label64";
		this.label64.Size = new System.Drawing.Size(13, 12);
		this.label64.TabIndex = 42;
		this.textBox32.Location = new System.Drawing.Point(205, 56);
		this.textBox32.Name = "textBox32";
		this.textBox32.Size = new System.Drawing.Size(23, 19);
		this.textBox32.TabIndex = 45;
		this.label63.AutoSize = true;
		this.label63.Location = new System.Drawing.Point(186, 59);
		this.label63.Name = "label63";
		this.label63.Size = new System.Drawing.Size(13, 12);
		this.label63.TabIndex = 44;
		this.textBox31.Location = new System.Drawing.Point(253, 56);
		this.textBox31.Name = "textBox31";
		this.textBox31.Size = new System.Drawing.Size(23, 19);
		this.textBox31.TabIndex = 46;
		this.label62.AutoSize = true;
		this.label62.Location = new System.Drawing.Point(234, 59);
		this.label62.Name = "label62";
		this.label62.Size = new System.Drawing.Size(13, 12);
		this.label62.TabIndex = 46;
		this.textBox30.Location = new System.Drawing.Point(301, 56);
		this.textBox30.Name = "textBox30";
		this.textBox30.Size = new System.Drawing.Size(23, 19);
		this.textBox30.TabIndex = 47;
		this.label61.AutoSize = true;
		this.label61.Location = new System.Drawing.Point(282, 59);
		this.label61.Name = "label61";
		this.label61.Size = new System.Drawing.Size(12, 12);
		this.label61.TabIndex = 48;
		this.textBox29.Location = new System.Drawing.Point(61, 86);
		this.textBox29.Name = "textBox29";
		this.textBox29.Size = new System.Drawing.Size(23, 19);
		this.textBox29.TabIndex = 48;
		this.label60.AutoSize = true;
		this.label60.Location = new System.Drawing.Point(42, 89);
		this.label60.Name = "label60";
		this.label60.Size = new System.Drawing.Size(13, 12);
		this.label60.TabIndex = 50;
		this.textBox28.Location = new System.Drawing.Point(109, 86);
		this.textBox28.Name = "textBox28";
		this.textBox28.Size = new System.Drawing.Size(23, 19);
		this.textBox28.TabIndex = 49;
		this.label59.AutoSize = true;
		this.label59.Location = new System.Drawing.Point(90, 89);
		this.label59.Name = "label59";
		this.label59.Size = new System.Drawing.Size(13, 12);
		this.label59.TabIndex = 52;
		this.textBox27.Location = new System.Drawing.Point(157, 86);
		this.textBox27.Name = "textBox27";
		this.textBox27.Size = new System.Drawing.Size(23, 19);
		this.textBox27.TabIndex = 50;
		this.label58.AutoSize = true;
		this.label58.Location = new System.Drawing.Point(138, 89);
		this.label58.Name = "label58";
		this.label58.Size = new System.Drawing.Size(13, 12);
		this.label58.TabIndex = 54;
		this.textBox26.Location = new System.Drawing.Point(205, 86);
		this.textBox26.Name = "textBox26";
		this.textBox26.Size = new System.Drawing.Size(23, 19);
		this.textBox26.TabIndex = 51;
		this.label57.AutoSize = true;
		this.label57.Location = new System.Drawing.Point(186, 89);
		this.label57.Name = "label57";
		this.label57.Size = new System.Drawing.Size(13, 12);
		this.label57.TabIndex = 56;
		this.textBox25.Location = new System.Drawing.Point(253, 86);
		this.textBox25.Name = "textBox25";
		this.textBox25.Size = new System.Drawing.Size(23, 19);
		this.textBox25.TabIndex = 52;
		this.label56.AutoSize = true;
		this.label56.Location = new System.Drawing.Point(234, 89);
		this.label56.Name = "label56";
		this.label56.Size = new System.Drawing.Size(13, 12);
		this.label56.TabIndex = 58;
		this.textBox24.Location = new System.Drawing.Point(301, 86);
		this.textBox24.Name = "textBox24";
		this.textBox24.Size = new System.Drawing.Size(23, 19);
		this.textBox24.TabIndex = 53;
		this.label55.AutoSize = true;
		this.label55.Location = new System.Drawing.Point(7, 27);
		this.label55.Name = "label55";
		this.label55.Size = new System.Drawing.Size(29, 12);
		this.label55.TabIndex = 66;
		this.label54.AutoSize = true;
		this.label54.Location = new System.Drawing.Point(282, 89);
		this.label54.Name = "label54";
		this.label54.Size = new System.Drawing.Size(12, 12);
		this.label54.TabIndex = 60;
		this.comboBox6.FormattingEnabled = true;
		this.comboBox6.Location = new System.Drawing.Point(42, 24);
		this.comboBox6.Name = "comboBox6";
		this.comboBox6.Size = new System.Drawing.Size(90, 20);
		this.comboBox6.TabIndex = 65;
		this.textBox23.Location = new System.Drawing.Point(25, 56);
		this.textBox23.Name = "textBox23";
		this.textBox23.Size = new System.Drawing.Size(29, 19);
		this.textBox23.TabIndex = 58;
		this.label53.AutoSize = true;
		this.label53.Location = new System.Drawing.Point(6, 59);
		this.label53.Name = "label53";
		this.label53.Size = new System.Drawing.Size(13, 12);
		this.label53.TabIndex = 50;
		this.label26.AutoSize = true;
		this.label26.Location = new System.Drawing.Point(60, 59);
		this.label26.Name = "label26";
		this.label26.Size = new System.Drawing.Size(13, 12);
		this.label26.TabIndex = 52;
		this.label25.AutoSize = true;
		this.label25.Location = new System.Drawing.Point(114, 59);
		this.label25.Name = "label25";
		this.label25.Size = new System.Drawing.Size(13, 12);
		this.label25.TabIndex = 54;
		this.label24.AutoSize = true;
		this.label24.Location = new System.Drawing.Point(168, 59);
		this.label24.Name = "label24";
		this.label24.Size = new System.Drawing.Size(13, 12);
		this.label24.TabIndex = 56;
		this.label23.AutoSize = true;
		this.label23.Location = new System.Drawing.Point(222, 59);
		this.label23.Name = "label23";
		this.label23.Size = new System.Drawing.Size(13, 12);
		this.label23.TabIndex = 58;
		this.label22.AutoSize = true;
		this.label22.Location = new System.Drawing.Point(276, 59);
		this.label22.Name = "label22";
		this.label22.Size = new System.Drawing.Size(12, 12);
		this.label22.TabIndex = 60;
		this.comboBox5.FormattingEnabled = true;
		this.comboBox5.Location = new System.Drawing.Point(41, 22);
		this.comboBox5.Name = "comboBox5";
		this.comboBox5.Size = new System.Drawing.Size(83, 20);
		this.comboBox5.TabIndex = 55;
		this.label21.AutoSize = true;
		this.label21.Location = new System.Drawing.Point(6, 25);
		this.label21.Name = "label21";
		this.label21.Size = new System.Drawing.Size(29, 12);
		this.label21.TabIndex = 62;
		this.label20.AutoSize = true;
		this.label20.Location = new System.Drawing.Point(136, 25);
		this.label20.Name = "label20";
		this.label20.Size = new System.Drawing.Size(29, 12);
		this.label20.TabIndex = 64;
		this.comboBox4.FormattingEnabled = true;
		this.comboBox4.Location = new System.Drawing.Point(171, 22);
		this.comboBox4.Name = "comboBox4";
		this.comboBox4.Size = new System.Drawing.Size(90, 20);
		this.comboBox4.TabIndex = 56;
		this.label19.AutoSize = true;
		this.label19.Location = new System.Drawing.Point(273, 25);
		this.label19.Name = "label19";
		this.label19.Size = new System.Drawing.Size(17, 12);
		this.label19.TabIndex = 65;
		this.textBox22.Location = new System.Drawing.Point(297, 22);
		this.textBox22.Name = "textBox22";
		this.textBox22.Size = new System.Drawing.Size(23, 19);
		this.textBox22.TabIndex = 57;
		this.textBox21.Location = new System.Drawing.Point(79, 56);
		this.textBox21.Name = "textBox21";
		this.textBox21.Size = new System.Drawing.Size(29, 19);
		this.textBox21.TabIndex = 59;
		this.textBox20.Location = new System.Drawing.Point(133, 56);
		this.textBox20.Name = "textBox20";
		this.textBox20.Size = new System.Drawing.Size(29, 19);
		this.textBox20.TabIndex = 60;
		this.textBox19.Location = new System.Drawing.Point(187, 56);
		this.textBox19.Name = "textBox19";
		this.textBox19.Size = new System.Drawing.Size(29, 19);
		this.textBox19.TabIndex = 61;
		this.textBox18.Location = new System.Drawing.Point(241, 56);
		this.textBox18.Name = "textBox18";
		this.textBox18.Size = new System.Drawing.Size(29, 19);
		this.textBox18.TabIndex = 62;
		this.textBox17.Location = new System.Drawing.Point(294, 56);
		this.textBox17.Name = "textBox17";
		this.textBox17.Size = new System.Drawing.Size(29, 19);
		this.textBox17.TabIndex = 63;
		this.checkBox6.AutoSize = true;
		this.checkBox6.Location = new System.Drawing.Point(225, 18);
		this.checkBox6.Name = "checkBox6";
		this.checkBox6.Size = new System.Drawing.Size(67, 16);
		this.checkBox6.TabIndex = 30;
		this.checkBox6.Text = "Method4";
		this.checkBox6.UseVisualStyleBackColor = true;
		this.checkBox5.AutoSize = true;
		this.checkBox5.Location = new System.Drawing.Point(152, 18);
		this.checkBox5.Name = "checkBox5";
		this.checkBox5.Size = new System.Drawing.Size(67, 16);
		this.checkBox5.TabIndex = 29;
		this.checkBox5.Text = "Method3";
		this.checkBox5.UseVisualStyleBackColor = true;
		this.checkBox4.AutoSize = true;
		this.checkBox4.Location = new System.Drawing.Point(79, 18);
		this.checkBox4.Name = "checkBox4";
		this.checkBox4.Size = new System.Drawing.Size(67, 16);
		this.checkBox4.TabIndex = 28;
		this.checkBox4.Text = "Method2";
		this.checkBox4.UseVisualStyleBackColor = true;
		this.checkBox3.AutoSize = true;
		this.checkBox3.Location = new System.Drawing.Point(6, 18);
		this.checkBox3.Name = "checkBox3";
		this.checkBox3.Size = new System.Drawing.Size(67, 16);
		this.checkBox3.TabIndex = 27;
		this.checkBox3.Text = "Method1";
		this.checkBox3.UseVisualStyleBackColor = true;
		this.textBox16.Location = new System.Drawing.Point(160, 162);
		this.textBox16.MaxLength = 6;
		this.textBox16.Name = "textBox16";
		this.textBox16.Size = new System.Drawing.Size(60, 19);
		this.textBox16.TabIndex = 38;
		this.textBox15.Location = new System.Drawing.Point(75, 162);
		this.textBox15.MaxLength = 6;
		this.textBox15.Name = "textBox15";
		this.textBox15.Size = new System.Drawing.Size(60, 19);
		this.textBox15.TabIndex = 37;
		this.label18.Location = new System.Drawing.Point(141, 165);
		this.label18.Name = "label18";
		this.label18.Size = new System.Drawing.Size(18, 16);
		this.label18.TabIndex = 61;
		this.label17.AutoSize = true;
		this.label17.Location = new System.Drawing.Point(48, 165);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(12, 12);
		this.label17.TabIndex = 64;
		this.textBox14.Location = new System.Drawing.Point(160, 194);
		this.textBox14.MaxLength = 6;
		this.textBox14.Name = "textBox14";
		this.textBox14.Size = new System.Drawing.Size(31, 19);
		this.textBox14.TabIndex = 40;
		this.textBox13.Location = new System.Drawing.Point(72, 194);
		this.textBox13.MaxLength = 6;
		this.textBox13.Name = "textBox13";
		this.textBox13.Size = new System.Drawing.Size(60, 19);
		this.textBox13.TabIndex = 39;
		this.label16.Location = new System.Drawing.Point(141, 197);
		this.label16.Name = "label16";
		this.label16.Size = new System.Drawing.Size(18, 16);
		this.label16.TabIndex = 65;
		this.label15.AutoSize = true;
		this.label15.Location = new System.Drawing.Point(25, 197);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(36, 12);
		this.label15.TabIndex = 68;
		this.label14.AutoSize = true;
		this.label14.Location = new System.Drawing.Point(231, 197);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(73, 12);
		this.label14.TabIndex = 69;
		this.button1.Location = new System.Drawing.Point(197, 192);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(23, 23);
		this.button1.TabIndex = 41;
		this.button1.Text = "↑";
		this.button1.UseVisualStyleBackColor = true;
		this.label13.AutoSize = true;
		this.label13.Location = new System.Drawing.Point(7, 25);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(53, 12);
		this.label13.TabIndex = 70;
		this.radioButton6.AutoSize = true;
		this.radioButton6.Checked = true;
		this.radioButton6.Location = new System.Drawing.Point(75, 23);
		this.radioButton6.Name = "radioButton6";
		this.radioButton6.Size = new System.Drawing.Size(35, 16);
		this.radioButton6.TabIndex = 71;
		this.radioButton6.TabStop = true;
		this.radioButton6.Text = "0x";
		this.radioButton6.UseVisualStyleBackColor = true;
		this.textBox12.Location = new System.Drawing.Point(108, 22);
		this.textBox12.Name = "textBox12";
		this.textBox12.Size = new System.Drawing.Size(29, 19);
		this.textBox12.TabIndex = 74;
		this.radioButton5.AutoSize = true;
		this.radioButton5.Location = new System.Drawing.Point(75, 48);
		this.radioButton5.Name = "radioButton5";
		this.radioButton5.Size = new System.Drawing.Size(33, 16);
		this.radioButton5.TabIndex = 75;
		this.radioButton5.Text = " a";
		this.radioButton5.UseVisualStyleBackColor = true;
		this.textBox11.Location = new System.Drawing.Point(95, 48);
		this.textBox11.Name = "textBox11";
		this.textBox11.Size = new System.Drawing.Size(29, 19);
		this.textBox11.TabIndex = 76;
		this.label12.Location = new System.Drawing.Point(129, 51);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(37, 16);
		this.label12.TabIndex = 77;
		this.textBox10.Location = new System.Drawing.Point(167, 48);
		this.textBox10.Name = "textBox10";
		this.textBox10.Size = new System.Drawing.Size(29, 19);
		this.textBox10.TabIndex = 78;
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(202, 51);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(17, 12);
		this.label11.TabIndex = 79;
		this.radioButton4.AutoSize = true;
		this.radioButton4.Location = new System.Drawing.Point(75, 76);
		this.radioButton4.Name = "radioButton4";
		this.radioButton4.Size = new System.Drawing.Size(35, 16);
		this.radioButton4.TabIndex = 80;
		this.radioButton4.Text = "0x";
		this.radioButton4.UseVisualStyleBackColor = true;
		this.textBox9.Location = new System.Drawing.Point(116, 73);
		this.textBox9.Multiline = true;
		this.textBox9.Name = "textBox9";
		this.textBox9.Size = new System.Drawing.Size(80, 70);
		this.textBox9.TabIndex = 81;
		this.checkBox2.AutoSize = true;
		this.checkBox2.Checked = true;
		this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkBox2.Location = new System.Drawing.Point(224, 234);
		this.checkBox2.Name = "checkBox2";
		this.checkBox2.Size = new System.Drawing.Size(103, 16);
		this.checkBox2.TabIndex = 84;
		this.checkBox2.Text = "色違いのみ出力";
		this.checkBox2.UseVisualStyleBackColor = true;
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(12, 235);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(23, 12);
		this.label8.TabIndex = 85;
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(114, 235);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(23, 12);
		this.label7.TabIndex = 86;
		this.textBox8.Location = new System.Drawing.Point(41, 232);
		this.textBox8.MaxLength = 5;
		this.textBox8.Name = "textBox8";
		this.textBox8.Size = new System.Drawing.Size(60, 19);
		this.textBox8.TabIndex = 82;
		this.textBox7.Location = new System.Drawing.Point(138, 232);
		this.textBox7.MaxLength = 5;
		this.textBox7.Name = "textBox7";
		this.textBox7.Size = new System.Drawing.Size(60, 19);
		this.textBox7.TabIndex = 83;
		this.radioButton7.AutoSize = true;
		this.radioButton7.Checked = true;
		this.radioButton7.Location = new System.Drawing.Point(12, 18);
		this.radioButton7.Name = "radioButton7";
		this.radioButton7.Size = new System.Drawing.Size(31, 16);
		this.radioButton7.TabIndex = 72;
		this.radioButton7.TabStop = true;
		this.radioButton7.Text = "R";
		this.radioButton7.UseVisualStyleBackColor = true;
		this.radioButton8.AutoSize = true;
		this.radioButton8.Location = new System.Drawing.Point(54, 18);
		this.radioButton8.Name = "radioButton8";
		this.radioButton8.Size = new System.Drawing.Size(30, 16);
		this.radioButton8.TabIndex = 73;
		this.radioButton8.Text = "S";
		this.radioButton8.UseVisualStyleBackColor = true;
		this.radioButton9.AutoSize = true;
		this.radioButton9.Location = new System.Drawing.Point(92, 18);
		this.radioButton9.Name = "radioButton9";
		this.radioButton9.Size = new System.Drawing.Size(39, 16);
		this.radioButton9.TabIndex = 74;
		this.radioButton9.Text = "Em";
		this.radioButton9.UseVisualStyleBackColor = true;
		this.radioButton10.AutoSize = true;
		this.radioButton10.Location = new System.Drawing.Point(133, 18);
		this.radioButton10.Name = "radioButton10";
		this.radioButton10.Size = new System.Drawing.Size(30, 16);
		this.radioButton10.TabIndex = 75;
		this.radioButton10.Text = "F";
		this.radioButton10.UseVisualStyleBackColor = true;
		this.radioButton11.AutoSize = true;
		this.radioButton11.Location = new System.Drawing.Point(172, 18);
		this.radioButton11.Name = "radioButton11";
		this.radioButton11.Size = new System.Drawing.Size(29, 16);
		this.radioButton11.TabIndex = 76;
		this.radioButton11.Text = "L";
		this.radioButton11.UseVisualStyleBackColor = true;
		this.comboBox7.FormattingEnabled = true;
		this.comboBox7.Location = new System.Drawing.Point(212, 17);
		this.comboBox7.Name = "comboBox7";
		this.comboBox7.Size = new System.Drawing.Size(112, 20);
		this.comboBox7.TabIndex = 77;
		this.textBox60.Location = new System.Drawing.Point(25, 56);
		this.textBox60.Name = "textBox60";
		this.textBox60.Size = new System.Drawing.Size(29, 19);
		this.textBox60.TabIndex = 34;
		this.label102.AutoSize = true;
		this.label102.Location = new System.Drawing.Point(6, 59);
		this.label102.Name = "label102";
		this.label102.Size = new System.Drawing.Size(13, 12);
		this.label102.TabIndex = 50;
		this.label101.AutoSize = true;
		this.label101.Location = new System.Drawing.Point(60, 59);
		this.label101.Name = "label101";
		this.label101.Size = new System.Drawing.Size(13, 12);
		this.label101.TabIndex = 52;
		this.label100.AutoSize = true;
		this.label100.Location = new System.Drawing.Point(114, 59);
		this.label100.Name = "label100";
		this.label100.Size = new System.Drawing.Size(13, 12);
		this.label100.TabIndex = 54;
		this.label99.AutoSize = true;
		this.label99.Location = new System.Drawing.Point(168, 59);
		this.label99.Name = "label99";
		this.label99.Size = new System.Drawing.Size(13, 12);
		this.label99.TabIndex = 56;
		this.label98.AutoSize = true;
		this.label98.Location = new System.Drawing.Point(222, 59);
		this.label98.Name = "label98";
		this.label98.Size = new System.Drawing.Size(13, 12);
		this.label98.TabIndex = 58;
		this.label97.AutoSize = true;
		this.label97.Location = new System.Drawing.Point(276, 59);
		this.label97.Name = "label97";
		this.label97.Size = new System.Drawing.Size(12, 12);
		this.label97.TabIndex = 60;
		this.comboBox8.FormattingEnabled = true;
		this.comboBox8.Location = new System.Drawing.Point(41, 22);
		this.comboBox8.Name = "comboBox8";
		this.comboBox8.Size = new System.Drawing.Size(83, 20);
		this.comboBox8.TabIndex = 31;
		this.label96.AutoSize = true;
		this.label96.Location = new System.Drawing.Point(6, 25);
		this.label96.Name = "label96";
		this.label96.Size = new System.Drawing.Size(29, 12);
		this.label96.TabIndex = 62;
		this.label95.AutoSize = true;
		this.label95.Location = new System.Drawing.Point(136, 25);
		this.label95.Name = "label95";
		this.label95.Size = new System.Drawing.Size(29, 12);
		this.label95.TabIndex = 64;
		this.comboBox3.FormattingEnabled = true;
		this.comboBox3.Location = new System.Drawing.Point(171, 22);
		this.comboBox3.Name = "comboBox3";
		this.comboBox3.Size = new System.Drawing.Size(90, 20);
		this.comboBox3.TabIndex = 32;
		this.label94.AutoSize = true;
		this.label94.Location = new System.Drawing.Point(273, 25);
		this.label94.Name = "label94";
		this.label94.Size = new System.Drawing.Size(17, 12);
		this.label94.TabIndex = 65;
		this.textBox59.Location = new System.Drawing.Point(297, 22);
		this.textBox59.Name = "textBox59";
		this.textBox59.Size = new System.Drawing.Size(23, 19);
		this.textBox59.TabIndex = 33;
		this.textBox58.Location = new System.Drawing.Point(79, 56);
		this.textBox58.Name = "textBox58";
		this.textBox58.Size = new System.Drawing.Size(29, 19);
		this.textBox58.TabIndex = 35;
		this.textBox57.Location = new System.Drawing.Point(133, 56);
		this.textBox57.Name = "textBox57";
		this.textBox57.Size = new System.Drawing.Size(29, 19);
		this.textBox57.TabIndex = 36;
		this.textBox56.Location = new System.Drawing.Point(187, 56);
		this.textBox56.Name = "textBox56";
		this.textBox56.Size = new System.Drawing.Size(29, 19);
		this.textBox56.TabIndex = 37;
		this.textBox55.Location = new System.Drawing.Point(241, 56);
		this.textBox55.Name = "textBox55";
		this.textBox55.Size = new System.Drawing.Size(29, 19);
		this.textBox55.TabIndex = 38;
		this.textBox54.Location = new System.Drawing.Point(294, 56);
		this.textBox54.Name = "textBox54";
		this.textBox54.Size = new System.Drawing.Size(29, 19);
		this.textBox54.TabIndex = 39;
		this.checkBox7.AutoSize = true;
		this.checkBox7.BackColor = System.Drawing.Color.White;
		this.checkBox7.Location = new System.Drawing.Point(6, 0);
		this.checkBox7.Name = "checkBox7";
		this.checkBox7.Size = new System.Drawing.Size(102, 16);
		this.checkBox7.TabIndex = 30;
		this.checkBox7.Text = "実数値から検索";
		this.checkBox7.UseVisualStyleBackColor = false;
		this.textBox53.Location = new System.Drawing.Point(172, 162);
		this.textBox53.MaxLength = 10;
		this.textBox53.Name = "textBox53";
		this.textBox53.Size = new System.Drawing.Size(70, 19);
		this.textBox53.TabIndex = 9;
		this.textBox52.Location = new System.Drawing.Point(72, 162);
		this.textBox52.MaxLength = 10;
		this.textBox52.Name = "textBox52";
		this.textBox52.Size = new System.Drawing.Size(70, 19);
		this.textBox52.TabIndex = 8;
		this.label93.Location = new System.Drawing.Point(148, 165);
		this.label93.Name = "label93";
		this.label93.Size = new System.Drawing.Size(18, 16);
		this.label93.TabIndex = 61;
		this.label92.AutoSize = true;
		this.label92.Location = new System.Drawing.Point(48, 165);
		this.label92.Name = "label92";
		this.label92.Size = new System.Drawing.Size(12, 12);
		this.label92.TabIndex = 64;
		this.textBox51.Location = new System.Drawing.Point(160, 194);
		this.textBox51.MaxLength = 6;
		this.textBox51.Name = "textBox51";
		this.textBox51.Size = new System.Drawing.Size(31, 19);
		this.textBox51.TabIndex = 11;
		this.textBox50.Location = new System.Drawing.Point(72, 194);
		this.textBox50.MaxLength = 10;
		this.textBox50.Name = "textBox50";
		this.textBox50.Size = new System.Drawing.Size(65, 19);
		this.textBox50.TabIndex = 10;
		this.label91.Location = new System.Drawing.Point(141, 197);
		this.label91.Name = "label91";
		this.label91.Size = new System.Drawing.Size(18, 16);
		this.label91.TabIndex = 65;
		this.label90.AutoSize = true;
		this.label90.Location = new System.Drawing.Point(25, 197);
		this.label90.Name = "label90";
		this.label90.Size = new System.Drawing.Size(36, 12);
		this.label90.TabIndex = 68;
		this.label89.AutoSize = true;
		this.label89.Location = new System.Drawing.Point(231, 197);
		this.label89.Name = "label89";
		this.label89.Size = new System.Drawing.Size(73, 12);
		this.label89.TabIndex = 69;
		this.button4.Location = new System.Drawing.Point(197, 192);
		this.button4.Name = "button4";
		this.button4.Size = new System.Drawing.Size(23, 23);
		this.button4.TabIndex = 41;
		this.button4.Text = "↑";
		this.button4.UseVisualStyleBackColor = true;
		this.label88.AutoSize = true;
		this.label88.Location = new System.Drawing.Point(7, 25);
		this.label88.Name = "label88";
		this.label88.Size = new System.Drawing.Size(53, 12);
		this.label88.TabIndex = 70;
		this.radioButton3.AutoSize = true;
		this.radioButton3.Checked = true;
		this.radioButton3.Location = new System.Drawing.Point(75, 23);
		this.radioButton3.Name = "radioButton3";
		this.radioButton3.Size = new System.Drawing.Size(35, 16);
		this.radioButton3.TabIndex = 1;
		this.radioButton3.TabStop = true;
		this.radioButton3.Text = "0x";
		this.radioButton3.UseVisualStyleBackColor = true;
		this.textBox49.Location = new System.Drawing.Point(116, 22);
		this.textBox49.Name = "textBox49";
		this.textBox49.Size = new System.Drawing.Size(64, 19);
		this.textBox49.TabIndex = 2;
		this.radioButton2.AutoSize = true;
		this.radioButton2.Location = new System.Drawing.Point(75, 48);
		this.radioButton2.Name = "radioButton2";
		this.radioButton2.Size = new System.Drawing.Size(31, 16);
		this.radioButton2.TabIndex = 3;
		this.radioButton2.Text = "  ";
		this.radioButton2.UseVisualStyleBackColor = true;
		this.textBox48.Location = new System.Drawing.Point(116, 47);
		this.textBox48.Name = "textBox48";
		this.textBox48.Size = new System.Drawing.Size(35, 19);
		this.textBox48.TabIndex = 4;
		this.label87.Location = new System.Drawing.Point(157, 50);
		this.label87.Name = "label87";
		this.label87.Size = new System.Drawing.Size(37, 16);
		this.label87.TabIndex = 77;
		this.textBox47.Location = new System.Drawing.Point(193, 47);
		this.textBox47.Name = "textBox47";
		this.textBox47.Size = new System.Drawing.Size(35, 19);
		this.textBox47.TabIndex = 5;
		this.label86.AutoSize = true;
		this.label86.Location = new System.Drawing.Point(234, 50);
		this.label86.Name = "label86";
		this.label86.Size = new System.Drawing.Size(17, 12);
		this.label86.TabIndex = 79;
		this.radioButton1.AutoSize = true;
		this.radioButton1.Location = new System.Drawing.Point(75, 76);
		this.radioButton1.Name = "radioButton1";
		this.radioButton1.Size = new System.Drawing.Size(35, 16);
		this.radioButton1.TabIndex = 6;
		this.radioButton1.Text = "0x";
		this.radioButton1.UseVisualStyleBackColor = true;
		this.textBox46.Location = new System.Drawing.Point(116, 73);
		this.textBox46.Multiline = true;
		this.textBox46.Name = "textBox46";
		this.textBox46.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.textBox46.Size = new System.Drawing.Size(83, 70);
		this.textBox46.TabIndex = 7;
		this.checkBox8.AutoSize = true;
		this.checkBox8.Location = new System.Drawing.Point(144, 294);
		this.checkBox8.Name = "checkBox8";
		this.checkBox8.Size = new System.Drawing.Size(103, 16);
		this.checkBox8.TabIndex = 15;
		this.checkBox8.Text = "色違いのみ出力";
		this.checkBox8.UseVisualStyleBackColor = true;
		this.label85.AutoSize = true;
		this.label85.Location = new System.Drawing.Point(34, 263);
		this.label85.Name = "label85";
		this.label85.Size = new System.Drawing.Size(23, 12);
		this.label85.TabIndex = 85;
		this.label84.AutoSize = true;
		this.label84.Location = new System.Drawing.Point(34, 295);
		this.label84.Name = "label84";
		this.label84.Size = new System.Drawing.Size(23, 12);
		this.label84.TabIndex = 86;
		this.textBox45.Location = new System.Drawing.Point(72, 260);
		this.textBox45.MaxLength = 5;
		this.textBox45.Name = "textBox45";
		this.textBox45.Size = new System.Drawing.Size(60, 19);
		this.textBox45.TabIndex = 13;
		this.textBox44.Location = new System.Drawing.Point(72, 292);
		this.textBox44.MaxLength = 5;
		this.textBox44.Name = "textBox44";
		this.textBox44.Size = new System.Drawing.Size(60, 19);
		this.textBox44.TabIndex = 14;
		this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBox2.FormattingEnabled = true;
		this.comboBox2.Items.AddRange(new object[4] { "Method1", "Method2", "Method3", "Method4" });
		this.comboBox2.Location = new System.Drawing.Point(72, 226);
		this.comboBox2.Name = "comboBox2";
		this.comboBox2.Size = new System.Drawing.Size(90, 20);
		this.comboBox2.TabIndex = 12;
		this.label83.AutoSize = true;
		this.label83.Location = new System.Drawing.Point(6, 59);
		this.label83.Name = "label83";
		this.label83.Size = new System.Drawing.Size(29, 12);
		this.label83.TabIndex = 35;
		this.label82.AutoSize = true;
		this.label82.Location = new System.Drawing.Point(7, 89);
		this.label82.Name = "label82";
		this.label82.Size = new System.Drawing.Size(29, 12);
		this.label82.TabIndex = 36;
		this.textBox43.Location = new System.Drawing.Point(61, 56);
		this.textBox43.Name = "textBox43";
		this.textBox43.Size = new System.Drawing.Size(23, 19);
		this.textBox43.TabIndex = 18;
		this.label81.AutoSize = true;
		this.label81.Location = new System.Drawing.Point(42, 59);
		this.label81.Name = "label81";
		this.label81.Size = new System.Drawing.Size(13, 12);
		this.label81.TabIndex = 38;
		this.textBox42.Location = new System.Drawing.Point(109, 56);
		this.textBox42.Name = "textBox42";
		this.textBox42.Size = new System.Drawing.Size(23, 19);
		this.textBox42.TabIndex = 19;
		this.label80.AutoSize = true;
		this.label80.Location = new System.Drawing.Point(90, 59);
		this.label80.Name = "label80";
		this.label80.Size = new System.Drawing.Size(13, 12);
		this.label80.TabIndex = 40;
		this.textBox41.Location = new System.Drawing.Point(157, 56);
		this.textBox41.Name = "textBox41";
		this.textBox41.Size = new System.Drawing.Size(23, 19);
		this.textBox41.TabIndex = 20;
		this.label79.AutoSize = true;
		this.label79.Location = new System.Drawing.Point(138, 59);
		this.label79.Name = "label79";
		this.label79.Size = new System.Drawing.Size(13, 12);
		this.label79.TabIndex = 42;
		this.textBox40.Location = new System.Drawing.Point(205, 56);
		this.textBox40.Name = "textBox40";
		this.textBox40.Size = new System.Drawing.Size(23, 19);
		this.textBox40.TabIndex = 21;
		this.label78.AutoSize = true;
		this.label78.Location = new System.Drawing.Point(186, 59);
		this.label78.Name = "label78";
		this.label78.Size = new System.Drawing.Size(13, 12);
		this.label78.TabIndex = 44;
		this.textBox39.Location = new System.Drawing.Point(253, 56);
		this.textBox39.Name = "textBox39";
		this.textBox39.Size = new System.Drawing.Size(23, 19);
		this.textBox39.TabIndex = 22;
		this.label77.AutoSize = true;
		this.label77.Location = new System.Drawing.Point(234, 59);
		this.label77.Name = "label77";
		this.label77.Size = new System.Drawing.Size(13, 12);
		this.label77.TabIndex = 46;
		this.textBox38.Location = new System.Drawing.Point(301, 56);
		this.textBox38.Name = "textBox38";
		this.textBox38.Size = new System.Drawing.Size(23, 19);
		this.textBox38.TabIndex = 23;
		this.label76.AutoSize = true;
		this.label76.Location = new System.Drawing.Point(282, 59);
		this.label76.Name = "label76";
		this.label76.Size = new System.Drawing.Size(12, 12);
		this.label76.TabIndex = 48;
		this.textBox37.Location = new System.Drawing.Point(61, 86);
		this.textBox37.Name = "textBox37";
		this.textBox37.Size = new System.Drawing.Size(23, 19);
		this.textBox37.TabIndex = 24;
		this.label75.AutoSize = true;
		this.label75.Location = new System.Drawing.Point(42, 89);
		this.label75.Name = "label75";
		this.label75.Size = new System.Drawing.Size(13, 12);
		this.label75.TabIndex = 50;
		this.textBox36.Location = new System.Drawing.Point(109, 86);
		this.textBox36.Name = "textBox36";
		this.textBox36.Size = new System.Drawing.Size(23, 19);
		this.textBox36.TabIndex = 25;
		this.label74.AutoSize = true;
		this.label74.Location = new System.Drawing.Point(90, 89);
		this.label74.Name = "label74";
		this.label74.Size = new System.Drawing.Size(13, 12);
		this.label74.TabIndex = 52;
		this.textBox6.Location = new System.Drawing.Point(157, 86);
		this.textBox6.Name = "textBox6";
		this.textBox6.Size = new System.Drawing.Size(23, 19);
		this.textBox6.TabIndex = 26;
		this.label73.AutoSize = true;
		this.label73.Location = new System.Drawing.Point(138, 89);
		this.label73.Name = "label73";
		this.label73.Size = new System.Drawing.Size(13, 12);
		this.label73.TabIndex = 54;
		this.textBox5.Location = new System.Drawing.Point(205, 86);
		this.textBox5.Name = "textBox5";
		this.textBox5.Size = new System.Drawing.Size(23, 19);
		this.textBox5.TabIndex = 27;
		this.label72.AutoSize = true;
		this.label72.Location = new System.Drawing.Point(186, 89);
		this.label72.Name = "label72";
		this.label72.Size = new System.Drawing.Size(13, 12);
		this.label72.TabIndex = 56;
		this.textBox4.Location = new System.Drawing.Point(253, 86);
		this.textBox4.Name = "textBox4";
		this.textBox4.Size = new System.Drawing.Size(23, 19);
		this.textBox4.TabIndex = 28;
		this.label71.AutoSize = true;
		this.label71.Location = new System.Drawing.Point(234, 89);
		this.label71.Name = "label71";
		this.label71.Size = new System.Drawing.Size(13, 12);
		this.label71.TabIndex = 58;
		this.textBox1.Location = new System.Drawing.Point(301, 86);
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(23, 19);
		this.textBox1.TabIndex = 29;
		this.label70.AutoSize = true;
		this.label70.Location = new System.Drawing.Point(7, 27);
		this.label70.Name = "label70";
		this.label70.Size = new System.Drawing.Size(29, 12);
		this.label70.TabIndex = 66;
		this.label69.AutoSize = true;
		this.label69.Location = new System.Drawing.Point(282, 89);
		this.label69.Name = "label69";
		this.label69.Size = new System.Drawing.Size(12, 12);
		this.label69.TabIndex = 60;
		this.comboBox1.FormattingEnabled = true;
		this.comboBox1.Location = new System.Drawing.Point(42, 24);
		this.comboBox1.Name = "comboBox1";
		this.comboBox1.Size = new System.Drawing.Size(90, 20);
		this.comboBox1.TabIndex = 17;
		this.checkBox1.AutoSize = true;
		this.checkBox1.BackColor = System.Drawing.Color.White;
		this.checkBox1.Checked = true;
		this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkBox1.Location = new System.Drawing.Point(6, 0);
		this.checkBox1.Name = "checkBox1";
		this.checkBox1.Size = new System.Drawing.Size(102, 16);
		this.checkBox1.TabIndex = 16;
		this.checkBox1.Text = "個体値から検索";
		this.checkBox1.UseVisualStyleBackColor = false;
		this.dataGridViewTextBoxColumn29.HeaderText = "ずれ";
		this.dataGridViewTextBoxColumn29.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn29.Name = "dataGridViewTextBoxColumn29";
		this.dataGridViewTextBoxColumn29.ReadOnly = true;
		this.dataGridViewTextBoxColumn29.Width = 51;
		this.dataGridViewTextBoxColumn28.HeaderText = "S";
		this.dataGridViewTextBoxColumn28.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn28.Name = "dataGridViewTextBoxColumn28";
		this.dataGridViewTextBoxColumn28.ReadOnly = true;
		this.dataGridViewTextBoxColumn28.Width = 30;
		this.dataGridViewTextBoxColumn27.HeaderText = "D";
		this.dataGridViewTextBoxColumn27.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn27.Name = "dataGridViewTextBoxColumn27";
		this.dataGridViewTextBoxColumn27.ReadOnly = true;
		this.dataGridViewTextBoxColumn27.Width = 30;
		this.dataGridViewTextBoxColumn26.HeaderText = "C";
		this.dataGridViewTextBoxColumn26.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn26.Name = "dataGridViewTextBoxColumn26";
		this.dataGridViewTextBoxColumn26.ReadOnly = true;
		this.dataGridViewTextBoxColumn26.Width = 30;
		this.dataGridViewTextBoxColumn25.HeaderText = "B";
		this.dataGridViewTextBoxColumn25.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn25.Name = "dataGridViewTextBoxColumn25";
		this.dataGridViewTextBoxColumn25.ReadOnly = true;
		this.dataGridViewTextBoxColumn25.Width = 30;
		this.dataGridViewTextBoxColumn24.HeaderText = "A";
		this.dataGridViewTextBoxColumn24.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn24.Name = "dataGridViewTextBoxColumn24";
		this.dataGridViewTextBoxColumn24.ReadOnly = true;
		this.dataGridViewTextBoxColumn24.Width = 30;
		this.dataGridViewTextBoxColumn23.HeaderText = "H";
		this.dataGridViewTextBoxColumn23.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn23.Name = "dataGridViewTextBoxColumn23";
		this.dataGridViewTextBoxColumn23.ReadOnly = true;
		this.dataGridViewTextBoxColumn23.Width = 30;
		this.dataGridViewTextBoxColumn22.HeaderText = "特性";
		this.dataGridViewTextBoxColumn22.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn22.Name = "dataGridViewTextBoxColumn22";
		this.dataGridViewTextBoxColumn22.ReadOnly = true;
		this.dataGridViewTextBoxColumn22.Width = 60;
		this.dataGridViewTextBoxColumn21.HeaderText = "性別";
		this.dataGridViewTextBoxColumn21.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn21.Name = "dataGridViewTextBoxColumn21";
		this.dataGridViewTextBoxColumn21.ReadOnly = true;
		this.dataGridViewTextBoxColumn21.Width = 54;
		this.dataGridViewTextBoxColumn20.HeaderText = "S";
		this.dataGridViewTextBoxColumn20.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn20.Name = "dataGridViewTextBoxColumn20";
		this.dataGridViewTextBoxColumn20.ReadOnly = true;
		this.dataGridViewTextBoxColumn20.Width = 27;
		this.dataGridViewTextBoxColumn19.HeaderText = "D";
		this.dataGridViewTextBoxColumn19.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn19.Name = "dataGridViewTextBoxColumn19";
		this.dataGridViewTextBoxColumn19.ReadOnly = true;
		this.dataGridViewTextBoxColumn19.Width = 27;
		this.dataGridViewTextBoxColumn18.HeaderText = "C";
		this.dataGridViewTextBoxColumn18.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn18.Name = "dataGridViewTextBoxColumn18";
		this.dataGridViewTextBoxColumn18.ReadOnly = true;
		this.dataGridViewTextBoxColumn18.Width = 27;
		this.dataGridViewTextBoxColumn17.HeaderText = "B";
		this.dataGridViewTextBoxColumn17.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn17.Name = "dataGridViewTextBoxColumn17";
		this.dataGridViewTextBoxColumn17.ReadOnly = true;
		this.dataGridViewTextBoxColumn17.Width = 27;
		this.dataGridViewTextBoxColumn16.HeaderText = "A";
		this.dataGridViewTextBoxColumn16.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn16.Name = "dataGridViewTextBoxColumn16";
		this.dataGridViewTextBoxColumn16.ReadOnly = true;
		this.dataGridViewTextBoxColumn16.Width = 27;
		this.dataGridViewTextBoxColumn15.HeaderText = "H";
		this.dataGridViewTextBoxColumn15.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn15.Name = "dataGridViewTextBoxColumn15";
		this.dataGridViewTextBoxColumn15.ReadOnly = true;
		this.dataGridViewTextBoxColumn15.Width = 27;
		this.dataGridViewTextBoxColumn14.HeaderText = "性格";
		this.dataGridViewTextBoxColumn14.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn14.Name = "dataGridViewTextBoxColumn14";
		this.dataGridViewTextBoxColumn14.ReadOnly = true;
		this.dataGridViewTextBoxColumn14.Width = 60;
		this.dataGridViewTextBoxColumn13.HeaderText = "性格値";
		this.dataGridViewTextBoxColumn13.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn13.Name = "dataGridViewTextBoxColumn13";
		this.dataGridViewTextBoxColumn13.ReadOnly = true;
		this.dataGridViewTextBoxColumn13.Width = 66;
		this.dataGridViewTextBoxColumn12.HeaderText = "乱数値";
		this.dataGridViewTextBoxColumn12.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn12.Name = "dataGridViewTextBoxColumn12";
		this.dataGridViewTextBoxColumn12.ReadOnly = true;
		this.dataGridViewTextBoxColumn12.Width = 66;
		this.dataGridViewTextBoxColumn11.HeaderText = "F";
		this.dataGridViewTextBoxColumn11.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
		this.dataGridViewTextBoxColumn11.ReadOnly = true;
		this.dataGridViewTextBoxColumn11.Width = 50;
		this.dataGridViewTextBoxColumn10.HeaderText = "初期seed";
		this.dataGridViewTextBoxColumn10.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
		this.dataGridViewTextBoxColumn10.ReadOnly = true;
		this.dataGridViewTextBoxColumn10.Width = 77;
		this.TabPage_Egg.Controls.Add(this.tabControl2);
		this.TabPage_Egg.Location = new System.Drawing.Point(4, 22);
		this.TabPage_Egg.Name = "TabPage_Egg";
		this.TabPage_Egg.Padding = new System.Windows.Forms.Padding(3);
		this.TabPage_Egg.Size = new System.Drawing.Size(1085, 611);
		this.TabPage_Egg.TabIndex = 3;
		this.TabPage_Egg.Text = "孵化";
		this.TabPage_Egg.UseVisualStyleBackColor = true;
		this.tabControl2.Controls.Add(this.TabPage_EggPID);
		this.tabControl2.Controls.Add(this.TabPage_EggIVs);
		this.tabControl2.Location = new System.Drawing.Point(6, 6);
		this.tabControl2.Name = "tabControl2";
		this.tabControl2.SelectedIndex = 0;
		this.tabControl2.Size = new System.Drawing.Size(1073, 599);
		this.tabControl2.TabIndex = 1;
		this.TabPage_EggPID.Controls.Add(this.es_groupBox2);
		this.TabPage_EggPID.Controls.Add(this.es_groupBox1);
		this.TabPage_EggPID.Controls.Add(this.es_dataGridView);
		this.TabPage_EggPID.Location = new System.Drawing.Point(4, 22);
		this.TabPage_EggPID.Name = "TabPage_EggPID";
		this.TabPage_EggPID.Padding = new System.Windows.Forms.Padding(3);
		this.TabPage_EggPID.Size = new System.Drawing.Size(1065, 573);
		this.TabPage_EggPID.TabIndex = 0;
		this.TabPage_EggPID.Text = "爺前固定";
		this.TabPage_EggPID.UseVisualStyleBackColor = true;
		this.es_groupBox2.Controls.Add(this.NaturePanel_EggPID);
		this.es_groupBox2.Controls.Add(this.label143);
		this.es_groupBox2.Controls.Add(this.SID_EggPID);
		this.es_groupBox2.Controls.Add(this.TID_EggPID);
		this.es_groupBox2.Controls.Add(this.OnlyShiny_EggPID);
		this.es_groupBox2.Controls.Add(this.label142);
		this.es_groupBox2.Controls.Add(this.es_pokedex);
		this.es_groupBox2.Controls.Add(this.label140);
		this.es_groupBox2.Controls.Add(this.es_sex);
		this.es_groupBox2.Controls.Add(this.label144);
		this.es_groupBox2.Controls.Add(this.label147);
		this.es_groupBox2.Controls.Add(this.label141);
		this.es_groupBox2.Controls.Add(this.es_ability);
		this.es_groupBox2.Location = new System.Drawing.Point(6, 200);
		this.es_groupBox2.Name = "es_groupBox2";
		this.es_groupBox2.Size = new System.Drawing.Size(535, 261);
		this.es_groupBox2.TabIndex = 0;
		this.es_groupBox2.TabStop = false;
		this.es_groupBox2.Text = "フィルター";
		this.label143.AutoSize = true;
		this.label143.Location = new System.Drawing.Point(25, 111);
		this.label143.Name = "label143";
		this.label143.Size = new System.Drawing.Size(29, 12);
		this.label143.TabIndex = 125;
		this.label143.Text = "性格";
		this.SID_EggPID.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.SID_EggPID.Location = new System.Drawing.Point(368, 83);
		this.SID_EggPID.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.SID_EggPID.Name = "SID_EggPID";
		this.SID_EggPID.Size = new System.Drawing.Size(80, 22);
		this.SID_EggPID.TabIndex = 24;
		this.SID_EggPID.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.TID_EggPID.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TID_EggPID.Location = new System.Drawing.Point(368, 55);
		this.TID_EggPID.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.TID_EggPID.Name = "TID_EggPID";
		this.TID_EggPID.Size = new System.Drawing.Size(80, 22);
		this.TID_EggPID.TabIndex = 23;
		this.TID_EggPID.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.OnlyShiny_EggPID.AutoSize = true;
		this.OnlyShiny_EggPID.Checked = true;
		this.OnlyShiny_EggPID.CheckState = System.Windows.Forms.CheckState.Checked;
		this.OnlyShiny_EggPID.Location = new System.Drawing.Point(334, 33);
		this.OnlyShiny_EggPID.Name = "OnlyShiny_EggPID";
		this.OnlyShiny_EggPID.Size = new System.Drawing.Size(103, 16);
		this.OnlyShiny_EggPID.TabIndex = 25;
		this.OnlyShiny_EggPID.Text = "色違いのみ出力";
		this.OnlyShiny_EggPID.UseVisualStyleBackColor = true;
		this.label142.AutoSize = true;
		this.label142.Location = new System.Drawing.Point(12, 30);
		this.label142.Name = "label142";
		this.label142.Size = new System.Drawing.Size(42, 12);
		this.label142.TabIndex = 105;
		this.label142.Text = "ポケモン";
		this.es_pokedex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.es_pokedex.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.es_pokedex.FormattingEnabled = true;
		this.es_pokedex.Location = new System.Drawing.Point(60, 27);
		this.es_pokedex.Name = "es_pokedex";
		this.es_pokedex.Size = new System.Drawing.Size(80, 22);
		this.es_pokedex.TabIndex = 17;
		this.es_pokedex.SelectedIndexChanged += new System.EventHandler(es_pokedex_SelectedIndexChanged);
		this.label140.AutoSize = true;
		this.label140.Location = new System.Drawing.Point(334, 88);
		this.label140.Name = "label140";
		this.label140.Size = new System.Drawing.Size(28, 12);
		this.label140.TabIndex = 108;
		this.label140.Text = "裏ID";
		this.es_sex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.es_sex.FormattingEnabled = true;
		this.es_sex.Items.AddRange(new object[4] { "指定なし", "♂", "♀", "-" });
		this.es_sex.Location = new System.Drawing.Point(60, 81);
		this.es_sex.Name = "es_sex";
		this.es_sex.Size = new System.Drawing.Size(80, 20);
		this.es_sex.TabIndex = 20;
		this.label144.AutoSize = true;
		this.label144.Location = new System.Drawing.Point(25, 84);
		this.label144.Name = "label144";
		this.label144.Size = new System.Drawing.Size(29, 12);
		this.label144.TabIndex = 124;
		this.label144.Text = "性別";
		this.label147.AutoSize = true;
		this.label147.Location = new System.Drawing.Point(25, 58);
		this.label147.Name = "label147";
		this.label147.Size = new System.Drawing.Size(29, 12);
		this.label147.TabIndex = 123;
		this.label147.Text = "特性";
		this.label141.AutoSize = true;
		this.label141.Location = new System.Drawing.Point(334, 60);
		this.label141.Name = "label141";
		this.label141.Size = new System.Drawing.Size(28, 12);
		this.label141.TabIndex = 107;
		this.label141.Text = "表ID";
		this.es_ability.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.es_ability.FormattingEnabled = true;
		this.es_ability.Location = new System.Drawing.Point(60, 55);
		this.es_ability.Name = "es_ability";
		this.es_ability.Size = new System.Drawing.Size(80, 20);
		this.es_ability.TabIndex = 18;
		this.es_groupBox1.Controls.Add(this.DiffMax);
		this.es_groupBox1.Controls.Add(this.Cancel_EggPID);
		this.es_groupBox1.Controls.Add(this.DiffMin);
		this.es_groupBox1.Controls.Add(this.Search_EggPID);
		this.es_groupBox1.Controls.Add(this.label122);
		this.es_groupBox1.Controls.Add(this.EverstoneNature_EggPID);
		this.es_groupBox1.Controls.Add(this.LastFrame_EggPID);
		this.es_groupBox1.Controls.Add(this.FirstFrame_EggPID);
		this.es_groupBox1.Controls.Add(this.label120);
		this.es_groupBox1.Controls.Add(this.calibration_value3);
		this.es_groupBox1.Controls.Add(this.EverstoneCheck);
		this.es_groupBox1.Controls.Add(this.label138);
		this.es_groupBox1.Controls.Add(this.calibration_value1);
		this.es_groupBox1.Controls.Add(this.label145);
		this.es_groupBox1.Controls.Add(this.calibration_value2);
		this.es_groupBox1.Controls.Add(this.label155);
		this.es_groupBox1.Location = new System.Drawing.Point(6, 6);
		this.es_groupBox1.Name = "es_groupBox1";
		this.es_groupBox1.Size = new System.Drawing.Size(535, 188);
		this.es_groupBox1.TabIndex = 0;
		this.es_groupBox1.TabStop = false;
		this.es_groupBox1.Text = "検索範囲";
		this.DiffMax.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.DiffMax.Location = new System.Drawing.Point(160, 55);
		this.DiffMax.Name = "DiffMax";
		this.DiffMax.Size = new System.Drawing.Size(60, 22);
		this.DiffMax.TabIndex = 13;
		this.DiffMax.Value = new decimal(new int[4] { 28, 0, 0, 0 });
		this.DiffMax.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.Cancel_EggPID.Enabled = false;
		this.Cancel_EggPID.Location = new System.Drawing.Point(454, 159);
		this.Cancel_EggPID.Name = "Cancel_EggPID";
		this.Cancel_EggPID.Size = new System.Drawing.Size(75, 23);
		this.Cancel_EggPID.TabIndex = 27;
		this.Cancel_EggPID.Text = "キャンセル";
		this.Cancel_EggPID.UseVisualStyleBackColor = true;
		this.Cancel_EggPID.Click += new System.EventHandler(Cancel_EggPID_Click);
		this.DiffMin.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.DiffMin.Location = new System.Drawing.Point(70, 55);
		this.DiffMin.Name = "DiffMin";
		this.DiffMin.Size = new System.Drawing.Size(60, 22);
		this.DiffMin.TabIndex = 12;
		this.DiffMin.Value = new decimal(new int[4] { 18, 0, 0, 0 });
		this.DiffMin.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.Search_EggPID.Location = new System.Drawing.Point(373, 159);
		this.Search_EggPID.Name = "Search_EggPID";
		this.Search_EggPID.Size = new System.Drawing.Size(75, 23);
		this.Search_EggPID.TabIndex = 26;
		this.Search_EggPID.Text = "検索";
		this.Search_EggPID.UseVisualStyleBackColor = true;
		this.Search_EggPID.Click += new System.EventHandler(Search_EggPID_Click);
		this.label122.Location = new System.Drawing.Point(136, 60);
		this.label122.Name = "label122";
		this.label122.Size = new System.Drawing.Size(18, 16);
		this.label122.TabIndex = 111;
		this.label122.Text = "～";
		this.EverstoneNature_EggPID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.EverstoneNature_EggPID.FormattingEnabled = true;
		this.EverstoneNature_EggPID.Location = new System.Drawing.Point(203, 112);
		this.EverstoneNature_EggPID.Name = "EverstoneNature_EggPID";
		this.EverstoneNature_EggPID.Size = new System.Drawing.Size(80, 20);
		this.EverstoneNature_EggPID.TabIndex = 22;
		this.LastFrame_EggPID.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.LastFrame_EggPID.Location = new System.Drawing.Point(160, 27);
		this.LastFrame_EggPID.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.LastFrame_EggPID.Name = "LastFrame_EggPID";
		this.LastFrame_EggPID.Size = new System.Drawing.Size(93, 22);
		this.LastFrame_EggPID.TabIndex = 11;
		this.LastFrame_EggPID.Value = new decimal(new int[4] { 5000, 0, 0, 0 });
		this.LastFrame_EggPID.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.FirstFrame_EggPID.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.FirstFrame_EggPID.Location = new System.Drawing.Point(37, 27);
		this.FirstFrame_EggPID.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.FirstFrame_EggPID.Name = "FirstFrame_EggPID";
		this.FirstFrame_EggPID.Size = new System.Drawing.Size(93, 22);
		this.FirstFrame_EggPID.TabIndex = 10;
		this.FirstFrame_EggPID.Value = new decimal(new int[4] { 2000, 0, 0, 0 });
		this.FirstFrame_EggPID.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.label120.Location = new System.Drawing.Point(136, 32);
		this.label120.Name = "label120";
		this.label120.Size = new System.Drawing.Size(18, 16);
		this.label120.TabIndex = 108;
		this.label120.Text = "～";
		this.calibration_value3.AutoSize = true;
		this.calibration_value3.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.calibration_value3.Location = new System.Drawing.Point(70, 136);
		this.calibration_value3.Name = "calibration_value3";
		this.calibration_value3.Size = new System.Drawing.Size(81, 18);
		this.calibration_value3.TabIndex = 16;
		this.calibration_value3.TabStop = true;
		this.calibration_value3.Text = "とっても良い";
		this.calibration_value3.UseVisualStyleBackColor = true;
		this.EverstoneCheck.AutoSize = true;
		this.EverstoneCheck.Location = new System.Drawing.Point(185, 91);
		this.EverstoneCheck.Name = "EverstoneCheck";
		this.EverstoneCheck.Size = new System.Drawing.Size(86, 16);
		this.EverstoneCheck.TabIndex = 21;
		this.EverstoneCheck.Text = "変わらずの石";
		this.EverstoneCheck.UseVisualStyleBackColor = true;
		this.EverstoneCheck.CheckStateChanged += new System.EventHandler(Everstone_CheckStateChanged);
		this.label138.AutoSize = true;
		this.label138.Location = new System.Drawing.Point(19, 60);
		this.label138.Name = "label138";
		this.label138.Size = new System.Drawing.Size(29, 12);
		this.label138.TabIndex = 73;
		this.label138.Text = "差分";
		this.calibration_value1.AutoSize = true;
		this.calibration_value1.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.calibration_value1.Location = new System.Drawing.Point(70, 88);
		this.calibration_value1.Name = "calibration_value1";
		this.calibration_value1.Size = new System.Drawing.Size(60, 18);
		this.calibration_value1.TabIndex = 14;
		this.calibration_value1.TabStop = true;
		this.calibration_value1.Text = "よくない";
		this.calibration_value1.UseVisualStyleBackColor = true;
		this.label145.AutoSize = true;
		this.label145.Location = new System.Drawing.Point(19, 30);
		this.label145.Name = "label145";
		this.label145.Size = new System.Drawing.Size(12, 12);
		this.label145.TabIndex = 64;
		this.label145.Text = "F";
		this.calibration_value2.AutoSize = true;
		this.calibration_value2.Checked = true;
		this.calibration_value2.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.calibration_value2.Location = new System.Drawing.Point(70, 112);
		this.calibration_value2.Name = "calibration_value2";
		this.calibration_value2.Size = new System.Drawing.Size(63, 18);
		this.calibration_value2.TabIndex = 15;
		this.calibration_value2.TabStop = true;
		this.calibration_value2.Text = "まずまず";
		this.calibration_value2.UseVisualStyleBackColor = true;
		this.label155.AutoSize = true;
		this.label155.Location = new System.Drawing.Point(19, 91);
		this.label155.Name = "label155";
		this.label155.Size = new System.Drawing.Size(29, 12);
		this.label155.TabIndex = 16;
		this.label155.Text = "相性";
		this.es_dataGridView.AllowUserToAddRows = false;
		this.es_dataGridView.AllowUserToDeleteRows = false;
		this.es_dataGridView.AllowUserToResizeRows = false;
		dataGridViewCellStyle.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
		this.es_dataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle;
		this.es_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.es_dataGridView.ContextMenuStrip = this.contextMenuStrip4;
		this.es_dataGridView.Location = new System.Drawing.Point(547, 6);
		this.es_dataGridView.Name = "es_dataGridView";
		this.es_dataGridView.ReadOnly = true;
		this.es_dataGridView.RowHeadersWidth = 30;
		this.es_dataGridView.RowTemplate.Height = 21;
		this.es_dataGridView.Size = new System.Drawing.Size(512, 561);
		this.es_dataGridView.TabIndex = 0;
		this.es_dataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(Es_dataGridView_CellFormatting);
		this.es_dataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(dataGridView_MouseDown);
		this.TabPage_EggIVs.Controls.Add(this.ek_groupBox3);
		this.TabPage_EggIVs.Controls.Add(this.ek_groupBox2);
		this.TabPage_EggIVs.Controls.Add(this.ek_cancel);
		this.TabPage_EggIVs.Controls.Add(this.ek_listup);
		this.TabPage_EggIVs.Controls.Add(this.ek_groupBox1);
		this.TabPage_EggIVs.Controls.Add(this.ek_dataGridView);
		this.TabPage_EggIVs.Controls.Add(this.ek_start);
		this.TabPage_EggIVs.Location = new System.Drawing.Point(4, 22);
		this.TabPage_EggIVs.Name = "TabPage_EggIVs";
		this.TabPage_EggIVs.Padding = new System.Windows.Forms.Padding(3);
		this.TabPage_EggIVs.Size = new System.Drawing.Size(1065, 573);
		this.TabPage_EggIVs.TabIndex = 1;
		this.TabPage_EggIVs.Text = "個体値乱数";
		this.TabPage_EggIVs.UseVisualStyleBackColor = true;
		this.ek_groupBox3.Controls.Add(this.ek_stats6);
		this.ek_groupBox3.Controls.Add(this.ek_stats5);
		this.ek_groupBox3.Controls.Add(this.ek_stats4);
		this.ek_groupBox3.Controls.Add(this.ek_stats3);
		this.ek_groupBox3.Controls.Add(this.ek_stats2);
		this.ek_groupBox3.Controls.Add(this.ek_stats1);
		this.ek_groupBox3.Controls.Add(this.label156);
		this.ek_groupBox3.Controls.Add(this.ek_mezapaPower);
		this.ek_groupBox3.Controls.Add(this.label157);
		this.ek_groupBox3.Controls.Add(this.ek_mezapaType);
		this.ek_groupBox3.Controls.Add(this.ek_search2);
		this.ek_groupBox3.Controls.Add(this.label158);
		this.ek_groupBox3.Controls.Add(this.label159);
		this.ek_groupBox3.Controls.Add(this.ek_search1);
		this.ek_groupBox3.Controls.Add(this.label162);
		this.ek_groupBox3.Controls.Add(this.label164);
		this.ek_groupBox3.Controls.Add(this.label165);
		this.ek_groupBox3.Controls.Add(this.label166);
		this.ek_groupBox3.Controls.Add(this.ek_pokedex);
		this.ek_groupBox3.Controls.Add(this.label167);
		this.ek_groupBox3.Controls.Add(this.label168);
		this.ek_groupBox3.Controls.Add(this.ek_IVup6);
		this.ek_groupBox3.Controls.Add(this.ek_IVlow1);
		this.ek_groupBox3.Controls.Add(this.label169);
		this.ek_groupBox3.Controls.Add(this.ek_IVlow2);
		this.ek_groupBox3.Controls.Add(this.ek_nature);
		this.ek_groupBox3.Controls.Add(this.ek_IVlow3);
		this.ek_groupBox3.Controls.Add(this.ek_IVup5);
		this.ek_groupBox3.Controls.Add(this.ek_IVlow4);
		this.ek_groupBox3.Controls.Add(this.label170);
		this.ek_groupBox3.Controls.Add(this.ek_IVlow5);
		this.ek_groupBox3.Controls.Add(this.label171);
		this.ek_groupBox3.Controls.Add(this.ek_IVlow6);
		this.ek_groupBox3.Controls.Add(this.ek_IVup4);
		this.ek_groupBox3.Controls.Add(this.label172);
		this.ek_groupBox3.Controls.Add(this.label173);
		this.ek_groupBox3.Controls.Add(this.ek_IVup1);
		this.ek_groupBox3.Controls.Add(this.ek_IVup3);
		this.ek_groupBox3.Controls.Add(this.label174);
		this.ek_groupBox3.Controls.Add(this.label175);
		this.ek_groupBox3.Controls.Add(this.ek_IVup2);
		this.ek_groupBox3.Controls.Add(this.ek_Lv);
		this.ek_groupBox3.Location = new System.Drawing.Point(6, 308);
		this.ek_groupBox3.Name = "ek_groupBox3";
		this.ek_groupBox3.Size = new System.Drawing.Size(407, 259);
		this.ek_groupBox3.TabIndex = 0;
		this.ek_groupBox3.TabStop = false;
		this.ek_stats6.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_stats6.Location = new System.Drawing.Point(41, 221);
		this.ek_stats6.Maximum = new decimal(new int[4] { 810, 0, 0, 0 });
		this.ek_stats6.Name = "ek_stats6";
		this.ek_stats6.Size = new System.Drawing.Size(119, 22);
		this.ek_stats6.TabIndex = 61;
		this.ek_stats6.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_stats6.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.ek_stats5.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_stats5.Location = new System.Drawing.Point(41, 189);
		this.ek_stats5.Maximum = new decimal(new int[4] { 810, 0, 0, 0 });
		this.ek_stats5.Name = "ek_stats5";
		this.ek_stats5.Size = new System.Drawing.Size(119, 22);
		this.ek_stats5.TabIndex = 60;
		this.ek_stats5.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_stats5.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.ek_stats4.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_stats4.Location = new System.Drawing.Point(41, 157);
		this.ek_stats4.Maximum = new decimal(new int[4] { 931, 0, 0, 0 });
		this.ek_stats4.Name = "ek_stats4";
		this.ek_stats4.Size = new System.Drawing.Size(119, 22);
		this.ek_stats4.TabIndex = 59;
		this.ek_stats4.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_stats4.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.ek_stats3.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_stats3.Location = new System.Drawing.Point(41, 125);
		this.ek_stats3.Maximum = new decimal(new int[4] { 1919, 0, 0, 0 });
		this.ek_stats3.Name = "ek_stats3";
		this.ek_stats3.Size = new System.Drawing.Size(119, 22);
		this.ek_stats3.TabIndex = 58;
		this.ek_stats3.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_stats3.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.ek_stats2.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_stats2.Location = new System.Drawing.Point(41, 93);
		this.ek_stats2.Maximum = new decimal(new int[4] { 810, 0, 0, 0 });
		this.ek_stats2.Name = "ek_stats2";
		this.ek_stats2.Size = new System.Drawing.Size(119, 22);
		this.ek_stats2.TabIndex = 57;
		this.ek_stats2.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_stats2.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.ek_stats1.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_stats1.Location = new System.Drawing.Point(41, 61);
		this.ek_stats1.Maximum = new decimal(new int[4] { 114514, 0, 0, 0 });
		this.ek_stats1.Name = "ek_stats1";
		this.ek_stats1.Size = new System.Drawing.Size(119, 22);
		this.ek_stats1.TabIndex = 56;
		this.ek_stats1.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_stats1.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label156.AutoSize = true;
		this.label156.Location = new System.Drawing.Point(361, 66);
		this.label156.Name = "label156";
		this.label156.Size = new System.Drawing.Size(17, 12);
		this.label156.TabIndex = 121;
		this.label156.Text = "～";
		this.ek_mezapaPower.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_mezapaPower.Location = new System.Drawing.Point(305, 61);
		this.ek_mezapaPower.Maximum = new decimal(new int[4] { 70, 0, 0, 0 });
		this.ek_mezapaPower.Minimum = new decimal(new int[4] { 30, 0, 0, 0 });
		this.ek_mezapaPower.Name = "ek_mezapaPower";
		this.ek_mezapaPower.Size = new System.Drawing.Size(50, 22);
		this.ek_mezapaPower.TabIndex = 64;
		this.ek_mezapaPower.Value = new decimal(new int[4] { 30, 0, 0, 0 });
		this.ek_mezapaPower.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_mezapaPower.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label157.AutoSize = true;
		this.label157.Location = new System.Drawing.Point(173, 66);
		this.label157.Name = "label157";
		this.label157.Size = new System.Drawing.Size(34, 12);
		this.label157.TabIndex = 117;
		this.label157.Text = "めざパ";
		this.ek_mezapaType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.ek_mezapaType.FormattingEnabled = true;
		this.ek_mezapaType.Location = new System.Drawing.Point(213, 63);
		this.ek_mezapaType.Name = "ek_mezapaType";
		this.ek_mezapaType.Size = new System.Drawing.Size(80, 20);
		this.ek_mezapaType.TabIndex = 63;
		this.ek_search2.AutoSize = true;
		this.ek_search2.BackColor = System.Drawing.Color.White;
		this.ek_search2.Location = new System.Drawing.Point(131, 0);
		this.ek_search2.Name = "ek_search2";
		this.ek_search2.Size = new System.Drawing.Size(101, 16);
		this.ek_search2.TabIndex = 41;
		this.ek_search2.Text = "能力値から検索";
		this.ek_search2.UseVisualStyleBackColor = false;
		this.label158.AutoSize = true;
		this.label158.Location = new System.Drawing.Point(22, 226);
		this.label158.Name = "label158";
		this.label158.Size = new System.Drawing.Size(12, 12);
		this.label158.TabIndex = 48;
		this.label158.Text = "S";
		this.label159.AutoSize = true;
		this.label159.Location = new System.Drawing.Point(22, 194);
		this.label159.Name = "label159";
		this.label159.Size = new System.Drawing.Size(13, 12);
		this.label159.TabIndex = 46;
		this.label159.Text = "D";
		this.ek_search1.AutoSize = true;
		this.ek_search1.BackColor = System.Drawing.Color.White;
		this.ek_search1.Checked = true;
		this.ek_search1.Location = new System.Drawing.Point(24, 0);
		this.ek_search1.Name = "ek_search1";
		this.ek_search1.Size = new System.Drawing.Size(105, 16);
		this.ek_search1.TabIndex = 40;
		this.ek_search1.TabStop = true;
		this.ek_search1.Text = "個体値から検索 ";
		this.ek_search1.UseVisualStyleBackColor = false;
		this.ek_search1.CheckedChanged += new System.EventHandler(ek_search1_CheckedChanged);
		this.label162.AutoSize = true;
		this.label162.Location = new System.Drawing.Point(22, 162);
		this.label162.Name = "label162";
		this.label162.Size = new System.Drawing.Size(13, 12);
		this.label162.TabIndex = 44;
		this.label162.Text = "C";
		this.label164.AutoSize = true;
		this.label164.Location = new System.Drawing.Point(22, 130);
		this.label164.Name = "label164";
		this.label164.Size = new System.Drawing.Size(13, 12);
		this.label164.TabIndex = 42;
		this.label164.Text = "B";
		this.label165.AutoSize = true;
		this.label165.Location = new System.Drawing.Point(22, 98);
		this.label165.Name = "label165";
		this.label165.Size = new System.Drawing.Size(13, 12);
		this.label165.TabIndex = 40;
		this.label165.Text = "A";
		this.label166.AutoSize = true;
		this.label166.Location = new System.Drawing.Point(22, 66);
		this.label166.Name = "label166";
		this.label166.Size = new System.Drawing.Size(13, 12);
		this.label166.TabIndex = 38;
		this.label166.Text = "H";
		this.ek_pokedex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.ek_pokedex.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_pokedex.FormattingEnabled = true;
		this.ek_pokedex.Location = new System.Drawing.Point(70, 27);
		this.ek_pokedex.Name = "ek_pokedex";
		this.ek_pokedex.Size = new System.Drawing.Size(80, 22);
		this.ek_pokedex.TabIndex = 42;
		this.ek_pokedex.SelectedIndexChanged += new System.EventHandler(Ek_pokedex_SelectedIndexChanged);
		this.label167.AutoSize = true;
		this.label167.Location = new System.Drawing.Point(22, 30);
		this.label167.Name = "label167";
		this.label167.Size = new System.Drawing.Size(42, 12);
		this.label167.TabIndex = 64;
		this.label167.Text = "ポケモン";
		this.label168.AutoSize = true;
		this.label168.Location = new System.Drawing.Point(174, 30);
		this.label168.Name = "label168";
		this.label168.Size = new System.Drawing.Size(17, 12);
		this.label168.TabIndex = 67;
		this.label168.Text = "Lv";
		this.label168.Visible = false;
		this.ek_IVup6.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_IVup6.Location = new System.Drawing.Point(115, 221);
		this.ek_IVup6.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.ek_IVup6.Name = "ek_IVup6";
		this.ek_IVup6.Size = new System.Drawing.Size(45, 22);
		this.ek_IVup6.TabIndex = 55;
		this.ek_IVup6.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.ek_IVup6.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_IVup6.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.ek_IVlow1.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_IVlow1.Location = new System.Drawing.Point(41, 61);
		this.ek_IVlow1.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.ek_IVlow1.Name = "ek_IVlow1";
		this.ek_IVlow1.Size = new System.Drawing.Size(45, 22);
		this.ek_IVlow1.TabIndex = 44;
		this.ek_IVlow1.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_IVlow1.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label169.AutoSize = true;
		this.label169.Location = new System.Drawing.Point(92, 226);
		this.label169.Name = "label169";
		this.label169.Size = new System.Drawing.Size(17, 12);
		this.label169.TabIndex = 86;
		this.label169.Text = "～";
		this.ek_IVlow2.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_IVlow2.Location = new System.Drawing.Point(41, 93);
		this.ek_IVlow2.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.ek_IVlow2.Name = "ek_IVlow2";
		this.ek_IVlow2.Size = new System.Drawing.Size(45, 22);
		this.ek_IVlow2.TabIndex = 46;
		this.ek_IVlow2.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_IVlow2.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.ek_nature.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.ek_nature.FormattingEnabled = true;
		this.ek_nature.Location = new System.Drawing.Point(300, 27);
		this.ek_nature.Name = "ek_nature";
		this.ek_nature.Size = new System.Drawing.Size(80, 20);
		this.ek_nature.TabIndex = 62;
		this.ek_IVlow3.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_IVlow3.Location = new System.Drawing.Point(41, 125);
		this.ek_IVlow3.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.ek_IVlow3.Name = "ek_IVlow3";
		this.ek_IVlow3.Size = new System.Drawing.Size(45, 22);
		this.ek_IVlow3.TabIndex = 48;
		this.ek_IVlow3.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_IVlow3.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.ek_IVup5.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_IVup5.Location = new System.Drawing.Point(115, 189);
		this.ek_IVup5.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.ek_IVup5.Name = "ek_IVup5";
		this.ek_IVup5.Size = new System.Drawing.Size(45, 22);
		this.ek_IVup5.TabIndex = 53;
		this.ek_IVup5.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.ek_IVup5.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_IVup5.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.ek_IVlow4.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_IVlow4.Location = new System.Drawing.Point(41, 157);
		this.ek_IVlow4.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.ek_IVlow4.Name = "ek_IVlow4";
		this.ek_IVlow4.Size = new System.Drawing.Size(45, 22);
		this.ek_IVlow4.TabIndex = 50;
		this.ek_IVlow4.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_IVlow4.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label170.AutoSize = true;
		this.label170.Location = new System.Drawing.Point(92, 194);
		this.label170.Name = "label170";
		this.label170.Size = new System.Drawing.Size(17, 12);
		this.label170.TabIndex = 84;
		this.label170.Text = "～";
		this.ek_IVlow5.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_IVlow5.Location = new System.Drawing.Point(41, 189);
		this.ek_IVlow5.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.ek_IVlow5.Name = "ek_IVlow5";
		this.ek_IVlow5.Size = new System.Drawing.Size(45, 22);
		this.ek_IVlow5.TabIndex = 52;
		this.ek_IVlow5.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_IVlow5.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label171.AutoSize = true;
		this.label171.Location = new System.Drawing.Point(265, 30);
		this.label171.Name = "label171";
		this.label171.Size = new System.Drawing.Size(29, 12);
		this.label171.TabIndex = 66;
		this.label171.Text = "性格";
		this.ek_IVlow6.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_IVlow6.Location = new System.Drawing.Point(41, 221);
		this.ek_IVlow6.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.ek_IVlow6.Name = "ek_IVlow6";
		this.ek_IVlow6.Size = new System.Drawing.Size(45, 22);
		this.ek_IVlow6.TabIndex = 54;
		this.ek_IVlow6.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_IVlow6.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.ek_IVup4.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_IVup4.Location = new System.Drawing.Point(115, 157);
		this.ek_IVup4.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.ek_IVup4.Name = "ek_IVup4";
		this.ek_IVup4.Size = new System.Drawing.Size(45, 22);
		this.ek_IVup4.TabIndex = 51;
		this.ek_IVup4.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.ek_IVup4.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_IVup4.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label172.AutoSize = true;
		this.label172.Location = new System.Drawing.Point(92, 66);
		this.label172.Name = "label172";
		this.label172.Size = new System.Drawing.Size(17, 12);
		this.label172.TabIndex = 76;
		this.label172.Text = "～";
		this.label173.AutoSize = true;
		this.label173.Location = new System.Drawing.Point(92, 162);
		this.label173.Name = "label173";
		this.label173.Size = new System.Drawing.Size(17, 12);
		this.label173.TabIndex = 82;
		this.label173.Text = "～";
		this.ek_IVup1.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_IVup1.Location = new System.Drawing.Point(115, 61);
		this.ek_IVup1.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.ek_IVup1.Name = "ek_IVup1";
		this.ek_IVup1.Size = new System.Drawing.Size(45, 22);
		this.ek_IVup1.TabIndex = 45;
		this.ek_IVup1.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.ek_IVup1.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_IVup1.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.ek_IVup3.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_IVup3.Location = new System.Drawing.Point(115, 125);
		this.ek_IVup3.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.ek_IVup3.Name = "ek_IVup3";
		this.ek_IVup3.Size = new System.Drawing.Size(45, 22);
		this.ek_IVup3.TabIndex = 49;
		this.ek_IVup3.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.ek_IVup3.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_IVup3.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label174.AutoSize = true;
		this.label174.Location = new System.Drawing.Point(92, 98);
		this.label174.Name = "label174";
		this.label174.Size = new System.Drawing.Size(17, 12);
		this.label174.TabIndex = 78;
		this.label174.Text = "～";
		this.label175.AutoSize = true;
		this.label175.Location = new System.Drawing.Point(92, 130);
		this.label175.Name = "label175";
		this.label175.Size = new System.Drawing.Size(17, 12);
		this.label175.TabIndex = 80;
		this.label175.Text = "～";
		this.ek_IVup2.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_IVup2.Location = new System.Drawing.Point(115, 93);
		this.ek_IVup2.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.ek_IVup2.Name = "ek_IVup2";
		this.ek_IVup2.Size = new System.Drawing.Size(45, 22);
		this.ek_IVup2.TabIndex = 47;
		this.ek_IVup2.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.ek_IVup2.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_IVup2.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.ek_Lv.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ek_Lv.Location = new System.Drawing.Point(197, 25);
		this.ek_Lv.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.ek_Lv.Name = "ek_Lv";
		this.ek_Lv.Size = new System.Drawing.Size(50, 22);
		this.ek_Lv.TabIndex = 43;
		this.ek_Lv.Value = new decimal(new int[4] { 5, 0, 0, 0 });
		this.ek_Lv.Visible = false;
		this.ek_Lv.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ek_Lv.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.ek_groupBox2.Controls.Add(this.FrameRange_EggIVs);
		this.ek_groupBox2.Controls.Add(this.TargetFrame_EggIVs);
		this.ek_groupBox2.Controls.Add(this.LastFrame_EggIVs);
		this.ek_groupBox2.Controls.Add(this.FirstFrame_EggIVs);
		this.ek_groupBox2.Controls.Add(this.SetFrameButton_EggIVs);
		this.ek_groupBox2.Controls.Add(this.label124);
		this.ek_groupBox2.Controls.Add(this.label139);
		this.ek_groupBox2.Controls.Add(this.label146);
		this.ek_groupBox2.Controls.Add(this.label160);
		this.ek_groupBox2.Controls.Add(this.Method1);
		this.ek_groupBox2.Controls.Add(this.Method2);
		this.ek_groupBox2.Controls.Add(this.label163);
		this.ek_groupBox2.Controls.Add(this.Method3);
		this.ek_groupBox2.Location = new System.Drawing.Point(6, 124);
		this.ek_groupBox2.Name = "ek_groupBox2";
		this.ek_groupBox2.Size = new System.Drawing.Size(407, 178);
		this.ek_groupBox2.TabIndex = 0;
		this.ek_groupBox2.TabStop = false;
		this.ek_groupBox2.Text = "検索範囲";
		this.FrameRange_EggIVs.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.FrameRange_EggIVs.Location = new System.Drawing.Point(204, 59);
		this.FrameRange_EggIVs.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.FrameRange_EggIVs.Name = "FrameRange_EggIVs";
		this.FrameRange_EggIVs.Size = new System.Drawing.Size(64, 22);
		this.FrameRange_EggIVs.TabIndex = 16;
		this.FrameRange_EggIVs.Value = new decimal(new int[4] { 100, 0, 0, 0 });
		this.TargetFrame_EggIVs.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TargetFrame_EggIVs.Location = new System.Drawing.Point(81, 59);
		this.TargetFrame_EggIVs.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.TargetFrame_EggIVs.Name = "TargetFrame_EggIVs";
		this.TargetFrame_EggIVs.Size = new System.Drawing.Size(93, 22);
		this.TargetFrame_EggIVs.TabIndex = 15;
		this.TargetFrame_EggIVs.Value = new decimal(new int[4] { 73780, 0, 0, 0 });
		this.TargetFrame_EggIVs.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.LastFrame_EggIVs.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.LastFrame_EggIVs.Location = new System.Drawing.Point(204, 27);
		this.LastFrame_EggIVs.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.LastFrame_EggIVs.Name = "LastFrame_EggIVs";
		this.LastFrame_EggIVs.Size = new System.Drawing.Size(93, 22);
		this.LastFrame_EggIVs.TabIndex = 14;
		this.LastFrame_EggIVs.Value = new decimal(new int[4] { 73880, 0, 0, 0 });
		this.LastFrame_EggIVs.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.FirstFrame_EggIVs.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.FirstFrame_EggIVs.Location = new System.Drawing.Point(81, 27);
		this.FirstFrame_EggIVs.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.FirstFrame_EggIVs.Name = "FirstFrame_EggIVs";
		this.FirstFrame_EggIVs.Size = new System.Drawing.Size(93, 22);
		this.FirstFrame_EggIVs.TabIndex = 13;
		this.FirstFrame_EggIVs.Value = new decimal(new int[4] { 73680, 0, 0, 0 });
		this.FirstFrame_EggIVs.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.SetFrameButton_EggIVs.Location = new System.Drawing.Point(274, 57);
		this.SetFrameButton_EggIVs.Name = "SetFrameButton_EggIVs";
		this.SetFrameButton_EggIVs.Size = new System.Drawing.Size(23, 23);
		this.SetFrameButton_EggIVs.TabIndex = 17;
		this.SetFrameButton_EggIVs.Text = "↑";
		this.SetFrameButton_EggIVs.UseVisualStyleBackColor = true;
		this.SetFrameButton_EggIVs.Click += new System.EventHandler(SetFrameButton_EggIVs_Click);
		this.label124.AutoSize = true;
		this.label124.Location = new System.Drawing.Point(302, 64);
		this.label124.Name = "label124";
		this.label124.Size = new System.Drawing.Size(73, 12);
		this.label124.TabIndex = 121;
		this.label124.Text = "(リスト表示用)";
		this.label139.Location = new System.Drawing.Point(180, 63);
		this.label139.Name = "label139";
		this.label139.Size = new System.Drawing.Size(18, 16);
		this.label139.TabIndex = 120;
		this.label139.Text = "±";
		this.label146.Location = new System.Drawing.Point(180, 32);
		this.label146.Name = "label146";
		this.label146.Size = new System.Drawing.Size(18, 16);
		this.label146.TabIndex = 119;
		this.label146.Text = "～";
		this.label160.AutoSize = true;
		this.label160.Location = new System.Drawing.Point(39, 62);
		this.label160.Name = "label160";
		this.label160.Size = new System.Drawing.Size(36, 12);
		this.label160.TabIndex = 68;
		this.label160.Text = "目標F";
		this.Method1.AutoSize = true;
		this.Method1.Checked = true;
		this.Method1.CheckState = System.Windows.Forms.CheckState.Checked;
		this.Method1.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Method1.Location = new System.Drawing.Point(24, 98);
		this.Method1.Name = "Method1";
		this.Method1.Size = new System.Drawing.Size(75, 18);
		this.Method1.TabIndex = 25;
		this.Method1.Text = "Method1";
		this.Method1.UseVisualStyleBackColor = true;
		this.Method1.Click += new System.EventHandler(Method1_Click);
		this.Method2.AutoSize = true;
		this.Method2.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Method2.Location = new System.Drawing.Point(105, 98);
		this.Method2.Name = "Method2";
		this.Method2.Size = new System.Drawing.Size(75, 18);
		this.Method2.TabIndex = 26;
		this.Method2.Text = "Method2";
		this.Method2.UseVisualStyleBackColor = true;
		this.Method2.Click += new System.EventHandler(Method2_Click);
		this.label163.AutoSize = true;
		this.label163.Location = new System.Drawing.Point(63, 30);
		this.label163.Name = "label163";
		this.label163.Size = new System.Drawing.Size(12, 12);
		this.label163.TabIndex = 64;
		this.label163.Text = "F";
		this.Method3.AutoSize = true;
		this.Method3.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Method3.Location = new System.Drawing.Point(186, 98);
		this.Method3.Name = "Method3";
		this.Method3.Size = new System.Drawing.Size(75, 18);
		this.Method3.TabIndex = 27;
		this.Method3.Text = "Method3";
		this.Method3.UseVisualStyleBackColor = true;
		this.Method3.Click += new System.EventHandler(Method3_Click);
		this.ek_cancel.Enabled = false;
		this.ek_cancel.Location = new System.Drawing.Point(581, 544);
		this.ek_cancel.Name = "ek_cancel";
		this.ek_cancel.Size = new System.Drawing.Size(75, 23);
		this.ek_cancel.TabIndex = 72;
		this.ek_cancel.Text = "キャンセル";
		this.ek_cancel.UseVisualStyleBackColor = true;
		this.ek_cancel.Click += new System.EventHandler(Cancel_EggIVs_Click);
		this.ek_listup.Location = new System.Drawing.Point(500, 544);
		this.ek_listup.Name = "ek_listup";
		this.ek_listup.Size = new System.Drawing.Size(75, 23);
		this.ek_listup.TabIndex = 71;
		this.ek_listup.Text = "リスト表示";
		this.ek_listup.UseVisualStyleBackColor = true;
		this.ek_listup.Click += new System.EventHandler(ListUp_EggIVs_Click);
		this.ek_groupBox1.Controls.Add(this.label177);
		this.ek_groupBox1.Controls.Add(this.pre_parent6);
		this.ek_groupBox1.Controls.Add(this.label178);
		this.ek_groupBox1.Controls.Add(this.pre_parent5);
		this.ek_groupBox1.Controls.Add(this.label179);
		this.ek_groupBox1.Controls.Add(this.pre_parent4);
		this.ek_groupBox1.Controls.Add(this.label153);
		this.ek_groupBox1.Controls.Add(this.pre_parent3);
		this.ek_groupBox1.Controls.Add(this.label152);
		this.ek_groupBox1.Controls.Add(this.pre_parent2);
		this.ek_groupBox1.Controls.Add(this.post_parent6);
		this.ek_groupBox1.Controls.Add(this.post_parent5);
		this.ek_groupBox1.Controls.Add(this.post_parent4);
		this.ek_groupBox1.Controls.Add(this.post_parent3);
		this.ek_groupBox1.Controls.Add(this.post_parent2);
		this.ek_groupBox1.Controls.Add(this.post_parent1);
		this.ek_groupBox1.Controls.Add(this.label199);
		this.ek_groupBox1.Controls.Add(this.label201);
		this.ek_groupBox1.Controls.Add(this.pre_parent1);
		this.ek_groupBox1.Controls.Add(this.label150);
		this.ek_groupBox1.Location = new System.Drawing.Point(6, 6);
		this.ek_groupBox1.Name = "ek_groupBox1";
		this.ek_groupBox1.Size = new System.Drawing.Size(407, 112);
		this.ek_groupBox1.TabIndex = 0;
		this.ek_groupBox1.TabStop = false;
		this.ek_groupBox1.Text = "両親の個体値";
		this.label177.AutoSize = true;
		this.label177.Location = new System.Drawing.Point(328, 23);
		this.label177.Name = "label177";
		this.label177.Size = new System.Drawing.Size(12, 12);
		this.label177.TabIndex = 139;
		this.label177.Text = "S";
		this.pre_parent6.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.pre_parent6.ForeColor = System.Drawing.Color.Red;
		this.pre_parent6.Location = new System.Drawing.Point(312, 38);
		this.pre_parent6.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.pre_parent6.Name = "pre_parent6";
		this.pre_parent6.Size = new System.Drawing.Size(45, 22);
		this.pre_parent6.TabIndex = 6;
		this.pre_parent6.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.pre_parent6.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.pre_parent6.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label178.AutoSize = true;
		this.label178.Location = new System.Drawing.Point(277, 23);
		this.label178.Name = "label178";
		this.label178.Size = new System.Drawing.Size(13, 12);
		this.label178.TabIndex = 137;
		this.label178.Text = "D";
		this.pre_parent5.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.pre_parent5.ForeColor = System.Drawing.Color.Red;
		this.pre_parent5.Location = new System.Drawing.Point(261, 38);
		this.pre_parent5.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.pre_parent5.Name = "pre_parent5";
		this.pre_parent5.Size = new System.Drawing.Size(45, 22);
		this.pre_parent5.TabIndex = 5;
		this.pre_parent5.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.pre_parent5.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.pre_parent5.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label179.AutoSize = true;
		this.label179.Location = new System.Drawing.Point(226, 23);
		this.label179.Name = "label179";
		this.label179.Size = new System.Drawing.Size(13, 12);
		this.label179.TabIndex = 135;
		this.label179.Text = "C";
		this.pre_parent4.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.pre_parent4.ForeColor = System.Drawing.Color.Red;
		this.pre_parent4.Location = new System.Drawing.Point(210, 38);
		this.pre_parent4.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.pre_parent4.Name = "pre_parent4";
		this.pre_parent4.Size = new System.Drawing.Size(45, 22);
		this.pre_parent4.TabIndex = 4;
		this.pre_parent4.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.pre_parent4.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.pre_parent4.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label153.AutoSize = true;
		this.label153.Location = new System.Drawing.Point(175, 23);
		this.label153.Name = "label153";
		this.label153.Size = new System.Drawing.Size(13, 12);
		this.label153.TabIndex = 133;
		this.label153.Text = "B";
		this.pre_parent3.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.pre_parent3.ForeColor = System.Drawing.Color.Red;
		this.pre_parent3.Location = new System.Drawing.Point(159, 38);
		this.pre_parent3.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.pre_parent3.Name = "pre_parent3";
		this.pre_parent3.Size = new System.Drawing.Size(45, 22);
		this.pre_parent3.TabIndex = 3;
		this.pre_parent3.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.pre_parent3.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.pre_parent3.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label152.AutoSize = true;
		this.label152.Location = new System.Drawing.Point(124, 23);
		this.label152.Name = "label152";
		this.label152.Size = new System.Drawing.Size(13, 12);
		this.label152.TabIndex = 131;
		this.label152.Text = "A";
		this.pre_parent2.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.pre_parent2.ForeColor = System.Drawing.Color.Red;
		this.pre_parent2.Location = new System.Drawing.Point(108, 38);
		this.pre_parent2.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.pre_parent2.Name = "pre_parent2";
		this.pre_parent2.Size = new System.Drawing.Size(45, 22);
		this.pre_parent2.TabIndex = 2;
		this.pre_parent2.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.pre_parent2.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.pre_parent2.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.post_parent6.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.post_parent6.ForeColor = System.Drawing.Color.DodgerBlue;
		this.post_parent6.Location = new System.Drawing.Point(311, 70);
		this.post_parent6.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.post_parent6.Name = "post_parent6";
		this.post_parent6.Size = new System.Drawing.Size(45, 22);
		this.post_parent6.TabIndex = 12;
		this.post_parent6.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.post_parent6.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.post_parent6.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.post_parent5.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.post_parent5.ForeColor = System.Drawing.Color.DodgerBlue;
		this.post_parent5.Location = new System.Drawing.Point(261, 70);
		this.post_parent5.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.post_parent5.Name = "post_parent5";
		this.post_parent5.Size = new System.Drawing.Size(45, 22);
		this.post_parent5.TabIndex = 11;
		this.post_parent5.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.post_parent5.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.post_parent5.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.post_parent4.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.post_parent4.ForeColor = System.Drawing.Color.DodgerBlue;
		this.post_parent4.Location = new System.Drawing.Point(210, 70);
		this.post_parent4.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.post_parent4.Name = "post_parent4";
		this.post_parent4.Size = new System.Drawing.Size(45, 22);
		this.post_parent4.TabIndex = 10;
		this.post_parent4.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.post_parent4.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.post_parent4.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.post_parent3.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.post_parent3.ForeColor = System.Drawing.Color.DodgerBlue;
		this.post_parent3.Location = new System.Drawing.Point(159, 70);
		this.post_parent3.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.post_parent3.Name = "post_parent3";
		this.post_parent3.Size = new System.Drawing.Size(45, 22);
		this.post_parent3.TabIndex = 9;
		this.post_parent3.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.post_parent3.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.post_parent3.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.post_parent2.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.post_parent2.ForeColor = System.Drawing.Color.DodgerBlue;
		this.post_parent2.Location = new System.Drawing.Point(108, 70);
		this.post_parent2.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.post_parent2.Name = "post_parent2";
		this.post_parent2.Size = new System.Drawing.Size(45, 22);
		this.post_parent2.TabIndex = 8;
		this.post_parent2.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.post_parent2.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.post_parent2.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.post_parent1.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.post_parent1.ForeColor = System.Drawing.Color.DodgerBlue;
		this.post_parent1.Location = new System.Drawing.Point(57, 70);
		this.post_parent1.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.post_parent1.Name = "post_parent1";
		this.post_parent1.Size = new System.Drawing.Size(45, 22);
		this.post_parent1.TabIndex = 7;
		this.post_parent1.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.post_parent1.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.post_parent1.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label199.AutoSize = true;
		this.label199.Location = new System.Drawing.Point(73, 23);
		this.label199.Name = "label199";
		this.label199.Size = new System.Drawing.Size(13, 12);
		this.label199.TabIndex = 12;
		this.label199.Text = "H";
		this.label201.AutoSize = true;
		this.label201.ForeColor = System.Drawing.Color.Red;
		this.label201.Location = new System.Drawing.Point(22, 43);
		this.label201.Name = "label201";
		this.label201.Size = new System.Drawing.Size(29, 12);
		this.label201.TabIndex = 9;
		this.label201.Text = "前親";
		this.pre_parent1.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.pre_parent1.ForeColor = System.Drawing.Color.Red;
		this.pre_parent1.Location = new System.Drawing.Point(57, 38);
		this.pre_parent1.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.pre_parent1.Name = "pre_parent1";
		this.pre_parent1.Size = new System.Drawing.Size(45, 22);
		this.pre_parent1.TabIndex = 1;
		this.pre_parent1.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.pre_parent1.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.pre_parent1.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label150.AutoSize = true;
		this.label150.ForeColor = System.Drawing.Color.DodgerBlue;
		this.label150.Location = new System.Drawing.Point(22, 75);
		this.label150.Name = "label150";
		this.label150.Size = new System.Drawing.Size(29, 12);
		this.label150.TabIndex = 124;
		this.label150.Text = "後親";
		this.ek_dataGridView.AllowUserToAddRows = false;
		this.ek_dataGridView.AllowUserToDeleteRows = false;
		this.ek_dataGridView.AllowUserToResizeRows = false;
		dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
		this.ek_dataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle2;
		this.ek_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.ek_dataGridView.ContextMenuStrip = this.contextMenuStrip5;
		this.ek_dataGridView.Location = new System.Drawing.Point(419, 6);
		this.ek_dataGridView.Name = "ek_dataGridView";
		this.ek_dataGridView.ReadOnly = true;
		this.ek_dataGridView.RowHeadersWidth = 30;
		this.ek_dataGridView.RowTemplate.Height = 21;
		this.ek_dataGridView.Size = new System.Drawing.Size(640, 532);
		this.ek_dataGridView.TabIndex = 0;
		this.ek_dataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(Ek_dataGridView_CellFormatting);
		this.ek_dataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(dataGridView_MouseDown);
		this.ek_start.Location = new System.Drawing.Point(419, 544);
		this.ek_start.Name = "ek_start";
		this.ek_start.Size = new System.Drawing.Size(75, 23);
		this.ek_start.TabIndex = 70;
		this.ek_start.Text = "検索";
		this.ek_start.UseVisualStyleBackColor = true;
		this.ek_start.Click += new System.EventHandler(Search_EggIVs_Click);
		this.TabPage_ID.Controls.Add(this.ID_groupBox1);
		this.TabPage_ID.Controls.Add(this.ID_groupBox2);
		this.TabPage_ID.Controls.Add(this.ID_cancel);
		this.TabPage_ID.Controls.Add(this.ID_listup);
		this.TabPage_ID.Controls.Add(this.ID_start);
		this.TabPage_ID.Controls.Add(this.ID_dataGridView);
		this.TabPage_ID.Location = new System.Drawing.Point(4, 22);
		this.TabPage_ID.Name = "TabPage_ID";
		this.TabPage_ID.Padding = new System.Windows.Forms.Padding(3);
		this.TabPage_ID.Size = new System.Drawing.Size(1085, 611);
		this.TabPage_ID.TabIndex = 2;
		this.TabPage_ID.Text = "ID調整";
		this.TabPage_ID.UseVisualStyleBackColor = true;
		this.ID_groupBox1.Controls.Add(this.FrameRange_ID);
		this.ID_groupBox1.Controls.Add(this.TargetFrame_ID);
		this.ID_groupBox1.Controls.Add(this.LastFrame_ID);
		this.ID_groupBox1.Controls.Add(this.FirstFrame_ID);
		this.ID_groupBox1.Controls.Add(this.SetFrameButton_ID);
		this.ID_groupBox1.Controls.Add(this.label132);
		this.ID_groupBox1.Controls.Add(this.label134);
		this.ID_groupBox1.Controls.Add(this.label136);
		this.ID_groupBox1.Controls.Add(this.label129);
		this.ID_groupBox1.Controls.Add(this.ID_RSmax);
		this.ID_groupBox1.Controls.Add(this.ID_RSmin);
		this.ID_groupBox1.Controls.Add(this.label130);
		this.ID_groupBox1.Controls.Add(this.IDInitialseed2);
		this.ID_groupBox1.Controls.Add(this.ID_Initialseed1);
		this.ID_groupBox1.Controls.Add(this.IDInitialseed1);
		this.ID_groupBox1.Controls.Add(this.label131);
		this.ID_groupBox1.Controls.Add(this.label133);
		this.ID_groupBox1.Controls.Add(this.label135);
		this.ID_groupBox1.Location = new System.Drawing.Point(6, 6);
		this.ID_groupBox1.Name = "ID_groupBox1";
		this.ID_groupBox1.Size = new System.Drawing.Size(387, 166);
		this.ID_groupBox1.TabIndex = 0;
		this.ID_groupBox1.TabStop = false;
		this.ID_groupBox1.Text = "検索範囲";
		this.FrameRange_ID.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.FrameRange_ID.Location = new System.Drawing.Point(210, 128);
		this.FrameRange_ID.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.FrameRange_ID.Name = "FrameRange_ID";
		this.FrameRange_ID.Size = new System.Drawing.Size(64, 22);
		this.FrameRange_ID.TabIndex = 9;
		this.FrameRange_ID.Value = new decimal(new int[4] { 100, 0, 0, 0 });
		this.FrameRange_ID.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.TargetFrame_ID.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TargetFrame_ID.Location = new System.Drawing.Point(87, 128);
		this.TargetFrame_ID.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.TargetFrame_ID.Name = "TargetFrame_ID";
		this.TargetFrame_ID.Size = new System.Drawing.Size(93, 22);
		this.TargetFrame_ID.TabIndex = 8;
		this.TargetFrame_ID.Value = new decimal(new int[4] { 19395, 0, 0, 0 });
		this.TargetFrame_ID.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.LastFrame_ID.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.LastFrame_ID.Location = new System.Drawing.Point(210, 96);
		this.LastFrame_ID.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.LastFrame_ID.Name = "LastFrame_ID";
		this.LastFrame_ID.Size = new System.Drawing.Size(93, 22);
		this.LastFrame_ID.TabIndex = 7;
		this.LastFrame_ID.Value = new decimal(new int[4] { 19495, 0, 0, 0 });
		this.LastFrame_ID.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.FirstFrame_ID.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.FirstFrame_ID.Location = new System.Drawing.Point(87, 96);
		this.FirstFrame_ID.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.FirstFrame_ID.Name = "FirstFrame_ID";
		this.FirstFrame_ID.Size = new System.Drawing.Size(93, 22);
		this.FirstFrame_ID.TabIndex = 6;
		this.FirstFrame_ID.Value = new decimal(new int[4] { 19295, 0, 0, 0 });
		this.FirstFrame_ID.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.SetFrameButton_ID.Location = new System.Drawing.Point(280, 126);
		this.SetFrameButton_ID.Name = "SetFrameButton_ID";
		this.SetFrameButton_ID.Size = new System.Drawing.Size(23, 23);
		this.SetFrameButton_ID.TabIndex = 10;
		this.SetFrameButton_ID.Text = "↑";
		this.SetFrameButton_ID.UseVisualStyleBackColor = true;
		this.SetFrameButton_ID.Click += new System.EventHandler(SetFrameButton_ID_Click);
		this.label132.AutoSize = true;
		this.label132.Location = new System.Drawing.Point(308, 131);
		this.label132.Name = "label132";
		this.label132.Size = new System.Drawing.Size(73, 12);
		this.label132.TabIndex = 129;
		this.label132.Text = "(リスト表示用)";
		this.label134.Location = new System.Drawing.Point(186, 131);
		this.label134.Name = "label134";
		this.label134.Size = new System.Drawing.Size(18, 16);
		this.label134.TabIndex = 128;
		this.label134.Text = "±";
		this.label136.Location = new System.Drawing.Point(186, 99);
		this.label136.Name = "label136";
		this.label136.Size = new System.Drawing.Size(18, 16);
		this.label136.TabIndex = 127;
		this.label136.Text = "～";
		this.label129.AutoSize = true;
		this.label129.Location = new System.Drawing.Point(217, 62);
		this.label129.Name = "label129";
		this.label129.Size = new System.Drawing.Size(33, 12);
		this.label129.TabIndex = 104;
		this.label129.Text = "分 ～";
		this.ID_RSmax.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ID_RSmax.Location = new System.Drawing.Point(256, 57);
		this.ID_RSmax.Maximum = new decimal(new int[4] { 63349, 0, 0, 0 });
		this.ID_RSmax.Name = "ID_RSmax";
		this.ID_RSmax.Size = new System.Drawing.Size(80, 22);
		this.ID_RSmax.TabIndex = 5;
		this.ID_RSmax.Value = new decimal(new int[4] { 3, 0, 0, 0 });
		this.ID_RSmax.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ID_RSmax.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.ID_RSmin.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ID_RSmin.Location = new System.Drawing.Point(131, 57);
		this.ID_RSmin.Maximum = new decimal(new int[4] { 63349, 0, 0, 0 });
		this.ID_RSmin.Name = "ID_RSmin";
		this.ID_RSmin.Size = new System.Drawing.Size(80, 22);
		this.ID_RSmin.TabIndex = 4;
		this.ID_RSmin.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ID_RSmin.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label130.AutoSize = true;
		this.label130.Location = new System.Drawing.Point(342, 62);
		this.label130.Name = "label130";
		this.label130.Size = new System.Drawing.Size(17, 12);
		this.label130.TabIndex = 79;
		this.label130.Text = "分";
		this.IDInitialseed2.AutoSize = true;
		this.IDInitialseed2.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.IDInitialseed2.Location = new System.Drawing.Point(90, 59);
		this.IDInitialseed2.Name = "IDInitialseed2";
		this.IDInitialseed2.Size = new System.Drawing.Size(39, 18);
		this.IDInitialseed2.TabIndex = 3;
		this.IDInitialseed2.Text = "  ";
		this.IDInitialseed2.UseVisualStyleBackColor = true;
		this.IDInitialseed2.CheckedChanged += new System.EventHandler(RadioButton_Checked);
		this.ID_Initialseed1.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ID_Initialseed1.Location = new System.Drawing.Point(131, 25);
		this.ID_Initialseed1.Name = "ID_Initialseed1";
		this.ID_Initialseed1.Size = new System.Drawing.Size(80, 22);
		this.ID_Initialseed1.TabIndex = 2;
		this.ID_Initialseed1.Text = "5a0";
		this.ID_Initialseed1.Enter += new System.EventHandler(TextBox_SelectText);
		this.ID_Initialseed1.Validating += new System.ComponentModel.CancelEventHandler(InitialSeedBox_Check);
		this.IDInitialseed1.AutoSize = true;
		this.IDInitialseed1.Checked = true;
		this.IDInitialseed1.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.IDInitialseed1.Location = new System.Drawing.Point(90, 27);
		this.IDInitialseed1.Name = "IDInitialseed1";
		this.IDInitialseed1.Size = new System.Drawing.Size(39, 18);
		this.IDInitialseed1.TabIndex = 1;
		this.IDInitialseed1.TabStop = true;
		this.IDInitialseed1.Text = "0x";
		this.IDInitialseed1.UseVisualStyleBackColor = true;
		this.IDInitialseed1.CheckedChanged += new System.EventHandler(RadioButton_Checked);
		this.label131.AutoSize = true;
		this.label131.Location = new System.Drawing.Point(22, 30);
		this.label131.Name = "label131";
		this.label131.Size = new System.Drawing.Size(53, 12);
		this.label131.TabIndex = 70;
		this.label131.Text = "初期seed";
		this.label133.AutoSize = true;
		this.label133.Location = new System.Drawing.Point(39, 131);
		this.label133.Name = "label133";
		this.label133.Size = new System.Drawing.Size(36, 12);
		this.label133.TabIndex = 68;
		this.label133.Text = "目標F";
		this.label135.AutoSize = true;
		this.label135.Location = new System.Drawing.Point(63, 99);
		this.label135.Name = "label135";
		this.label135.Size = new System.Drawing.Size(12, 12);
		this.label135.TabIndex = 64;
		this.label135.Text = "F";
		this.ID_groupBox2.Controls.Add(this.label137);
		this.ID_groupBox2.Controls.Add(this.label128);
		this.ID_groupBox2.Controls.Add(this.CheckSID);
		this.ID_groupBox2.Controls.Add(this.CheckTID);
		this.ID_groupBox2.Controls.Add(this.TID_ID);
		this.ID_groupBox2.Controls.Add(this.SID_ID);
		this.ID_groupBox2.Controls.Add(this.ID_PID);
		this.ID_groupBox2.Controls.Add(this.ID_shiny);
		this.ID_groupBox2.Location = new System.Drawing.Point(6, 178);
		this.ID_groupBox2.Name = "ID_groupBox2";
		this.ID_groupBox2.Size = new System.Drawing.Size(387, 224);
		this.ID_groupBox2.TabIndex = 0;
		this.ID_groupBox2.TabStop = false;
		this.ID_groupBox2.Text = "フィルター";
		this.label137.AutoSize = true;
		this.label137.Location = new System.Drawing.Point(44, 106);
		this.label137.Name = "label137";
		this.label137.Size = new System.Drawing.Size(41, 12);
		this.label137.TabIndex = 131;
		this.label137.Text = "性格値";
		this.label128.AutoSize = true;
		this.label128.Location = new System.Drawing.Point(89, 161);
		this.label128.Name = "label128";
		this.label128.Size = new System.Drawing.Size(198, 12);
		this.label128.TabIndex = 130;
		this.label128.Text = "(いずれかが色違いになるIDを出力します)";
		this.CheckSID.AutoSize = true;
		this.CheckSID.Location = new System.Drawing.Point(28, 57);
		this.CheckSID.Name = "CheckSID";
		this.CheckSID.Size = new System.Drawing.Size(47, 16);
		this.CheckSID.TabIndex = 109;
		this.CheckSID.TabStop = false;
		this.CheckSID.Text = "裏ID";
		this.CheckSID.UseVisualStyleBackColor = true;
		this.CheckTID.AutoSize = true;
		this.CheckTID.Location = new System.Drawing.Point(28, 29);
		this.CheckTID.Name = "CheckTID";
		this.CheckTID.Size = new System.Drawing.Size(47, 16);
		this.CheckTID.TabIndex = 108;
		this.CheckTID.TabStop = false;
		this.CheckTID.Text = "表ID";
		this.CheckTID.UseVisualStyleBackColor = true;
		this.TID_ID.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TID_ID.Location = new System.Drawing.Point(93, 27);
		this.TID_ID.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.TID_ID.Name = "TID_ID";
		this.TID_ID.Size = new System.Drawing.Size(80, 22);
		this.TID_ID.TabIndex = 11;
		this.TID_ID.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.SID_ID.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.SID_ID.Location = new System.Drawing.Point(93, 55);
		this.SID_ID.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.SID_ID.Name = "SID_ID";
		this.SID_ID.Size = new System.Drawing.Size(80, 22);
		this.SID_ID.TabIndex = 12;
		this.SID_ID.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ID_PID.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ID_PID.Location = new System.Drawing.Point(91, 102);
		this.ID_PID.Multiline = true;
		this.ID_PID.Name = "ID_PID";
		this.ID_PID.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.ID_PID.Size = new System.Drawing.Size(100, 56);
		this.ID_PID.TabIndex = 14;
		this.ID_PID.TabStop = false;
		this.ID_PID.Enter += new System.EventHandler(TextBox_SelectText);
		this.ID_shiny.AutoSize = true;
		this.ID_shiny.Location = new System.Drawing.Point(26, 83);
		this.ID_shiny.Name = "ID_shiny";
		this.ID_shiny.Size = new System.Drawing.Size(103, 16);
		this.ID_shiny.TabIndex = 13;
		this.ID_shiny.Text = "色違いのみ出力";
		this.ID_shiny.UseVisualStyleBackColor = true;
		this.ID_cancel.Enabled = false;
		this.ID_cancel.Location = new System.Drawing.Point(561, 582);
		this.ID_cancel.Name = "ID_cancel";
		this.ID_cancel.Size = new System.Drawing.Size(75, 23);
		this.ID_cancel.TabIndex = 22;
		this.ID_cancel.Text = "キャンセル";
		this.ID_cancel.UseVisualStyleBackColor = true;
		this.ID_cancel.Click += new System.EventHandler(ID_cancel_Click);
		this.ID_listup.Location = new System.Drawing.Point(480, 582);
		this.ID_listup.Name = "ID_listup";
		this.ID_listup.Size = new System.Drawing.Size(75, 23);
		this.ID_listup.TabIndex = 21;
		this.ID_listup.Text = "リスト表示";
		this.ID_listup.UseVisualStyleBackColor = true;
		this.ID_listup.Click += new System.EventHandler(ID_list_Click);
		this.ID_start.Location = new System.Drawing.Point(399, 582);
		this.ID_start.Name = "ID_start";
		this.ID_start.Size = new System.Drawing.Size(75, 23);
		this.ID_start.TabIndex = 20;
		this.ID_start.Text = "計算";
		this.ID_start.UseVisualStyleBackColor = true;
		this.ID_start.Click += new System.EventHandler(ID_start_Click);
		this.ID_dataGridView.AllowUserToAddRows = false;
		this.ID_dataGridView.AllowUserToDeleteRows = false;
		this.ID_dataGridView.AllowUserToResizeRows = false;
		dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
		this.ID_dataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
		this.ID_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.ID_dataGridView.ContextMenuStrip = this.contextMenuStrip3;
		this.ID_dataGridView.Location = new System.Drawing.Point(399, 6);
		this.ID_dataGridView.Name = "ID_dataGridView";
		this.ID_dataGridView.ReadOnly = true;
		this.ID_dataGridView.RowHeadersWidth = 30;
		this.ID_dataGridView.RowTemplate.Height = 21;
		this.ID_dataGridView.Size = new System.Drawing.Size(680, 570);
		this.ID_dataGridView.TabIndex = 0;
		this.ID_dataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(dataGridView_MouseDown);
		this.TabPage_wild.Controls.Add(this.NaturePanel_wild);
		this.TabPage_wild.Controls.Add(this.panel2);
		this.TabPage_wild.Controls.Add(this.groupBox6);
		this.TabPage_wild.Controls.Add(this.y_dataGridView);
		this.TabPage_wild.Controls.Add(this.y_table_display);
		this.TabPage_wild.Controls.Add(this.y_groupBox2);
		this.TabPage_wild.Controls.Add(this.y_groupBox1);
		this.TabPage_wild.Controls.Add(this.y_cancel);
		this.TabPage_wild.Controls.Add(this.y_listup);
		this.TabPage_wild.Controls.Add(this.y_start);
		this.TabPage_wild.Controls.Add(this.groupBox8);
		this.TabPage_wild.Controls.Add(this.dataGridView_table);
		this.TabPage_wild.Controls.Add(this.groupBox7);
		this.TabPage_wild.Controls.Add(this.groupBox9);
		this.TabPage_wild.Controls.Add(this.y_groupBox3);
		this.TabPage_wild.Location = new System.Drawing.Point(4, 22);
		this.TabPage_wild.Name = "TabPage_wild";
		this.TabPage_wild.Padding = new System.Windows.Forms.Padding(3);
		this.TabPage_wild.Size = new System.Drawing.Size(1085, 611);
		this.TabPage_wild.TabIndex = 1;
		this.TabPage_wild.Text = "野生";
		this.TabPage_wild.UseVisualStyleBackColor = true;
		this.panel2.Controls.Add(this.CheckAppearing_wild);
		this.panel2.Location = new System.Drawing.Point(848, 506);
		this.panel2.Name = "panel2";
		this.panel2.Size = new System.Drawing.Size(132, 20);
		this.panel2.TabIndex = 123;
		this.CheckAppearing_wild.AutoSize = true;
		this.CheckAppearing_wild.Location = new System.Drawing.Point(5, 3);
		this.CheckAppearing_wild.Name = "CheckAppearing_wild";
		this.CheckAppearing_wild.Size = new System.Drawing.Size(124, 16);
		this.CheckAppearing_wild.TabIndex = 84;
		this.CheckAppearing_wild.Text = "出現判定を考慮する";
		this.CheckAppearing_wild.UseVisualStyleBackColor = true;
		this.CheckAppearing_wild.CheckedChanged += new System.EventHandler(CheckAppearing_wild_CheckedChanged);
		this.groupBox6.Controls.Add(this.RidingBicycle_wild);
		this.groupBox6.Controls.Add(this.BlackFlute_wild);
		this.groupBox6.Controls.Add(this.OFUDA_wild);
		this.groupBox6.Controls.Add(this.WhiteFlute_wild);
		this.groupBox6.Location = new System.Drawing.Point(842, 508);
		this.groupBox6.Name = "groupBox6";
		this.groupBox6.Size = new System.Drawing.Size(237, 97);
		this.groupBox6.TabIndex = 115;
		this.groupBox6.TabStop = false;
		this.RidingBicycle_wild.AutoSize = true;
		this.RidingBicycle_wild.Enabled = false;
		this.RidingBicycle_wild.Location = new System.Drawing.Point(11, 29);
		this.RidingBicycle_wild.Name = "RidingBicycle_wild";
		this.RidingBicycle_wild.Size = new System.Drawing.Size(60, 16);
		this.RidingBicycle_wild.TabIndex = 85;
		this.RidingBicycle_wild.Text = "自転車";
		this.RidingBicycle_wild.UseVisualStyleBackColor = true;
		this.BlackFlute_wild.AutoSize = true;
		this.BlackFlute_wild.Enabled = false;
		this.BlackFlute_wild.Location = new System.Drawing.Point(120, 28);
		this.BlackFlute_wild.Name = "BlackFlute_wild";
		this.BlackFlute_wild.Size = new System.Drawing.Size(85, 16);
		this.BlackFlute_wild.TabIndex = 86;
		this.BlackFlute_wild.Text = "くろいビードロ";
		this.BlackFlute_wild.UseVisualStyleBackColor = true;
		this.BlackFlute_wild.Click += new System.EventHandler(BlackFlute_wild_Click);
		this.OFUDA_wild.AutoSize = true;
		this.OFUDA_wild.Enabled = false;
		this.OFUDA_wild.Location = new System.Drawing.Point(11, 59);
		this.OFUDA_wild.Name = "OFUDA_wild";
		this.OFUDA_wild.Size = new System.Drawing.Size(91, 16);
		this.OFUDA_wild.TabIndex = 88;
		this.OFUDA_wild.Text = "きよめのおふだ";
		this.OFUDA_wild.UseVisualStyleBackColor = true;
		this.WhiteFlute_wild.AutoSize = true;
		this.WhiteFlute_wild.Enabled = false;
		this.WhiteFlute_wild.Location = new System.Drawing.Point(120, 59);
		this.WhiteFlute_wild.Name = "WhiteFlute_wild";
		this.WhiteFlute_wild.Size = new System.Drawing.Size(88, 16);
		this.WhiteFlute_wild.TabIndex = 87;
		this.WhiteFlute_wild.Text = "しろいビードロ";
		this.WhiteFlute_wild.UseVisualStyleBackColor = true;
		this.WhiteFlute_wild.Click += new System.EventHandler(WhiteFlute_wild_Click);
		this.y_dataGridView.AllowUserToAddRows = false;
		this.y_dataGridView.AllowUserToDeleteRows = false;
		this.y_dataGridView.AllowUserToResizeRows = false;
		dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
		this.y_dataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle4;
		this.y_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.y_dataGridView.ContextMenuStrip = this.contextMenuStrip2;
		this.y_dataGridView.Location = new System.Drawing.Point(6, 6);
		this.y_dataGridView.Name = "y_dataGridView";
		this.y_dataGridView.ReadOnly = true;
		this.y_dataGridView.RowHeadersWidth = 30;
		this.y_dataGridView.RowTemplate.Height = 21;
		this.y_dataGridView.Size = new System.Drawing.Size(1073, 200);
		this.y_dataGridView.TabIndex = 0;
		this.y_dataGridView.TabStop = false;
		this.y_dataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(Y_dataGridView_CellFormatting);
		this.y_dataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(dataGridView_MouseDown);
		this.y_table_display.BackColor = System.Drawing.Color.Silver;
		this.y_table_display.Location = new System.Drawing.Point(842, 346);
		this.y_table_display.Name = "y_table_display";
		this.y_table_display.Size = new System.Drawing.Size(237, 56);
		this.y_table_display.TabIndex = 70;
		this.y_table_display.Text = "出現リストを隠す";
		this.y_table_display.UseVisualStyleBackColor = false;
		this.y_table_display.Click += new System.EventHandler(y_table_display_Click);
		this.y_groupBox2.Controls.Add(this.SID_wild);
		this.y_groupBox2.Controls.Add(this.TID_wild);
		this.y_groupBox2.Controls.Add(this.y_stats1);
		this.y_groupBox2.Controls.Add(this.y_stats6);
		this.y_groupBox2.Controls.Add(this.y_stats5);
		this.y_groupBox2.Controls.Add(this.y_stats3);
		this.y_groupBox2.Controls.Add(this.y_stats4);
		this.y_groupBox2.Controls.Add(this.y_stats2);
		this.y_groupBox2.Controls.Add(this.y_check_LvEnable);
		this.y_groupBox2.Controls.Add(this.y_shiny);
		this.y_groupBox2.Controls.Add(this.y_sex);
		this.y_groupBox2.Controls.Add(this.label46);
		this.y_groupBox2.Controls.Add(this.label49);
		this.y_groupBox2.Controls.Add(this.y_ability);
		this.y_groupBox2.Controls.Add(this.label104);
		this.y_groupBox2.Controls.Add(this.label125);
		this.y_groupBox2.Controls.Add(this.label126);
		this.y_groupBox2.Controls.Add(this.y_mezapaPower);
		this.y_groupBox2.Controls.Add(this.label127);
		this.y_groupBox2.Controls.Add(this.y_mezapaType);
		this.y_groupBox2.Controls.Add(this.y_search2);
		this.y_groupBox2.Controls.Add(this.label43);
		this.y_groupBox2.Controls.Add(this.label45);
		this.y_groupBox2.Controls.Add(this.y_search1);
		this.y_groupBox2.Controls.Add(this.label47);
		this.y_groupBox2.Controls.Add(this.label48);
		this.y_groupBox2.Controls.Add(this.label103);
		this.y_groupBox2.Controls.Add(this.label105);
		this.y_groupBox2.Controls.Add(this.y_pokedex);
		this.y_groupBox2.Controls.Add(this.NatureButton_wild);
		this.y_groupBox2.Controls.Add(this.label107);
		this.y_groupBox2.Controls.Add(this.label108);
		this.y_groupBox2.Controls.Add(this.y_IVup6);
		this.y_groupBox2.Controls.Add(this.y_IVlow1);
		this.y_groupBox2.Controls.Add(this.label109);
		this.y_groupBox2.Controls.Add(this.y_IVlow2);
		this.y_groupBox2.Controls.Add(this.y_IVlow3);
		this.y_groupBox2.Controls.Add(this.y_IVup5);
		this.y_groupBox2.Controls.Add(this.y_IVlow4);
		this.y_groupBox2.Controls.Add(this.label110);
		this.y_groupBox2.Controls.Add(this.y_IVlow5);
		this.y_groupBox2.Controls.Add(this.label111);
		this.y_groupBox2.Controls.Add(this.y_IVlow6);
		this.y_groupBox2.Controls.Add(this.y_IVup4);
		this.y_groupBox2.Controls.Add(this.label112);
		this.y_groupBox2.Controls.Add(this.label113);
		this.y_groupBox2.Controls.Add(this.y_IVup1);
		this.y_groupBox2.Controls.Add(this.y_IVup3);
		this.y_groupBox2.Controls.Add(this.label114);
		this.y_groupBox2.Controls.Add(this.label115);
		this.y_groupBox2.Controls.Add(this.y_IVup2);
		this.y_groupBox2.Controls.Add(this.y_Lv);
		this.y_groupBox2.Location = new System.Drawing.Point(399, 346);
		this.y_groupBox2.Name = "y_groupBox2";
		this.y_groupBox2.Size = new System.Drawing.Size(437, 259);
		this.y_groupBox2.TabIndex = 19;
		this.y_groupBox2.TabStop = false;
		this.SID_wild.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.SID_wild.Location = new System.Drawing.Point(338, 189);
		this.SID_wild.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.SID_wild.Name = "SID_wild";
		this.SID_wild.Size = new System.Drawing.Size(80, 22);
		this.SID_wild.TabIndex = 50;
		this.SID_wild.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.TID_wild.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TID_wild.Location = new System.Drawing.Point(214, 189);
		this.TID_wild.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.TID_wild.Name = "TID_wild";
		this.TID_wild.Size = new System.Drawing.Size(80, 22);
		this.TID_wild.TabIndex = 49;
		this.TID_wild.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_stats1.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_stats1.Location = new System.Drawing.Point(41, 61);
		this.y_stats1.Maximum = new decimal(new int[4] { 114514, 0, 0, 0 });
		this.y_stats1.Name = "y_stats1";
		this.y_stats1.Size = new System.Drawing.Size(119, 22);
		this.y_stats1.TabIndex = 37;
		this.y_stats1.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_stats1.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.y_stats6.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_stats6.Location = new System.Drawing.Point(41, 221);
		this.y_stats6.Maximum = new decimal(new int[4] { 810, 0, 0, 0 });
		this.y_stats6.Name = "y_stats6";
		this.y_stats6.Size = new System.Drawing.Size(119, 22);
		this.y_stats6.TabIndex = 42;
		this.y_stats6.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_stats6.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.y_stats5.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_stats5.Location = new System.Drawing.Point(41, 189);
		this.y_stats5.Maximum = new decimal(new int[4] { 810, 0, 0, 0 });
		this.y_stats5.Name = "y_stats5";
		this.y_stats5.Size = new System.Drawing.Size(119, 22);
		this.y_stats5.TabIndex = 41;
		this.y_stats5.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_stats5.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.y_stats3.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_stats3.Location = new System.Drawing.Point(41, 125);
		this.y_stats3.Maximum = new decimal(new int[4] { 1919, 0, 0, 0 });
		this.y_stats3.Name = "y_stats3";
		this.y_stats3.Size = new System.Drawing.Size(119, 22);
		this.y_stats3.TabIndex = 39;
		this.y_stats3.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_stats3.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.y_stats4.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_stats4.Location = new System.Drawing.Point(41, 157);
		this.y_stats4.Maximum = new decimal(new int[4] { 931, 0, 0, 0 });
		this.y_stats4.Name = "y_stats4";
		this.y_stats4.Size = new System.Drawing.Size(119, 22);
		this.y_stats4.TabIndex = 40;
		this.y_stats4.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_stats4.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.y_stats2.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_stats2.Location = new System.Drawing.Point(41, 93);
		this.y_stats2.Maximum = new decimal(new int[4] { 810, 0, 0, 0 });
		this.y_stats2.Name = "y_stats2";
		this.y_stats2.Size = new System.Drawing.Size(119, 22);
		this.y_stats2.TabIndex = 38;
		this.y_stats2.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_stats2.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.y_check_LvEnable.AutoSize = true;
		this.y_check_LvEnable.BackColor = System.Drawing.Color.Transparent;
		this.y_check_LvEnable.Location = new System.Drawing.Point(258, 29);
		this.y_check_LvEnable.Name = "y_check_LvEnable";
		this.y_check_LvEnable.Size = new System.Drawing.Size(97, 16);
		this.y_check_LvEnable.TabIndex = 24;
		this.y_check_LvEnable.Text = "Lvを有効にする";
		this.y_check_LvEnable.UseVisualStyleBackColor = false;
		this.y_check_LvEnable.Click += new System.EventHandler(y_check_LvEnable_Click);
		this.y_shiny.AutoSize = true;
		this.y_shiny.Location = new System.Drawing.Point(214, 225);
		this.y_shiny.Name = "y_shiny";
		this.y_shiny.Size = new System.Drawing.Size(103, 16);
		this.y_shiny.TabIndex = 51;
		this.y_shiny.Text = "色違いのみ出力";
		this.y_shiny.UseVisualStyleBackColor = true;
		this.y_sex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.y_sex.FormattingEnabled = true;
		this.y_sex.Items.AddRange(new object[4] { "指定なし", "♂", "♀", "-" });
		this.y_sex.Location = new System.Drawing.Point(214, 127);
		this.y_sex.Name = "y_sex";
		this.y_sex.Size = new System.Drawing.Size(80, 20);
		this.y_sex.TabIndex = 46;
		this.label46.AutoSize = true;
		this.label46.Location = new System.Drawing.Point(179, 130);
		this.label46.Name = "label46";
		this.label46.Size = new System.Drawing.Size(29, 12);
		this.label46.TabIndex = 120;
		this.label46.Text = "性別";
		this.label49.AutoSize = true;
		this.label49.Location = new System.Drawing.Point(304, 194);
		this.label49.Name = "label49";
		this.label49.Size = new System.Drawing.Size(28, 12);
		this.label49.TabIndex = 122;
		this.label49.Text = "裏ID";
		this.y_ability.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.y_ability.FormattingEnabled = true;
		this.y_ability.Location = new System.Drawing.Point(214, 95);
		this.y_ability.Name = "y_ability";
		this.y_ability.Size = new System.Drawing.Size(80, 20);
		this.y_ability.TabIndex = 45;
		this.label104.AutoSize = true;
		this.label104.Location = new System.Drawing.Point(179, 98);
		this.label104.Name = "label104";
		this.label104.Size = new System.Drawing.Size(29, 12);
		this.label104.TabIndex = 116;
		this.label104.Text = "特性";
		this.label125.AutoSize = true;
		this.label125.Location = new System.Drawing.Point(180, 194);
		this.label125.Name = "label125";
		this.label125.Size = new System.Drawing.Size(28, 12);
		this.label125.TabIndex = 113;
		this.label125.Text = "表ID";
		this.label126.AutoSize = true;
		this.label126.Location = new System.Drawing.Point(362, 162);
		this.label126.Name = "label126";
		this.label126.Size = new System.Drawing.Size(17, 12);
		this.label126.TabIndex = 121;
		this.label126.Text = "～";
		this.y_mezapaPower.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_mezapaPower.Location = new System.Drawing.Point(306, 157);
		this.y_mezapaPower.Maximum = new decimal(new int[4] { 70, 0, 0, 0 });
		this.y_mezapaPower.Minimum = new decimal(new int[4] { 30, 0, 0, 0 });
		this.y_mezapaPower.Name = "y_mezapaPower";
		this.y_mezapaPower.Size = new System.Drawing.Size(50, 22);
		this.y_mezapaPower.TabIndex = 48;
		this.y_mezapaPower.Value = new decimal(new int[4] { 30, 0, 0, 0 });
		this.y_mezapaPower.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_mezapaPower.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label127.AutoSize = true;
		this.label127.Location = new System.Drawing.Point(174, 162);
		this.label127.Name = "label127";
		this.label127.Size = new System.Drawing.Size(34, 12);
		this.label127.TabIndex = 117;
		this.label127.Text = "めざパ";
		this.y_mezapaType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.y_mezapaType.FormattingEnabled = true;
		this.y_mezapaType.Location = new System.Drawing.Point(214, 159);
		this.y_mezapaType.Name = "y_mezapaType";
		this.y_mezapaType.Size = new System.Drawing.Size(80, 20);
		this.y_mezapaType.TabIndex = 47;
		this.y_search2.AutoSize = true;
		this.y_search2.BackColor = System.Drawing.Color.White;
		this.y_search2.Location = new System.Drawing.Point(131, 0);
		this.y_search2.Name = "y_search2";
		this.y_search2.Size = new System.Drawing.Size(101, 16);
		this.y_search2.TabIndex = 21;
		this.y_search2.Text = "能力値から検索";
		this.y_search2.UseVisualStyleBackColor = false;
		this.label43.AutoSize = true;
		this.label43.Location = new System.Drawing.Point(22, 226);
		this.label43.Name = "label43";
		this.label43.Size = new System.Drawing.Size(12, 12);
		this.label43.TabIndex = 48;
		this.label43.Text = "S";
		this.label45.AutoSize = true;
		this.label45.Location = new System.Drawing.Point(22, 194);
		this.label45.Name = "label45";
		this.label45.Size = new System.Drawing.Size(13, 12);
		this.label45.TabIndex = 46;
		this.label45.Text = "D";
		this.y_search1.AutoSize = true;
		this.y_search1.BackColor = System.Drawing.Color.White;
		this.y_search1.Checked = true;
		this.y_search1.Location = new System.Drawing.Point(24, 0);
		this.y_search1.Name = "y_search1";
		this.y_search1.Size = new System.Drawing.Size(105, 16);
		this.y_search1.TabIndex = 20;
		this.y_search1.TabStop = true;
		this.y_search1.Text = "個体値から検索 ";
		this.y_search1.UseVisualStyleBackColor = false;
		this.y_search1.CheckedChanged += new System.EventHandler(y_search1_CheckedChanged);
		this.label47.AutoSize = true;
		this.label47.Location = new System.Drawing.Point(22, 162);
		this.label47.Name = "label47";
		this.label47.Size = new System.Drawing.Size(13, 12);
		this.label47.TabIndex = 44;
		this.label47.Text = "C";
		this.label48.AutoSize = true;
		this.label48.Location = new System.Drawing.Point(22, 130);
		this.label48.Name = "label48";
		this.label48.Size = new System.Drawing.Size(13, 12);
		this.label48.TabIndex = 42;
		this.label48.Text = "B";
		this.label103.AutoSize = true;
		this.label103.Location = new System.Drawing.Point(22, 98);
		this.label103.Name = "label103";
		this.label103.Size = new System.Drawing.Size(13, 12);
		this.label103.TabIndex = 40;
		this.label103.Text = "A";
		this.label105.AutoSize = true;
		this.label105.Location = new System.Drawing.Point(22, 66);
		this.label105.Name = "label105";
		this.label105.Size = new System.Drawing.Size(13, 12);
		this.label105.TabIndex = 38;
		this.label105.Text = "H";
		this.y_pokedex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.y_pokedex.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_pokedex.FormattingEnabled = true;
		this.y_pokedex.Location = new System.Drawing.Point(70, 27);
		this.y_pokedex.Name = "y_pokedex";
		this.y_pokedex.Size = new System.Drawing.Size(90, 22);
		this.y_pokedex.TabIndex = 22;
		this.y_pokedex.SelectedIndexChanged += new System.EventHandler(y_pokedex_SelectedIndexChanged);
		this.NatureButton_wild.Location = new System.Drawing.Point(214, 61);
		this.NatureButton_wild.Name = "NatureButton_wild";
		this.NatureButton_wild.Size = new System.Drawing.Size(75, 23);
		this.NatureButton_wild.TabIndex = 44;
		this.NatureButton_wild.Text = "▼選択";
		this.NatureButton_wild.UseVisualStyleBackColor = true;
		this.NatureButton_wild.Click += new System.EventHandler(NatureButton_wild_Click);
		this.label107.AutoSize = true;
		this.label107.Location = new System.Drawing.Point(22, 30);
		this.label107.Name = "label107";
		this.label107.Size = new System.Drawing.Size(42, 12);
		this.label107.TabIndex = 64;
		this.label107.Text = "ポケモン";
		this.label108.AutoSize = true;
		this.label108.Location = new System.Drawing.Point(174, 30);
		this.label108.Name = "label108";
		this.label108.Size = new System.Drawing.Size(17, 12);
		this.label108.TabIndex = 67;
		this.label108.Text = "Lv";
		this.y_IVup6.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_IVup6.Location = new System.Drawing.Point(115, 221);
		this.y_IVup6.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.y_IVup6.Name = "y_IVup6";
		this.y_IVup6.Size = new System.Drawing.Size(45, 22);
		this.y_IVup6.TabIndex = 36;
		this.y_IVup6.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.y_IVup6.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_IVup6.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.y_IVlow1.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_IVlow1.Location = new System.Drawing.Point(41, 61);
		this.y_IVlow1.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.y_IVlow1.Name = "y_IVlow1";
		this.y_IVlow1.Size = new System.Drawing.Size(45, 22);
		this.y_IVlow1.TabIndex = 25;
		this.y_IVlow1.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_IVlow1.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label109.AutoSize = true;
		this.label109.Location = new System.Drawing.Point(92, 226);
		this.label109.Name = "label109";
		this.label109.Size = new System.Drawing.Size(17, 12);
		this.label109.TabIndex = 86;
		this.label109.Text = "～";
		this.y_IVlow2.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_IVlow2.Location = new System.Drawing.Point(41, 93);
		this.y_IVlow2.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.y_IVlow2.Name = "y_IVlow2";
		this.y_IVlow2.Size = new System.Drawing.Size(45, 22);
		this.y_IVlow2.TabIndex = 27;
		this.y_IVlow2.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_IVlow2.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.y_IVlow3.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_IVlow3.Location = new System.Drawing.Point(41, 125);
		this.y_IVlow3.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.y_IVlow3.Name = "y_IVlow3";
		this.y_IVlow3.Size = new System.Drawing.Size(45, 22);
		this.y_IVlow3.TabIndex = 29;
		this.y_IVlow3.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_IVlow3.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.y_IVup5.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_IVup5.Location = new System.Drawing.Point(115, 189);
		this.y_IVup5.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.y_IVup5.Name = "y_IVup5";
		this.y_IVup5.Size = new System.Drawing.Size(45, 22);
		this.y_IVup5.TabIndex = 34;
		this.y_IVup5.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.y_IVup5.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_IVup5.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.y_IVlow4.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_IVlow4.Location = new System.Drawing.Point(41, 157);
		this.y_IVlow4.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.y_IVlow4.Name = "y_IVlow4";
		this.y_IVlow4.Size = new System.Drawing.Size(45, 22);
		this.y_IVlow4.TabIndex = 31;
		this.y_IVlow4.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_IVlow4.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label110.AutoSize = true;
		this.label110.Location = new System.Drawing.Point(92, 194);
		this.label110.Name = "label110";
		this.label110.Size = new System.Drawing.Size(17, 12);
		this.label110.TabIndex = 84;
		this.label110.Text = "～";
		this.y_IVlow5.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_IVlow5.Location = new System.Drawing.Point(41, 189);
		this.y_IVlow5.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.y_IVlow5.Name = "y_IVlow5";
		this.y_IVlow5.Size = new System.Drawing.Size(45, 22);
		this.y_IVlow5.TabIndex = 33;
		this.y_IVlow5.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_IVlow5.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label111.AutoSize = true;
		this.label111.Location = new System.Drawing.Point(179, 66);
		this.label111.Name = "label111";
		this.label111.Size = new System.Drawing.Size(29, 12);
		this.label111.TabIndex = 66;
		this.label111.Text = "性格";
		this.y_IVlow6.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_IVlow6.Location = new System.Drawing.Point(41, 221);
		this.y_IVlow6.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.y_IVlow6.Name = "y_IVlow6";
		this.y_IVlow6.Size = new System.Drawing.Size(45, 22);
		this.y_IVlow6.TabIndex = 35;
		this.y_IVlow6.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_IVlow6.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.y_IVup4.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_IVup4.Location = new System.Drawing.Point(115, 157);
		this.y_IVup4.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.y_IVup4.Name = "y_IVup4";
		this.y_IVup4.Size = new System.Drawing.Size(45, 22);
		this.y_IVup4.TabIndex = 32;
		this.y_IVup4.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.y_IVup4.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_IVup4.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label112.AutoSize = true;
		this.label112.Location = new System.Drawing.Point(92, 66);
		this.label112.Name = "label112";
		this.label112.Size = new System.Drawing.Size(17, 12);
		this.label112.TabIndex = 76;
		this.label112.Text = "～";
		this.label113.AutoSize = true;
		this.label113.Location = new System.Drawing.Point(92, 162);
		this.label113.Name = "label113";
		this.label113.Size = new System.Drawing.Size(17, 12);
		this.label113.TabIndex = 82;
		this.label113.Text = "～";
		this.y_IVup1.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_IVup1.Location = new System.Drawing.Point(115, 61);
		this.y_IVup1.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.y_IVup1.Name = "y_IVup1";
		this.y_IVup1.Size = new System.Drawing.Size(45, 22);
		this.y_IVup1.TabIndex = 26;
		this.y_IVup1.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.y_IVup1.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_IVup1.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.y_IVup3.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_IVup3.Location = new System.Drawing.Point(115, 125);
		this.y_IVup3.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.y_IVup3.Name = "y_IVup3";
		this.y_IVup3.Size = new System.Drawing.Size(45, 22);
		this.y_IVup3.TabIndex = 30;
		this.y_IVup3.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.y_IVup3.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_IVup3.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label114.AutoSize = true;
		this.label114.Location = new System.Drawing.Point(92, 98);
		this.label114.Name = "label114";
		this.label114.Size = new System.Drawing.Size(17, 12);
		this.label114.TabIndex = 78;
		this.label114.Text = "～";
		this.label115.AutoSize = true;
		this.label115.Location = new System.Drawing.Point(92, 130);
		this.label115.Name = "label115";
		this.label115.Size = new System.Drawing.Size(17, 12);
		this.label115.TabIndex = 80;
		this.label115.Text = "～";
		this.y_IVup2.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_IVup2.Location = new System.Drawing.Point(115, 93);
		this.y_IVup2.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.y_IVup2.Name = "y_IVup2";
		this.y_IVup2.Size = new System.Drawing.Size(45, 22);
		this.y_IVup2.TabIndex = 28;
		this.y_IVup2.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.y_IVup2.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_IVup2.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.y_Lv.Enabled = false;
		this.y_Lv.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_Lv.Location = new System.Drawing.Point(197, 25);
		this.y_Lv.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.y_Lv.Name = "y_Lv";
		this.y_Lv.Size = new System.Drawing.Size(50, 22);
		this.y_Lv.TabIndex = 23;
		this.y_Lv.Value = new decimal(new int[4] { 50, 0, 0, 0 });
		this.y_Lv.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_Lv.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.y_groupBox1.Controls.Add(this.PaintPanel_wild);
		this.y_groupBox1.Controls.Add(this.FrameRange_wild);
		this.y_groupBox1.Controls.Add(this.TargetFrame_wild);
		this.y_groupBox1.Controls.Add(this.LastFrame_wild);
		this.y_groupBox1.Controls.Add(this.FirstFrame_wild);
		this.y_groupBox1.Controls.Add(this.SetFrameButton_wild);
		this.y_groupBox1.Controls.Add(this.label180);
		this.y_groupBox1.Controls.Add(this.label181);
		this.y_groupBox1.Controls.Add(this.label182);
		this.y_groupBox1.Controls.Add(this.label116);
		this.y_groupBox1.Controls.Add(this.label117);
		this.y_groupBox1.Controls.Add(this.y_RSmax);
		this.y_groupBox1.Controls.Add(this.y_RSmin);
		this.y_groupBox1.Controls.Add(this.Method_wild);
		this.y_groupBox1.Controls.Add(this.y_Initialseed3);
		this.y_groupBox1.Controls.Add(this.ForMultipleSeed_wild);
		this.y_groupBox1.Controls.Add(this.label118);
		this.y_groupBox1.Controls.Add(this.ForRTC_wild);
		this.y_groupBox1.Controls.Add(this.y_Initialseed1);
		this.y_groupBox1.Controls.Add(this.ForSimpleSeed_wild);
		this.y_groupBox1.Controls.Add(this.label119);
		this.y_groupBox1.Controls.Add(this.label121);
		this.y_groupBox1.Controls.Add(this.label123);
		this.y_groupBox1.Location = new System.Drawing.Point(6, 346);
		this.y_groupBox1.Name = "y_groupBox1";
		this.y_groupBox1.Size = new System.Drawing.Size(387, 259);
		this.y_groupBox1.TabIndex = 0;
		this.y_groupBox1.TabStop = false;
		this.y_groupBox1.Text = "検索範囲";
		this.PaintPanel_wild.Controls.Add(this.label151);
		this.PaintPanel_wild.Controls.Add(this.PaintSeedMaxFrameBox_wild);
		this.PaintPanel_wild.Controls.Add(this.PaintSeedMinFrameBox_wild);
		this.PaintPanel_wild.Controls.Add(this.label149);
		this.PaintPanel_wild.Location = new System.Drawing.Point(131, 89);
		this.PaintPanel_wild.Name = "PaintPanel_wild";
		this.PaintPanel_wild.Size = new System.Drawing.Size(228, 60);
		this.PaintPanel_wild.TabIndex = 124;
		this.label151.AutoSize = true;
		this.label151.Location = new System.Drawing.Point(211, 6);
		this.label151.Name = "label151";
		this.label151.Size = new System.Drawing.Size(12, 12);
		this.label151.TabIndex = 130;
		this.label151.Text = "F";
		this.PaintSeedMaxFrameBox_wild.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.PaintSeedMaxFrameBox_wild.Location = new System.Drawing.Point(125, 0);
		this.PaintSeedMaxFrameBox_wild.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.PaintSeedMaxFrameBox_wild.Name = "PaintSeedMaxFrameBox_wild";
		this.PaintSeedMaxFrameBox_wild.Size = new System.Drawing.Size(80, 22);
		this.PaintSeedMaxFrameBox_wild.TabIndex = 129;
		this.PaintSeedMaxFrameBox_wild.Value = new decimal(new int[4] { 1500, 0, 0, 0 });
		this.PaintSeedMaxFrameBox_wild.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.PaintSeedMinFrameBox_wild.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.PaintSeedMinFrameBox_wild.Location = new System.Drawing.Point(0, 0);
		this.PaintSeedMinFrameBox_wild.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.PaintSeedMinFrameBox_wild.Name = "PaintSeedMinFrameBox_wild";
		this.PaintSeedMinFrameBox_wild.Size = new System.Drawing.Size(80, 22);
		this.PaintSeedMinFrameBox_wild.TabIndex = 128;
		this.PaintSeedMinFrameBox_wild.Value = new decimal(new int[4] { 1200, 0, 0, 0 });
		this.PaintSeedMinFrameBox_wild.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.label149.AutoSize = true;
		this.label149.Location = new System.Drawing.Point(86, 6);
		this.label149.Name = "label149";
		this.label149.Size = new System.Drawing.Size(28, 12);
		this.label149.TabIndex = 127;
		this.label149.Text = "F ～";
		this.FrameRange_wild.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.FrameRange_wild.Location = new System.Drawing.Point(210, 187);
		this.FrameRange_wild.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.FrameRange_wild.Name = "FrameRange_wild";
		this.FrameRange_wild.Size = new System.Drawing.Size(64, 22);
		this.FrameRange_wild.TabIndex = 117;
		this.FrameRange_wild.Value = new decimal(new int[4] { 100, 0, 0, 0 });
		this.FrameRange_wild.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.TargetFrame_wild.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TargetFrame_wild.Location = new System.Drawing.Point(87, 187);
		this.TargetFrame_wild.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.TargetFrame_wild.Name = "TargetFrame_wild";
		this.TargetFrame_wild.Size = new System.Drawing.Size(93, 22);
		this.TargetFrame_wild.TabIndex = 116;
		this.TargetFrame_wild.Value = new decimal(new int[4] { 2049, 0, 0, 0 });
		this.TargetFrame_wild.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.LastFrame_wild.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.LastFrame_wild.Location = new System.Drawing.Point(210, 155);
		this.LastFrame_wild.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.LastFrame_wild.Name = "LastFrame_wild";
		this.LastFrame_wild.Size = new System.Drawing.Size(93, 22);
		this.LastFrame_wild.TabIndex = 115;
		this.LastFrame_wild.Value = new decimal(new int[4] { 2149, 0, 0, 0 });
		this.LastFrame_wild.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.FirstFrame_wild.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.FirstFrame_wild.Location = new System.Drawing.Point(87, 155);
		this.FirstFrame_wild.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.FirstFrame_wild.Name = "FirstFrame_wild";
		this.FirstFrame_wild.Size = new System.Drawing.Size(93, 22);
		this.FirstFrame_wild.TabIndex = 114;
		this.FirstFrame_wild.Value = new decimal(new int[4] { 1949, 0, 0, 0 });
		this.FirstFrame_wild.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.SetFrameButton_wild.Location = new System.Drawing.Point(280, 186);
		this.SetFrameButton_wild.Name = "SetFrameButton_wild";
		this.SetFrameButton_wild.Size = new System.Drawing.Size(23, 23);
		this.SetFrameButton_wild.TabIndex = 110;
		this.SetFrameButton_wild.Text = "↑";
		this.SetFrameButton_wild.UseVisualStyleBackColor = true;
		this.SetFrameButton_wild.Click += new System.EventHandler(SetFrameButton_wild_Click);
		this.label180.AutoSize = true;
		this.label180.Location = new System.Drawing.Point(308, 192);
		this.label180.Name = "label180";
		this.label180.Size = new System.Drawing.Size(73, 12);
		this.label180.TabIndex = 113;
		this.label180.Text = "(リスト表示用)";
		this.label181.Location = new System.Drawing.Point(186, 191);
		this.label181.Name = "label181";
		this.label181.Size = new System.Drawing.Size(18, 16);
		this.label181.TabIndex = 112;
		this.label181.Text = "±";
		this.label182.Location = new System.Drawing.Point(186, 160);
		this.label182.Name = "label182";
		this.label182.Size = new System.Drawing.Size(18, 16);
		this.label182.TabIndex = 111;
		this.label182.Text = "～";
		this.label116.AutoSize = true;
		this.label116.Location = new System.Drawing.Point(33, 226);
		this.label116.Name = "label116";
		this.label116.Size = new System.Drawing.Size(42, 12);
		this.label116.TabIndex = 105;
		this.label116.Text = "Method";
		this.label117.AutoSize = true;
		this.label117.Location = new System.Drawing.Point(217, 62);
		this.label117.Name = "label117";
		this.label117.Size = new System.Drawing.Size(33, 12);
		this.label117.TabIndex = 104;
		this.label117.Text = "分 ～";
		this.y_RSmax.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_RSmax.Location = new System.Drawing.Point(256, 57);
		this.y_RSmax.Maximum = new decimal(new int[4] { 63349, 0, 0, 0 });
		this.y_RSmax.Name = "y_RSmax";
		this.y_RSmax.Size = new System.Drawing.Size(80, 22);
		this.y_RSmax.TabIndex = 5;
		this.y_RSmax.Value = new decimal(new int[4] { 3, 0, 0, 0 });
		this.y_RSmax.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_RSmax.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.y_RSmin.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_RSmin.Location = new System.Drawing.Point(131, 57);
		this.y_RSmin.Maximum = new decimal(new int[4] { 63349, 0, 0, 0 });
		this.y_RSmin.Name = "y_RSmin";
		this.y_RSmin.Size = new System.Drawing.Size(80, 22);
		this.y_RSmin.TabIndex = 4;
		this.y_RSmin.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.y_RSmin.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.Method_wild.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.Method_wild.FormattingEnabled = true;
		this.Method_wild.Items.AddRange(new object[3] { "Method1", "Method2", "Method4" });
		this.Method_wild.Location = new System.Drawing.Point(87, 221);
		this.Method_wild.Name = "Method_wild";
		this.Method_wild.Size = new System.Drawing.Size(93, 20);
		this.Method_wild.TabIndex = 14;
		this.y_Initialseed3.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_Initialseed3.Location = new System.Drawing.Point(131, 89);
		this.y_Initialseed3.Multiline = true;
		this.y_Initialseed3.Name = "y_Initialseed3";
		this.y_Initialseed3.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.y_Initialseed3.Size = new System.Drawing.Size(100, 56);
		this.y_Initialseed3.TabIndex = 7;
		this.y_Initialseed3.Enter += new System.EventHandler(TextBox_SelectText);
		this.ForMultipleSeed_wild.AutoSize = true;
		this.ForMultipleSeed_wild.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ForMultipleSeed_wild.Location = new System.Drawing.Point(90, 91);
		this.ForMultipleSeed_wild.Name = "ForMultipleSeed_wild";
		this.ForMultipleSeed_wild.Size = new System.Drawing.Size(14, 13);
		this.ForMultipleSeed_wild.TabIndex = 6;
		this.ForMultipleSeed_wild.UseVisualStyleBackColor = true;
		this.label118.AutoSize = true;
		this.label118.Location = new System.Drawing.Point(342, 62);
		this.label118.Name = "label118";
		this.label118.Size = new System.Drawing.Size(17, 12);
		this.label118.TabIndex = 79;
		this.label118.Text = "分";
		this.ForRTC_wild.AutoSize = true;
		this.ForRTC_wild.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ForRTC_wild.Location = new System.Drawing.Point(90, 59);
		this.ForRTC_wild.Name = "ForRTC_wild";
		this.ForRTC_wild.Size = new System.Drawing.Size(39, 18);
		this.ForRTC_wild.TabIndex = 3;
		this.ForRTC_wild.Text = "  ";
		this.ForRTC_wild.UseVisualStyleBackColor = true;
		this.y_Initialseed1.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.y_Initialseed1.Location = new System.Drawing.Point(131, 25);
		this.y_Initialseed1.Name = "y_Initialseed1";
		this.y_Initialseed1.Size = new System.Drawing.Size(80, 22);
		this.y_Initialseed1.TabIndex = 2;
		this.y_Initialseed1.Text = "0";
		this.y_Initialseed1.Enter += new System.EventHandler(TextBox_SelectText);
		this.y_Initialseed1.Validating += new System.ComponentModel.CancelEventHandler(InitialSeedBox_Check);
		this.ForSimpleSeed_wild.AutoSize = true;
		this.ForSimpleSeed_wild.Checked = true;
		this.ForSimpleSeed_wild.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ForSimpleSeed_wild.Location = new System.Drawing.Point(90, 27);
		this.ForSimpleSeed_wild.Name = "ForSimpleSeed_wild";
		this.ForSimpleSeed_wild.Size = new System.Drawing.Size(14, 13);
		this.ForSimpleSeed_wild.TabIndex = 1;
		this.ForSimpleSeed_wild.TabStop = true;
		this.ForSimpleSeed_wild.UseVisualStyleBackColor = true;
		this.ForSimpleSeed_wild.CheckedChanged += new System.EventHandler(RadioButton_Checked);
		this.label119.AutoSize = true;
		this.label119.Location = new System.Drawing.Point(22, 30);
		this.label119.Name = "label119";
		this.label119.Size = new System.Drawing.Size(53, 12);
		this.label119.TabIndex = 70;
		this.label119.Text = "初期seed";
		this.label121.AutoSize = true;
		this.label121.Location = new System.Drawing.Point(39, 192);
		this.label121.Name = "label121";
		this.label121.Size = new System.Drawing.Size(36, 12);
		this.label121.TabIndex = 68;
		this.label121.Text = "目標F";
		this.label123.AutoSize = true;
		this.label123.Location = new System.Drawing.Point(63, 160);
		this.label123.Name = "label123";
		this.label123.Size = new System.Drawing.Size(12, 12);
		this.label123.TabIndex = 64;
		this.label123.Text = "F";
		this.y_cancel.Enabled = false;
		this.y_cancel.Location = new System.Drawing.Point(1004, 408);
		this.y_cancel.Name = "y_cancel";
		this.y_cancel.Size = new System.Drawing.Size(75, 23);
		this.y_cancel.TabIndex = 73;
		this.y_cancel.Text = "キャンセル";
		this.y_cancel.UseVisualStyleBackColor = true;
		this.y_cancel.Click += new System.EventHandler(y_cancel_Click);
		this.y_listup.Location = new System.Drawing.Point(923, 408);
		this.y_listup.Name = "y_listup";
		this.y_listup.Size = new System.Drawing.Size(75, 23);
		this.y_listup.TabIndex = 72;
		this.y_listup.Text = "リスト表示";
		this.y_listup.UseVisualStyleBackColor = true;
		this.y_listup.Click += new System.EventHandler(y_list_Click);
		this.y_start.Location = new System.Drawing.Point(842, 408);
		this.y_start.Name = "y_start";
		this.y_start.Size = new System.Drawing.Size(75, 23);
		this.y_start.TabIndex = 71;
		this.y_start.Text = "計算";
		this.y_start.UseVisualStyleBackColor = true;
		this.y_start.Click += new System.EventHandler(y_start_Click);
		this.groupBox8.Controls.Add(this.Encounter_OldRod);
		this.groupBox8.Controls.Add(this.Encounter_GoodRod);
		this.groupBox8.Controls.Add(this.Encounter_SuperRod);
		this.groupBox8.Controls.Add(this.Encounter_RockSmash);
		this.groupBox8.Controls.Add(this.Encounter_Surf);
		this.groupBox8.Controls.Add(this.Encounter_Grass);
		this.groupBox8.Location = new System.Drawing.Point(742, 212);
		this.groupBox8.Name = "groupBox8";
		this.groupBox8.Size = new System.Drawing.Size(337, 56);
		this.groupBox8.TabIndex = 16;
		this.groupBox8.TabStop = false;
		this.groupBox8.Text = "エンカウント方法";
		this.Encounter_OldRod.AutoSize = true;
		this.Encounter_OldRod.Location = new System.Drawing.Point(286, 23);
		this.Encounter_OldRod.Name = "Encounter_OldRod";
		this.Encounter_OldRod.Size = new System.Drawing.Size(42, 16);
		this.Encounter_OldRod.TabIndex = 108;
		this.Encounter_OldRod.Text = "ボロ";
		this.Encounter_OldRod.UseVisualStyleBackColor = true;
		this.Encounter_OldRod.Click += new System.EventHandler(Encounter_OldRod_Click);
		this.Encounter_GoodRod.AutoSize = true;
		this.Encounter_GoodRod.Location = new System.Drawing.Point(237, 23);
		this.Encounter_GoodRod.Name = "Encounter_GoodRod";
		this.Encounter_GoodRod.Size = new System.Drawing.Size(43, 16);
		this.Encounter_GoodRod.TabIndex = 107;
		this.Encounter_GoodRod.Text = "いい";
		this.Encounter_GoodRod.UseVisualStyleBackColor = true;
		this.Encounter_GoodRod.Click += new System.EventHandler(Encounter_GoodRod_Click);
		this.Encounter_SuperRod.AutoSize = true;
		this.Encounter_SuperRod.Location = new System.Drawing.Point(179, 23);
		this.Encounter_SuperRod.Name = "Encounter_SuperRod";
		this.Encounter_SuperRod.Size = new System.Drawing.Size(52, 16);
		this.Encounter_SuperRod.TabIndex = 106;
		this.Encounter_SuperRod.Text = "すごい";
		this.Encounter_SuperRod.UseVisualStyleBackColor = true;
		this.Encounter_SuperRod.Click += new System.EventHandler(Encounter_SuperRod_Click);
		this.Encounter_RockSmash.AutoSize = true;
		this.Encounter_RockSmash.Location = new System.Drawing.Point(117, 23);
		this.Encounter_RockSmash.Name = "Encounter_RockSmash";
		this.Encounter_RockSmash.Size = new System.Drawing.Size(56, 16);
		this.Encounter_RockSmash.TabIndex = 105;
		this.Encounter_RockSmash.Text = "岩砕き";
		this.Encounter_RockSmash.UseVisualStyleBackColor = true;
		this.Encounter_RockSmash.Click += new System.EventHandler(Encounter_RockSmash_Click);
		this.Encounter_Surf.AutoSize = true;
		this.Encounter_Surf.Location = new System.Drawing.Point(56, 23);
		this.Encounter_Surf.Name = "Encounter_Surf";
		this.Encounter_Surf.Size = new System.Drawing.Size(55, 16);
		this.Encounter_Surf.TabIndex = 104;
		this.Encounter_Surf.Text = "波乗り";
		this.Encounter_Surf.UseVisualStyleBackColor = true;
		this.Encounter_Surf.Click += new System.EventHandler(Encounter_Surf_Click);
		this.Encounter_Grass.AutoSize = true;
		this.Encounter_Grass.Checked = true;
		this.Encounter_Grass.Location = new System.Drawing.Point(15, 23);
		this.Encounter_Grass.Name = "Encounter_Grass";
		this.Encounter_Grass.Size = new System.Drawing.Size(35, 16);
		this.Encounter_Grass.TabIndex = 103;
		this.Encounter_Grass.TabStop = true;
		this.Encounter_Grass.Text = "草";
		this.Encounter_Grass.UseVisualStyleBackColor = true;
		this.Encounter_Grass.Click += new System.EventHandler(Encounter_Grass_Click);
		this.dataGridView_table.AllowUserToAddRows = false;
		this.dataGridView_table.AllowUserToDeleteRows = false;
		this.dataGridView_table.AllowUserToResizeColumns = false;
		this.dataGridView_table.AllowUserToResizeRows = false;
		this.dataGridView_table.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
		this.dataGridView_table.BackgroundColor = System.Drawing.SystemColors.Window;
		this.dataGridView_table.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.dataGridView_table.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGridView_table.ColumnHeadersVisible = false;
		this.dataGridView_table.Columns.AddRange(this.Column22, this.Column23, this.Column24, this.Column25, this.Column26, this.Column27, this.Column28, this.Column29, this.Column30, this.Column31, this.Column32, this.Column33);
		this.dataGridView_table.Enabled = false;
		this.dataGridView_table.Location = new System.Drawing.Point(263, 274);
		this.dataGridView_table.Name = "dataGridView_table";
		this.dataGridView_table.ReadOnly = true;
		this.dataGridView_table.RowHeadersVisible = false;
		this.dataGridView_table.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToFirstHeader;
		this.dataGridView_table.RowTemplate.Height = 21;
		this.dataGridView_table.Size = new System.Drawing.Size(816, 66);
		this.dataGridView_table.TabIndex = 82;
		this.Column22.HeaderText = "1";
		this.Column22.MinimumWidth = 10;
		this.Column22.Name = "Column22";
		this.Column22.ReadOnly = true;
		this.Column23.HeaderText = "2";
		this.Column23.MinimumWidth = 10;
		this.Column23.Name = "Column23";
		this.Column23.ReadOnly = true;
		this.Column24.HeaderText = "3";
		this.Column24.MinimumWidth = 10;
		this.Column24.Name = "Column24";
		this.Column24.ReadOnly = true;
		this.Column25.HeaderText = "4";
		this.Column25.MinimumWidth = 10;
		this.Column25.Name = "Column25";
		this.Column25.ReadOnly = true;
		this.Column26.HeaderText = "5";
		this.Column26.MinimumWidth = 10;
		this.Column26.Name = "Column26";
		this.Column26.ReadOnly = true;
		this.Column27.HeaderText = "6";
		this.Column27.MinimumWidth = 10;
		this.Column27.Name = "Column27";
		this.Column27.ReadOnly = true;
		this.Column28.HeaderText = "7";
		this.Column28.MinimumWidth = 10;
		this.Column28.Name = "Column28";
		this.Column28.ReadOnly = true;
		this.Column29.HeaderText = "8";
		this.Column29.MinimumWidth = 10;
		this.Column29.Name = "Column29";
		this.Column29.ReadOnly = true;
		this.Column30.HeaderText = "9";
		this.Column30.MinimumWidth = 10;
		this.Column30.Name = "Column30";
		this.Column30.ReadOnly = true;
		this.Column31.HeaderText = "10";
		this.Column31.MinimumWidth = 10;
		this.Column31.Name = "Column31";
		this.Column31.ReadOnly = true;
		this.Column32.HeaderText = "11";
		this.Column32.MinimumWidth = 10;
		this.Column32.Name = "Column32";
		this.Column32.ReadOnly = true;
		this.Column33.HeaderText = "12";
		this.Column33.MinimumWidth = 10;
		this.Column33.Name = "Column33";
		this.Column33.ReadOnly = true;
		this.groupBox7.Controls.Add(this.ROM_LG);
		this.groupBox7.Controls.Add(this.ROM_S);
		this.groupBox7.Controls.Add(this.ROM_FR);
		this.groupBox7.Controls.Add(this.ROM_Em);
		this.groupBox7.Controls.Add(this.ROM_R);
		this.groupBox7.Location = new System.Drawing.Point(350, 212);
		this.groupBox7.Name = "groupBox7";
		this.groupBox7.Size = new System.Drawing.Size(420, 56);
		this.groupBox7.TabIndex = 15;
		this.groupBox7.TabStop = false;
		this.groupBox7.Text = "バージョン";
		this.ROM_LG.AutoSize = true;
		this.ROM_LG.Location = new System.Drawing.Point(304, 23);
		this.ROM_LG.Name = "ROM_LG";
		this.ROM_LG.Size = new System.Drawing.Size(83, 16);
		this.ROM_LG.TabIndex = 104;
		this.ROM_LG.Text = "リーフグリーン";
		this.ROM_LG.UseVisualStyleBackColor = true;
		this.ROM_LG.Click += new System.EventHandler(ROM_LG_Click);
		this.ROM_S.AutoSize = true;
		this.ROM_S.Location = new System.Drawing.Point(72, 23);
		this.ROM_S.Name = "ROM_S";
		this.ROM_S.Size = new System.Drawing.Size(66, 16);
		this.ROM_S.TabIndex = 103;
		this.ROM_S.Text = "サファイア";
		this.ROM_S.UseVisualStyleBackColor = true;
		this.ROM_S.Click += new System.EventHandler(ROM_S_Click);
		this.ROM_FR.AutoSize = true;
		this.ROM_FR.Location = new System.Drawing.Point(217, 23);
		this.ROM_FR.Name = "ROM_FR";
		this.ROM_FR.Size = new System.Drawing.Size(81, 16);
		this.ROM_FR.TabIndex = 102;
		this.ROM_FR.Text = "ファイアレッド";
		this.ROM_FR.UseVisualStyleBackColor = true;
		this.ROM_FR.Click += new System.EventHandler(ROM_FR_Click);
		this.ROM_Em.AutoSize = true;
		this.ROM_Em.Location = new System.Drawing.Point(144, 23);
		this.ROM_Em.Name = "ROM_Em";
		this.ROM_Em.Size = new System.Drawing.Size(67, 16);
		this.ROM_Em.TabIndex = 101;
		this.ROM_Em.Text = "エメラルド";
		this.ROM_Em.UseVisualStyleBackColor = true;
		this.ROM_Em.Click += new System.EventHandler(ROM_Em_Click);
		this.ROM_R.AutoSize = true;
		this.ROM_R.Checked = true;
		this.ROM_R.Location = new System.Drawing.Point(15, 23);
		this.ROM_R.Name = "ROM_R";
		this.ROM_R.Size = new System.Drawing.Size(51, 16);
		this.ROM_R.TabIndex = 100;
		this.ROM_R.TabStop = true;
		this.ROM_R.Text = "ルビー";
		this.ROM_R.UseVisualStyleBackColor = true;
		this.ROM_R.Click += new System.EventHandler(ROM_R_Click);
		this.groupBox9.Controls.Add(this.MapBox);
		this.groupBox9.Location = new System.Drawing.Point(120, 212);
		this.groupBox9.Name = "groupBox9";
		this.groupBox9.Size = new System.Drawing.Size(232, 56);
		this.groupBox9.TabIndex = 17;
		this.groupBox9.TabStop = false;
		this.groupBox9.Text = "フィールド";
		this.MapBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.MapBox.FormattingEnabled = true;
		this.MapBox.Location = new System.Drawing.Point(6, 22);
		this.MapBox.Name = "MapBox";
		this.MapBox.Size = new System.Drawing.Size(211, 20);
		this.MapBox.TabIndex = 0;
		this.MapBox.TabStop = false;
		this.MapBox.SelectedIndexChanged += new System.EventHandler(SelectedMapChanged);
		this.y_groupBox3.Controls.Add(this.label_pb);
		this.y_groupBox3.Controls.Add(this.PokeBlockBox);
		this.y_groupBox3.Controls.Add(this.CCGender_wild);
		this.y_groupBox3.Controls.Add(this.label106);
		this.y_groupBox3.Controls.Add(this.SyncNature_wild);
		this.y_groupBox3.Controls.Add(this.FieldAbility_wild);
		this.y_groupBox3.Location = new System.Drawing.Point(842, 433);
		this.y_groupBox3.Name = "y_groupBox3";
		this.y_groupBox3.Size = new System.Drawing.Size(237, 74);
		this.y_groupBox3.TabIndex = 59;
		this.y_groupBox3.TabStop = false;
		this.y_groupBox3.Text = "その他";
		this.label_pb.AutoSize = true;
		this.label_pb.Location = new System.Drawing.Point(6, 47);
		this.label_pb.Name = "label_pb";
		this.label_pb.Size = new System.Drawing.Size(39, 12);
		this.label_pb.TabIndex = 114;
		this.label_pb.Text = "ポロック";
		this.label_pb.Visible = false;
		this.PokeBlockBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.PokeBlockBox.FormattingEnabled = true;
		this.PokeBlockBox.Items.AddRange(new object[6] { "なし", "赤(A↑)", "黄(B↑)", "青(C↑)", "緑(D↑)", "桃(S↑)" });
		this.PokeBlockBox.Location = new System.Drawing.Point(51, 44);
		this.PokeBlockBox.Name = "PokeBlockBox";
		this.PokeBlockBox.Size = new System.Drawing.Size(80, 20);
		this.PokeBlockBox.TabIndex = 113;
		this.PokeBlockBox.Visible = false;
		this.CCGender_wild.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.CCGender_wild.FormattingEnabled = true;
		this.CCGender_wild.Items.AddRange(new object[2] { "♂", "♀" });
		this.CCGender_wild.Location = new System.Drawing.Point(137, 18);
		this.CCGender_wild.Name = "CCGender_wild";
		this.CCGender_wild.Size = new System.Drawing.Size(80, 20);
		this.CCGender_wild.TabIndex = 112;
		this.CCGender_wild.Visible = false;
		this.label106.AutoSize = true;
		this.label106.Location = new System.Drawing.Point(16, 21);
		this.label106.Name = "label106";
		this.label106.Size = new System.Drawing.Size(29, 12);
		this.label106.TabIndex = 110;
		this.label106.Text = "特性";
		this.SyncNature_wild.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.SyncNature_wild.FormattingEnabled = true;
		this.SyncNature_wild.Location = new System.Drawing.Point(137, 18);
		this.SyncNature_wild.Name = "SyncNature_wild";
		this.SyncNature_wild.Size = new System.Drawing.Size(80, 20);
		this.SyncNature_wild.TabIndex = 61;
		this.FieldAbility_wild.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.FieldAbility_wild.FormattingEnabled = true;
		this.FieldAbility_wild.Items.AddRange(new object[3] { "---", "はっこう", "あくしゅう" });
		this.FieldAbility_wild.Location = new System.Drawing.Point(51, 18);
		this.FieldAbility_wild.Name = "FieldAbility_wild";
		this.FieldAbility_wild.Size = new System.Drawing.Size(80, 20);
		this.FieldAbility_wild.TabIndex = 60;
		this.FieldAbility_wild.SelectedIndexChanged += new System.EventHandler(SelectedFieldAbilityChanged);
		this.TabPage_stationary.Controls.Add(this.NaturePanel_stationary);
		this.TabPage_stationary.Controls.Add(this.groupBox5);
		this.TabPage_stationary.Controls.Add(this.k_groupBox2);
		this.TabPage_stationary.Controls.Add(this.k_groupBox1);
		this.TabPage_stationary.Controls.Add(this.k_dataGridView);
		this.TabPage_stationary.Controls.Add(this.k_start);
		this.TabPage_stationary.Controls.Add(this.k_cancel);
		this.TabPage_stationary.Controls.Add(this.k_listup);
		this.TabPage_stationary.Location = new System.Drawing.Point(4, 22);
		this.TabPage_stationary.Name = "TabPage_stationary";
		this.TabPage_stationary.Padding = new System.Windows.Forms.Padding(3);
		this.TabPage_stationary.Size = new System.Drawing.Size(1085, 611);
		this.TabPage_stationary.TabIndex = 0;
		this.TabPage_stationary.Text = "固定";
		this.TabPage_stationary.UseVisualStyleBackColor = true;
		this.groupBox5.Controls.Add(this.LG_stationary);
		this.groupBox5.Controls.Add(this.FR_stationary);
		this.groupBox5.Controls.Add(this.Sapphire_stationary);
		this.groupBox5.Controls.Add(this.Em_stationary);
		this.groupBox5.Controls.Add(this.Ruby_stationary);
		this.groupBox5.Location = new System.Drawing.Point(842, 346);
		this.groupBox5.Name = "groupBox5";
		this.groupBox5.Size = new System.Drawing.Size(237, 96);
		this.groupBox5.TabIndex = 202;
		this.groupBox5.TabStop = false;
		this.groupBox5.Text = "バージョン";
		this.LG_stationary.AutoSize = true;
		this.LG_stationary.Location = new System.Drawing.Point(102, 68);
		this.LG_stationary.Name = "LG_stationary";
		this.LG_stationary.Size = new System.Drawing.Size(83, 16);
		this.LG_stationary.TabIndex = 204;
		this.LG_stationary.TabStop = true;
		this.LG_stationary.Text = "リーフグリーン";
		this.LG_stationary.UseVisualStyleBackColor = true;
		this.LG_stationary.Click += new System.EventHandler(LG_stationary_Click);
		this.FR_stationary.AutoSize = true;
		this.FR_stationary.Location = new System.Drawing.Point(15, 68);
		this.FR_stationary.Name = "FR_stationary";
		this.FR_stationary.Size = new System.Drawing.Size(81, 16);
		this.FR_stationary.TabIndex = 102;
		this.FR_stationary.Text = "ファイアレッド";
		this.FR_stationary.UseVisualStyleBackColor = true;
		this.FR_stationary.Click += new System.EventHandler(FRLG_stationary_Click);
		this.Sapphire_stationary.AutoSize = true;
		this.Sapphire_stationary.Location = new System.Drawing.Point(72, 23);
		this.Sapphire_stationary.Name = "Sapphire_stationary";
		this.Sapphire_stationary.Size = new System.Drawing.Size(66, 16);
		this.Sapphire_stationary.TabIndex = 203;
		this.Sapphire_stationary.TabStop = true;
		this.Sapphire_stationary.Text = "サファイア";
		this.Sapphire_stationary.UseVisualStyleBackColor = true;
		this.Sapphire_stationary.Click += new System.EventHandler(S_stationary_Click);
		this.Em_stationary.AutoSize = true;
		this.Em_stationary.Location = new System.Drawing.Point(15, 45);
		this.Em_stationary.Name = "Em_stationary";
		this.Em_stationary.Size = new System.Drawing.Size(67, 16);
		this.Em_stationary.TabIndex = 101;
		this.Em_stationary.Text = "エメラルド";
		this.Em_stationary.UseVisualStyleBackColor = true;
		this.Em_stationary.Click += new System.EventHandler(Em_stationary_Click);
		this.Ruby_stationary.AutoSize = true;
		this.Ruby_stationary.Checked = true;
		this.Ruby_stationary.Location = new System.Drawing.Point(15, 23);
		this.Ruby_stationary.Name = "Ruby_stationary";
		this.Ruby_stationary.Size = new System.Drawing.Size(51, 16);
		this.Ruby_stationary.TabIndex = 100;
		this.Ruby_stationary.TabStop = true;
		this.Ruby_stationary.Text = "ルビー";
		this.Ruby_stationary.UseVisualStyleBackColor = true;
		this.Ruby_stationary.Click += new System.EventHandler(RS_stationary_Click);
		this.k_groupBox2.Controls.Add(this.AbilityBox_stationary);
		this.k_groupBox2.Controls.Add(this.label192);
		this.k_groupBox2.Controls.Add(this.GenderBox_stationary);
		this.k_groupBox2.Controls.Add(this.label32);
		this.k_groupBox2.Controls.Add(this.panel1);
		this.k_groupBox2.Controls.Add(this.RSFLRoamingCheck);
		this.k_groupBox2.Controls.Add(this.SID_stationary);
		this.k_groupBox2.Controls.Add(this.TID_stationary);
		this.k_groupBox2.Controls.Add(this.k_search2);
		this.k_groupBox2.Controls.Add(this.k_shiny);
		this.k_groupBox2.Controls.Add(this.label6);
		this.k_groupBox2.Controls.Add(this.k_search1);
		this.k_groupBox2.Controls.Add(this.label27);
		this.k_groupBox2.Controls.Add(this.label29);
		this.k_groupBox2.Controls.Add(this.k_mezapaPower);
		this.k_groupBox2.Controls.Add(this.label31);
		this.k_groupBox2.Controls.Add(this.k_pokedex);
		this.k_groupBox2.Controls.Add(this.NatureButton_stationary);
		this.k_groupBox2.Controls.Add(this.k_mezapaType);
		this.k_groupBox2.Controls.Add(this.label33);
		this.k_groupBox2.Controls.Add(this.k_Lv);
		this.k_groupBox2.Controls.Add(this.label36);
		this.k_groupBox2.Location = new System.Drawing.Point(399, 346);
		this.k_groupBox2.Name = "k_groupBox2";
		this.k_groupBox2.Size = new System.Drawing.Size(437, 259);
		this.k_groupBox2.TabIndex = 19;
		this.k_groupBox2.TabStop = false;
		this.AbilityBox_stationary.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.AbilityBox_stationary.FormattingEnabled = true;
		this.AbilityBox_stationary.Location = new System.Drawing.Point(214, 95);
		this.AbilityBox_stationary.Name = "AbilityBox_stationary";
		this.AbilityBox_stationary.Size = new System.Drawing.Size(80, 20);
		this.AbilityBox_stationary.TabIndex = 123;
		this.label192.AutoSize = true;
		this.label192.Location = new System.Drawing.Point(179, 98);
		this.label192.Name = "label192";
		this.label192.Size = new System.Drawing.Size(29, 12);
		this.label192.TabIndex = 124;
		this.label192.Text = "特性";
		this.GenderBox_stationary.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.GenderBox_stationary.FormattingEnabled = true;
		this.GenderBox_stationary.Items.AddRange(new object[4] { "指定なし", "♂", "♀", "-" });
		this.GenderBox_stationary.Location = new System.Drawing.Point(214, 127);
		this.GenderBox_stationary.Name = "GenderBox_stationary";
		this.GenderBox_stationary.Size = new System.Drawing.Size(80, 20);
		this.GenderBox_stationary.TabIndex = 121;
		this.label32.AutoSize = true;
		this.label32.Location = new System.Drawing.Point(179, 130);
		this.label32.Name = "label32";
		this.label32.Size = new System.Drawing.Size(29, 12);
		this.label32.TabIndex = 122;
		this.label32.Text = "性別";
		this.panel1.Controls.Add(this.label30);
		this.panel1.Controls.Add(this.k_stats1);
		this.panel1.Controls.Add(this.k_stats5);
		this.panel1.Controls.Add(this.k_IVup1);
		this.panel1.Controls.Add(this.k_stats6);
		this.panel1.Controls.Add(this.k_stats4);
		this.panel1.Controls.Add(this.label37);
		this.panel1.Controls.Add(this.k_stats3);
		this.panel1.Controls.Add(this.k_IVup4);
		this.panel1.Controls.Add(this.k_stats2);
		this.panel1.Controls.Add(this.k_IVlow6);
		this.panel1.Controls.Add(this.k_IVlow5);
		this.panel1.Controls.Add(this.label35);
		this.panel1.Controls.Add(this.label4);
		this.panel1.Controls.Add(this.k_IVlow4);
		this.panel1.Controls.Add(this.label5);
		this.panel1.Controls.Add(this.k_IVup5);
		this.panel1.Controls.Add(this.k_IVlow3);
		this.panel1.Controls.Add(this.k_IVlow2);
		this.panel1.Controls.Add(this.label9);
		this.panel1.Controls.Add(this.label34);
		this.panel1.Controls.Add(this.label10);
		this.panel1.Controls.Add(this.k_IVlow1);
		this.panel1.Controls.Add(this.k_IVup6);
		this.panel1.Controls.Add(this.label28);
		this.panel1.Controls.Add(this.k_IVup2);
		this.panel1.Controls.Add(this.label41);
		this.panel1.Controls.Add(this.k_IVup3);
		this.panel1.Controls.Add(this.label42);
		this.panel1.Controls.Add(this.label38);
		this.panel1.Location = new System.Drawing.Point(19, 58);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(144, 186);
		this.panel1.TabIndex = 103;
		this.label30.AutoSize = true;
		this.label30.Location = new System.Drawing.Point(3, 8);
		this.label30.Name = "label30";
		this.label30.Size = new System.Drawing.Size(13, 12);
		this.label30.TabIndex = 38;
		this.label30.Text = "H";
		this.k_stats1.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_stats1.Location = new System.Drawing.Point(22, 3);
		this.k_stats1.Maximum = new decimal(new int[4] { 114514, 0, 0, 0 });
		this.k_stats1.Name = "k_stats1";
		this.k_stats1.Size = new System.Drawing.Size(119, 22);
		this.k_stats1.TabIndex = 37;
		this.k_stats1.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_stats1.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.k_stats5.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_stats5.Location = new System.Drawing.Point(22, 131);
		this.k_stats5.Maximum = new decimal(new int[4] { 810, 0, 0, 0 });
		this.k_stats5.Name = "k_stats5";
		this.k_stats5.Size = new System.Drawing.Size(119, 22);
		this.k_stats5.TabIndex = 41;
		this.k_stats5.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_stats5.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.k_IVup1.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_IVup1.Location = new System.Drawing.Point(96, 3);
		this.k_IVup1.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.k_IVup1.Name = "k_IVup1";
		this.k_IVup1.Size = new System.Drawing.Size(45, 22);
		this.k_IVup1.TabIndex = 26;
		this.k_IVup1.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.k_IVup1.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_IVup1.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.k_stats6.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_stats6.Location = new System.Drawing.Point(22, 163);
		this.k_stats6.Maximum = new decimal(new int[4] { 810, 0, 0, 0 });
		this.k_stats6.Name = "k_stats6";
		this.k_stats6.Size = new System.Drawing.Size(119, 22);
		this.k_stats6.TabIndex = 42;
		this.k_stats6.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_stats6.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.k_stats4.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_stats4.Location = new System.Drawing.Point(22, 99);
		this.k_stats4.Maximum = new decimal(new int[4] { 931, 0, 0, 0 });
		this.k_stats4.Name = "k_stats4";
		this.k_stats4.Size = new System.Drawing.Size(119, 22);
		this.k_stats4.TabIndex = 40;
		this.k_stats4.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_stats4.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label37.AutoSize = true;
		this.label37.Location = new System.Drawing.Point(73, 8);
		this.label37.Name = "label37";
		this.label37.Size = new System.Drawing.Size(17, 12);
		this.label37.TabIndex = 76;
		this.label37.Text = "～";
		this.k_stats3.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_stats3.Location = new System.Drawing.Point(22, 67);
		this.k_stats3.Maximum = new decimal(new int[4] { 1919, 0, 0, 0 });
		this.k_stats3.Name = "k_stats3";
		this.k_stats3.Size = new System.Drawing.Size(119, 22);
		this.k_stats3.TabIndex = 39;
		this.k_stats3.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_stats3.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.k_IVup4.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_IVup4.Location = new System.Drawing.Point(96, 99);
		this.k_IVup4.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.k_IVup4.Name = "k_IVup4";
		this.k_IVup4.Size = new System.Drawing.Size(45, 22);
		this.k_IVup4.TabIndex = 32;
		this.k_IVup4.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.k_IVup4.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_IVup4.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.k_stats2.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_stats2.Location = new System.Drawing.Point(22, 35);
		this.k_stats2.Maximum = new decimal(new int[4] { 810, 0, 0, 0 });
		this.k_stats2.Name = "k_stats2";
		this.k_stats2.Size = new System.Drawing.Size(119, 22);
		this.k_stats2.TabIndex = 38;
		this.k_stats2.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_stats2.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.k_IVlow6.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_IVlow6.Location = new System.Drawing.Point(22, 163);
		this.k_IVlow6.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.k_IVlow6.Name = "k_IVlow6";
		this.k_IVlow6.Size = new System.Drawing.Size(45, 22);
		this.k_IVlow6.TabIndex = 35;
		this.k_IVlow6.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_IVlow6.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.k_IVlow5.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_IVlow5.Location = new System.Drawing.Point(22, 131);
		this.k_IVlow5.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.k_IVlow5.Name = "k_IVlow5";
		this.k_IVlow5.Size = new System.Drawing.Size(45, 22);
		this.k_IVlow5.TabIndex = 33;
		this.k_IVlow5.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_IVlow5.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label35.AutoSize = true;
		this.label35.Location = new System.Drawing.Point(73, 136);
		this.label35.Name = "label35";
		this.label35.Size = new System.Drawing.Size(17, 12);
		this.label35.TabIndex = 84;
		this.label35.Text = "～";
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(3, 168);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(12, 12);
		this.label4.TabIndex = 48;
		this.label4.Text = "S";
		this.k_IVlow4.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_IVlow4.Location = new System.Drawing.Point(22, 99);
		this.k_IVlow4.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.k_IVlow4.Name = "k_IVlow4";
		this.k_IVlow4.Size = new System.Drawing.Size(45, 22);
		this.k_IVlow4.TabIndex = 31;
		this.k_IVlow4.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_IVlow4.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(3, 136);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(13, 12);
		this.label5.TabIndex = 46;
		this.label5.Text = "D";
		this.k_IVup5.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_IVup5.Location = new System.Drawing.Point(96, 131);
		this.k_IVup5.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.k_IVup5.Name = "k_IVup5";
		this.k_IVup5.Size = new System.Drawing.Size(45, 22);
		this.k_IVup5.TabIndex = 34;
		this.k_IVup5.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.k_IVup5.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_IVup5.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.k_IVlow3.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_IVlow3.Location = new System.Drawing.Point(22, 67);
		this.k_IVlow3.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.k_IVlow3.Name = "k_IVlow3";
		this.k_IVlow3.Size = new System.Drawing.Size(45, 22);
		this.k_IVlow3.TabIndex = 29;
		this.k_IVlow3.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_IVlow3.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.k_IVlow2.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_IVlow2.Location = new System.Drawing.Point(22, 35);
		this.k_IVlow2.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.k_IVlow2.Name = "k_IVlow2";
		this.k_IVlow2.Size = new System.Drawing.Size(45, 22);
		this.k_IVlow2.TabIndex = 27;
		this.k_IVlow2.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_IVlow2.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label9.AutoSize = true;
		this.label9.Location = new System.Drawing.Point(3, 104);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(13, 12);
		this.label9.TabIndex = 44;
		this.label9.Text = "C";
		this.label34.AutoSize = true;
		this.label34.Location = new System.Drawing.Point(73, 168);
		this.label34.Name = "label34";
		this.label34.Size = new System.Drawing.Size(17, 12);
		this.label34.TabIndex = 86;
		this.label34.Text = "～";
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(3, 72);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(13, 12);
		this.label10.TabIndex = 42;
		this.label10.Text = "B";
		this.k_IVlow1.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_IVlow1.Location = new System.Drawing.Point(22, 3);
		this.k_IVlow1.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.k_IVlow1.Name = "k_IVlow1";
		this.k_IVlow1.Size = new System.Drawing.Size(45, 22);
		this.k_IVlow1.TabIndex = 25;
		this.k_IVlow1.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_IVlow1.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.k_IVup6.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_IVup6.Location = new System.Drawing.Point(96, 163);
		this.k_IVup6.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.k_IVup6.Name = "k_IVup6";
		this.k_IVup6.Size = new System.Drawing.Size(45, 22);
		this.k_IVup6.TabIndex = 36;
		this.k_IVup6.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.k_IVup6.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_IVup6.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label28.AutoSize = true;
		this.label28.Location = new System.Drawing.Point(3, 40);
		this.label28.Name = "label28";
		this.label28.Size = new System.Drawing.Size(13, 12);
		this.label28.TabIndex = 40;
		this.label28.Text = "A";
		this.k_IVup2.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_IVup2.Location = new System.Drawing.Point(96, 35);
		this.k_IVup2.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.k_IVup2.Name = "k_IVup2";
		this.k_IVup2.Size = new System.Drawing.Size(45, 22);
		this.k_IVup2.TabIndex = 28;
		this.k_IVup2.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.k_IVup2.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_IVup2.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label41.AutoSize = true;
		this.label41.Location = new System.Drawing.Point(73, 40);
		this.label41.Name = "label41";
		this.label41.Size = new System.Drawing.Size(17, 12);
		this.label41.TabIndex = 78;
		this.label41.Text = "～";
		this.k_IVup3.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_IVup3.Location = new System.Drawing.Point(96, 67);
		this.k_IVup3.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.k_IVup3.Name = "k_IVup3";
		this.k_IVup3.Size = new System.Drawing.Size(45, 22);
		this.k_IVup3.TabIndex = 30;
		this.k_IVup3.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.k_IVup3.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_IVup3.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label42.AutoSize = true;
		this.label42.Location = new System.Drawing.Point(73, 72);
		this.label42.Name = "label42";
		this.label42.Size = new System.Drawing.Size(17, 12);
		this.label42.TabIndex = 80;
		this.label42.Text = "～";
		this.label38.AutoSize = true;
		this.label38.Location = new System.Drawing.Point(73, 104);
		this.label38.Name = "label38";
		this.label38.Size = new System.Drawing.Size(17, 12);
		this.label38.TabIndex = 82;
		this.label38.Text = "～";
		this.RSFLRoamingCheck.AutoSize = true;
		this.RSFLRoamingCheck.BackColor = System.Drawing.Color.Transparent;
		this.RSFLRoamingCheck.Location = new System.Drawing.Point(258, 29);
		this.RSFLRoamingCheck.Name = "RSFLRoamingCheck";
		this.RSFLRoamingCheck.Size = new System.Drawing.Size(98, 16);
		this.RSFLRoamingCheck.TabIndex = 101;
		this.RSFLRoamingCheck.TabStop = false;
		this.RSFLRoamingCheck.Text = "RS/FRLG徘徊";
		this.RSFLRoamingCheck.UseVisualStyleBackColor = false;
		this.SID_stationary.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.SID_stationary.Location = new System.Drawing.Point(338, 189);
		this.SID_stationary.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.SID_stationary.Name = "SID_stationary";
		this.SID_stationary.Size = new System.Drawing.Size(80, 22);
		this.SID_stationary.TabIndex = 48;
		this.SID_stationary.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.TID_stationary.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TID_stationary.Location = new System.Drawing.Point(214, 189);
		this.TID_stationary.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.TID_stationary.Name = "TID_stationary";
		this.TID_stationary.Size = new System.Drawing.Size(80, 22);
		this.TID_stationary.TabIndex = 47;
		this.TID_stationary.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_search2.AutoSize = true;
		this.k_search2.BackColor = System.Drawing.Color.White;
		this.k_search2.Location = new System.Drawing.Point(131, 0);
		this.k_search2.Name = "k_search2";
		this.k_search2.Size = new System.Drawing.Size(109, 16);
		this.k_search2.TabIndex = 21;
		this.k_search2.TabStop = true;
		this.k_search2.Text = "能力値から検索  ";
		this.k_search2.UseVisualStyleBackColor = false;
		this.k_shiny.AutoSize = true;
		this.k_shiny.Location = new System.Drawing.Point(214, 225);
		this.k_shiny.Name = "k_shiny";
		this.k_shiny.Size = new System.Drawing.Size(103, 16);
		this.k_shiny.TabIndex = 49;
		this.k_shiny.Text = "色違いのみ出力";
		this.k_shiny.UseVisualStyleBackColor = true;
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(304, 194);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(28, 12);
		this.label6.TabIndex = 100;
		this.label6.Text = "裏ID";
		this.k_search1.AutoSize = true;
		this.k_search1.BackColor = System.Drawing.Color.White;
		this.k_search1.Checked = true;
		this.k_search1.Location = new System.Drawing.Point(24, 0);
		this.k_search1.Name = "k_search1";
		this.k_search1.Size = new System.Drawing.Size(109, 16);
		this.k_search1.TabIndex = 20;
		this.k_search1.TabStop = true;
		this.k_search1.Text = "個体値から検索  ";
		this.k_search1.UseVisualStyleBackColor = false;
		this.k_search1.CheckedChanged += new System.EventHandler(k_search1_CheckedChanged);
		this.label27.AutoSize = true;
		this.label27.Location = new System.Drawing.Point(180, 194);
		this.label27.Name = "label27";
		this.label27.Size = new System.Drawing.Size(28, 12);
		this.label27.TabIndex = 68;
		this.label27.Text = "表ID";
		this.label29.AutoSize = true;
		this.label29.Location = new System.Drawing.Point(362, 162);
		this.label29.Name = "label29";
		this.label29.Size = new System.Drawing.Size(17, 12);
		this.label29.TabIndex = 99;
		this.label29.Text = "～";
		this.k_mezapaPower.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_mezapaPower.Location = new System.Drawing.Point(306, 157);
		this.k_mezapaPower.Maximum = new decimal(new int[4] { 70, 0, 0, 0 });
		this.k_mezapaPower.Minimum = new decimal(new int[4] { 30, 0, 0, 0 });
		this.k_mezapaPower.Name = "k_mezapaPower";
		this.k_mezapaPower.Size = new System.Drawing.Size(50, 22);
		this.k_mezapaPower.TabIndex = 46;
		this.k_mezapaPower.Value = new decimal(new int[4] { 30, 0, 0, 0 });
		this.k_mezapaPower.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_mezapaPower.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label31.AutoSize = true;
		this.label31.Location = new System.Drawing.Point(174, 162);
		this.label31.Name = "label31";
		this.label31.Size = new System.Drawing.Size(34, 12);
		this.label31.TabIndex = 97;
		this.label31.Text = "めざパ";
		this.k_pokedex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.k_pokedex.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_pokedex.FormattingEnabled = true;
		this.k_pokedex.Location = new System.Drawing.Point(19, 27);
		this.k_pokedex.Name = "k_pokedex";
		this.k_pokedex.Size = new System.Drawing.Size(141, 22);
		this.k_pokedex.TabIndex = 22;
		this.k_pokedex.SelectedIndexChanged += new System.EventHandler(K_pokedex_SelectedIndexChanged);
		this.NatureButton_stationary.Location = new System.Drawing.Point(214, 61);
		this.NatureButton_stationary.Name = "NatureButton_stationary";
		this.NatureButton_stationary.Size = new System.Drawing.Size(75, 23);
		this.NatureButton_stationary.TabIndex = 44;
		this.NatureButton_stationary.Text = "▼選択";
		this.NatureButton_stationary.UseVisualStyleBackColor = true;
		this.NatureButton_stationary.Click += new System.EventHandler(NatureButton_stationary_Click);
		this.k_mezapaType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.k_mezapaType.FormattingEnabled = true;
		this.k_mezapaType.Location = new System.Drawing.Point(214, 159);
		this.k_mezapaType.Name = "k_mezapaType";
		this.k_mezapaType.Size = new System.Drawing.Size(80, 20);
		this.k_mezapaType.TabIndex = 45;
		this.label33.AutoSize = true;
		this.label33.Location = new System.Drawing.Point(174, 30);
		this.label33.Name = "label33";
		this.label33.Size = new System.Drawing.Size(17, 12);
		this.label33.TabIndex = 67;
		this.label33.Text = "Lv";
		this.k_Lv.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_Lv.Increment = new decimal(new int[4]);
		this.k_Lv.Location = new System.Drawing.Point(197, 25);
		this.k_Lv.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.k_Lv.Name = "k_Lv";
		this.k_Lv.ReadOnly = true;
		this.k_Lv.Size = new System.Drawing.Size(50, 22);
		this.k_Lv.TabIndex = 23;
		this.k_Lv.Value = new decimal(new int[4] { 50, 0, 0, 0 });
		this.k_Lv.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_Lv.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.label36.AutoSize = true;
		this.label36.Location = new System.Drawing.Point(179, 66);
		this.label36.Name = "label36";
		this.label36.Size = new System.Drawing.Size(29, 12);
		this.label36.TabIndex = 66;
		this.label36.Text = "性格";
		this.k_groupBox1.Controls.Add(this.PaintPanel_stationary);
		this.k_groupBox1.Controls.Add(this.FrameRange_stationary);
		this.k_groupBox1.Controls.Add(this.TargetFrame_stationary);
		this.k_groupBox1.Controls.Add(this.LastFrame_stationary);
		this.k_groupBox1.Controls.Add(this.FirstFrame_stationary);
		this.k_groupBox1.Controls.Add(this.label2);
		this.k_groupBox1.Controls.Add(this.label44);
		this.k_groupBox1.Controls.Add(this.k_RSmax);
		this.k_groupBox1.Controls.Add(this.k_RSmin);
		this.k_groupBox1.Controls.Add(this.Method_stationary);
		this.k_groupBox1.Controls.Add(this.k_Initialseed3);
		this.k_groupBox1.Controls.Add(this.ForMultipleSeed_stationary);
		this.k_groupBox1.Controls.Add(this.label3);
		this.k_groupBox1.Controls.Add(this.ForRTC_stationary);
		this.k_groupBox1.Controls.Add(this.k_Initialseed1);
		this.k_groupBox1.Controls.Add(this.ForSimpleSeed_stationary);
		this.k_groupBox1.Controls.Add(this.label1);
		this.k_groupBox1.Controls.Add(this.SetFrameButton_stationary);
		this.k_groupBox1.Controls.Add(this.label52);
		this.k_groupBox1.Controls.Add(this.label50);
		this.k_groupBox1.Controls.Add(this.label51);
		this.k_groupBox1.Controls.Add(this.label40);
		this.k_groupBox1.Controls.Add(this.label39);
		this.k_groupBox1.Location = new System.Drawing.Point(6, 346);
		this.k_groupBox1.Name = "k_groupBox1";
		this.k_groupBox1.Size = new System.Drawing.Size(387, 259);
		this.k_groupBox1.TabIndex = 0;
		this.k_groupBox1.TabStop = false;
		this.k_groupBox1.Text = "検索範囲";
		this.FrameRange_stationary.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.FrameRange_stationary.Location = new System.Drawing.Point(210, 187);
		this.FrameRange_stationary.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.FrameRange_stationary.Name = "FrameRange_stationary";
		this.FrameRange_stationary.Size = new System.Drawing.Size(64, 22);
		this.FrameRange_stationary.TabIndex = 12;
		this.FrameRange_stationary.Value = new decimal(new int[4] { 100, 0, 0, 0 });
		this.FrameRange_stationary.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.TargetFrame_stationary.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TargetFrame_stationary.Location = new System.Drawing.Point(87, 187);
		this.TargetFrame_stationary.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.TargetFrame_stationary.Name = "TargetFrame_stationary";
		this.TargetFrame_stationary.Size = new System.Drawing.Size(93, 22);
		this.TargetFrame_stationary.TabIndex = 11;
		this.TargetFrame_stationary.Value = new decimal(new int[4] { 2049, 0, 0, 0 });
		this.TargetFrame_stationary.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.LastFrame_stationary.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.LastFrame_stationary.Location = new System.Drawing.Point(210, 155);
		this.LastFrame_stationary.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.LastFrame_stationary.Name = "LastFrame_stationary";
		this.LastFrame_stationary.Size = new System.Drawing.Size(93, 22);
		this.LastFrame_stationary.TabIndex = 10;
		this.LastFrame_stationary.Value = new decimal(new int[4] { 2149, 0, 0, 0 });
		this.LastFrame_stationary.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.FirstFrame_stationary.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.FirstFrame_stationary.Location = new System.Drawing.Point(87, 155);
		this.FirstFrame_stationary.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.FirstFrame_stationary.Name = "FirstFrame_stationary";
		this.FirstFrame_stationary.Size = new System.Drawing.Size(93, 22);
		this.FirstFrame_stationary.TabIndex = 9;
		this.FirstFrame_stationary.Value = new decimal(new int[4] { 1949, 0, 0, 0 });
		this.FirstFrame_stationary.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(33, 226);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(42, 12);
		this.label2.TabIndex = 105;
		this.label2.Text = "Method";
		this.label44.AutoSize = true;
		this.label44.Location = new System.Drawing.Point(217, 62);
		this.label44.Name = "label44";
		this.label44.Size = new System.Drawing.Size(33, 12);
		this.label44.TabIndex = 104;
		this.label44.Text = "分 ～";
		this.k_RSmax.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_RSmax.Location = new System.Drawing.Point(256, 57);
		this.k_RSmax.Maximum = new decimal(new int[4] { 63349, 0, 0, 0 });
		this.k_RSmax.Name = "k_RSmax";
		this.k_RSmax.Size = new System.Drawing.Size(80, 22);
		this.k_RSmax.TabIndex = 5;
		this.k_RSmax.Value = new decimal(new int[4] { 3, 0, 0, 0 });
		this.k_RSmax.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_RSmax.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.k_RSmin.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_RSmin.Location = new System.Drawing.Point(131, 57);
		this.k_RSmin.Maximum = new decimal(new int[4] { 63349, 0, 0, 0 });
		this.k_RSmin.Name = "k_RSmin";
		this.k_RSmin.Size = new System.Drawing.Size(80, 22);
		this.k_RSmin.TabIndex = 4;
		this.k_RSmin.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.k_RSmin.Validating += new System.ComponentModel.CancelEventHandler(NumericUpDown_Check);
		this.Method_stationary.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.Method_stationary.FormattingEnabled = true;
		this.Method_stationary.Items.AddRange(new object[3] { "Method1", "Method2", "Method4" });
		this.Method_stationary.Location = new System.Drawing.Point(87, 221);
		this.Method_stationary.Name = "Method_stationary";
		this.Method_stationary.Size = new System.Drawing.Size(93, 20);
		this.Method_stationary.TabIndex = 14;
		this.k_Initialseed3.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_Initialseed3.Location = new System.Drawing.Point(131, 89);
		this.k_Initialseed3.Multiline = true;
		this.k_Initialseed3.Name = "k_Initialseed3";
		this.k_Initialseed3.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.k_Initialseed3.Size = new System.Drawing.Size(100, 56);
		this.k_Initialseed3.TabIndex = 7;
		this.k_Initialseed3.Enter += new System.EventHandler(TextBox_SelectText);
		this.ForMultipleSeed_stationary.AutoSize = true;
		this.ForMultipleSeed_stationary.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ForMultipleSeed_stationary.Location = new System.Drawing.Point(90, 96);
		this.ForMultipleSeed_stationary.Name = "ForMultipleSeed_stationary";
		this.ForMultipleSeed_stationary.Size = new System.Drawing.Size(14, 13);
		this.ForMultipleSeed_stationary.TabIndex = 6;
		this.ForMultipleSeed_stationary.TabStop = true;
		this.ForMultipleSeed_stationary.UseVisualStyleBackColor = true;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(342, 62);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(17, 12);
		this.label3.TabIndex = 79;
		this.label3.Text = "分";
		this.ForRTC_stationary.AutoSize = true;
		this.ForRTC_stationary.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ForRTC_stationary.Location = new System.Drawing.Point(90, 59);
		this.ForRTC_stationary.Name = "ForRTC_stationary";
		this.ForRTC_stationary.Size = new System.Drawing.Size(39, 18);
		this.ForRTC_stationary.TabIndex = 3;
		this.ForRTC_stationary.TabStop = true;
		this.ForRTC_stationary.Text = "  ";
		this.ForRTC_stationary.UseVisualStyleBackColor = true;
		this.k_Initialseed1.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.k_Initialseed1.Location = new System.Drawing.Point(131, 25);
		this.k_Initialseed1.Name = "k_Initialseed1";
		this.k_Initialseed1.Size = new System.Drawing.Size(80, 22);
		this.k_Initialseed1.TabIndex = 2;
		this.k_Initialseed1.Text = "0";
		this.k_Initialseed1.Enter += new System.EventHandler(TextBox_SelectText);
		this.k_Initialseed1.Validating += new System.ComponentModel.CancelEventHandler(InitialSeedBox_Check);
		this.ForSimpleSeed_stationary.AutoSize = true;
		this.ForSimpleSeed_stationary.Checked = true;
		this.ForSimpleSeed_stationary.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ForSimpleSeed_stationary.Location = new System.Drawing.Point(90, 29);
		this.ForSimpleSeed_stationary.Name = "ForSimpleSeed_stationary";
		this.ForSimpleSeed_stationary.Size = new System.Drawing.Size(14, 13);
		this.ForSimpleSeed_stationary.TabIndex = 1;
		this.ForSimpleSeed_stationary.TabStop = true;
		this.ForSimpleSeed_stationary.UseVisualStyleBackColor = true;
		this.ForSimpleSeed_stationary.CheckedChanged += new System.EventHandler(RadioButton_Checked);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(22, 30);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(53, 12);
		this.label1.TabIndex = 70;
		this.label1.Text = "初期seed";
		this.SetFrameButton_stationary.Location = new System.Drawing.Point(280, 186);
		this.SetFrameButton_stationary.Name = "SetFrameButton_stationary";
		this.SetFrameButton_stationary.Size = new System.Drawing.Size(23, 23);
		this.SetFrameButton_stationary.TabIndex = 13;
		this.SetFrameButton_stationary.Text = "↑";
		this.SetFrameButton_stationary.UseVisualStyleBackColor = true;
		this.SetFrameButton_stationary.Click += new System.EventHandler(k_update_frame_Click);
		this.label52.AutoSize = true;
		this.label52.Location = new System.Drawing.Point(308, 192);
		this.label52.Name = "label52";
		this.label52.Size = new System.Drawing.Size(73, 12);
		this.label52.TabIndex = 69;
		this.label52.Text = "(リスト表示用)";
		this.label50.AutoSize = true;
		this.label50.Location = new System.Drawing.Point(39, 192);
		this.label50.Name = "label50";
		this.label50.Size = new System.Drawing.Size(36, 12);
		this.label50.TabIndex = 68;
		this.label50.Text = "目標F";
		this.label51.Location = new System.Drawing.Point(186, 191);
		this.label51.Name = "label51";
		this.label51.Size = new System.Drawing.Size(18, 16);
		this.label51.TabIndex = 65;
		this.label51.Text = "±";
		this.label40.AutoSize = true;
		this.label40.Location = new System.Drawing.Point(63, 160);
		this.label40.Name = "label40";
		this.label40.Size = new System.Drawing.Size(12, 12);
		this.label40.TabIndex = 64;
		this.label40.Text = "F";
		this.label39.Location = new System.Drawing.Point(186, 160);
		this.label39.Name = "label39";
		this.label39.Size = new System.Drawing.Size(18, 16);
		this.label39.TabIndex = 61;
		this.label39.Text = "～";
		this.k_dataGridView.AllowUserToAddRows = false;
		this.k_dataGridView.AllowUserToDeleteRows = false;
		this.k_dataGridView.AllowUserToResizeColumns = false;
		this.k_dataGridView.AllowUserToResizeRows = false;
		dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
		this.k_dataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle5;
		this.k_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.k_dataGridView.ContextMenuStrip = this.contextMenuStrip1;
		this.k_dataGridView.Location = new System.Drawing.Point(6, 6);
		this.k_dataGridView.Name = "k_dataGridView";
		this.k_dataGridView.ReadOnly = true;
		this.k_dataGridView.RowHeadersWidth = 30;
		this.k_dataGridView.RowTemplate.Height = 21;
		this.k_dataGridView.Size = new System.Drawing.Size(1073, 334);
		this.k_dataGridView.TabIndex = 0;
		this.k_dataGridView.TabStop = false;
		this.k_dataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(DataGridView1_CellFormatting);
		this.k_dataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(dataGridView_MouseDown);
		this.k_start.Location = new System.Drawing.Point(842, 448);
		this.k_start.Name = "k_start";
		this.k_start.Size = new System.Drawing.Size(75, 23);
		this.k_start.TabIndex = 50;
		this.k_start.Text = "計算";
		this.k_start.UseVisualStyleBackColor = true;
		this.k_start.Click += new System.EventHandler(k_start_Click);
		this.k_cancel.Enabled = false;
		this.k_cancel.Location = new System.Drawing.Point(1004, 448);
		this.k_cancel.Name = "k_cancel";
		this.k_cancel.Size = new System.Drawing.Size(75, 23);
		this.k_cancel.TabIndex = 52;
		this.k_cancel.Text = "キャンセル";
		this.k_cancel.UseVisualStyleBackColor = true;
		this.k_cancel.Click += new System.EventHandler(k_cancel_Click);
		this.k_listup.Location = new System.Drawing.Point(923, 448);
		this.k_listup.Name = "k_listup";
		this.k_listup.Size = new System.Drawing.Size(75, 23);
		this.k_listup.TabIndex = 51;
		this.k_listup.Text = "リスト表示";
		this.k_listup.UseVisualStyleBackColor = true;
		this.k_listup.Click += new System.EventHandler(k_list_Click);
		this.tabControl1.Controls.Add(this.TabPage_stationary);
		this.tabControl1.Controls.Add(this.TabPage_wild);
		this.tabControl1.Controls.Add(this.TabPage_Egg);
		this.tabControl1.Controls.Add(this.TabPage_ID);
		this.tabControl1.Controls.Add(this.TabPage_backword);
		this.tabControl1.Controls.Add(this.TabPage_other);
		this.tabControl1.Location = new System.Drawing.Point(12, 12);
		this.tabControl1.Name = "tabControl1";
		this.tabControl1.SelectedIndex = 0;
		this.tabControl1.Size = new System.Drawing.Size(1093, 637);
		this.tabControl1.TabIndex = 0;
		this.tabControl1.TabStop = false;
		this.tabControl1.Click += new System.EventHandler(tabControl1_Click);
		this.tabControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(UnvisiblizeNaturePanel);
		this.TabPage_backword.Controls.Add(this.NaturePanel_back);
		this.TabPage_backword.Controls.Add(this.label148);
		this.TabPage_backword.Controls.Add(this.MaxLvBox_back);
		this.TabPage_backword.Controls.Add(this.MinLvBox_back);
		this.TabPage_backword.Controls.Add(this.checkLv_back);
		this.TabPage_backword.Controls.Add(this.checkSpecies_back);
		this.TabPage_backword.Controls.Add(this.PokemonBox_back);
		this.TabPage_backword.Controls.Add(this.EncounterTable_back);
		this.TabPage_backword.Controls.Add(this.Map_back);
		this.TabPage_backword.Controls.Add(this.groupBox10);
		this.TabPage_backword.Controls.Add(this.groupBox11);
		this.TabPage_backword.Controls.Add(this.groupBox2);
		this.TabPage_backword.Controls.Add(this.dataGridView1_back);
		this.TabPage_backword.Controls.Add(this.CalcButton_back);
		this.TabPage_backword.Controls.Add(this.DataCount_back);
		this.TabPage_backword.Controls.Add(this.Wild_back);
		this.TabPage_backword.Controls.Add(this.Stationary_back);
		this.TabPage_backword.Location = new System.Drawing.Point(4, 22);
		this.TabPage_backword.Name = "TabPage_backword";
		this.TabPage_backword.Size = new System.Drawing.Size(1085, 611);
		this.TabPage_backword.TabIndex = 5;
		this.TabPage_backword.Text = "個体逆算";
		this.TabPage_backword.UseVisualStyleBackColor = true;
		this.label148.AutoSize = true;
		this.label148.Location = new System.Drawing.Point(461, 485);
		this.label148.Name = "label148";
		this.label148.Size = new System.Drawing.Size(17, 12);
		this.label148.TabIndex = 305;
		this.label148.Text = "～";
		this.MaxLvBox_back.Enabled = false;
		this.MaxLvBox_back.Location = new System.Drawing.Point(484, 483);
		this.MaxLvBox_back.Name = "MaxLvBox_back";
		this.MaxLvBox_back.Size = new System.Drawing.Size(46, 19);
		this.MaxLvBox_back.TabIndex = 304;
		this.MinLvBox_back.Enabled = false;
		this.MinLvBox_back.Location = new System.Drawing.Point(409, 483);
		this.MinLvBox_back.Name = "MinLvBox_back";
		this.MinLvBox_back.Size = new System.Drawing.Size(46, 19);
		this.MinLvBox_back.TabIndex = 303;
		this.checkLv_back.AutoSize = true;
		this.checkLv_back.Enabled = false;
		this.checkLv_back.Location = new System.Drawing.Point(342, 484);
		this.checkLv_back.Name = "checkLv_back";
		this.checkLv_back.Size = new System.Drawing.Size(53, 16);
		this.checkLv_back.TabIndex = 302;
		this.checkLv_back.Text = "レベル";
		this.checkLv_back.UseVisualStyleBackColor = true;
		this.checkLv_back.CheckedChanged += new System.EventHandler(CheckLv_back_CheckedChanged);
		this.checkSpecies_back.AutoSize = true;
		this.checkSpecies_back.Location = new System.Drawing.Point(342, 459);
		this.checkSpecies_back.Name = "checkSpecies_back";
		this.checkSpecies_back.Size = new System.Drawing.Size(61, 16);
		this.checkSpecies_back.TabIndex = 301;
		this.checkSpecies_back.Text = "ポケモン";
		this.checkSpecies_back.UseVisualStyleBackColor = true;
		this.checkSpecies_back.CheckedChanged += new System.EventHandler(CheckSpecies_back_CheckedChanged);
		this.PokemonBox_back.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.PokemonBox_back.Enabled = false;
		this.PokemonBox_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.PokemonBox_back.FormattingEnabled = true;
		this.PokemonBox_back.Location = new System.Drawing.Point(409, 457);
		this.PokemonBox_back.Name = "PokemonBox_back";
		this.PokemonBox_back.Size = new System.Drawing.Size(121, 22);
		this.PokemonBox_back.TabIndex = 300;
		this.PokemonBox_back.SelectedIndexChanged += new System.EventHandler(PokemonBox_back_SelectedIndexChanged);
		this.EncounterTable_back.AllowUserToAddRows = false;
		this.EncounterTable_back.AllowUserToDeleteRows = false;
		this.EncounterTable_back.AllowUserToResizeColumns = false;
		this.EncounterTable_back.AllowUserToResizeRows = false;
		this.EncounterTable_back.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
		this.EncounterTable_back.BackgroundColor = System.Drawing.SystemColors.Window;
		this.EncounterTable_back.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.EncounterTable_back.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.EncounterTable_back.ColumnHeadersVisible = false;
		this.EncounterTable_back.Columns.AddRange(this.dataGridViewTextBoxColumn30, this.dataGridViewTextBoxColumn57, this.dataGridViewTextBoxColumn66, this.dataGridViewTextBoxColumn67, this.dataGridViewTextBoxColumn68, this.dataGridViewTextBoxColumn69, this.dataGridViewTextBoxColumn70, this.dataGridViewTextBoxColumn71, this.dataGridViewTextBoxColumn72, this.dataGridViewTextBoxColumn73, this.dataGridViewTextBoxColumn74, this.dataGridViewTextBoxColumn75);
		this.EncounterTable_back.Enabled = false;
		this.EncounterTable_back.Location = new System.Drawing.Point(263, 299);
		this.EncounterTable_back.Name = "EncounterTable_back";
		this.EncounterTable_back.ReadOnly = true;
		this.EncounterTable_back.RowHeadersVisible = false;
		this.EncounterTable_back.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToFirstHeader;
		this.EncounterTable_back.RowTemplate.Height = 21;
		this.EncounterTable_back.Size = new System.Drawing.Size(816, 66);
		this.EncounterTable_back.TabIndex = 299;
		this.dataGridViewTextBoxColumn30.HeaderText = "1";
		this.dataGridViewTextBoxColumn30.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn30.Name = "dataGridViewTextBoxColumn30";
		this.dataGridViewTextBoxColumn30.ReadOnly = true;
		this.dataGridViewTextBoxColumn57.HeaderText = "2";
		this.dataGridViewTextBoxColumn57.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn57.Name = "dataGridViewTextBoxColumn57";
		this.dataGridViewTextBoxColumn57.ReadOnly = true;
		this.dataGridViewTextBoxColumn66.HeaderText = "3";
		this.dataGridViewTextBoxColumn66.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn66.Name = "dataGridViewTextBoxColumn66";
		this.dataGridViewTextBoxColumn66.ReadOnly = true;
		this.dataGridViewTextBoxColumn67.HeaderText = "4";
		this.dataGridViewTextBoxColumn67.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn67.Name = "dataGridViewTextBoxColumn67";
		this.dataGridViewTextBoxColumn67.ReadOnly = true;
		this.dataGridViewTextBoxColumn68.HeaderText = "5";
		this.dataGridViewTextBoxColumn68.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn68.Name = "dataGridViewTextBoxColumn68";
		this.dataGridViewTextBoxColumn68.ReadOnly = true;
		this.dataGridViewTextBoxColumn69.HeaderText = "6";
		this.dataGridViewTextBoxColumn69.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn69.Name = "dataGridViewTextBoxColumn69";
		this.dataGridViewTextBoxColumn69.ReadOnly = true;
		this.dataGridViewTextBoxColumn70.HeaderText = "7";
		this.dataGridViewTextBoxColumn70.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn70.Name = "dataGridViewTextBoxColumn70";
		this.dataGridViewTextBoxColumn70.ReadOnly = true;
		this.dataGridViewTextBoxColumn71.HeaderText = "8";
		this.dataGridViewTextBoxColumn71.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn71.Name = "dataGridViewTextBoxColumn71";
		this.dataGridViewTextBoxColumn71.ReadOnly = true;
		this.dataGridViewTextBoxColumn72.HeaderText = "9";
		this.dataGridViewTextBoxColumn72.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn72.Name = "dataGridViewTextBoxColumn72";
		this.dataGridViewTextBoxColumn72.ReadOnly = true;
		this.dataGridViewTextBoxColumn73.HeaderText = "10";
		this.dataGridViewTextBoxColumn73.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn73.Name = "dataGridViewTextBoxColumn73";
		this.dataGridViewTextBoxColumn73.ReadOnly = true;
		this.dataGridViewTextBoxColumn74.HeaderText = "11";
		this.dataGridViewTextBoxColumn74.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn74.Name = "dataGridViewTextBoxColumn74";
		this.dataGridViewTextBoxColumn74.ReadOnly = true;
		this.dataGridViewTextBoxColumn75.HeaderText = "12";
		this.dataGridViewTextBoxColumn75.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn75.Name = "dataGridViewTextBoxColumn75";
		this.dataGridViewTextBoxColumn75.ReadOnly = true;
		this.Map_back.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.Map_back.FormattingEnabled = true;
		this.Map_back.Location = new System.Drawing.Point(319, 431);
		this.Map_back.Name = "Map_back";
		this.Map_back.Size = new System.Drawing.Size(211, 20);
		this.Map_back.TabIndex = 261;
		this.Map_back.TabStop = false;
		this.Map_back.SelectedIndexChanged += new System.EventHandler(Map_back_SelectedIndexChanged);
		this.groupBox10.Controls.Add(this.OldRod_back);
		this.groupBox10.Controls.Add(this.GoodRod_back);
		this.groupBox10.Controls.Add(this.SuperRod_back);
		this.groupBox10.Controls.Add(this.RockSmash_back);
		this.groupBox10.Controls.Add(this.Surf_back);
		this.groupBox10.Controls.Add(this.Grass_back);
		this.groupBox10.Location = new System.Drawing.Point(709, 374);
		this.groupBox10.Name = "groupBox10";
		this.groupBox10.Size = new System.Drawing.Size(337, 56);
		this.groupBox10.TabIndex = 297;
		this.groupBox10.TabStop = false;
		this.groupBox10.Text = "エンカウント方法";
		this.OldRod_back.AutoSize = true;
		this.OldRod_back.Location = new System.Drawing.Point(117, 23);
		this.OldRod_back.Name = "OldRod_back";
		this.OldRod_back.Size = new System.Drawing.Size(42, 16);
		this.OldRod_back.TabIndex = 108;
		this.OldRod_back.Text = "ボロ";
		this.OldRod_back.UseVisualStyleBackColor = true;
		this.OldRod_back.CheckedChanged += new System.EventHandler(EncounterType_back_ChackedChanged);
		this.GoodRod_back.AutoSize = true;
		this.GoodRod_back.Location = new System.Drawing.Point(165, 23);
		this.GoodRod_back.Name = "GoodRod_back";
		this.GoodRod_back.Size = new System.Drawing.Size(43, 16);
		this.GoodRod_back.TabIndex = 107;
		this.GoodRod_back.Text = "いい";
		this.GoodRod_back.UseVisualStyleBackColor = true;
		this.GoodRod_back.CheckedChanged += new System.EventHandler(EncounterType_back_ChackedChanged);
		this.SuperRod_back.AutoSize = true;
		this.SuperRod_back.Location = new System.Drawing.Point(214, 23);
		this.SuperRod_back.Name = "SuperRod_back";
		this.SuperRod_back.Size = new System.Drawing.Size(52, 16);
		this.SuperRod_back.TabIndex = 106;
		this.SuperRod_back.Text = "すごい";
		this.SuperRod_back.UseVisualStyleBackColor = true;
		this.SuperRod_back.CheckedChanged += new System.EventHandler(EncounterType_back_ChackedChanged);
		this.RockSmash_back.AutoSize = true;
		this.RockSmash_back.Location = new System.Drawing.Point(272, 23);
		this.RockSmash_back.Name = "RockSmash_back";
		this.RockSmash_back.Size = new System.Drawing.Size(56, 16);
		this.RockSmash_back.TabIndex = 105;
		this.RockSmash_back.Text = "岩砕き";
		this.RockSmash_back.UseVisualStyleBackColor = true;
		this.RockSmash_back.CheckedChanged += new System.EventHandler(EncounterType_back_ChackedChanged);
		this.Surf_back.AutoSize = true;
		this.Surf_back.Location = new System.Drawing.Point(56, 23);
		this.Surf_back.Name = "Surf_back";
		this.Surf_back.Size = new System.Drawing.Size(55, 16);
		this.Surf_back.TabIndex = 104;
		this.Surf_back.Text = "波乗り";
		this.Surf_back.UseVisualStyleBackColor = true;
		this.Surf_back.CheckedChanged += new System.EventHandler(EncounterType_back_ChackedChanged);
		this.Grass_back.AutoSize = true;
		this.Grass_back.Checked = true;
		this.Grass_back.Location = new System.Drawing.Point(15, 23);
		this.Grass_back.Name = "Grass_back";
		this.Grass_back.Size = new System.Drawing.Size(35, 16);
		this.Grass_back.TabIndex = 103;
		this.Grass_back.TabStop = true;
		this.Grass_back.Text = "草";
		this.Grass_back.UseVisualStyleBackColor = true;
		this.Grass_back.CheckedChanged += new System.EventHandler(EncounterType_back_ChackedChanged);
		this.groupBox11.Controls.Add(this.LG_back);
		this.groupBox11.Controls.Add(this.Sapphire_back);
		this.groupBox11.Controls.Add(this.FR_back);
		this.groupBox11.Controls.Add(this.Em_back);
		this.groupBox11.Controls.Add(this.Ruby_back);
		this.groupBox11.Location = new System.Drawing.Point(319, 374);
		this.groupBox11.Name = "groupBox11";
		this.groupBox11.Size = new System.Drawing.Size(399, 56);
		this.groupBox11.TabIndex = 296;
		this.groupBox11.TabStop = false;
		this.groupBox11.Text = "バージョン";
		this.LG_back.AutoSize = true;
		this.LG_back.Location = new System.Drawing.Point(304, 23);
		this.LG_back.Name = "LG_back";
		this.LG_back.Size = new System.Drawing.Size(83, 16);
		this.LG_back.TabIndex = 104;
		this.LG_back.TabStop = true;
		this.LG_back.Text = "リーフグリーン";
		this.LG_back.UseVisualStyleBackColor = true;
		this.LG_back.CheckedChanged += new System.EventHandler(Rom_back_CheckedChanged);
		this.Sapphire_back.AutoSize = true;
		this.Sapphire_back.Location = new System.Drawing.Point(72, 23);
		this.Sapphire_back.Name = "Sapphire_back";
		this.Sapphire_back.Size = new System.Drawing.Size(66, 16);
		this.Sapphire_back.TabIndex = 103;
		this.Sapphire_back.TabStop = true;
		this.Sapphire_back.Text = "サファイア";
		this.Sapphire_back.UseVisualStyleBackColor = true;
		this.Sapphire_back.CheckedChanged += new System.EventHandler(Rom_back_CheckedChanged);
		this.FR_back.AutoSize = true;
		this.FR_back.Location = new System.Drawing.Point(217, 23);
		this.FR_back.Name = "FR_back";
		this.FR_back.Size = new System.Drawing.Size(81, 16);
		this.FR_back.TabIndex = 102;
		this.FR_back.Text = "ファイアレッド";
		this.FR_back.UseVisualStyleBackColor = true;
		this.FR_back.CheckedChanged += new System.EventHandler(Rom_back_CheckedChanged);
		this.Em_back.AutoSize = true;
		this.Em_back.Location = new System.Drawing.Point(144, 23);
		this.Em_back.Name = "Em_back";
		this.Em_back.Size = new System.Drawing.Size(67, 16);
		this.Em_back.TabIndex = 101;
		this.Em_back.Text = "エメラルド";
		this.Em_back.UseVisualStyleBackColor = true;
		this.Em_back.CheckedChanged += new System.EventHandler(Rom_back_CheckedChanged);
		this.Ruby_back.AutoSize = true;
		this.Ruby_back.Checked = true;
		this.Ruby_back.Location = new System.Drawing.Point(15, 23);
		this.Ruby_back.Name = "Ruby_back";
		this.Ruby_back.Size = new System.Drawing.Size(51, 16);
		this.Ruby_back.TabIndex = 100;
		this.Ruby_back.TabStop = true;
		this.Ruby_back.Text = "ルビー";
		this.Ruby_back.UseVisualStyleBackColor = true;
		this.Ruby_back.CheckedChanged += new System.EventHandler(Rom_back_CheckedChanged);
		this.groupBox2.Controls.Add(this.SID_back);
		this.groupBox2.Controls.Add(this.Method1_back);
		this.groupBox2.Controls.Add(this.label194);
		this.groupBox2.Controls.Add(this.TID_back);
		this.groupBox2.Controls.Add(this.Method2_back);
		this.groupBox2.Controls.Add(this.OnlyShiny_back);
		this.groupBox2.Controls.Add(this.Method3_back);
		this.groupBox2.Controls.Add(this.label197);
		this.groupBox2.Controls.Add(this.label196);
		this.groupBox2.Controls.Add(this.HiddenPowerPower_back);
		this.groupBox2.Controls.Add(this.Amax_back);
		this.groupBox2.Controls.Add(this.label198);
		this.groupBox2.Controls.Add(this.label191);
		this.groupBox2.Controls.Add(this.HiddenPowerType_back);
		this.groupBox2.Controls.Add(this.label190);
		this.groupBox2.Controls.Add(this.Bmax_back);
		this.groupBox2.Controls.Add(this.NatureButton_back);
		this.groupBox2.Controls.Add(this.Hmax_back);
		this.groupBox2.Controls.Add(this.label200);
		this.groupBox2.Controls.Add(this.label189);
		this.groupBox2.Controls.Add(this.label154);
		this.groupBox2.Controls.Add(this.label188);
		this.groupBox2.Controls.Add(this.label161);
		this.groupBox2.Controls.Add(this.Cmax_back);
		this.groupBox2.Controls.Add(this.label176);
		this.groupBox2.Controls.Add(this.Smin_back);
		this.groupBox2.Controls.Add(this.label183);
		this.groupBox2.Controls.Add(this.Dmin_back);
		this.groupBox2.Controls.Add(this.label184);
		this.groupBox2.Controls.Add(this.label187);
		this.groupBox2.Controls.Add(this.label185);
		this.groupBox2.Controls.Add(this.Cmin_back);
		this.groupBox2.Controls.Add(this.Smax_back);
		this.groupBox2.Controls.Add(this.Dmax_back);
		this.groupBox2.Controls.Add(this.Hmin_back);
		this.groupBox2.Controls.Add(this.Bmin_back);
		this.groupBox2.Controls.Add(this.label186);
		this.groupBox2.Controls.Add(this.Amin_back);
		this.groupBox2.Location = new System.Drawing.Point(6, 365);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(307, 233);
		this.groupBox2.TabIndex = 295;
		this.groupBox2.TabStop = false;
		this.SID_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.SID_back.Location = new System.Drawing.Point(213, 184);
		this.SID_back.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.SID_back.Name = "SID_back";
		this.SID_back.Size = new System.Drawing.Size(80, 22);
		this.SID_back.TabIndex = 129;
		this.Method1_back.AutoSize = true;
		this.Method1_back.Checked = true;
		this.Method1_back.CheckState = System.Windows.Forms.CheckState.Checked;
		this.Method1_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Method1_back.Location = new System.Drawing.Point(17, 18);
		this.Method1_back.Name = "Method1_back";
		this.Method1_back.Size = new System.Drawing.Size(75, 18);
		this.Method1_back.TabIndex = 294;
		this.Method1_back.Text = "Method1";
		this.Method1_back.UseVisualStyleBackColor = true;
		this.Method1_back.Click += new System.EventHandler(Method1_back_Click);
		this.label194.AutoSize = true;
		this.label194.Location = new System.Drawing.Point(179, 187);
		this.label194.Name = "label194";
		this.label194.Size = new System.Drawing.Size(28, 12);
		this.label194.TabIndex = 137;
		this.label194.Text = "裏ID";
		this.TID_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TID_back.Location = new System.Drawing.Point(213, 156);
		this.TID_back.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.TID_back.Name = "TID_back";
		this.TID_back.Size = new System.Drawing.Size(80, 22);
		this.TID_back.TabIndex = 128;
		this.Method2_back.AutoSize = true;
		this.Method2_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Method2_back.Location = new System.Drawing.Point(98, 18);
		this.Method2_back.Name = "Method2_back";
		this.Method2_back.Size = new System.Drawing.Size(75, 18);
		this.Method2_back.TabIndex = 295;
		this.Method2_back.Text = "Method2";
		this.Method2_back.UseVisualStyleBackColor = true;
		this.Method2_back.Click += new System.EventHandler(Method2_back_Click);
		this.OnlyShiny_back.AutoSize = true;
		this.OnlyShiny_back.Location = new System.Drawing.Point(174, 134);
		this.OnlyShiny_back.Name = "OnlyShiny_back";
		this.OnlyShiny_back.Size = new System.Drawing.Size(58, 16);
		this.OnlyShiny_back.TabIndex = 130;
		this.OnlyShiny_back.Text = "色違い";
		this.OnlyShiny_back.UseVisualStyleBackColor = true;
		this.Method3_back.AutoSize = true;
		this.Method3_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Method3_back.Location = new System.Drawing.Point(179, 18);
		this.Method3_back.Name = "Method3_back";
		this.Method3_back.Size = new System.Drawing.Size(75, 18);
		this.Method3_back.TabIndex = 296;
		this.Method3_back.Text = "Method4";
		this.Method3_back.UseVisualStyleBackColor = true;
		this.Method3_back.Click += new System.EventHandler(Method3_back_Click);
		this.label197.AutoSize = true;
		this.label197.Location = new System.Drawing.Point(270, 103);
		this.label197.Name = "label197";
		this.label197.Size = new System.Drawing.Size(17, 12);
		this.label197.TabIndex = 136;
		this.label197.Text = "～";
		this.label196.AutoSize = true;
		this.label196.Location = new System.Drawing.Point(179, 159);
		this.label196.Name = "label196";
		this.label196.Size = new System.Drawing.Size(28, 12);
		this.label196.TabIndex = 132;
		this.label196.Text = "表ID";
		this.HiddenPowerPower_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.HiddenPowerPower_back.Location = new System.Drawing.Point(213, 100);
		this.HiddenPowerPower_back.Maximum = new decimal(new int[4] { 70, 0, 0, 0 });
		this.HiddenPowerPower_back.Minimum = new decimal(new int[4] { 30, 0, 0, 0 });
		this.HiddenPowerPower_back.Name = "HiddenPowerPower_back";
		this.HiddenPowerPower_back.Size = new System.Drawing.Size(50, 22);
		this.HiddenPowerPower_back.TabIndex = 127;
		this.HiddenPowerPower_back.Value = new decimal(new int[4] { 30, 0, 0, 0 });
		this.Amax_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Amax_back.Location = new System.Drawing.Point(108, 74);
		this.Amax_back.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Amax_back.Name = "Amax_back";
		this.Amax_back.Size = new System.Drawing.Size(45, 22);
		this.Amax_back.TabIndex = 268;
		this.Amax_back.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Amax_back.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.label198.AutoSize = true;
		this.label198.Location = new System.Drawing.Point(172, 77);
		this.label198.Name = "label198";
		this.label198.Size = new System.Drawing.Size(34, 12);
		this.label198.TabIndex = 134;
		this.label198.Text = "めざパ";
		this.label191.AutoSize = true;
		this.label191.Location = new System.Drawing.Point(85, 111);
		this.label191.Name = "label191";
		this.label191.Size = new System.Drawing.Size(17, 12);
		this.label191.TabIndex = 285;
		this.label191.Text = "～";
		this.HiddenPowerType_back.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.HiddenPowerType_back.FormattingEnabled = true;
		this.HiddenPowerType_back.Location = new System.Drawing.Point(213, 74);
		this.HiddenPowerType_back.Name = "HiddenPowerType_back";
		this.HiddenPowerType_back.Size = new System.Drawing.Size(80, 20);
		this.HiddenPowerType_back.TabIndex = 126;
		this.label190.AutoSize = true;
		this.label190.Location = new System.Drawing.Point(85, 79);
		this.label190.Name = "label190";
		this.label190.Size = new System.Drawing.Size(17, 12);
		this.label190.TabIndex = 284;
		this.label190.Text = "～";
		this.Bmax_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Bmax_back.Location = new System.Drawing.Point(108, 106);
		this.Bmax_back.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Bmax_back.Name = "Bmax_back";
		this.Bmax_back.Size = new System.Drawing.Size(45, 22);
		this.Bmax_back.TabIndex = 270;
		this.Bmax_back.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Bmax_back.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.NatureButton_back.Location = new System.Drawing.Point(212, 42);
		this.NatureButton_back.Name = "NatureButton_back";
		this.NatureButton_back.Size = new System.Drawing.Size(75, 23);
		this.NatureButton_back.TabIndex = 123;
		this.NatureButton_back.Text = "▼選択";
		this.NatureButton_back.UseVisualStyleBackColor = true;
		this.NatureButton_back.Click += new System.EventHandler(NatureButton_back_Click);
		this.Hmax_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Hmax_back.Location = new System.Drawing.Point(108, 42);
		this.Hmax_back.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Hmax_back.Name = "Hmax_back";
		this.Hmax_back.Size = new System.Drawing.Size(45, 22);
		this.Hmax_back.TabIndex = 266;
		this.Hmax_back.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Hmax_back.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.label200.AutoSize = true;
		this.label200.Location = new System.Drawing.Point(177, 47);
		this.label200.Name = "label200";
		this.label200.Size = new System.Drawing.Size(29, 12);
		this.label200.TabIndex = 131;
		this.label200.Text = "性格";
		this.label189.AutoSize = true;
		this.label189.Location = new System.Drawing.Point(85, 143);
		this.label189.Name = "label189";
		this.label189.Size = new System.Drawing.Size(17, 12);
		this.label189.TabIndex = 286;
		this.label189.Text = "～";
		this.label154.AutoSize = true;
		this.label154.Location = new System.Drawing.Point(15, 207);
		this.label154.Name = "label154";
		this.label154.Size = new System.Drawing.Size(12, 12);
		this.label154.TabIndex = 282;
		this.label154.Text = "S";
		this.label188.AutoSize = true;
		this.label188.Location = new System.Drawing.Point(85, 47);
		this.label188.Name = "label188";
		this.label188.Size = new System.Drawing.Size(17, 12);
		this.label188.TabIndex = 283;
		this.label188.Text = "～";
		this.label161.AutoSize = true;
		this.label161.Location = new System.Drawing.Point(15, 175);
		this.label161.Name = "label161";
		this.label161.Size = new System.Drawing.Size(13, 12);
		this.label161.TabIndex = 281;
		this.label161.Text = "D";
		this.Cmax_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Cmax_back.Location = new System.Drawing.Point(108, 138);
		this.Cmax_back.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Cmax_back.Name = "Cmax_back";
		this.Cmax_back.Size = new System.Drawing.Size(45, 22);
		this.Cmax_back.TabIndex = 272;
		this.Cmax_back.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Cmax_back.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.label176.AutoSize = true;
		this.label176.Location = new System.Drawing.Point(15, 143);
		this.label176.Name = "label176";
		this.label176.Size = new System.Drawing.Size(13, 12);
		this.label176.TabIndex = 280;
		this.label176.Text = "C";
		this.Smin_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Smin_back.Location = new System.Drawing.Point(34, 202);
		this.Smin_back.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Smin_back.Name = "Smin_back";
		this.Smin_back.Size = new System.Drawing.Size(45, 22);
		this.Smin_back.TabIndex = 275;
		this.Smin_back.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Smin_back.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.label183.AutoSize = true;
		this.label183.Location = new System.Drawing.Point(15, 111);
		this.label183.Name = "label183";
		this.label183.Size = new System.Drawing.Size(13, 12);
		this.label183.TabIndex = 279;
		this.label183.Text = "B";
		this.Dmin_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Dmin_back.Location = new System.Drawing.Point(34, 170);
		this.Dmin_back.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Dmin_back.Name = "Dmin_back";
		this.Dmin_back.Size = new System.Drawing.Size(45, 22);
		this.Dmin_back.TabIndex = 273;
		this.Dmin_back.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Dmin_back.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.label184.AutoSize = true;
		this.label184.Location = new System.Drawing.Point(15, 79);
		this.label184.Name = "label184";
		this.label184.Size = new System.Drawing.Size(13, 12);
		this.label184.TabIndex = 278;
		this.label184.Text = "A";
		this.label187.AutoSize = true;
		this.label187.Location = new System.Drawing.Point(85, 175);
		this.label187.Name = "label187";
		this.label187.Size = new System.Drawing.Size(17, 12);
		this.label187.TabIndex = 287;
		this.label187.Text = "～";
		this.label185.AutoSize = true;
		this.label185.Location = new System.Drawing.Point(15, 47);
		this.label185.Name = "label185";
		this.label185.Size = new System.Drawing.Size(13, 12);
		this.label185.TabIndex = 277;
		this.label185.Text = "H";
		this.Cmin_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Cmin_back.Location = new System.Drawing.Point(34, 138);
		this.Cmin_back.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Cmin_back.Name = "Cmin_back";
		this.Cmin_back.Size = new System.Drawing.Size(45, 22);
		this.Cmin_back.TabIndex = 271;
		this.Cmin_back.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Cmin_back.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.Smax_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Smax_back.Location = new System.Drawing.Point(108, 202);
		this.Smax_back.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Smax_back.Name = "Smax_back";
		this.Smax_back.Size = new System.Drawing.Size(45, 22);
		this.Smax_back.TabIndex = 276;
		this.Smax_back.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Smax_back.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.Dmax_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Dmax_back.Location = new System.Drawing.Point(108, 170);
		this.Dmax_back.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Dmax_back.Name = "Dmax_back";
		this.Dmax_back.Size = new System.Drawing.Size(45, 22);
		this.Dmax_back.TabIndex = 274;
		this.Dmax_back.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Dmax_back.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.Hmin_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Hmin_back.Location = new System.Drawing.Point(34, 42);
		this.Hmin_back.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Hmin_back.Name = "Hmin_back";
		this.Hmin_back.Size = new System.Drawing.Size(45, 22);
		this.Hmin_back.TabIndex = 265;
		this.Hmin_back.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Hmin_back.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.Bmin_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Bmin_back.Location = new System.Drawing.Point(34, 106);
		this.Bmin_back.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Bmin_back.Name = "Bmin_back";
		this.Bmin_back.Size = new System.Drawing.Size(45, 22);
		this.Bmin_back.TabIndex = 269;
		this.Bmin_back.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Bmin_back.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.label186.AutoSize = true;
		this.label186.Location = new System.Drawing.Point(85, 207);
		this.label186.Name = "label186";
		this.label186.Size = new System.Drawing.Size(17, 12);
		this.label186.TabIndex = 288;
		this.label186.Text = "～";
		this.Amin_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Amin_back.Location = new System.Drawing.Point(34, 74);
		this.Amin_back.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Amin_back.Name = "Amin_back";
		this.Amin_back.Size = new System.Drawing.Size(45, 22);
		this.Amin_back.TabIndex = 267;
		this.Amin_back.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Amin_back.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.dataGridView1_back.AllowUserToAddRows = false;
		this.dataGridView1_back.AllowUserToDeleteRows = false;
		this.dataGridView1_back.AllowUserToResizeRows = false;
		dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
		this.dataGridView1_back.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle6;
		this.dataGridView1_back.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
		this.dataGridView1_back.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGridView1_back.Columns.AddRange(this.StartSeedColumn_back, this.GenerateSeedColumn_back, this.FieldAbilityColumn_back, this.MethodColumn_back, this.SlotColumn_back, this.LvColumn_back, this.PIDColumn_back, this.NatureColumn_back, this.HColumn_back, this.AColumn_back, this.BColumn_back, this.CColumn_back, this.DColumn_back, this.SColumn_back, this.GenderColumn_back, this.AbilityColumn_back, this.HiddenPowerColumn_back);
		this.dataGridView1_back.ContextMenuStrip = this.contextMenuStrip6;
		this.dataGridView1_back.Location = new System.Drawing.Point(6, 6);
		this.dataGridView1_back.Name = "dataGridView1_back";
		this.dataGridView1_back.ReadOnly = true;
		this.dataGridView1_back.RowHeadersWidth = 30;
		this.dataGridView1_back.RowTemplate.Height = 21;
		this.dataGridView1_back.Size = new System.Drawing.Size(1073, 287);
		this.dataGridView1_back.TabIndex = 291;
		this.dataGridView1_back.TabStop = false;
		this.dataGridView1_back.MouseDown += new System.Windows.Forms.MouseEventHandler(dataGridView_MouseDown);
		this.StartSeedColumn_back.HeaderText = "開始seed";
		this.StartSeedColumn_back.MinimumWidth = 10;
		this.StartSeedColumn_back.Name = "StartSeedColumn_back";
		this.StartSeedColumn_back.ReadOnly = true;
		this.StartSeedColumn_back.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.StartSeedColumn_back.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.StartSeedColumn_back.Width = 80;
		this.GenerateSeedColumn_back.HeaderText = "生成seed";
		this.GenerateSeedColumn_back.MinimumWidth = 10;
		this.GenerateSeedColumn_back.Name = "GenerateSeedColumn_back";
		this.GenerateSeedColumn_back.ReadOnly = true;
		this.GenerateSeedColumn_back.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.GenerateSeedColumn_back.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.GenerateSeedColumn_back.Width = 80;
		this.FieldAbilityColumn_back.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.FieldAbilityColumn_back.HeaderText = "先頭の特性";
		this.FieldAbilityColumn_back.MinimumWidth = 10;
		this.FieldAbilityColumn_back.Name = "FieldAbilityColumn_back";
		this.FieldAbilityColumn_back.ReadOnly = true;
		this.FieldAbilityColumn_back.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.FieldAbilityColumn_back.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.FieldAbilityColumn_back.Width = 88;
		this.MethodColumn_back.HeaderText = "Method";
		this.MethodColumn_back.MinimumWidth = 10;
		this.MethodColumn_back.Name = "MethodColumn_back";
		this.MethodColumn_back.ReadOnly = true;
		this.MethodColumn_back.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.MethodColumn_back.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.MethodColumn_back.Width = 67;
		this.SlotColumn_back.HeaderText = "ポケモン";
		this.SlotColumn_back.MinimumWidth = 10;
		this.SlotColumn_back.Name = "SlotColumn_back";
		this.SlotColumn_back.ReadOnly = true;
		this.SlotColumn_back.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.SlotColumn_back.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.SlotColumn_back.Width = 90;
		this.LvColumn_back.HeaderText = "Lv";
		this.LvColumn_back.Name = "LvColumn_back";
		this.LvColumn_back.ReadOnly = true;
		this.LvColumn_back.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.LvColumn_back.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.LvColumn_back.Width = 42;
		this.PIDColumn_back.HeaderText = "性格値";
		this.PIDColumn_back.MinimumWidth = 10;
		this.PIDColumn_back.Name = "PIDColumn_back";
		this.PIDColumn_back.ReadOnly = true;
		this.PIDColumn_back.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.PIDColumn_back.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.PIDColumn_back.Width = 66;
		this.NatureColumn_back.HeaderText = "性格";
		this.NatureColumn_back.MinimumWidth = 10;
		this.NatureColumn_back.Name = "NatureColumn_back";
		this.NatureColumn_back.ReadOnly = true;
		this.NatureColumn_back.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.NatureColumn_back.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.NatureColumn_back.Width = 60;
		this.HColumn_back.HeaderText = "H";
		this.HColumn_back.MinimumWidth = 10;
		this.HColumn_back.Name = "HColumn_back";
		this.HColumn_back.ReadOnly = true;
		this.HColumn_back.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.HColumn_back.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.HColumn_back.Width = 27;
		this.AColumn_back.HeaderText = "A";
		this.AColumn_back.MinimumWidth = 10;
		this.AColumn_back.Name = "AColumn_back";
		this.AColumn_back.ReadOnly = true;
		this.AColumn_back.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.AColumn_back.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.AColumn_back.Width = 27;
		this.BColumn_back.HeaderText = "B";
		this.BColumn_back.MinimumWidth = 10;
		this.BColumn_back.Name = "BColumn_back";
		this.BColumn_back.ReadOnly = true;
		this.BColumn_back.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.BColumn_back.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.BColumn_back.Width = 27;
		this.CColumn_back.HeaderText = "C";
		this.CColumn_back.MinimumWidth = 10;
		this.CColumn_back.Name = "CColumn_back";
		this.CColumn_back.ReadOnly = true;
		this.CColumn_back.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.CColumn_back.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.CColumn_back.Width = 27;
		this.DColumn_back.HeaderText = "D";
		this.DColumn_back.MinimumWidth = 10;
		this.DColumn_back.Name = "DColumn_back";
		this.DColumn_back.ReadOnly = true;
		this.DColumn_back.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.DColumn_back.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.DColumn_back.Width = 27;
		this.SColumn_back.HeaderText = "S";
		this.SColumn_back.MinimumWidth = 10;
		this.SColumn_back.Name = "SColumn_back";
		this.SColumn_back.ReadOnly = true;
		this.SColumn_back.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.SColumn_back.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.SColumn_back.Width = 27;
		this.GenderColumn_back.HeaderText = "性別";
		this.GenderColumn_back.Name = "GenderColumn_back";
		this.GenderColumn_back.ReadOnly = true;
		this.GenderColumn_back.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.GenderColumn_back.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.GenderColumn_back.Width = 54;
		this.AbilityColumn_back.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.AbilityColumn_back.HeaderText = "特性";
		this.AbilityColumn_back.Name = "AbilityColumn_back";
		this.AbilityColumn_back.ReadOnly = true;
		this.AbilityColumn_back.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.AbilityColumn_back.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.AbilityColumn_back.Width = 54;
		this.HiddenPowerColumn_back.HeaderText = "めざパ";
		this.HiddenPowerColumn_back.Name = "HiddenPowerColumn_back";
		this.HiddenPowerColumn_back.ReadOnly = true;
		this.HiddenPowerColumn_back.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.HiddenPowerColumn_back.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.HiddenPowerColumn_back.Width = 60;
		this.contextMenuStrip6.ImageScalingSize = new System.Drawing.Size(32, 32);
		this.contextMenuStrip6.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem1, this.toolStripMenuItem2 });
		this.contextMenuStrip6.Name = "contextMenuStrip1";
		this.contextMenuStrip6.Size = new System.Drawing.Size(120, 48);
		this.toolStripMenuItem1.Name = "toolStripMenuItem1";
		this.toolStripMenuItem1.Size = new System.Drawing.Size(119, 22);
		this.toolStripMenuItem1.Text = "コピー";
		this.toolStripMenuItem1.Click += new System.EventHandler(copyToolStripMenuItem6_Click);
		this.toolStripMenuItem2.Name = "toolStripMenuItem2";
		this.toolStripMenuItem2.Size = new System.Drawing.Size(119, 22);
		this.toolStripMenuItem2.Text = "全て選択";
		this.toolStripMenuItem2.Click += new System.EventHandler(SelectAllToolStripMenuItem6_Click);
		this.CalcButton_back.Location = new System.Drawing.Point(963, 549);
		this.CalcButton_back.Name = "CalcButton_back";
		this.CalcButton_back.Size = new System.Drawing.Size(102, 49);
		this.CalcButton_back.TabIndex = 252;
		this.CalcButton_back.Text = "計算";
		this.CalcButton_back.UseVisualStyleBackColor = true;
		this.CalcButton_back.Click += new System.EventHandler(CalcButton_back_click);
		this.DataCount_back.AutoSize = true;
		this.DataCount_back.Location = new System.Drawing.Point(4, 296);
		this.DataCount_back.Name = "DataCount_back";
		this.DataCount_back.Size = new System.Drawing.Size(53, 12);
		this.DataCount_back.TabIndex = 290;
		this.DataCount_back.Text = "データ数:0";
		this.Wild_back.AutoSize = true;
		this.Wild_back.Location = new System.Drawing.Point(119, 326);
		this.Wild_back.Name = "Wild_back";
		this.Wild_back.Size = new System.Drawing.Size(47, 16);
		this.Wild_back.TabIndex = 293;
		this.Wild_back.Text = "野生";
		this.Wild_back.UseVisualStyleBackColor = true;
		this.Stationary_back.AutoSize = true;
		this.Stationary_back.Checked = true;
		this.Stationary_back.Location = new System.Drawing.Point(38, 326);
		this.Stationary_back.Name = "Stationary_back";
		this.Stationary_back.Size = new System.Drawing.Size(47, 16);
		this.Stationary_back.TabIndex = 292;
		this.Stationary_back.TabStop = true;
		this.Stationary_back.Text = "固定";
		this.Stationary_back.UseVisualStyleBackColor = true;
		this.TabPage_other.Controls.Add(this.groupBox15);
		this.TabPage_other.Controls.Add(this.groupBox12);
		this.TabPage_other.Controls.Add(this.groupBox14);
		this.TabPage_other.Controls.Add(this.groupBox13);
		this.TabPage_other.Controls.Add(this.groupBox1);
		this.TabPage_other.Controls.Add(this.groupBox3);
		this.TabPage_other.Controls.Add(this.poyo);
		this.TabPage_other.Controls.Add(this.groupBox4);
		this.TabPage_other.Location = new System.Drawing.Point(4, 22);
		this.TabPage_other.Name = "TabPage_other";
		this.TabPage_other.Padding = new System.Windows.Forms.Padding(3);
		this.TabPage_other.Size = new System.Drawing.Size(1085, 611);
		this.TabPage_other.TabIndex = 4;
		this.TabPage_other.Text = "その他";
		this.TabPage_other.UseVisualStyleBackColor = true;
		this.groupBox15.Controls.Add(this.button8);
		this.groupBox15.Location = new System.Drawing.Point(566, 6);
		this.groupBox15.Name = "groupBox15";
		this.groupBox15.Size = new System.Drawing.Size(106, 76);
		this.groupBox15.TabIndex = 306;
		this.groupBox15.TabStop = false;
		this.groupBox15.Text = "絵画seed調整";
		this.button8.Location = new System.Drawing.Point(15, 28);
		this.button8.Name = "button8";
		this.button8.Size = new System.Drawing.Size(75, 23);
		this.button8.TabIndex = 21;
		this.button8.Text = "開く";
		this.button8.UseVisualStyleBackColor = true;
		this.button8.Click += new System.EventHandler(button8_Click);
		this.groupBox12.Controls.Add(this.button7);
		this.groupBox12.Location = new System.Drawing.Point(454, 6);
		this.groupBox12.Name = "groupBox12";
		this.groupBox12.Size = new System.Drawing.Size(106, 76);
		this.groupBox12.TabIndex = 305;
		this.groupBox12.TabStop = false;
		this.groupBox12.Text = "絵画seed調整";
		this.button7.Location = new System.Drawing.Point(15, 28);
		this.button7.Name = "button7";
		this.button7.Size = new System.Drawing.Size(75, 23);
		this.button7.TabIndex = 21;
		this.button7.Text = "開く";
		this.button7.UseVisualStyleBackColor = true;
		this.button7.Click += new System.EventHandler(button7_Click);
		this.groupBox14.Controls.Add(this.button6);
		this.groupBox14.Location = new System.Drawing.Point(342, 6);
		this.groupBox14.Name = "groupBox14";
		this.groupBox14.Size = new System.Drawing.Size(106, 76);
		this.groupBox14.TabIndex = 304;
		this.groupBox14.TabStop = false;
		this.groupBox14.Text = "◇☆計算";
		this.button6.Location = new System.Drawing.Point(15, 28);
		this.button6.Name = "button6";
		this.button6.Size = new System.Drawing.Size(75, 23);
		this.button6.TabIndex = 21;
		this.button6.Text = "開く";
		this.button6.UseVisualStyleBackColor = true;
		this.button6.Click += new System.EventHandler(Button6_Click);
		this.groupBox13.Controls.Add(this.button5);
		this.groupBox13.Location = new System.Drawing.Point(230, 6);
		this.groupBox13.Name = "groupBox13";
		this.groupBox13.Size = new System.Drawing.Size(106, 76);
		this.groupBox13.TabIndex = 303;
		this.groupBox13.TabStop = false;
		this.groupBox13.Text = "裏ID計算";
		this.button5.Location = new System.Drawing.Point(15, 28);
		this.button5.Name = "button5";
		this.button5.Size = new System.Drawing.Size(75, 23);
		this.button5.TabIndex = 21;
		this.button5.Text = "開く";
		this.button5.UseVisualStyleBackColor = true;
		this.button5.Click += new System.EventHandler(Button5_Click);
		this.groupBox1.Controls.Add(this.button3);
		this.groupBox1.Location = new System.Drawing.Point(6, 6);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(106, 76);
		this.groupBox1.TabIndex = 303;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "個体値計算";
		this.button3.Location = new System.Drawing.Point(15, 28);
		this.button3.Name = "button3";
		this.button3.Size = new System.Drawing.Size(75, 23);
		this.button3.TabIndex = 21;
		this.button3.Text = "開く";
		this.button3.UseVisualStyleBackColor = true;
		this.button3.Click += new System.EventHandler(Button3_Click);
		this.groupBox3.Controls.Add(this.button2);
		this.groupBox3.Location = new System.Drawing.Point(118, 6);
		this.groupBox3.Name = "groupBox3";
		this.groupBox3.Size = new System.Drawing.Size(106, 76);
		this.groupBox3.TabIndex = 302;
		this.groupBox3.TabStop = false;
		this.groupBox3.Text = "初期seed検索";
		this.button2.Location = new System.Drawing.Point(15, 28);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(75, 23);
		this.button2.TabIndex = 21;
		this.button2.Text = "開く";
		this.button2.UseVisualStyleBackColor = true;
		this.button2.Click += new System.EventHandler(Button2_Click);
		this.poyo.Enabled = false;
		this.poyo.Location = new System.Drawing.Point(1033, 582);
		this.poyo.Name = "poyo";
		this.poyo.Size = new System.Drawing.Size(46, 23);
		this.poyo.TabIndex = 1;
		this.poyo.Text = "ぽよ";
		this.poyo.UseVisualStyleBackColor = true;
		this.poyo.Click += new System.EventHandler(poyo_Click);
		this.groupBox4.Controls.Add(this.L_button);
		this.groupBox4.Controls.Add(this.Language);
		this.groupBox4.Location = new System.Drawing.Point(6, 88);
		this.groupBox4.Name = "groupBox4";
		this.groupBox4.Size = new System.Drawing.Size(218, 76);
		this.groupBox4.TabIndex = 0;
		this.groupBox4.TabStop = false;
		this.groupBox4.Text = "言語";
		this.groupBox4.Visible = false;
		this.L_button.Location = new System.Drawing.Point(111, 29);
		this.L_button.Name = "L_button";
		this.L_button.Size = new System.Drawing.Size(75, 23);
		this.L_button.TabIndex = 21;
		this.L_button.Text = "適用";
		this.L_button.UseVisualStyleBackColor = true;
		this.L_button.Click += new System.EventHandler(Language_button_Click);
		this.Language.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.Language.FormattingEnabled = true;
		this.Language.Items.AddRange(new object[6] { "日本語", "英語", "ドイツ語", "フランス語", "イタリア語", "スペイン語" });
		this.Language.Location = new System.Drawing.Point(25, 31);
		this.Language.Name = "Language";
		this.Language.Size = new System.Drawing.Size(80, 20);
		this.Language.TabIndex = 20;
		this.PaintPanel_stationary.Controls.Add(this.label193);
		this.PaintPanel_stationary.Controls.Add(this.PaintSeedMaxFrameBox_stationary);
		this.PaintPanel_stationary.Controls.Add(this.PaintSeedMinFrameBox_stationary);
		this.PaintPanel_stationary.Controls.Add(this.label195);
		this.PaintPanel_stationary.Location = new System.Drawing.Point(131, 91);
		this.PaintPanel_stationary.Name = "PaintPanel_stationary";
		this.PaintPanel_stationary.Size = new System.Drawing.Size(228, 60);
		this.PaintPanel_stationary.TabIndex = 125;
		this.label193.AutoSize = true;
		this.label193.Location = new System.Drawing.Point(211, 6);
		this.label193.Name = "label193";
		this.label193.Size = new System.Drawing.Size(12, 12);
		this.label193.TabIndex = 130;
		this.label193.Text = "F";
		this.PaintSeedMaxFrameBox_stationary.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.PaintSeedMaxFrameBox_stationary.Location = new System.Drawing.Point(125, 0);
		this.PaintSeedMaxFrameBox_stationary.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.PaintSeedMaxFrameBox_stationary.Name = "PaintSeedMaxFrameBox_stationary";
		this.PaintSeedMaxFrameBox_stationary.Size = new System.Drawing.Size(80, 22);
		this.PaintSeedMaxFrameBox_stationary.TabIndex = 129;
		this.PaintSeedMaxFrameBox_stationary.Value = new decimal(new int[4] { 1500, 0, 0, 0 });
		this.PaintSeedMinFrameBox_stationary.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.PaintSeedMinFrameBox_stationary.Location = new System.Drawing.Point(0, 0);
		this.PaintSeedMinFrameBox_stationary.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.PaintSeedMinFrameBox_stationary.Name = "PaintSeedMinFrameBox_stationary";
		this.PaintSeedMinFrameBox_stationary.Size = new System.Drawing.Size(80, 22);
		this.PaintSeedMinFrameBox_stationary.TabIndex = 128;
		this.PaintSeedMinFrameBox_stationary.Value = new decimal(new int[4] { 1200, 0, 0, 0 });
		this.label195.AutoSize = true;
		this.label195.Location = new System.Drawing.Point(86, 6);
		this.label195.Name = "label195";
		this.label195.Size = new System.Drawing.Size(28, 12);
		this.label195.TabIndex = 127;
		this.label195.Text = "F ～";
		this.NaturePanel_stationary.Location = new System.Drawing.Point(614, 430);
		this.NaturePanel_stationary.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
		this.NaturePanel_stationary.Name = "NaturePanel_stationary";
		this.NaturePanel_stationary.Size = new System.Drawing.Size(388, 140);
		this.NaturePanel_stationary.TabIndex = 201;
		this.NaturePanel_stationary.TabStop = false;
		this.NaturePanel_stationary.Visible = false;
		this.NaturePanel_wild.Location = new System.Drawing.Point(614, 430);
		this.NaturePanel_wild.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
		this.NaturePanel_wild.Name = "NaturePanel_wild";
		this.NaturePanel_wild.Size = new System.Drawing.Size(388, 140);
		this.NaturePanel_wild.TabIndex = 83;
		this.NaturePanel_wild.TabStop = false;
		this.NaturePanel_wild.Visible = false;
		this.NaturePanel_EggPID.Location = new System.Drawing.Point(60, 111);
		this.NaturePanel_EggPID.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
		this.NaturePanel_EggPID.Name = "NaturePanel_EggPID";
		this.NaturePanel_EggPID.Size = new System.Drawing.Size(388, 140);
		this.NaturePanel_EggPID.TabIndex = 117;
		this.NaturePanel_EggPID.TabStop = false;
		this.NaturePanel_back.Location = new System.Drawing.Point(220, 431);
		this.NaturePanel_back.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
		this.NaturePanel_back.Name = "NaturePanel_back";
		this.NaturePanel_back.Size = new System.Drawing.Size(388, 140);
		this.NaturePanel_back.TabIndex = 138;
		this.NaturePanel_back.TabStop = false;
		this.NaturePanel_back.Visible = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1126, 661);
		base.Controls.Add(this.tabControl1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.Name = "Form1";
		this.Text = "3genSearch";
		base.Load += new System.EventHandler(Form1_Load);
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(UnvisiblizeNaturePanel);
		this.contextMenuStrip1.ResumeLayout(false);
		this.contextMenuStrip2.ResumeLayout(false);
		this.contextMenuStrip3.ResumeLayout(false);
		this.contextMenuStrip4.ResumeLayout(false);
		this.contextMenuStrip5.ResumeLayout(false);
		this.TabPage_Egg.ResumeLayout(false);
		this.tabControl2.ResumeLayout(false);
		this.TabPage_EggPID.ResumeLayout(false);
		this.es_groupBox2.ResumeLayout(false);
		this.es_groupBox2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.SID_EggPID).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TID_EggPID).EndInit();
		this.es_groupBox1.ResumeLayout(false);
		this.es_groupBox1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.DiffMax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DiffMin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.LastFrame_EggPID).EndInit();
		((System.ComponentModel.ISupportInitialize)this.FirstFrame_EggPID).EndInit();
		((System.ComponentModel.ISupportInitialize)this.es_dataGridView).EndInit();
		this.TabPage_EggIVs.ResumeLayout(false);
		this.ek_groupBox3.ResumeLayout(false);
		this.ek_groupBox3.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.ek_stats6).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_stats5).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_stats4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_stats3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_stats2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_stats1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_mezapaPower).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVup6).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVlow1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVlow2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVlow3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVup5).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVlow4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVlow5).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVlow6).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVup4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVup1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVup3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_IVup2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_Lv).EndInit();
		this.ek_groupBox2.ResumeLayout(false);
		this.ek_groupBox2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.FrameRange_EggIVs).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TargetFrame_EggIVs).EndInit();
		((System.ComponentModel.ISupportInitialize)this.LastFrame_EggIVs).EndInit();
		((System.ComponentModel.ISupportInitialize)this.FirstFrame_EggIVs).EndInit();
		this.ek_groupBox1.ResumeLayout(false);
		this.ek_groupBox1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pre_parent6).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pre_parent5).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pre_parent4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pre_parent3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pre_parent2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.post_parent6).EndInit();
		((System.ComponentModel.ISupportInitialize)this.post_parent5).EndInit();
		((System.ComponentModel.ISupportInitialize)this.post_parent4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.post_parent3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.post_parent2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.post_parent1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pre_parent1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ek_dataGridView).EndInit();
		this.TabPage_ID.ResumeLayout(false);
		this.ID_groupBox1.ResumeLayout(false);
		this.ID_groupBox1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.FrameRange_ID).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TargetFrame_ID).EndInit();
		((System.ComponentModel.ISupportInitialize)this.LastFrame_ID).EndInit();
		((System.ComponentModel.ISupportInitialize)this.FirstFrame_ID).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ID_RSmax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ID_RSmin).EndInit();
		this.ID_groupBox2.ResumeLayout(false);
		this.ID_groupBox2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.TID_ID).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SID_ID).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ID_dataGridView).EndInit();
		this.TabPage_wild.ResumeLayout(false);
		this.panel2.ResumeLayout(false);
		this.panel2.PerformLayout();
		this.groupBox6.ResumeLayout(false);
		this.groupBox6.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.y_dataGridView).EndInit();
		this.y_groupBox2.ResumeLayout(false);
		this.y_groupBox2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.SID_wild).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TID_wild).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_stats1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_stats6).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_stats5).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_stats3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_stats4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_stats2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_mezapaPower).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVup6).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVlow1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVlow2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVlow3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVup5).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVlow4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVlow5).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVlow6).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVup4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVup1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVup3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_IVup2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_Lv).EndInit();
		this.y_groupBox1.ResumeLayout(false);
		this.y_groupBox1.PerformLayout();
		this.PaintPanel_wild.ResumeLayout(false);
		this.PaintPanel_wild.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.PaintSeedMaxFrameBox_wild).EndInit();
		((System.ComponentModel.ISupportInitialize)this.PaintSeedMinFrameBox_wild).EndInit();
		((System.ComponentModel.ISupportInitialize)this.FrameRange_wild).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TargetFrame_wild).EndInit();
		((System.ComponentModel.ISupportInitialize)this.LastFrame_wild).EndInit();
		((System.ComponentModel.ISupportInitialize)this.FirstFrame_wild).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_RSmax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.y_RSmin).EndInit();
		this.groupBox8.ResumeLayout(false);
		this.groupBox8.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.dataGridView_table).EndInit();
		this.groupBox7.ResumeLayout(false);
		this.groupBox7.PerformLayout();
		this.groupBox9.ResumeLayout(false);
		this.y_groupBox3.ResumeLayout(false);
		this.y_groupBox3.PerformLayout();
		this.TabPage_stationary.ResumeLayout(false);
		this.groupBox5.ResumeLayout(false);
		this.groupBox5.PerformLayout();
		this.k_groupBox2.ResumeLayout(false);
		this.k_groupBox2.PerformLayout();
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.k_stats1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_stats5).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVup1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_stats6).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_stats4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_stats3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVup4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_stats2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVlow6).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVlow5).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVlow4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVup5).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVlow3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVlow2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVlow1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVup6).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVup2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_IVup3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SID_stationary).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TID_stationary).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_mezapaPower).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_Lv).EndInit();
		this.k_groupBox1.ResumeLayout(false);
		this.k_groupBox1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.FrameRange_stationary).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TargetFrame_stationary).EndInit();
		((System.ComponentModel.ISupportInitialize)this.LastFrame_stationary).EndInit();
		((System.ComponentModel.ISupportInitialize)this.FirstFrame_stationary).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_RSmax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_RSmin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.k_dataGridView).EndInit();
		this.tabControl1.ResumeLayout(false);
		this.TabPage_backword.ResumeLayout(false);
		this.TabPage_backword.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.MaxLvBox_back).EndInit();
		((System.ComponentModel.ISupportInitialize)this.MinLvBox_back).EndInit();
		((System.ComponentModel.ISupportInitialize)this.EncounterTable_back).EndInit();
		this.groupBox10.ResumeLayout(false);
		this.groupBox10.PerformLayout();
		this.groupBox11.ResumeLayout(false);
		this.groupBox11.PerformLayout();
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.SID_back).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TID_back).EndInit();
		((System.ComponentModel.ISupportInitialize)this.HiddenPowerPower_back).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Amax_back).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Bmax_back).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Hmax_back).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Cmax_back).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Smin_back).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Dmin_back).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Cmin_back).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Smax_back).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Dmax_back).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Hmin_back).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Bmin_back).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Amin_back).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dataGridView1_back).EndInit();
		this.contextMenuStrip6.ResumeLayout(false);
		this.TabPage_other.ResumeLayout(false);
		this.groupBox15.ResumeLayout(false);
		this.groupBox12.ResumeLayout(false);
		this.groupBox14.ResumeLayout(false);
		this.groupBox13.ResumeLayout(false);
		this.groupBox1.ResumeLayout(false);
		this.groupBox3.ResumeLayout(false);
		this.groupBox4.ResumeLayout(false);
		this.PaintPanel_stationary.ResumeLayout(false);
		this.PaintPanel_stationary.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.PaintSeedMaxFrameBox_stationary).EndInit();
		((System.ComponentModel.ISupportInitialize)this.PaintSeedMinFrameBox_stationary).EndInit();
		base.ResumeLayout(false);
	}

	private bool ExistsID(uint TID, uint SID)
	{
		uint num = SID << 16;
		for (uint num2 = 0u; num2 < 65536; num2++)
		{
			uint seed = num | num2;
			if (seed.GetRand() == TID)
			{
				return true;
			}
		}
		return false;
	}

	private Task<List<IDBinder>> CalcID(CalcParam param1, CalcIDParam param2)
	{
		return Task.Run(delegate
		{
			List<IDBinder> list = new List<IDBinder>();
			uint[] array = param2.PIDList.Select((uint _) => (_ & 0xFFFFu) ^ (_ >> 16)).ToArray();
			for (int i = 0; i < param1.InitialSeedList.Count; i++)
			{
				uint num = param1.InitialSeedList[i];
				uint seed = num.NextSeed(param1.FirstFrame);
				uint num2 = seed.GetRand();
				uint rand = seed.GetRand();
				ulong num3 = param1.FirstFrame;
				while (num3 <= param1.LastFrame)
				{
					if (param1.token.IsCancellationRequested)
					{
						param1.token.ThrowIfCancellationRequested();
					}
					if ((!param2.ExistTargetSID || num2 == param2.TargetSID) && (!param2.ExistTargetTID || rand == param2.TargetTID))
					{
						if (param2.OnlyShiny)
						{
							foreach (uint pID in param2.PIDList)
							{
								uint num4 = (pID >> 16) ^ (pID & 0xFFFFu);
								if ((num4 ^ rand ^ num2) < 8)
								{
									IDBinder item = (param1.DisplayRTC ? new IDBinder((num, param1.RTCList[i]), (uint)(num3 & 0xFFFFFFFFu), (int)(num3 - param1.TargetFrame), rand, num2, pID) : new IDBinder(num, (uint)(num3 & 0xFFFFFFFFu), (int)(num3 - param1.TargetFrame), rand, num2, pID));
									list.Add(item);
								}
							}
						}
						else
						{
							IDBinder item2 = (param1.DisplayRTC ? new IDBinder((num, param1.RTCList[i]), (uint)(num3 & 0xFFFFFFFFu), (int)(num3 - param1.TargetFrame), rand, num2) : new IDBinder(num, (uint)(num3 & 0xFFFFFFFFu), (int)(num3 - param1.TargetFrame), rand, num2));
							list.Add(item2);
						}
					}
					num3++;
					num2 = rand;
					rand = seed.GetRand();
				}
			}
			return list;
		});
	}

	private async void ID_start_Click(object sender, EventArgs e)
	{
		uint valueDec = Util.GetValueDec(TID_ID);
		uint valueDec2 = Util.GetValueDec(SID_ID);
		if (CheckTID.Checked && CheckSID.Checked && !ExistsID(valueDec, valueDec2))
		{
			MessageBox.Show("RSでは存在し得ないTIDとSIDの組み合わせです。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		if (IDInitialseed1.Checked & !Regex.IsMatch(ID_Initialseed1.Text, "^[0-9a-fA-F]{0,8}$"))
		{
			MessageBox.Show("初期seedに不正な値が含まれています。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		if (IDInitialseed1.Checked & (ID_Initialseed1.Text == ""))
		{
			MessageBox.Show("初期seedが空白になっています。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		if (IDInitialseed2.Checked & (Util.GetValueDec(ID_RSmin) > Util.GetValueDec(ID_RSmax)))
		{
			MessageBox.Show("分設定が 下限 ＞上限 になっています。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		if (Util.GetValueDec(FirstFrame_ID) > Util.GetValueDec(LastFrame_ID))
		{
			MessageBox.Show("Fが 下限 ＞上限 になっています。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		if (ID_shiny.Checked && !Regex.IsMatch(ID_PID.Text, "^[0-9a-fA-F]{0,8}$", RegexOptions.Multiline))
		{
			MessageBox.Show("不正な性格値が入力されています。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		ID_start.Enabled = false;
		ID_listup.Enabled = false;
		List<uint> pIDList = new List<uint>();
		if (ID_shiny.Checked)
		{
			pIDList = Util.GetHexList(ID_PID);
		}
		CalcIDParam param = new CalcIDParam(valueDec, valueDec2, CheckTID.Checked, CheckSID.Checked, ID_shiny.Checked, pIDList);
		ID_dataGridView.ClearSelection();
		IDTab.SearchWorker.isBusy = true;
		ID_cancel.Enabled = true;
		try
		{
			using (IDTab.SearchWorker.newSource())
			{
				List<IDBinder> data = await CalcID(IDTab.GetParam(ListMode: false), param);
				_idDGV.SetData(data);
				IDTab.SearchWorker.isBusy = false;
			}
		}
		catch
		{
		}
		ID_cancel.Enabled = false;
		ID_start.Text = "計算";
		ID_dataGridView.CurrentCell = null;
		ID_start.Enabled = true;
		ID_listup.Enabled = true;
	}

	private async void ID_list_Click(object sender, EventArgs e)
	{
		if (IDInitialseed1.Checked && !Regex.IsMatch(ID_Initialseed1.Text, "^[0-9a-fA-F]{0,8}$"))
		{
			ShowCaution("初期seedに不正な値が含まれています。");
			return;
		}
		if (IDInitialseed1.Checked && ID_Initialseed1.Text == "")
		{
			MessageBox.Show("初期seedが空白になっています。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		if (IDInitialseed2.Checked && (int)ID_RSmin.Value > (int)ID_RSmax.Value)
		{
			MessageBox.Show("分設定が 下限 ＞上限 になっています。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		ID_start.Enabled = false;
		ID_listup.Enabled = false;
		ID_dataGridView.ClearSelection();
		IDTab.SearchWorker.isBusy = true;
		ID_cancel.Enabled = true;
		try
		{
			using (IDTab.SearchWorker.newSource())
			{
				List<IDBinder> data = await CalcID(IDTab.GetParam(ListMode: true), new CalcIDParam());
				_idDGV.SetData(data);
				IDTab.SearchWorker.isBusy = false;
			}
		}
		catch
		{
		}
		ID_cancel.Enabled = false;
		ID_dataGridView.CurrentCell = null;
		ID_start.Enabled = true;
		ID_listup.Enabled = true;
	}

	private void ID_cancel_Click(object sender, EventArgs e)
	{
		if (IDTab.SearchWorker.isBusy)
		{
			IDTab.SearchWorker.Cancel();
		}
	}

	private void SetFrameButton_ID_Click(object sender, EventArgs e)
	{
		NumericUpDown firstFrame_ID = FirstFrame_ID;
		NumericUpDown lastFrame_ID = LastFrame_ID;
		(uint, uint) range = Util.GetRange(TargetFrame_ID, FrameRange_ID);
		decimal num = range.Item1;
		decimal num2 = range.Item2;
		decimal num4 = (firstFrame_ID.Value = num);
		num4 = (lastFrame_ID.Value = num2);
	}

	private bool ShowCaution(string message)
	{
		if (message != "")
		{
			MessageBox.Show(message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}
		return message != "";
	}

	private string CheckError_InitialSeed()
	{
		if (ForSimpleSeed_stationary.Checked && !Regex.IsMatch(k_Initialseed1.Text, "^[0-9a-fA-F]{1,8}$"))
		{
			return "初期seedに不正な値が含まれています。";
		}
		if (ForSimpleSeed_stationary.Checked && k_Initialseed1.Text == "")
		{
			return "初期seedが空白になっています。";
		}
		if ((FR_stationary.Checked || LG_stationary.Checked) && ForMultipleSeed_stationary.Checked && k_Initialseed3.Text == "")
		{
			return "初期seedが1つも入力されていません。";
		}
		if ((FR_stationary.Checked || LG_stationary.Checked) && ForMultipleSeed_stationary.Checked && !Regex.IsMatch(k_Initialseed3.Text, "^[0-9a-fA-F]{1,8}$", RegexOptions.Multiline))
		{
			return "初期seedに不正な値が含まれています。";
		}
		if (ForRTC_stationary.Checked && (int)k_RSmin.Value > (int)k_RSmax.Value)
		{
			return "分設定が 下限 ＞ 上限 になっています。";
		}
		return "";
	}

	private IList<uint> GetInitialSeedsStationary()
	{
		if (ForRTC_stationary.Checked)
		{
			return (from _ in Enumerable.Range((int)k_RSmin.Value, (int)(k_RSmax.Value - k_RSmin.Value + 1m))
				select new GBARealTimeClock((uint)_).Seed).ToArray();
		}
		if (ForMultipleSeed_stationary.Checked)
		{
			if (Ruby_stationary.Checked || Sapphire_stationary.Checked || Em_stationary.Checked)
			{
				int num = (int)PaintSeedMinFrameBox_stationary.Value;
				int count = (int)PaintSeedMaxFrameBox_stationary.Value - num + 1;
				return (from _ in Enumerable.Range(num, count)
					select (uint)_).ToArray();
			}
			return Util.GetHexList(k_Initialseed3);
		}
		return new uint[1] { k_Initialseed1.Seed() ?? throw new Exception() };
	}

	private ICriteria<Pokemon.Individual> GetCriteriaStationary()
	{
		uint tsv = TID_stationary.GetValueAsUint32() ^ SID_stationary.GetValueAsUint32();
		List<ICriteria<Pokemon.Individual>> list = new List<ICriteria<Pokemon.Individual>>();
		if (k_shiny.Checked)
		{
			list.Add(new ShinyCriteria(tsv, (ShinyType)3));
		}
		list.Add(new HiddenPowerPowerCriteria((uint)k_mezapaPower.Value));
		if (k_mezapaType.SelectedIndex != 0)
		{
			list.Add(new HiddenPowerTypeCriteria(k_mezapaType.Text.KanjiToPokeType()));
		}
		if (k_search1.Checked)
		{
			list.Add(new IVsCriteria(new uint[6]
			{
				Util.GetValueDec(k_IVlow1),
				Util.GetValueDec(k_IVlow2),
				Util.GetValueDec(k_IVlow3),
				Util.GetValueDec(k_IVlow4),
				Util.GetValueDec(k_IVlow5),
				Util.GetValueDec(k_IVlow6)
			}, new uint[6]
			{
				Util.GetValueDec(k_IVup1),
				Util.GetValueDec(k_IVup2),
				Util.GetValueDec(k_IVup3),
				Util.GetValueDec(k_IVup4),
				Util.GetValueDec(k_IVup5),
				Util.GetValueDec(k_IVup6)
			}));
		}
		else if (k_search2.Checked)
		{
			list.Add(new StatsCriteria(new uint[6]
			{
				Util.GetValueDec(k_stats1),
				Util.GetValueDec(k_stats2),
				Util.GetValueDec(k_stats3),
				Util.GetValueDec(k_stats4),
				Util.GetValueDec(k_stats5),
				Util.GetValueDec(k_stats6)
			}));
		}
		if (AbilityBox_stationary.SelectedIndex > 0)
		{
			list.Add(new AbilityCriteria(AbilityBox_stationary.Text));
		}
		if (GenderBox_stationary.SelectedIndex > 0)
		{
			list.Add(new GenderCriteria(GenderBox_stationary.Text.ConvertToGender()));
		}
		(Nature[], bool) natureList = NaturePanel_stationary.GetNatureList();
		var (array, _) = natureList;
		if (natureList.Item2)
		{
			list.Add(new NatureCriteria(array));
		}
		return Criteria.AND(list.ToArray());
	}

	private async void k_start_Click(object sender, EventArgs e)
	{
		if (ShowCaution(CheckError_InitialSeed()))
		{
			return;
		}
		k_dataGridView.ClearSelection();
		k_start.Enabled = false;
		k_listup.Enabled = false;
		uint tsv = TID_stationary.GetValueAsUint32() ^ SID_stationary.GetValueAsUint32();
		ICriteria<Pokemon.Individual> criteriaStationary = GetCriteriaStationary();
		IList<uint> initialSeedsStationary = GetInitialSeedsStationary();
		uint valueAsUint = FirstFrame_stationary.GetValueAsUint32();
		uint valueAsUint2 = LastFrame_stationary.GetValueAsUint32();
		uint valueAsUint3 = TargetFrame_stationary.GetValueAsUint32();
		IIVsGenerator iIVsGenerator;
		if (!RSFLRoamingCheck.Checked || !RSFLRoamingCheck.Visible)
		{
			IIVsGenerator selectedMethod_stationary = SelectedMethod_stationary;
			iIVsGenerator = selectedMethod_stationary;
		}
		else
		{
			iIVsGenerator = RoamingBuggyIVsGenerator.GetInstance();
		}
		IIVsGenerator method = iIVsGenerator;
		string legacyName = SelectedMethod_stationary.LegacyName;
		StationaryGenerator generator = SelectedRom_stationary.GetStationarySymbol(k_pokedex.SelectedIndex).CreateGenerator(method);
		StationaryTab.SearchWorker.isBusy = true;
		k_cancel.Enabled = true;
		try
		{
			using (StationaryTab.SearchWorker.newSource())
			{
				List<StationaryBinder> data = await CalcStationaryAsync(initialSeedsStationary, generator, valueAsUint, valueAsUint2, valueAsUint3, legacyName, tsv, criteriaStationary, StationaryTab.SearchWorker.GetToken());
				_stationaryDGV.SetData(data);
			}
		}
		catch
		{
		}
		StationaryTab.SearchWorker.isBusy = false;
		k_cancel.Enabled = false;
		k_start.Text = "計算";
		k_dataGridView.CurrentCell = null;
		k_start.Enabled = true;
		k_listup.Enabled = true;
	}

	private async void k_list_Click(object sender, EventArgs e)
	{
		if (ShowCaution(CheckError_InitialSeed()))
		{
			return;
		}
		k_dataGridView.ClearSelection();
		k_start.Enabled = false;
		k_listup.Enabled = false;
		uint valueAsUint = TargetFrame_stationary.GetValueAsUint32();
		uint valueAsUint2 = FrameRange_stationary.GetValueAsUint32();
		IList<uint> initialSeedsStationary = GetInitialSeedsStationary();
		uint min = ((valueAsUint >= valueAsUint2) ? (valueAsUint - valueAsUint2) : 0u);
		uint max = valueAsUint + valueAsUint2;
		IIVsGenerator iIVsGenerator;
		if (!RSFLRoamingCheck.Checked || !RSFLRoamingCheck.Visible)
		{
			IIVsGenerator selectedMethod_stationary = SelectedMethod_stationary;
			iIVsGenerator = selectedMethod_stationary;
		}
		else
		{
			iIVsGenerator = RoamingBuggyIVsGenerator.GetInstance();
		}
		IIVsGenerator method = iIVsGenerator;
		string legacyName = SelectedMethod_stationary.LegacyName;
		StationaryGenerator generator = SelectedRom_stationary.GetStationarySymbol(k_pokedex.SelectedIndex).CreateGenerator(method);
		uint tsv = Util.GetValueDec(TID_stationary) ^ Util.GetValueDec(SID_stationary);
		StationaryTab.SearchWorker.isBusy = true;
		k_cancel.Enabled = true;
		try
		{
			using (StationaryTab.SearchWorker.newSource())
			{
				List<StationaryBinder> data = await CalcStationaryAsync(initialSeedsStationary, generator, min, max, valueAsUint, legacyName, tsv, null, StationaryTab.SearchWorker.GetToken());
				_stationaryDGV.SetData(data);
			}
		}
		catch
		{
		}
		k_cancel.Enabled = false;
		StationaryTab.SearchWorker.isBusy = false;
		k_start.Text = "計算";
		k_dataGridView.CurrentCell = null;
		k_start.Enabled = true;
		k_listup.Enabled = true;
	}

	private void k_cancel_Click(object sender, EventArgs e)
	{
		if (StationaryTab.SearchWorker.isBusy)
		{
			StationaryTab.SearchWorker.Cancel();
		}
	}

	private Task<List<StationaryBinder>> CalcStationaryAsync(IEnumerable<uint> initSeeds, StationaryGenerator generator, uint min, uint max, uint target, string methodName, uint tsv, ICriteria<Pokemon.Individual> criteria, CancellationToken token)
	{
		int len = (int)(max - min + 1);
		return Task.Run(delegate
		{
			List<StationaryBinder> list = new List<StationaryBinder>();
			foreach (uint initSeed in initSeeds)
			{
				foreach (var (num, rNGResult) in CommonEnumerator.WithIndex<RNGResult<Pokemon.Individual, uint>>(initSeed.NextSeed(min).EnumerateSeed().EnumerateGeneration(generator)).Take(len))
				{
					if (token.IsCancellationRequested)
					{
						token.ThrowIfCancellationRequested();
					}
					if (rNGResult.Content != null && (criteria == null || criteria.CheckConditions(rNGResult.Content)))
					{
						StationaryBinder item = new StationaryBinder(initSeed, (uint)(num + min), target, methodName, rNGResult, tsv);
						list.Add(item);
					}
				}
			}
			return list;
		});
	}

	private void k_update_frame_Click(object sender, EventArgs e)
	{
		NumericUpDown firstFrame_stationary = FirstFrame_stationary;
		NumericUpDown lastFrame_stationary = LastFrame_stationary;
		(uint, uint) range = Util.GetRange(TargetFrame_stationary, FrameRange_stationary);
		decimal num = range.Item1;
		decimal num2 = range.Item2;
		decimal num4 = (firstFrame_stationary.Value = num);
		num4 = (lastFrame_stationary.Value = num2);
	}

	private void k_search1_CheckedChanged(object sender, EventArgs e)
	{
		NumericUpDown numericUpDown = k_IVlow1;
		NumericUpDown numericUpDown2 = k_IVlow2;
		NumericUpDown numericUpDown3 = k_IVlow3;
		NumericUpDown numericUpDown4 = k_IVlow4;
		NumericUpDown numericUpDown5 = k_IVlow5;
		bool flag = (k_IVlow6.Visible = k_search1.Checked);
		bool flag3 = (numericUpDown5.Visible = flag);
		bool flag5 = (numericUpDown4.Visible = flag3);
		bool flag7 = (numericUpDown3.Visible = flag5);
		bool visible = (numericUpDown2.Visible = flag7);
		numericUpDown.Visible = visible;
		NumericUpDown numericUpDown6 = k_IVup1;
		NumericUpDown numericUpDown7 = k_IVup2;
		NumericUpDown numericUpDown8 = k_IVup3;
		NumericUpDown numericUpDown9 = k_IVup4;
		NumericUpDown numericUpDown10 = k_IVup5;
		flag = (k_IVup6.Visible = k_search1.Checked);
		flag3 = (numericUpDown10.Visible = flag);
		flag5 = (numericUpDown9.Visible = flag3);
		flag7 = (numericUpDown8.Visible = flag5);
		visible = (numericUpDown7.Visible = flag7);
		numericUpDown6.Visible = visible;
		NumericUpDown numericUpDown11 = k_stats1;
		NumericUpDown numericUpDown12 = k_stats2;
		NumericUpDown numericUpDown13 = k_stats3;
		NumericUpDown numericUpDown14 = k_stats4;
		NumericUpDown numericUpDown15 = k_stats5;
		flag = (k_stats6.Visible = !k_search1.Checked);
		flag3 = (numericUpDown15.Visible = flag);
		flag5 = (numericUpDown14.Visible = flag3);
		flag7 = (numericUpDown13.Visible = flag5);
		visible = (numericUpDown12.Visible = flag7);
		numericUpDown11.Visible = visible;
	}

	private void NatureButton_stationary_Click(object sender, EventArgs e)
	{
		NaturePanel naturePanel_stationary = NaturePanel_stationary;
		naturePanel_stationary.Visible = !naturePanel_stationary.Visible;
	}

	private void UpdateStatBox_stationary()
	{
		uint[] ivs = ((SelectedRom_stationary == Rom.Em || !SelectedRom_stationary.GetStationarySymbolLabelList()[k_pokedex.SelectedIndex].Contains("徘徊")) ? new uint[6] { 31u, 31u, 31u, 31u, 31u, 31u } : new uint[6] { 31u, 7u, 0u, 0u, 0u, 0u });
		IReadOnlyList<uint> stats = SelectedRom_stationary.GetStationarySymbolList()[k_pokedex.SelectedIndex].Pokemon.GetIndividual(Util.GetValueDec(k_Lv), ivs, 0u).Stats;
		k_stats1.Value = stats[0];
		k_stats2.Value = stats[1];
		k_stats3.Value = stats[2];
		k_stats4.Value = stats[3];
		k_stats5.Value = stats[4];
		k_stats6.Value = stats[5];
	}

	private void K_pokedex_SelectedIndexChanged(object sender, EventArgs e)
	{
		GBASlot gBASlot = SelectedRom_stationary.GetStationarySymbolList()[k_pokedex.SelectedIndex];
		k_Lv.Value = gBASlot.BasicLv;
		UpdateAbility(AbilityBox_stationary, gBASlot.Pokemon);
		UpdateSex(GenderBox_stationary, gBASlot.Pokemon);
		UpdateStatBox_stationary();
		CheckBox rSFLRoamingCheck = RSFLRoamingCheck;
		bool visible = (RSFLRoamingCheck.Checked = SelectedRom_stationary != Rom.Em && SelectedRom_stationary.GetStationarySymbolLabelList()[k_pokedex.SelectedIndex].Contains("徘徊"));
		rSFLRoamingCheck.Visible = visible;
	}

	private void RS_stationary_Click(object sender, EventArgs e)
	{
		if (SelectedRom_stationary != Rom.Ruby)
		{
			SelectedRom_stationary = Rom.Ruby;
			PaintPanel_stationary.Visible = true;
			UpdateSymbolList();
		}
	}

	private void S_stationary_Click(object sender, EventArgs e)
	{
		if (SelectedRom_stationary != Rom.Sapphire)
		{
			SelectedRom_stationary = Rom.Sapphire;
			PaintPanel_stationary.Visible = true;
			UpdateSymbolList();
		}
	}

	private void Em_stationary_Click(object sender, EventArgs e)
	{
		if (SelectedRom_stationary != Rom.Em)
		{
			SelectedRom_stationary = Rom.Em;
			PaintPanel_stationary.Visible = true;
			UpdateSymbolList();
		}
	}

	private void FRLG_stationary_Click(object sender, EventArgs e)
	{
		if (SelectedRom_stationary != Rom.FireRed)
		{
			SelectedRom_stationary = Rom.FireRed;
			PaintPanel_stationary.Visible = false;
			UpdateSymbolList();
		}
	}

	private void LG_stationary_Click(object sender, EventArgs e)
	{
		if (SelectedRom_stationary != Rom.LeafGreen)
		{
			SelectedRom_stationary = Rom.LeafGreen;
			PaintPanel_stationary.Visible = false;
			UpdateSymbolList();
		}
	}

	private void UpdateSymbolList()
	{
		IEnumerable<string> source = from _ in SelectedRom_stationary.GetStationarySymbolList()
			select _.Pokemon.GetDefaultName();
		k_pokedex.Items.Clear();
		ComboBox.ObjectCollection items = k_pokedex.Items;
		object[] items2 = source.ToArray();
		items.AddRange(items2);
		k_pokedex.SelectedIndex = 0;
	}

	private WildGenerationArgument GetEncounterOption()
	{
		return new WildGenerationArgument
		{
			RidingBicycle = RidingBicycle_wild.Checked,
			HasCleanseTag = OFUDA_wild.Checked,
			UsingFlute = ((!BlackFlute_wild.Checked) ? (WhiteFlute_wild.Checked ? Flute.WhiteFlute : Flute.Other) : Flute.BlackFlute),
			FieldAbility = SelectedAbility,
			GenerateMethod = SelectedMethod_wild,
			PokeBlock = SelectedPokeBlock,
			ForceEncounter = !CheckAppearing_wild.Checked
		};
	}

	private IList<uint> GetInitialSeedsWild()
	{
		if (ForRTC_wild.Checked)
		{
			return (from _ in Enumerable.Range((int)y_RSmin.Value, (int)(y_RSmax.Value - y_RSmin.Value + 1m))
				select new GBARealTimeClock((uint)_).Seed).ToArray();
		}
		if (ForMultipleSeed_wild.Checked)
		{
			if (ROM_R.Checked || ROM_S.Checked || ROM_Em.Checked)
			{
				int num = (int)PaintSeedMinFrameBox_wild.Value;
				int count = (int)PaintSeedMaxFrameBox_wild.Value - num + 1;
				return (from _ in Enumerable.Range(num, count)
					select (uint)_).ToArray();
			}
			return Util.GetHexList(y_Initialseed3);
		}
		return new uint[1] { y_Initialseed1.Seed() ?? throw new Exception() };
	}

	private async void y_start_Click(object sender, EventArgs e)
	{
		if (ShowCaution(y_CheckingError()))
		{
			return;
		}
		y_dataGridView.ClearSelection();
		y_start.Enabled = false;
		y_listup.Enabled = false;
		WildGenerator generator = new WildGenerator(SelectedMap, GetEncounterOption());
		WildCalcParam wildCalcParam = new WildCalcParam();
		if (wildCalcParam.ForIVs = y_search1.Checked)
		{
			wildCalcParam.LowestIVs = new uint[6]
			{
				Util.GetValueDec(y_IVlow1),
				Util.GetValueDec(y_IVlow2),
				Util.GetValueDec(y_IVlow3),
				Util.GetValueDec(y_IVlow4),
				Util.GetValueDec(y_IVlow5),
				Util.GetValueDec(y_IVlow6)
			};
			wildCalcParam.HighestIVs = new uint[6]
			{
				Util.GetValueDec(y_IVup1),
				Util.GetValueDec(y_IVup2),
				Util.GetValueDec(y_IVup3),
				Util.GetValueDec(y_IVup4),
				Util.GetValueDec(y_IVup5),
				Util.GetValueDec(y_IVup6)
			};
		}
		if (wildCalcParam.ForStats = y_search2.Checked)
		{
			wildCalcParam.TargetStats = new uint[6]
			{
				Util.GetValueDec(y_stats1),
				Util.GetValueDec(y_stats2),
				Util.GetValueDec(y_stats3),
				Util.GetValueDec(y_stats4),
				Util.GetValueDec(y_stats5),
				Util.GetValueDec(y_stats6)
			};
		}
		wildCalcParam.OnlyShiny = y_shiny.Checked;
		wildCalcParam.TSV = Util.GetValueDec(TID_wild) ^ Util.GetValueDec(SID_wild);
		wildCalcParam.LowestHiddenPower = (uint)y_mezapaPower.Value;
		if (wildCalcParam.CheckHP = y_mezapaType.SelectedIndex != 0)
		{
			wildCalcParam.TargetHiddenPowerType = y_mezapaType.Text.KanjiToPokeType();
		}
		(wildCalcParam.TargetNatureList, wildCalcParam.CheckNature) = NaturePanel_wild.GetNatureList();
		if (wildCalcParam.CheckPokemon = y_pokedex.SelectedIndex != 0)
		{
			wildCalcParam.TargetPokemon = y_pokedex.Text;
		}
		if (wildCalcParam.CheckLv = y_check_LvEnable.Checked)
		{
			wildCalcParam.TargetLv = Util.GetValueDec(y_Lv);
		}
		if (wildCalcParam.CheckAbility = y_ability.SelectedIndex != 0)
		{
			wildCalcParam.TargetAbility = y_ability.Text;
		}
		if (wildCalcParam.CheckGender = y_sex.SelectedIndex != 0)
		{
			wildCalcParam.TargetGender = y_sex.Text;
		}
		uint valueAsUint = FirstFrame_wild.GetValueAsUint32();
		uint valueAsUint2 = LastFrame_wild.GetValueAsUint32();
		uint valueAsUint3 = TargetFrame_wild.GetValueAsUint32();
		IList<uint> initialSeedsWild = GetInitialSeedsWild();
		_wildTab.SearchWorker.isBusy = true;
		y_cancel.Enabled = true;
		try
		{
			using (_wildTab.SearchWorker.newSource())
			{
				List<WildBinder> data = await CalcWildAsync(generator, valueAsUint, valueAsUint2, valueAsUint3, initialSeedsWild, wildCalcParam, _wildTab.SearchWorker.GetToken());
				_wildDGV.SetData(data);
			}
		}
		catch
		{
		}
		_wildTab.SearchWorker.isBusy = false;
		y_cancel.Enabled = false;
		y_start.Text = "計算";
		y_dataGridView.CurrentCell = null;
		y_start.Enabled = true;
		y_listup.Enabled = true;
	}

	private async void y_list_Click(object sender, EventArgs e)
	{
		if (ShowCaution(y_CheckingError()))
		{
			return;
		}
		y_dataGridView.ClearSelection();
		y_start.Enabled = false;
		y_listup.Enabled = false;
		WildGenerator generator = new WildGenerator(SelectedMap, GetEncounterOption());
		WildCalcParam param = new WildCalcParam
		{
			ListMode = true,
			TSV = (Util.GetValueDec(TID_wild) ^ Util.GetValueDec(SID_wild))
		};
		uint valueAsUint = TargetFrame_wild.GetValueAsUint32();
		uint valueAsUint2 = FrameRange_wild.GetValueAsUint32();
		uint min = ((valueAsUint >= valueAsUint2) ? (valueAsUint - valueAsUint2) : 0u);
		uint max = valueAsUint + valueAsUint2;
		IEnumerable<uint> initialSeeds = new uint[0];
		if (ForSimpleSeed_wild.Checked)
		{
			initialSeeds = new uint[1] { y_Initialseed1.Seed() ?? throw new Exception() };
		}
		if (ForRTC_wild.Checked)
		{
			initialSeeds = (from _ in Enumerable.Range((int)y_RSmin.Value, (int)(y_RSmax.Value - y_RSmin.Value + 1m))
				select new GBARealTimeClock((uint)_).Seed).ToArray();
		}
		if (ForMultipleSeed_wild.Checked)
		{
			if (ROM_R.Checked || ROM_S.Checked || ROM_Em.Checked)
			{
				int num = (int)PaintSeedMinFrameBox_wild.Value;
				int count = (int)PaintSeedMaxFrameBox_wild.Value - num + 1;
				initialSeeds = (from _ in Enumerable.Range(num, count)
					select (uint)_).ToArray();
			}
			else
			{
				initialSeeds = Util.GetHexList(y_Initialseed3);
			}
		}
		_wildTab.SearchWorker.isBusy = true;
		y_cancel.Enabled = true;
		try
		{
			using (_wildTab.SearchWorker.newSource())
			{
				List<WildBinder> data = await CalcWildAsync(generator, min, max, valueAsUint, initialSeeds, param, _wildTab.SearchWorker.GetToken());
				_wildDGV.SetData(data);
			}
		}
		catch
		{
		}
		_wildTab.SearchWorker.isBusy = false;
		y_cancel.Enabled = false;
		y_dataGridView.CurrentCell = null;
		y_start.Enabled = true;
		y_listup.Enabled = true;
	}

	private string y_CheckingError()
	{
		if (ForSimpleSeed_wild.Checked && !Regex.IsMatch(y_Initialseed1.Text, "^[0-9a-fA-F]{0,8}$"))
		{
			return "初期seedに不正な値が含まれています。";
		}
		if (ForSimpleSeed_wild.Checked && y_Initialseed1.Text == "")
		{
			return "初期seedが空白になっています。";
		}
		if ((ROM_FR.Checked || ROM_LG.Checked) && ForMultipleSeed_wild.Checked && y_Initialseed3.Text == "")
		{
			return "初期seedが1つも入力されていません。";
		}
		if ((ROM_FR.Checked || ROM_LG.Checked) && ForMultipleSeed_wild.Checked && !Regex.IsMatch(y_Initialseed3.Text, "^[0-9a-fA-F]{0,8}$", RegexOptions.Multiline))
		{
			return "初期seedに不正な値が含まれています。";
		}
		if (ForRTC_wild.Checked && Convert.ToUInt32(y_RSmin.Text) > Convert.ToUInt32(y_RSmax.Text))
		{
			return "分設定が 下限 ＞上限 になっています。";
		}
		if (Util.GetValueDec(FirstFrame_wild) > Util.GetValueDec(LastFrame_wild))
		{
			return "Fが 下限 ＞上限 になっています。";
		}
		return "";
	}

	private void y_cancel_Click(object sender, EventArgs e)
	{
		if (_wildTab.SearchWorker.isBusy)
		{
			_wildTab.SearchWorker.Cancel();
		}
	}

	private Task<List<WildBinder>> CalcWildAsync(WildGenerator generator, uint min, uint max, uint target, IEnumerable<uint> initialSeeds, WildCalcParam param2, CancellationToken token)
	{
		int len = (int)(max - min + 1);
		return Task.Run(delegate
		{
			List<WildBinder> list = new List<WildBinder>();
			foreach (uint initialSeed in initialSeeds)
			{
				foreach (var (num, rNGResult) in CommonEnumerator.WithIndex<RNGResult<Pokemon.Individual, uint>>(initialSeed.NextSeed(min).EnumerateSeed().EnumerateGeneration(generator)).Take(len))
				{
					if (token.IsCancellationRequested)
					{
						token.ThrowIfCancellationRequested();
					}
					if (rNGResult.Content != null && param2.Check(rNGResult))
					{
						WildBinder item = new WildBinder(initialSeed, (uint)(num + min), target, rNGResult, param2.TSV);
						list.Add(item);
					}
				}
			}
			return list;
		}, token);
	}

	private void SetFrameButton_wild_Click(object sender, EventArgs e)
	{
		NumericUpDown firstFrame_wild = FirstFrame_wild;
		NumericUpDown lastFrame_wild = LastFrame_wild;
		(uint, uint) range = Util.GetRange(TargetFrame_wild, FrameRange_wild);
		decimal num = range.Item1;
		decimal num2 = range.Item2;
		decimal num4 = (firstFrame_wild.Value = num);
		num4 = (lastFrame_wild.Value = num2);
	}

	private void y_search1_CheckedChanged(object sender, EventArgs e)
	{
		NumericUpDown numericUpDown = y_IVlow1;
		NumericUpDown numericUpDown2 = y_IVlow2;
		NumericUpDown numericUpDown3 = y_IVlow3;
		NumericUpDown numericUpDown4 = y_IVlow4;
		NumericUpDown numericUpDown5 = y_IVlow5;
		bool flag = (y_IVlow6.Visible = y_search1.Checked);
		bool flag3 = (numericUpDown5.Visible = flag);
		bool flag5 = (numericUpDown4.Visible = flag3);
		bool flag7 = (numericUpDown3.Visible = flag5);
		bool visible = (numericUpDown2.Visible = flag7);
		numericUpDown.Visible = visible;
		NumericUpDown numericUpDown6 = y_IVup1;
		NumericUpDown numericUpDown7 = y_IVup2;
		NumericUpDown numericUpDown8 = y_IVup3;
		NumericUpDown numericUpDown9 = y_IVup4;
		NumericUpDown numericUpDown10 = y_IVup5;
		flag = (y_IVup6.Visible = y_search1.Checked);
		flag3 = (numericUpDown10.Visible = flag);
		flag5 = (numericUpDown9.Visible = flag3);
		flag7 = (numericUpDown8.Visible = flag5);
		visible = (numericUpDown7.Visible = flag7);
		numericUpDown6.Visible = visible;
		NumericUpDown numericUpDown11 = y_stats1;
		NumericUpDown numericUpDown12 = y_stats2;
		NumericUpDown numericUpDown13 = y_stats3;
		NumericUpDown numericUpDown14 = y_stats4;
		NumericUpDown numericUpDown15 = y_stats5;
		flag = (y_stats6.Visible = !y_search1.Checked);
		flag3 = (numericUpDown15.Visible = flag);
		flag5 = (numericUpDown14.Visible = flag3);
		flag7 = (numericUpDown13.Visible = flag5);
		visible = (numericUpDown12.Visible = flag7);
		numericUpDown11.Visible = visible;
		y_Lv.Enabled = y_check_LvEnable.Checked;
	}

	private void y_table_display_Click(object sender, EventArgs e)
	{
		if (y_table_display.Text == "出現リストを表示する")
		{
			y_table_display.Text = "出現リストを隠す";
			y_dataGridView.Height = 200;
		}
		else
		{
			y_table_display.Text = "出現リストを表示する";
			y_dataGridView.Height = 334;
		}
	}

	private void SelectedFieldAbilityChanged(object sender, EventArgs e)
	{
		SyncNature_wild.Visible = FieldAbility_wild.Text == "シンクロ";
		CCGender_wild.Visible = FieldAbility_wild.Text == "メロボ";
	}

	private void y_check_LvEnable_Click(object sender, EventArgs e)
	{
		y_Lv.Enabled = y_check_LvEnable.Checked;
	}

	private void NatureButton_wild_Click(object sender, EventArgs e)
	{
		NaturePanel naturePanel_wild = NaturePanel_wild;
		naturePanel_wild.Visible = !naturePanel_wild.Visible;
	}

	private void SelectedMapChanged(object sender, EventArgs e)
	{
		Label label = label_pb;
		bool visible = (PokeBlockBox.Visible = SelectedRom_wild != Rom.FireRed && SelectedRom_wild != Rom.LeafGreen && SelectedMap.MapName.Contains("サファリ") && SelectedEncounterType != EncounterType.RockSmash);
		label.Visible = visible;
		dataGridView_table.UpdateTable(SelectedMap, SelectedEncounterType);
		UpdatePokelist();
	}

	private void y_pokedex_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (y_pokedex.Text == "指定なし")
		{
			y_sex.Items.Clear();
			ComboBox.ObjectCollection items = y_sex.Items;
			object[] items2 = new string[4] { "指定なし", "♂", "♀", "-" };
			items.AddRange(items2);
			y_sex.SelectedIndex = 0;
			y_ability.Items.Clear();
			y_ability.Items.Add("指定なし");
			y_ability.SelectedIndex = 0;
		}
		else
		{
			string text = ((ComboBox)sender).Text;
			string name = text.Substring(0, Math.Min(5, text.Length));
			Pokemon.Species poke = Pokemon.GetPokemon(name);
			UpdateSex(y_sex, poke);
			UpdateAbility(y_ability, poke);
			y_Lv.Value = ((y_pokedex.Text == "ヒンバス") ? 20u : SelectedMap.EncounterTable.Where((GBASlot _) => _.Pokemon.Name == poke.Name).First().BasicLv);
			UpdateStatBox_wild();
		}
	}

	private void UpdateStatBox_wild()
	{
		uint[] ivs = new uint[6] { 31u, 31u, 31u, 31u, 31u, 31u };
		IReadOnlyList<uint> stats = Pokemon.GetPokemon(k_pokedex.Text).GetIndividual(Util.GetValueDec(y_Lv), ivs, 0u).Stats;
		y_stats1.Value = stats[0];
		y_stats2.Value = stats[1];
		y_stats3.Value = stats[2];
		y_stats4.Value = stats[3];
		y_stats5.Value = stats[4];
		y_stats6.Value = stats[5];
	}

	private void CheckAppearing_wild_CheckedChanged(object sender, EventArgs e)
	{
		CheckBox ridingBicycle_wild = RidingBicycle_wild;
		CheckBox oFUDA_wild = OFUDA_wild;
		CheckBox blackFlute_wild = BlackFlute_wild;
		bool flag = (WhiteFlute_wild.Enabled = CheckAppearing_wild.Checked);
		bool flag3 = (blackFlute_wild.Enabled = flag);
		bool enabled = (oFUDA_wild.Enabled = flag3);
		ridingBicycle_wild.Enabled = enabled;
	}

	private void BlackFlute_wild_Click(object sender, EventArgs e)
	{
		WhiteFlute_wild.Checked &= !BlackFlute_wild.Checked;
	}

	private void WhiteFlute_wild_Click(object sender, EventArgs e)
	{
		BlackFlute_wild.Checked &= !WhiteFlute_wild.Checked;
	}

	private void UpdateMapBox()
	{
		MapBox.Items.Clear();
		dataGridView_table.Rows.Clear();
		ComboBox.ObjectCollection items = MapBox.Items;
		object[] items2 = SelectedRom_wild.GetMapNameList(SelectedEncounterType).ToArray();
		items.AddRange(items2);
		MapBox.SelectedIndex = 0;
	}

	private void UpdatePokelist()
	{
		y_pokedex.Items.Clear();
		y_pokedex.Items.Add("指定なし");
		string[] array = SelectedMap.EncounterTable.Select((GBASlot _) => _.Pokemon.GetDefaultName()).Distinct().ToArray();
		if (MapBox.Text.Contains("ヒンバス"))
		{
			y_pokedex.Items.Add("ヒンバス");
		}
		ComboBox.ObjectCollection items = y_pokedex.Items;
		object[] items2 = array;
		items.AddRange(items2);
		y_pokedex.SelectedIndex = 0;
	}

	private void UpdateSex(ComboBox ComboBox, Pokemon.Species pokemon)
	{
		if (pokemon != null)
		{
			ComboBox.Items.Clear();
			ComboBox.Items.Add("指定なし");
			switch (pokemon.GenderRatio)
			{
			case GenderRatio.Genderless:
				ComboBox.Items.Add("-");
				break;
			case GenderRatio.MaleOnly:
				ComboBox.Items.Add("♂");
				break;
			case GenderRatio.FemaleOnly:
				ComboBox.Items.Add("♀");
				break;
			default:
			{
				ComboBox.ObjectCollection items = ComboBox.Items;
				object[] items2 = new string[2] { "♂", "♀" };
				items.AddRange(items2);
				break;
			}
			}
			ComboBox.SelectedIndex = 0;
		}
	}

	private void UpdateAbility(ComboBox ComboBox, Pokemon.Species pokemon)
	{
		ComboBox.Items.Clear();
		ComboBox.Items.Add("指定なし");
		ComboBox.ObjectCollection items = ComboBox.Items;
		object[] items2 = pokemon.Ability.Distinct().ToArray();
		items.AddRange(items2);
		ComboBox.SelectedIndex = 0;
	}

	private void ROM_R_Click(object sender, EventArgs e)
	{
		SelectedRom_wild = Rom.Ruby;
		ComboBox syncNature_wild = SyncNature_wild;
		bool enabled = (CCGender_wild.Enabled = false);
		syncNature_wild.Enabled = enabled;
		PaintPanel_wild.Visible = true;
		FieldAbility_wild.Items.Clear();
		ComboBox.ObjectCollection items = FieldAbility_wild.Items;
		object[] items2 = new string[3] { "---", "はっこう", "あくしゅう" };
		items.AddRange(items2);
		FieldAbility_wild.SelectedIndex = 0;
		UpdateMapBox();
	}

	private void ROM_S_Click(object sender, EventArgs e)
	{
		SelectedRom_wild = Rom.Sapphire;
		ComboBox syncNature_wild = SyncNature_wild;
		bool enabled = (CCGender_wild.Enabled = false);
		syncNature_wild.Enabled = enabled;
		PaintPanel_wild.Visible = true;
		FieldAbility_wild.Items.Clear();
		ComboBox.ObjectCollection items = FieldAbility_wild.Items;
		object[] items2 = new string[3] { "---", "はっこう", "あくしゅう" };
		items.AddRange(items2);
		FieldAbility_wild.SelectedIndex = 0;
		UpdateMapBox();
	}

	private void ROM_Em_Click(object sender, EventArgs e)
	{
		SelectedRom_wild = Rom.Em;
		ComboBox syncNature_wild = SyncNature_wild;
		bool enabled = (CCGender_wild.Enabled = true);
		syncNature_wild.Enabled = enabled;
		PaintPanel_wild.Visible = true;
		FieldAbility_wild.Items.Clear();
		ComboBox.ObjectCollection items = FieldAbility_wild.Items;
		object[] items2 = new string[8] { "---", "シンクロ", "メロボ", "プレッシャー", "じりょく", "せいでんき", "はっこう", "あくしゅう" };
		items.AddRange(items2);
		FieldAbility_wild.SelectedIndex = 0;
		UpdateMapBox();
	}

	private void ROM_FR_Click(object sender, EventArgs e)
	{
		SelectedRom_wild = Rom.FireRed;
		ComboBox syncNature_wild = SyncNature_wild;
		bool enabled = (CCGender_wild.Enabled = false);
		syncNature_wild.Enabled = enabled;
		PaintPanel_wild.Visible = false;
		FieldAbility_wild.Items.Clear();
		ComboBox.ObjectCollection items = FieldAbility_wild.Items;
		object[] items2 = new string[3] { "---", "はっこう", "あくしゅう" };
		items.AddRange(items2);
		FieldAbility_wild.SelectedIndex = 0;
		UpdateMapBox();
	}

	private void ROM_LG_Click(object sender, EventArgs e)
	{
		SelectedRom_wild = Rom.LeafGreen;
		ComboBox syncNature_wild = SyncNature_wild;
		bool enabled = (CCGender_wild.Enabled = false);
		syncNature_wild.Enabled = enabled;
		PaintPanel_wild.Visible = false;
		FieldAbility_wild.Items.Clear();
		ComboBox.ObjectCollection items = FieldAbility_wild.Items;
		object[] items2 = new string[3] { "---", "はっこう", "あくしゅう" };
		items.AddRange(items2);
		FieldAbility_wild.SelectedIndex = 0;
		UpdateMapBox();
	}

	private void Encounter_Grass_Click(object sender, EventArgs e)
	{
		SelectedEncounterType = EncounterType.Grass;
		UpdateMapBox();
		Label label = label_pb;
		bool visible = (PokeBlockBox.Visible = SelectedMap.MapName.Contains("サファリ") && SelectedRom_wild != Rom.FireRed && SelectedRom_wild != Rom.LeafGreen);
		label.Visible = visible;
	}

	private void Encounter_Surf_Click(object sender, EventArgs e)
	{
		SelectedEncounterType = EncounterType.Surf;
		UpdateMapBox();
		Label label = label_pb;
		bool visible = (PokeBlockBox.Visible = SelectedMap.MapName.Contains("サファリ") && SelectedRom_wild != Rom.FireRed && SelectedRom_wild != Rom.LeafGreen);
		label.Visible = visible;
	}

	private void Encounter_RockSmash_Click(object sender, EventArgs e)
	{
		SelectedEncounterType = EncounterType.RockSmash;
		UpdateMapBox();
		Label label = label_pb;
		bool visible = (PokeBlockBox.Visible = false);
		label.Visible = visible;
	}

	private void Encounter_OldRod_Click(object sender, EventArgs e)
	{
		SelectedEncounterType = EncounterType.OldRod;
		UpdateMapBox();
		Label label = label_pb;
		bool visible = (PokeBlockBox.Visible = SelectedMap.MapName.Contains("サファリ") && SelectedRom_wild != Rom.FireRed && SelectedRom_wild != Rom.LeafGreen);
		label.Visible = visible;
	}

	private void Encounter_GoodRod_Click(object sender, EventArgs e)
	{
		SelectedEncounterType = EncounterType.GoodRod;
		UpdateMapBox();
		Label label = label_pb;
		bool visible = (PokeBlockBox.Visible = SelectedMap.MapName.Contains("サファリ") && SelectedRom_wild != Rom.FireRed && SelectedRom_wild != Rom.LeafGreen);
		label.Visible = visible;
	}

	private void Encounter_SuperRod_Click(object sender, EventArgs e)
	{
		SelectedEncounterType = EncounterType.SuperRod;
		UpdateMapBox();
		Label label = label_pb;
		bool visible = (PokeBlockBox.Visible = SelectedMap.MapName.Contains("サファリ") && SelectedRom_wild != Rom.FireRed && SelectedRom_wild != Rom.LeafGreen);
		label.Visible = visible;
	}
}
