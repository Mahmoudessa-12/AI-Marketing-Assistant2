using Microsoft.AspNetCore.Http;

namespace WebApplication1.Models;

public class GenerateCampaignRequest
{
    public IFormFile File { get; set; } = default!;

    public string CampaignGoal { get; set; } = string.Empty;

    public string OutputLanguage { get; set; } = "English";

    public decimal TargetBudget { get; set; }

    public int DurationWeeks { get; set; }
}