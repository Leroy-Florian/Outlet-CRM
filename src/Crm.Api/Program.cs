using Crm.Api;
using Crm.Core.Application.Abstractions;
using Crm.Core.Application.Analytics;
using Crm.Core.Application.ApiMetrics;
using Crm.Core.Application.Payments;
using Crm.Core.Application.Prospects;
using Crm.Core.Infrastructure.NuGet;
using Crm.Core.Infrastructure.Persistence;
using Crm.Core.Infrastructure.Persistence.Repositories;
using Crm.Core.Infrastructure.Time;
using Crm.Kernel.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CrmDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CrmDatabase")));

builder.Services.AddHttpClient<INuGetStatsClient, NuGetStatsHttpClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["NuGet:SearchBaseUrl"] ?? "https://azuresearch-usnc.nuget.org/"));

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
builder.Services.AddScoped<IProspectRepository, ProspectRepository>();
builder.Services.AddScoped<IDownloadSnapshotRepository, DownloadSnapshotRepository>();
builder.Services.AddScoped<IApiMetricRepository, ApiMetricRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

builder.Services.AddScoped<CreateProspect>();
builder.Services.AddScoped<AdvanceProspectStage>();
builder.Services.AddScoped<CaptureDownloadSnapshot>();
builder.Services.AddScoped<GetDownloadTrend>();
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
