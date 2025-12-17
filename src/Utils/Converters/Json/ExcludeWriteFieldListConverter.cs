using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

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

	public JsonTypeInfo<T> JsonTypeInfo;
	public IJsonTypeInfoResolver JsonTypeInfoResolver;

	public ExcludeWriteFieldListConverter(string fieldToExclude)
	{
		FieldsToExclude = new List<string>() { fieldToExclude };
	}

	/// <summary>This constructor is NOT trimming/AOT compatible, left for convenience in case you don't need trimming.</summary>
	public ExcludeWriteFieldListConverter(params string[] fieldsToExclude)
	{
		FieldsToExclude = fieldsToExclude;
	}

	/// <summary>This constructor IS trimming/AOT compatible. More rigid, uses the serializer options from the 'jsonTypeInfo' parameter.</summary>
	public ExcludeWriteFieldListConverter(JsonTypeInfo<T> jsonTypeInfo, params string[] fieldsToExclude)
	{
		JsonTypeInfo = jsonTypeInfo;
		FieldsToExclude = fieldsToExclude;
	}

	/// <summary>This constructor IS trimming/AOT compatible. More flexible, copies the serializer options from the parent instead of using the ones from 'jsonTypeInfoResolver'.</summary>
	public ExcludeWriteFieldListConverter(IJsonTypeInfoResolver jsonTypeInfoResolver, params string[] fieldsToExclude)
	{
		JsonTypeInfoResolver = jsonTypeInfoResolver;
		FieldsToExclude = fieldsToExclude;
	}

	public JsonSerializerOptions GetSerializerOptions(JsonSerializerOptions options)
	{
		// If the same JsonSerializerOptions converter is used in the de/serialize options, it will enter an infinite loop.
		// To solve this, create a new cloned JsonSerializerOptions without the converters that would enter the infinite loop.
		var optionsWithoutConverters = new JsonSerializerOptions(options); // This clones the same options.
		optionsWithoutConverters.Converters.Clear();
		//optionsWithoutConverters.TypeInfoResolver = null; // No need to remove this.
		if (JsonTypeInfoResolver != null) {
			optionsWithoutConverters.TypeInfoResolver = JsonTypeInfoResolver;
		}
		return optionsWithoutConverters;
	}

	public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (JsonTypeInfo != null) {
			return JsonSerializer.Deserialize<T>(ref reader, JsonTypeInfo);
		}
		else {
			var optionsWithoutConverters = GetSerializerOptions(options);
			return JsonSerializer.Deserialize<T>(ref reader, optionsWithoutConverters);
		}
	}

	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		JsonElement element;
		if (JsonTypeInfo != null) {
			element = JsonSerializer.SerializeToElement(value, JsonTypeInfo);
		}
		else {
			var optionsWithoutConverters = GetSerializerOptions(options);
			element = JsonSerializer.SerializeToElement(value, optionsWithoutConverters);
		}

		writer.WriteStartObject();
		foreach (var prop in element.EnumerateObject())
		{
			var skipField = FieldsToExclude.Any(f => prop.NameEquals(f));
			if (skipField) {
				continue;
			}
			prop.WriteTo(writer);
		}
		writer.WriteEndObject();
	}
}
