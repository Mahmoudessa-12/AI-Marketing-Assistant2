using CsvHelper;
using System.Globalization;

namespace WebApplication1.Services;

public class CsvCampaignService
{
    public async Task<List<Dictionary<string, string>>> ReadCsvAsync(
        IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new Exception("CSV file is empty.");

        if (file.Length > 5 * 1024 * 1024)
            throw new Exception("CSV file must be 5 MB or smaller.");

        if (!Path.GetExtension(file.FileName)
            .Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("Only CSV files are allowed.");
        }

        using var stream = file.OpenReadStream();

        using var reader = new StreamReader(stream);

        using var csv = new CsvReader(
            reader,
            CultureInfo.InvariantCulture);

        var records = new List<Dictionary<string, string>>();

        await csv.ReadAsync();
        csv.ReadHeader();

        var headers = csv.HeaderRecord;

        if (headers == null || headers.Length == 0)
            throw new Exception("CSV file must contain headers.");

        while (await csv.ReadAsync())
        {
            var row = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var header in headers)
            {
                row[header] = csv.GetField(header) ?? "";
            }

            records.Add(row);
        }

        if (records.Count == 0)
            throw new Exception("CSV file contains no products.");

        return records;
    }
}