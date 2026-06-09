using Blazored.Toast;
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using CarRental.Web;
using CarRental.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configurar la cultura para toda la aplicación
var culture = new CultureInfo("en-US");
culture.NumberFormat.CurrencySymbol = "$";
culture.NumberFormat.CurrencyDecimalDigits = 2;
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ===== SERVICIOS DE AUTENTICACIÓN =====
builder.Services.AddScoped<AuthStateService>();

// ===== HTTP CLIENTS =====

// HttpClient NO autenticado (para login y endpoints públicos)
builder.Services.AddHttpClient<AuthApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7036/");
});

// HttpClient autenticado (para endpoints que requieren JWT)
builder.Services.AddHttpClient<AuthorizedHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7036/");
});

// HttpClient estándar (para servicios legacy - migrar gradualmente)
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7036/")
});

// ===== API CLIENTS CON AUTENTICACIÓN =====
builder.Services.AddScoped<UserApiClient>();
builder.Services.AddScoped<CustomerApiClient>();
builder.Services.AddScoped<PermissionApiClient>();
builder.Services.AddScoped<AuditApiClient>();

// ===== API CLIENTS LEGACY (sin autenticación por ahora) =====
builder.Services.AddScoped<VehicleApiClient>();

builder.Services.AddScoped<RentalApiClient>(sp =>
{
    var httpClient = sp.GetRequiredService<AuthorizedHttpClient>();
    var jsRuntime = sp.GetRequiredService<IJSRuntime>();
    return new RentalApiClient(httpClient, jsRuntime);
});

builder.Services.AddScoped<PartnerApiClient>();
builder.Services.AddScoped<CompanySettingsApiClient>();
builder.Services.AddScoped<DashboardApiClient>();
builder.Services.AddScoped<PaymentApiClient>();

// ===== SUB-RECURSOS DE VEHÍCULOS =====
builder.Services.AddScoped<IVehicleSubResourceApiClient<InsurancePolicy, InsurancePolicyForCreationDto>>(sp =>
    new VehicleSubResourceApiClient<InsurancePolicy, InsurancePolicyForCreationDto>(
        sp.GetRequiredService<HttpClient>(),
        "insurancepolicies"
    ));

builder.Services.AddScoped<IVehicleSubResourceApiClient<Maintenance, MaintenanceForCreationDto>>(sp =>
    new VehicleSubResourceApiClient<Maintenance, MaintenanceForCreationDto>(
        sp.GetRequiredService<HttpClient>(),
        "maintenances"
    ));

builder.Services.AddScoped<IVehicleSubResourceApiClient<Repair, RepairForCreationDto>>(sp =>
    new VehicleSubResourceApiClient<Repair, RepairForCreationDto>(
        sp.GetRequiredService<HttpClient>(),
        "repairs"
    ));

// ===== SERVICIOS ADICIONALES =====
builder.Services.AddBlazoredToast();
builder.Services.AddScoped<CarRental.Web.Services.ModalService>();
// ===== CONSTRUIR Y EJECUTAR =====
await builder.Build().RunAsync();