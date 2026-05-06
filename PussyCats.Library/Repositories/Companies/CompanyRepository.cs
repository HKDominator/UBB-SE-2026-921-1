using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Companies;

public class CompanyRepository : ICompanyRepository
{
    private readonly PussyCatsDbContext db;

    public CompanyRepository(PussyCatsDbContext db)
    {
        this.db = db;
    }

    /// <summary>
    /// Includes Jobs so company-detail screens can render the company's postings without a second
    /// round trip. Tracked because the typical caller intends to mutate.
    /// </summary>
    public async Task<Company?> GetByIdAsync(int companyId, CancellationToken cancellationToken = default)
    {
        return await db.Companies
            .Include(c => c.Jobs)
            .FirstOrDefaultAsync(c => c.CompanyId == companyId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await db.Companies
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Company> AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        db.Companies.Add(company);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return company;
    }

    public async Task UpdateAsync(Company company, CancellationToken cancellationToken = default)
    {
        db.Companies.Update(company);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int companyId, CancellationToken cancellationToken = default)
    {
        var company = await db.Companies.FindAsync(new object?[] { companyId }, cancellationToken).ConfigureAwait(false);
        if (company is null)
        {
            return;
        }
        db.Companies.Remove(company);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
