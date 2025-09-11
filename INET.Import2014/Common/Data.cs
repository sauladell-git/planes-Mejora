using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INET.Data;
using INET.Services.DTO;

namespace INET.Import2014
{
    class Data
    {
        /// <summary>
        /// Data context
        /// </summary>
        private INETContext context;

        private List<ImprovementPlansType> ImprovementTypesList;
        private List<Province> ProvincesList;
        private List<SchoolYear> SchoolYearsList;
        private List<Field> FieldsList;
        private List<Line> LinesList;
        private List<ExpenditureType> ExpenditureTypesList;
        private List<MeasurementUnit> MeasurementUnitsList;
        

        public Data(INETContext db)
        {
            this.context = db;
        }

        /// <summary>
        /// Lista de tipos de planes
        /// </summary>
        /// <returns></returns>
        public List<ImprovementPlansType>  GetImprovementPlanTypes()
        {
            if (ImprovementTypesList == null)
            {
                ImprovementTypesList = context.ImprovementPlansTypes.ToList();
            }
            return ImprovementTypesList;
        }

        /// <summary>
        /// Lista de provincias
        /// </summary>
        /// <returns></returns>
        public List<Province> GetProvinces()
        {
            if (ProvincesList == null)
            {
                ProvincesList = context.Provinces.ToList();
            }
            return ProvincesList;
        }

        /// <summary>
        /// Obtiene datos de la institución a traves de un Web Service del ministerio
        /// </summary>
        /// <param name="cue">CUE de la institución de la cual se desea obtener info</param>
        /// <returns>Devuelve un institutionDTO con la info de la institución perteneciente al CUE.</returns>
        public InstitutionDTO GetInstitutionData(string cue)
        {
            var service = new INET.Services.wsINET.ConsultaRegistro();
            System.Net.ServicePointManager.Expect100Continue = false;
            var data = service.obtenerDatosInstitucion(cue.Trim());

            if (data.cue == "0")
                return null;

            var dto = new InstitutionDTO();
            dto.CUE = data.cue;
            dto.Name = data.nombre;
            dto.Province = data.provincia;
            dto.Department = data.departamento;
            dto.Locality = data.localidad;
            dto.Orientation = data.orientacion;
            dto.Type = data.tipo;
            dto.Active = data.activo;
            dto.Ambit = data.ambito;
            dto.Specializations = data.especializaciones;
            dto.Dependence = data.dependencia;

            return dto;
        }

        /// <summary>
        /// Lista de ciclos escolares
        /// </summary>
        /// <returns></returns>
        public List<SchoolYear> GetSchoolYearsList()
        {
            if (SchoolYearsList == null)
            {
                SchoolYearsList = context.SchoolYears.ToList();
            }
            return SchoolYearsList;
        }

        /// <summary>
        /// Lista de Ejes
        /// </summary>
        /// <returns></returns>
        public List<Field> GetFields()
        {
            if (FieldsList == null)
            {
                FieldsList = context.Fields.ToList();
            }
            return FieldsList;
        }

        /// <summary>
        /// Lista de lineas
        /// </summary>
        /// <returns></returns>
        public List<Line> GetLinesList()
        {
            if (LinesList == null)
            {
                LinesList = context.Lines.ToList();
            }
            return LinesList;
        }

        /// <summary>
        /// Lista de tipos de gastos
        /// </summary>
        /// <returns></returns>
        public List<ExpenditureType> GetExpenditureTypesList()
        {
            if (ExpenditureTypesList == null)
            {
                ExpenditureTypesList = context.ExpenditureTypes.ToList();
            }
            return ExpenditureTypesList;
        }

        /// <summary>
        /// Lista de unidades de medida
        /// </summary>
        /// <returns></returns>
        public List<MeasurementUnit> GetMeasurementUnitsList()
        {
            if (MeasurementUnitsList == null)
            {
                MeasurementUnitsList = context.MeasurementUnits.ToList();
            }
            return MeasurementUnitsList;
        }
    }
}
