// CarRental.Infrastructure/Services/PartnerService.cs
using CarRental.Infrastructure.Interfaces; // Para IPartnerService e IPartnerRepository
using CarRental.Core.Interfaces;
using CarRental.Core.Models;     // Para el modelo Partner
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental.Infrastructure.Services
{
    public class PartnerService : IPartnerService
    {
        private readonly IPartnerRepository _partnerRepository;

        public PartnerService(IPartnerRepository partnerRepository)
        {
            _partnerRepository = partnerRepository;
        }

        public async Task<IEnumerable<Partner>> GetAllPartnersAsync()
        {
            return await _partnerRepository.GetAllAsync();
        }

        public async Task<Partner> GetPartnerByIdAsync(int id)
        {
            return await _partnerRepository.GetByIdAsync(id);
        }

        public async Task<Partner> AddPartnerAsync(Partner partner)
        {
            await _partnerRepository.AddAsync(partner);
            return partner;
        }

        public async Task UpdatePartnerAsync(Partner partner)
        {
            var existingPartner = await _partnerRepository.GetByIdAsync(partner.Id);

            if (existingPartner == null)
            {
                throw new KeyNotFoundException($"Partner con ID {partner.Id} no encontrado para actualizar.");
            }

            existingPartner.FirstName = partner.FirstName;
            existingPartner.LastName = partner.LastName;
            existingPartner.Cedula = partner.Cedula;
            existingPartner.Email = partner.Email;
            existingPartner.PhoneNumber = partner.PhoneNumber;
            existingPartner.Country = partner.Country;
            existingPartner.Province = partner.Province;
            existingPartner.City = partner.City;
            existingPartner.Address = partner.Address;
            existingPartner.Bank = partner.Bank;
            existingPartner.TypeOfAccount = partner.TypeOfAccount;
            existingPartner.AccountNumber = partner.AccountNumber;
            // RegistrationDate no se actualiza ya que es la fecha de creación

            await _partnerRepository.UpdateAsync(existingPartner);
        }

        public async Task DeletePartnerAsync(int id)
        {
            await _partnerRepository.DeleteAsync(id);
        }
    }
}