using Common.Application.GraphQL.Types;
using FluentAssertions;
using HotChocolate.Language;

namespace Common.Application.Tests.GraphQL;

public class AnyTypeTests
{
    private readonly AnyType _sut = new();

    [Fact]
    public void RuntimeType_IsObject()
    {
        _sut.RuntimeType.Should().Be(typeof(object));
    }

    [Theory]
    [InlineData(typeof(NullValueNode))]
    [InlineData(typeof(IntValueNode))]
    [InlineData(typeof(BooleanValueNode))]
    [InlineData(typeof(StringValueNode))]
    public void IsInstanceOfType_SupportedTypes_ReturnsTrue(Type nodeType)
    {
        IValueNode node = nodeType.Name switch
        {
            nameof(NullValueNode) => NullValueNode.Default,
            nameof(IntValueNode) => new IntValueNode(42),
            nameof(BooleanValueNode) => new BooleanValueNode(true),
            nameof(StringValueNode) => new StringValueNode("test"),
            _ => throw new InvalidOperationException()
        };
        _sut.IsInstanceOfType(node).Should().BeTrue();
    }

    [Fact]
    public void IsInstanceOfType_FloatValueNode_ReturnsTrue()
    {
        var node = new FloatValueNode(3.14);
        _sut.IsInstanceOfType(node).Should().BeTrue();
    }

    [Fact]
    public void IsInstanceOfType_UnsupportedType_ReturnsFalse()
    {
        var node = new ListValueNode([]);
        _sut.IsInstanceOfType(node).Should().BeFalse();
    }

    [Fact]
    public void ParseValue_Null_ReturnsNullValueNode()
    {
        var result = _sut.ParseValue(null);
        result.Should().Be(NullValueNode.Default);
    }

    [Fact]
    public void ParseValue_Int_ReturnsIntValueNode()
    {
        var result = _sut.ParseValue(42);
        result.Should().BeOfType<IntValueNode>();
    }

    [Fact]
    public void ParseValue_Long_ReturnsIntValueNode()
    {
        var result = _sut.ParseValue(42L);
        result.Should().BeOfType<IntValueNode>();
    }

    [Fact]
    public void ParseValue_Float_ReturnsFloatValueNode()
    {
        var result = _sut.ParseValue(3.14f);
        result.Should().BeOfType<FloatValueNode>();
    }

    [Fact]
    public void ParseValue_Double_ReturnsFloatValueNode()
    {
        var result = _sut.ParseValue(3.14);
        result.Should().BeOfType<FloatValueNode>();
    }

    [Fact]
    public void ParseValue_Bool_ReturnsBooleanValueNode()
    {
        var result = _sut.ParseValue(true);
        result.Should().BeOfType<BooleanValueNode>();
    }

    [Fact]
    public void ParseValue_String_ReturnsStringValueNode()
    {
        var result = _sut.ParseValue("hello");
        result.Should().BeOfType<StringValueNode>();
    }

    [Fact]
    public void ParseValue_Guid_ReturnsStringValueNode()
    {
        var guid = Guid.NewGuid();
        var result = _sut.ParseValue(guid);
        result.Should().BeOfType<StringValueNode>();
        ((StringValueNode)result).Value.Should().Be(guid.ToString());
    }

    [Fact]
    public void ParseValue_UnsupportedType_ThrowsNotSupportedException()
    {
        var act = () => _sut.ParseValue(new DateTime(2026, 1, 1));
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void ParseLiteral_NullValueNode_ReturnsNull()
    {
        var result = _sut.ParseLiteral(NullValueNode.Default);
        result.Should().BeNull();
    }

    [Fact]
    public void ParseLiteral_IntValueNode_ReturnsInt()
    {
        var result = _sut.ParseLiteral(new IntValueNode(42));
        result.Should().Be(42);
    }

    [Fact]
    public void ParseLiteral_FloatValueNode_ReturnsDouble()
    {
        var result = _sut.ParseLiteral(new FloatValueNode(3.14));
        result.Should().BeOfType<double>();
    }

    [Fact]
    public void ParseLiteral_BooleanValueNode_ReturnsBool()
    {
        var result = _sut.ParseLiteral(new BooleanValueNode(true));
        result.Should().Be(true);
    }

    [Fact]
    public void ParseLiteral_StringWithGuid_ReturnsGuid()
    {
        var guid = Guid.NewGuid();
        var result = _sut.ParseLiteral(new StringValueNode(guid.ToString()));
        result.Should().Be(guid);
    }

    [Fact]
    public void ParseLiteral_UnsupportedType_ThrowsNotSupportedException()
    {
        var act = () => _sut.ParseLiteral(new ListValueNode([]));
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void ParseResult_DelegatesToParseValue()
    {
        var result = _sut.ParseResult(42);
        result.Should().BeOfType<IntValueNode>();
    }

    [Fact]
    public void TrySerialize_ReturnsTrue_AndSameValue()
    {
        var success = _sut.TrySerialize("hello", out var resultValue);
        success.Should().BeTrue();
        resultValue.Should().Be("hello");
    }

    [Fact]
    public void TryDeserialize_ReturnsTrue_AndSameValue()
    {
        var success = _sut.TryDeserialize(42, out var runtimeValue);
        success.Should().BeTrue();
        runtimeValue.Should().Be(42);
    }
}
