using FarmaDiBusiness.DTOs.SaleDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Services
{
    public class SaleService : ISaleService
    {
        private readonly ISalesRepository _salesRepository;
        private readonly IProductService _productService; // Para validar productos
        // private readonly IPaymentMethodService _paymentMethodService; 

        public SaleService(ISalesRepository salesRepository, IProductService productService)
        {
            _salesRepository = salesRepository;
            _productService = productService;
        }

        public async Task<ServiceResponse<SaleResponseDto>> InsertAsync(CreateSaleDto dto)
        {
            var response = new ServiceResponse<SaleResponseDto>();

            try
            {
                // 1. VALIDACIONES PREVIAS
                // Validar que los productos existan para evitar conflictos de FK (Error 547)
                foreach (var detail in dto.Details)
                {
                    var productCheck = await _productService.GetByIdAsync(detail.ProductId);
                    if (!productCheck.IsSuccess || productCheck.Data == null)
                    {
                        return new ServiceResponse<SaleResponseDto>
                        {
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorValidation,
                            Message = $"El producto con ID {detail.ProductId} no existe o no está activo."
                        };
                    }
                }

                // 2. MAPEO DE ENTRADA 

                // Maestro (Invoice)
                var invoiceMaster = new Invoice
                {
                    UserId = dto.UserId,
                    ClientName = dto.ClientName,
                    Discount = dto.Discount,
                    RegisteredDate = DateTime.Now, 
                };

                // Detalles (SaleDetail)
                var saleDetails = dto.Details.Select(d => new SaleDetails
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                   
                }).ToList();

                // 3. LLAMADA A LA CAPA DE DATOS
                var repoResponse = await _salesRepository.InsertAsync(invoiceMaster, saleDetails, dto.PaymentMethodId);

                // 4. VERIFICAR RESPUESTA DEL REPOSITORIO
                if (repoResponse.OperationStatusCode != 0)
                {
                    // Manejo de errores específicos del SP (ej. Stock Insuficiente - 51001)
                    var msgCode = MessageCodes.ErrorDataBase;
                    if (repoResponse.OperationStatusCode == 51001) msgCode = MessageCodes.ErrorValidation; // O un código específico de Stock

                    return new ServiceResponse<SaleResponseDto>
                    {
                        IsSuccess = false,
                        MessageCode = msgCode,
                        Message = repoResponse.Message
                    };
                }

                // 5. MAPEO DE SALIDA (Entidades -> SaleResponseDto)
                // Aquí transformamos los datos complejos de la BD en el JSON bonito para imprimir.

                var resultDto = new SaleResponseDto
                {
                    InvoiceId = repoResponse.Data.InvoiceMaster.InvoiceId,
                    UserId = repoResponse.Data.InvoiceMaster.UserId,
                    ClientName = repoResponse.Data.InvoiceMaster.ClientName,
                    RegisteredDate = repoResponse.Data.InvoiceMaster.RegisteredDate,
                    SubTotal = repoResponse.Data.InvoiceMaster.SubTotal,
                    Discount = repoResponse.Data.InvoiceMaster.Discount,
                    Total = repoResponse.Data.InvoiceMaster.Total,
                    IsPrinted = repoResponse.Data.InvoiceMaster.IsPrinted,
                    PaymentMethodId = dto.PaymentMethodId, 

                    Details = repoResponse.Data.InvoiceDetails.Select(d => new SalesDetailsResponseDto
                    {
                        ProductId = d.ProductId,
                        ProductTradeName = d.ProductTradeName,
                        ProductGenericName = d.ProductGenericName,

                        Quantity = d.Quantity,
                        UnitPrice = d.UnitPrice,
                        TotalPrice = d.TotalPrice
                    }).ToList()
                };

                response.Data = resultDto;
                response.IsSuccess = true;
                response.MessageCode = MessageCodes.Success;
                response.Message = "Venta registrada correctamente.";
            }
            catch (Exception ex)
            {
                // Loguear el error real ex.Message
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorDataBase;
                response.Message = "Ocurrió un error inesperado al procesar la venta.";
            }

            return response;
        }




        public async Task<ServiceResponse<IEnumerable<InvoiceQueueDto>>> GetPrintQueueAsync()
        {
           
            var repoResponse = await _salesRepository.GetPendingInvoicesAsync();
            if (repoResponse.OperationStatusCode != 0)
            {
                return new ServiceResponse<IEnumerable<InvoiceQueueDto>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message
                };
            }

            var dtoList = repoResponse.Data.Select(invoice =>
            {
                //Calculamos el tiempo de espera 
                // el tiempo actual - el tiempo de la factura 
               
                var diff = DateTime.Now - invoice.RegisteredDate;
                string timeFriendly = diff.TotalMinutes < 60
                    ? $"Hace {diff.Minutes} min"
                    : $"Hace {Math.Round(diff.TotalHours, 1)} horas";

                return new InvoiceQueueDto
                {
                    InvoiceId = invoice.InvoiceId,
                    ClientName = invoice.ClientName,
                    RegisteredDate = invoice.RegisteredDate,
                    Total = invoice.Total,
                    WaitTime = timeFriendly 
                };
            }).ToList(); 

          
            return new ServiceResponse<IEnumerable<InvoiceQueueDto>>
            {
                Data = dtoList, 
                IsSuccess = true,
                MessageCode = MessageCodes.Success
            };
        }






        public async Task<ServiceResponse<SaleResponseDto>> GetByIdAsync(int id)
        {
            var repoResponse = await _salesRepository.GetInvoiceByIdAsync(id);

            if (repoResponse.OperationStatusCode != 0)
            {
                // Manejo si no encuentra la factura
                return new ServiceResponse<SaleResponseDto>
                {
                    IsSuccess = false,
                    MessageCode = repoResponse.OperationStatusCode == 404 ? MessageCodes.NotFound : MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message
                };
            }

            // MAPEO: Convertimos Entidades a DTO para la vista
            var invoice = repoResponse.Data.InvoiceMaster;
            var details = repoResponse.Data.InvoiceDetails;

            var resultDto = new SaleResponseDto
            {
                InvoiceId = invoice.InvoiceId,
                UserId = invoice.UserId,
                ClientName = invoice.ClientName,
                RegisteredDate = invoice.RegisteredDate,
                SubTotal = invoice.SubTotal,
                Discount = invoice.Discount, 
                Total = invoice.Total,
                IsPrinted = invoice.IsPrinted,
                // PaymentMethodId =

                Details = details.Select(d => new SalesDetailsResponseDto
                {
                    ProductId = d.ProductId,
                    ProductTradeName = d.ProductTradeName,     
                    ProductGenericName = d.ProductGenericName, 
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,    
                    TotalPrice = d.TotalPrice  
                }).ToList()
            };

            return new ServiceResponse<SaleResponseDto>
            {
                Data = resultDto,
                IsSuccess = true,
                MessageCode = MessageCodes.Success
            };
        }


        public async Task<ServiceResponse<bool>> ConfirmPrintAsync(int invoiceId)
        {
            var result = await _salesRepository.ConfirmPrintAsync(invoiceId);

            if (result)
            {
                return new ServiceResponse<bool> {
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Data = true,
                    Message = "Factura marcada como impresa" 
                };
            }

            return new ServiceResponse<bool> { 
                IsSuccess = false, 
                MessageCode = MessageCodes.ErrorDataBase,
                Data = false,
                Message = "No se pudo actualizar el estado" 
            };
        }
    }
}