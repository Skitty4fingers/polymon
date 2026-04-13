using FluentAssertions;
using BlazorMon.Domain.Enums;
using BlazorMon.Domain.Plugins;

namespace BlazorMon.Domain.Tests;

public class MonitorResultTests
{
    [Fact]
    public void Ok_SetsStatusToOk()
    {
        var result = MonitorResult.Ok("All good");
        result.Status.Should().Be(MonitorStatus.Ok);
        result.Message.Should().Be("All good");
        result.Counters.Should().BeEmpty();
    }

    [Fact]
    public void Warning_SetsStatusToWarning()
    {
        var result = MonitorResult.Warning("High CPU", new Dictionary<string, decimal> { ["CPU %"] = 85m });
        result.Status.Should().Be(MonitorStatus.Warning);
        result.Counters["CPU %"].Should().Be(85m);
    }

    [Fact]
    public void Fail_SetsStatusToFail()
    {
        var result = MonitorResult.Fail("Service down");
        result.Status.Should().Be(MonitorStatus.Fail);
        result.Message.Should().Be("Service down");
    }

    [Fact]
    public void Constructor_WithCounters_StoresAllCounters()
    {
        var counters = new Dictionary<string, decimal> { ["Disk %"] = 90m, ["Free GB"] = 10m };
        var result = MonitorResult.Ok("Disk check", counters);
        result.Counters.Should().HaveCount(2);
        result.Counters["Free GB"].Should().Be(10m);
    }
}
