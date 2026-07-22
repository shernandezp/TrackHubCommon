using Common.Application.Attributes;
using Common.Application.Behaviors;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Mediator;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Common.Application.Tests.Behaviors;

public class AccountScopeBehaviorTests
{
    private readonly Mock<ICurrentPrincipal> _principalMock = new();
    private readonly Mock<ILogger<AccountScopeBehavior<AccountScopedRequest, string>>> _scopedLoggerMock = new();
    private readonly Mock<ILogger<AccountScopeBehavior<PlainRequest, string>>> _plainLoggerMock = new();
    private readonly Mock<ILogger<AccountScopeBehavior<CrossAccountRequest, string>>> _crossLoggerMock = new();

    public class PlainRequest : IRequest<string> { }

    public class AccountScopedRequest : IRequest<string>
    {
        public Guid AccountId { get; init; }
    }

    [AllowCrossAccount("Test fixture standing in for the Router/SyncWorker global feed.")]
    public class CrossAccountRequest : IRequest<string>
    {
        public Guid AccountId { get; init; }
    }

    private AccountScopeBehavior<AccountScopedRequest, string> ScopedBehavior()
        => new(_principalMock.Object, _scopedLoggerMock.Object);

    [Fact]
    public async Task Handle_SameAccount_ProceedsToNext()
    {
        var accountId = Guid.NewGuid();
        _principalMock.Setup(p => p.AccountId).Returns(accountId);

        var result = await ScopedBehavior().HandleAsync(
            new AccountScopedRequest { AccountId = accountId }, () => Task.FromResult("OK"), CancellationToken.None);

        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_CrossAccount_ThrowsForbidden()
    {
        _principalMock.Setup(p => p.AccountId).Returns(Guid.NewGuid());

        var act = () => ScopedBehavior().HandleAsync(
            new AccountScopedRequest { AccountId = Guid.NewGuid() }, () => Task.FromResult("OK"), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_PrincipalWithNoAccount_ThrowsForbidden()
    {
        _principalMock.Setup(p => p.AccountId).Returns((Guid?)null);
        _principalMock.Setup(p => p.PrincipalType).Returns(PrincipalType.ServiceClient);

        var act = () => ScopedBehavior().HandleAsync(
            new AccountScopedRequest { AccountId = Guid.NewGuid() }, () => Task.FromResult("OK"), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_PrincipalWithEmptyAccount_ThrowsForbidden()
    {
        _principalMock.Setup(p => p.AccountId).Returns(Guid.Empty);

        var act = () => ScopedBehavior().HandleAsync(
            new AccountScopedRequest { AccountId = Guid.NewGuid() }, () => Task.FromResult("OK"), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_AllowCrossAccount_PermitsMismatch()
    {
        _principalMock.Setup(p => p.AccountId).Returns(Guid.NewGuid());
        var behavior = new AccountScopeBehavior<CrossAccountRequest, string>(
            _principalMock.Object, _crossLoggerMock.Object);

        var result = await behavior.HandleAsync(
            new CrossAccountRequest { AccountId = Guid.NewGuid() }, () => Task.FromResult("OK"), CancellationToken.None);

        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_AllowCrossAccount_PermitsGlobalServiceIdentityWithNoAccount()
    {
        _principalMock.Setup(p => p.AccountId).Returns((Guid?)null);
        _principalMock.Setup(p => p.PrincipalType).Returns(PrincipalType.ServiceClient);
        var behavior = new AccountScopeBehavior<CrossAccountRequest, string>(
            _principalMock.Object, _crossLoggerMock.Object);

        var result = await behavior.HandleAsync(
            new CrossAccountRequest { AccountId = Guid.NewGuid() }, () => Task.FromResult("OK"), CancellationToken.None);

        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_RequestWithoutAccountId_IsUnaffected()
    {
        _principalMock.Setup(p => p.AccountId).Returns((Guid?)null);
        var behavior = new AccountScopeBehavior<PlainRequest, string>(
            _principalMock.Object, _plainLoggerMock.Object);

        var result = await behavior.HandleAsync(
            new PlainRequest(), () => Task.FromResult("OK"), CancellationToken.None);

        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_EmptyAccountIdOnRequest_IsUnaffected()
    {
        // Guid.Empty means "the request named no account"; scope is then resolved by the handler
        // from the principal itself, which cannot cross tenants.
        _principalMock.Setup(p => p.AccountId).Returns(Guid.NewGuid());

        var result = await ScopedBehavior().HandleAsync(
            new AccountScopedRequest { AccountId = Guid.Empty }, () => Task.FromResult("OK"), CancellationToken.None);

        result.Should().Be("OK");
    }

    [Fact]
    public void AllowCrossAccount_RequiresAJustification()
    {
        var act = () => new AllowCrossAccountAttribute("  ");

        act.Should().Throw<ArgumentException>();
    }

    // ---------------------------------------------------------------------------------------
    // Nested account ids. Several commands carry the tenant inside a DTO member
    // (CreateBackgroundJobRunCommand, RecordAlertEventCommand, CreateAuditEventCommand,
    // CreatePublicLinkGrantCommand, Router's OperatorVm-carrying sync commands). Before
    // TrackHubCommon 1.0.7 those escaped the guard purely by SHAPE. These tests are the proof
    // that they no longer do — the handler-level unit suites never run the pipeline, so nothing
    // else in the platform exercises this.
    // ---------------------------------------------------------------------------------------

    public readonly record struct AccountBearingDto(Guid AccountId, string Payload);

    public readonly record struct AccountlessDto(string Payload);

    public readonly record struct OuterDto(AccountBearingDto Inner);

    public readonly record struct DeeperDto(OuterDto Middle);

    public class NestedAccountRequest : IRequest<string>
    {
        public AccountBearingDto Dto { get; init; }
    }

    [AllowCrossAccount("Test fixture standing in for a global service identity emitting per-tenant events.")]
    public class NestedCrossAccountRequest : IRequest<string>
    {
        public AccountBearingDto Dto { get; init; }
    }

    public class NoAccountAnywhereRequest : IRequest<string>
    {
        public AccountlessDto Dto { get; init; }

        public string Name { get; init; } = string.Empty;

        public Uri Endpoint { get; init; } = new("https://example.invalid");
    }

    /// <summary>Top-level and nested accounts disagree — the root must win.</summary>
    public class TopLevelAndNestedRequest : IRequest<string>
    {
        public Guid AccountId { get; init; }

        public AccountBearingDto Dto { get; init; }
    }

    /// <summary>Account two levels below the root (request → OuterDto → AccountBearingDto).</summary>
    public class DepthTwoRequest : IRequest<string>
    {
        public OuterDto Outer { get; init; }
    }

    /// <summary>Account three levels below the root — deliberately BEYOND the depth limit.</summary>
    public class DepthThreeRequest : IRequest<string>
    {
        public DeeperDto Deep { get; init; }
    }

    /// <summary>A batch names many accounts, not one: collections are deliberately not walked.</summary>
    public class CollectionNestedRequest : IRequest<string>
    {
        public IReadOnlyCollection<AccountBearingDto> Items { get; init; } = [];
    }

    public class NullableNestedRequest : IRequest<string>
    {
        public AccountBearingDto? Dto { get; init; }
    }

    private AccountScopeBehavior<TRequest, string> BehaviorFor<TRequest>() where TRequest : notnull
        => new(_principalMock.Object, Mock.Of<ILogger<AccountScopeBehavior<TRequest, string>>>());

    [Fact]
    public async Task Handle_NestedAccount_SameAccount_ProceedsToNext()
    {
        var accountId = Guid.NewGuid();
        _principalMock.Setup(p => p.AccountId).Returns(accountId);

        var result = await BehaviorFor<NestedAccountRequest>().HandleAsync(
            new NestedAccountRequest { Dto = new AccountBearingDto(accountId, "x") },
            () => Task.FromResult("OK"),
            CancellationToken.None);

        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_NestedAccount_CrossAccount_ThrowsForbidden()
    {
        _principalMock.Setup(p => p.AccountId).Returns(Guid.NewGuid());

        var act = () => BehaviorFor<NestedAccountRequest>().HandleAsync(
            new NestedAccountRequest { Dto = new AccountBearingDto(Guid.NewGuid(), "x") },
            () => Task.FromResult("OK"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_NestedAccount_GlobalServiceIdentity_ThrowsForbidden()
    {
        _principalMock.Setup(p => p.AccountId).Returns((Guid?)null);
        _principalMock.Setup(p => p.PrincipalType).Returns(PrincipalType.ServiceClient);

        var act = () => BehaviorFor<NestedAccountRequest>().HandleAsync(
            new NestedAccountRequest { Dto = new AccountBearingDto(Guid.NewGuid(), "x") },
            () => Task.FromResult("OK"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_NestedAccount_AllowCrossAccount_PermitsMismatch()
    {
        _principalMock.Setup(p => p.AccountId).Returns((Guid?)null);
        _principalMock.Setup(p => p.PrincipalType).Returns(PrincipalType.ServiceClient);

        var result = await BehaviorFor<NestedCrossAccountRequest>().HandleAsync(
            new NestedCrossAccountRequest { Dto = new AccountBearingDto(Guid.NewGuid(), "x") },
            () => Task.FromResult("OK"),
            CancellationToken.None);

        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_NoAccountAnywhere_IsUnaffected()
    {
        _principalMock.Setup(p => p.AccountId).Returns((Guid?)null);

        var result = await BehaviorFor<NoAccountAnywhereRequest>().HandleAsync(
            new NoAccountAnywhereRequest { Dto = new AccountlessDto("x") },
            () => Task.FromResult("OK"),
            CancellationToken.None);

        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_NestedEmptyAccount_IsUnaffected()
    {
        _principalMock.Setup(p => p.AccountId).Returns(Guid.NewGuid());

        var result = await BehaviorFor<NestedAccountRequest>().HandleAsync(
            new NestedAccountRequest { Dto = new AccountBearingDto(Guid.Empty, "x") },
            () => Task.FromResult("OK"),
            CancellationToken.None);

        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_NullNestedDto_IsUnaffected()
    {
        _principalMock.Setup(p => p.AccountId).Returns(Guid.NewGuid());

        var result = await BehaviorFor<NullableNestedRequest>().HandleAsync(
            new NullableNestedRequest { Dto = null },
            () => Task.FromResult("OK"),
            CancellationToken.None);

        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_TopLevelAccountWinsOverNested_PassesOnMatchingTopLevel()
    {
        var accountId = Guid.NewGuid();
        _principalMock.Setup(p => p.AccountId).Returns(accountId);

        // The nested DTO names a foreign account; the root names the caller's own. The ROOT is
        // authoritative, so this passes — the handler is responsible for not trusting the DTO's
        // copy of the id (the platform convention is to overwrite it from the root).
        var result = await BehaviorFor<TopLevelAndNestedRequest>().HandleAsync(
            new TopLevelAndNestedRequest
            {
                AccountId = accountId,
                Dto = new AccountBearingDto(Guid.NewGuid(), "x")
            },
            () => Task.FromResult("OK"),
            CancellationToken.None);

        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_TopLevelAccountWinsOverNested_ForbidsOnMismatchingTopLevel()
    {
        var accountId = Guid.NewGuid();
        _principalMock.Setup(p => p.AccountId).Returns(accountId);

        // Converse of the above: a matching nested id cannot rescue a foreign root id.
        var act = () => BehaviorFor<TopLevelAndNestedRequest>().HandleAsync(
            new TopLevelAndNestedRequest
            {
                AccountId = Guid.NewGuid(),
                Dto = new AccountBearingDto(accountId, "x")
            },
            () => Task.FromResult("OK"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_AccountTwoLevelsDeep_IsGuarded()
    {
        _principalMock.Setup(p => p.AccountId).Returns(Guid.NewGuid());

        var act = () => BehaviorFor<DepthTwoRequest>().HandleAsync(
            new DepthTwoRequest { Outer = new OuterDto(new AccountBearingDto(Guid.NewGuid(), "x")) },
            () => Task.FromResult("OK"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_AccountBeyondTheDepthLimit_IsNotSeen()
    {
        // Documents the bound, not an endorsement: an account buried three levels deep is out of
        // the guard's reach by design. Requests must not nest the tenant that far — the platform
        // convention is a top-level AccountId.
        _principalMock.Setup(p => p.AccountId).Returns(Guid.NewGuid());

        var result = await BehaviorFor<DepthThreeRequest>().HandleAsync(
            new DepthThreeRequest { Deep = new DeeperDto(new OuterDto(new AccountBearingDto(Guid.NewGuid(), "x"))) },
            () => Task.FromResult("OK"),
            CancellationToken.None);

        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_AccountInsideACollection_IsNotSeen()
    {
        // A batch names zero or many accounts, so there is no single account to bind to. Such a
        // request is a cross-account surface and must declare itself with [AllowCrossAccount].
        _principalMock.Setup(p => p.AccountId).Returns(Guid.NewGuid());

        var result = await BehaviorFor<CollectionNestedRequest>().HandleAsync(
            new CollectionNestedRequest { Items = [new AccountBearingDto(Guid.NewGuid(), "x")] },
            () => Task.FromResult("OK"),
            CancellationToken.None);

        result.Should().Be("OK");
    }
}
