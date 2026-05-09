using FarmaDiBusiness.DTOs;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IConcentrationService
    {
        Task<ServiceResponse<IEnumerable<Concentrations>>> GetAllAsync();
        Task<ServiceResponse<Concentrations>> GetByIdAsync(int id);
        Task<ServiceResponse<Concentrations>> AddAsync(Concentrations concentration);
        Task<ServiceResponse<Concentrations>> UpdateAsync(int id, Concentrations concentration);
        Task<ServiceResponse<Concentrations>> GetByCodeAsync(string code);
    }
}
