using System.Text.Json;

namespace FortiConnect.UnitTests;

public class PasswordStringTest
{
	#region Serialize
	[Fact]
	public void DoesNotSerializePlainTextPassword_WhenConfigured()
	{
		var passwordInPlainText = "Password-in-plain-text";
		var passwordString = new PasswordString() {
			Password = passwordInPlainText,
		};

		var jsonSerializerOptions = new JsonSerializerOptions {
			WriteIndented = true,
			Converters = {
				new PasswordStringConverter(PasswordStringConverter.PasswordStringConverterOptions.EncodedBase64)
			},
		};

		string result = JsonSerializer.Serialize(passwordString, jsonSerializerOptions);

		result.Should().Contain(passwordString.PasswordEncoded);
		result.Should().NotContain(passwordInPlainText);

		result.Should().NotContain($@"""{nameof(PasswordString.Password)}""");
		result.Should().Contain($@"""{nameof(PasswordString.PasswordEncoded)}""");
	}

	[Fact]
	public void DoesNotSerializeEncodedPassword_WhenConfigured()
	{
		var passwordInPlainText = "Password-in-plain-text";
		var passwordString = new PasswordString() {
			Password = passwordInPlainText,
		};

		var jsonSerializerOptions = new JsonSerializerOptions {
			WriteIndented = true,
			Converters = {
				new PasswordStringConverter(PasswordStringConverter.PasswordStringConverterOptions.PlainText)
			},
		};

		string result = JsonSerializer.Serialize(passwordString, jsonSerializerOptions);

		result.Should().NotContain(passwordString.PasswordEncoded);
		result.Should().Contain(passwordInPlainText);

		result.Should().Contain($@"""{nameof(PasswordString.Password)}""");
		result.Should().NotContain($@"""{nameof(PasswordString.PasswordEncoded)}""");
	}
	
	[Fact]
	public void SerializesBothEncodedAndPlainTextPassword_WhenConfigured()
	{
		var passwordInPlainText = "Password-in-plain-text";
		var passwordString = new PasswordString() {
			Password = passwordInPlainText,
		};

		var jsonSerializerOptions = new JsonSerializerOptions {
			WriteIndented = true,
			Converters = {
				new PasswordStringConverter(PasswordStringConverter.PasswordStringConverterOptions.PlainTextAndEncodedBase64)
			},
		};

		string result = JsonSerializer.Serialize(passwordString, jsonSerializerOptions);

		result.Should().Contain(passwordString.PasswordEncoded);
		result.Should().Contain(passwordInPlainText);

		result.Should().Contain($@"""{nameof(PasswordString.Password)}""");
		result.Should().Contain($@"""{nameof(PasswordString.PasswordEncoded)}""");
	}
	#endregion

	#region De-serialize

	[Fact]
	public void CanDeserializePasswordEncoded()
	{
		var passwordInPlainText = "Password-in-plain-text";
		var json = $@"{{ ""{nameof(PasswordString.Password)}"": ""{passwordInPlainText}"" }}";
		var result = JsonSerializer.Deserialize<PasswordString>(json);
		result.Password.Should().Be(passwordInPlainText);
	}

	[Fact]
	public void DeserializeGivesPlainTextPriorityOverEncodedPasswordRegardlessOfPosition()
	{
		var passwordInPlainText = "Password-in-plain-text";
		var anotherPassword = new PasswordString() { Password = "anotherPassword" };
		var json = $@"{{ ""{nameof(PasswordString.Password)}"": ""{passwordInPlainText}""
				,""{nameof(PasswordString.PasswordEncoded)}"": ""{anotherPassword.PasswordEncoded}""
			}}";
		var result = JsonSerializer.Deserialize<PasswordString>(json);
		result.Password.Should().Be(passwordInPlainText);
	}

	#endregion

	[Theory]
	[InlineData(PasswordStringConverter.PasswordStringConverterOptions.PlainText)]
	[InlineData(PasswordStringConverter.PasswordStringConverterOptions.EncodedBase64)]
	[InlineData(PasswordStringConverter.PasswordStringConverterOptions.PlainTextAndEncodedBase64)]
	public void CanDeserializeASerializedObject(PasswordStringConverter.PasswordStringConverterOptions option)
	{
		var passwordInPlainText = "Password-in-plain-text";
		var passwordString = new PasswordString() {
			Password = passwordInPlainText,
		};

		var jsonSerializerOptions = new JsonSerializerOptions {
			WriteIndented = true,
			Converters = {
				new PasswordStringConverter(option)
			},
		};

		string json = JsonSerializer.Serialize(passwordString, jsonSerializerOptions);

		var result = JsonSerializer.Deserialize<PasswordString>(json);
		result.Password.Should().Be(passwordInPlainText);
	}
}
