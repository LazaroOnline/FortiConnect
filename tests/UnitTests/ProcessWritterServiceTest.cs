using System;
using Xunit;
using FortiConnect.Services;
using FluentAssertions;

namespace FortiConnect.UnitTests
{
	public class ProcessWritterServiceTest
	{
		[Theory]
		[InlineData("", 0)]
		[InlineData(null, 0)]
		[InlineData("a", 1)]
		[InlineData("a1", 2)]
		[InlineData("{}", 1)]
		[InlineData("{{}", 1)]
		[InlineData("{}}", 1)]
		[InlineData("{}}a", 2)]
		[InlineData("a{}}a", 3)]
		[InlineData("€", 1)]
		[InlineData("^", 1, "Key modifier for CTRL.")]
		[InlineData("+", 1, "Key modifier for SHIFT.")]
		[InlineData("%", 1, "Key modifier for ALT.")]
		[InlineData("^o", 1, "Key modifier for CTRL with character.")]
		[InlineData("+o", 1, "Key modifier for SHIFT with character.")]
		[InlineData("%o", 1, "Key modifier for ALT with character.")]
		[InlineData("^ab", 2)]
		[InlineData("^(abc)", 1, "Key modifier for CTRL with multiple keys.")]
		[InlineData("+(abc)", 1, "Key modifier for SHIFT with multiple keys.")]
		[InlineData("%(abc)", 1, "Key modifier for ALT with multiple keys.")]
		[InlineData("+^%(abc)", 1, "All key modifiers with multiple keys.")]
		[InlineData("{ENTER}", 1, "Special key command by name.")]
		[InlineData("{ENTER 12}", 1, "Character repetition.")]
		[InlineData("[some possible command as per Microsoft docs]", 1, "Square brackets may represent commands.")]
		[InlineData("{[}not a command{]}", 15, "Escaped square brackets are not commands.")]
		[InlineData("~", 1, "~ represents the ENTER key.")]
		[InlineData("~a", 2)]
		[InlineData("o1{}}{%}{+}{^}o%o+o^o~o^(abc)o%(abc)+(abc){BACKSPACE},.$[][oo]{}", 23, "Almost all special cases in 1 at the same time.")]
		public void SplitSendKeysTextIntoCommands(string text, int expectedCommandCount, string reason = null)
		{
			var sut = new ProcessWritterService();
			var result = sut.SplitSendKeysTextIntoCommands(text);
			result.Count.Should().Be(expectedCommandCount, reason);
		}
	}
}
