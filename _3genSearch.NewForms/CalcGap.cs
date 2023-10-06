using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Pokemon3genRNGLibrary;
using PokemonPRNG.LCG32;
using PokemonPRNG.LCG32.StandardLCG;
using PokemonStandardLibrary;
using PokemonStandardLibrary.CommonExtension;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch.NewForms;

public class CalcGap : Form
{
	private class Binder
	{
		private readonly RNGResult<Pokemon.Individual, uint> _result;

		[DataGridViewRowHeader(80, "初期seed", false, null)]
		public string InitialSeed { get; }

		[DataGridViewRowHeader(66, "F", true, null)]
		public int Frame { get; }

		[DataGridViewRowHeader(66, "seed", false, null)]
		public string Seed => $"{_result.HeadSeed:X8}";

		[DataGridViewRowHeader(67, "ポケモン", true, null)]
		public string Name => $"{_result.Content.Name:X8}";

		[DataGridViewRowHeader(66, "性格値", false, null)]
		public string PID => $"{_result.Content.PID:X8}";

		[DataGridViewRowHeader(66, "性格", false, null)]
		public string Nature => _result.Content.Nature.ToJapanese();

		[DataGridViewRowHeader(27, "H", false, null)]
		public uint IVs_H => _result.Content.IVs[0];

		[DataGridViewRowHeader(27, "A", false, null)]
		public uint IVs_A => _result.Content.IVs[1];

		[DataGridViewRowHeader(27, "B", false, null)]
		public uint IVs_B => _result.Content.IVs[2];

		[DataGridViewRowHeader(27, "C", false, null)]
		public uint IVs_C => _result.Content.IVs[3];

		[DataGridViewRowHeader(27, "D", false, null)]
		public uint IVs_D => _result.Content.IVs[4];

		[DataGridViewRowHeader(27, "S", false, null)]
		public uint IVs_S => _result.Content.IVs[5];

		[DataGridViewRowHeader(80, "Method", false, null)]
		public string Method { get; }

		[DataGridViewRowHeader(66, "性別", false, "ＭＳ ゴシック")]
		public string Gender => _result.Content.Gender.ToSymbol();

		[DataGridViewRowHeader(100, "特性", true, null)]
		public string Ability => _result.Content.Ability;

		[DataGridViewRowHeader(60, "めざパ", false, null)]
		public string HiddenPower => $"{_result.Content.HiddenPowerType.ToKanji()}{_result.Content.HiddenPower}";

		public Binder(uint initSeed, int frame, string method, RNGResult<Pokemon.Individual, uint> result)
		{
			string text = $"{initSeed:X}";
			InitialSeed = text;
			_result = result;
			Frame = frame;
			Method = method;
		}
	}

	private readonly DataGridViewWrapper<Binder> wrap;

	private IContainer components;

	private DataGridView dataGridView1;

	private NaturePanel NaturePanel;

	private GroupBox k_groupBox2;

	private ComboBox AbilityBox;

	private ComboBox GenderBox;

	private NumericUpDown SID_stationary;

	private NumericUpDown TID_stationary;

	private CheckBox k_shiny;

	private Label label6;

	private Label label27;

	private Label label29;

	private NumericUpDown HPPowerBox;

	private Label label31;

	private ComboBox PokemonBox;

	private ComboBox HPTypeBox;

	private GroupBox k_groupBox1;

	private NumericUpDown MaxFrameBox;

	private NumericUpDown MinFrameBox;

	private TextBox SeedBox;

	private Label label1;

	private Label label40;

	private Label label39;

	private Button CalcButton;

	private Panel panel2;

	private Label label7;

	private NumericUpDown Hmax;

	private Label label8;

	private NumericUpDown Cmax;

	private NumericUpDown Smin;

	private NumericUpDown Dmin;

	private Label label11;

	private Label label12;

	private NumericUpDown Cmin;

	private Label label13;

	private NumericUpDown Dmax;

	private NumericUpDown Bmin;

	private NumericUpDown Amin;

	private Label label14;

	private Label label15;

	private Label label16;

	private NumericUpDown Hmin;

	private NumericUpDown Smax;

