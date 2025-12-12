using System.Text.Json;
using System.Text.Json.Serialization;

namespace FortiConnect.Utils;

// In Newtonsoft's Json.NET it would be done like this:
// https://stackoverflow.com/questions/25749509/how-can-i-tell-json-net-to-ignore-properties-in-a-3rd-party-object

/// <summary>
/// Excludes a list of fields (or properties) from being written during serialization.
/// </summary>
public class ExcludeWriteFieldListConverter<T> : JsonConverter<T>
{
	public IEnumerable<string> FieldsToExclude { get; set; }
	public StringComparison FieldNameComparison { get; set; } = StringComparison.InvariantCultureIgnoreCase;

	public ExcludeWriteFieldListConverter(string fieldToExclude)
	{
		FieldsToExclude = new List<string>() { fieldToExclude };
	}

	public ExcludeWriteFieldListConverter(params string[] fieldsToExclude)
	{
		FieldsToExclude = fieldsToExclude;
	}

	public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		// TODO: find a better way of calling the default "Read" method without having to copy the options or entering an infinite loop.

		// If the same converter is used in the de-serialize options, it will enter an infinite loop.
		var optionsWithoutConverters = new JsonSerializerOptions(
		//	options
		) {
			AllowTrailingCommas = options.AllowTrailingCommas,
			DefaultIgnoreCondition = options.DefaultIgnoreCondition,
			Encoder = options.Encoder,
			DefaultBufferSize = options.DefaultBufferSize,
			DictionaryKeyPolicy = options.DictionaryKeyPolicy,
			IgnoreReadOnlyFields = options.IgnoreReadOnlyFields,
			IgnoreReadOnlyProperties = options.IgnoreReadOnlyProperties,
			IncludeFields = options.IncludeFields,
			MaxDepth = options.MaxDepth,
			NumberHandling = options.NumberHandling,
			PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive,
			PropertyNamingPolicy = options.PropertyNamingPolicy,
			ReadCommentHandling = options.ReadCommentHandling,
			ReferenceHandler = options.ReferenceHandler,
			WriteIndented = options.WriteIndented,
			Converters = { } // In reality, this should have the same conveters from the original "options" parameter.
		};
		return JsonSerializer.Deserialize<T>(ref reader, optionsWithoutConverters);
	}

	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		// This implementation doesn't have the best performance,
		// but offers the best maintainability because you don't have to serialize all the non-excluded fields one by one.
		// https://stackoverflow.com/questions/58566735/how-to-exclude-a-property-from-being-serialized-in-system-text-json-jsonserializ/58566925#58566925
		writer.WriteStartObject();
		using (JsonDocument document = JsonDocument.Parse(JsonSerializer.Serialize(value)))
		{
			var propertyList = document.RootElement.EnumerateObject();
			var propertiesNotExcluded = propertyList.Where(p => !FieldsToExclude.Any(f => string.Equals(f, p.Name, FieldNameComparison)));
			foreach (var property in propertiesNotExcluded)
			{
				property.WriteTo(writer);
			}
		}

		writer.WriteEndObject();
	}
}
