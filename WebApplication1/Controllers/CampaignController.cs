using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CampaignController : ControllerBase
{
    private readonly CsvCampaignService _csvService;
    private readonly OpenAiMarketingService _aiService;

    public CampaignController(
        CsvCampaignService csvService,
        OpenAiMarketingService aiService)
    {
        _csvService = csvService;
        _aiService = aiService;
    }

    // Test endpoint
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            success = true,
            message = "AI Marketing Assistant Backend is running"
        });
    }

    // Upload CSV
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadCsv(IFormFile file)
    {
        try
        {
            var products = await _csvService.ReadCsvAsync(file);

            return Ok(new
            {
                success = true,
                productCount = products.Count,
                products = products
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    // Generate AI Campaign
    [HttpPost("generate")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> GenerateCampaign(
        [FromForm] GenerateCampaignRequest request)
    {
        try
        {
            // Validate file
            if (request.File == null ||
                request.File.Length == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please upload a CSV file."
                });
            }

            // Validate campaign goal
            if (string.IsNullOrWhiteSpace(
                    request.CampaignGoal))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Campaign goal is required."
                });
            }

            // Validate budget
            if (request.TargetBudget <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Target budget must be greater than 0."
                });
            }

            // Validate duration
            if (request.DurationWeeks <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Duration must be greater than 0."
                });
            }

            // Read CSV
            var products =
                await _csvService.ReadCsvAsync(
                    request.File);

            // Generate campaign using AI
            var aiResult =
    await _aiService.GenerateCampaignAsync(
        request.CampaignGoal,
        request.TargetBudget,
        request.DurationWeeks,
        products,
        request.OutputLanguage);

            return Ok(new
            {
                success = true,
                result = aiResult
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }
}