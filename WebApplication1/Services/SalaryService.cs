using DefaultHRManagementSystem.Data.Models;
using DefaultHRManagementSystem.Services.Interfaces;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace DefaultHRManagementSystem.Services
{
    public class SalaryService(AppDbContext _context) : ISalaryService
    {
        public async Task<(double TotalSalary, int TotalOvertimeHours, int TotalLateHours, List<Attendance> attendences)> CalculateSalaryWithDetailsAsync(
    int employeeId, DateOnly? startDate, DateOnly? endDate, int? month, int? year, string? employeeName)
        {
            var employee = await _context.Employees
                .Include(e => e.Attendances)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
                throw new ArgumentException("Employee not found.");

            // Validate date range
            if (startDate > endDate)
                throw new ArgumentException("Start date cannot be greater than end date.");

            // Validate month and year
            if (month is < 1 or > 12)
                throw new ArgumentException("Month must be between 1 and 12.");

            if (year is < 2000 or > 2100)
                throw new ArgumentException("Year must be between 2000 and 2100.");

            double totalSalary = employee.BasicSalary;
            int totalOvertimeHours = 0;
            int totalLateHours = 0;

            var query = _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.EmployeeId == employeeId)
                .AsQueryable();

            // Apply date range filter
            if (startDate != null && endDate != null)
                query = query.Where(a => a.Date >= startDate && a.Date <= endDate);

            // Apply month filter
            if (month != null)
                query = query.Where(a => a.Date.Month == month);

            // Apply year filter
            if (year != null)
                query = query.Where(a => a.Date.Year == year);

            // Apply employee name filter
            if (!string.IsNullOrEmpty(employeeName))
                query = query.Where(a => a.Employee.FullName.Contains(employeeName));

            var attendances = query.ToList();

            if (attendances.Count == 0)
                throw new ArgumentException("Employee doesn't have any attendances in the specified period.");

            foreach (var attendance in attendances)
            {
                totalOvertimeHours += attendance.OverTimeHours;
                totalLateHours += attendance.LateTimeHours;

                var dailyAdjustment =
                    ((attendance.OverTimeHours * employee.BasicSalary / 160) * 1.5) -
                    ((attendance.LateTimeHours * employee.BasicSalary / 160) * 0.5);

                totalSalary += dailyAdjustment;
            }

            totalSalary += employee.Commission - employee.Deduction;

            return (totalSalary, totalOvertimeHours, totalLateHours, attendances);
        }

        public async Task<byte[]> GenerateSalaryReportAsync(
            int employeeId, DateOnly? startDate, DateOnly? endDate, int? month, int? year, string? employeeName, bool? printAllAttendances = false)
        {
            var employee = await _context.Employees
                .Include(e => e.Attendances)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
                throw new ArgumentException("Employee not found.");

            var (totalSalary, totalOvertimeHours, totalLateHours, attendences) = await CalculateSalaryWithDetailsAsync(
                employeeId, startDate, endDate, month, year, employeeName);

            var list = attendences.ToList();

            using (var memoryStream = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 30, 30, 30, 30); // Set margins (left, right, top, bottom)
                PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                // Add a company logo (if available)
                var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.*");
                var logoFile = Directory.GetFiles(Path.GetDirectoryName(logoPath)!, Path.GetFileName(logoPath)).FirstOrDefault();

                if (logoFile != null)
                {
                    try
                    {
                        var logo = Image.GetInstance(logoFile);
                        logo.ScaleToFit(100, 100); // Resize the logo
                        logo.Alignment = Image.ALIGN_CENTER;
                        document.Add(logo);
                    }
                    catch (Exception ex)
                    {
                        // Log the error (optional)
                        Console.WriteLine($"Error loading logo: {ex.Message}");
                    }
                }

                // Add a title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, BaseColor.DARK_GRAY);
                var title = new Paragraph("Salary Report", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(title);

                // Add employee details
                var subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
                var employeeDetails = new Paragraph($"Employee: {employee.FullName}", subtitleFont)
                {
                    SpacingAfter = 10
                };
                document.Add(employeeDetails);

                var periodDetails = new Paragraph($"Period: {startDate} to {endDate}", subtitleFont)
                {
                    SpacingAfter = 10
                };
                document.Add(periodDetails);

                // Add salary details
                var salaryDetailsFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);
                document.Add(new Paragraph($"Basic Salary: {employee.BasicSalary:C}", salaryDetailsFont));
                document.Add(new Paragraph($"Commission: {employee.Commission:C}", salaryDetailsFont));
                document.Add(new Paragraph($"Deduction: {employee.Deduction:C}", salaryDetailsFont));
                document.Add(new Paragraph($"Total Overtime Hours: {totalOvertimeHours}", salaryDetailsFont));
                document.Add(new Paragraph($"Total LateTtime Hours: {totalLateHours}", salaryDetailsFont));
                document.Add(new Paragraph($"Final Calculated Salary: {totalSalary:C}", salaryDetailsFont));

                // Add a spacer
                document.Add(new Paragraph(" "));

                // Add a table for attendance details (if requested)
                if (printAllAttendances == true)
                {
                    var tableHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
                    var tableCellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

                    var table = new PdfPTable(5)
                    {
                        WidthPercentage = 100,
                        SpacingBefore = 10,
                        SpacingAfter = 10
                    };

                    // Set table headers
                    var headerBackground = new BaseColor(51, 102, 153); // Dark blue background
                    table.AddCell(new PdfPCell(new Phrase("Date", tableHeaderFont))
                    {
                        BackgroundColor = headerBackground,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    });
                    table.AddCell(new PdfPCell(new Phrase("Arrival Time", tableHeaderFont))
                    {
                        BackgroundColor = headerBackground,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    });
                    table.AddCell(new PdfPCell(new Phrase("Leaving Time", tableHeaderFont))
                    {
                        BackgroundColor = headerBackground,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    });
                    table.AddCell(new PdfPCell(new Phrase("Overtime Hours", tableHeaderFont))
                    {
                        BackgroundColor = headerBackground,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    });
                    table.AddCell(new PdfPCell(new Phrase("Late Hours", tableHeaderFont))
                    {
                        BackgroundColor = headerBackground,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    });

                    // Add table rows
                    //var attendances = employee.Attendances!
                    //    .Where(a => a.Date >= startDate && a.Date <= endDate)
                    //    .OrderBy(a => a.Date)
                    //    .ToList();

                    foreach (var attendance in list)
                    {
                        table.AddCell(new PdfPCell(new Phrase(attendance.Date.ToString("yyyy-MM-dd"), tableCellFont))
                        {
                            Padding = 5
                        });
                        table.AddCell(new PdfPCell(new Phrase(attendance.ArrivingTime.ToString(), tableCellFont))
                        {
                            Padding = 5
                        });
                        table.AddCell(new PdfPCell(new Phrase(attendance.LeavingTime.ToString(), tableCellFont))
                        {
                            Padding = 5
                        });
                        table.AddCell(new PdfPCell(new Phrase(attendance.OverTimeHours.ToString(), tableCellFont))
                        {
                            Padding = 5
                        });
                        table.AddCell(new PdfPCell(new Phrase(attendance.LateTimeHours.ToString(), tableCellFont))
                        {
                            Padding = 5
                        });
                    }

                    document.Add(table);
                }

                // Add a footer with page numbers
                var footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.GRAY);
                var footer = new Paragraph($"Page {document.PageNumber + 1}", footerFont)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingBefore = 20
                };
                document.Add(footer);

                document.Close();

                return memoryStream.ToArray();
            }
        }

    }
}
