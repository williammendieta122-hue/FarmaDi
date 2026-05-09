using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using FarmaDiDataAccess.Repositories;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Services
{
    public class ConcentrationService : IConcentrationService
    {
        private readonly IConcentrationsRepository _concentrationRepository;

        public ConcentrationService(IConcentrationsRepository concentrationRepository)
        {
            _concentrationRepository = concentrationRepository;
        }

        public async Task<ServiceResponse<IEnumerable<Concentrations>>> GetAllAsync()
        {
            var result = await _concentrationRepository.GetAllAsync();

            if (result.OperationStatusCode == 50008)
            {
                return new ServiceResponse<IEnumerable<Concentrations>>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operación exitosa"
                };
            }

            switch (result.OperationStatusCode)
            {
                case 50009:
                    return new ServiceResponse<IEnumerable<Concentrations>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron concentraciones"
                    };
                default:
                    return new ServiceResponse<IEnumerable<Concentrations>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = "Ocurrió un error inesperado"
                    };
            }
        }

        public async Task<ServiceResponse<Concentrations>> GetByIdAsync(int id)
        {
            var result = await _concentrationRepository.GetByIdAsync(id);
            if (result.OperationStatusCode == 50008)
            {
                return new ServiceResponse<Concentrations>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operación exitosa"
                };
            }

            switch (result.OperationStatusCode)
            {
                case 50009:
                    return new ServiceResponse<Concentrations>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "La concentración no existe"
                    };
                default:
                    return new ServiceResponse<Concentrations>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = "Ocurrió un error inesperado al obtener la concentración"
                    };
            }
        }

        public async Task<ServiceResponse<Concentrations>> AddAsync(Concentrations concentration)
        {
            var result = await _concentrationRepository.AddAsync(concentration);
            if (result.OperationStatusCode == 50008)
            {
                return new ServiceResponse<Concentrations>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Concentración registrada correctamente"
                };
            }

            switch (result.OperationStatusCode)
            {
                case 50005:
                    return new ServiceResponse<Concentrations>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.DuplicateData,
                        Message = "Ya existe una concentración con este código"
                    };

                default:
                    return new ServiceResponse<Concentrations>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = "Ocurrió un error inesperado al registrar la concentración"
                    };
            }
        }

        public async Task<ServiceResponse<Concentrations>> UpdateAsync(int id, Concentrations concentration)
        {
            var result = await _concentrationRepository.UpdateAsync(id, concentration);
            if (result.OperationStatusCode == 50008)
            {
                return new ServiceResponse<Concentrations>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Concentración actualizada correctamente"
                };
            }

            switch (result.OperationStatusCode)
            {
                case 50009:
                    return new ServiceResponse<Concentrations>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "La concentración no existe"
                    };
                case 50005:
                    return new ServiceResponse<Concentrations>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.DuplicateData,
                        Message = "Ya existe otra concentración con este código"
                    };
                default:
                    return new ServiceResponse<Concentrations>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = "Ocurrió un error inesperado al actualizar la concentración"
                    };
            }
        }

        public async Task<ServiceResponse<Concentrations>> GetByCodeAsync(string code)
        {
            var result = await _concentrationRepository.GetByCodeAsync(code);
            if (result.OperationStatusCode == 50008)
            {
                return new ServiceResponse<Concentrations>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operación exitosa"
                };
            }

            switch (result.OperationStatusCode)
            {
                case 50009:
                    return new ServiceResponse<Concentrations>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No se encontró la concentración por código"
                    };
                default:
                    return new ServiceResponse<Concentrations>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = "Ocurrió un error inesperado"
                    };
            }
        }
    }
}
