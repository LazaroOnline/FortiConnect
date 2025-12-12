namespace FortiConnect.Utils;

// https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-5-0

/// <summary>
/// VpnConfig converter that excludes the plain text passwords from being serialized,
/// but can read them when de-serializing.
/// </summary>
public class VpnConfigConverter : ExcludeWriteFieldListConverter<VpnConfig>
{
	public VpnConfigConverter() : base(
		fieldToExclude: nameof(VpnConfig.Password)
	)
	{ }
}

/// <summary>
/// EmailAccountConfig converter that excludes the plain text passwords from being serialized,
/// but can read them when de-serializing.
/// </summary>
public class EmailAccountConfigConverter : ExcludeWriteFieldListConverter<EmailAccountConfig>
{
	public EmailAccountConfigConverter() : base(
		fieldToExclude: nameof(EmailAccountConfig.Password)
	)
	{ }
}
