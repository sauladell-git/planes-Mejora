using INET.Core.Enums;
using INET.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace INET.Import2014
{
    class PlanFormatter:Formatter
    {

        private int dummySolId = 0;

        public PlanFormatter(Data data)
        {
            this.data = data;
        }

        /// <summary>
        /// Formatear un row a un plan
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public ImprovementPlan Format(ClosedXML.Excel.IXLRow row)
        {
            var plan = new ImprovementPlan();

            plan.StatusId = (int)INET.Core.Enums.StageStatusEnum.Cerrado;
            plan.EvaluatorUserId = 1;

            this.log(string.Format("Procesando fila {0}: {1}", row.RowNumber(), plan.Identifier),  ImportForm.LOG_INFO);
            if (
                setSchoolYear(row, SchoolYearCycle, plan)
                && parseReceptionDate(row, plan)
                && parsePlanType(row, plan)
                && parseCueData(row, plan)
                && parseFieldDate(row, plan)
                && parseSolicitudes(row, plan))
            {
                this.log(string.Format("Plan procesado"), ImportForm.LOG_INFO);
                plan.Summary = "Importado año 2014";
                return plan;
            }
            else
            {
                this.log(string.Format("Plan ignorado"), ImportForm.LOG_INFO);
                return null;
            }
        }

        /// <summary>
        /// Parse Field Date
        /// </summary>
        /// <param name="row"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        private bool parseFieldDate(ClosedXML.Excel.IXLRow row, ImprovementPlan plan)
        {
            var valid = true;
            var value = row.Cell(40).Value.ToString();
            if (string.IsNullOrEmpty(value))
            {
                value = "01/01/2014";
            }
            DateTime d;
            if (DateTime.TryParse(value, out d))
            {
                plan.FieldDate = d;
            }
            else
            {
                this.log(string.Format("Fecha de ingreso inválida: {0}", value), ImportForm.LOG_ERROR);
                valid = false;
            }
            return valid;
        }

        /// <summary>
        /// Parse Reception Date
        /// </summary>
        /// <param name="row"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        private bool parseReceptionDate(ClosedXML.Excel.IXLRow row, ImprovementPlan plan)
        {
            var valid = true;
            var value = row.Cell(2).Value.ToString();
            DateTime d;
            if (DateTime.TryParse(value, out d))
            {
                plan.ReceptionDate = d;
            }
            else
            {
                this.log(string.Format("Fecha de recepción inválida: {0}", value), ImportForm.LOG_ERROR);
                valid = false;
            }
            return valid;
        }

        /// <summary>
        /// Parse plan identifier
        /// </summary>
        /// <param name="row"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        private bool parseIdentifier(ClosedXML.Excel.IXLRow row, ImprovementPlan plan)
        {
            var valid = true;
            var value = row.Cell(8).Value.ToString().Trim().ToUpper();
            if (value != null)
            {
                plan.Identifier = value;
            }
            else
            {
                this.log(string.Format("Código de plan inválido: {0}", value), ImportForm.LOG_ERROR);
                valid = false;
            }
            return valid;
        }

        /// <summary>
        /// Parse plan type
        /// </summary>
        /// <param name="row"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        private bool parsePlanType(ClosedXML.Excel.IXLRow row, ImprovementPlan plan)
        {
            var valid = true;
            var value = row.Cell(3).Value.ToString().Trim().ToLower();
            var planType = data.GetImprovementPlanTypes().Where(x => x.Description.ToLower().Equals(value)).FirstOrDefault();
            if (planType != null)
            {
                plan.ImprovementPlanTypeId = planType.Id;
            }
            else
            {
                this.log(string.Format("Tipo de plan inválida: {0}", value), ImportForm.LOG_ERROR);
                valid = false;
            }
            return valid;
        }

        /// <summary>
        /// Parse CUE
        /// </summary>
        /// <param name="row"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        private bool parseCueData(ClosedXML.Excel.IXLRow row, ImprovementPlan plan)
        {
            Province province;
            string value;
            bool valid = true;
            if (plan.ImprovementPlanTypeId == (int)INET.Core.Enums.ImprovementPlanTypeEnum.Institucional)
            {
                value = row.Cell(4).Value.ToString().Trim().ToLower();
                plan.CUE = value;
                var cueData = data.GetInstitutionData(plan.CUE);
                if (cueData != null)
                {
                    plan.InstitutionName = cueData.Name;
                    plan.Location = cueData.Locality;
                    plan.Department = cueData.Department;
                }
                else
                {
                    this.log(string.Format("CUE inválido según WS de registro: {0}", value), ImportForm.LOG_ERROR);
                }
            }
            else if (plan.ImprovementPlanTypeId == (int)INET.Core.Enums.ImprovementPlanTypeEnum.Jurisdiccional)
            {
                value = row.Cell(1).Value.ToString().Trim().ToLower();
                province = data.GetProvinces().Where(x => x.Name.ToLower().Equals(value)).FirstOrDefault();
                if (province != null)
                {
                    plan.CUE = province.Number.PadRight(9, '0');
                }
                else
                {
                    this.log(string.Format("No se puede inferir la provincia: {0}", value), ImportForm.LOG_ERROR);
                    valid = false;
                }
            }
            else
            {
                this.log(string.Format("No puede validarse un CUE sin un tipo de plan válido"), ImportForm.LOG_ERROR);
                valid = false;
            }
            return valid;
        }

        /// <summary>
        /// Resuelve el año escolar
        /// </summary>
        /// <param name="row"></param>
        /// <param name="SchoolYearCycle"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        private bool setSchoolYear(ClosedXML.Excel.IXLRow row, string SchoolYearCycle, ImprovementPlan plan)
        {
            var valid = true;
            var schoolYear = data.GetSchoolYearsList().Where(x => x.Cycle == SchoolYearCycle).FirstOrDefault();
            if (schoolYear != null)
            {
                plan.SchoolYearId = schoolYear.Id;
            }
            else
            {
                this.log(string.Format("Ciclo escolar no válido"), ImportForm.LOG_ERROR);
                valid = false;
            }

            return valid;
        }

        /// <summary>
        /// Importar y crear solicitados para el plan desde el listado de planes
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="plan"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private bool parseSolicitudes(ClosedXML.Excel.IXLRow row, ImprovementPlan plan)
        {
            this.log(string.Format("Importando solicitados desde el plan"), ImportForm.LOG_ALL);
            List<Solicitude> list = new List<Solicitude>();

            var offset = 0;
            // # Eje 1
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "A" && x.Field.Code == "I").First().Id, (int)ExpenditureTypeEnum.GastoCorriente, offset++, row);
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "B" && x.Field.Code == "I").First().Id, (int)ExpenditureTypeEnum.GastoCorriente, offset++, row);
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "C" && x.Field.Code == "I").First().Id, (int)ExpenditureTypeEnum.GastoCorriente, offset++, row);
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "D" && x.Field.Code == "I").First().Id, (int)ExpenditureTypeEnum.GastoCorriente, offset++, row);
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "E" && x.Field.Code == "I").First().Id, (int)ExpenditureTypeEnum.GastoCorriente, offset++, row);
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "F" && x.Field.Code == "I").First().Id, (int)ExpenditureTypeEnum.GastoCorriente, offset++, row);
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "G" && x.Field.Code == "I").First().Id, (int)ExpenditureTypeEnum.GastoCapital, offset++, row);
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "H" && x.Field.Code == "I").First().Id, (int)ExpenditureTypeEnum.GastoCapital, offset++, row);
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "I" && x.Field.Code == "I").First().Id, (int)ExpenditureTypeEnum.GastoCapital, offset++, row);
            // # Eje 2
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "A" && x.Field.Code == "II").First().Id, (int)ExpenditureTypeEnum.GastoCorriente, offset++, row);
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "B" && x.Field.Code == "II").First().Id, (int)ExpenditureTypeEnum.GastoCorriente, offset++, row);
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "C" && x.Field.Code == "II").First().Id, (int)ExpenditureTypeEnum.GastoCorriente, offset++, row);
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "D" && x.Field.Code == "II").First().Id, (int)ExpenditureTypeEnum.GastoCorriente, offset++, row);
            // # Eje 3
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "A" && x.Field.Code == "III").First().Id, (int)ExpenditureTypeEnum.GastoCapital, offset++, row);
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "B" && x.Field.Code == "III").First().Id, (int)ExpenditureTypeEnum.GastoCapital, offset++, row);
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "C" && x.Field.Code == "III").First().Id, (int)ExpenditureTypeEnum.GastoCapital, offset++, row);
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "D" && x.Field.Code == "III").First().Id, (int)ExpenditureTypeEnum.GastoCapital, offset++, row);
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "E" && x.Field.Code == "III").First().Id, (int)ExpenditureTypeEnum.GastoCapital, offset++, row);
            // # Eje 4
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "A" && x.Field.Code == "IV").First().Id, (int)ExpenditureTypeEnum.GastoCapital, offset++, row);
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "B" && x.Field.Code == "IV").First().Id, (int)ExpenditureTypeEnum.GastoCorriente, offset++, row);
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "C" && x.Field.Code == "IV").First().Id, (int)ExpenditureTypeEnum.GastoCapital, offset++, row);
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "D" && x.Field.Code == "IV").First().Id, (int)ExpenditureTypeEnum.GastoCorriente, offset++, row);
            addSolicitudeToPlan(plan, data.GetLinesList().Where(x => x.Code == "E" && x.Field.Code == "IV").First().Id, (int)ExpenditureTypeEnum.GastoCapital, offset++, row);

            this.log(string.Format("Total Solicitados ({0}): {1}", plan.Solicitudes.Count, plan.Solicitudes.Sum(x => x.ApprovedPriceUnit).ToString()), ImportForm.LOG_INFO);

            return plan.Solicitudes.Count > 0;
        }

        private void addSolicitudeToPlan(ImprovementPlan plan, int lineId, int expeditureTypeId, int offset, ClosedXML.Excel.IXLRow row)
        {
            string requested = row.Cell(offset + 9).Value.ToString();
            string approved_ni = row.Cell(offset * 2 + 43).Value.ToString();
            string approved_i = row.Cell(offset * 2 + 44).Value.ToString();
            string fileNumber = GetFileNumber(row);
            if (!string.IsNullOrEmpty(requested) || !string.IsNullOrEmpty(approved_ni) || !string.IsNullOrEmpty(approved_i))
            {
                this.log(string.Format("Linea: {0}, Pedido: {1}, Aprobado NI: {2}, Aprobado I: {3}, Expediente: {4}", lineId, requested, approved_ni, approved_i, fileNumber), ImportForm.LOG_ALL);
            }
            decimal _requested = 0;
            decimal _approved_ni = 0;
            decimal _approved_i = 0;
            if (!string.IsNullOrEmpty(requested))
            {
                _requested = Decimal.Parse(requested);
            }
            if (!string.IsNullOrEmpty(approved_ni))
            {
                _approved_ni = Decimal.Parse(approved_ni);
            }
            if (!string.IsNullOrEmpty(approved_i))
            {
                _approved_i = Decimal.Parse(approved_i);
            }

            if ((_approved_i > 0 || _approved_ni > 0) && _requested > 0 && !string.IsNullOrEmpty(fileNumber))
            {
                var sol = new Solicitude();
                sol.SchoolYearId = plan.SchoolYearId;
                sol.SolicitudeTypeId = (int)INET.Core.Enums.SolicitudeTypeEnum.Original;
                sol.ExpenditureTypeId = expeditureTypeId;
                sol.StatusId = (int)INET.Core.Enums.SolicitudeStatusEnum.Aprobado;
                sol.LineId = lineId;
                sol.CUE = plan.CUE;
                sol.Details = "Solicitado generico plan importado 2014";
                sol.FileNumber = fileNumber;
                sol.Locked = true;
                sol.Specialization = "Todas";
                sol.RequestedAmount = 1;
                sol.RequestedPriceUnit = _requested;
                sol.ApprovedAmount = 1;
                sol.ApprovedPriceUnit = _approved_i + _approved_ni;
                sol.MeasurementUnitId = 80;
                sol.Id = dummySolId++;
                plan.Solicitudes.Add(sol);
            }
        }

    }
}
