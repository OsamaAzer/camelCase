namespace DefaultHRManagementSystem.Services.Interfaces
{
    public interface IDeepSeekService
    {
        Task<string> GetResponseAsync(string prompt);
    }
}
