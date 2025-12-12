using System.Text.Json;

namespace FortiConnect.UnitTests;

public class AppSettingsConverterTest
{
	[Fact]
	public void DoesntWriteThePlainPasswordField()
	{
		var passwordInPlainText = "Password-in-plain-text";
		var vpnConfig = new VpnConfig() {
			UserName = "SomeUser",
			Password = passwordInPlainText,
		};

		var jsonSerializerOptions = new JsonSerializerOptions {
			WriteIndented = true,
			Converters = {
				new VpnConfigConverter()
			},
		};

		string result = JsonSerializer.Serialize(vpnConfig, jsonSerializerOptions);

		result.Should().NotContain(passwordInPlainText);
		result.Should().Contain(vpnConfig.PasswordEncoded);

		result.Should().NotContain($@"""{nameof(VpnConfig.Password)}""");
		result.Should().Contain($@"""{nameof(VpnConfig.PasswordEncoded)}""");
	}

	[Fact]
	public void CanReadThePlainPassword()
	{
		var userName = "SomeUserName";
		var password = "SomePassword";
		var json = $@"
			{{
				""{nameof(VpnConfig.UserName)}"": ""{userName}"",
				""{nameof(VpnConfig.Password)}"": ""{password}""
			}}";
		var jsonSerializerOptions = new JsonSerializerOptions {
			Converters = { new VpnConfigConverter() }
		};

		var result = JsonSerializer.Deserialize<VpnConfig>(json, jsonSerializerOptions);

		result.UserName.Should().Be(userName);
		result.Password.Should().Be(password);
	}

	[Fact]
	public void CanReadTheEncodedPassword()
	{
		var password = "SomePassword";
		var passwordEncoded = Base64Converter.ConvertTextUnicodeToBase64(password);
		var json = $@"
			{{
				""{nameof(VpnConfig.PasswordEncoded)}"": ""{passwordEncoded}""
			}}";
		var jsonSerializerOptions = new JsonSerializerOptions {
			Converters = { new VpnConfigConverter() }
		};

		var result = JsonSerializer.Deserialize<VpnConfig>(json, jsonSerializerOptions);

		result.Password.Should().Contain(password);
	}
	
	// [Fact] // TODO: Make this test pass:
	public void DeserializeGivesPlainTextPriorityOverEncodedPasswordRegardlessOfPosition()
	{
		var passwordInPlainText = "Password-in-plain-text";
		var anotherPassword = "AnotherPassword";
		var anotherPasswordEncoded = Base64Converter.ConvertTextUnicodeToBase64(anotherPassword);
		var json = $@"{{ ""{nameof(VpnConfig.Password)}"": ""{passwordInPlainText}""
				,""{nameof(VpnConfig.PasswordEncoded)}"": ""{anotherPasswordEncoded}""
			}}";
		var result = JsonSerializer.Deserialize<VpnConfig>(json);
		result.Password.Should().Be(passwordInPlainText);
	}
}
