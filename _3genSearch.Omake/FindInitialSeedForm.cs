using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Pokemon3genRNGLibrary;
using Pokemon3genRNGLibrary.MapData;
using PokemonPRNG.LCG32;
using PokemonPRNG.LCG32.StandardLCG;
using PokemonStandardLibrary.CommonExtension;
using PokemonStandardLibrary.Gen3;

namespace _3genSearch.Omake;

public class FindInitialSeedForm : Form
{
	private IContainer components;

	private Label label3;

	private NumericUpDown MaxFrameBox;

	private NumericUpDown MinFrameBox;

	private NumericUpDown RangeBox;

	private Label label2;

	private Label label1;

	private TextBox SeedBox;

	private Label label30;

	private NumericUpDown HBox;

	private NumericUpDown DBox;

	private NumericUpDown SBox;

	private NumericUpDown CBox;

	private NumericUpDown BBox;

	private NumericUpDown ABox;

	private Label label4;

	private Label label5;

	private Label label9;

	private Label label10;

	private Label label28;

	private ComboBox MapBox;

	private Button CalcButton;

	private TextBox ResultBox;

	public FindInitialSeedForm()
	{
		InitializeComponent();
		ComboBox.ObjectCollection items = MapBox.Items;
		object[] items2 = (from _ in EmMapData.Field.SelectMap()
			select _.MapName).ToArray();
		items.AddRange(items2);
		MapBox.SelectedIndex = 0;
	}

	private void button1_Click(object sender, EventArgs e)
	{
		uint num = SeedBox.Seed() ?? 65536;
		if (num == 65536)
		{
			return;
		}
		uint num2 = RangeBox.UValue();
		uint num3 = MinFrameBox.UValue();
		uint num4 = MaxFrameBox.UValue();
		GBAMap gBAMap = EmMapData.Field.SelectMap(MapBox.Text).FirstOrDefault();
		if (gBAMap == null)
		{
			return;
		}
		uint[] second = new NumericUpDown[6] { HBox, ABox, BBox, CBox, DBox, SBox }.Select((NumericUpDown _) => (uint)_.Value).ToArray();
		StringBuilder stringBuilder = new StringBuilder();
		WildGenerator igenerator = new WildGenerator(gBAMap, new WildGenerationArgument
		{
			ForceEncounter = true,
			GenerateMethod = PriorInterruptIVsGenerator.GetInstance()
		});
		WildGenerator igenerator2 = new WildGenerator(gBAMap, new WildGenerationArgument
		{
			ForceEncounter = true,
			GenerateMethod = MiddleInterruptedIVsGenerator.GetInstance()
		});
		for (long num5 = 0L - (long)num2; num5 <= num2; num5++)
		{
			uint num6 = (uint)(int)(num + num5) & 0xFFFFu;
			foreach (var (num7, rNGResult) in CommonEnumerator.WithIndex<RNGResult<Pokemon.Individual, uint>>(num6.EnumerateSeed().EnumerateGeneration(igenerator).Skip((int)num3)
				.Take((int)(num4 - num3 + 1))))
			{
				if (rNGResult.Content.Stats.SequenceEqual(second))
				{
					string text = ((num5 > 0) ? "+" : "");
					stringBuilder.AppendLine($"{num6:X4} ({text}{num5}F) {num7 + num3}[F] {rNGResult.Content.Name} Lv.{rNGResult.Content.Lv} {rNGResult.Content.Gender.ToSymbol()} {rNGResult.Content.Ability} {rNGResult.Content.Nature.ToJapanese()}");
				}
			}
			foreach (var (num8, rNGResult2) in CommonEnumerator.WithIndex<RNGResult<Pokemon.Individual, uint>>(num6.EnumerateSeed().EnumerateGeneration(igenerator2).Skip((int)num3)
				.Take((int)(num4 - num3 + 1))))
			{
				if (rNGResult2.Content.Stats.SequenceEqual(second))
				{
					string text2 = ((num5 > 0) ? "+" : "");
					stringBuilder.AppendLine($"{num6:X4} ({text2}{num5}F) {num8 + num3}[F] {rNGResult2.Content.Name} Lv.{rNGResult2.Content.Lv} {rNGResult2.Content.Gender.ToSymbol()} {rNGResult2.Content.Ability} {rNGResult2.Content.Nature.ToJapanese()}");
				}
			}
		}
		ResultBox.Text = stringBuilder.ToString();
	}

	private void TextBox_SelectText(object sender, EventArgs e)
	{
		((TextBox)sender).SelectAll();
	}

