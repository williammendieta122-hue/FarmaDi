using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities
{
    public class Concentrations
    {
        public int ConcentrationId { get; set; }
        public string Volume { get; set; } = string.Empty; // maps to Volume (nvarchar(50))
        public string Porcentage { get; set; } = string.Empty; // maps to Porcentage (nvarchar(50))
        public DateTime RegisteredDate { get; set; }
        public bool IsActive { get; set; }

        public string ConcentrationName { get; set; }

    }
}