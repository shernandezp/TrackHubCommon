using Common.Application.Exceptions;
using FluentAssertions;

namespace Common.Application.Tests.Exceptions;

public class ForbiddenAccessExceptionTests
{
    [Fact]
    public void Constructor_CreatesException()
    {
        var ex = new ForbiddenAccessException();
        ex.Should().BeOfType<ForbiddenAccessException>();
        ex.Should().BeAssignableTo<Exception>();
    }
}
