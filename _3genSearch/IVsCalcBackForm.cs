using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PokemonStandardLibrary;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch;

public class IVsCalcBackForm : Form
{
	private IContainer components;

	private NumericUpDown Stat_H;

	private NumericUpDown Stat_A;

	private NumericUpDown Stat_B;

	private NumericUpDown Stat_C;

	private NumericUpDown Stat_D;

	private NumericUpDown Stat_S;

	private ComboBox PokemonBox;

	private NumericUpDown LvBox;

	private ComboBox NatureBox;

	private TextBox textBox1;

	private Label label1;

	private Label label_H;

	private Label label_A;

	private Label label_B;

	private Label label_C;

	private Label label_D;

	private Label label_S;

	private Button CalcButton;

	public IVsCalcBackForm()
	{
		InitializeComponent();
		string[] array = new string[25]
		{
			"がんばりや", "さみしがり", "ゆうかん", "いじっぱり", "やんちゃ", "ずぶとい", "すなお", "のんき", "わんぱく", "のうてんき",
			"おくびょう", "せっかち", "まじめ", "ようき", "むじゃき", "ひかえめ", "おっとり", "れいせい", "てれや", "うっかりや",
			"おだやか", "おとなしい", "なまいき", "しんちょう", "きまぐれ"
		};
		ComboBox.ObjectCollection items = NatureBox.Items;
		object[] items2 = array;
		items.AddRange(items2);
		string[] array2 = (from _ in Pokemon.GetUniquePokemonList()
			select _.Name).ToArray();
		ComboBox.ObjectCollection items3 = PokemonBox.Items;
		items2 = array2;
		items3.AddRange(items2);
		NatureBox.SelectedIndex = 0;
		PokemonBox.SelectedIndex = 381;
	}

	private void UpdateStats()
	{
		IReadOnlyList<uint> stats = Pokemon.GetPokemon(PokemonBox.Text).GetIndividual(Util.GetValueDec(LvBox), new uint[6] { 31u, 31u, 31u, 31u, 31u, 31u }, (uint)NatureBox.SelectedIndex).Stats;
		IReadOnlyList<uint> stats2 = Pokemon.GetPokemon(PokemonBox.Text).GetIndividual(Util.GetValueDec(LvBox), new uint[6], (uint)NatureBox.SelectedIndex).Stats;
		Stat_H.Maximum = stats[0];
		Stat_A.Maximum = stats[1];
		Stat_B.Maximum = stats[2];
		Stat_C.Maximum = stats[3];
		Stat_D.Maximum = stats[4];
		Stat_S.Maximum = stats[5];
		Stat_H.Minimum = stats2[0];
		Stat_A.Minimum = stats2[1];
		Stat_B.Minimum = stats2[2];
		Stat_C.Minimum = stats2[3];
		Stat_D.Minimum = stats2[4];
		Stat_S.Minimum = stats2[5];
	}

	private void CalcButton_Click(object sender, EventArgs e)
	{
		(uint[], uint[]) tuple = SpeciesExtensions.CalcIVsRange(stats: new uint[6]
		{
			Util.GetValueDec(Stat_H),
			Util.GetValueDec(Stat_A),
			Util.GetValueDec(Stat_B),
			Util.GetValueDec(Stat_C),
			Util.GetValueDec(Stat_D),
			Util.GetValueDec(Stat_S)
		}, species: Pokemon.GetPokemon(PokemonBox.Text), lv: Util.GetValueDec(LvBox), nature: (Nature)NatureBox.SelectedIndex);
		string text = "";
		text = text + "H : " + ((tuple.Item1[0] == 32) ? "???" : ($"{tuple.Item1[0]}" + ((tuple.Item2[0] != tuple.Item1[0]) ? $" - {tuple.Item2[0]}" : ""))) + Environment.NewLine;
		text = text + "A : " + ((tuple.Item1[1] == 32) ? "???" : ($"{tuple.Item1[1]}" + ((tuple.Item2[1] != tuple.Item1[1]) ? $" - {tuple.Item2[1]}" : ""))) + Environment.NewLine;
		text = text + "B : " + ((tuple.Item1[2] == 32) ? "???" : ($"{tuple.Item1[2]}" + ((tuple.Item2[2] != tuple.Item1[2]) ? $" - {tuple.Item2[2]}" : ""))) + Environment.NewLine;
		text = text + "C : " + ((tuple.Item1[3] == 32) ? "???" : ($"{tuple.Item1[3]}" + ((tuple.Item2[3] != tuple.Item1[3]) ? $" - {tuple.Item2[3]}" : ""))) + Environment.NewLine;
		text = text + "D : " + ((tuple.Item1[4] == 32) ? "???" : ($"{tuple.Item1[4]}" + ((tuple.Item2[4] != tuple.Item1[4]) ? $" - {tuple.Item2[4]}" : ""))) + Environment.NewLine;
		text = text + "S : " + ((tuple.Item1[5] == 32) ? "???" : ($"{tuple.Item1[5]}" + ((tuple.Item2[5] != tuple.Item1[5]) ? $" - {tuple.Item2[5]}" : ""))) + Environment.NewLine;
		textBox1.Text = text;
	}

