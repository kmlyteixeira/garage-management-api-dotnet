using System;
using Shouldly;
using Xunit;

namespace GarageManagement;

public class DocumentTests
{
    [Fact]
    public void Constructor_Should_Remove_Formatting_Characters()
    {
        var document = new Document("123.456.789-01");

        document.Value.ShouldBe("12345678901");
    }

    [Fact]
    public void Constructor_Should_Accept_Cnpj_With_Formatting()
    {
        var document = new Document("12.345.678/0001-95");

        document.Value.ShouldBe("12345678000195");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_Should_Reject_Empty_Value(string value)
    {
        Should.Throw<ArgumentException>(() => new Document(value));
    }

    [Theory]
    [InlineData("1234567890")]
    [InlineData("123456789012")]
    public void Constructor_Should_Reject_Invalid_Length(string value)
    {
        Should.Throw<ArgumentException>(() => new Document(value));
    }
}