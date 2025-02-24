namespace DefaultHRManagementSystem.Services.Interfaces
{
    public interface ISalaryService
    {
        Task<(double TotalSalary, int TotalOvertimeHours, int TotalLateHours, List<Attendance> attendences)> CalculateSalaryWithDetailsAsync
            (int employeeId, DateOnly? startDate, DateOnly? endDate, int? month, int? year, string? employeeName);

        Task<byte[]> GenerateSalaryReportAsync
            (int employeeId, DateOnly? startDate, DateOnly? endDate, int? month, int? year, string? employeeName, bool? printAllAttendances = false);
    }
}
