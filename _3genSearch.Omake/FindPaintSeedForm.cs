using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Pokemon3genRNGLibrary.Frontier;
using PokemonPRNG.LCG32;
using PokemonPRNG.LCG32.StandardLCG;

namespace _3genSearch.Omake;

public class FindPaintSeedForm : Form
{
	private IContainer components;

	private TextBox textBox1;

	private Label label1;

	private Label label2;

	private NumericUpDown numericUpDown1;

	private TextBox textBox2;

	private TextBox textBox3;

	private TextBox textBox4;

	private Button button1;

	private NumericUpDown numericUpDown2;

	private NumericUpDown numericUpDown3;

	private Label label3;

	private TextBox textBox5;

	public FindPaintSeedForm()
	{
		InitializeComponent();
	}

	private void button1_Click(object sender, EventArgs e)
	{
		uint num = textBox1.Seed() ?? 65536;
		if (num == 65536)
		{
			return;
		}
		uint num2 = numericUpDown1.UValue();
		uint num3 = numericUpDown2.UValue();
		uint num4 = numericUpDown3.UValue();
		string[] array = new TextBox[3] { textBox2, textBox3, textBox4 }.Select((TextBox _) => _.Text).ToArray();
		FactoryGenerator igenerator = new FactoryGenerator(1,false);
		StringBuilder stringBuilder = new StringBuilder();
		for (long num5 = 0L - (long)num2; num5 <= num2; num5++)
		{
			uint num6 = (uint)(int)(num + num5) & 0xFFFFu;
			foreach (var item3 in CommonEnumerator.WithIndex<FactoryStarterResult>(num6.EnumerateSeed().EnumerateGeneration(igenerator).Skip((int)num3)
				.Take((int)(num4 - num3 + 1))))
			{
				int item = item3.Item1;
				FactoryStarterResult item2 = item3.Item2;
				FrontierPokemon[] rentalPokemons = item2.RentalPokemons;
				if (!(rentalPokemons[0].Species.Name != array[0]) && !(rentalPokemons[1].Species.Name != array[1]) && !(rentalPokemons[2].Species.Name != array[2]))
				{
					string text = ((num5 > 0) ? "+" : "");
					stringBuilder.AppendLine(string.Format("{0:X4} ({1}{2}F) {3}[F] {4}", num6, text, num5, item + num3, string.Join(" ", rentalPokemons.Select((FrontierPokemon _) => _.Species.Name))));
				}
			}
		}
		textBox5.Text = stringBuilder.ToString();
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
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
		this.textBox2 = new System.Windows.Forms.TextBox();
		this.textBox3 = new System.Windows.Forms.TextBox();
		this.textBox4 = new System.Windows.Forms.TextBox();
		this.button1 = new System.Windows.Forms.Button();
		this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
		this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
		this.label3 = new System.Windows.Forms.Label();
		this.textBox5 = new System.Windows.Forms.TextBox();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown3).BeginInit();
		base.SuspendLayout();
		this.textBox1.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.textBox1.Location = new System.Drawing.Point(54, 24);
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(100, 22);
		this.textBox1.TabIndex = 0;
		this.textBox1.Enter += new System.EventHandler(TextBox_SelectText);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(19, 27);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(29, 12);
		this.label1.TabIndex = 1;
		this.label1.Text = "目標";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(160, 27);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(17, 12);
		this.label2.TabIndex = 2;
		this.label2.Text = "±";
		this.numericUpDown1.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.numericUpDown1.Location = new System.Drawing.Point(183, 24);
		this.numericUpDown1.Maximum = new decimal(new int[4] { 32767, 0, 0, 0 });
		this.numericUpDown1.Name = "numericUpDown1";
		this.numericUpDown1.Size = new System.Drawing.Size(73, 22);
		this.numericUpDown1.TabIndex = 3;
		this.numericUpDown1.Value = new decimal(new int[4] { 30, 0, 0, 0 });
		this.textBox2.Location = new System.Drawing.Point(54, 98);
		this.textBox2.Name = "textBox2";
		this.textBox2.Size = new System.Drawing.Size(100, 19);
		this.textBox2.TabIndex = 4;
		this.textBox2.Enter += new System.EventHandler(TextBox_SelectText);
		this.textBox3.Location = new System.Drawing.Point(54, 123);
		this.textBox3.Name = "textBox3";
		this.textBox3.Size = new System.Drawing.Size(100, 19);
		this.textBox3.TabIndex = 5;
		this.textBox3.Enter += new System.EventHandler(TextBox_SelectText);
		this.textBox4.Location = new System.Drawing.Point(54, 148);
		this.textBox4.Name = "textBox4";
		this.textBox4.Size = new System.Drawing.Size(100, 19);
		this.textBox4.TabIndex = 6;
		this.textBox4.Enter += new System.EventHandler(TextBox_SelectText);
		this.button1.Location = new System.Drawing.Point(54, 173);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(100, 38);
		this.button1.TabIndex = 7;
		this.button1.Text = "button1";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(button1_Click);
		this.numericUpDown2.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.numericUpDown2.Location = new System.Drawing.Point(54, 52);
		this.numericUpDown2.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.numericUpDown2.Name = "numericUpDown2";
		this.numericUpDown2.Size = new System.Drawing.Size(100, 22);
		this.numericUpDown2.TabIndex = 8;
		this.numericUpDown2.Value = new decimal(new int[4] { 2000, 0, 0, 0 });
		this.numericUpDown3.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.numericUpDown3.Location = new System.Drawing.Point(183, 52);
		this.numericUpDown3.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.numericUpDown3.Name = "numericUpDown3";
		this.numericUpDown3.Size = new System.Drawing.Size(100, 22);
		this.numericUpDown3.TabIndex = 9;
		this.numericUpDown3.Value = new decimal(new int[4] { 3500, 0, 0, 0 });
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(160, 55);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(17, 12);
		this.label3.TabIndex = 10;
		this.label3.Text = "～";
		this.textBox5.Location = new System.Drawing.Point(183, 98);
		this.textBox5.Multiline = true;
		this.textBox5.Name = "textBox5";
		this.textBox5.Size = new System.Drawing.Size(359, 113);
		this.textBox5.TabIndex = 11;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(558, 228);
		base.Controls.Add(this.textBox5);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.numericUpDown3);
		base.Controls.Add(this.numericUpDown2);
		base.Controls.Add(this.button1);
		base.Controls.Add(this.textBox4);
		base.Controls.Add(this.textBox3);
		base.Controls.Add(this.textBox2);
		base.Controls.Add(this.numericUpDown1);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.textBox1);
		base.Name = "FindPaintSeedForm";
		this.Text = "FindPaintSeedForm";
		((System.ComponentModel.ISupportInitialize)this.numericUpDown1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown3).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
