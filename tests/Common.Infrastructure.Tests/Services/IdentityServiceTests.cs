using Common.Application.Interfaces;
using Common.Infrastructure;
using FluentAssertions;
using GraphQL;
using GraphQL.Client.Abstractions;
using Moq;

namespace Common.Infrastructure.Tests.Services;

public class IdentityServiceTests
{
    private readonly Mock<IGraphQLClient> _mockClient = new();
    private readonly Mock<IGraphQLClientFactory> _mockFactory = new();
    private readonly IdentityService _service;

    public IdentityServiceTests()
    {
        _mockFactory.Setup(f => f.CreateClient("Identity")).Returns(_mockClient.Object);
        _service = new IdentityService(_mockFactory.Object);
    }

    [Fact]
    public async Task GetUserNameAsync_ReturnsUserName()
    {
        var response = new GraphQLResponse<object>
        {
            Data = System.Text.Json.JsonSerializer.Deserialize<object>("""{"userName":"John"}""")!
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _service.GetUserNameAsync(Guid.NewGuid(), CancellationToken.None);
        result.Should().Be("John");
    }

    [Fact]
    public async Task AuthorizeAsync_ReturnsTrue()
    {
        var response = new GraphQLResponse<object>
        {
            Data = System.Text.Json.JsonSerializer.Deserialize<object>("""{"authorize":true}""")!
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _service.AuthorizeAsync(Guid.NewGuid(), "Resource", "Read", CancellationToken.None);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsInRoleAsync_ReturnsFalse()
    {
        var response = new GraphQLResponse<object>
        {
            Data = System.Text.Json.JsonSerializer.Deserialize<object>("""{"isInRole":false}""")!
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _service.IsInRoleAsync(Guid.NewGuid(), "Resource", "Write", CancellationToken.None);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsValidServiceAsync_ReturnsTrue()
    {
        var response = new GraphQLResponse<object>
        {
            Data = System.Text.Json.JsonSerializer.Deserialize<object>("""{"isValidService":true}""")!
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _service.IsValidServiceAsync("Hub", CancellationToken.None);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserNameAsync_WithGraphQLError_ThrowsException()
    {
        var response = new GraphQLResponse<object>
        {
            Data = new object(),
            Errors = [new GraphQLError { Message = "Not found" }]
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var act = () => _service.GetUserNameAsync(Guid.NewGuid(), CancellationToken.None);
        await act.Should().ThrowAsync<HotChocolate.GraphQLException>();
    }
}
