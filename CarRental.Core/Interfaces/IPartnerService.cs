// CarRental.Core/Interfaces/IPartnerService.cs
using CarRental.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental.Core.Interfaces
{
    public interface IPartnerService
    {
        Task<IEnumerable<Partner>> GetAllPartnersAsync();
        Task<Partner> GetPartnerByIdAsync(int id);
        Task<Partner> AddPartnerAsync(Partner partner);
        Task UpdatePartnerAsync(Partner partner); // Firma estándar, el servicio manejará la lógica de actualización
        Task DeletePartnerAsync(int id);
    }
}