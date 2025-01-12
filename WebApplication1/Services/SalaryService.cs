using iTextSharp.text;
using iTextSharp.text.pdf;

namespace DefaultHRManagementSystem.Services
{
    public class SalaryService(AppDbContext _context)
    {
        public async Task<(double TotalSalary, int TotalOvertimeHours, int TotalLateHours)> CalculateSalaryWithDetailsAsync(int employeeId, DateOnly startDate, DateOnly endDate)
        {
            var employee = await _context.Employees
                .Include(e => e.Attendances)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
                throw new ArgumentException("Employee not found.");

            double totalSalary = employee.BasicSalary; 
            int totalOvertimeHours = 0;
            int totalLateHours = 0;

            var attendances = employee.Attendances!.Where(a => a.Date >= startDate && a.Date <= endDate).ToList();

            if(attendances.Count == 0)
                throw new ArgumentException("Employee dosen't have any attendancesin the specific period!!");

            foreach (var attendance in attendances)
            {
                totalOvertimeHours += attendance.OverTimeHours;
                totalLateHours += attendance.LateTimeHours;

                var dailyAdjustment = 
                    ((attendance.OverTimeHours * employee.BasicSalary / 160) * 1.5) - ((attendance.LateTimeHours * employee.BasicSalary / 160) * 0.5); 

                totalSalary += dailyAdjustment;
            }

            totalSalary += employee.Commission - employee.Deduction;

            return (totalSalary, totalOvertimeHours, totalLateHours);
        }

        public async Task<byte[]> GenerateSalaryReportAsync(int employeeId, DateOnly startDate, DateOnly endDate, bool? printAllAttendances = false)
        {
            var employee = await _context.Employees
                .Include(e => e.Attendances)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
                throw new ArgumentException("Employee not found.");

            var (totalSalary, totalOvertimeHours, totalLateHours) = await CalculateSalaryWithDetailsAsync(employeeId, startDate, endDate);

            using (var memoryStream = new MemoryStream())
            {
                var document = new Document();
                PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                document.Add(new Paragraph($"Salary Report for {employee.FullName}"));
                document.Add(new Paragraph($"Period: {startDate} to {endDate}"));
                document.Add(new Paragraph($"Basic Salary: {employee.BasicSalary}"));
                document.Add(new Paragraph($"Commission: {employee.Commission}"));
                document.Add(new Paragraph($"Deduction: {employee.Deduction}"));
                document.Add(new Paragraph($"Total Overtime Hours: {totalOvertimeHours}"));
                document.Add(new Paragraph($"Total Late Hours: {totalLateHours}"));
                document.Add(new Paragraph($"Final Calculated Salary: {totalSalary}"));

                var table = new PdfPTable(5);
                if (printAllAttendances == true)
                {
                    //var table = new PdfPTable(5);
                    table.AddCell("Date");
                    table.AddCell("Arrival Time");
                    table.AddCell("Leaving Time");
                    table.AddCell("Overtime Hours");
                    table.AddCell("Late Hours");

                    var attendances = employee.Attendances!
                        .Where(a => a.Date >= startDate && a.Date <= endDate)
                        .OrderBy(a => a.Date)
                        .ToList();

                    foreach (var attendance in attendances)
                    {
                        table.AddCell(attendance.Date.ToString("yyyy-MM-dd"));
                        table.AddCell(attendance.ArrivingTime.ToString());
                        table.AddCell(attendance.LeavingTime.ToString());
                        table.AddCell(attendance.OverTimeHours.ToString());
                        table.AddCell(attendance.LateTimeHours.ToString());
                    }

                    document.Add(table);
                }

                document.Close();

                return memoryStream.ToArray();
            }
        }
    }
}
