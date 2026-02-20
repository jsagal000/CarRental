// CarRental.Api/Program.cs
using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using CarRental.Core.Models.Dtos;
using CarRental.Infrastructure.Data;
using CarRental.Infrastructure.Interfaces;
using CarRental.Infrastructure.Repositories;
using CarRental.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.IO;
using System.Text;
// ✅ USING PARA SWAGGER
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using CarRental.Api;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// ⚠️ CRÍTICO: CONFIGURACIÓN DE QUESTPDF
// ========================================
// ✅ AGREGAR ESTA LÍNEA AQUÍ (antes de cualquier configuración)
QuestPDF.Settings.License = LicenseType.Community;

// ========================================
// 1. CONFIGURACIÓN DEL DbContext
// ========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Esto evita el conflicto de ciclo de vida entre Scoped y Singleton
builder.Services.AddPooledDbContextFactory<CarRentalDbContext>(options =>
    options.UseSqlite(connectionString));

// DbContext principal derivado del factory (para controladores normales)
builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IDbContextFactory<CarRentalDbContext>>();
    return factory.CreateDbContext();
});

// ========================================
// 2. REGISTRO DE REPOSITORIOS
// ========================================
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IRentalRepository, RentalRepository>();
builder.Services.AddScoped<IPartnerRepository, PartnerRepository>();
builder.Services.AddScoped<IInsurancePolicyRepository, InsurancePolicyRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
// Repositorios para el sistema de permisos y auditoría
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();

// ========================================
// 3. REGISTRO DE SERVICIOS DE NEGOCIO
// ========================================
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IRentalService, RentalService>();
builder.Services.AddScoped<IPartnerService, PartnerService>();
builder.Services.AddScoped<IInsurancePolicyService, InsurancePolicyService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<IRepairService, RepairService>();
builder.Services.AddHostedService<CarRental.Api.Services.RentalStatusBackgroundService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// ✅ Servicios para generación de PDF (ya están registrados correctamente)
builder.Services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();
builder.Services.AddScoped<ICompanySettingsService, CompanySettingsService>();
builder.Services.AddScoped<ContractNumberService>();
builder.Services.AddScoped<RentalStatusUpdateService>();

// Servicios para el sistema de permisos y auditoría
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Inyección de IWebHostEnvironment
builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);

// ========================================
// 4. CONFIGURACIÓN DE SWAGGER/OPENAPI
// ========================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();

// ✅ CONFIGURACIÓN COMPLETA DE SWAGGER CON SOPORTE PARA ARCHIVOS
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CarRental API",
        Version = "v1",
        Description = "API para sistema de alquiler de vehículos"
    });

    // IMPORTANTE: El orden importa
    c.OperationFilter<FileUploadOperationFilter>();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ========================================
// 5. CONFIGURACIÓN DE CORS
// ========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins(
                "https://localhost:7158",
                "http://localhost:4200",
                "http://localhost:3000",
                "http://localhost:5000",
                "http://localhost:8080",
                "https://localhost:7036"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

// ========================================
// 6. AUTENTICACIÓN JWT
// ========================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// ========================================
// 7. CONFIGURACIÓN DE CULTURA (FORMATO DECIMAL)
// ========================================
var supportedCultures = new[] { new CultureInfo("en-US") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Zona horaria Ecuador
var ecuadorTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"); // UTC-5 Ecuador

// ========================================
// 8. PIPELINE DE MIDDLEWARE HTTP
// ========================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CarRental API V1");
        c.RoutePrefix = "swagger";
    });

    // Aplicar migraciones automáticamente en desarrollo
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<CarRentalDbContext>();
        try
        {
            app.Logger.LogInformation("Aplicando migraciones de base de datos...");
            dbContext.Database.Migrate();
            app.Logger.LogInformation("✓ Migraciones aplicadas exitosamente");

            // ✅ Verificar tabla AuditLogs con enfoque robusto
            try
            {
                var auditCount = dbContext.AuditLogs.Count();
                app.Logger.LogInformation("✓ Tabla AuditLogs verificada correctamente ({Count} registros)", auditCount);
            }
            catch (Exception auditEx)
            {
                app.Logger.LogWarning(auditEx, "⚠ Tabla AuditLogs no existe - se creará en la próxima migración");
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "✗ Error al aplicar migraciones");
        }
    }
}

app.UseHttpsRedirection();

// ========================================
// 9. ARCHIVOS ESTÁTICOS
// ========================================

// Servir archivos desde wwwroot (como Contrato_Template.pdf)
app.UseStaticFiles();

// Servir imágenes desde la carpeta 'Uploads'
string uploadsPhysicalPath = Path.Combine(app.Environment.ContentRootPath, "Uploads");

if (!Directory.Exists(uploadsPhysicalPath))
{
    Directory.CreateDirectory(uploadsPhysicalPath);
    app.Logger.LogInformation("Carpeta 'Uploads' creada en: {UploadsPath}", uploadsPhysicalPath);
}

var uploadsPhysicalFileProvider = new PhysicalFileProvider(uploadsPhysicalPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = uploadsPhysicalFileProvider,
    RequestPath = "/Uploads"
});

// Permite navegar por directorios en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseDirectoryBrowser(new DirectoryBrowserOptions
    {
        FileProvider = uploadsPhysicalFileProvider,
        RequestPath = "/Uploads"
    });
}

// ========================================
// 10. ROUTING Y AUTENTICACIÓN
// ========================================
app.UseRouting();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ========================================
// 11. MENSAJE DE INICIO
// ========================================
app.Logger.LogInformation("========================================");
app.Logger.LogInformation("CarRental API Iniciada");
app.Logger.LogInformation("Entorno: {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("Sistema de Auditoría: ACTIVO");
app.Logger.LogInformation("Autenticación JWT: ACTIVA");
app.Logger.LogInformation("Generación de PDF: ACTIVA");
app.Logger.LogInformation("Swagger UI: https://localhost:7036/swagger");
app.Logger.LogInformation("========================================");

app.Run();