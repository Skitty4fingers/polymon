using FluentAssertions;
using PolyMon.Application.Services;
using PolyMon.Domain.Enums;
using PolyMon.Domain.Models;

namespace PolyMon.Application.Tests;

public class AlertEvaluationServiceTests
{
    private readonly AlertEvaluationService _sut = new();

    private static MonitorDef DefaultMonitor() => new()
    {
        MonitorId = 1,
        Name = "Test",
        AlertAfterEveryNewFailure = true,
        AlertAfterEveryFailToOk = true,
        AlertAfterEveryNewWarning = false,
        AlertAfterEveryWarnToOk = false,
        AlertAfterEveryNEvent = 0,
        OfflineTime1Start = "00:00",
        OfflineTime1End = "00:00",
        OfflineTime2Start = "00:00",
        OfflineTime2End = "00:00",
    };

    private static MonitorEvent Event(MonitorStatus status, int id = 1) => new()
    {
        EventId = id,
        StatusId = (int)status,
        EventDt = DateTime.UtcNow,
    };

    // --- New failure ---

    [Fact]
    public void ShouldSendAlert_NewFailure_ReturnsTrue_WhenAlertAfterEveryNewFailureEnabled()
    {
        var monitor = DefaultMonitor();
        var current = Event(MonitorStatus.Fail);
        var previous = Event(MonitorStatus.Ok);

        _sut.ShouldSendAlert(monitor, current, previous).Should().BeTrue();
    }

    [Fact]
    public void ShouldSendAlert_ContinuedFailure_ReturnsFalse_WhenOnlyNewFailureEnabled()
    {
        var monitor = DefaultMonitor();
        monitor.AlertAfterEveryNEvent = 0;
        var current = Event(MonitorStatus.Fail);
        var previous = Event(MonitorStatus.Fail);

        _sut.ShouldSendAlert(monitor, current, previous).Should().BeFalse();
    }

    // --- Fail-to-OK recovery ---

    [Fact]
    public void ShouldSendAlert_FailToOk_ReturnsTrue_WhenFailToOkEnabled()
    {
        var monitor = DefaultMonitor();
        var current = Event(MonitorStatus.Ok);
        var previous = Event(MonitorStatus.Fail);

        _sut.ShouldSendAlert(monitor, current, previous).Should().BeTrue();
    }

    [Fact]
    public void ShouldSendAlert_FailToOk_ReturnsFalse_WhenFailToOkDisabled()
    {
        var monitor = DefaultMonitor();
        monitor.AlertAfterEveryFailToOk = false;
        var current = Event(MonitorStatus.Ok);
        var previous = Event(MonitorStatus.Fail);

        _sut.ShouldSendAlert(monitor, current, previous).Should().BeFalse();
    }

    // --- Warning rules ---

    [Fact]
    public void ShouldSendAlert_NewWarning_ReturnsTrue_WhenAlertAfterEveryNewWarningEnabled()
    {
        var monitor = DefaultMonitor();
        monitor.AlertAfterEveryNewWarning = true;
        var current = Event(MonitorStatus.Warning);
        var previous = Event(MonitorStatus.Ok);

        _sut.ShouldSendAlert(monitor, current, previous).Should().BeTrue();
    }

    [Fact]
    public void ShouldSendAlert_NewWarning_ReturnsFalse_WhenAlertAfterEveryNewWarningDisabled()
    {
        var monitor = DefaultMonitor();
        monitor.AlertAfterEveryNewWarning = false;
        var current = Event(MonitorStatus.Warning);
        var previous = Event(MonitorStatus.Ok);

        _sut.ShouldSendAlert(monitor, current, previous).Should().BeFalse();
    }

    [Fact]
    public void ShouldSendAlert_WarnToOk_ReturnsTrue_WhenWarnToOkEnabled()
    {
        var monitor = DefaultMonitor();
        monitor.AlertAfterEveryWarnToOk = true;
        var current = Event(MonitorStatus.Ok);
        var previous = Event(MonitorStatus.Warning);

        _sut.ShouldSendAlert(monitor, current, previous).Should().BeTrue();
    }

    // --- Every N events ---

    [Fact]
    public void ShouldSendAlert_NthEvent_ReturnsTrue_WhenEventIdDivisibleByN()
    {
        var monitor = DefaultMonitor();
        monitor.AlertAfterEveryNewFailure = false;
        monitor.AlertAfterEveryNEvent = 5;
        var current = Event(MonitorStatus.Fail, id: 10);
        var previous = Event(MonitorStatus.Fail);

        _sut.ShouldSendAlert(monitor, current, previous).Should().BeTrue();
    }

    [Fact]
    public void ShouldSendAlert_NthEvent_ReturnsFalse_WhenEventIdNotDivisibleByN()
    {
        var monitor = DefaultMonitor();
        monitor.AlertAfterEveryNewFailure = false;
        monitor.AlertAfterEveryNEvent = 5;
        var current = Event(MonitorStatus.Fail, id: 7);
        var previous = Event(MonitorStatus.Fail);

        _sut.ShouldSendAlert(monitor, current, previous).Should().BeFalse();
    }

    // --- First event (no previous) ---

    [Fact]
    public void ShouldSendAlert_FirstEvent_Fail_ReturnsTrue()
    {
        var monitor = DefaultMonitor();
        var current = Event(MonitorStatus.Fail);

        _sut.ShouldSendAlert(monitor, current, null).Should().BeTrue();
    }

    [Fact]
    public void ShouldSendAlert_FirstEvent_Ok_ReturnsFalse()
    {
        var monitor = DefaultMonitor();
        var current = Event(MonitorStatus.Ok);

        _sut.ShouldSendAlert(monitor, current, null).Should().BeFalse();
    }

    // --- Offline time suppression ---

    [Fact]
    public void ShouldSendAlert_InOfflineWindow_ReturnsFalse()
    {
        var monitor = DefaultMonitor();
        // Set offline window to cover the whole day
        monitor.OfflineTime1Start = "00:00";
        monitor.OfflineTime1End = "23:59";

        var current = Event(MonitorStatus.Fail);
        var previous = Event(MonitorStatus.Ok);

        _sut.ShouldSendAlert(monitor, current, previous).Should().BeFalse();
    }

    // --- IsOfflineTime tests ---

    [Fact]
    public void IsOfflineTime_NoWindows_ReturnsFalse()
    {
        var monitor = DefaultMonitor();
        _sut.IsOfflineTime(monitor).Should().BeFalse();
    }

    [Fact]
    public void IsOfflineTime_FullDayWindow_ReturnsTrue()
    {
        var monitor = DefaultMonitor();
        monitor.OfflineTime1Start = "00:00";
        monitor.OfflineTime1End = "23:59";

        _sut.IsOfflineTime(monitor).Should().BeTrue();
    }
}
