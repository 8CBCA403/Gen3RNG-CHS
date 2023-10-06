using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace _3genSearch;

internal class SearchTab
{
	internal TextBox InitialSeedBox;

	internal NumericUpDown[] RTCBox;

	internal TextBox MultipleInitialSeedBox;

	internal NumericUpDown[] FrameBox;

	internal NumericUpDown TargetFrameBox;

	internal NumericUpDown FrameRangeBox;

	internal RadioButton[] InitialSeedButtons;

	internal DataGridView DataGridView;

	internal Button CalcButton;

	internal SearchWorker SearchWorker;

	internal bool SimpleInitialSeed()
	{
		RadioButton[] initialSeedButtons = InitialSeedButtons;
		if (initialSeedButtons == null)
		{
			return false;
		}
		return initialSeedButtons[0].Checked;
	}

	internal bool RTCInitialSeed()
	{
		RadioButton[] initialSeedButtons = InitialSeedButtons;
		if (initialSeedButtons == null)
		{
			return false;
		}
		return initialSeedButtons[1].Checked;
	}

	internal bool MultipleInitialSeed()
	{
		RadioButton[] initialSeedButtons = InitialSeedButtons;
		if (initialSeedButtons == null)
		{
			return false;
		}
		return initialSeedButtons[2].Checked;
	}

	internal CalcParam GetParam(bool ListMode)
	{
		NumericUpDown[] rTCBox = RTCBox;
		int min = (int)((rTCBox != null) ? rTCBox[0].Value : 0m);
		NumericUpDown[] rTCBox2 = RTCBox;
		int num = (int)((rTCBox2 != null) ? rTCBox2[1].Value : 0m);
		List<uint> list = (from x in Enumerable.Range(0, num - min + 1)
			select (uint)(x + min)).ToList();
		List<uint> initialSeedList = new List<uint> { 0u };
		if (SimpleInitialSeed())
		{
			initialSeedList = new List<uint> { Util.GetValueHex(InitialSeedBox) };
		}
		else if (RTCInitialSeed())
		{
			initialSeedList = list.Select((uint x) => Util.GetRTCSeed(x)).ToList();
		}
		else if (MultipleInitialSeed())
		{
			initialSeedList = Util.GetHexList(MultipleInitialSeedBox);
		}
		DataGridViewRow dataGridViewRow = new DataGridViewRow();
		dataGridViewRow.CreateCells(DataGridView);
		uint firstFrame;
		uint lastFrame;
		if (ListMode)
		{
			(firstFrame, lastFrame) = Util.GetRange(TargetFrameBox, FrameRangeBox);
		}
		else
		{
			uint valueDec = Util.GetValueDec(FrameBox[0]);
			uint valueDec2 = Util.GetValueDec(FrameBox[1]);
			firstFrame = valueDec;
			lastFrame = valueDec2;
		}
		if (RTCInitialSeed())
		{
			return new CalcParam(initialSeedList, list, firstFrame, lastFrame, Util.GetValueDec(TargetFrameBox), dataGridViewRow, SearchWorker.GetToken(), CalcButton);
		}
		return new CalcParam(initialSeedList, firstFrame, lastFrame, Util.GetValueDec(TargetFrameBox), dataGridViewRow, SearchWorker.GetToken(), CalcButton);
	}

	internal SearchTab()
	{
		SearchWorker = new SearchWorker();
	}
}
