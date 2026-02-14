// CarRental.Api/Controllers/DashboardController.cs
using CarRental.Api.Attributes;
using CarRental.Api.Extensions;
using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRental.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IAuditService _auditService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IDashboardService dashboardService,
            IAuditService auditService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene los datos del dashboard: alquileres activos y cumpleaños del día
        /// </summary>
        [HttpGet]
        [RequirePermission("Dashboard", "View")]
        public async Task<ActionResult<DashboardDataDto>> GetDashboardData()
        {
            var userId = GetCurrentUserId();

            try
            {
                // Obtener alquileres activos con relaciones
                var activeRentals = await _dashboardService.GetActiveRentalsAsync();

                // Obtener clientes con cumpleaños hoy
                var customersWithBirthday = await _dashboardService.GetTodayBirthdaysAsync();

                var dashboardData = new DashboardDataDto
                {
                    ActiveRentals = activeRentals.Select(r => new ActiveRentalDto
                    {
                        RentalId = r.Id,
                        CustomerName = $"{r.Customer?.FirstName} {r.Customer?.LastName}",
                        CustomerPhone = r.Customer?.PhoneNumber,
                        VehicleName = $"{r.Vehicle?.Make} {r.Vehicle?.Model}",
                        VehiclePlate = r.Vehicle?.LicensePlate,
                        StartDate = r.StartDate,
                        EndDate = r.EndDate,
                        DailyRate = r.DailyRate,
                        TotalCost = r.TotalCost,
                        DestinationType = r.DestinationType.ToString(),
                        DestinationCity = r.DestinationCityName
                    }).ToList(),

                    TodayBirthdays = customersWithBirthday.Select(c => new CustomerBirthdayDto
                    {
                        CustomerId = c.Id,
                        CustomerName = $"{c.FirstName} {c.LastName}",
                        Email = c.Email,
                        PhoneNumber = c.PhoneNumber,
                        Age = c.DateOfBirth.HasValue ? DateTime.Today.Year - c.DateOfBirth.Value.Year : 0,
                        BirthYear = c.DateOfBirth.HasValue ? c.DateOfBirth.Value.Year : 0
                    }).ToList()
                };

                _logger.LogInformation(
                    "Dashboard data loaded: {ActiveRentals} active rentals, {Birthdays} birthdays",
                    dashboardData.ActiveRentals.Count,
                    dashboardData.TodayBirthdays.Count);

                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Dashboard",
                    action: "View",
                    description: "Error al cargar datos del dashboard",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                _logger.LogError(ex, "Error loading dashboard data");
                return StatusCode(500, "Error interno al cargar datos del dashboard");
            }
        }

        /// <summary>
        /// Obtiene estadísticas resumidas del dashboard
        /// </summary>
        [HttpGet("statistics")]
        [RequirePermission("Dashboard", "View")]
        public async Task<ActionResult<DashboardStatisticsDto>> GetDashboardStatistics()
        {
            var userId = GetCurrentUserId();

            try
            {
                var statistics = await _dashboardService.GetDashboardStatisticsAsync();

                var statisticsDto = new DashboardStatisticsDto
                {
                    TotalActiveRentals = statistics.TotalActiveRentals,
                    TotalReservedRentals = statistics.TotalReservedRentals,
                    TotalOverdueRentals = statistics.TotalOverdueRentals,
                    AvailableVehicles = statistics.AvailableVehicles,
                    TotalCustomers = statistics.TotalCustomers
                };

                return Ok(statisticsDto);
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Dashboard",
                    action: "View",
                    description: "Error al cargar estadísticas del dashboard",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                _logger.LogError(ex, "Error loading dashboard statistics");
                return StatusCode(500, "Error interno al cargar estadísticas del dashboard");
            }
        }

        /// <summary>
        /// Obtiene información de vehículos alquilados y disponibles
        /// </summary>
        [HttpGet("vehicles")]
        [RequirePermission("Dashboard", "View")]
        public async Task<ActionResult<List<VehicleInfoDto>>> GetVehiclesInfo()
        {
            var userId = GetCurrentUserId();

            try
            {
                var vehiclesInfo = await _dashboardService.GetVehiclesInfoAsync();

                _logger.LogInformation(
                    "Vehicles info loaded: {TotalVehicles} vehicles",
                    vehiclesInfo.Count());

                return Ok(vehiclesInfo.ToList());
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync(
                    userId: userId,
                    module: "Dashboard",
                    action: "View",
                    description: "Error al cargar información de vehículos",
                    ipAddress: HttpContext.GetClientIpAddress(),
                    userAgent: HttpContext.GetUserAgent(),
                    isSuccess: false,
                    errorMessage: ex.Message
                );

                _logger.LogError(ex, "Error loading vehicles info");
                return StatusCode(500, "Error interno al cargar información de vehículos");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 0;
        }
    }

    // DTOs para el Dashboard
    public class DashboardDataDto
    {
        public List<ActiveRentalDto> ActiveRentals { get; set; } = new();
        public List<CustomerBirthdayDto> TodayBirthdays { get; set; } = new();
    }

    public class ActiveRentalDto
    {
        public int RentalId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string VehicleName { get; set; }
        public string VehiclePlate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal DailyRate { get; set; }
        public decimal TotalCost { get; set; }
        public string DestinationType { get; set; }
        public string DestinationCity { get; set; }
    }

    public class CustomerBirthdayDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int Age { get; set; }
        public int BirthYear { get; set; }
    }

    public class VehicleInfoDto
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string LicensePlate { get; set; }
        public decimal DailyRate { get; set; }
        public string State { get; set; }
        public string Ownership { get; set; }
        public string CustomerName { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class DashboardStatisticsDto
    {
        public int TotalActiveRentals { get; set; }
        public int TotalReservedRentals { get; set; }
        public int TotalOverdueRentals { get; set; }
        public int AvailableVehicles { get; set; }
        public int TotalCustomers { get; set; }
    }
}