using System.ComponentModel;

namespace DefaultHRManagementSystem.Services
{
    public class ImportExcelService(AppDbContext _context, ILogger<ImportExcelService> _logger) :IImportExcelService
    {
        public async Task<(bool Success, List<string> Errors, int ImportedCount)>
    ImportExcelFile<TDto, TEntity>(IFormFile file, Dictionary<string, string> columnMappings = null, string sheetName = null)
    where TDto : class, new()
    where TEntity : class, new()
        {
            var errorDetails = new List<string>();
            var importedEntities = new List<TEntity>();

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                using var package = new ExcelPackage(stream);
                var worksheet = string.IsNullOrEmpty(sheetName)
                    ? package.Workbook.Worksheets[0] // Default to first sheet
                    : package.Workbook.Worksheets[sheetName];

                if (worksheet == null)
                    return (false, new List<string> { $"Worksheet '{sheetName ?? "first"}' not found." }, 0);

                var rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount <= 1)
                    return (false, new List<string> { "No data found in the Excel file." }, 0);

                var properties = typeof(TDto).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

                // Get the header row to map columns to properties
                var headerRow = worksheet.Cells[1, 1, 1, worksheet.Dimension.Columns]
                    .Select(c => c.Text.Trim())
                    .ToList();

                for (int row = 2; row <= rowCount; row++)
                {
                    var dto = new TDto();
                    var validationContext = new ValidationContext(dto);
                    var validationResults = new List<ValidationResult>();

                    try
                    {
                        for (int col = 1; col <= headerRow.Count; col++)
                        {
                            var header = headerRow[col - 1];
                            var cellValue = worksheet.Cells[row, col].Text;

                            // Use columnMappings to map Excel columns to DTO properties
                            var propertyName = columnMappings?.ContainsKey(header) == true
                                ? columnMappings[header]
                                : header;

                            var property = properties.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
                            if (property == null)
                            {
                                errorDetails.Add($"Row {row}: Column '{header}' does not match any property in the DTO.");
                                continue;
                            }

                            if (!string.IsNullOrEmpty(cellValue))
                            {
                                if (property.PropertyType == typeof(DateOnly))
                                {
                                    if (DateTime.TryParse(cellValue, out var dateValue))
                                    {
                                        property.SetValue(dto, DateOnly.FromDateTime(dateValue));
                                    }
                                    else
                                    {
                                        errorDetails.Add($"Row {row}: Invalid date format for '{property.Name}'.");
                                        continue;
                                    }
                                }
                                else
                                {
                                    var converter = TypeDescriptor.GetConverter(property.PropertyType);
                                    var convertedValue = converter.ConvertFrom(cellValue);
                                    property.SetValue(dto, convertedValue);
                                }
                            }
                            else if (property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) == null)
                            {
                                errorDetails.Add($"Row {row}: '{property.Name}' cannot be null.");
                                continue;
                            }
                        }

                        if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
                        {
                            var errors = string.Join(", ", validationResults.Select(vr => vr.ErrorMessage));
                            errorDetails.Add($"Row {row}: Validation failed - {errors}");
                            continue;
                        }

                        var entity = dto.Adapt<TEntity>();
                        importedEntities.Add(entity);
                    }
                    catch (Exception ex)
                    {
                        errorDetails.Add($"Row {row}: Error processing data - {ex.Message}");
                    }
                }

                if (errorDetails.Any())
                {
                    return (false, errorDetails, 0);
                }

                // Save in batches to improve performance
                var batchSize = 100;
                for (int i = 0; i < importedEntities.Count; i += batchSize)
                {
                    var batch = importedEntities.Skip(i).Take(batchSize).ToList();
                    await _context.Set<TEntity>().AddRangeAsync(batch);
                    await _context.SaveChangesAsync();
                }

                return (true, null, importedEntities.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing Excel file.");
                errorDetails.Add($"Unexpected error: {ex.Message}");
                return (false, errorDetails, 0);
            }
        }
    }
       
}
