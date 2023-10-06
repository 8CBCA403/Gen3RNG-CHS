using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace _3genSearch;

public class CalcSIDForm : Form
{
	private IContainer components;

	private NumericUpDown TID;

	private TextBox PID;

	private TextBox SID;

	private Label label1;

	private Label label2;

	private Label label3;

	public CalcSIDForm()
	{
		InitializeComponent();
	}

	private void PID_TextChanged(object sender, EventArgs e)
	{
		if (Regex.IsMatch(PID.Text, "^[0-9a-fA-F]{1,8}$"))
		{
			uint num = Util.GetValueHex(PID) & 0xFFFFu;
			uint num2 = Util.GetValueHex(PID) >> 16;
			SID.Text = $"{(num ^ num2 ^ Util.GetValueDec(TID)) & 0xFFF8u}";
		}
	}

	private void TID_ValueChanged(object sender, EventArgs e)
	{
		if (Regex.IsMatch(PID.Text, "^[0-9a-fA-F]{1,8}$"))
		{
			uint num = Util.GetValueHex(PID) & 0xFFFFu;
			uint num2 = Util.GetValueHex(PID) >> 16;
			SID.Text = $"{(num ^ num2 ^ Util.GetValueDec(TID)) & 0xFFF8u}";
		}
	}

	private void CalcSIDForm_Load(object sender, EventArgs e)
	{
		PID.Text = "DEADFACE";
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
		this.TID = new System.Windows.Forms.NumericUpDown();
		this.PID = new System.Windows.Forms.TextBox();
		this.SID = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.TID).BeginInit();
		base.SuspendLayout();
		this.TID.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TID.Location = new System.Drawing.Point(47, 12);
		this.TID.Maximum = new decimal(new int[4] { 65535, 0, 0, 0 });
		this.TID.Name = "TID";
		this.TID.Size = new System.Drawing.Size(100, 22);
		this.TID.TabIndex = 2;
		this.TID.ValueChanged += new System.EventHandler(TID_ValueChanged);
		this.PID.Location = new System.Drawing.Point(47, 40);
		this.PID.Name = "PID";
		this.PID.Size = new System.Drawing.Size(100, 19);
		this.PID.TabIndex = 3;
		this.PID.TextChanged += new System.EventHandler(PID_TextChanged);
		this.SID.Location = new System.Drawing.Point(47, 65);
		this.SID.Name = "SID";
		this.SID.ReadOnly = true;
		this.SID.Size = new System.Drawing.Size(100, 19);
		this.SID.TabIndex = 4;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(18, 15);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(23, 12);
		this.label1.TabIndex = 5;
		this.label1.Text = "TID";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(18, 43);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(23, 12);
		this.label2.TabIndex = 6;
		this.label2.Text = "PID";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(18, 68);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(23, 12);
		this.label3.TabIndex = 7;
		this.label3.Text = "SID";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(174, 107);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.SID);
		base.Controls.Add(this.PID);
		base.Controls.Add(this.TID);
		base.MaximizeBox = false;
		base.Name = "CalcSIDForm";
		base.ShowIcon = false;
		this.Text = "CalcSID";
		base.Load += new System.EventHandler(CalcSIDForm_Load);
		((System.ComponentModel.ISupportInitialize)this.TID).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
