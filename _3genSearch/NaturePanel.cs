using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using PokemonStandardLibrary;

namespace _3genSearch;

public class NaturePanel : UserControl
{
	private readonly CheckBox[][] Plus;

	private readonly CheckBox[][] Minus;

	private IContainer components;

	private CheckBox Nature0;

	private CheckBox Nature1;

	private CheckBox Nature2;

	private CheckBox Nature3;

	private CheckBox Nature4;

	private CheckBox Nature9;

	private CheckBox Nature8;

	private CheckBox Nature7;

	private CheckBox Nature6;

	private CheckBox Nature5;

	private CheckBox Nature14;

	private CheckBox Nature13;

	private CheckBox Nature12;

	private CheckBox Nature11;

	private CheckBox Nature10;

	private CheckBox Nature18;

	private CheckBox Nature17;

	private CheckBox Nature16;

	private CheckBox Nature15;

	private CheckBox Nature24;

	private CheckBox Nature23;

	private CheckBox Nature22;

	private CheckBox Nature21;

	private CheckBox Nature20;

	private CheckBox Aplus;

	private CheckBox Bplus;

	private CheckBox Splus;

	private CheckBox Cplus;

	private CheckBox Dplus;

	private CheckBox SelectAll;

	private CheckBox Aminus;

	private CheckBox Bminus;

	private CheckBox Sminus;

	private CheckBox Cminus;

	private CheckBox Dminus;

	private Panel panel1;

	private Panel panel2;

	private Panel panel3;

	private Panel panel4;

	private Panel panel5;

	private Panel panel6;

	private Panel panel7;

	private Panel panel8;

	private Panel panel9;

	private Panel panel10;

	private Panel panel11;

	private Panel panel12;

	private Panel panel13;

	private Panel panel14;

	private Panel panel15;

	private Panel panel16;

	private Panel panel17;

	private Panel panel18;

	private Panel panel19;

	private Panel panel20;

	private Panel panel21;

	private Panel panel22;

	private Panel panel23;

	private Panel panel24;

	private Panel panel25;

	private Panel panel26;

	private Panel panel27;

	private Panel panel28;

	private Panel panel29;

	private Panel panel30;

	private Panel panel31;

	private Panel panel32;

	private CheckBox Nature19;

	private Panel panel33;

	private Panel panel34;

	private Panel panel35;

	private Panel panel36;

	public NaturePanel()
	{
		InitializeComponent();
		Plus = new CheckBox[5][];
		Minus = new CheckBox[5][];
		Plus[0] = new CheckBox[5] { Nature0, Nature1, Nature2, Nature3, Nature4 };
		Plus[1] = new CheckBox[5] { Nature5, Nature6, Nature7, Nature8, Nature9 };
		Plus[2] = new CheckBox[5] { Nature10, Nature11, Nature12, Nature13, Nature14 };
		Plus[3] = new CheckBox[5] { Nature15, Nature16, Nature17, Nature18, Nature19 };
		Plus[4] = new CheckBox[5] { Nature20, Nature21, Nature22, Nature23, Nature24 };
		Minus[0] = new CheckBox[5] { Nature0, Nature5, Nature10, Nature15, Nature20 };
		Minus[1] = new CheckBox[5] { Nature1, Nature6, Nature11, Nature16, Nature21 };
		Minus[2] = new CheckBox[5] { Nature2, Nature7, Nature12, Nature17, Nature22 };
		Minus[3] = new CheckBox[5] { Nature3, Nature8, Nature13, Nature18, Nature23 };
		Minus[4] = new CheckBox[5] { Nature4, Nature9, Nature14, Nature19, Nature24 };
		CheckBox[] array = Plus[0];
		foreach (CheckBox checkBox in array)
		{
			checkBox.Click += AplusNature_Click;
		}
		CheckBox[] array2 = Plus[1];
		foreach (CheckBox checkBox2 in array2)
		{
			checkBox2.Click += BplusNature_Click;
		}
		CheckBox[] array3 = Plus[2];
		foreach (CheckBox checkBox3 in array3)
		{
			checkBox3.Click += SplusNature_Click;
		}
		CheckBox[] array4 = Plus[3];
		foreach (CheckBox checkBox4 in array4)
		{
			checkBox4.Click += CplusNature_Click;
		}
		CheckBox[] array5 = Plus[4];
		foreach (CheckBox checkBox5 in array5)
		{
			checkBox5.Click += DplusNature_Click;
		}
		CheckBox[] array6 = Minus[0];
		foreach (CheckBox checkBox6 in array6)
		{
			checkBox6.Click += AminusNature_Click;
		}
		CheckBox[] array7 = Minus[1];
		foreach (CheckBox checkBox7 in array7)
		{
			checkBox7.Click += BminusNature_Click;
		}
		CheckBox[] array8 = Minus[2];
		foreach (CheckBox checkBox8 in array8)
		{
			checkBox8.Click += SminusNature_Click;
		}
		CheckBox[] array9 = Minus[3];
		foreach (CheckBox checkBox9 in array9)
		{
			checkBox9.Click += CminusNature_Click;
		}
		CheckBox[] array10 = Minus[4];
		foreach (CheckBox checkBox10 in array10)
		{
			checkBox10.Click += DminusNature_Click;
		}
	}

	private void AplusNature_Click(object sender, EventArgs e)
	{
		Aplus.Checked = Plus[0].And();
	}

	private void BplusNature_Click(object sender, EventArgs e)
	{
		Bplus.Checked = Plus[1].And();
	}

	private void SplusNature_Click(object sender, EventArgs e)
	{
		Splus.Checked = Plus[2].And();
	}

	private void CplusNature_Click(object sender, EventArgs e)
	{
		Cplus.Checked = Plus[3].And();
	}

	private void DplusNature_Click(object sender, EventArgs e)
	{
		Dplus.Checked = Plus[4].And();
	}