	private void PokemonBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		UpdateStats();
		IReadOnlyList<uint> stats = Pokemon.GetPokemon(PokemonBox.Text).GetIndividual(Util.GetValueDec(LvBox), new uint[6] { 31u, 31u, 31u, 31u, 31u, 31u }, (uint)NatureBox.SelectedIndex).Stats;
		Stat_H.Value = stats[0];
		Stat_A.Value = stats[1];
		Stat_B.Value = stats[2];
		Stat_C.Value = stats[3];
		Stat_D.Value = stats[4];
		Stat_S.Value = stats[5];
	}

	private void NatureBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		Label label = label_A;
		Label label2 = label_B;
		Label label3 = label_C;
		Label label4 = label_D;
		Color color = (label_S.ForeColor = Color.Black);
		Color color3 = (label4.ForeColor = color);
		Color color5 = (label3.ForeColor = color3);
		Color foreColor = (label2.ForeColor = color5);
		label.ForeColor = foreColor;
		if (NatureBox.SelectedIndex / 5 != NatureBox.SelectedIndex % 5)
		{
			if (NatureBox.SelectedIndex / 5 == 0)
			{
				label_A.ForeColor = Color.Red;
			}
			if (NatureBox.SelectedIndex / 5 == 1)
			{
				label_B.ForeColor = Color.Red;
			}
			if (NatureBox.SelectedIndex / 5 == 2)
			{
				label_S.ForeColor = Color.Red;
			}
			if (NatureBox.SelectedIndex / 5 == 3)
			{
				label_C.ForeColor = Color.Red;
			}
			if (NatureBox.SelectedIndex / 5 == 4)
			{
				label_D.ForeColor = Color.Red;
			}
			if (NatureBox.SelectedIndex % 5 == 0)
			{
				label_A.ForeColor = Color.DodgerBlue;
			}
			if (NatureBox.SelectedIndex % 5 == 1)
			{
				label_B.ForeColor = Color.DodgerBlue;
			}
			if (NatureBox.SelectedIndex % 5 == 2)
			{
				label_S.ForeColor = Color.DodgerBlue;
			}
			if (NatureBox.SelectedIndex % 5 == 3)
			{
				label_C.ForeColor = Color.DodgerBlue;
			}
			if (NatureBox.SelectedIndex % 5 == 4)
			{
				label_D.ForeColor = Color.DodgerBlue;
			}
			UpdateStats();
		}
	}

	private void NumericUpDown_SelectValue(object sender, EventArgs e)
	{
		((NumericUpDown)sender).Select(0, ((NumericUpDown)sender).Text.Length);
	}

	private void LvBox_ValueChanged(object sender, EventArgs e)
	{
		UpdateStats();
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
		this.Stat_H = new System.Windows.Forms.NumericUpDown();
		this.Stat_A = new System.Windows.Forms.NumericUpDown();
		this.Stat_B = new System.Windows.Forms.NumericUpDown();
		this.Stat_C = new System.Windows.Forms.NumericUpDown();
		this.Stat_D = new System.Windows.Forms.NumericUpDown();
		this.Stat_S = new System.Windows.Forms.NumericUpDown();
		this.PokemonBox = new System.Windows.Forms.ComboBox();
		this.LvBox = new System.Windows.Forms.NumericUpDown();
		this.NatureBox = new System.Windows.Forms.ComboBox();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label_H = new System.Windows.Forms.Label();
		this.label_A = new System.Windows.Forms.Label();
		this.label_B = new System.Windows.Forms.Label();
		this.label_C = new System.Windows.Forms.Label();
		this.label_D = new System.Windows.Forms.Label();
		this.label_S = new System.Windows.Forms.Label();
		this.CalcButton = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.Stat_H).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Stat_A).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Stat_B).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Stat_C).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Stat_D).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.Stat_S).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.LvBox).BeginInit();
		base.SuspendLayout();
		this.Stat_H.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Stat_H.Location = new System.Drawing.Point(38, 38);
		this.Stat_H.Maximum = new decimal(new int[4] { 999, 0, 0, 0 });
		this.Stat_H.Name = "Stat_H";
		this.Stat_H.Size = new System.Drawing.Size(64, 22);
		this.Stat_H.TabIndex = 1;
		this.Stat_H.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.Stat_A.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Stat_A.Location = new System.Drawing.Point(38, 66);
		this.Stat_A.Maximum = new decimal(new int[4] { 999, 0, 0, 0 });
		this.Stat_A.Name = "Stat_A";
		this.Stat_A.Size = new System.Drawing.Size(64, 22);
		this.Stat_A.TabIndex = 2;
		this.Stat_A.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.Stat_B.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Stat_B.Location = new System.Drawing.Point(38, 94);
		this.Stat_B.Maximum = new decimal(new int[4] { 999, 0, 0, 0 });
		this.Stat_B.Name = "Stat_B";
		this.Stat_B.Size = new System.Drawing.Size(64, 22);
		this.Stat_B.TabIndex = 3;
		this.Stat_B.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.Stat_C.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Stat_C.Location = new System.Drawing.Point(38, 122);
		this.Stat_C.Maximum = new decimal(new int[4] { 999, 0, 0, 0 });
		this.Stat_C.Name = "Stat_C";
		this.Stat_C.Size = new System.Drawing.Size(64, 22);
		this.Stat_C.TabIndex = 4;
		this.Stat_C.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.Stat_D.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Stat_D.Location = new System.Drawing.Point(38, 150);
		this.Stat_D.Maximum = new decimal(new int[4] { 999, 0, 0, 0 });
		this.Stat_D.Name = "Stat_D";
		this.Stat_D.Size = new System.Drawing.Size(64, 22);
		this.Stat_D.TabIndex = 5;
		this.Stat_D.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.Stat_S.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.Stat_S.Location = new System.Drawing.Point(38, 178);
		this.Stat_S.Maximum = new decimal(new int[4] { 999, 0, 0, 0 });
		this.Stat_S.Name = "Stat_S";
		this.Stat_S.Size = new System.Drawing.Size(64, 22);
		this.Stat_S.TabIndex = 6;
		this.Stat_S.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.PokemonBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.PokemonBox.FormattingEnabled = true;
		this.PokemonBox.Location = new System.Drawing.Point(12, 12);
		this.PokemonBox.Name = "PokemonBox";
		this.PokemonBox.Size = new System.Drawing.Size(90, 20);
		this.PokemonBox.TabIndex = 24;
		this.PokemonBox.SelectedIndexChanged += new System.EventHandler(PokemonBox_SelectedIndexChanged);
		this.LvBox.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.LvBox.Location = new System.Drawing.Point(139, 12);
		this.LvBox.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.LvBox.Name = "LvBox";
		this.LvBox.Size = new System.Drawing.Size(50, 22);
		this.LvBox.TabIndex = 25;
		this.LvBox.Value = new decimal(new int[4] { 50, 0, 0, 0 });
		this.LvBox.ValueChanged += new System.EventHandler(LvBox_ValueChanged);
		this.LvBox.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.NatureBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.NatureBox.FormattingEnabled = true;
		this.NatureBox.Location = new System.Drawing.Point(109, 40);
		this.NatureBox.Name = "NatureBox";
		this.NatureBox.Size = new System.Drawing.Size(80, 20);
		this.NatureBox.TabIndex = 62;
		this.NatureBox.SelectedIndexChanged += new System.EventHandler(NatureBox_SelectedIndexChanged);
		this.textBox1.Location = new System.Drawing.Point(195, 13);
		this.textBox1.Multiline = true;
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(204, 189);
		this.textBox1.TabIndex = 63;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(114, 15);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(19, 12);
		this.label1.TabIndex = 64;
		this.label1.Text = "Lv.";
		this.label_H.AutoSize = true;
		this.label_H.Font = new System.Drawing.Font("MS UI Gothic", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 128);
		this.label_H.Location = new System.Drawing.Point(19, 43);
		this.label_H.Name = "label_H";
		this.label_H.Size = new System.Drawing.Size(14, 12);
		this.label_H.TabIndex = 65;
		this.label_H.Text = "H";
		this.label_A.AutoSize = true;
		this.label_A.Font = new System.Drawing.Font("MS UI Gothic", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 128);
		this.label_A.Location = new System.Drawing.Point(19, 69);
		this.label_A.Name = "label_A";
		this.label_A.Size = new System.Drawing.Size(14, 12);
		this.label_A.TabIndex = 66;
		this.label_A.Text = "A";
		this.label_B.AutoSize = true;
		this.label_B.Font = new System.Drawing.Font("MS UI Gothic", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 128);
		this.label_B.Location = new System.Drawing.Point(19, 97);
		this.label_B.Name = "label_B";
		this.label_B.Size = new System.Drawing.Size(14, 12);
		this.label_B.TabIndex = 67;
		this.label_B.Text = "B";
		this.label_C.AutoSize = true;
		this.label_C.Font = new System.Drawing.Font("MS UI Gothic", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 128);
		this.label_C.Location = new System.Drawing.Point(19, 125);
		this.label_C.Name = "label_C";
		this.label_C.Size = new System.Drawing.Size(14, 12);
		this.label_C.TabIndex = 68;
		this.label_C.Text = "C";
		this.label_D.AutoSize = true;
		this.label_D.Font = new System.Drawing.Font("MS UI Gothic", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 128);
		this.label_D.Location = new System.Drawing.Point(19, 153);
		this.label_D.Name = "label_D";
		this.label_D.Size = new System.Drawing.Size(14, 12);
		this.label_D.TabIndex = 69;
		this.label_D.Text = "D";
		this.label_S.AutoSize = true;
		this.label_S.Font = new System.Drawing.Font("MS UI Gothic", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 128);
		this.label_S.Location = new System.Drawing.Point(19, 181);
		this.label_S.Name = "label_S";
		this.label_S.Size = new System.Drawing.Size(13, 12);
		this.label_S.TabIndex = 70;
		this.label_S.Text = "S";
		this.CalcButton.Location = new System.Drawing.Point(114, 176);
		this.CalcButton.Name = "CalcButton";
		this.CalcButton.Size = new System.Drawing.Size(75, 23);
		this.CalcButton.TabIndex = 71;
		this.CalcButton.Text = "計算";
		this.CalcButton.UseVisualStyleBackColor = true;
		this.CalcButton.Click += new System.EventHandler(CalcButton_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(411, 214);
		base.Controls.Add(this.CalcButton);
		base.Controls.Add(this.label_S);
		base.Controls.Add(this.label_D);
		base.Controls.Add(this.label_C);
		base.Controls.Add(this.label_B);
		base.Controls.Add(this.label_A);
		base.Controls.Add(this.label_H);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.textBox1);
		base.Controls.Add(this.NatureBox);
		base.Controls.Add(this.PokemonBox);
		base.Controls.Add(this.LvBox);
		base.Controls.Add(this.Stat_S);
		base.Controls.Add(this.Stat_D);
		base.Controls.Add(this.Stat_C);
		base.Controls.Add(this.Stat_B);
		base.Controls.Add(this.Stat_A);
		base.Controls.Add(this.Stat_H);
		base.Name = "IVsCalcBackForm";
		this.Text = "IVsCalcBackForm";
		((System.ComponentModel.ISupportInitialize)this.Stat_H).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Stat_A).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Stat_B).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Stat_C).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Stat_D).EndInit();
		((System.ComponentModel.ISupportInitialize)this.Stat_S).EndInit();
		((System.ComponentModel.ISupportInitialize)this.LvBox).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
