using System.Text.Json.Serialization;

namespace FortiConnect.Utils;

[JsonSourceGenerationOptions(
	 WriteIndented = true
	,DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	,Converters = [
			 typeof(VpnConfigConverter)
			,typeof(EmailAccountConfigConverter)
			,typeof(JsonStringEnumConverter<EmailServerProtocol>)
	]
)]
[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(VpnConfig))]
[JsonSerializable(typeof(FortiClientConfig))]
[JsonSerializable(typeof(EmailServerConfig))]
[JsonSerializable(typeof(EmailAccountConfig))]
[JsonSerializable(typeof(EmailServerProtocol))]
public partial class SourceGenerationContext : JsonSerializerContext { }


/// <summary>Context to be used from inside JsonConverters where it cant use itself as converter to avoid an infinite loop.</summary>
[JsonSourceGenerationOptions(
	 WriteIndented = true
	,DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	,Converters = []
)]
[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(VpnConfig))]
[JsonSerializable(typeof(FortiClientConfig))]
[JsonSerializable(typeof(EmailServerConfig))]
[JsonSerializable(typeof(EmailAccountConfig))]
[JsonSerializable(typeof(EmailServerProtocol))]
public partial class ConvertlessContext : JsonSerializerContext { }

// https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-5-0

/// <summary>
/// VpnConfig converter that excludes the plain text passwords from being serialized,
/// but can read them when de-serializing.
/// </summary>
public class VpnConfigConverter : ExcludeWriteFieldListConverter<VpnConfig>
{
	public VpnConfigConverter() : base(
		//ConvertlessContext.Default.VpnConfig,
		ConvertlessContext.Default,
		nameof(VpnConfig.Password)
	) { }
}

/// <summary>
/// EmailAccountConfig converter that excludes the plain text passwords from being serialized,
/// but can read them when de-serializing.
/// </summary>
public class EmailAccountConfigConverter : ExcludeWriteFieldListConverter<EmailAccountConfig>
{
	public EmailAccountConfigConverter() : base(
		//ConvertlessContext.Default.EmailAccountConfig,
		ConvertlessContext.Default,
		nameof(EmailAccountConfig.Password
	)) { }
}
