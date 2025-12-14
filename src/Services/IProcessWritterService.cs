using System.Diagnostics;

namespace FortiConnect.Services;

public interface IProcessWritterService
{
	public void WriteToProcess(Process process, string textToWrite);

	public string EscapeLiteralTextToWrite(string literalTextToWrite);
}
