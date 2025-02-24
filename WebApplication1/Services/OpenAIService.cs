using OpenAI.Chat;

public class OpenAIService: IOpenAIService
{
    private readonly ChatClient _chatClient;

    public OpenAIService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"];
        _chatClient = new ChatClient(model: "gpt-4o-mini", apiKey);
    }

    public async Task<string> GetResponseAsync(string prompt)
    {
        var chatCompletion = await _chatClient.CompleteChatAsync(
            [
                new UserChatMessage(prompt),
            ]);

        return chatCompletion.Value.Content.ToString();
    }

}