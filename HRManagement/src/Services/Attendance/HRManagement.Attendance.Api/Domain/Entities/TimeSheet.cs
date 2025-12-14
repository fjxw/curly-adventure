namespace HRManagement.Attendance.Api.Domain.Entities;

public class TimeSheet
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalWorkedHours { get; set; }
    public decimal TotalOvertimeHours { get; set; }
    public int WorkingDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }
    public int SickLeaveDays { get; set; }
    public int VacationDays { get; set; }
    public TimeSheetStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedById { get; set; }
}

public enum TimeSheetStatus
{
    Draft,
    Submitted,
    Approved,
    Rejected
}
