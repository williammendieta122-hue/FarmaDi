using FarmaDiBusiness.DTOs.SaleDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Empleado, Admin")]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _saleService;

        public SalesController(ISaleService saleService)
        {
            _saleService = saleService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SaleResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] CreateSaleDto dto)
        {
            var response = await _saleService.InsertAsync(dto);

            if (response.IsSuccess)
            {
                // Retornamos 201 Created
               
                return CreatedAtAction(nameof(GetById), new { id = response.Data.InvoiceId }, response.Data);
            }

            // Manejo de Errores
            switch (response.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    
                    return BadRequest(new {
                        message = response.Message });

                case MessageCodes.ErrorDataBase:
                default:
                
                    return StatusCode(500, new {
                         
                        message = response.Message });
            }
        }


        [HttpGet("print-queue")]
        public async Task<IActionResult> GetPrintQueue()
        {
            var response = await _saleService.GetPrintQueueAsync();

            if (response.IsSuccess)
            {
                
                return Ok(response.Data);
            }

          
            return StatusCode(500, new { message = response.Message });
        }


        
        /*    [HttpGet("{id}")]
            public IActionResult GetById(int id)
            {
                return Ok($"Detalle de venta {id} (Pendiente de implementar)");
            }
        */









        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _saleService.GetByIdAsync(id);

            if (response.IsSuccess)
            {
                return Ok(response.Data);
            }

            if (response.MessageCode == MessageCodes.NotFound)
                return NotFound(new { message = response.Message });

            return StatusCode(500, new { message = response.Message });
        }

        [HttpPut("confirm-print/{id}")]
        public async Task<IActionResult> ConfirmPrint(int id)
        {
            var response = await _saleService.ConfirmPrintAsync(id);

            if (response.IsSuccess) return Ok(response);

            return BadRequest(response.Message);
        }
    }
}