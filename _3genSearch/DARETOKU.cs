using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace _3genSearch;

public class DARETOKU : Form
{
	private string[] Berrys = new string[10] { "クラボ", "カゴ", "モモン", "チーゴ", "ナナシ", "ヒメリ", "オレン", "キー", "ラム", "オボン" };

	private string[] RareBerrys = new string[10] { "ザロク", "ネコブ", "タポル", "ロメ", "ウブ", "マトマ", "モコシ", "ゴス", "ラブタ", "ノメル" };

	private string[] RareBerrys2 = new string[5] { "ズリ", "ブリー", "ナナ", "セシナ", "パイル" };

	private string[] ConfusionBerrys = new string[2] { "フィラ", "イア" };

	private IContainer components;

	private ComboBox comboBox1;

	private Button button1;

	public DARETOKU()
	{
		InitializeComponent();
	}

	private void Button1_Click(object sender, EventArgs e)
	{
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
		this.comboBox1 = new System.Windows.Forms.ComboBox();
		this.button1 = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBox1.FormattingEnabled = true;
		this.comboBox1.Items.AddRange(new object[5] { "サン・トウカ", "114番道路", "きのみ名人", "ミナモシティ", "ルネシティ" });
		this.comboBox1.Location = new System.Drawing.Point(12, 12);
		this.comboBox1.Name = "comboBox1";
		this.comboBox1.Size = new System.Drawing.Size(121, 20);
		this.comboBox1.TabIndex = 0;
		this.button1.Location = new System.Drawing.Point(12, 38);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(75, 23);
		this.button1.TabIndex = 1;
		this.button1.Text = "button1";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(Button1_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(800, 450);
		base.Controls.Add(this.button1);
		base.Controls.Add(this.comboBox1);
		base.Name = "DARETOKU";
		this.Text = "DARETOKU";
		base.ResumeLayout(false);
	}
}
