using System.Text.Json;
using System.Text.Json.Serialization;

namespace FortiConnect.UnitTests.Converters.Json;

public class JsonIgnoreWriteAsNullTest
{
	[Fact]
	public void CanIgnoreFieldsFromSerialization()
	{
		var name = "SomeName";
		var surname = "SomeSurname";
		var model = new TestModel() {
			Name = name,
			Surname = surname,
			Age = 123,
		};

		var jsonSerializerOptions = new JsonSerializerOptions {
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			WriteIndented = true,
			Converters = { },
		};

		string result = JsonSerializer.Serialize(model, jsonSerializerOptions);

		result.Should().Contain(model.Name);
		result.Should().NotContain(model.Surname);
		result.Should().NotContain(model.Age.ToString());

		// TODO: ideally these test would also pass:
		// result.Should().NotContain($@"""{nameof(TestModel.Surname)}""");
		// result.Should().NotContain($@"""{nameof(TestModel.Age)}""");
	}

	[Fact]
	public void CanDeserializeIgnoredFields()
	{
		var surname = "SomeSurname";
		var json = $@"{{ ""{nameof(TestModel.Surname)}"": ""{surname}"" }}";
		var result = JsonSerializer.Deserialize<TestModel>(json);
		result.Surname.Should().Be(surname);
	}
}

public class TestModel
{
	public string Name { get; set; }

	[JsonConverter(typeof(JsonIgnoreWriteAsNull<string>))]
	public string Surname { get; set; }

	
	[JsonConverter(typeof(JsonIgnoreWriteAsNull<int>))]
	public int Age { get; set; }
}
