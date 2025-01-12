
namespace DefaultHRManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class AttendencesController(AppDbContext context) : ControllerBase
    {
        [PermissionAuthorize(Permissions.View)]
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id == 0)
                return BadRequest("Please enter a valid Id");

            var attendance = await context.Attendances
                .Include(a => a.Employee) // Include related data if needed
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
                return NotFound($"The Attendance with Id:({id}) doesn't exist!");

            return Ok(attendance);
        }

        [PermissionAuthorize(Permissions.View)]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(DateOnly? date, string? employeeName)
        {
            var query = context.Attendances.Include(a => a.Employee).AsQueryable();

            if(!string.IsNullOrEmpty(employeeName))
                query = query.Where(a => a.Employee!.FullName.ToLower() == employeeName.ToLower());

            if (date != null)
                query = query.Where(a => a.Date == date);

            query = query.OrderBy(s => s.Date);

            var attendances = await query.ToListAsync();

            if (attendances.Count == 0)
                return NotFound("No Attendances Found");

            return Ok(attendances);

        }

        [PermissionAuthorize(Permissions.Create)]
        [HttpPost("CreateAttendance")]
        public async Task<IActionResult> Create([FromBody] AttendanceDto dto)
        {
            if (dto == null)
                return BadRequest("Please fill required data");

            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            if (dto.Date < currentDate)
                return BadRequest("Attendance can't be added in the past!");

            if (/*dto.Date > currentDate||*/ dto.Date.Year != currentDate.Year || dto.Date.Month != currentDate.Month)
                return BadRequest("Attendance can't be added in the future!");

            if (currentDate.DayOfWeek == DayOfWeek.Friday || currentDate.DayOfWeek == DayOfWeek.Saturday)
                return BadRequest("Unable to add an Attendance in a weekend!");
            
            var employee = await context.Employees.SingleOrDefaultAsync(e => e.FullName.ToLower() == dto.EmployeeName.ToLower());

            if (employee == null)
                return BadRequest("The employee does not exist.");

            var attendance = dto.Adapt<Attendance>();
            attendance.EmployeeId = employee.Id;

            await context.Attendances.AddAsync(attendance);

            await context.SaveChangesAsync();

            return Ok(attendance);
        }

        [PermissionAuthorize(Permissions.Update)]
        [HttpPut("UpdateAttendance/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AttendanceDto dto)
        {
            if (id == 0)
                return BadRequest("Please enter a valid id");

            if (dto == null)
                return BadRequest("Please fill Attendance data");

            var attendance = await context.Attendances.FindAsync(id);

            if (attendance == null)
                return NotFound($"The Attendance with Id:({id} doesn't exist)");

            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            if (dto.Date < currentDate)
                return BadRequest("Attendance can't be added in the past!");

            if (/*dto.Date > currentDate||*/ dto.Date.Year != currentDate.Year || dto.Date.Month != currentDate.Month)
                return BadRequest("Attendance can't be added in the future!");

            if (currentDate.DayOfWeek == DayOfWeek.Friday || currentDate.DayOfWeek == DayOfWeek.Saturday)
                return BadRequest("Unable to add an Attendance in a weekend!");

            var employee = await context.Employees.SingleOrDefaultAsync(e => e.FullName.ToLower() == dto.EmployeeName.ToLower());

            if (employee == null)
                return BadRequest("The employee does not exist.");

            attendance = dto.Adapt(attendance);
            attendance.EmployeeId = employee.Id;

            context.Update(attendance);

            await context.SaveChangesAsync();

            return Ok(attendance);
        }

        [PermissionAuthorize(Permissions.Delete)]
        [HttpDelete("DeleteAttendance/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0)
                return BadRequest("Please enter a valid id");

            var attendance = await context.Attendances.FindAsync(id);

            if (attendance == null)
                return NotFound($"The Attendance with Id:({id} doesn't exist)");

            context.Attendances.Remove(attendance);

            await context.SaveChangesAsync();

            return Ok(attendance);
        }
    }
}
