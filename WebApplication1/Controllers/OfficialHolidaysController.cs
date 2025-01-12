
namespace DefaultHRManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class OfficialHolidaysController(AppDbContext context, ImportService importService) : ControllerBase
    {
        //[PermissionAuthorize(Permissions.View)]
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id == 0)
                return BadRequest("Please enter a valid Id");

            var officialHoliday = await context.OfficialHolidays.FindAsync(id);

            if (officialHoliday == null)
                return NotFound($"The Official Holiday with Id:({id}) dosen't exist!");

            return Ok(officialHoliday);
        }

        //[PermissionAuthorize(Permissions.View)]
        [HttpGet("GetAllHolidays")]
        public async Task<IActionResult> GetAll()
        {
            var officialHolidays = await context.OfficialHolidays.OrderBy(s => s.Date).ToListAsync();

            if (officialHolidays.Count == 0)
                return NotFound("No OfficialHolidays Found");

            return Ok(officialHolidays);

        }

        //[PermissionAuthorize(Permissions.Create)]
        [HttpPost("CreateHoliday")]
        public async Task<IActionResult> Create([FromBody] OfficialHolidayDto dto)
        {
            if (dto == null)
                return BadRequest("Please fill required data");

            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            if (dto.Date > currentDate || dto.Date.Year != currentDate.Year || dto.Date.Month != currentDate.Month)
                return BadRequest("Please enter a valid date!");

            if (currentDate.DayOfWeek == DayOfWeek.Friday || currentDate.DayOfWeek == DayOfWeek.Saturday)
                return BadRequest("Unable to add an Official Holiday in a weekend!");

            var officialHoliday = dto.Adapt<OfficialHoliday>();

            await context.OfficialHolidays.AddAsync(officialHoliday);

            await context.SaveChangesAsync();

            return Ok(officialHoliday);
        }

        //[PermissionAuthorize(Permissions.Create)]
        [HttpPost("importData")]
        public async Task<IActionResult> ImportData(IFormFile file, string sheetName = null)
        {
            var (success, errors, importedCount) = await importService.ImportExcelFile<OfficialHolidayDto, OfficialHoliday>(file, null, sheetName);

            if (!success)
            {
                return BadRequest(new
                {
                    ImportResult = false,
                    Errors = errors
                });
            }

            return Ok(new
            {
                ImportResult = true,
                ImportedCount = importedCount
            });
        }
        
        //[PermissionAuthorize(Permissions.Update)]
        [HttpPut("UpdateHoliday/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] OfficialHolidayDto dto)
        {
            if (id == 0)
                return BadRequest("Please enter a valid id");

            if (dto == null)
                return BadRequest("Please fill OfficialHoliday data");

            var officialHoliday = await context.OfficialHolidays.FindAsync(id);

            if (officialHoliday == null)
                return NotFound($"The OfficialHoliday with Id:({id} doesn't exist)");

            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            if (dto.Date > currentDate || dto.Date.Year != currentDate.Year || dto.Date.Month != currentDate.Month)
                return BadRequest("Please enter a valid date!");

            if (currentDate.DayOfWeek == DayOfWeek.Friday || currentDate.DayOfWeek == DayOfWeek.Saturday)
                return BadRequest("Unable to add an Official Holiday in a weekend!");

            officialHoliday = dto.Adapt(officialHoliday);

            context.Update(officialHoliday);

            await context.SaveChangesAsync();

            return Ok(officialHoliday);
        }

        //[PermissionAuthorize(Permissions.Delete)]
        [HttpDelete("DeleteHoliday/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0)
                return BadRequest("Please enter a valid id");

            var officialHoliday = await context.OfficialHolidays.FindAsync(id);

            if (officialHoliday == null)
                return NotFound($"The OfficialHoliday with Id:({id} doesn't exist)");

            context.OfficialHolidays.Remove(officialHoliday);

            await context.SaveChangesAsync();

            return Ok(officialHoliday);
        }

        
    }
}
