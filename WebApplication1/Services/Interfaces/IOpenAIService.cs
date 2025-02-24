namespace DefaultHRManagementSystem.Services.Interfaces
{
    public interface IOpenAIService
    {
        Task<string> GetResponseAsync(string prompt);
    }
}
