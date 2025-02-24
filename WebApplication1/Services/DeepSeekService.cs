
namespace DefaultHRManagementSystem.Services
{
    public class DeepSeekService : IDeepSeekService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = Environment.GetEnvironmentVariable("OpenAI__ApiKey")!;

        public DeepSeekService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            //_apiKey = Environment.GetEnvironmentVariable("OpenAI__ApiKey");

            // Set the base URL for the DeepSeek API
            _httpClient.BaseAddress = new Uri("https://api.deepseek.com/v1/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<string> GetResponseAsync(string prompt)
        {
            var requestBody = new
            {
                model = "deepseek-chat", // Replace with the correct model name
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                max_tokens = 150 // Adjust as needed
            };

            var response = await _httpClient.PostAsJsonAsync("chat/completions", requestBody);

            // Log the raw response
            var rawResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"API Response: {rawResponse}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DeepSeekResponse>();
                if (result?.Messages?.FirstOrDefault()?.Content != null)
                {
                    return result.Messages.First().Content;
                }
                else if (result?.Error != null)
                {
                    throw new Exception($"API error: {result.Error.Message}");
                }
                else
                {
                    return "No response from the model.";
                }
            }
            else
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<DeepSeekErrorResponse>();
                if (errorResponse?.Error?.Message == "Insufficient Balance")
                {
                    throw new Exception("API request failed: Insufficient balance. Please add funds to your account.");
                }
                else
                {
                    throw new Exception($"API request failed: {rawResponse}");
                }
            }
        }

        private class DeepSeekResponse
        {
            public string Model { get; set; } // Add this field
            public List<Message> Messages { get; set; }
            public Error Error { get; set; }
        }

        private class DeepSeekErrorResponse
        {
            public Error Error { get; set; }
        }

        private class Message
        {
            public string Role { get; set; }
            public string Content { get; set; }
        }

        private class Error
        {
            public string Message { get; set; }
            public string Type { get; set; }
        }
    }
}