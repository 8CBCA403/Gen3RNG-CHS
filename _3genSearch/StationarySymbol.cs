using Pokemon3genRNGLibrary;

namespace _3genSearch;

internal class StationarySymbol
{
	private readonly GBASlot symbol;

	private readonly string label;

	public string GetLabel()
	{
		return label;
	}

	public GBASlot GetSymbol()
	{
		return symbol;
	}

	public StationaryGenerator CreateGenerator(IIVsGenerator method)
	{
		return new StationaryGenerator(symbol, method);
	}

	internal StationarySymbol(string label, GBASlot symbol)
	{
		this.label = label;
		this.symbol = symbol;
	}
}
