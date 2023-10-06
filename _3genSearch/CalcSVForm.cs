using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace _3genSearch;

public class CalcSVForm : Form
{
	private IContainer components;

	private Label label3;

	private Label label2;

	private Label label1;

	private TextBox PID;

	private NumericUpDown TID;

	private NumericUpDown SID;

	private TextBox textBox1;

	private Button button1;

	public CalcSVForm()
	{
		InitializeComponent();
	}

	private void Button1_Click(object sender, EventArgs e)
	{
		if (!Regex.IsMatch(PID.Text, "^[0-9a-fA-F]{1,8}$"))
		{
			MessageBox.Show("PIDが不正です");
			return;
		}
		uint num = (Util.GetValueHex(PID) & 0xFFFFu) ^ (Util.GetValueHex(PID) >> 16) ^ Util.GetValueDec(TID) ^ Util.GetValueDec(SID);
		TextBox textBox = textBox1;
		object obj;
		switch (num)
		{
		case 1u:
		case 2u:
		case 3u:
		case 4u:
		case 5u:
		case 6u:
		case 7u:
			obj = "☆";
			break;
		case 0u:
			obj = "◇";
			break;
		default:
			obj = "色違いじゃないです.";
			break;
		}
		textBox.Text = (string)obj;
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
		this.label2 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.PID = new System.Windows.Forms.TextBox();
		this.TID = new System.Windows.Forms.NumericUpDown();
		this.SID = new System.Windows.Forms.NumericUpDown();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.button1 = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.TID).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.SID).BeginInit();
		base.SuspendLayout();
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(15, 43);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(23, 12);
		this.label3.TabIndex = 13;
		this.label3.Text = "SID";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(15, 71);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(23, 12);
		this.label2.TabIndex = 12;
		this.label2.Text = "PID";
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(15, 15);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(23, 12);
		this.label1.TabIndex = 11;
		this.label1.Text = "TID";
		this.PID.Location = new System.Drawing.Point(44, 68);
		this.PID.Name = "PID";
		this.PID.Size = new System.Drawing.Size(100, 19);
		this.PID.TabIndex = 9;
		this.TID.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TID.Location = new System.Drawing.Point(44, 12);
		this.TID.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.TID.Name = "TID";
		this.TID.Size = new System.Drawing.Size(100, 22);
		this.TID.TabIndex = 8;
		this.SID.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.SID.Location = new System.Drawing.Point(44, 40);
		this.SID.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.SID.Name = "SID";
		this.SID.Size = new System.Drawing.Size(100, 22);
		this.SID.TabIndex = 14;
		this.textBox1.Location = new System.Drawing.Point(150, 40);
		this.textBox1.Multiline = true;
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(100, 47);
		this.textBox1.TabIndex = 15;
		this.button1.Location = new System.Drawing.Point(150, 10);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(100, 23);
		this.button1.TabIndex = 16;
		this.button1.Text = "計算";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(Button1_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(262, 104);
		base.Controls.Add(this.button1);
		base.Controls.Add(this.textBox1);
		base.Controls.Add(this.SID);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.PID);
		base.Controls.Add(this.TID);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.MaximizeBox = false;
		base.Name = "CalcSVForm";
		base.ShowIcon = false;
		this.Text = "CalcSVForm";
		((System.ComponentModel.ISupportInitialize)this.TID).EndInit();
		((System.ComponentModel.ISupportInitialize)this.SID).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
