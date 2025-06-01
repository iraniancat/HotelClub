// src/HotelReservation.Application/Contracts/Persistence/IDependentDataRepository.cs
using HotelReservation.Domain.Entities;

namespace HotelReservation.Application.Contracts.Persistence;

public interface IDependentDataRepository : IGenericRepository<DependentData>
{
    Task<IReadOnlyList<DependentData>> GetByEmployeeDataIdAsync(Guid employeeDataId);
    Task<DependentData?> GetByEmployeeDataIdAndNationalCodeAsync(Guid employeeDataId, string dependentNationalCode);
}