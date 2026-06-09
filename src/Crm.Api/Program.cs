using Crm.Api;
using Crm.Core.Application.Abstractions;
using Crm.Core.Application.Analytics;
using Crm.Core.Application.ApiMetrics;
using Crm.Core.Application.Organizations;
using Crm.Core.Application.Payments;
using Crm.Core.Application.Products;
using Crm.Core.Application.Prospects;
using Crm.Core.Infrastructure.PackageStats;
using Crm.Core.Infrastructure.Persistence;
using Crm.Core.Infrastructure.Persistence.Repositories;
using Crm.Core.Infrastructure.Time;
using Crm.Kernel.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CrmDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CrmDatabase")));

builder.Services.AddHttpClient<NuGetStatsHttpClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["NuGet:SearchBaseUrl"] ?? "https://azuresearch-usnc.nuget.org/"));
builder.Services.AddHttpClient<NpmStatsHttpClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Npm:ApiBaseUrl"] ?? "https://api.npmjs.org/"));
builder.Services.AddHttpClient<GitHubStatsHttpClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["GitHub:ApiBaseUrl"] ?? "https://api.github.com/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Outlet-CRM");
    var token = builder.Configuration["GitHub:Token"];
    if (!string.IsNullOrWhiteSpace(token))
    {
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
    }
});

builder.Services.AddScoped<IPackageStatsClient, PackageStatsClient>();
builder.Services.AddScoped<IRepoStatsClient>(sp => sp.GetRequiredService<GitHubStatsHttpClient>());

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IProspectRepository, ProspectRepository>();
builder.Services.AddScoped<IDownloadSnapshotRepository, DownloadSnapshotRepository>();
builder.Services.AddScoped<IRepositorySnapshotRepository, RepositorySnapshotRepository>();
builder.Services.AddScoped<IApiMetricRepository, ApiMetricRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

builder.Services.AddScoped<CreateProduct>();
builder.Services.AddScoped<CreateOrganization>();
builder.Services.AddScoped<TrackPackage>();
builder.Services.AddScoped<TrackRepository>();
builder.Services.AddScoped<CreateProspect>();
builder.Services.AddScoped<AdvanceProspectStage>();
builder.Services.AddScoped<CaptureDownloadSnapshot>();
builder.Services.AddScoped<CaptureProductSnapshots>();
builder.Services.AddScoped<GetDownloadTrend>();
builder.Services.AddScoped<GetRepositoryHistory>();
builder.Services.AddScoped<RecordApiMetric>();
builder.Services.AddScoped<GetEndpointStatistics>();
builder.Services.AddScoped<RecordPayment>();
builder.Services.AddScoped<SettlePayment>();

// Authentification déléguée au SSO Outlet (Outlet-SSO) : l'API ne gère aucune identité,
// elle valide les tokens émis par l'authority configurée. Sans authority configurée
// (dev local), les endpoints restent ouverts.
var ssoAuthority = builder.Configuration["Sso:Authority"];
var ssoConfigured = !string.IsNullOrWhiteSpace(ssoAuthority);

if (ssoConfigured)
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = ssoAuthority;
            options.Audience = builder.Configuration["Sso:Audience"] ?? "outlet-crm";
        });
    builder.Services.AddAuthorization();
}

var app = builder.Build();

if (ssoConfigured)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapCrmEndpoints(requireAuthorization: ssoConfigured);

app.Run();
