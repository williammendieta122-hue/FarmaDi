using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IProductPricingService
    {
        Task<ServiceResponse<IEnumerable<Inventory>>> GetAllAsync();
        Task<ServiceResponse<Inventory>> GetByIdAsync(int id);
    }
}
