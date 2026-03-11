using Common.Domain.Constants;
using FluentAssertions;

namespace Common.Domain.Tests.Constants;

public class ViewMetadataTests
{
    [Theory]
    [InlineData(ViewMetadata.VwGeofence, "vw_geofence")]
    [InlineData(ViewMetadata.VwTransporterPosition, "vw_transporter_position")]
    [InlineData(ViewMetadata.VwUsers, "vw_users")]
    public void ViewMetadata_HaveExpectedValues(string actual, string expected) =>
        actual.Should().Be(expected);
}