	private void NumericUpDown_SelectValue(object sender, EventArgs e)
	{
		((NumericUpDown)sender).Select(0, ((NumericUpDown)sender).Text.Length);
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
		this.label3 = new System.Windows.Forms.Label();
		this.MaxFrameBox = new System.Windows.Forms.NumericUpDown();
		this.MinFrameBox = new System.Windows.Forms.NumericUpDown();
		this.RangeBox = new System.Windows.Forms.NumericUpDown();
		this.label2 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.SeedBox = new System.Windows.Forms.TextBox();
		this.label30 = new System.Windows.Forms.Label();
		this.HBox = new System.Windows.Forms.NumericUpDown();
		this.DBox = new System.Windows.Forms.NumericUpDown();
		this.SBox = new System.Windows.Forms.NumericUpDown();
		this.CBox = new System.Windows.Forms.NumericUpDown();
		this.BBox = new System.Windows.Forms.NumericUpDown();
		this.ABox = new System.Windows.Forms.NumericUpDown();
		this.label4 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.label10 = new System.Windows.Forms.Label();
		this.label28 = new System.Windows.Forms.Label();
		this.MapBox = new System.Windows.Forms.ComboBox();
		this.ResultBox = new System.Windows.Forms.TextBox();
		this.CalcButton = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.MaxFrameBox).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.MinFrameBox).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.RangeBox).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.HBox).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.DBox).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SBox).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.CBox).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.BBox).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ABox).BeginInit();
		base.SuspendLayout();
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(158, 43);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(17, 12);
		this.label3.TabIndex = 17;
		this.label3.Text = "～";
		this.MaxFrameBox.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.MaxFrameBox.Location = new System.Drawing.Point(181, 40);
		this.MaxFrameBox.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.MaxFrameBox.Name = "MaxFrameBox";
		this.MaxFrameBox.Size = new System.Drawing.Size(100, 22);
		this.MaxFrameBox.TabIndex = 3;
		this.MaxFrameBox.Value = new decimal(new int[4] { 3500, 0, 0, 0 });
		this.MaxFrameBox.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.MinFrameBox.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.MinFrameBox.Location = new System.Drawing.Point(52, 40);
		this.MinFrameBox.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.MinFrameBox.Name = "MinFrameBox";
		this.MinFrameBox.Size = new System.Drawing.Size(100, 22);
		this.MinFrameBox.TabIndex = 2;
		this.MinFrameBox.Value = new decimal(new int[4] { 2000, 0, 0, 0 });
		this.MinFrameBox.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.RangeBox.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.RangeBox.Location = new System.Drawing.Point(181, 12);
		this.RangeBox.Maximum = new decimal(new int[4] { 32767, 0, 0, 0 });
		this.RangeBox.Name = "RangeBox";
		this.RangeBox.Size = new System.Drawing.Size(73, 22);
		this.RangeBox.TabIndex = 1;
		this.RangeBox.Value = new decimal(new int[4] { 30, 0, 0, 0 });
		this.RangeBox.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(158, 15);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(17, 12);
		this.label2.TabIndex = 13;
		this.label2.Text = "±";
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(17, 15);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(29, 12);
		this.label1.TabIndex = 12;
		this.label1.Text = "目標";
		this.SeedBox.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.SeedBox.Location = new System.Drawing.Point(52, 12);
		this.SeedBox.Name = "SeedBox";
		this.SeedBox.Size = new System.Drawing.Size(100, 22);
		this.SeedBox.TabIndex = 0;
		this.SeedBox.Enter += new System.EventHandler(TextBox_SelectText);
		this.label30.AutoSize = true;
		this.label30.Location = new System.Drawing.Point(33, 137);
		this.label30.Name = "label30";
		this.label30.Size = new System.Drawing.Size(13, 12);
		this.label30.TabIndex = 38;
		this.label30.Text = "H";
		this.HBox.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.HBox.Location = new System.Drawing.Point(52, 134);
		this.HBox.Maximum = new decimal(new int[4] { 999, 0, 0, 0 });
		this.HBox.Name = "HBox";
		this.HBox.Size = new System.Drawing.Size(48, 22);
		this.HBox.TabIndex = 11;
		this.HBox.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.DBox.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.DBox.Location = new System.Drawing.Point(125, 166);
		this.DBox.Maximum = new decimal(new int[4] { 999, 0, 0, 0 });
		this.DBox.Name = "DBox";
		this.DBox.Size = new System.Drawing.Size(48, 22);
		this.DBox.TabIndex = 15;
		this.DBox.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.SBox.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.SBox.Location = new System.Drawing.Point(124, 198);
		this.SBox.Maximum = new decimal(new int[4] { 999, 0, 0, 0 });
		this.SBox.Name = "SBox";
		this.SBox.Size = new System.Drawing.Size(48, 22);
		this.SBox.TabIndex = 16;
		this.SBox.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.CBox.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.CBox.Location = new System.Drawing.Point(125, 134);
		this.CBox.Maximum = new decimal(new int[4] { 999, 0, 0, 0 });
		this.CBox.Name = "CBox";
		this.CBox.Size = new System.Drawing.Size(48, 22);
		this.CBox.TabIndex = 14;
		this.CBox.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.BBox.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.BBox.Location = new System.Drawing.Point(52, 198);
		this.BBox.Maximum = new decimal(new int[4] { 999, 0, 0, 0 });
		this.BBox.Name = "BBox";
		this.BBox.Size = new System.Drawing.Size(48, 22);
		this.BBox.TabIndex = 13;
		this.BBox.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.ABox.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ABox.Location = new System.Drawing.Point(52, 166);
		this.ABox.Maximum = new decimal(new int[4] { 999, 0, 0, 0 });
		this.ABox.Name = "ABox";
		this.ABox.Size = new System.Drawing.Size(48, 22);
		this.ABox.TabIndex = 12;
		this.ABox.Enter += new System.EventHandler(NumericUpDown_SelectValue);
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(106, 201);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(12, 12);
		this.label4.TabIndex = 48;
		this.label4.Text = "S";
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(106, 169);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(13, 12);
		this.label5.TabIndex = 46;
		this.label5.Text = "D";
		this.label9.AutoSize = true;
		this.label9.Location = new System.Drawing.Point(106, 137);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(13, 12);
		this.label9.TabIndex = 44;
		this.label9.Text = "C";
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(33, 201);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(13, 12);
		this.label10.TabIndex = 42;
		this.label10.Text = "B";
		this.label28.AutoSize = true;
		this.label28.Location = new System.Drawing.Point(33, 169);
		this.label28.Name = "label28";
		this.label28.Size = new System.Drawing.Size(13, 12);
		this.label28.TabIndex = 40;
		this.label28.Text = "A";
		this.MapBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.MapBox.FormattingEnabled = true;
		this.MapBox.Location = new System.Drawing.Point(19, 77);
		this.MapBox.Name = "MapBox";
		this.MapBox.Size = new System.Drawing.Size(154, 20);
		this.MapBox.TabIndex = 0;
		this.MapBox.TabStop = false;
		this.ResultBox.Location = new System.Drawing.Point(181, 77);
		this.ResultBox.Multiline = true;
		this.ResultBox.Name = "ResultBox";
		this.ResultBox.Size = new System.Drawing.Size(359, 187);
		this.ResultBox.TabIndex = 0;
		this.ResultBox.TabStop = false;
		this.CalcButton.Location = new System.Drawing.Point(52, 226);
		this.CalcButton.Name = "CalcButton";
		this.CalcButton.Size = new System.Drawing.Size(120, 38);
		this.CalcButton.TabIndex = 20;
		this.CalcButton.Text = "button1";
		this.CalcButton.UseVisualStyleBackColor = true;
		this.CalcButton.Click += new System.EventHandler(button1_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(558, 279);
		base.Controls.Add(this.ResultBox);
		base.Controls.Add(this.CalcButton);
		base.Controls.Add(this.MapBox);
		base.Controls.Add(this.label30);
		base.Controls.Add(this.HBox);
		base.Controls.Add(this.DBox);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.MaxFrameBox);
		base.Controls.Add(this.SBox);
		base.Controls.Add(this.MinFrameBox);
		base.Controls.Add(this.CBox);
		base.Controls.Add(this.RangeBox);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.BBox);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.SeedBox);
		base.Controls.Add(this.ABox);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.label28);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.label10);
		base.Controls.Add(this.label9);
		base.Name = "FindInitialSeedForm";
		this.Text = "FindInitialSeedForm";
		((System.ComponentModel.ISupportInitialize)this.MaxFrameBox).EndInit();
		((System.ComponentModel.ISupportInitialize)this.MinFrameBox).EndInit();
		((System.ComponentModel.ISupportInitialize)this.RangeBox).EndInit();
		((System.ComponentModel.ISupportInitialize)this.HBox).EndInit();
		((System.ComponentModel.ISupportInitialize)this.DBox).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SBox).EndInit();
		((System.ComponentModel.ISupportInitialize)this.CBox).EndInit();
		((System.ComponentModel.ISupportInitialize)this.BBox).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ABox).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
