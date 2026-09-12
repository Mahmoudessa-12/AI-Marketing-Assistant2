namespace WebApplication1.Models;

public class CampaignRequest
{
    public string CampaignGoal { get; set; } = "";
    public decimal TargetBudget { get; set; }
    public int DurationWeeks { get; set; }
}

public class CampaignResponse
{
    public CampaignData Campaign { get; set; } = new();
    public object DeveloperPayload { get; set; } = new();
}

public class CampaignData
{
    public string Summary { get; set; } = "";
    public List<CampaignDay> Calendar { get; set; } = new();
    public List<Persona> Personas { get; set; } = new();
}

public class CampaignDay
{
    public int Week { get; set; }
    public string Channel { get; set; } = "";
    public string Objective { get; set; } = "";
    public string Audience { get; set; } = "";
    public string ContentType { get; set; } = "";
    public string Copy { get; set; } = "";
    public decimal Budget { get; set; }
    public string Kpi { get; set; } = "";
}

public class Persona
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> PainPoints { get; set; } = new();
    public List<string> CopyVariations { get; set; } = new();
}