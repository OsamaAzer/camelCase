using OpenAI.Chat;

//[Authorize] // Authorization Attripute
[ApiController]
[Route("api/[controller]")]
public class OpenAIController(OpenAIService _openAIService) : ControllerBase
{
    [HttpPost("complete")]
    public async Task<IActionResult> GetCompletion(string prompt)
    {
        try
        {
            var response = await _openAIService.GetResponseAsync(prompt);
            return Ok(new { response });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("ask")]
    public IActionResult Post(string prompt)
    {
        ChatClient client = new(model: "gpt-3.5-turbo", Environment.GetEnvironmentVariable("OpenAI__ApiKey"));
        try
        {
            var response = client.CompleteChat(messages: prompt);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    //[HttpPost("api")]
    //public async Task<IActionResult> Post([FromBody] string prompt)
    //{
    //    var apiKey = Environment.GetEnvironmentVariable("OpenAI__ApiKey");
    //    if (string.IsNullOrEmpty(apiKey))
    //    {
    //        return BadRequest("OpenAI API key is missing.");
    //    }

    //    var client = new ChatClient(model: "gpt-3.5-turbo", apiKey);

    //    try
    //    {
    //        // Create a list of chat messages
    //        var messages = new List<ChatMessage>
    //        {
    //            new ChatMessage { Role = "user", Content = prompt }
    //        };

    //        // Call the CompleteChat method with the messages
    //        var response = await client.CompleteChatAsync(messages);

    //        // Return the response
    //        return Ok(response);
    //    }
    //    catch (Exception ex)
    //    {
    //        // Return a user-friendly error message
    //        return BadRequest($"An error occurred: {ex.Message}");
    //    }
    //}
    public class ChatMessage
    {
        public string Role { get; set; } // e.g., "user", "system", "assistant"
        public string Content { get; set; } // the actual message content
    }
}