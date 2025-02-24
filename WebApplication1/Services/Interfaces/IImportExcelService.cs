namespace DefaultHRManagementSystem.Services.Interfaces
{
    public interface IImportExcelService
    {
        Task<(bool Success, List<string> Errors, int ImportedCount)>
    ImportExcelFile<TDto, TEntity>(IFormFile file, Dictionary<string, string> columnMappings = null, string sheetName = null)
    where TDto : class, new()
    where TEntity : class, new();
    }
}
