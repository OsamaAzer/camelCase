
namespace DefaultHRManagementSystem.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[Controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if(!ModelState.IsValid) 
                return BadRequest(ModelState);

            var result = await authService.RegisterAsync(model);

            if(!result.IsAuthenticated)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPost("LogIn")]
        public async Task<IActionResult> LogIn([FromBody] TokenRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await authService.GetTokenAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            return Ok(result);
        }
    }
}
