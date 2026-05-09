using FarmaDiBusiness.DTOs.SaleDto;
using FarmaDiCore.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface ISaleService
    {
        Task<ServiceResponse<SaleResponseDto>> InsertAsync(CreateSaleDto dto);
        Task<ServiceResponse<IEnumerable<InvoiceQueueDto>>> GetPrintQueueAsync();

        Task<ServiceResponse<SaleResponseDto>> GetByIdAsync(int id);

        Task<ServiceResponse<bool>> ConfirmPrintAsync(int invoiceId);
    }
}
