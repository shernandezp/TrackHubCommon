using Common.Domain.Enums;
using FluentAssertions;

namespace Common.Domain.Tests.Enums;

public class EnumTests
{
    [Theory]
    [InlineData(AccountType.Personal, 1)]
    [InlineData(AccountType.Business, 2)]
    [InlineData(AccountType.Associate, 3)]
    public void AccountType_HasExpectedValues(AccountType type, int expected) =>
        ((int)type).Should().Be(expected);

    [Fact]
    public void AccountType_HasExpectedCount() =>
        Enum.GetValues<AccountType>().Should().HaveCount(3);

    [Theory]
    [InlineData(CategoryType.Product, 1)]
    [InlineData(CategoryType.Service, 2)]
    public void CategoryType_HasExpectedValues(CategoryType type, int expected) =>
        ((int)type).Should().Be(expected);

    [Theory]
    [InlineData(DeviceType.Aviation, 1)]
    [InlineData(DeviceType.Phone, 12)]
    [InlineData(DeviceType.Wearable, 15)]
    public void DeviceType_HasExpectedValues(DeviceType type, int expected) =>
        ((int)type).Should().Be(expected);

    [Fact]
    public void DeviceType_HasExpectedCount() =>
        Enum.GetValues<DeviceType>().Should().HaveCount(15);

    [Theory]
    [InlineData(ProtocolType.CommandTrack, 1)]
    [InlineData(ProtocolType.Traccar, 2)]
    [InlineData(ProtocolType.Wialon, 8)]
    public void ProtocolType_HasExpectedValues(ProtocolType type, int expected) =>
        ((int)type).Should().Be(expected);

    [Fact]
    public void ProtocolType_HasExpectedCount() =>
        Enum.GetValues<ProtocolType>().Should().HaveCount(10);

    [Theory]
    [InlineData(ReportType.Basic, 1)]
    [InlineData(ReportType.Custom, 2)]
    [InlineData(ReportType.External, 3)]
    public void ReportType_HasExpectedValues(ReportType type, int expected) =>
        ((int)type).Should().Be(expected);

    [Theory]
    [InlineData(TransporterType.Aircraft, 1)]
    [InlineData(TransporterType.Car, 5)]
    [InlineData(TransporterType.Pet, 18)]
    [InlineData(TransporterType.Tractor, 24)]
    public void TransporterType_HasExpectedValues(TransporterType type, int expected) =>
        ((int)type).Should().Be(expected);

    [Fact]
    public void TransporterType_HasExpectedCount() =>
        Enum.GetValues<TransporterType>().Should().HaveCount(24);
}
