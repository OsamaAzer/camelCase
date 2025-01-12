
namespace DefaultHRManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class EmployeesController(AppDbContext context) : ControllerBase
    {
        //[PermissionAuthorize(Permissions.View)]
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id == 0)
                return BadRequest("Please enter a valid Id");

            var employee = await context.Employees.FindAsync(id);

            if (employee == null)
                return NotFound($"The Employee with Id:({id}) dosen't exist!");

            return Ok(employee);
        }

        //[PermissionAuthorize(Permissions.View)]
        [HttpGet("GetAllEmployees")]
        public async Task<ActionResult<PagedResponse<EmployeeDto>>> 
                                    GetAll([FromQuery] string? name,[FromQuery] int page = 1,[FromQuery] int pageSize = 10)
        {
            var query = context.Employees.Include(e => e.Department).AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(e => e.FullName.ToLower() == name.ToLower());

            var totalCount = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var employees = await query
                .OrderBy(e => e.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (employees.Count == 0)
                return NotFound("No Employees Found");

            //var employeesDto = employees.Adapt<List<EmployeeDto>>();

            var response = new PagedResponse<Employee>
            {
                Data = employees,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            return Ok(response);
        }

        //[PermissionAuthorize(Permissions.Create)]
        [HttpPost("CreateEmployee")]
        public async Task<IActionResult> Create([FromBody] EmployeeDto dto)
        {
            if (dto == null)
                return BadRequest("Please fill required data");

            var existingEmployee = await context.Employees
                .SingleOrDefaultAsync(e => e.FullName.ToLower() == dto.FullName.ToLower());

            if (existingEmployee != null)
                return BadRequest("There's an employee with the same name!");

            var department = await context.Departments.FindAsync(dto.DepartmentId);
            if (department == null)
                return BadRequest("The Department you want to assign for Employee doesn't exist!");

            var employee = dto.Adapt<Employee>();
            await context.Employees.AddAsync(employee);
            await context.SaveChangesAsync();

            return Ok(employee);
        }

        //[PermissionAuthorize(Permissions.Update)]
        [HttpPut("UpdateEmployee/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeDto dto)
        {
            if (id == 0)
                return BadRequest("Please enter a valid id");

            if (dto == null)
                return BadRequest("Please fill Employee data");

            var employee = await context.Employees.FindAsync(id);

            if (employee == null)
                return NotFound($"The Employee with Id:({id} doesn't exist)");

            //var employeeWithName = await context.Employees.SingleOrDefaultAsync(e => e.FullName.ToLower() == dto.FullName.ToLower());

            var department = await context.Departments.FindAsync(dto.DepartmentId);
            
            if (department is null)
                return BadRequest("The Department you want to assign for Employee doesn't exist!");
            
            employee = dto.Adapt(employee);
            await context.SaveChangesAsync();
            return Ok(employee);
        }

        //[PermissionAuthorize(Permissions.Delete)]
        [HttpDelete("DeleteEmployee/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0)
                return BadRequest("Please enter a valid id");

            var employee = await context.Employees.FindAsync(id);

            if (employee == null)
                return NotFound($"The Employee with Id:({id} doesn't exist)");

            context.Employees.Remove(employee);
            await context.SaveChangesAsync();
            return Ok(employee);
        }

        
    }
}
