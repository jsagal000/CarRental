// CarRental.Infrastructure/Services/CustomerService.cs
using CarRental.Infrastructure.Interfaces; // Para ICustomerService e ICustomerRepository
using CarRental.Core.Interfaces;
using CarRental.Core.Models;     // Para el modelo Customer
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarRental.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _customerRepository.GetAllAsync();
        }

        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            return await _customerRepository.GetByIdAsync(id);
        }

        public async Task<Customer> AddCustomerAsync(Customer customer)
        {
            await _customerRepository.AddAsync(customer);
            return customer;
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            var existingCustomer = await _customerRepository.GetByIdAsync(customer.Id);

            if (existingCustomer == null)
            {
                throw new KeyNotFoundException($"Cliente con ID {customer.Id} no encontrado para actualizar.");
            }

            existingCustomer.FirstName = customer.FirstName;
            existingCustomer.LastName = customer.LastName;
            existingCustomer.Email = customer.Email;
            existingCustomer.PhoneNumber = customer.PhoneNumber;
            // Propiedades eliminadas: LicenseNumber, PostalCode

            // <<-- NUEVAS PROPIEDADES A ACTUALIZAR -->>
            existingCustomer.TypeOfDocument = customer.TypeOfDocument;
            existingCustomer.DocumentNumber = customer.DocumentNumber;
            // <<-- FIN DE NUEVAS PROPIEDADES -->>

            existingCustomer.DateOfBirth = customer.DateOfBirth;
            existingCustomer.Address = customer.Address;
            existingCustomer.City = customer.City;
            existingCustomer.StateProvince = customer.StateProvince;
            existingCustomer.Country = customer.Country;
            // RegistrationDate no se actualiza ya que es la fecha de creación

            await _customerRepository.UpdateAsync(existingCustomer);
        }

        public async Task DeleteCustomerAsync(int id)
        {
            await _customerRepository.DeleteAsync(id);
        }
    }
}
