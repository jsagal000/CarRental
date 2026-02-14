// CarRental.Infrastructure/Data/CarRentalDbContext.cs
using CarRental.Core.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using BCrypt.Net;

namespace CarRental.Infrastructure.Data
{
    public class CarRentalDbContext : DbContext
    {
        public CarRentalDbContext(DbContextOptions<CarRentalDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<InsurancePolicy> InsurancePolicies { get; set; }
        public DbSet<Maintenance> Maintenances { get; set; }
        public DbSet<Repair> Repairs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<CompanySettings> CompanySettings { get; set; }
        public DbSet<ContractNumber> ContractNumbers { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureVehicle(modelBuilder);
            ConfigureCustomer(modelBuilder);
            ConfigureRental(modelBuilder);
            ConfigureUser(modelBuilder);
            ConfigureUserSession(modelBuilder);
            ConfigurePermissions(modelBuilder);
            ConfigureAudit(modelBuilder);
            ConfigureCompanySettings(modelBuilder);
            ConfigureContractNumber(modelBuilder);
            SeedDefaultData(modelBuilder);
        }

        private void ConfigureVehicle(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vehicle>()
                .Property(v => v.ImageUrls)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>()
                )
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()
                ));

            modelBuilder.Entity<Vehicle>()
                .Property(v => v.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Vehicle>()
                .Property(v => v.State)
                .HasConversion<string>();

            modelBuilder.Entity<Vehicle>()
                .Property(v => v.DailyRate)
                .HasColumnType("decimal(18, 2)");
        }

        private void ConfigureCustomer(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Customer>()
                .Property(c => c.TypeOfDocument)
                .HasConversion<string>();
        }

        private void ConfigureRental(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rental>()
                .Property(r => r.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Rental>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rental>()
                .HasOne(r => r.Vehicle)
                .WithMany()
                .HasForeignKey(r => r.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rental>()
                .Property(r => r.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Rental>()
                .Property(r => r.DailyRate)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Rental>()
                .Property(r => r.TotalCost)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Rental>()
                .Property(r => r.OverdueCharges)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Rental>()
                .Property(r => r.DestinationCityName)
                .HasMaxLength(100);

            modelBuilder.Entity<Rental>()
                .Property(r => r.MileageAtDelivery)
                .IsRequired();

            modelBuilder.Entity<Rental>()
                .Property(r => r.MileageAtReturn)
                .IsRequired(false);

            modelBuilder.Entity<Rental>()
                .Property(r => r.DestinationType)
                .HasConversion<string>();

            modelBuilder.Entity<Rental>()
                .Property(r => r.DriverLicenseType)
                .HasConversion<string>();
        }

        private void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Role).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });
        }

        private void ConfigureUserSession(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ExpiresAt).IsRequired();
                entity.Property(e => e.IsRevoked).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Token);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ExpiresAt);
            });
        }

        private void ConfigurePermissions(ModelBuilder modelBuilder)
        {
            // CONFIGURACIÓN ACTUALIZADA para Permission con jerarquía
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Relación jerárquica (auto-referencia)
                entity.HasOne(e => e.ParentPermission)
                      .WithMany(e => e.ChildPermissions)
                      .HasForeignKey(e => e.ParentPermissionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.Module).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Type).IsRequired(); // NUEVO
                entity.Property(e => e.DisplayOrder).HasDefaultValue(0); // NUEVO
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                // Índices para optimización
                entity.HasIndex(e => e.Module);
                entity.HasIndex(e => e.Type); // NUEVO
                entity.HasIndex(e => e.DisplayOrder); // NUEVO
                entity.HasIndex(e => e.ParentPermissionId); // NUEVO
                entity.HasIndex(e => new { e.Module, e.Action });
            });

            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Permission)
                      .WithMany(p => p.RolePermissions)
                      .HasForeignKey(e => e.PermissionId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.Role, e.PermissionId }).IsUnique();
                entity.Property(e => e.IsGranted).HasDefaultValue(true);
            });

            modelBuilder.Entity<UserPermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Permission)
                      .WithMany(p => p.UserPermissions)
                      .HasForeignKey(e => e.PermissionId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.UserId, e.PermissionId }).IsUnique();
                entity.Property(e => e.IsGranted).HasDefaultValue(true);
            });
        }

        private void ConfigureAudit(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.Property(e => e.Module).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(20);
                entity.Property(e => e.EntityName).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
                entity.Property(e => e.IsSuccess).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => new { e.Module, e.Action });
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IsSuccess);
            });
        }

        private void SeedDefaultData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@carrental.com",
                PasswordHash = "$2a$12$7VKXd8IvLzNrMnazAXGjteZr3chY8vtU7F8hRMutTqxqEaB9kPbD6",
                FirstName = "Administrador",
                LastName = "Sistema",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            // NOTA: El seed de permisos ahora se hace mediante SQL o el endpoint de inicialización
            // porque es más complejo con la jerarquía. 
            // Elimina los métodos SeedPermissions y SeedRolePermissions del DbContext
            // y usa el script SQL proporcionado o el botón "Inicializar Permisos"
        }

        private void ConfigureCompanySettings(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompanySettings>(entity =>
            {
                entity.ToTable("CompanySettings");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ActivityDescription).HasMaxLength(500);
                entity.Property(e => e.Address).HasMaxLength(300);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Phone1).HasMaxLength(20);
                entity.Property(e => e.Phone2).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Website).HasMaxLength(100);
                entity.Property(e => e.CreatedDate).IsRequired();
            });
        }

        private void ConfigureContractNumber(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContractNumber>(entity =>
            {
                entity.ToTable("ContractNumbers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ContractCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SequentialNumber).IsRequired();
                entity.Property(e => e.Year).IsRequired();
                entity.Property(e => e.GeneratedDate).IsRequired();

                // Índices
                entity.HasIndex(e => e.RentalId).IsUnique();
                entity.HasIndex(e => e.ContractCode).IsUnique();
                entity.HasIndex(e => e.Year);

                // Relación con Rental
                entity.HasOne(e => e.Rental)
                    .WithMany()
                    .HasForeignKey(e => e.RentalId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}