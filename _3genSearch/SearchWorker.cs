using System.Threading;

namespace _3genSearch;

internal class SearchWorker
{
	internal bool isBusy;

	private CancellationTokenSource tokenSource;

	internal CancellationTokenSource newSource()
	{
		return tokenSource = new CancellationTokenSource();
	}

	internal void Cancel()
	{
		tokenSource.Cancel();
	}

	internal CancellationToken GetToken()
	{
		return tokenSource.Token;
	}
}
