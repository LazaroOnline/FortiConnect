using System.Text.Json;
using System.Text.Json.Serialization;

namespace FortiConnect.Utils;

/// <summary>
/// Provides a class with options to be serialized to Json as encoded to prevent storing passwords in plain text. 
/// It does allow READING the password when de-serializing from the plain text property, but it will not serialize it back in plain text unless configured to do so.
/// NOTE: encoding the password as Base64 is not secure, as it can be easily decoded by anyone, 
/// however encoded passwords may work as a deterrent.
/// </summary>
[JsonConverter(typeof(PasswordStringConverter))]
public class PasswordString
{
	//[JsonIgnore]
	[JsonConverter(typeof(JsonIgnoreWriteAsNull<string>))]
	public string Password { get; set; }

	/// <summary>
	/// Computed property using Base 64 encoding as Unicode.
	/// </summary>
	public string PasswordEncoded {
		get { return Base64Converter.ConvertTextUnicodeToBase64(this.Password); }
		set { this.Password = Base64Converter.ConvertBase64ToTextUnicode(value); }
	}
}

// https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-5-0
public class PasswordStringConverter : JsonConverter<PasswordString>
{
	/// <summary>
	/// Options to select how to serialize the password.
	/// </summary>
	public enum PasswordStringConverterOptions
	{
		EncodedBase64,
		PlainTextAndEncodedBase64,
		PlainText,
	}

	public const PasswordStringConverterOptions DEFAULT_OPTION = PasswordStringConverterOptions.EncodedBase64;

	public PasswordStringConverterOptions SerializeOption { get; set; }

	public PasswordStringConverter() : this(DEFAULT_OPTION)
	{ }

	public PasswordStringConverter(PasswordStringConverterOptions serializeOption = DEFAULT_OPTION)
	{
		SerializeOption = serializeOption;
	}

	// https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-5-0#sample-factory-pattern-converter
	public override PasswordString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		// This makes an infinite loop if the converter is added to the class itself.
		// return JsonSerializer.Deserialize<PasswordString>(ref reader, options);
		// TODO: find another way of invoking the default de-serializer.
		//var converter = options.GetConverter(typeof(PasswordString));

		var passwordString = new PasswordString();

		if (reader.TokenType != JsonTokenType.StartObject) {
			throw new JsonException();
		}
		string passwordValue = null;
		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject) {
				// If the pain-text version of the password is provided, then it takes precedence over the encoded version.
				// This is the only difference from the default reader.
				if (passwordValue != null) {
					passwordString.Password = passwordValue;
				}
				return passwordString;
			}
			if (reader.TokenType != JsonTokenType.PropertyName) {
				throw new JsonException();
			}
			string propertyName = reader.GetString();
			reader.Read();
			string value = JsonSerializer.Deserialize<string>(ref reader, options);
			switch (propertyName)
			{
				case nameof(PasswordString.PasswordEncoded): passwordString.PasswordEncoded = value; break;
				case nameof(PasswordString.Password):		 passwordString.Password = value;
					passwordValue = value;
					break;
			}
		}
		throw new JsonException();
	}

	public override void Write(Utf8JsonWriter writer, PasswordString passwordString, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		if (SerializeOption == PasswordStringConverterOptions.PlainText
			|| SerializeOption == PasswordStringConverterOptions.PlainTextAndEncodedBase64)
		{
			writer.WriteString(nameof(PasswordString.Password), passwordString.Password);
		}
		if (SerializeOption == PasswordStringConverterOptions.EncodedBase64
			|| SerializeOption == PasswordStringConverterOptions.PlainTextAndEncodedBase64)
		{
			writer.WriteString(nameof(PasswordString.PasswordEncoded), passwordString.PasswordEncoded);
		}
		writer.WriteEndObject();
	}
}
