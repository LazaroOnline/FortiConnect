using System.Diagnostics;

namespace FortiConnect.Services;

public interface IProcessWritterService
{
	public void WriteToProcess(Process process, string textToWrite);

	public string EscapeLiteralTextToWrite(string literalTextToWrite);

	public string GetKeyEnter();

	public string GetKeyTab();

	// Prevent dependency with "System.Windows.Forms".
	//public string GetSpecialKey(System.Windows.Forms.Keys key);
}
