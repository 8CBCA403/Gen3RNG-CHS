namespace _3genSearch;

public class IDBinder
{
	[DataGridViewRowHeader(78, "初期seed", false, null)]
	public string Initialseed { get; set; }

	[DataGridViewRowHeader(66, "[F]", true, null)]
	public uint Index { get; set; }

	[DataGridViewRowHeader(51, "ズレ", true, null)]
	public int Gap { get; set; }

	[DataGridViewRowHeader(60, "TID", false, null)]
	public uint TID { get; set; }

	[DataGridViewRowHeader(60, "SID", false, null)]
	public uint SID { get; set; }

	[DataGridViewRowHeader(66, "性格値", false, null)]
	public string PID { get; set; }

	public IDBinder(uint InitialSeed, uint Index, int Gap, uint TID, uint SID)
	{
		Initialseed = $"{InitialSeed:X}";
		this.Index = Index;
		this.Gap = Gap;
		this.TID = TID;
		this.SID = SID;
	}

	public IDBinder((uint, uint) InitialSeed, uint Index, int Gap, uint TID, uint SID)
	{
		Initialseed = $"{InitialSeed.Item1:X}({InitialSeed.Item2})";
		this.Index = Index;
		this.Gap = Gap;
		this.TID = TID;
		this.SID = SID;
	}

	public IDBinder(uint InitialSeed, uint Index, int Gap, uint TID, uint SID, uint PID)
	{
		Initialseed = $"{InitialSeed:X}";
		this.Index = Index;
		this.Gap = Gap;
		this.TID = TID;
		this.SID = SID;
		this.PID = $"{PID:X8}";
	}

	public IDBinder((uint, uint) InitialSeed, uint Index, int Gap, uint TID, uint SID, uint PID)
	{
		Initialseed = $"{InitialSeed.Item1:X}({InitialSeed.Item2})";
		this.Index = Index;
		this.Gap = Gap;
		this.TID = TID;
		this.SID = SID;
		this.PID = $"{PID:X8}";
	}
}
