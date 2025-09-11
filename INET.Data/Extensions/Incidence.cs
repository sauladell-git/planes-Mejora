using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Data
{
    public partial class Incidence
    {
        /// <summary>
        /// Texto descriptivo del estado de la incidencia
        /// </summary>
        public string Status
        {
            get
            {
                return (Active) ? "Abierta" : "Cerrado";
            }
        }
    }
}
