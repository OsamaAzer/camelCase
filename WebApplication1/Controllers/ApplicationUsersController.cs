
namespace DefaultHRManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUsersController(UserManager<ApplicationUser> _userManager) : ControllerBase
    {
        [Authorize(Roles = Role.Admin)]// Enables Users with (Admin) role only to access this action method.
        [PermissionAuthorize(Permissions.View)]// Enables Users with (Admin) role with (View) permission to access this action method.
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();
            return Ok(users);
        }

        [PermissionAuthorize(Permissions.View)]   
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetByID(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var userModel = user.Adapt<AddUserDto>();

            return Ok(userModel);
        }

        [PermissionAuthorize(Permissions.Create)]
        [HttpPost("CreateUser")]
        public async Task<IActionResult> Create([FromBody] AddUserDto user)
        {
            if (user == null)
            {
                return BadRequest("User data is required.");
            }
            var newUser = user.Adapt<ApplicationUser>();

            var result = await _userManager.CreateAsync(newUser, user.Password);

            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetByID), new { id = newUser.Id }, newUser);
            }

            return BadRequest(result.Errors);
        }

        [PermissionAuthorize(Permissions.Update)]
        [HttpPut("UpdateUserData/{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto request)
        {
            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Update basic user information
            existingUser.FullName = request.FullName;
            existingUser.Email = request.Email;
            existingUser.UserName = request.UserName;

            // Update the user's information
            var updateResult = await _userManager.UpdateAsync(existingUser);
            if (!updateResult.Succeeded)
            {
                return BadRequest(updateResult.Errors);
            }

            // If a new password is provided, validate the current password and update the password
            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                // Validate the current password
                var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(existingUser, request.CurrentPassword);
                if (!isCurrentPasswordValid)
                {
                    return BadRequest("Current password is incorrect.");
                }

                if (request.NewPassword == request.CurrentPassword)
                {
                    return BadRequest("New password cannot be the same as the current password.");
                }
                // Change the password
                var changePasswordResult = await _userManager.ChangePasswordAsync(existingUser, request.CurrentPassword, request.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    return BadRequest(changePasswordResult.Errors);
                }
            }

            return NoContent();
        }
        
        [PermissionAuthorize(Permissions.Delete)]
        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }
    }
}