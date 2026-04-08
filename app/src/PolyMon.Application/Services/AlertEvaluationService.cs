using PolyMon.Domain.Enums;
using PolyMon.Domain.Models;

namespace PolyMon.Application.Services;

public class AlertEvaluationService
{
    public bool ShouldSendAlert(MonitorDef monitor, MonitorEvent current, MonitorEvent? previous)
    {
        if (IsOfflineTime(monitor)) return false;

        var prevStatus = previous is null ? MonitorStatus.Ok : (MonitorStatus)previous.StatusId;
        var currStatus = (MonitorStatus)current.StatusId;

        if (currStatus == MonitorStatus.Ok)
        {
            if (prevStatus == MonitorStatus.Fail && monitor.AlertAfterEveryFailToOk) return true;
            if (prevStatus == MonitorStatus.Warning && monitor.AlertAfterEveryWarnToOk) return true;
            return false;
        }

        if (currStatus == MonitorStatus.Fail)
        {
            if (monitor.AlertAfterEveryNewFailure && prevStatus != MonitorStatus.Fail) return true;
            if (monitor.AlertAfterEveryNEvent > 0 && current.EventId % monitor.AlertAfterEveryNEvent == 0) return true;
            return false;
        }

        if (currStatus == MonitorStatus.Warning)
        {
            if (monitor.AlertAfterEveryNewWarning && prevStatus != MonitorStatus.Warning) return true;
            if (monitor.AlertAfterEveryNEvent > 0 && current.EventId % monitor.AlertAfterEveryNEvent == 0) return true;
            return false;
        }

        return false;
    }

    public bool IsOfflineTime(MonitorDef monitor)
    {
        return IsInWindow(monitor.OfflineTime1Start, monitor.OfflineTime1End) ||
               IsInWindow(monitor.OfflineTime2Start, monitor.OfflineTime2End);
    }

    private static bool IsInWindow(string start, string end)
    {
        if (start == "00:00" && end == "00:00") return false;

        var now = DateTime.Now.TimeOfDay;
        if (!TimeSpan.TryParse(start, out var startTs) || !TimeSpan.TryParse(end, out var endTs))
            return false;

        if (startTs <= endTs)
            return now >= startTs && now <= endTs;

        return now >= startTs || now <= endTs;
    }
}
