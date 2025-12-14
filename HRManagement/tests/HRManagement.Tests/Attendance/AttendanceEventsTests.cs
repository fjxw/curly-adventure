using HRManagement.Shared.Contracts.Events;

namespace HRManagement.Tests.Attendance;

public class AttendanceEventsTests
{
    [Fact]
    public void AttendanceMarkedEvent_ShouldHaveCorrectProperties()
    {
        var attendanceId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;
        var status = "Present";

        var attendanceEvent = new AttendanceMarkedEvent(attendanceId, employeeId, date, status);

        Assert.Equal(attendanceId, attendanceEvent.AttendanceId);
        Assert.Equal(employeeId, attendanceEvent.EmployeeId);
        Assert.Equal(date, attendanceEvent.Date);
        Assert.Equal(status, attendanceEvent.Status);
    }

    [Fact]
    public void TimeSheetApprovedEvent_ShouldHaveCorrectProperties()
    {
        var timesheetId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var year = 2024;
        var month = 12;
        var workedHours = 168m;
        var overtimeHours = 8m;

        var timesheetEvent = new TimeSheetApprovedEvent(timesheetId, employeeId, year, month, workedHours, overtimeHours);

        Assert.Equal(timesheetId, timesheetEvent.TimeSheetId);
        Assert.Equal(employeeId, timesheetEvent.EmployeeId);
        Assert.Equal(year, timesheetEvent.Year);
        Assert.Equal(month, timesheetEvent.Month);
        Assert.Equal(workedHours, timesheetEvent.TotalWorkedHours);
        Assert.Equal(overtimeHours, timesheetEvent.TotalOvertimeHours);
    }

    [Theory]
    [InlineData("Present")]
    [InlineData("Absent")]
    [InlineData("Late")]
    [InlineData("OnLeave")]
    [InlineData("Remote")]
    [InlineData("SickLeave")]
    public void AttendanceMarkedEvent_ShouldSupportVariousStatuses(string status)
    {
        var attendanceEvent = new AttendanceMarkedEvent(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, status);
        Assert.Equal(status, attendanceEvent.Status);
    }

    [Fact]
    public void TimeSheetApprovedEvent_ShouldHandleZeroOvertime()
    {
        var timesheetEvent = new TimeSheetApprovedEvent(Guid.NewGuid(), Guid.NewGuid(), 2024, 1, 160m, 0m);
        Assert.Equal(0m, timesheetEvent.TotalOvertimeHours);
    }
}
