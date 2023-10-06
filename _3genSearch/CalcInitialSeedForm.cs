using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using PokemonPRNG.LCG32.StandardLCG;

namespace _3genSearch;

public class CalcInitialSeedForm : Form
{
	private IContainer components;

	private GroupBox groupBox3;

	private CheckBox checkBox9;

	private Button CalcButton2_back;

	private NumericUpDown LastFrame_back;

	private NumericUpDown FirstFrame_back;

	private Label label193;

	private Label label194;

	private Label label192;

	private Label label32;

	private TextBox InitialSeedBox_back;

	private TextBox TargetSeedBox_back;

	private DataGridView dataGridView2_back;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn30;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn57;

	private DataGridViewTextBoxColumn dataGridViewTextBoxColumn76;

	public CalcInitialSeedForm()
	{
		InitializeComponent();
		dataGridView2_back.DefaultCellStyle.Font = new Font("Consolas", 9f);
		PropertyInfo property = typeof(DataGridView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
		property.SetValue(dataGridView2_back, true, null);
	}

	private void Calc_InitialSeed()
	{
		List<uint> list = (checkBox9.Checked ? (from _ in Enumerable.Range(0, 65536)
			select (uint)_).ToList() : Util.GetHexList(InitialSeedBox_back));
		List<uint> hexList = Util.GetHexList(TargetSeedBox_back);
		uint valueDec = Util.GetValueDec(FirstFrame_back);
		uint valueDec2 = Util.GetValueDec(LastFrame_back);
		foreach (uint item in list)
		{
			foreach (uint item2 in hexList)
			{
				uint index = item2.GetIndex(item);
				if (valueDec <= index && index <= valueDec2)
				{
					DataGridViewRow dataGridViewRow = new DataGridViewRow();
					dataGridViewRow.CreateCells(dataGridView2_back);
					dataGridViewRow.SetValues($"{item:X}", $"{index}", $"{item2:X8}");
					dataGridView2_back.Rows.Add(dataGridViewRow);
				}
			}
		}
	}

	private void CalcButton2_back_Click(object sender, EventArgs e)
	{
		dataGridView2_back.Rows.Clear();
		Calc_InitialSeed();
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
		this.groupBox3 = new System.Windows.Forms.GroupBox();
		this.checkBox9 = new System.Windows.Forms.CheckBox();
		this.CalcButton2_back = new System.Windows.Forms.Button();
		this.LastFrame_back = new System.Windows.Forms.NumericUpDown();
		this.FirstFrame_back = new System.Windows.Forms.NumericUpDown();
		this.label193 = new System.Windows.Forms.Label();
		this.label194 = new System.Windows.Forms.Label();
		this.label192 = new System.Windows.Forms.Label();
		this.label32 = new System.Windows.Forms.Label();
		this.InitialSeedBox_back = new System.Windows.Forms.TextBox();
		this.TargetSeedBox_back = new System.Windows.Forms.TextBox();
		this.dataGridView2_back = new System.Windows.Forms.DataGridView();
		this.dataGridViewTextBoxColumn30 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn57 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewTextBoxColumn76 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.groupBox3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.LastFrame_back).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.FirstFrame_back).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dataGridView2_back).BeginInit();
		base.SuspendLayout();
		this.groupBox3.Controls.Add(this.checkBox9);
		this.groupBox3.Controls.Add(this.LastFrame_back);
		this.groupBox3.Controls.Add(this.FirstFrame_back);
		this.groupBox3.Controls.Add(this.label193);
		this.groupBox3.Controls.Add(this.label194);
		this.groupBox3.Controls.Add(this.label192);
		this.groupBox3.Controls.Add(this.label32);
		this.groupBox3.Controls.Add(this.InitialSeedBox_back);
		this.groupBox3.Controls.Add(this.TargetSeedBox_back);
		this.groupBox3.Location = new System.Drawing.Point(12, 12);
		this.groupBox3.Name = "groupBox3";
		this.groupBox3.Size = new System.Drawing.Size(261, 257);
		this.groupBox3.TabIndex = 299;
		this.groupBox3.TabStop = false;
		this.checkBox9.AutoSize = true;
		this.checkBox9.Location = new System.Drawing.Point(143, 36);
		this.checkBox9.Name = "checkBox9";
		this.checkBox9.Size = new System.Drawing.Size(108, 16);
		this.checkBox9.TabIndex = 306;
		this.checkBox9.Text = "初期seed全探索";
		this.checkBox9.UseVisualStyleBackColor = true;
		this.CalcButton2_back.Location = new System.Drawing.Point(279, 240);
		this.CalcButton2_back.Name = "CalcButton2_back";
		this.CalcButton2_back.Size = new System.Drawing.Size(75, 23);
		this.CalcButton2_back.TabIndex = 305;
		this.CalcButton2_back.Text = "計算";
		this.CalcButton2_back.UseVisualStyleBackColor = true;
		this.CalcButton2_back.Click += new System.EventHandler(CalcButton2_back_Click);
		this.LastFrame_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.LastFrame_back.Location = new System.Drawing.Point(143, 209);
		this.LastFrame_back.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.LastFrame_back.Name = "LastFrame_back";
		this.LastFrame_back.Size = new System.Drawing.Size(93, 22);
		this.LastFrame_back.TabIndex = 304;
		this.LastFrame_back.Value = new decimal(new int[4] { 10000000, 0, 0, 0 });
		this.FirstFrame_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.FirstFrame_back.Location = new System.Drawing.Point(22, 209);
		this.FirstFrame_back.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.FirstFrame_back.Name = "FirstFrame_back";
		this.FirstFrame_back.Size = new System.Drawing.Size(93, 22);
		this.FirstFrame_back.TabIndex = 303;
		this.label193.Location = new System.Drawing.Point(119, 214);
		this.label193.Name = "label193";
		this.label193.Size = new System.Drawing.Size(18, 16);
		this.label193.TabIndex = 302;
		this.label193.Text = "～";
		this.label194.AutoSize = true;
		this.label194.Location = new System.Drawing.Point(7, 188);
		this.label194.Name = "label194";
		this.label194.Size = new System.Drawing.Size(20, 12);
		this.label194.TabIndex = 301;
		this.label194.Text = "[F]";
		this.label192.AutoSize = true;
		this.label192.Location = new System.Drawing.Point(7, 15);
		this.label192.Name = "label192";
		this.label192.Size = new System.Drawing.Size(53, 12);
		this.label192.TabIndex = 300;
		this.label192.Text = "初期seed";
		this.label32.AutoSize = true;
		this.label32.Location = new System.Drawing.Point(7, 102);
		this.label32.Name = "label32";
		this.label32.Size = new System.Drawing.Size(53, 12);
		this.label32.TabIndex = 299;
		this.label32.Text = "目標seed";
		this.InitialSeedBox_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.InitialSeedBox_back.Location = new System.Drawing.Point(22, 36);
		this.InitialSeedBox_back.Multiline = true;
		this.InitialSeedBox_back.Name = "InitialSeedBox_back";
		this.InitialSeedBox_back.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.InitialSeedBox_back.Size = new System.Drawing.Size(100, 56);
		this.InitialSeedBox_back.TabIndex = 298;
		this.InitialSeedBox_back.Text = "0";
		this.TargetSeedBox_back.Font = new System.Drawing.Font("Consolas", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TargetSeedBox_back.Location = new System.Drawing.Point(22, 123);
		this.TargetSeedBox_back.Multiline = true;
		this.TargetSeedBox_back.Name = "TargetSeedBox_back";
		this.TargetSeedBox_back.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.TargetSeedBox_back.Size = new System.Drawing.Size(100, 56);
		this.TargetSeedBox_back.TabIndex = 297;
		this.dataGridView2_back.AllowUserToAddRows = false;
		this.dataGridView2_back.AllowUserToDeleteRows = false;
		this.dataGridView2_back.AllowUserToResizeRows = false;
		dataGridViewCellStyle.BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
		this.dataGridView2_back.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle;
		this.dataGridView2_back.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGridView2_back.Columns.AddRange(this.dataGridViewTextBoxColumn30, this.dataGridViewTextBoxColumn57, this.dataGridViewTextBoxColumn76);
		this.dataGridView2_back.Location = new System.Drawing.Point(279, 18);
		this.dataGridView2_back.Name = "dataGridView2_back";
		this.dataGridView2_back.ReadOnly = true;
		this.dataGridView2_back.RowHeadersWidth = 30;
		this.dataGridView2_back.RowTemplate.Height = 21;
		this.dataGridView2_back.Size = new System.Drawing.Size(322, 216);
		this.dataGridView2_back.TabIndex = 300;
		this.dataGridView2_back.TabStop = false;
		this.dataGridView2_back.MouseDown += new System.Windows.Forms.MouseEventHandler(dataGridView_MouseDown);
		this.dataGridViewTextBoxColumn30.HeaderText = "初期seed";
		this.dataGridViewTextBoxColumn30.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn30.Name = "dataGridViewTextBoxColumn30";
		this.dataGridViewTextBoxColumn30.ReadOnly = true;
		this.dataGridViewTextBoxColumn30.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.dataGridViewTextBoxColumn30.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.dataGridViewTextBoxColumn30.Width = 80;
		this.dataGridViewTextBoxColumn57.HeaderText = "F";
		this.dataGridViewTextBoxColumn57.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn57.Name = "dataGridViewTextBoxColumn57";
		this.dataGridViewTextBoxColumn57.ReadOnly = true;
		this.dataGridViewTextBoxColumn57.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.dataGridViewTextBoxColumn57.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.dataGridViewTextBoxColumn57.Width = 80;
		this.dataGridViewTextBoxColumn76.HeaderText = "seed";
		this.dataGridViewTextBoxColumn76.MinimumWidth = 10;
		this.dataGridViewTextBoxColumn76.Name = "dataGridViewTextBoxColumn76";
		this.dataGridViewTextBoxColumn76.ReadOnly = true;
		this.dataGridViewTextBoxColumn76.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.dataGridViewTextBoxColumn76.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
		this.dataGridViewTextBoxColumn76.Width = 80;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(608, 275);
		base.Controls.Add(this.dataGridView2_back);
		base.Controls.Add(this.CalcButton2_back);
		base.Controls.Add(this.groupBox3);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.MaximizeBox = false;
		base.Name = "CalcInitialSeedForm";
		base.ShowIcon = false;
		this.Text = "CalcInitialSeedForm";
		this.groupBox3.ResumeLayout(false);
		this.groupBox3.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.LastFrame_back).EndInit();
		((System.ComponentModel.ISupportInitialize)this.FirstFrame_back).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dataGridView2_back).EndInit();
		base.ResumeLayout(false);
	}
}
