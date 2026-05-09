using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.Concentrations;
using FarmaDiBusiness.Interfaces;
using FarmaDiBusiness.Services;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace WebApiPharmacy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin, Empleado")]
    public class ConcentrationController : ControllerBase
    {
        private readonly IConcentrationService _concentrationService;

        public ConcentrationController(IConcentrationService concentrationService)
        {
            _concentrationService = concentrationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _concentrationService.GetAllAsync();

            if (serviceResponse.IsSuccess)
            {
                var concentrationsDtoCollection = serviceResponse.Data.Select(c => new GetAllConcentrationDto
                {
                    ConcentrationId = c.ConcentrationId,
                    Volume = c.Volume,
                    Porcentage = c.Porcentage,
                    IsActive = c.IsActive,
                    RegisteredDate = c.RegisteredDate
                });

                var apiResponse = new ApiResponse<IEnumerable<GetAllConcentrationDto>>
                {
                    Data = concentrationsDtoCollection,
                    Meta = new
                    {
                        TotalAmount = concentrationsDtoCollection.Count(),
                        message = serviceResponse.Message
                    }
                };
                return Ok(apiResponse);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NoData:
                    unsuccessfulResponse.Code = "200";
                    unsuccessfulResponse.Message = "No se encontraron concentraciones";
                    unsuccessfulResponse.Details = new { info = "Temporalmente no hay concentraciones registradas en la BD" };
                    return Ok(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno en la aplicación" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                var response = new UnsuccessfulResponseDto()
                {
                    Code = "400",
                    Message = "Id proporcionado debe ser mayor a 0",
                    Details = new { info = "Error en el formato de valor enviado" }
                };
                return BadRequest(response);
            }

            var serviceResponse = await _concentrationService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                var concentrationDto = new GetConcentrationByIdDto
                {
                    ConcentrationId = serviceResponse.Data!.ConcentrationId,
                    Volume = serviceResponse.Data.Volume,
                    Porcentage = serviceResponse.Data.Porcentage,
                    IsActive = serviceResponse.Data.IsActive,
                    RegisteredDate = serviceResponse.Data.RegisteredDate
                };

                return Ok(concentrationDto);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = "404";
                    unsuccessfulResponse.Message = "No se encontró una concentración asociada al Id proporcionado";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "No se encontró el recurso solicitado" };
                    return NotFound(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error";
                    unsuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno no esperado" };
                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(AddConcentrationDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new UnsuccessfulResponseDto { Code = "400", Message = "Payload inválido", Details = new { info = "Cuerpo de la petición es null" } });
            }

            var entity = new Concentrations { Volume = dto.Volume, Porcentage = dto.Porcentage };
            var serviceResponse = await _concentrationService.AddAsync(entity);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var created = serviceResponse.Data;
                var createdDto = new GetConcentrationByIdDto
                {
                    ConcentrationId = created.ConcentrationId,
                    Volume = created.Volume,
                    Porcentage = created.Porcentage,
                    IsActive = created.IsActive,
                    RegisteredDate = created.RegisteredDate
                };

                return CreatedAtAction(nameof(GetById), new { id = createdDto.ConcentrationId }, createdDto);
            }

            var unSuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.DuplicateData:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "La concentración ya existe";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "No se puede duplicar la concentración" };
                    return Conflict(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    return StatusCode(500, unSuccessfulResponse);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, UpdateConcentrationDto dto)
        {
            if (id <= 0)
            {
                var response = new UnsuccessfulResponseDto()
                {
                    Code = "400",
                    Message = "Id proporcionado debe ser mayor a 0",
                    Details = new { info = "Error en el formato de valor enviado" }
                };
                return BadRequest(response);
            }

            var entity = new Concentrations { ConcentrationId = id, Volume = dto.Volume, Porcentage = dto.Porcentage, IsActive = dto.IsActive };
            var serviceResponse = await _concentrationService.UpdateAsync(id, entity);

            if (serviceResponse.IsSuccess)
            {
                var updatedConcentration = new GetConcentrationByIdDto
                {
                    ConcentrationId = serviceResponse.Data!.ConcentrationId,
                    Volume = serviceResponse.Data.Volume,
                    Porcentage = serviceResponse.Data.Porcentage,
                    IsActive = serviceResponse.Data.IsActive,
                    RegisteredDate = serviceResponse.Data.RegisteredDate
                };

                return Ok(updatedConcentration);
            }

            var unSuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontró una concentración con el Id proporcionado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                    return NotFound(unSuccessfulResponse);

                case MessageCodes.DuplicateData:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El registro no pudo guardarse por un conflicto";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Hubo conflicto en la actualización" };
                    return Conflict(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    return StatusCode(500, unSuccessfulResponse);
            }
        }

        
    }
}