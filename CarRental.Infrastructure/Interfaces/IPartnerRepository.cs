// CarRental.Core/Interfaces/IPartnerRepository.cs
using CarRental.Core.Models;

namespace CarRental.Infrastructure.Interfaces
{
    // Heredamos de IGenericRepository para obtener las operaciones CRUD básicas
    public interface IPartnerRepository : IGenericRepository<Partner>
    {
        // Puedes añadir métodos específicos para partners aquí si son necesarios.
        // Por ejemplo: Task<Partner> GetPartnerByCedulaAsync(string cedula);
        // Task<IEnumerable<Partner>> GetPartnersByBankAsync(string bank);
    }
}