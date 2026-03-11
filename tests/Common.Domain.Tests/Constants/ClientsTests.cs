using Common.Domain.Constants;
using FluentAssertions;

namespace Common.Domain.Tests.Constants;

public class ClientsTests
{
    [Theory]
    [InlineData(Clients.Hub, "Hub")]
    [InlineData(Clients.Geofence, "Geofence")]
    [InlineData(Clients.Identity, "Identity")]
    [InlineData(Clients.Manager, "Manager")]
    [InlineData(Clients.Router, "Router")]
    [InlineData(Clients.Security, "Security")]
    [InlineData(Clients.TripManagememt, "TripManagememt")]
    public void Clients_HaveExpectedValues(string actual, string expected) =>
        actual.Should().Be(expected);
}
