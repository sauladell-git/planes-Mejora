using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INET.Core.Enums
{

    public enum MessageResultTypeEnum
    {
        Error = -1,
        Confirmation = 0,
        Attention = 1
    }

    public enum StageStatusEnum
    {
        EnIngreso = 1,
        EnCargaProvincia = 31,
        AElevarProvincia = 32,
        EnComisionRecepcionProvincia = 33,
        EnCaratulizacion = 2,
        EnEvaluacion = 3,
        EnAdministracion = 30,
        Cerrado = 4,
        Anulado = 5
    }

    public enum AprobationStatusEnum    
    {
        Rechazo = 6,
        AprobacionParcial = 7,
        AprobacionTotal = 8,
        AprobacionPendiente = 9       
    }

    public enum SolicitudeStatusEnum
    {
        Pendiente = 10,
        EnProceso = 11,
        Aprobado = 12,
        Elegible = 13,
        Rechazado = 14,
        Anulado =  15    
    }

    public enum AxisStatusEnum
    {
        Archivado = 34,
        NoVigente = 35,
        Vigente = 36,
        Vigente_214_22 =37
    }

    public enum SolicitudeTypeEnum
    {
        Original = 1
    }

    public enum StatusCriterionEnum
    { 
        Stage = 1,
        Aprobation = 2,
        Solicitude = 3,
        Dictum = 4,
        Elegibility = 5,
        Resolution = 6,
        Axis = 7
    }

    public enum ImprovementPlanTypeEnum
    {
        Nacional = 1,
        Jurisdiccional = 2,
        Institucional = 3
    }

    public enum DictumTypeEnum
    { 
        Aprobacion = 1,
        Rechazo = 2,
        Elegibilidad = 3,
        Reasignacion = 4
    }

    public enum DictumStatusEnum
    { 
        Borrador = 16,
        PendienteAprobacion = 17,
        Emitido = 18,
        Firmado = 19,
        Anulado = 20
    }

    public enum ResolutionStatusEnum
    { 
        Borrador = 24,
        PendienteAprobacion = 25,
        Emitido = 26,
        Firmado = 27,
        Protocolizado = 28,
        Anulado = 29
    }

    public enum EligibilityStatusEnum
    { 
        Pendiente = 21,
        Aprobado = 22,
        Rechazado = 23
    }

    public enum TemplateTypeEnum
    { 
        Dictamen = 1,
        DictamenDeEligibilidad = 2,
        Protocolo = 3,
        Resolucion = 4
    }

    public enum ExpenditureTypeEnum
    { 
        GastoCorriente = 1,
        GastoCapital = 2
    }

    public enum ExpenditureObjectTypeEnum
    {
        Bienes_Servicios = 1,
        Viaticos         = 2,
        RRHH             = 3
    }

    public static class EnumExtensions
    {
        public static int ToInt(this Enum enumValue)
        {
            return Convert.ToInt32(enumValue);
        }

        public static List<T> ToList<T>() 
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToList<T>();
        }
    }
}