using System.ComponentModel.DataAnnotations;

namespace CarRental.Core.Models.Dtos
{
    public class AuditLogDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserFullName { get; set; }
        public string Module { get; set; }
        public string Action { get; set; }
        public int? EntityId { get; set; }
        public string EntityName { get; set; }
        public string Description { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class AuditLogFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Module { get; set; }
        public string? Action { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public bool? IsSuccess { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}