	private Label label17;

	private NumericUpDown Amax;

	private Label label18;

	private NumericUpDown Bmax;

	private Label label19;

	private Label label20;

	private CheckBox CheckMethod2;

	private CheckBox CheckMethod4;

	private CheckBox CheckMethod1;

	private CheckBox CheckGender;

	private CheckBox CheckAbility;

	public CalcGap()
	{
		InitializeComponent();
		wrap = new DataGridViewWrapper<Binder>(dataGridView1);
		PokeType[] array = new PokeType[15];
		for (int i = 1; i < 16; i++)
		{
			array[i - 1] = (PokeType)(1 << i);
		}
		string[] array2 = array.Select((PokeType _) => _.ToJapanese()).ToArray();
		Array.Sort(array2);
		HPTypeBox.Items.Add("---");
		ComboBox.ObjectCollection items = HPTypeBox.Items;
		object[] items2 = array2;
		items.AddRange(items2);
		HPTypeBox.SelectedIndex = 0;
	}

	private IEnumerable<(IIVsGenerator Generator, string Label)> GetSelectedMethods()
	{
		if (CheckMethod1.Checked)
		{
			yield return (StandardIVsGenerator.GetInstance(), "Method1");
		}
		if (CheckMethod4.Checked)
		{
			yield return (MiddleInterruptedIVsGenerator.GetInstance(), "Method4");
		}
		if (CheckMethod2.Checked)
		{
			yield return (PriorInterruptIVsGenerator.GetInstance(), "Method2");
		}
	}

	private bool TryGetSeed(out uint seed)
	{
		try
		{
			seed = Convert.ToUInt32(SeedBox.Text, 16);
			return true;
		}
		catch
		{
			seed = 0u;
			return false;
		}
	}

	private (uint[] Min, uint[] Max) GetIVs()
	{
		return (new uint[6]
		{
			Hmin.GetValueAsUint32(),
			Amin.GetValueAsUint32(),
			Bmin.GetValueAsUint32(),
			Cmin.GetValueAsUint32(),
			Dmin.GetValueAsUint32(),
			Smin.GetValueAsUint32()
		}, new uint[6]
		{
			Hmax.GetValueAsUint32(),
			Amax.GetValueAsUint32(),
			Bmax.GetValueAsUint32(),
			Cmax.GetValueAsUint32(),
			Dmax.GetValueAsUint32(),
			Smax.GetValueAsUint32()
		});
	}

	private ICriteria<Pokemon.Individual> GetCriteria()
	{
		(uint[] Min, uint[] Max) iVs = GetIVs();
		uint[] item = iVs.Min;
		uint[] item2 = iVs.Max;
		Nature[] array = NaturePanel.GetSelectedNatures().ToArray();
		List<ICriteria<Pokemon.Individual>> list = new List<ICriteria<Pokemon.Individual>>();
		list.Add(new IVsCriteria(item, item2));
		if (array.Length != 0 && array.Length < 25)
		{
			list.Add(new NatureCriteria(array));
		}
		if (HPTypeBox.SelectedIndex > 0)
		{
			list.Add(new HiddenPowerTypeCriteria(HPTypeBox.Text.JpnToPokeType()));
		}
		list.Add(new HiddenPowerPowerCriteria((uint)HPPowerBox.Value));
		return Criteria.AND(list.ToArray());
	}

