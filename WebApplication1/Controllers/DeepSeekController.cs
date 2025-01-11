
namespace DefaultHRManagementSystem.Controllers
{
    //[Authorize] // Authorization Attripute
    [ApiController]
    [Route("api/[controller]")]
    public class DeepSeekController(DeepSeekService _deepSeekService) : ControllerBase
    {
        [HttpPost("ask")]
        public async Task<IActionResult> AskQuestion(string prompt)
        {
            try
            {
                var response = await _deepSeekService.GetResponseAsync(prompt);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}