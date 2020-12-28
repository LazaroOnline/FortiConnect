﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Reflection;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using FortiConnect.Utils;

namespace FortiConnect.Services
{
	// https://www.newtonsoft.com/json
	// https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to
	public class AppSettingsWriter
	{
		public const string DEFAULT_FILENAME = "AppSettings.AutoSave.json";
		public void Save(AppSettings appSettings, string fileName = DEFAULT_FILENAME)
		{
			var executingAssembly = Assembly.GetExecutingAssembly();
			var folderPath = Path.GetDirectoryName(executingAssembly.Location);
			var settingsFullPath = $"{folderPath}/{fileName}";

			var jsonSerializerOptions = new JsonSerializerOptions {
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				//PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				Converters = {
					 new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
					,new VpnConfigConverter()
					,new EmailAccountConfigConverter()
				},
			};

			string appSettingsJson = JsonSerializer.Serialize(appSettings, jsonSerializerOptions);
			File.WriteAllText(settingsFullPath, appSettingsJson);
		}
	}

}
