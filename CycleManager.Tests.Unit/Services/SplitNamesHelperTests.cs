using CycleManager.Services.Helpers;
using FluentAssertions;

namespace CycleManager.Tests.Unit.Services.Helpers
{
    public class SplitNamesHelperTests
    {
        [Fact]
        public void SplitName_Null_ReturnsUnknown()
        {
            var result = SplitNamesHelper.SplitName(null!);

            result.FirstName.Should().Be("Unknown");
            result.LastName.Should().Be("Unknown");
        }

        [Fact]
        public void SplitName_EmptyString_ReturnsUnknown()
        {
            var result = SplitNamesHelper.SplitName("");

            result.FirstName.Should().Be("Unknown");
            result.LastName.Should().Be("Unknown");
        }

        [Fact]
        public void SplitName_Whitespace_ReturnsUnknown()
        {
            var result = SplitNamesHelper.SplitName("   ");

            result.FirstName.Should().Be("Unknown");
            result.LastName.Should().Be("Unknown");
        }

        [Fact]
        public void SplitName_SingleName_ReturnsNameAsFirstName()
        {
            var result = SplitNamesHelper.SplitName("Pogacar");

            result.FirstName.Should().Be("Pogacar");
            result.LastName.Should().Be("");
        }

        [Fact]
        public void SplitName_TwoNames_SplitsIntoLastNameAndFirstName()
        {
            var result = SplitNamesHelper.SplitName("Tadej Pogacar");

            result.FirstName.Should().Be("Pogacar");
            result.LastName.Should().Be("Tadej");
        }

        [Fact]
        public void SplitName_MultipleNames_UsesLastPartAsFirstName()
        {
            var result = SplitNamesHelper.SplitName("Juan Ayuso Pesquera");

            result.FirstName.Should().Be("Pesquera");
            result.LastName.Should().Be("Juan Ayuso");
        }

        [Fact]
        public void SplitName_TrimsAndRemovesExtraSpaces()
        {
            var result = SplitNamesHelper.SplitName("  Tadej   Pogacar  ");

            result.FirstName.Should().Be("Pogacar");
            result.LastName.Should().Be("Tadej");
        }

        [Fact]
        public void FormatPart_NullOrWhitespace_ReturnsEmpty()
        {
            SplitNamesHelper.FormatPart(null!)
                .Should().Be("");

            SplitNamesHelper.FormatPart("")
                .Should().Be("");

            SplitNamesHelper.FormatPart("   ")
                .Should().Be("");
        }

        [Theory]
        [InlineData("van", "van")]
        [InlineData("Van", "van")]
        [InlineData("DER", "der")]
        [InlineData("de", "de")]
        [InlineData("den", "den")]
        [InlineData("del", "del")]
        [InlineData("della", "della")]
        [InlineData("la", "la")]
        [InlineData("le", "le")]
        [InlineData("di", "di")]
        [InlineData("da", "da")]
        [InlineData("dos", "dos")]
        [InlineData("das", "das")]
        [InlineData("von", "von")]
        public void FormatPart_LastnamePrefix_ReturnsLowercase(
            string input,
            string expected)
        {
            SplitNamesHelper.FormatPart(input)
                .Should().Be(expected);
        }

        [Theory]
        [InlineData("pogacar", "Pogacar")]
        [InlineData("POGACAR", "Pogacar")]
        [InlineData("aYuso", "Ayuso")]
        [InlineData("mAyEr", "Mayer")]
        public void FormatPart_NormalName_CapitalizesFirstLetter(
            string input,
            string expected)
        {
            SplitNamesHelper.FormatPart(input)
                .Should().Be(expected);
        }

        [Fact]
        public void FormatName_NullOrWhitespace_ReturnsEmpty()
        {
            SplitNamesHelper.FormatName(null!)
                .Should().Be("");

            SplitNamesHelper.FormatName("")
                .Should().Be("");

            SplitNamesHelper.FormatName("   ")
                .Should().Be("");
        }

        [Fact]
        public void FormatName_FormatsAllParts()
        {
            var result = SplitNamesHelper.FormatName("TADEJ POGACAR");

            result.Should().Be("Tadej Pogacar");
        }

        [Fact]
        public void FormatName_FormatsPrefixesCorrectly()
        {
            var result = SplitNamesHelper.FormatName("JAN VAN DER POEL");

            result.Should().Be("Jan van der Poel");
        }

        [Fact]
        public void FormatName_RemovesExtraSpaces()
        {
            var result = SplitNamesHelper.FormatName("  JAN   VAN   DER   POEL  ");

            result.Should().Be("Jan van der Poel");
        }
    }
}