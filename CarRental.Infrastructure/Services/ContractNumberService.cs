using CarRental.Core.Models;
using CarRental.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace CarRental.Infrastructure.Services
{
    public class ContractNumberService
    {
        private readonly CarRentalDbContext _context;

        public ContractNumberService(CarRentalDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateContractNumberAsync(int rentalId)
        {
            try
            {
                var currentYear = DateTime.Now.Year;

                // Verificar si ya existe un número para este alquiler
                var existingContract = await _context.ContractNumbers
                    .FirstOrDefaultAsync(c => c.RentalId == rentalId);

                if (existingContract != null)
                {
                    return existingContract.ContractCode;
                }

                // Obtener el último número secuencial del año actual
                var lastContract = await _context.ContractNumbers
                    .Where(c => c.Year == currentYear)
                    .OrderByDescending(c => c.SequentialNumber)
                    .FirstOrDefaultAsync();

                var nextSequential = (lastContract?.SequentialNumber ?? 0) + 1;

                // Formato: CONT-2025-0001
                var contractCode = $"CONT-{currentYear}-{nextSequential:D4}";

                var contractNumber = new ContractNumber
                {
                    RentalId = rentalId,
                    ContractCode = contractCode,
                    SequentialNumber = nextSequential,
                    Year = currentYear,
                    GeneratedDate = DateTime.Now
                };

                _context.ContractNumbers.Add(contractNumber);
                await _context.SaveChangesAsync();

                return contractCode;
            }
            catch (Exception ex)
            {
                return $"CONT-{DateTime.Now.Year}-ERROR";
            }
        }

        public async Task<string> GetContractNumberByRentalIdAsync(int rentalId)
        {
            var contract = await _context.ContractNumbers
                .FirstOrDefaultAsync(c => c.RentalId == rentalId);

            return contract?.ContractCode ?? string.Empty;
        }
    }
}