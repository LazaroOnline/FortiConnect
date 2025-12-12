using System.Text.Json;
using System.Text.Json.Serialization;

namespace FortiConnect.UnitTests.Converters.Json;

public class ExcludeWriteFieldListConverterTest
{
	public TestModel GetTestModel()
	{
		return new TestModel() {
			Name = "SomeName",
			Surname = "SomeSurname",
			Age = 123,
		};
	}

	[Fact]
	public void CanExcludeAnIntFieldFromSerialization()
	{
		var model = GetTestModel();
		var jsonSerializerOptions = new JsonSerializerOptions {
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			WriteIndented = true,
			Converters = { new TestModelConverterExcludeAge() },
		};

		string result = JsonSerializer.Serialize(model, jsonSerializerOptions);

		result.Should().Contain(model.Name);
		result.Should().Contain(model.Surname);
		result.Should().NotContain(model.Age.ToString());

		result.Should().NotContain($@"""{nameof(TestModel.Age)}""");
	}

	[Fact]
	public void CanExcludeAStringFieldFromSerialization()
	{
		var model = GetTestModel();
		var jsonSerializerOptions = new JsonSerializerOptions {
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			WriteIndented = true,
			Converters = { new ExcludeWriteFieldListConverter<TestModel>(fieldToExclude: nameof(TestModel.Surname)) },
		};

		string result = JsonSerializer.Serialize(model, jsonSerializerOptions);

		result.Should().Contain(model.Name);
		result.Should().Contain(model.Age.ToString());
		result.Should().NotContain(model.Surname);

		result.Should().NotContain($@"""{nameof(TestModel.Surname)}""");
	}

	[Fact]
	public void CanExcludeAllFieldsFromAnObjectFromSerialization()
	{
		var model = GetTestModel();
		var jsonSerializerOptions = new JsonSerializerOptions {
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			WriteIndented = true,
			Converters = { new ExcludeWriteFieldListConverter<TestModel>(
				nameof(TestModel.Name),
				nameof(TestModel.Surname),
				nameof(TestModel.Age)
			)},
		};

		string result = JsonSerializer.Serialize(model, jsonSerializerOptions);

		result.Should().NotContain(model.Name);
		result.Should().NotContain(model.Surname);
		result.Should().NotContain(model.Age.ToString());

		result.Should().NotContain($@"""{nameof(TestModel.Name)}""");
		result.Should().NotContain($@"""{nameof(TestModel.Surname)}""");
		result.Should().NotContain($@"""{nameof(TestModel.Age)}""");
	}

	[Theory]
	[InlineData(nameof(TestModel.Age))]
	[InlineData(nameof(TestModel.Name))]
	[InlineData(nameof(TestModel.Surname))]
	public void CanExcludeIndividualFieldsFromSerialization(string fieldName)
	{
		var model = GetTestModel();
		var jsonSerializerOptions = new JsonSerializerOptions {
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			WriteIndented = true,
			Converters = { new ExcludeWriteFieldListConverter<TestModel>(fieldToExclude: fieldName) },
		};

		string result = JsonSerializer.Serialize(model, jsonSerializerOptions);

		result.Should().NotContain($@"""{fieldName}""");
	}

	[Fact]
	public void CanDeserializeExludedProperties()
	{
		var jsonSerializerOptions = new JsonSerializerOptions {
			Converters = { new ExcludeWriteFieldListConverter<TestModel>(fieldToExclude: nameof(TestModel.Surname)) },
		};

		var surname = "SomeSurname";
		var json = $@"{{ ""{nameof(TestModel.Surname)}"": ""{surname}"" }}";
		var result = JsonSerializer.Deserialize<TestModel>(json, jsonSerializerOptions);
		result.Surname.Should().Be(surname);
	}
	
	// [Fact] // TODO: make this test pass.
	public void CanDeserializeExludedPropertiesFromAModelWithClassAnnotations()
	{
		var surname = "SomeSurname";
		var json = $@"{{ ""{nameof(TestModelWithAnotations.Surname)}"": ""{surname}"" }}";
		var result = JsonSerializer.Deserialize<TestModelWithAnotations>(json);
		result.Surname.Should().Be(surname);
	}

	// Optionally you can create a class with a built-in list of fields to exclude:
	public class TestModelConverterExcludeAge : ExcludeWriteFieldListConverter<TestModel>
	{
		public TestModelConverterExcludeAge() : base(nameof(TestModel.Age)) { }
	}

	public class TestModel
	{
		public string Name { get; set; }

		public string Surname { get; set; }
	
		public int Age { get; set; }
	}

	public class TestModelWithAnotationsConverterExcludeAge : ExcludeWriteFieldListConverter<TestModelWithAnotations>
	{
		public TestModelWithAnotationsConverterExcludeAge() : base(nameof(TestModel.Age)) { }
	}

	[JsonConverter(typeof(TestModelWithAnotationsConverterExcludeAge))]
	public class TestModelWithAnotations
	{
		public string Name { get; set; }

		public string Surname { get; set; }
	
		public int Age { get; set; }
	}
}
