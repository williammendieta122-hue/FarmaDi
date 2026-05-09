using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.PurchaseDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    
    [Route("api/[controller]")]
    /* [ApiController]
     [Authorize(Roles = "Admin, Empleado")]
     */
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;
        public PurchaseController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
         
            return Ok($"Obteniendo compra {id}");
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] CreatePurchaseDto dto)
        {
          
            var serviceResponse = await _purchaseService.InsertAsync(dto);
            if (serviceResponse.IsSuccess)
            {
              
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = serviceResponse.Data!.Id },
                    serviceResponse.Data
                );
            }

          
            var UnSuccessFulresponse = new UnsuccessfulResponseDto();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    UnSuccessFulresponse.Code = "400";
                    UnSuccessFulresponse.Message = "Ocurrió un error en la validación de datos";
                    UnSuccessFulresponse.Details = new { info = serviceResponse.Message };
                    return BadRequest(UnSuccessFulresponse);

               

                default:
                    UnSuccessFulresponse.Code = "500";
                    UnSuccessFulresponse.Message = "Ocurrió un error inesperado";
                    UnSuccessFulresponse.Details = new { info = serviceResponse.Message };
                    return StatusCode(500, UnSuccessFulresponse);
            }
        }
    }
}