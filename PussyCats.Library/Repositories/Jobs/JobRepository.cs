using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Jobs;

public class JobRepository : IJobRepository
{
    private readonly PussyCatsDbContext db;

    public JobRepository(PussyCatsDbContext db)
    {
        this.db = db;
    }

    /// <summary>
    /// Includes Company and RequiredSkills.Skill so a job-detail screen has everything it needs
    /// to render. Tracked because the typical caller (recruiter editing a posting) mutates.
    /// </summary>
    public async Task<Job?> GetByIdAsync(int jobId, CancellationToken ct = default)
    {
        return await db.Jobs
            .Include(j => j.Company)
            .Include(j => j.RequiredSkills).ThenInclude(s => s.Skill)
            .FirstOrDefaultAsync(j => j.JobId == jobId, ct)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Browse-jobs listing — includes Company so the listing card can show the employer name
    /// without an N+1.
    /// </summary>
    public async Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Jobs
            .AsNoTracking()
            .Include(j => j.Company)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Original: matchmaking JobRepository.GetByCompanyId — straight LINQ port of the foreach
    /// filter on CompanyId. Read-only, no Includes (callers already have the Company).
    /// </summary>
    public async Task<IReadOnlyList<Job>> GetByCompanyIdAsync(int companyId, CancellationToken ct = default)
    {
        return await db.Jobs
            .AsNoTracking()
            .Where(j => j.CompanyId == companyId)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<Job> AddAsync(Job job, CancellationToken ct = default)
    {
        db.Jobs.Add(job);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
        return job;
    }

    public async Task UpdateAsync(Job job, CancellationToken ct = default)
    {
        db.Jobs.Update(job);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int jobId, CancellationToken ct = default)
    {
        var job = await db.Jobs.FindAsync(new object?[] { jobId }, ct).ConfigureAwait(false);
        if (job is null)
        {
            return;
        }
        db.Jobs.Remove(job);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
