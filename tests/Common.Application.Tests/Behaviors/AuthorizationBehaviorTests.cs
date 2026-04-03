using Common.Application.Attributes;
using Common.Application.Behaviors;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Mediator;
using FluentAssertions;
using Moq;

namespace Common.Application.Tests.Behaviors;

public class AuthorizationBehaviorTests
{
    private readonly Mock<IUser> _userMock = new();
    private readonly Mock<IIdentityService> _identityServiceMock = new();

    [Authorize(Resource = "Users", Action = "Read")]
    private class AuthorizedRequest : IRequest<string> { }

    private class NonAuthorizedRequest : IRequest<string> { }

    [Fact]
    public async Task Handle_NoAuthorizeAttribute_ProceedsToNext()
    {
        var behavior = new AuthorizationBehavior<NonAuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var result = await behavior.HandleAsync(new NonAuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_AuthorizeAttribute_NullUserId_ThrowsUnauthorized()
    {
        _userMock.Setup(u => u.Id).Returns((string?)null);
        var behavior = new AuthorizationBehavior<AuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var act = () => behavior.HandleAsync(new AuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_ServiceRole_ValidService_ProceedsToNext()
    {
        var userId = Guid.NewGuid().ToString();
        _userMock.Setup(u => u.Id).Returns(userId);
        _userMock.Setup(u => u.Role).Returns("service");
        _userMock.Setup(u => u.Client).Returns("TestClient");
        _identityServiceMock.Setup(s => s.IsValidServiceAsync("TestClient", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var behavior = new AuthorizationBehavior<AuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var result = await behavior.HandleAsync(new AuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_ServiceRole_InvalidService_ThrowsUnauthorized()
    {
        var userId = Guid.NewGuid().ToString();
        _userMock.Setup(u => u.Id).Returns(userId);
        _userMock.Setup(u => u.Role).Returns("service");
        _userMock.Setup(u => u.Client).Returns("BadClient");
        _identityServiceMock.Setup(s => s.IsValidServiceAsync("BadClient", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var behavior = new AuthorizationBehavior<AuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var act = () => behavior.HandleAsync(new AuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_UserRole_BothAuthorized_ProceedsToNext()
    {
        var userId = Guid.NewGuid();
        _userMock.Setup(u => u.Id).Returns(userId.ToString());
        _userMock.Setup(u => u.Role).Returns("admin");
        _identityServiceMock.Setup(s => s.IsInRoleAsync(userId, "Users", "Read", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _identityServiceMock.Setup(s => s.AuthorizeAsync(userId, "Users", "Read", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var behavior = new AuthorizationBehavior<AuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var result = await behavior.HandleAsync(new AuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_UserRole_NotInRole_ThrowsForbidden()
    {
        var userId = Guid.NewGuid();
        _userMock.Setup(u => u.Id).Returns(userId.ToString());
        _userMock.Setup(u => u.Role).Returns("user");
        _identityServiceMock.Setup(s => s.IsInRoleAsync(userId, "Users", "Read", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _identityServiceMock.Setup(s => s.AuthorizeAsync(userId, "Users", "Read", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var behavior = new AuthorizationBehavior<AuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var act = () => behavior.HandleAsync(new AuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_UserRole_NotAuthorized_ThrowsForbidden()
    {
        var userId = Guid.NewGuid();
        _userMock.Setup(u => u.Id).Returns(userId.ToString());
        _userMock.Setup(u => u.Role).Returns("user");
        _identityServiceMock.Setup(s => s.IsInRoleAsync(userId, "Users", "Read", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _identityServiceMock.Setup(s => s.AuthorizeAsync(userId, "Users", "Read", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var behavior = new AuthorizationBehavior<AuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var act = () => behavior.HandleAsync(new AuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_EmptyRole_ProceedsWithRoleCheck()
    {
        var userId = Guid.NewGuid();
        _userMock.Setup(u => u.Id).Returns(userId.ToString());
        _userMock.Setup(u => u.Role).Returns(string.Empty);
        _identityServiceMock.Setup(s => s.IsInRoleAsync(userId, "Users", "Read", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _identityServiceMock.Setup(s => s.AuthorizeAsync(userId, "Users", "Read", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var behavior = new AuthorizationBehavior<AuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var result = await behavior.HandleAsync(new AuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }
}
