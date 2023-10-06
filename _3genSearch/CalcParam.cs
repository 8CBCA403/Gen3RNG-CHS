using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace _3genSearch;

internal class CalcParam
{
	internal List<uint> InitialSeedList;

	internal List<uint> RTCList;

	internal uint FirstFrame;

	internal uint LastFrame;

	internal uint TargetFrame;

	internal DataGridViewRow RowTemplate;

	internal CancellationToken token;

	internal bool DisplayRTC;

	internal IProgress<uint> Progress;

	internal CalcParam(List<uint> InitialSeedList, uint FirstFrame, uint LastFrame, uint TargetFrame, Button button)
	{
		this.InitialSeedList = InitialSeedList;
		this.FirstFrame = FirstFrame;
		this.LastFrame = LastFrame;
		this.TargetFrame = TargetFrame;
		RTCList = new List<uint>();
		RowTemplate = new DataGridViewRow();
		DisplayRTC = false;
		Progress = new Progress<uint>(delegate(uint progress)
		{
			button.Text = $"{100.0 * (double)progress / 1000.0:F1}%";
		});
	}

	internal CalcParam(List<uint> InitialSeedList, List<uint> RTCList, uint FirstFrame, uint LastFrame, uint TargetFrame, Button button)
	{
		this.InitialSeedList = InitialSeedList;
		this.RTCList = RTCList;
		this.FirstFrame = FirstFrame;
		this.LastFrame = LastFrame;
		this.TargetFrame = TargetFrame;
		RowTemplate = new DataGridViewRow();
		DisplayRTC = true;
		Progress = new Progress<uint>(delegate(uint progress)
		{
			button.Text = $"{100.0 * (double)progress / 1000.0:F1}%";
		});
	}

	internal CalcParam(List<uint> InitialSeedList, uint FirstFrame, uint LastFrame, uint TargetFrame, DataGridViewRow RowTemplate, CancellationToken token, Button button)
	{
		this.InitialSeedList = InitialSeedList;
		RTCList = new List<uint>();
		this.FirstFrame = FirstFrame;
		this.LastFrame = LastFrame;
		this.TargetFrame = TargetFrame;
		this.RowTemplate = RowTemplate;
		this.token = token;
		DisplayRTC = false;
		Progress = new Progress<uint>(delegate(uint progress)
		{
			button.Text = $"{100.0 * (double)progress / 1000.0:F1}%";
		});
	}

	internal CalcParam(List<uint> InitialSeedList, List<uint> RTCList, uint FirstFrame, uint LastFrame, uint TargetFrame, DataGridViewRow RowTemplate, CancellationToken token, Button button)
	{
		this.InitialSeedList = InitialSeedList;
		this.RTCList = RTCList;
		this.FirstFrame = FirstFrame;
		this.LastFrame = LastFrame;
		this.TargetFrame = TargetFrame;
		this.RowTemplate = RowTemplate;
		this.token = token;
		DisplayRTC = true;
		Progress = new Progress<uint>(delegate(uint progress)
		{
			button.Text = $"{100.0 * (double)progress / 1000.0:F1}%";
		});
	}
}
