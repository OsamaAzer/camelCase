﻿
using DefaultHRManagementSystem.Data.Models;

namespace DefaultHRManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class SpecialLeavesController(AppDbContext context, ImportService importService) : ControllerBase
    {
        [PermissionAuthorize(Permissions.View)]
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id == 0)
                return BadRequest("Please enter a valid Id");

            var specialLeave = await context.SpecialLeaves.FindAsync(id);

            if (specialLeave == null)
                return NotFound($"The Special Leave with Id:({id}) dosen't exist!");

            return Ok(specialLeave);
        }

        [PermissionAuthorize(Permissions.View)]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(DateOnly? fromDate, DateOnly? toDate, string? employeeName)
        {
            var specialLeaves = new List<SpecialLeave>();

            var specialLeavesDto = new List<SpecialLeaveDto>();

            if (employeeName is not null && !await context.Employees.AnyAsync(e => e.FullName.ToLower() == employeeName.ToLower()))
                return BadRequest("There's no employee with this name has a SpecialLeave!!");

            if (fromDate > toDate)
                return BadRequest("FromDate can't be greater than ToDate!");

            if (employeeName is not null && fromDate is null && toDate is null)
            {
                specialLeaves = await context.SpecialLeaves.Include(s => s.Employee)
                        .Where(e => e.Employee!.FullName!.ToLower() == employeeName.ToLower())
                        .OrderBy(s => s.Date).ToListAsync();
            }

            if (employeeName is null && fromDate is not null && toDate is not null && fromDate > toDate )
            {
                specialLeaves = await context.SpecialLeaves.Include(s => s.Employee)
                        .Where(s => (s.Date <= toDate && s.Date >= fromDate))
                        .OrderBy(s => s.Date).ToListAsync();
            }

            if (fromDate is not null && toDate is not null && fromDate > toDate && employeeName is not null)
            {
                specialLeaves = await context.SpecialLeaves.Include(s => s.Employee)
                        .Where(s => (s.Date >= fromDate && s.Date <= toDate))
                        .Where(s => s.Employee!.FullName!.ToLower() == employeeName.ToLower())
                        .OrderBy(s => s.Date).ToListAsync();
            }

            if (fromDate is null && toDate is null && employeeName is null)
                specialLeaves = await context.SpecialLeaves.Include(s => s.Employee)
                    .OrderBy(s => s.Date).ToListAsync();

            if (specialLeaves.Count == 0)
                return NotFound("No Special Leaves found!");

            //specialLeavesDto = specialLeaves.Adapt(specialLeavesDto);

            return Ok(specialLeaves);

        }

        [PermissionAuthorize(Permissions.Create)]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] SpecialLeaveDto dto)
        {
            if (dto == null)
                return BadRequest("Please fill required data");

            var employee = await context.Employees.SingleOrDefaultAsync(e => e.FullName.ToLower() == dto.EmployeeName.ToLower());
            if (employee is null)
                return BadRequest("The Employee you want to assign a Special Leave is not found!!");

            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            if (dto.Date < currentDate)
                return BadRequest("Please enter a valid date!");

            if (dto.Date.Year != currentDate.Year)
                return BadRequest("Special Leave year should be current year!");

            //if (dto.Date.Month != currentDate.Month)
            //    return BadRequest("Special Leave month should be current month!");

            if (currentDate.DayOfWeek == DayOfWeek.Friday || currentDate.DayOfWeek == DayOfWeek.Saturday)
                return BadRequest("Unable to add a Special Leave in a weekend!");

            var specialLeave = dto.Adapt<SpecialLeave>();
            specialLeave.EmployeeId = employee.Id;

            await context.SpecialLeaves.AddAsync(specialLeave);

            await context.SaveChangesAsync();

            return Ok(specialLeave);
        }

        [PermissionAuthorize(Permissions.Update)]
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SpecialLeaveDto dto)
        {
            if (id == 0)
                return BadRequest("Please enter a valid id");

            if (dto == null)
                return BadRequest("Please fill object data");

            var specialLeave = await context.SpecialLeaves.FindAsync(id);

            if (specialLeave == null)
                return NotFound($"The Special Leave with Id:({id} doesn't exist)");

            var employee = await context.Employees.SingleOrDefaultAsync(e => e.FullName.ToLower() == dto.EmployeeName.ToLower());

            if (employee is null)
                return BadRequest("There's no employee with this name!");

            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            if (dto.Date < currentDate)
                return BadRequest("Please enter a valid date!");

            if (dto.Date.Year != currentDate.Year)
                return BadRequest("Special Leave year should be current year!");

            //if (dto.Date.Month != currentDate.Month)
            //    return BadRequest("Special Leave month should be current month!");

            if (currentDate.DayOfWeek == DayOfWeek.Friday || currentDate.DayOfWeek == DayOfWeek.Saturday)
                return BadRequest("Unable to add a Special Leave in a weekend!");

            specialLeave = dto.Adapt(specialLeave);
            specialLeave.EmployeeId = employee.Id;

            context.SpecialLeaves.Update(specialLeave);

            await context.SaveChangesAsync();

            return Ok(specialLeave);
        }

        [PermissionAuthorize(Permissions.Delete)]
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0)
                return BadRequest("Please enter a valid id");

            var specialLeave = await context.SpecialLeaves.FindAsync(id);

            if (specialLeave is null)
                return NotFound($"The Special Leave with Id:({id} doesn't exist)");

            context.SpecialLeaves.Remove(specialLeave);

            await context.SaveChangesAsync();

            return Ok(specialLeave);
        }

        //[HttpPost("importData")]
        //public async Task<IActionResult> ImportData(IFormFile file, string sheetName = null)
        //{
        //    var (success, errors, importedCount) = await importService.ImportExcelFile<SpecialLeaveDto, SpecialLeave>(file,null,sheetName);

        //    if (!success)
        //    {
        //        return BadRequest(new
        //        {
        //            ImportResult = false,
        //            Errors = errors
        //        });
        //    }

        //    return Ok(new
        //    {
        //        ImportResult = true,
        //        ImportedCount = importedCount
        //    });
        //}

    }
}