using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FarmaDiBusiness.DTOs.Concentrations
{
    public class UpdateConcentrationDto
    {
        public string Volume { get; set; }
        public string Porcentage { get; set; }
        public bool IsActive { get; set; }
    }
}
