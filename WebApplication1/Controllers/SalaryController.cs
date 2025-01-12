namespace DefaultHRManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class SalaryController : ControllerBase
    {
        private readonly SalaryService _salaryService;

        public SalaryController(SalaryService salaryService)
        {
            _salaryService = salaryService;
        }

        //[PermissionAuthorize(Permissions.View)]
        //[HttpGet("CalculateSalary/{employeeId}")]
        //public async Task<IActionResult> CalculateSalary(int employeeId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        //{
        //    try
        //    {
        //        var salary = await _salaryService.CalculateSalaryAsync(employeeId, startDate, endDate);
        //        return Ok(new { EmployeeId = employeeId, Salary = salary });
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        [PermissionAuthorize(Permissions.View)]
        [HttpGet("GenerateSalaryReport/{employeeId}")]
        public async Task<IActionResult> GenerateSalaryReport(int employeeId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            try
            {
                var reportBytes = await _salaryService.GenerateSalaryReportAsync(employeeId, startDate, endDate);
                return File(reportBytes, "application/pdf", $"SalaryReport_{employeeId}_{startDate}_{endDate}.pdf");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
