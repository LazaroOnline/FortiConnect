using System.Text.Json;
using System.Text.Json.Serialization;

namespace FortiConnect.Utils;

/// <summary>
/// Prevent the property value from being written on serialization, writing "null" instead.
/// 
/// TODO: try to find a way to do a "JsonIgnoreWriteTest" class
/// that instead of writing "null" would not write the property at all,
/// like [JsonIgnore] does, but dynamically.
/// 
/// </summary>
/// <typeparam name="T">Type of the property to be ignored</typeparam>
public class JsonIgnoreWriteAsNull<T> : JsonConverter<T>
{
	public override bool HandleNull => true;

	public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return JsonSerializer.Deserialize<T>(ref reader, options);
	}

	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		writer.WriteNullValue();
	}
}
