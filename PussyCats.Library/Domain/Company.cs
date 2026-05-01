namespace PussyCats.Library.Domain;

public class Company
{
    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = string.Empty;
    public string LogoText { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public List<Job> Jobs { get; set; } = new();
}