	private void CalcButton_Click(object sender, EventArgs e)
	{
		IEnumerable<(IIVsGenerator, string)> selectedMethods = GetSelectedMethods();
		if (selectedMethods.Count() == 0 || !TryGetSeed(out var seed))
		{
			return;
		}
		uint valueAsUint = MinFrameBox.GetValueAsUint32();
		uint valueAsUint2 = MaxFrameBox.GetValueAsUint32();
		seed.Advance(valueAsUint);
		GBASlot slot = new GBASlot(-1, "ヌオー", 50u);
		IEnumerable<(StationaryGenerator, string)> generators = selectedMethods.Select((IIVsGenerator _) => new StationaryGenerator(slot, _));
		ICriteria<Pokemon.Individual> criteria = GetCriteria();
		IEnumerable<Binder> data = from _ in CommonEnumerator.WithIndex<uint>(seed.EnumerateSeed().Take((int)(valueAsUint2 - valueAsUint + 1))).SelectMany<(int, uint), (RNGResult<Pokemon.Individual, uint>, int, string)>(((int index, uint element) _) => generators.Select(((StationaryGenerator, string) x) => (x.Item1.Generate(_.element), _.index, x.Item2)))
			where criteria.CheckConditions(_.Content)
			select new Binder(seed, _.Item2, _.Item3, _.Item1);
		wrap.SetData(data);
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
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
		this.dataGridView1 = new System.Windows.Forms.DataGridView();
		this.k_groupBox2 = new System.Windows.Forms.GroupBox();
		this.CheckGender = new System.Windows.Forms.CheckBox();
		this.CheckAbility = new System.Windows.Forms.CheckBox();
		this.SID_stationary = new System.Windows.Forms.NumericUpDown();
		this.CalcButton = new System.Windows.Forms.Button();
		this.AbilityBox = new System.Windows.Forms.ComboBox();
		this.TID_stationary = new System.Windows.Forms.NumericUpDown();
		this.GenderBox = new System.Windows.Forms.ComboBox();
		this.label6 = new System.Windows.Forms.Label();
		this.k_shiny = new System.Windows.Forms.CheckBox();
		this.label27 = new System.Windows.Forms.Label();
		this.PokemonBox = new System.Windows.Forms.ComboBox();
		this.panel2 = new System.Windows.Forms.Panel();
		this.label7 = new System.Windows.Forms.Label();
		this.Hmax = new System.Windows.Forms.NumericUpDown();
		this.label8 = new System.Windows.Forms.Label();
		this.Cmax = new System.Windows.Forms.NumericUpDown();
		this.Smin = new System.Windows.Forms.NumericUpDown();
		this.Dmin = new System.Windows.Forms.NumericUpDown();
		this.label11 = new System.Windows.Forms.Label();
		this.label12 = new System.Windows.Forms.Label();
		this.Cmin = new System.Windows.Forms.NumericUpDown();
		this.label13 = new System.Windows.Forms.Label();
		this.Dmax = new System.Windows.Forms.NumericUpDown();
		this.Bmin = new System.Windows.Forms.NumericUpDown();
		this.Amin = new System.Windows.Forms.NumericUpDown();
		this.label14 = new System.Windows.Forms.Label();
		this.label15 = new System.Windows.Forms.Label();
		this.label16 = new System.Windows.Forms.Label();
		this.Hmin = new System.Windows.Forms.NumericUpDown();
		this.Smax = new System.Windows.Forms.NumericUpDown();
		this.label17 = new System.Windows.Forms.Label();
		this.Amax = new System.Windows.Forms.NumericUpDown();
		this.label18 = new System.Windows.Forms.Label();
		this.Bmax = new System.Windows.Forms.NumericUpDown();
		this.label19 = new System.Windows.Forms.Label();
		this.label20 = new System.Windows.Forms.Label();
		this.label29 = new System.Windows.Forms.Label();
		this.HPPowerBox = new System.Windows.Forms.NumericUpDown();
		this.HPTypeBox = new System.Windows.Forms.ComboBox();
		this.label31 = new System.Windows.Forms.Label();
		this.k_groupBox1 = new System.Windows.Forms.GroupBox();
		this.CheckMethod1 = new System.Windows.Forms.CheckBox();
		this.CheckMethod2 = new System.Windows.Forms.CheckBox();
		this.CheckMethod4 = new System.Windows.Forms.CheckBox();
		this.MaxFrameBox = new System.Windows.Forms.NumericUpDown();
		this.MinFrameBox = new System.Windows.Forms.NumericUpDown();
		this.SeedBox = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label40 = new System.Windows.Forms.Label();
		this.label39 = new System.Windows.Forms.Label();
		this.NaturePanel = new _3genSearch.NaturePanel();
		((System.ComponentModel.ISupportInitialize)this.dataGridView1).BeginInit();
		this.k_groupBox2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.SID_stationary).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.TID_stationary).BeginInit();
		this.panel2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.Hmax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Cmax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Smin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Dmin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Cmin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Dmax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Bmin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Amin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Hmin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Smax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Amax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Bmax).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.HPPowerBox).BeginInit();
		this.k_groupBox1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.MaxFrameBox).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.MinFrameBox).BeginInit();
		base.SuspendLayout();
		this.dataGridView1.AllowUserToAddRows = false;
		this.dataGridView1.AllowUserToDeleteRows = false;
		this.dataGridView1.AllowUserToResizeColumns = false;
		this.dataGridView1.AllowUserToResizeRows = false;
		dataGridViewCellStyle.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
		this.dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle;
		this.dataGridView1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
		dataGridViewCellStyle2.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle2;
		this.dataGridView1.Location = new System.Drawing.Point(12, 12);
		this.dataGridView1.Name = "dataGridView1";
		dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle3.Font = new System.Drawing.Font("Yu Gothic UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
		this.dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
		this.dataGridView1.RowTemplate.Height = 21;
		this.dataGridView1.RowTemplate.ReadOnly = true;
		this.dataGridView1.Size = new System.Drawing.Size(960, 222);
		this.dataGridView1.TabIndex = 1;
		this.k_groupBox2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.k_groupBox2.Controls.Add(this.CheckGender);
		this.k_groupBox2.Controls.Add(this.CheckAbility);
		this.k_groupBox2.Controls.Add(this.NaturePanel);
		this.k_groupBox2.Controls.Add(this.SID_stationary);
		this.k_groupBox2.Controls.Add(this.CalcButton);
		this.k_groupBox2.Controls.Add(this.AbilityBox);
		this.k_groupBox2.Controls.Add(this.TID_stationary);
		this.k_groupBox2.Controls.Add(this.GenderBox);
		this.k_groupBox2.Controls.Add(this.label6);
		this.k_groupBox2.Controls.Add(this.k_shiny);
		this.k_groupBox2.Controls.Add(this.label27);
		this.k_groupBox2.Controls.Add(this.PokemonBox);
		this.k_groupBox2.Location = new System.Drawing.Point(405, 240);
		this.k_groupBox2.Name = "k_groupBox2";
		this.k_groupBox2.Size = new System.Drawing.Size(567, 279);
		this.k_groupBox2.TabIndex = 204;
		this.k_groupBox2.TabStop = false;
		this.CheckGender.AutoSize = true;
		this.CheckGender.Location = new System.Drawing.Point(20, 77);
		this.CheckGender.Name = "CheckGender";
		this.CheckGender.Size = new System.Drawing.Size(48, 16);
		this.CheckGender.TabIndex = 210;
		this.CheckGender.Text = "性別";
		this.CheckGender.UseVisualStyleBackColor = true;
		this.CheckAbility.AutoSize = true;
		this.CheckAbility.Location = new System.Drawing.Point(20, 51);
		this.CheckAbility.Name = "CheckAbility";
		this.CheckAbility.Size = new System.Drawing.Size(48, 16);
		this.CheckAbility.TabIndex = 209;
		this.CheckAbility.Text = "特性";
		this.CheckAbility.UseVisualStyleBackColor = true;
		this.SID_stationary.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.SID_stationary.Location = new System.Drawing.Point(324, 73);
		this.SID_stationary.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.SID_stationary.Name = "SID_stationary";
		this.SID_stationary.Size = new System.Drawing.Size(80, 22);
		this.SID_stationary.TabIndex = 48;
		this.CalcButton.Location = new System.Drawing.Point(486, 235);
		this.CalcButton.Name = "CalcButton";
		this.CalcButton.Size = new System.Drawing.Size(75, 38);
		this.CalcButton.TabIndex = 205;
		this.CalcButton.Text = "計算";
		this.CalcButton.UseVisualStyleBackColor = true;
		this.CalcButton.Click += new System.EventHandler(CalcButton_Click);
		this.AbilityBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.AbilityBox.FormattingEnabled = true;
		this.AbilityBox.Location = new System.Drawing.Point(70, 49);
		this.AbilityBox.Name = "AbilityBox";
		this.AbilityBox.Size = new System.Drawing.Size(106, 20);
		this.AbilityBox.TabIndex = 123;
		this.TID_stationary.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TID_stationary.Location = new System.Drawing.Point(324, 45);
		this.TID_stationary.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.TID_stationary.Name = "TID_stationary";
		this.TID_stationary.Size = new System.Drawing.Size(80, 22);
		this.TID_stationary.TabIndex = 47;
		this.GenderBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.GenderBox.FormattingEnabled = true;
		this.GenderBox.Items.AddRange(new object[4] { "指定なし", "♂", "♀", "-" });
		this.GenderBox.Location = new System.Drawing.Point(70, 75);
		this.GenderBox.Name = "GenderBox";
		this.GenderBox.Size = new System.Drawing.Size(106, 20);
		this.GenderBox.TabIndex = 121;
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(290, 78);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(28, 12);
		this.label6.TabIndex = 100;
		this.label6.Text = "裏ID";
		this.k_shiny.AutoSize = true;
		this.k_shiny.Location = new System.Drawing.Point(292, 18);
		this.k_shiny.Name = "k_shiny";
		this.k_shiny.Size = new System.Drawing.Size(103, 16);
		this.k_shiny.TabIndex = 49;
		this.k_shiny.Text = "色違いのみ出力";
		this.k_shiny.UseVisualStyleBackColor = true;
		this.label27.AutoSize = true;
		this.label27.Location = new System.Drawing.Point(290, 50);
		this.label27.Name = "label27";
		this.label27.Size = new System.Drawing.Size(28, 12);
		this.label27.TabIndex = 68;
		this.label27.Text = "表ID";
		this.PokemonBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.PokemonBox.Enabled = false;
		this.PokemonBox.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.PokemonBox.FormattingEnabled = true;
		this.PokemonBox.Location = new System.Drawing.Point(14, 21);
		this.PokemonBox.Name = "PokemonBox";
		this.PokemonBox.Size = new System.Drawing.Size(162, 22);
		this.PokemonBox.TabIndex = 22;
		this.panel2.Controls.Add(this.label7);
		this.panel2.Controls.Add(this.Hmax);
		this.panel2.Controls.Add(this.label8);
		this.panel2.Controls.Add(this.Cmax);
		this.panel2.Controls.Add(this.Smin);
		this.panel2.Controls.Add(this.Dmin);
		this.panel2.Controls.Add(this.label11);
		this.panel2.Controls.Add(this.label12);
		this.panel2.Controls.Add(this.Cmin);
		this.panel2.Controls.Add(this.label13);
		this.panel2.Controls.Add(this.Dmax);
		this.panel2.Controls.Add(this.Bmin);
		this.panel2.Controls.Add(this.Amin);
		this.panel2.Controls.Add(this.label14);
		this.panel2.Controls.Add(this.label15);
		this.panel2.Controls.Add(this.label16);
		this.panel2.Controls.Add(this.Hmin);
		this.panel2.Controls.Add(this.Smax);
		this.panel2.Controls.Add(this.label17);
		this.panel2.Controls.Add(this.Amax);
		this.panel2.Controls.Add(this.label18);
		this.panel2.Controls.Add(this.Bmax);
		this.panel2.Controls.Add(this.label19);
		this.panel2.Controls.Add(this.label20);
		this.panel2.Location = new System.Drawing.Point(59, 82);
		this.panel2.Name = "panel2";
		this.panel2.Size = new System.Drawing.Size(144, 186);
		this.panel2.TabIndex = 104;
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(3, 8);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(13, 12);
		this.label7.TabIndex = 38;
		this.label7.Text = "H";
		this.Hmax.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Hmax.Location = new System.Drawing.Point(96, 3);
		this.Hmax.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Hmax.Name = "Hmax";
		this.Hmax.Size = new System.Drawing.Size(45, 22);
		this.Hmax.TabIndex = 26;
		this.Hmax.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(73, 8);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(17, 12);
		this.label8.TabIndex = 76;
		this.label8.Text = "～";
		this.Cmax.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Cmax.Location = new System.Drawing.Point(96, 99);
		this.Cmax.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Cmax.Name = "Cmax";
		this.Cmax.Size = new System.Drawing.Size(45, 22);
		this.Cmax.TabIndex = 32;
		this.Cmax.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Smin.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Smin.Location = new System.Drawing.Point(22, 163);
		this.Smin.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Smin.Name = "Smin";
		this.Smin.Size = new System.Drawing.Size(45, 22);
		this.Smin.TabIndex = 35;
		this.Dmin.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Dmin.Location = new System.Drawing.Point(22, 131);
		this.Dmin.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Dmin.Name = "Dmin";
		this.Dmin.Size = new System.Drawing.Size(45, 22);
		this.Dmin.TabIndex = 33;
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(73, 136);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(17, 12);
		this.label11.TabIndex = 84;
		this.label11.Text = "～";
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(3, 168);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(12, 12);
		this.label12.TabIndex = 48;
		this.label12.Text = "S";
		this.Cmin.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Cmin.Location = new System.Drawing.Point(22, 99);
		this.Cmin.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Cmin.Name = "Cmin";
		this.Cmin.Size = new System.Drawing.Size(45, 22);
		this.Cmin.TabIndex = 31;
		this.label13.AutoSize = true;
		this.label13.Location = new System.Drawing.Point(3, 136);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(13, 12);
		this.label13.TabIndex = 46;
		this.label13.Text = "D";
		this.Dmax.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Dmax.Location = new System.Drawing.Point(96, 131);
		this.Dmax.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Dmax.Name = "Dmax";
		this.Dmax.Size = new System.Drawing.Size(45, 22);
		this.Dmax.TabIndex = 34;
		this.Dmax.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Bmin.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Bmin.Location = new System.Drawing.Point(22, 67);
		this.Bmin.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Bmin.Name = "Bmin";
		this.Bmin.Size = new System.Drawing.Size(45, 22);
		this.Bmin.TabIndex = 29;
		this.Amin.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Amin.Location = new System.Drawing.Point(22, 35);
		this.Amin.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Amin.Name = "Amin";
		this.Amin.Size = new System.Drawing.Size(45, 22);
		this.Amin.TabIndex = 27;
		this.label14.AutoSize = true;
		this.label14.Location = new System.Drawing.Point(3, 104);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(13, 12);
		this.label14.TabIndex = 44;
		this.label14.Text = "C";
		this.label15.AutoSize = true;
		this.label15.Location = new System.Drawing.Point(73, 168);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(17, 12);
		this.label15.TabIndex = 86;
		this.label15.Text = "～";
		this.label16.AutoSize = true;
		this.label16.Location = new System.Drawing.Point(3, 72);
		this.label16.Name = "label16";
		this.label16.Size = new System.Drawing.Size(13, 12);
		this.label16.TabIndex = 42;
		this.label16.Text = "B";
		this.Hmin.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Hmin.Location = new System.Drawing.Point(22, 3);
		this.Hmin.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Hmin.Name = "Hmin";
		this.Hmin.Size = new System.Drawing.Size(45, 22);
		this.Hmin.TabIndex = 25;
		this.Smax.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Smax.Location = new System.Drawing.Point(96, 163);
		this.Smax.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Smax.Name = "Smax";
		this.Smax.Size = new System.Drawing.Size(45, 22);
		this.Smax.TabIndex = 36;
		this.Smax.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.label17.AutoSize = true;
		this.label17.Location = new System.Drawing.Point(3, 40);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(13, 12);
		this.label17.TabIndex = 40;
		this.label17.Text = "A";
		this.Amax.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Amax.Location = new System.Drawing.Point(96, 35);
		this.Amax.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Amax.Name = "Amax";
		this.Amax.Size = new System.Drawing.Size(45, 22);
		this.Amax.TabIndex = 28;
		this.Amax.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.label18.AutoSize = true;
		this.label18.Location = new System.Drawing.Point(73, 40);
		this.label18.Name = "label18";
		this.label18.Size = new System.Drawing.Size(17, 12);
		this.label18.TabIndex = 78;
		this.label18.Text = "～";
		this.Bmax.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Bmax.Location = new System.Drawing.Point(96, 67);
		this.Bmax.Maximum = new decimal(new int[4] { 31, 0, 0, 0 });
		this.Bmax.Name = "Bmax";
		this.Bmax.Size = new System.Drawing.Size(45, 22);
		this.Bmax.TabIndex = 30;
		this.Bmax.Value = new decimal(new int[4] { 31, 0, 0, 0 });
		this.label19.AutoSize = true;
		this.label19.Location = new System.Drawing.Point(73, 72);
		this.label19.Name = "label19";
		this.label19.Size = new System.Drawing.Size(17, 12);
		this.label19.TabIndex = 80;
		this.label19.Text = "～";
		this.label20.AutoSize = true;
		this.label20.Location = new System.Drawing.Point(73, 104);
		this.label20.Name = "label20";
		this.label20.Size = new System.Drawing.Size(17, 12);
		this.label20.TabIndex = 82;
		this.label20.Text = "～";
		this.label29.AutoSize = true;
		this.label29.Location = new System.Drawing.Point(308, 246);
		this.label29.Name = "label29";
		this.label29.Size = new System.Drawing.Size(17, 12);
		this.label29.TabIndex = 99;
		this.label29.Text = "～";
		this.HPPowerBox.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.HPPowerBox.Location = new System.Drawing.Point(252, 241);
		this.HPPowerBox.Maximum = new decimal(new int[4] { 70, 0, 0, 0 });
		this.HPPowerBox.Minimum = new decimal(new int[4] { 30, 0, 0, 0 });
		this.HPPowerBox.Name = "HPPowerBox";
		this.HPPowerBox.Size = new System.Drawing.Size(50, 22);
		this.HPPowerBox.TabIndex = 46;
		this.HPPowerBox.Value = new decimal(new int[4] { 30, 0, 0, 0 });
		this.HPTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.HPTypeBox.FormattingEnabled = true;
		this.HPTypeBox.Location = new System.Drawing.Point(252, 215);
		this.HPTypeBox.Name = "HPTypeBox";
		this.HPTypeBox.Size = new System.Drawing.Size(80, 20);
		this.HPTypeBox.TabIndex = 45;
		this.label31.AutoSize = true;
		this.label31.Location = new System.Drawing.Point(212, 218);
		this.label31.Name = "label31";
		this.label31.Size = new System.Drawing.Size(34, 12);
		this.label31.TabIndex = 97;
		this.label31.Text = "めざパ";
		this.k_groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.k_groupBox1.Controls.Add(this.CheckMethod1);
		this.k_groupBox1.Controls.Add(this.CheckMethod2);
		this.k_groupBox1.Controls.Add(this.CheckMethod4);
		this.k_groupBox1.Controls.Add(this.panel2);
		this.k_groupBox1.Controls.Add(this.MaxFrameBox);
		this.k_groupBox1.Controls.Add(this.MinFrameBox);
		this.k_groupBox1.Controls.Add(this.SeedBox);
		this.k_groupBox1.Controls.Add(this.label1);
		this.k_groupBox1.Controls.Add(this.label40);
		this.k_groupBox1.Controls.Add(this.label39);
		this.k_groupBox1.Controls.Add(this.HPTypeBox);
		this.k_groupBox1.Controls.Add(this.label29);
		this.k_groupBox1.Controls.Add(this.HPPowerBox);
		this.k_groupBox1.Controls.Add(this.label31);
		this.k_groupBox1.Location = new System.Drawing.Point(12, 240);
		this.k_groupBox1.Name = "k_groupBox1";
		this.k_groupBox1.Size = new System.Drawing.Size(387, 279);
		this.k_groupBox1.TabIndex = 203;
		this.k_groupBox1.TabStop = false;
		this.CheckMethod1.AutoSize = true;
		this.CheckMethod1.Checked = true;
		this.CheckMethod1.CheckState = System.Windows.Forms.CheckState.Checked;
		this.CheckMethod1.Location = new System.Drawing.Point(214, 89);
		this.CheckMethod1.Name = "CheckMethod1";
		this.CheckMethod1.Size = new System.Drawing.Size(67, 16);
		this.CheckMethod1.TabIndex = 106;
		this.CheckMethod1.Text = "Method1";
		this.CheckMethod1.UseVisualStyleBackColor = true;
		this.CheckMethod2.AutoSize = true;
		this.CheckMethod2.Location = new System.Drawing.Point(214, 121);
		this.CheckMethod2.Name = "CheckMethod2";
		this.CheckMethod2.Size = new System.Drawing.Size(67, 16);
		this.CheckMethod2.TabIndex = 108;
		this.CheckMethod2.Text = "Method2";
		this.CheckMethod2.UseVisualStyleBackColor = true;
		this.CheckMethod4.AutoSize = true;
		this.CheckMethod4.Location = new System.Drawing.Point(287, 89);
		this.CheckMethod4.Name = "CheckMethod4";
		this.CheckMethod4.Size = new System.Drawing.Size(67, 16);
		this.CheckMethod4.TabIndex = 107;
		this.CheckMethod4.Text = "Method4";
		this.CheckMethod4.UseVisualStyleBackColor = true;
		this.MaxFrameBox.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.MaxFrameBox.Location = new System.Drawing.Point(204, 54);
		this.MaxFrameBox.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.MaxFrameBox.Name = "MaxFrameBox";
		this.MaxFrameBox.Size = new System.Drawing.Size(93, 22);
		this.MaxFrameBox.TabIndex = 10;
		this.MaxFrameBox.Value = new decimal(new int[4] { 100000, 0, 0, 0 });
		this.MinFrameBox.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.MinFrameBox.Location = new System.Drawing.Point(81, 54);
		this.MinFrameBox.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.MinFrameBox.Name = "MinFrameBox";
		this.MinFrameBox.Size = new System.Drawing.Size(93, 22);
		this.MinFrameBox.TabIndex = 9;
		this.SeedBox.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.SeedBox.Location = new System.Drawing.Point(81, 26);
		this.SeedBox.Name = "SeedBox";
		this.SeedBox.Size = new System.Drawing.Size(93, 22);
		this.SeedBox.TabIndex = 2;
		this.SeedBox.Text = "5A0";
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(22, 30);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(53, 12);
		this.label1.TabIndex = 70;
		this.label1.Text = "初期seed";
		this.label40.AutoSize = true;
		this.label40.Location = new System.Drawing.Point(57, 59);
		this.label40.Name = "label40";
		this.label40.Size = new System.Drawing.Size(12, 12);
		this.label40.TabIndex = 64;
		this.label40.Text = "F";
		this.label39.Location = new System.Drawing.Point(180, 59);
		this.label39.Name = "label39";
		this.label39.Size = new System.Drawing.Size(18, 16);
		this.label39.TabIndex = 61;
		this.label39.Text = "～";
		this.NaturePanel.Location = new System.Drawing.Point(14, 104);
		this.NaturePanel.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
		this.NaturePanel.Name = "NaturePanel";
		this.NaturePanel.Size = new System.Drawing.Size(388, 140);
		this.NaturePanel.TabIndex = 208;
		this.NaturePanel.TabStop = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(983, 531);
		base.Controls.Add(this.k_groupBox2);
		base.Controls.Add(this.k_groupBox1);
		base.Controls.Add(this.dataGridView1);
		base.Name = "CalcGap";
		this.Text = "RSSimpleSearch";
		((System.ComponentModel.ISupportInitialize)this.dataGridView1).EndInit();
		this.k_groupBox2.ResumeLayout(false);
		this.k_groupBox2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.SID_stationary).EndInit();
		((System.ComponentModel.ISupportInitialize)this.TID_stationary).EndInit();
		this.panel2.ResumeLayout(false);
		this.panel2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.Hmax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Cmax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Smin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Dmin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Cmin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Dmax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Bmin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Amin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Hmin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Smax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Amax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Bmax).EndInit();
		((System.ComponentModel.ISupportInitialize)this.HPPowerBox).EndInit();
		this.k_groupBox1.ResumeLayout(false);
		this.k_groupBox1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.MaxFrameBox).EndInit();
		((System.ComponentModel.ISupportInitialize)this.MinFrameBox).EndInit();
		base.ResumeLayout(false);
	}
}
