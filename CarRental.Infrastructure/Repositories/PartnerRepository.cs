// CarRental.Infrastructure/Repositories/PartnerRepository.cs
using CarRental.Core.Models;
using CarRental.Infrastructure.Data;
using CarRental.Infrastructure.Interfaces;

namespace CarRental.Infrastructure.Repositories
{
    public class PartnerRepository : GenericRepository<Partner>, IPartnerRepository
    {
        public PartnerRepository(CarRentalDbContext context) : base(context)
        {
        }

        // Aquí puedes implementar métodos específicos para partners si son necesarios
        // Por ejemplo:
        // public async Task<Partner> GetPartnerByCedulaAsync(string cedula)
        // {
        //     return await _context.Partners.FirstOrDefaultAsync(p => p.Cedula == cedula);
        // }
    }
}