using System.Reflection;
using Moq;

namespace pvNugsEnumConvNc9.ut;

using System.ComponentModel;
using Xunit;

public class EnumConvertTests
{
    public enum TestEnu
    {
        [Description("A")]
        ValueA,
        
        [Description("B,BETA")]
        ValueB,
        
        [Description("C,CHARLIE,SEE")]
        ValueC
    }

    private enum EmptyEnu
    {
        NoDescription
    }

    [Fact]
    public void GetCode_SingleDescription_ReturnsCode()
    {
        // Arrange
        var value = TestEnu.ValueA;

        // Act
        var result = value.GetCode();

        // Assert
        Assert.Equal("A", result);
    }

    [Fact]
    public void GetCode_MultipleDescriptions_ReturnsFirstCode()
    {
        // Arrange
        var value = TestEnu.ValueB;

        // Act
        var result = value.GetCode();

        // Assert
        Assert.Equal("B", result);
    }

    [Fact]
    public void GetCode_NoDescription_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var value = EmptyEnu.NoDescription;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => value.GetCode());
    }

    [Fact]
    public void GetValue_WithMatchingCode_ReturnsEnum()
    {
        // Arrange & Act
        var result = EnumConvert.GetValue<TestEnu>("A", (x, y) => x == y);

        // Assert
        Assert.Equal(TestEnu.ValueA, result);
    }

    [Fact]
    public void GetValue_WithNullCode_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            EnumConvert.GetValue<TestEnu>(null!, (x, y) => x == y));
    }

    [Fact]
    public void GetValue_WithNonExistentCode_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            EnumConvert.GetValue<TestEnu>("X", (x, y) => x == y));
    }

    [Fact]
    public void GetValue_WithDefaultValue_ReturnsMatchingEnum()
    {
        // Arrange & Act
        var result = EnumConvert.GetValue("B", TestEnu.ValueA);

        // Assert
        Assert.Equal(TestEnu.ValueB, result);
    }

    [Fact]
    public void GetValue_WithDefaultValue_AndNullCode_ReturnsDefault()
    {
        // Arrange & Act
        var result = EnumConvert.GetValue(null, TestEnu.ValueA);

        // Assert
        Assert.Equal(TestEnu.ValueA, result);
    }

    [Fact]
    public void GetValue_WithDefaultValue_AndNonExistentCode_ReturnsDefault()
    {
        // Arrange & Act
        var result = EnumConvert.GetValue("X", TestEnu.ValueA);

        // Assert
        Assert.Equal(TestEnu.ValueA, result);
    }

    [Fact]
    public void GetValue_WithCustomMatcher_MatchesUsingCustomLogic()
    {
        // Arrange
        bool CustomMatcher(string x, string y) => x.StartsWith(y);

        // Act
        var result = EnumConvert.GetValue("CHAR", TestEnu.ValueA, CustomMatcher);

        // Assert
        Assert.Equal(TestEnu.ValueC, result);
    }

    [Theory]
    [InlineData("a", TestEnu.ValueA)]
    [InlineData("A", TestEnu.ValueA)]
    [InlineData("BETA", TestEnu.ValueB)]
    [InlineData("beta", TestEnu.ValueB)]
    [InlineData("SEE", TestEnu.ValueC)]
    public void GetValue_DefaultMatcher_IsCaseInsensitive(string code, TestEnu expected)
    {
        // Arrange & Act
        var result = EnumConvert.GetValue(code, TestEnu.ValueA);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetValue_WithMultipleDescriptions_MatchesAnyCode()
    {
        // Act & Assert
        Assert.Equal(TestEnu.ValueC, EnumConvert.GetValue("C", TestEnu.ValueA));
        Assert.Equal(TestEnu.ValueC, EnumConvert.GetValue("CHARLIE", TestEnu.ValueA));
        Assert.Equal(TestEnu.ValueC, EnumConvert.GetValue("SEE", TestEnu.ValueA));
    }

}