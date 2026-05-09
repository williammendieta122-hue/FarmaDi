using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.InventoryLossDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using FarmaDiDataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Services
{
    public class InventoryLossService: IInventoryLossService
    {
        private readonly IInventoryLossRepository _InventoryLossRepository;
        private readonly IProductBatchesRepository _ProductBatchesRepository;
        private readonly IStockRepository _StockRepository;
        public InventoryLossService(IInventoryLossRepository InventoryLossRepository, IProductBatchesRepository productBatchesRepository, IStockRepository stockRepository)
        {
            
            _InventoryLossRepository = InventoryLossRepository;
            _ProductBatchesRepository = productBatchesRepository;
            _StockRepository = stockRepository;
        }


        public async Task<ServiceResponse<IEnumerable<InventoryLoss>>> GetAllAsync()
        {
            var result = await _InventoryLossRepository.GetAllAsync();

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<IEnumerable<InventoryLoss>>()
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operacion exitosa"
                };


            }
            switch (result.OperationStatusCode)
            {
                case 50009:
                    return new ServiceResponse<IEnumerable<InventoryLoss>>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };


                default:
                    return new ServiceResponse<IEnumerable<InventoryLoss>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NoData,
                        Message = "Ocurrió un error inesperado"
                    };

            }

        }


        public async Task<ServiceResponse<InventoryLoss>> GetByIdAsync(int id)
        {
            var result = await _InventoryLossRepository.GetByIdAsync(id);
            try
            {
                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<InventoryLoss>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = result.Message ?? "Operación exitosa"
                    };
                }
                switch (result.OperationStatusCode)
                {
                    case 50009: // Ejemplo: código para no encontrado
                        return new ServiceResponse<InventoryLoss>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "La baja no existe"
                        };

                    case 50007: 
                        return new ServiceResponse<InventoryLoss>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.Conflict,
                            Message = "Cantidad inferior a la requerida"
                        };
                    default:
                        return new ServiceResponse<InventoryLoss>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = result.Message ?? "Error inesperado"
                        };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<InventoryLoss >
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = result.Message ?? "Ocurrió un error inesperado"

                };
            }
        }


        public async Task<ServiceResponse<InventoryLoss>> AddAsync(AddInventoryLossDto newInventoryLoss)
        {

            try
            {
                /*
                var existing = await _InventoryLossRepository.GetByNameAsync(newbrand.BrandName);

                if (existing.Data!.BrandId != 0 && !existing.Data.BrandName.IsNullOrEmpty())
                {
                    return new ServiceResponse<InventoryLoss>
                    {
                        Data = null,
                        IsSuccess = false, ///.//
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "Existe un registro con el nombre proporcionado"

                    };

                }*/
             
                // validar que exista el lote 
                var existBatch = await _ProductBatchesRepository.GetByIdAsync( newInventoryLoss.BatchId);
                if (existBatch.Data == null)
                {
                    return new ServiceResponse<InventoryLoss>
                    {
                        Data = null,
                        IsSuccess = false, ///.//
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = $"No se encontró un lote con el id { newInventoryLoss.BatchId}"

                    };



                }

                // validar que exista la cantidad necesaria para poder registar la operación

                var existQuantity = await _StockRepository.GetByIdAsync(newInventoryLoss.Quantity);
                if (existQuantity.Data.AvailableQuantity < newInventoryLoss.Quantity)
                {
                    return new ServiceResponse<InventoryLoss>
                    {
                        Data = null,
                        IsSuccess = false, ///.//
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "No hay la cantidad necesaria para poder registar la baja"

                    };

                }

                var inventoryLoss = new InventoryLoss
                {

                    BatchId = newInventoryLoss.BatchId,
                    Quantity = newInventoryLoss.Quantity,
                    ProductId = newInventoryLoss.ProductId,
                    UserId = newInventoryLoss.UserId,
                    Reason = newInventoryLoss.Reason,

                };

                var result = await _InventoryLossRepository.AddAsync(inventoryLoss);

                return new ServiceResponse<InventoryLoss>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Baja registrada correctamente"
                };



            }
            catch (Exception)
            {
                return new ServiceResponse<InventoryLoss>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado",
                };
            }


        }







       

    }
}