	private void AminusNature_Click(object sender, EventArgs e)
	{
		Aminus.Checked = Minus[0].And();
	}

	private void BminusNature_Click(object sender, EventArgs e)
	{
		Bminus.Checked = Minus[1].And();
	}

	private void SminusNature_Click(object sender, EventArgs e)
	{
		Sminus.Checked = Minus[2].And();
	}

	private void CminusNature_Click(object sender, EventArgs e)
	{
		Cminus.Checked = Minus[3].And();
	}

	private void DminusNature_Click(object sender, EventArgs e)
	{
		Dminus.Checked = Minus[4].And();
	}

	private void Aplus_Click(object sender, EventArgs e)
	{
		CheckBox[] array = Plus[0];
		foreach (CheckBox checkBox in array)
		{
			checkBox.Checked = Aplus.Checked;
		}
		CheckMinus();
	}

	private void Bplus_Click(object sender, EventArgs e)
	{
		CheckBox[] array = Plus[1];
		foreach (CheckBox checkBox in array)
		{
			checkBox.Checked = Bplus.Checked;
		}
		CheckMinus();
	}

	private void Splus_Click(object sender, EventArgs e)
	{
		CheckBox[] array = Plus[2];
		foreach (CheckBox checkBox in array)
		{
			checkBox.Checked = Splus.Checked;
		}
		CheckMinus();
	}

	private void Cplus_Click(object sender, EventArgs e)
	{
		CheckBox[] array = Plus[3];
		foreach (CheckBox checkBox in array)
		{
			checkBox.Checked = Cplus.Checked;
		}
		CheckMinus();
	}

	private void Dplus_Click(object sender, EventArgs e)
	{
		CheckBox[] array = Plus[4];
		foreach (CheckBox checkBox in array)
		{
			checkBox.Checked = Dplus.Checked;
		}
		CheckMinus();
	}

	private void Aminus_Click(object sender, EventArgs e)
	{
		CheckBox[] array = Minus[0];
		foreach (CheckBox checkBox in array)
		{
			checkBox.Checked = Aminus.Checked;
		}
		CheckPlus();
	}

	private void Bminus_Click(object sender, EventArgs e)
	{
		CheckBox[] array = Minus[1];
		foreach (CheckBox checkBox in array)
		{
			checkBox.Checked = Bminus.Checked;
		}
		CheckPlus();
	}

	private void Sminus_Click(object sender, EventArgs e)
	{
		CheckBox[] array = Minus[2];
		foreach (CheckBox checkBox in array)
		{
			checkBox.Checked = Sminus.Checked;
		}
		CheckPlus();
	}

	private void Cminus_Click(object sender, EventArgs e)
	{
		CheckBox[] array = Minus[3];
		foreach (CheckBox checkBox in array)
		{
			checkBox.Checked = Cminus.Checked;
		}
		CheckPlus();
	}

	private void Dminus_Click(object sender, EventArgs e)
	{
		CheckBox[] array = Minus[4];
		foreach (CheckBox checkBox in array)
		{
			checkBox.Checked = Dminus.Checked;
		}
		CheckPlus();
	}

	private void SelectAll_Click(object sender, EventArgs e)
	{
		CheckBox[][] plus = Plus;
		foreach (CheckBox[] array in plus)
		{
			CheckBox[] array2 = array;
			foreach (CheckBox checkBox in array2)
			{
				checkBox.Checked = SelectAll.Checked;
			}
		}
		CheckPlus();
		CheckMinus();
	}

	private void CheckPlus()
	{
		Aplus.Checked = Plus[0].And();
		Bplus.Checked = Plus[1].And();
		Splus.Checked = Plus[2].And();
		Cplus.Checked = Plus[3].And();
		Dplus.Checked = Plus[4].And();
		SelectAll.Checked = Aplus.Checked & Bplus.Checked & Splus.Checked & Cplus.Checked & Dplus.Checked;
	}

	private void CheckMinus()
	{
		Aminus.Checked = Minus[0].And();
		Bminus.Checked = Minus[1].And();
		Sminus.Checked = Minus[2].And();
		Cminus.Checked = Minus[3].And();
		Dminus.Checked = Minus[4].And();
		SelectAll.Checked = Aminus.Checked & Bminus.Checked & Sminus.Checked & Cminus.Checked & Dminus.Checked;
	}

