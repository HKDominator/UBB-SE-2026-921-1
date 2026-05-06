using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Persistence;
using PussyCats.Library.Repositories.Companies;
using PussyCats.Library.Repositories.Documents;
using PussyCats.Library.Repositories.Jobs;
using PussyCats.Library.Repositories.Matches;
using PussyCats.Library.Repositories.PersonalityTests;
using PussyCats.Library.Repositories.Recommendations;
using PussyCats.Library.Repositories.Skills;
using PussyCats.Library.Repositories.SkillTests;
using PussyCats.Library.Repositories.Users;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<PussyCatsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PussyCatsDb")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IPersonalityTestRepository, PersonalityTestRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IRecommendationRepository, RecommendationRepository>();
builder.Services.AddScoped<IJobSkillRepository, JobSkillRepository>();
builder.Services.AddScoped<ISkillGroupRepository, SkillGroupRepository>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<IUserSkillRepository, UserSkillRepository>();
builder.Services.AddScoped<ISkillTestRepository, SkillTestRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
