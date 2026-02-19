using Microsoft.Data.SqlClient;
using DataReconciliationEngine.Application.Interfaces;
using DataReconciliationEngine.Infrastructure.Persistence.Contexts;
using DataReconciliationEngine.Infrastructure.Services;
using DataReconciliationEngine.Web.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;


internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // =====================
        // Connection strings (Doppler / appsettings)
        // =====================

        static string Require(string? value, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(errorMessage);
            return value;
        }

        // Base SQL Server connection 
        var baseServerConn =
            Environment.GetEnvironmentVariable("CONN_STRING")
            ?? builder.Configuration.GetConnectionString("BaseServer");

        baseServerConn = Require(baseServerConn,
            "Base server connection string not found. Set CONN_STRING via Doppler or ConnectionStrings:BaseServer.");

        // Build per-database connection strings from base
        var baseBuilder = new SqlConnectionStringBuilder(baseServerConn);

        // System A = PRO_BE01
        var systemAConn = new SqlConnectionStringBuilder(baseBuilder.ConnectionString)
        {
            InitialCatalog = "PRO_BE01"
        }.ConnectionString;

        // System B = Company
        var systemBConn = new SqlConnectionStringBuilder(baseBuilder.ConnectionString)
        {
            InitialCatalog = "Company"
        }.ConnectionString;

        // Local reconciliation DB (LocalDB) 
        var localConn =
            Environment.GetEnvironmentVariable("LOCAL_CONN_STRING")
            ?? builder.Configuration.GetConnectionString("LocalReconciliation");

        localConn = Require(localConn,
            "Local reconciliation connection string not found. Set LOCAL_CONN_STRING via Doppler or ConnectionStrings:LocalReconciliation.");

        //exposing them in Configuration so existing GetConnectionString() still works
        builder.Configuration["ConnectionStrings:SystemA"] = systemAConn;
        builder.Configuration["ConnectionStrings:SystemB"] = systemBConn;
        builder.Configuration["ConnectionStrings:LocalReconciliation"] = localConn;


        builder.Services.AddDbContext<SystemADbContext>(options =>
        {
            options.UseSqlServer(systemAConn, sql => sql.EnableRetryOnFailure());
            if (builder.Environment.IsDevelopment())
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
                options.LogTo(Console.WriteLine);
            }
        });

        builder.Services.AddDbContext<SystemBDbContext>(options =>
        {
            options.UseSqlServer(systemBConn, sql => sql.EnableRetryOnFailure());
            if (builder.Environment.IsDevelopment())
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
                options.LogTo(Console.WriteLine);
            }
        });

        builder.Services.AddDbContext<ReconciliationDbContext>(options =>
        {
            options.UseSqlServer(localConn, sql => sql.EnableRetryOnFailure());
            if (builder.Environment.IsDevelopment())
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
                options.LogTo(Console.WriteLine);
            }
        });



        builder.Services.AddScoped<IComparisonEngine, ComparisonEngine>();
        builder.Services.AddScoped<IRunQueryService, RunQueryService>();
        builder.Services.AddScoped<IComparisonConfigService, ComparisonConfigService>();
        builder.Services.AddScoped<IFieldMappingService, FieldMappingService>();
        builder.Services.AddScoped<IResultExportService, ResultExportService>();

        builder.Services.AddMudServices();

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        // MigrateAsync() in the seeder handles both schema creation AND seed data.
        await DataReconciliationEngine.Web.Data.DbSeeder.SeedAsync(app.Services);

        app.Run();
    }
}