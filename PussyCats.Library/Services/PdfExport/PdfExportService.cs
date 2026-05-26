using Microsoft.Playwright;
using PussyCats.Library.Domain;

namespace PussyCats.Library.Services.PdfExport;

using System.Text;
using Microsoft.Playwright;

public class PdfExportService : IPdfExportService
{
    private readonly string templateHtml;

    public PdfExportService(string templateHtml)
    {
        this.templateHtml = templateHtml;
    }

    public Task<string> RenderHtmlAsync(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        string html = templateHtml;

        html = ReplaceBasicFields(html, user);
        html = ReplaceSkills(html, user);
        html = ReplaceWorkExperience(html, user);
        html = ReplaceProjects(html, user);
        html = ReplaceActivities(html, user);

        return Task.FromResult(html);
    }

    public async Task<byte[]> GeneratePdfAsync(User user)
    {
        string html = await RenderHtmlAsync(user);

        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                Headless = true
            });

        var page = await browser.NewPageAsync();

        await page.SetContentAsync(html);

        return await page.PdfAsync(new PagePdfOptions
        {
            Format = "A4",
            PrintBackground = true
        });
    }

    private static string ReplaceBasicFields(string html, User user)
    {
        return html
            .Replace("{{FIRST_NAME}}", user.FirstName ?? string.Empty)
            .Replace("{{LAST_NAME}}", user.LastName ?? string.Empty)
            .Replace("{{EMAIL}}", user.Email ?? string.Empty)
            .Replace("{{PHONE}}", user.Phone ?? string.Empty)
            .Replace("{{COUNTRY}}", user.Country ?? string.Empty)
            .Replace("{{CITY}}", user.City ?? string.Empty)
            .Replace("{{UNIVERSITY}}", user.University ?? string.Empty)
            .Replace("{{MOTIVATION}}", user.Motivation ?? string.Empty)
            .Replace("{{LINKEDIN}}", user.LinkedIn ?? string.Empty)
            .Replace("{{GITHUB}}", user.GitHub ?? string.Empty);
    }

    private static string ReplaceSkills(string html, User user)
    {
        var builder = new StringBuilder();

        foreach (var skill in user.Skills)
        {
            string skillName = skill.Skill?.Name ?? string.Empty;

            builder.Append($"""
                <span class="skill-badge">
                    {skillName}
                </span>
            """);
        }

        return html.Replace("{{SKILLS}}", builder.ToString());
    }

    private static string ReplaceWorkExperience(string html, User user)
    {
        var builder = new StringBuilder();

        foreach (var work in user.WorkExperiences)
        {
            builder.Append($"""
                <div class="cv-section-item">
                    <h3>{work.JobTitle}</h3>
                    <h4>{work.Company}</h4>

                    <p>
                        {work.StartDate:yyyy-MM-dd} -
                        {(work.CurrentlyWorking ? "Present" : work.EndDate?.ToString("yyyy-MM-dd"))}
                    </p>

                    <p>{work.Description}</p>
                </div>
            """);
        }

        return html.Replace("{{WORK_EXPERIENCE}}", builder.ToString());
    }

    private static string ReplaceProjects(string html, User user)
    {
        var builder = new StringBuilder();

        foreach (var project in user.Projects)
        {
            builder.Append($"""
                <div class="cv-section-item">
                    <h3>{project.Name}</h3>
                    <p>{project.Description}</p>
                </div>
            """);
        }

        return html.Replace("{{PROJECTS}}", builder.ToString());
    }

    private static string ReplaceActivities(string html, User user)
    {
        var builder = new StringBuilder();

        foreach (var activity in user.ExtraCurricularActivities)
        {
            builder.Append($"""
                <div class="cv-section-item">
                    <h3>{activity.ActivityName}</h3>

                    <h4>
                        {activity.Organization}
                        ({activity.Role})
                    </h4>

                    <p>{activity.Period}</p>

                    <p>{activity.Description}</p>
                </div>
            """);
        }

        return html.Replace("{{ACTIVITIES}}", builder.ToString());
    }
}
