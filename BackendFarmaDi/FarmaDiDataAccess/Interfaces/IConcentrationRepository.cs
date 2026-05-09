using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace FarmaDiDataAccess.Interfaces
{
    public interface IConcentrationsRepository
    {
        Task<RepositoryResponse<IEnumerable<Concentrations>>> GetAllAsync();
        Task<RepositoryResponse<Concentrations>> GetByIdAsync(int id);
        Task<RepositoryResponse<Concentrations>> AddAsync(Concentrations concentrations);
        Task<RepositoryResponse<Concentrations>> UpdateAsync(int id, Concentrations concentrations);
        Task<RepositoryResponse<Concentrations>> GetByCodeAsync(string code);
    }
}
