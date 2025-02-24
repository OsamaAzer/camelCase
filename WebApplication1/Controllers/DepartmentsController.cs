
namespace DefaultHRManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class DepartmentsController(AppDbContext context) : ControllerBase
    {
        //[PermissionAuthorize(Permissions.View)]
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id == 0)
                return BadRequest("Please enter a valid Id");

            var department = await context.Departments.FindAsync(id);

            if (department == null)
                return NotFound($"The Department with Id:({id}) dosen't exist!");

            return Ok(department);
        }

        //[PermissionAuthorize(Permissions.View)]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var departments = await context.Departments.OrderBy(s => s.Name).ToListAsync();

            if (departments.Count() <= 0)
                return NotFound("No Departments Found");

            return Ok(departments);

        }

        //[PermissionAuthorize(Permissions.Create)]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] DepartmentDto dto)
        {
            if (dto == null)
                return BadRequest("Please fill required data");

            var department = dto.Adapt<Department>();

            await context.Departments.AddAsync(department);

            await context.SaveChangesAsync();

            return Ok(department);
        }

        //[PermissionAuthorize(Permissions.Update)]
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DepartmentDto dto)
        {
            if (id == 0)
                return BadRequest("Please enter a valid id");

            if (dto == null)
                return BadRequest("Please fill object data");

            var department = await context.Departments.FindAsync(id);

            if (department == null)
                return NotFound($"The Department with Id:({id} doesn't exist)");

            department = dto.Adapt(department);

            context.Update(department);

            await context.SaveChangesAsync();

            return Ok(department);
        }

        //[PermissionAuthorize(Permissions.Delete)]
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0)
                return BadRequest("Please enter a valid id");

            var department = await context.Departments.FindAsync(id);

            if (department == null)
                return NotFound($"The Department with Id:({id} doesn't exist)");

            context.Departments.Remove(department);

            await context.SaveChangesAsync();

            return Ok(department);
        }
    }
}
