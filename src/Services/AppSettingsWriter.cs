using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FortiConnect.Services
{
	// https://www.newtonsoft.com/json
	// https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to
	public class AppSettingsWriter
	{
		public void Save(AppSettings appSettings, string fileName = "AppSettings.AutoSave.json")
		{
			var executingAssembly = Assembly.GetExecutingAssembly();
			var folderPath = Path.GetDirectoryName(executingAssembly.Location);
			var settingsFullPath = $"{folderPath}/{fileName}";
			string appSettingsJson = JsonSerializer.Serialize(appSettings, new JsonSerializerOptions{ WriteIndented = true });
			File.WriteAllText(settingsFullPath, appSettingsJson);
		}
	}
}
