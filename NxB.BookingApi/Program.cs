using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NxB.BookingApi.Controllers.Ordering;
using NxB.BookingApi.Controllers.Accounting;
using NxB.BookingApi.Controllers.Inventory;
using NxB.BookingApi.Controllers.Login;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;
using Munk.AspNetCore;
using NxB.Domain.Common.Interfaces;
using NxB.Settings.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Add Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add custom services from merged APIs
ConfigureCommonServices(builder.Services, builder.Configuration);
ConfigureOrderingServices(builder.Services, builder.Configuration);
ConfigureAccountingServices(builder.Services, builder.Configuration);
ConfigureInventoryServices(builder.Services, builder.Configuration);
ConfigureLoginServices(builder.Services, builder.Configuration);
ConfigureTenantServices(builder.Services, builder.Configuration);
ConfigurePricingServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static void ConfigureOrderingServices(IServiceCollection services, IConfiguration configuration)
{
    // Services from OrderingApi Startup.cs
    services.AddScoped<IAvailabilityMatrixRepository, AvailabilityMatrixRepository<AppDbContext>>();
    services.AddScoped<AvailabilityMatrixFactory>();
    services.AddScoped<IAppDbContextFactory<AppDbContext>, AppDbContextFactory<AppDbContext>>();
    services.AddScoped<IAllocationRepository, AllocationRepository<AppDbContext>>();
    services.AddScoped<IAllocationRepositoryCached, AllocationRepositoryCached>();
    services.AddScoped<IStartDateTimeChunkDivider, StartDateTimeChunkDivider>();
    services.AddScoped<IRentalCacheProvider, RentalAvailabilityCacheFactory>();
    services.AddScoped<IRentalCaches, RentalCaches>();
    services.AddScoped<IRentalUnitRepository, RentalUnitRepository<AppDbContext>>();
    services.AddScoped<ICounterIdProvider, CounterIdProvider<AppDbContext>>();
    services.AddSingleton<IAuthorTranslator<AppDbContext>, AuthorTranslator<AppDbContext>>();
    services.AddScoped<ISettingsRepository, SettingsRepository<AppDbContext>>();
    //services.AddScoped<IKeyCodeGenerator, KeyCodeGenerator>();

    //services.AddScoped<IPriceProfilesValidator, PriceProfile>();
    //services.AddSingleton(sp => ServiceProxyHelper.CreateActorServiceProxy<IMemCacheActor>(0));}
}

static void ConfigureAccountingServices(IServiceCollection services, IConfiguration configuration)
{
    // Services from AccountingApi Startup.cs
    // Note: Service Fabric specific services removed/replaced with standard equivalents
    services.AddAutoMapper(typeof(Program));
}

static void ConfigureInventoryServices(IServiceCollection services, IConfiguration configuration)
{
    // Services from InventoryApi Startup.cs
    services.AddAutoMapper(typeof(Program));
}

static void ConfigureLoginServices(IServiceCollection services, IConfiguration configuration)
{
    // Services from LoginApi Startup.cs
    services.AddAutoMapper(typeof(Program));
}

static void ConfigureTenantServices(IServiceCollection services, IConfiguration configuration)
{
    // Services from TenantApi Startup.cs (Service Fabric dependencies removed)

    // Register repositories
    services.AddScoped<ITenantRepository, TenantRepository>();
    services.AddScoped<ITextSectionRepository, TextSectionRepository>();
    services.AddScoped<ITextSectionUserRepository, TextSectionUserRepository>();
    services.AddScoped<IBillableItemsRepository, BillableItemsRepository>();
    services.AddScoped<IKioskRepository, KioskRepository>();
    services.AddScoped<IFeatureModuleRepository, FeatureModuleRepository>();
    services.AddScoped<IExternalPaymentTransactionRepository, ExternalPaymentTransactionRepository>();

    // Register services
    services.AddScoped<IFeatureModuleService, FeatureModuleService>();
    services.AddScoped<BillingService>();
    // services.AddScoped<FileWritingService>(); // TODO: Review if this service is needed
    services.AddScoped<IVerifoneGateway, VerifoneGateway>();

    // Register factories
    services.AddScoped<TenantFactory>();
    services.AddScoped<BillableItemFactory>();
    services.AddScoped<KioskFactory>();
    services.AddScoped<FeatureModuleFactory>();
    services.AddScoped<ExternalPaymentTransactionFactory>();
}

static void ConfigurePricingServices(IServiceCollection services, IConfiguration configuration)
{
    // Services from PricingApi Startup.cs (Service Fabric dependencies removed)

    // Register repositories
    services.AddScoped<IPriceProfileRepository, PriceProfileRepository>();
    services.AddScoped<ICostIntervalRepository, CostIntervalRepository>();

    // Register services
    services.AddScoped<IPriceCalculator, PriceCalculator>();
    services.AddScoped<IPriceProfilesValidator, PriceProfilesValidator>();

    // Register factories
    services.AddScoped<PriceProfileFactory>();
    services.AddScoped<CostIntervalFactory>();

    // Add pricing AutoMapper profile
    services.AddAutoMapper(typeof(PricingMappingProfile));
}

static void ConfigureCommonServices(IServiceCollection services, ConfigurationManager configuration)
{
    services.AddAutoMapper(typeof(Program));
}