	public bool[] GetCheckList()
	{
		bool[] array = new bool[25];
		for (int i = 0; i < 5; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				array[i * 5 + j] = Plus[i][j].Checked;
			}
		}
		return array;
	}

	public (Nature[], bool) GetNatureList()
	{
		List<Nature> list = new List<Nature>();
		for (int i = 0; i < 5; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				if (Plus[i][j].Checked)
				{
					list.Add((Nature)(5 * i + j));
				}
			}
		}
		return (list.ToArray(), list.Count != 0 && list.Count != 25);
	}

	public IEnumerable<Nature> GetSelectedNatures()
	{
		for (int i = 0; i < 5; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				if (Plus[i][j].Checked)
				{
					yield return (Nature)(5 * i + j);
				}
			}
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
		this.Nature0 = new System.Windows.Forms.CheckBox();
		this.Nature1 = new System.Windows.Forms.CheckBox();
		this.Nature2 = new System.Windows.Forms.CheckBox();
		this.Nature3 = new System.Windows.Forms.CheckBox();
		this.Nature4 = new System.Windows.Forms.CheckBox();
		this.Nature9 = new System.Windows.Forms.CheckBox();
		this.Nature8 = new System.Windows.Forms.CheckBox();
		this.Nature7 = new System.Windows.Forms.CheckBox();
		this.Nature6 = new System.Windows.Forms.CheckBox();
		this.Nature5 = new System.Windows.Forms.CheckBox();
		this.Nature14 = new System.Windows.Forms.CheckBox();
		this.Nature13 = new System.Windows.Forms.CheckBox();
		this.Nature12 = new System.Windows.Forms.CheckBox();
		this.Nature11 = new System.Windows.Forms.CheckBox();
		this.Nature10 = new System.Windows.Forms.CheckBox();
		this.Nature18 = new System.Windows.Forms.CheckBox();
		this.Nature17 = new System.Windows.Forms.CheckBox();
		this.Nature16 = new System.Windows.Forms.CheckBox();
		this.Nature15 = new System.Windows.Forms.CheckBox();
		this.Nature24 = new System.Windows.Forms.CheckBox();
		this.Nature23 = new System.Windows.Forms.CheckBox();
		this.Nature22 = new System.Windows.Forms.CheckBox();
		this.Nature21 = new System.Windows.Forms.CheckBox();
		this.Nature20 = new System.Windows.Forms.CheckBox();
		this.Aplus = new System.Windows.Forms.CheckBox();
		this.Bplus = new System.Windows.Forms.CheckBox();
		this.Splus = new System.Windows.Forms.CheckBox();
		this.Cplus = new System.Windows.Forms.CheckBox();
		this.Dplus = new System.Windows.Forms.CheckBox();
		this.SelectAll = new System.Windows.Forms.CheckBox();
		this.Aminus = new System.Windows.Forms.CheckBox();
		this.Bminus = new System.Windows.Forms.CheckBox();
		this.Sminus = new System.Windows.Forms.CheckBox();
		this.Cminus = new System.Windows.Forms.CheckBox();
		this.Dminus = new System.Windows.Forms.CheckBox();
		this.panel1 = new System.Windows.Forms.Panel();
		this.panel2 = new System.Windows.Forms.Panel();
		this.panel3 = new System.Windows.Forms.Panel();
		this.panel4 = new System.Windows.Forms.Panel();
		this.panel5 = new System.Windows.Forms.Panel();
		this.panel6 = new System.Windows.Forms.Panel();
		this.panel7 = new System.Windows.Forms.Panel();
		this.panel8 = new System.Windows.Forms.Panel();
		this.panel9 = new System.Windows.Forms.Panel();
		this.panel10 = new System.Windows.Forms.Panel();
		this.panel11 = new System.Windows.Forms.Panel();
		this.panel12 = new System.Windows.Forms.Panel();
		this.panel13 = new System.Windows.Forms.Panel();
		this.panel14 = new System.Windows.Forms.Panel();
		this.panel15 = new System.Windows.Forms.Panel();
		this.panel16 = new System.Windows.Forms.Panel();
		this.panel17 = new System.Windows.Forms.Panel();
		this.panel18 = new System.Windows.Forms.Panel();
		this.panel19 = new System.Windows.Forms.Panel();
		this.panel20 = new System.Windows.Forms.Panel();
		this.panel21 = new System.Windows.Forms.Panel();
		this.panel22 = new System.Windows.Forms.Panel();
		this.panel23 = new System.Windows.Forms.Panel();
		this.panel24 = new System.Windows.Forms.Panel();
		this.panel25 = new System.Windows.Forms.Panel();
		this.panel26 = new System.Windows.Forms.Panel();
		this.panel27 = new System.Windows.Forms.Panel();
		this.panel28 = new System.Windows.Forms.Panel();
		this.panel29 = new System.Windows.Forms.Panel();
		this.panel30 = new System.Windows.Forms.Panel();
		this.panel31 = new System.Windows.Forms.Panel();
		this.panel32 = new System.Windows.Forms.Panel();
		this.Nature19 = new System.Windows.Forms.CheckBox();
		this.panel33 = new System.Windows.Forms.Panel();
		this.panel34 = new System.Windows.Forms.Panel();
		this.panel35 = new System.Windows.Forms.Panel();
		this.panel36 = new System.Windows.Forms.Panel();
		this.panel1.SuspendLayout();
		this.panel2.SuspendLayout();
		this.panel3.SuspendLayout();
		this.panel4.SuspendLayout();
		this.panel5.SuspendLayout();
		this.panel6.SuspendLayout();
		this.panel7.SuspendLayout();
		this.panel8.SuspendLayout();
		this.panel9.SuspendLayout();
		this.panel10.SuspendLayout();
		this.panel11.SuspendLayout();
		this.panel12.SuspendLayout();
		this.panel13.SuspendLayout();
		this.panel14.SuspendLayout();
		this.panel15.SuspendLayout();
		this.panel16.SuspendLayout();
		this.panel17.SuspendLayout();
		this.panel18.SuspendLayout();
		this.panel19.SuspendLayout();
		this.panel20.SuspendLayout();
		this.panel21.SuspendLayout();
		this.panel22.SuspendLayout();
		this.panel23.SuspendLayout();
		this.panel24.SuspendLayout();
		this.panel25.SuspendLayout();
		this.panel26.SuspendLayout();
		this.panel27.SuspendLayout();
		this.panel28.SuspendLayout();
		this.panel29.SuspendLayout();
		this.panel30.SuspendLayout();
		this.panel31.SuspendLayout();
		this.panel32.SuspendLayout();
		this.panel33.SuspendLayout();
		this.panel34.SuspendLayout();
		this.panel35.SuspendLayout();
		this.panel36.SuspendLayout();
		base.SuspendLayout();
		this.Nature0.AutoSize = true;
		this.Nature0.Location = new System.Drawing.Point(3, 3);
		this.Nature0.Name = "Nature0";
		this.Nature0.Size = new System.Drawing.Size(72, 16);
		this.Nature0.TabIndex = 0;
		this.Nature0.Text = "がんばりや";
		this.Nature0.UseVisualStyleBackColor = true;
		this.Nature1.AutoSize = true;
		this.Nature1.Location = new System.Drawing.Point(3, 3);
		this.Nature1.Name = "Nature1";
		this.Nature1.Size = new System.Drawing.Size(70, 16);
		this.Nature1.TabIndex = 1;
		this.Nature1.Text = "さみしがり";
		this.Nature1.UseVisualStyleBackColor = true;
		this.Nature2.AutoSize = true;
		this.Nature2.Location = new System.Drawing.Point(3, 3);
		this.Nature2.Name = "Nature2";
		this.Nature2.Size = new System.Drawing.Size(61, 16);
		this.Nature2.TabIndex = 2;
		this.Nature2.Text = "ゆうかん";
		this.Nature2.UseVisualStyleBackColor = true;
		this.Nature3.AutoSize = true;
		this.Nature3.Location = new System.Drawing.Point(3, 3);
		this.Nature3.Name = "Nature3";
		this.Nature3.Size = new System.Drawing.Size(69, 16);
		this.Nature3.TabIndex = 3;
		this.Nature3.Text = "いじっぱり";
		this.Nature3.UseVisualStyleBackColor = true;
		this.Nature4.AutoSize = true;
		this.Nature4.Location = new System.Drawing.Point(3, 3);
		this.Nature4.Name = "Nature4";
		this.Nature4.Size = new System.Drawing.Size(61, 16);
		this.Nature4.TabIndex = 4;
		this.Nature4.Text = "やんちゃ";
		this.Nature4.UseVisualStyleBackColor = true;
		this.Nature9.AutoSize = true;
		this.Nature9.Location = new System.Drawing.Point(3, 3);
		this.Nature9.Name = "Nature9";
		this.Nature9.Size = new System.Drawing.Size(69, 16);
		this.Nature9.TabIndex = 9;
		this.Nature9.Text = "のうてんき";
		this.Nature9.UseVisualStyleBackColor = true;
		this.Nature8.AutoSize = true;
		this.Nature8.Location = new System.Drawing.Point(3, 3);
		this.Nature8.Name = "Nature8";
		this.Nature8.Size = new System.Drawing.Size(60, 16);
		this.Nature8.TabIndex = 8;
		this.Nature8.Text = "わんぱく";
		this.Nature8.UseVisualStyleBackColor = true;
		this.Nature7.AutoSize = true;
		this.Nature7.Location = new System.Drawing.Point(3, 3);
		this.Nature7.Name = "Nature7";
		this.Nature7.Size = new System.Drawing.Size(53, 16);
		this.Nature7.TabIndex = 7;
		this.Nature7.Text = "のんき";
		this.Nature7.UseVisualStyleBackColor = true;
		this.Nature6.AutoSize = true;
		this.Nature6.Location = new System.Drawing.Point(3, 3);
		this.Nature6.Name = "Nature6";
		this.Nature6.Size = new System.Drawing.Size(54, 16);
		this.Nature6.TabIndex = 6;
		this.Nature6.Text = "すなお";
		this.Nature6.UseVisualStyleBackColor = true;
		this.Nature5.AutoSize = true;
		this.Nature5.Location = new System.Drawing.Point(3, 3);
		this.Nature5.Name = "Nature5";
		this.Nature5.Size = new System.Drawing.Size(62, 16);
		this.Nature5.TabIndex = 5;
		this.Nature5.Text = "ずぶとい";
		this.Nature5.UseVisualStyleBackColor = true;
		this.Nature14.AutoSize = true;
		this.Nature14.Location = new System.Drawing.Point(3, 3);
		this.Nature14.Name = "Nature14";
		this.Nature14.Size = new System.Drawing.Size(60, 16);
		this.Nature14.TabIndex = 14;
		this.Nature14.Text = "むじゃき";
		this.Nature14.UseVisualStyleBackColor = true;
		this.Nature13.AutoSize = true;
		this.Nature13.Location = new System.Drawing.Point(3, 3);
		this.Nature13.Name = "Nature13";
		this.Nature13.Size = new System.Drawing.Size(49, 16);
		this.Nature13.TabIndex = 13;
		this.Nature13.Text = "ようき";
		this.Nature13.UseVisualStyleBackColor = true;
		this.Nature12.AutoSize = true;
		this.Nature12.Location = new System.Drawing.Point(3, 3);
		this.Nature12.Name = "Nature12";
		this.Nature12.Size = new System.Drawing.Size(52, 16);
		this.Nature12.TabIndex = 12;
		this.Nature12.Text = "まじめ";
		this.Nature12.UseVisualStyleBackColor = true;
		this.Nature11.AutoSize = true;
		this.Nature11.Location = new System.Drawing.Point(3, 3);
		this.Nature11.Name = "Nature11";
		this.Nature11.Size = new System.Drawing.Size(61, 16);
		this.Nature11.TabIndex = 11;
		this.Nature11.Text = "せっかち";
		this.Nature11.UseVisualStyleBackColor = true;
		this.Nature10.AutoSize = true;
		this.Nature10.Location = new System.Drawing.Point(3, 3);
		this.Nature10.Name = "Nature10";
		this.Nature10.Size = new System.Drawing.Size(64, 16);
		this.Nature10.TabIndex = 10;
		this.Nature10.Text = "おくびょう";
		this.Nature10.UseVisualStyleBackColor = true;
		this.Nature18.AutoSize = true;
		this.Nature18.Location = new System.Drawing.Point(3, 3);
		this.Nature18.Name = "Nature18";
		this.Nature18.Size = new System.Drawing.Size(54, 16);
		this.Nature18.TabIndex = 18;
		this.Nature18.Text = "てれや";
		this.Nature18.UseVisualStyleBackColor = true;
		this.Nature17.AutoSize = true;
		this.Nature17.Location = new System.Drawing.Point(3, 3);
		this.Nature17.Name = "Nature17";
		this.Nature17.Size = new System.Drawing.Size(65, 16);
		this.Nature17.TabIndex = 17;
		this.Nature17.Text = "れいせい";
		this.Nature17.UseVisualStyleBackColor = true;
		this.Nature16.AutoSize = true;
		this.Nature16.Location = new System.Drawing.Point(3, 3);
		this.Nature16.Name = "Nature16";
		this.Nature16.Size = new System.Drawing.Size(58, 16);
		this.Nature16.TabIndex = 16;
		this.Nature16.Text = "おっとり";
		this.Nature16.UseVisualStyleBackColor = true;
		this.Nature15.AutoSize = true;
		this.Nature15.Location = new System.Drawing.Point(3, 3);
		this.Nature15.Name = "Nature15";
		this.Nature15.Size = new System.Drawing.Size(63, 16);
		this.Nature15.TabIndex = 15;
		this.Nature15.Text = "ひかえめ";
		this.Nature15.UseVisualStyleBackColor = true;
		this.Nature24.AutoSize = true;
		this.Nature24.Location = new System.Drawing.Point(3, 3);
		this.Nature24.Name = "Nature24";
		this.Nature24.Size = new System.Drawing.Size(61, 16);
		this.Nature24.TabIndex = 24;
		this.Nature24.Text = "きまぐれ";
		this.Nature24.UseVisualStyleBackColor = true;
		this.Nature23.AutoSize = true;
		this.Nature23.Location = new System.Drawing.Point(3, 3);
		this.Nature23.Name = "Nature23";
		this.Nature23.Size = new System.Drawing.Size(66, 16);
		this.Nature23.TabIndex = 23;
		this.Nature23.Text = "しんちょう";
		this.Nature23.UseVisualStyleBackColor = true;
		this.Nature22.AutoSize = true;
		this.Nature22.Location = new System.Drawing.Point(3, 3);
		this.Nature22.Name = "Nature22";
		this.Nature22.Size = new System.Drawing.Size(62, 16);
		this.Nature22.TabIndex = 22;
		this.Nature22.Text = "なまいき";
		this.Nature22.UseVisualStyleBackColor = true;
		this.Nature21.AutoSize = true;
		this.Nature21.Location = new System.Drawing.Point(3, 3);
		this.Nature21.Name = "Nature21";
		this.Nature21.Size = new System.Drawing.Size(71, 16);
		this.Nature21.TabIndex = 21;
		this.Nature21.Text = "おとなしい";
		this.Nature21.UseVisualStyleBackColor = true;
		this.Nature20.AutoSize = true;
		this.Nature20.Location = new System.Drawing.Point(3, 3);
		this.Nature20.Name = "Nature20";
		this.Nature20.Size = new System.Drawing.Size(63, 16);
		this.Nature20.TabIndex = 20;
		this.Nature20.Text = "おだやか";
		this.Nature20.UseVisualStyleBackColor = true;
		this.Aplus.AutoSize = true;
		this.Aplus.Location = new System.Drawing.Point(4, 4);
		this.Aplus.Name = "Aplus";
		this.Aplus.Size = new System.Drawing.Size(15, 14);
		this.Aplus.TabIndex = 25;
		this.Aplus.UseVisualStyleBackColor = true;
		this.Aplus.Click += new System.EventHandler(Aplus_Click);
		this.Bplus.AutoSize = true;
		this.Bplus.Location = new System.Drawing.Point(4, 3);
		this.Bplus.Name = "Bplus";
		this.Bplus.Size = new System.Drawing.Size(15, 14);
		this.Bplus.TabIndex = 26;
		this.Bplus.UseVisualStyleBackColor = true;
		this.Bplus.Click += new System.EventHandler(Bplus_Click);
		this.Splus.AutoSize = true;
		this.Splus.Location = new System.Drawing.Point(4, 3);
		this.Splus.Name = "Splus";
		this.Splus.Size = new System.Drawing.Size(15, 14);
		this.Splus.TabIndex = 27;
		this.Splus.UseVisualStyleBackColor = true;
		this.Splus.Click += new System.EventHandler(Splus_Click);
		this.Cplus.AutoSize = true;
		this.Cplus.Location = new System.Drawing.Point(4, 4);
		this.Cplus.Name = "Cplus";
		this.Cplus.Size = new System.Drawing.Size(15, 14);
		this.Cplus.TabIndex = 28;
		this.Cplus.UseVisualStyleBackColor = true;
		this.Cplus.Click += new System.EventHandler(Cplus_Click);
		this.Dplus.AutoSize = true;
		this.Dplus.Location = new System.Drawing.Point(4, 4);
		this.Dplus.Name = "Dplus";
		this.Dplus.Size = new System.Drawing.Size(15, 14);
		this.Dplus.TabIndex = 29;
		this.Dplus.UseVisualStyleBackColor = true;
		this.Dplus.Click += new System.EventHandler(Dplus_Click);
		this.SelectAll.AutoSize = true;
		this.SelectAll.Location = new System.Drawing.Point(4, 4);
		this.SelectAll.Name = "SelectAll";
		this.SelectAll.Size = new System.Drawing.Size(15, 14);
		this.SelectAll.TabIndex = 30;
		this.SelectAll.UseVisualStyleBackColor = true;
		this.SelectAll.Click += new System.EventHandler(SelectAll_Click);
		this.Aminus.AutoSize = true;
		this.Aminus.Location = new System.Drawing.Point(29, 4);
		this.Aminus.Name = "Aminus";
		this.Aminus.Size = new System.Drawing.Size(15, 14);
		this.Aminus.TabIndex = 31;
		this.Aminus.UseVisualStyleBackColor = true;
		this.Aminus.Click += new System.EventHandler(Aminus_Click);
		this.Bminus.AutoSize = true;
		this.Bminus.Location = new System.Drawing.Point(29, 4);
		this.Bminus.Name = "Bminus";
		this.Bminus.Size = new System.Drawing.Size(15, 14);
		this.Bminus.TabIndex = 32;
		this.Bminus.UseVisualStyleBackColor = true;
		this.Bminus.Click += new System.EventHandler(Bminus_Click);
		this.Sminus.AutoSize = true;
		this.Sminus.Location = new System.Drawing.Point(29, 4);
		this.Sminus.Name = "Sminus";
		this.Sminus.Size = new System.Drawing.Size(15, 14);
		this.Sminus.TabIndex = 33;
		this.Sminus.UseVisualStyleBackColor = true;
		this.Sminus.Click += new System.EventHandler(Sminus_Click);
		this.Cminus.AutoSize = true;
		this.Cminus.Location = new System.Drawing.Point(29, 4);
		this.Cminus.Name = "Cminus";
		this.Cminus.Size = new System.Drawing.Size(15, 14);
		this.Cminus.TabIndex = 34;
		this.Cminus.UseVisualStyleBackColor = true;
		this.Cminus.Click += new System.EventHandler(Cminus_Click);
		this.Dminus.AutoSize = true;
		this.Dminus.Location = new System.Drawing.Point(29, 4);
		this.Dminus.Name = "Dminus";
		this.Dminus.Size = new System.Drawing.Size(15, 14);
		this.Dminus.TabIndex = 35;
		this.Dminus.UseVisualStyleBackColor = true;
		this.Dminus.Click += new System.EventHandler(Dminus_Click);
		this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel1.Controls.Add(this.SelectAll);
		this.panel1.Location = new System.Drawing.Point(0, 0);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(24, 24);
		this.panel1.TabIndex = 36;
		this.panel2.BackColor = System.Drawing.Color.Coral;
		this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel2.Controls.Add(this.Aplus);
		this.panel2.Location = new System.Drawing.Point(0, 24);
		this.panel2.Name = "panel2";
		this.panel2.Size = new System.Drawing.Size(24, 24);
		this.panel2.TabIndex = 37;
		this.panel3.BackColor = System.Drawing.Color.Pink;
		this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel3.Controls.Add(this.Splus);
		this.panel3.Location = new System.Drawing.Point(0, 70);
		this.panel3.Name = "panel3";
		this.panel3.Size = new System.Drawing.Size(24, 24);
		this.panel3.TabIndex = 39;
		this.panel4.BackColor = System.Drawing.Color.Gold;
		this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel4.Controls.Add(this.Bplus);
		this.panel4.Location = new System.Drawing.Point(0, 47);
		this.panel4.Name = "panel4";
		this.panel4.Size = new System.Drawing.Size(24, 24);
		this.panel4.TabIndex = 38;
		this.panel5.BackColor = System.Drawing.Color.DeepSkyBlue;
		this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel5.Controls.Add(this.Cplus);
		this.panel5.Location = new System.Drawing.Point(0, 93);
		this.panel5.Name = "panel5";
		this.panel5.Size = new System.Drawing.Size(24, 24);
		this.panel5.TabIndex = 39;
		this.panel6.BackColor = System.Drawing.Color.Lime;
		this.panel6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel6.Controls.Add(this.Dplus);
		this.panel6.Location = new System.Drawing.Point(0, 116);
		this.panel6.Name = "panel6";
		this.panel6.Size = new System.Drawing.Size(24, 24);
		this.panel6.TabIndex = 40;
		this.panel7.BackColor = System.Drawing.Color.Lime;
		this.panel7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel7.Controls.Add(this.Nature20);
		this.panel7.Location = new System.Drawing.Point(24, 116);
		this.panel7.Name = "panel7";
		this.panel7.Size = new System.Drawing.Size(74, 24);
		this.panel7.TabIndex = 46;
		this.panel8.BackColor = System.Drawing.Color.DeepSkyBlue;
		this.panel8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel8.Controls.Add(this.Nature15);
		this.panel8.Location = new System.Drawing.Point(24, 93);
		this.panel8.Name = "panel8";
		this.panel8.Size = new System.Drawing.Size(74, 24);
		this.panel8.TabIndex = 44;
		this.panel9.BackColor = System.Drawing.Color.Pink;
		this.panel9.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel9.Controls.Add(this.Nature10);
		this.panel9.Location = new System.Drawing.Point(24, 70);
		this.panel9.Name = "panel9";
		this.panel9.Size = new System.Drawing.Size(74, 24);
		this.panel9.TabIndex = 45;
		this.panel10.BackColor = System.Drawing.SystemColors.Control;
		this.panel10.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel10.Controls.Add(this.Nature0);
		this.panel10.Location = new System.Drawing.Point(24, 24);
		this.panel10.Name = "panel10";
		this.panel10.Size = new System.Drawing.Size(74, 24);
		this.panel10.TabIndex = 42;
		this.panel11.BackColor = System.Drawing.Color.Gold;
		this.panel11.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel11.Controls.Add(this.Nature5);
		this.panel11.Location = new System.Drawing.Point(24, 47);
		this.panel11.Name = "panel11";
		this.panel11.Size = new System.Drawing.Size(74, 24);
		this.panel11.TabIndex = 43;
		this.panel12.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel12.Controls.Add(this.Aminus);
		this.panel12.Location = new System.Drawing.Point(24, 0);
		this.panel12.Name = "panel12";
		this.panel12.Size = new System.Drawing.Size(74, 24);
		this.panel12.TabIndex = 41;
		this.panel13.BackColor = System.Drawing.Color.Lime;
		this.panel13.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel13.Controls.Add(this.Nature21);
		this.panel13.Location = new System.Drawing.Point(97, 116);
		this.panel13.Name = "panel13";
		this.panel13.Size = new System.Drawing.Size(74, 24);
		this.panel13.TabIndex = 52;
		this.panel14.BackColor = System.Drawing.Color.DeepSkyBlue;
		this.panel14.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel14.Controls.Add(this.Nature16);
		this.panel14.Location = new System.Drawing.Point(97, 93);
		this.panel14.Name = "panel14";
		this.panel14.Size = new System.Drawing.Size(74, 24);
		this.panel14.TabIndex = 50;
		this.panel15.BackColor = System.Drawing.Color.Pink;
		this.panel15.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel15.Controls.Add(this.Nature11);
		this.panel15.Location = new System.Drawing.Point(97, 70);
		this.panel15.Name = "panel15";
		this.panel15.Size = new System.Drawing.Size(74, 24);
		this.panel15.TabIndex = 51;
		this.panel16.BackColor = System.Drawing.Color.Coral;
		this.panel16.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel16.Controls.Add(this.Nature1);
		this.panel16.Location = new System.Drawing.Point(97, 24);
		this.panel16.Name = "panel16";
		this.panel16.Size = new System.Drawing.Size(74, 24);
		this.panel16.TabIndex = 48;
		this.panel17.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel17.Controls.Add(this.Nature6);
		this.panel17.Location = new System.Drawing.Point(97, 47);
		this.panel17.Name = "panel17";
		this.panel17.Size = new System.Drawing.Size(74, 24);
		this.panel17.TabIndex = 49;
		this.panel18.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel18.Controls.Add(this.Bminus);
		this.panel18.Location = new System.Drawing.Point(97, 0);
		this.panel18.Name = "panel18";
		this.panel18.Size = new System.Drawing.Size(74, 24);
		this.panel18.TabIndex = 47;
		this.panel19.BackColor = System.Drawing.Color.Lime;
		this.panel19.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel19.Controls.Add(this.Nature22);
		this.panel19.Location = new System.Drawing.Point(168, 116);
		this.panel19.Name = "panel19";
		this.panel19.Size = new System.Drawing.Size(74, 24);
		this.panel19.TabIndex = 58;
		this.panel20.BackColor = System.Drawing.Color.DeepSkyBlue;
		this.panel20.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel20.Controls.Add(this.Nature17);
		this.panel20.Location = new System.Drawing.Point(168, 93);
		this.panel20.Name = "panel20";
		this.panel20.Size = new System.Drawing.Size(74, 24);
		this.panel20.TabIndex = 56;
		this.panel21.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel21.Controls.Add(this.Nature12);
		this.panel21.Location = new System.Drawing.Point(168, 70);
		this.panel21.Name = "panel21";
		this.panel21.Size = new System.Drawing.Size(74, 24);
		this.panel21.TabIndex = 57;
		this.panel22.BackColor = System.Drawing.Color.Coral;
		this.panel22.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel22.Controls.Add(this.Nature2);
		this.panel22.Location = new System.Drawing.Point(168, 24);
		this.panel22.Name = "panel22";
		this.panel22.Size = new System.Drawing.Size(74, 24);
		this.panel22.TabIndex = 54;
		this.panel23.BackColor = System.Drawing.Color.Gold;
		this.panel23.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel23.Controls.Add(this.Nature7);
		this.panel23.Location = new System.Drawing.Point(168, 47);
		this.panel23.Name = "panel23";
		this.panel23.Size = new System.Drawing.Size(74, 24);
		this.panel23.TabIndex = 55;
		this.panel24.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel24.Controls.Add(this.Sminus);
		this.panel24.Location = new System.Drawing.Point(168, 0);
		this.panel24.Name = "panel24";
		this.panel24.Size = new System.Drawing.Size(74, 24);
		this.panel24.TabIndex = 53;
		this.panel25.BackColor = System.Drawing.Color.Lime;
		this.panel25.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel25.Controls.Add(this.Nature23);
		this.panel25.Location = new System.Drawing.Point(241, 116);
		this.panel25.Name = "panel25";
		this.panel25.Size = new System.Drawing.Size(74, 24);
		this.panel25.TabIndex = 64;
		this.panel26.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel26.Controls.Add(this.Nature18);
		this.panel26.Location = new System.Drawing.Point(241, 93);
		this.panel26.Name = "panel26";
		this.panel26.Size = new System.Drawing.Size(74, 24);
		this.panel26.TabIndex = 62;
		this.panel27.BackColor = System.Drawing.Color.Pink;
		this.panel27.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel27.Controls.Add(this.Nature13);
		this.panel27.Location = new System.Drawing.Point(241, 70);
		this.panel27.Name = "panel27";
		this.panel27.Size = new System.Drawing.Size(74, 24);
		this.panel27.TabIndex = 63;
		this.panel28.BackColor = System.Drawing.Color.Coral;
		this.panel28.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel28.Controls.Add(this.Nature3);
		this.panel28.Location = new System.Drawing.Point(241, 24);
		this.panel28.Name = "panel28";
		this.panel28.Size = new System.Drawing.Size(74, 24);
		this.panel28.TabIndex = 60;
		this.panel29.BackColor = System.Drawing.Color.Gold;
		this.panel29.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel29.Controls.Add(this.Nature8);
		this.panel29.Location = new System.Drawing.Point(241, 47);
		this.panel29.Name = "panel29";
		this.panel29.Size = new System.Drawing.Size(74, 24);
		this.panel29.TabIndex = 61;
		this.panel30.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel30.Controls.Add(this.Cminus);
		this.panel30.Location = new System.Drawing.Point(241, 0);
		this.panel30.Name = "panel30";
		this.panel30.Size = new System.Drawing.Size(74, 24);
		this.panel30.TabIndex = 59;
		this.panel31.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel31.Controls.Add(this.Nature24);
		this.panel31.Location = new System.Drawing.Point(314, 116);
		this.panel31.Name = "panel31";
		this.panel31.Size = new System.Drawing.Size(74, 24);
		this.panel31.TabIndex = 70;
		this.panel32.BackColor = System.Drawing.Color.DeepSkyBlue;
		this.panel32.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel32.Controls.Add(this.Nature19);
		this.panel32.Location = new System.Drawing.Point(314, 93);
		this.panel32.Name = "panel32";
		this.panel32.Size = new System.Drawing.Size(74, 24);
		this.panel32.TabIndex = 68;
		this.Nature19.AutoSize = true;
		this.Nature19.Location = new System.Drawing.Point(3, 3);
		this.Nature19.Name = "Nature19";
		this.Nature19.Size = new System.Drawing.Size(67, 16);
		this.Nature19.TabIndex = 15;
		this.Nature19.Text = "うっかりや";
		this.Nature19.UseVisualStyleBackColor = true;
		this.panel33.BackColor = System.Drawing.Color.Pink;
		this.panel33.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel33.Controls.Add(this.Nature14);
		this.panel33.Location = new System.Drawing.Point(314, 70);
		this.panel33.Name = "panel33";
		this.panel33.Size = new System.Drawing.Size(74, 24);
		this.panel33.TabIndex = 69;
		this.panel34.BackColor = System.Drawing.Color.Coral;
		this.panel34.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel34.Controls.Add(this.Nature4);
		this.panel34.Location = new System.Drawing.Point(314, 24);
		this.panel34.Name = "panel34";
		this.panel34.Size = new System.Drawing.Size(74, 24);
		this.panel34.TabIndex = 66;
		this.panel35.BackColor = System.Drawing.Color.Gold;
		this.panel35.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel35.Controls.Add(this.Nature9);
		this.panel35.Location = new System.Drawing.Point(314, 47);
		this.panel35.Name = "panel35";
		this.panel35.Size = new System.Drawing.Size(74, 24);
		this.panel35.TabIndex = 67;
		this.panel36.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel36.Controls.Add(this.Dminus);
		this.panel36.Location = new System.Drawing.Point(314, 0);
		this.panel36.Name = "panel36";
		this.panel36.Size = new System.Drawing.Size(74, 24);
		this.panel36.TabIndex = 65;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.panel31);
		base.Controls.Add(this.panel32);
		base.Controls.Add(this.panel33);
		base.Controls.Add(this.panel34);
		base.Controls.Add(this.panel35);
		base.Controls.Add(this.panel36);
		base.Controls.Add(this.panel25);
		base.Controls.Add(this.panel26);
		base.Controls.Add(this.panel27);
		base.Controls.Add(this.panel28);
		base.Controls.Add(this.panel29);
		base.Controls.Add(this.panel30);
		base.Controls.Add(this.panel19);
		base.Controls.Add(this.panel20);
		base.Controls.Add(this.panel21);
		base.Controls.Add(this.panel22);
		base.Controls.Add(this.panel23);
		base.Controls.Add(this.panel24);
		base.Controls.Add(this.panel13);
		base.Controls.Add(this.panel14);
		base.Controls.Add(this.panel15);
		base.Controls.Add(this.panel16);
		base.Controls.Add(this.panel17);
		base.Controls.Add(this.panel18);
		base.Controls.Add(this.panel7);
		base.Controls.Add(this.panel6);
		base.Controls.Add(this.panel8);
		base.Controls.Add(this.panel5);
		base.Controls.Add(this.panel9);
		base.Controls.Add(this.panel3);
		base.Controls.Add(this.panel10);
		base.Controls.Add(this.panel2);
		base.Controls.Add(this.panel11);
		base.Controls.Add(this.panel4);
		base.Controls.Add(this.panel12);
		base.Controls.Add(this.panel1);
		base.Name = "NaturePanel";
		base.Size = new System.Drawing.Size(388, 140);
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		this.panel2.ResumeLayout(false);
		this.panel2.PerformLayout();
		this.panel3.ResumeLayout(false);
		this.panel3.PerformLayout();
		this.panel4.ResumeLayout(false);
		this.panel4.PerformLayout();
		this.panel5.ResumeLayout(false);
		this.panel5.PerformLayout();
		this.panel6.ResumeLayout(false);
		this.panel6.PerformLayout();
		this.panel7.ResumeLayout(false);
		this.panel7.PerformLayout();
		this.panel8.ResumeLayout(false);
		this.panel8.PerformLayout();
		this.panel9.ResumeLayout(false);
		this.panel9.PerformLayout();
		this.panel10.ResumeLayout(false);
		this.panel10.PerformLayout();
		this.panel11.ResumeLayout(false);
		this.panel11.PerformLayout();
		this.panel12.ResumeLayout(false);
		this.panel12.PerformLayout();
		this.panel13.ResumeLayout(false);
		this.panel13.PerformLayout();
		this.panel14.ResumeLayout(false);
		this.panel14.PerformLayout();
		this.panel15.ResumeLayout(false);
		this.panel15.PerformLayout();
		this.panel16.ResumeLayout(false);
		this.panel16.PerformLayout();
		this.panel17.ResumeLayout(false);
		this.panel17.PerformLayout();
		this.panel18.ResumeLayout(false);
		this.panel18.PerformLayout();
		this.panel19.ResumeLayout(false);
		this.panel19.PerformLayout();
		this.panel20.ResumeLayout(false);
		this.panel20.PerformLayout();
		this.panel21.ResumeLayout(false);
		this.panel21.PerformLayout();
		this.panel22.ResumeLayout(false);
		this.panel22.PerformLayout();
		this.panel23.ResumeLayout(false);
		this.panel23.PerformLayout();
		this.panel24.ResumeLayout(false);
		this.panel24.PerformLayout();
		this.panel25.ResumeLayout(false);
		this.panel25.PerformLayout();
		this.panel26.ResumeLayout(false);
		this.panel26.PerformLayout();
		this.panel27.ResumeLayout(false);
		this.panel27.PerformLayout();
		this.panel28.ResumeLayout(false);
		this.panel28.PerformLayout();
		this.panel29.ResumeLayout(false);
		this.panel29.PerformLayout();
		this.panel30.ResumeLayout(false);
		this.panel30.PerformLayout();
		this.panel31.ResumeLayout(false);
		this.panel31.PerformLayout();
		this.panel32.ResumeLayout(false);
		this.panel32.PerformLayout();
		this.panel33.ResumeLayout(false);
		this.panel33.PerformLayout();
		this.panel34.ResumeLayout(false);
		this.panel34.PerformLayout();
		this.panel35.ResumeLayout(false);
		this.panel35.PerformLayout();
		this.panel36.ResumeLayout(false);
		this.panel36.PerformLayout();
		base.ResumeLayout(false);
	}
}
