using System;
using System.Linq;
using INET.Core.Interfaces;
using INET.Core.Enums;

namespace INET.Data
{
    public partial class Solicitude : IAuditable
    {
        /// <summary>
        /// Total solicitado
        /// </summary>
        public decimal RequestedTotal 
        {
            get 
            {
                return Math.Round(((RequestedAmount.HasValue ? RequestedAmount.Value : 0) * (RequestedPriceUnit.HasValue ? RequestedPriceUnit.Value : 0)), 2);
            }
        }

        /// <summary>
        /// Total aprobado para el solicitado
        /// </summary>
        public decimal? ApprovedTotal
        {
            get
            {
                if (ApprovedAmount.HasValue && ApprovedPriceUnit.HasValue)
                    return Math.Round((ApprovedAmount.Value * ApprovedPriceUnit.Value), 2);

                return null;
            }
        }

        /// <summary>
        /// Codigo de especialización del solicitado
        /// </summary>
        public string SpecializationCode
        {
            get 
            {
                if (!String.IsNullOrWhiteSpace(Specialization))
                    return Specialization.Substring(0, Specialization.IndexOf("-")).Trim();

                return null;
            }
        }

        /// <summary>
        /// Total disponible del solicitado para ser utilizado en reasignaciones
        /// </summary>
        public decimal AvailableTotal
        {
            get
            {
                return ((ApprovedTotal.HasValue) ? ApprovedTotal.Value : RequestedTotal) - (ReassignedGrantedTotal.HasValue ? ReassignedGrantedTotal.Value : 0);
            }
        }
    }
}