using ClosedXML.Excel;
using Excel;
using INET.Data;
using INET.Core.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INET.Import2014
{
    class DictumFormatter : Formatter
    {

        /// <summary>
        /// Dir de dictámenes
        /// </summary>
        public string dictumsPath;

        public DictumFormatter(Data data)
        {
            this.data = data;
        }

        /// <summary>
        /// Parsea los dictamenes y solicitados de un plan
        /// </summary>
        /// <param name="row"></param>
        /// <param name="plan"></param>
        public bool Format(ClosedXML.Excel.IXLRow row, ImprovementPlan plan)
        {
            var valid = false;
            var dictumNumber = GetDictumNumber(row);
            if (string.IsNullOrEmpty(dictumNumber))
            {
                this.log(string.Format("No tiene nro de dictamen: {0}", dictumNumber), ImportForm.LOG_ERROR);
            }
            else
            {
                int idx = dictumNumber.LastIndexOf('-');
                var expression = "(.*)" + dictumNumber.Substring(0, idx) + "_" + SchoolYearCycle.Substring(SchoolYearCycle.Length - 2) + "\\.(xls|xlsm|xlsx)$";
                var files = FindDocumentsByRegex(dictumsPath, expression);
                this.log(string.Format("Se encontraron {0} dictámenes", files.Count), ImportForm.LOG_ALL);
                if (files.Count == 1)
                {
                    var fileName = @files[0];
                    using (FileStream fsSource = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    {
                        IExcelDataReader excelReader;
                        if (fileName.EndsWith(".xls"))
                        {
                            excelReader = ExcelReaderFactory.CreateBinaryReader(fsSource);
                        }
                        else
                        {
                            excelReader = ExcelReaderFactory.CreateOpenXmlReader(fsSource);
                        }
                        excelReader.IsFirstRowAsColumnNames = false;
                        DataSet ds = excelReader.AsDataSet();
                        if (processDictum(ds, plan, row, fileName))
                        {
                            valid = true;
                        }
                    }
                }
                else
                {
                    this.log(string.Format("Ignorando, se encontraron más de un dictámen ({0})", files.Count), ImportForm.LOG_ERROR);
                }
                if (valid)
                {
                    this.log("Dictámen procesado", ImportForm.LOG_INFO);
                }
                else
                {
                    this.log("Error al procesar el dictámen", ImportForm.LOG_INFO);
                }
            }
            return valid;
        }

        /// <summary>
        /// Procesa la caratua del dictámen
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="plan"></param>
        /// <param name="row"></param>
        /// <param name="fileName"></param>
        /// <returns>bool</returns>
        private bool processDictum(DataSet dataSet, ImprovementPlan plan, ClosedXML.Excel.IXLRow row, string fileName)
        {
            var valid = false;
            this.log(string.Format("Dictámen con {0} hojas de datos", dataSet.Tables.Count), ImportForm.LOG_ALL);
            DataRowCollection caratulaWs = dataSet.Tables[0].Rows;
            char modeloDictum = '-';
            if (caratulaWs[3][8].ToString() == "DICTAMEN")
            {
                modeloDictum = 'A';
            }
            else if (caratulaWs[3][0].ToString() == "DICTAMEN COMPLEMENTARIO" || caratulaWs[3][0].ToString() == "DICTAMEN" || caratulaWs[3][0].ToString() == "DICTAMEN RECTIFICATORIO")
            {
                modeloDictum = 'B';
            }
            else if (caratulaWs[4][0].ToString() == "DICTAMEN COMPLEMENTARIO" || caratulaWs[4][0].ToString() == "DICTAMEN")
            {
                modeloDictum = 'C';
            }
            else
            {
                this.log("Modelo de dictámen desconocido", ImportForm.LOG_ERROR);
            }
            this.log(string.Format("Dictámen tipo: {0}", modeloDictum), ImportForm.LOG_ALL);

            if (
                setIdentifier(caratulaWs, modeloDictum, plan)
                && setField(caratulaWs, modeloDictum, plan)
            )
            {
                createDictum(dataSet, plan, row, fileName);
                if (plan.Documents.OfType<Dictum>().Count() > 0)
                {
                    valid = true;
                }
                else
                {
                    this.log("No se generó el ojeto dictámen", ImportForm.LOG_ERROR);
                }
                if (valid == false)
                {
                    this.log("Error al procesar los solicitados del dictámen", ImportForm.LOG_ERROR);
                }
            }
            else
            {
                this.log("Error al procesar la carátula dictámen", ImportForm.LOG_INFO);
            }

            return valid;
        }

        private void createDictum(DataSet dataSet, ImprovementPlan plan, IXLRow row, string fileName)
        {
            var dictum = new Dictum();
            dictum.TemplateId = plan.ImprovementPlanTypeId == (int)INET.Core.Enums.ImprovementPlanTypeEnum.Institucional ? 31 : 32;
            dictum.StatusId = (int)INET.Core.Enums.DictumStatusEnum.Firmado;
            dictum.FileNumber = GetFileNumber(row).Replace('-', '/').Trim();
            dictum.Ammount = plan.Solicitudes.Sum(x => x.ApprovedPriceUnit);
            dictum.Balance = 0;
            dictum.Locked = true;
            var _signatureDate = row.Cell(41).Value.ToString();
            var baseDate = new DateTime(2014, 12, 1);
            DateTime signatureDate;
            if (string.IsNullOrEmpty(_signatureDate))
            {
                signatureDate = baseDate;
            }
            else if (!DateTime.TryParse(_signatureDate, out signatureDate))
            {
                signatureDate = baseDate;
            }
            dictum.CreationDate = baseDate > signatureDate ? signatureDate : baseDate;
            dictum.EmissionDate = baseDate > signatureDate ? signatureDate : baseDate;
            dictum.SignatureDate = signatureDate;
            dictum.CreationUserId = 1;
            dictum.Body = fileName;
            dictum.DictumNumber = GetDictumNumber(row);
            dictum.EligibilityRequired = false;
            var solis = plan.Solicitudes;
            foreach (var sol in solis) dictum.Solicitudes.Add(sol);
            plan.Documents.Add(dictum);
        }

        /// <summary>
        /// Extrae el Identificador
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="modeloDictum"></param>
        /// <param name="plan"></param>
        /// <returns>bool</returns>
        private bool setIdentifier(DataRowCollection ws, char modeloDictum, ImprovementPlan plan)
        {
            var valid = true;
            string value = null;
            if (modeloDictum == 'A')
            {
                value = ws[13][3].ToString().Replace('_', '-');
            }
            else if (modeloDictum == 'B')
            {
                value = ws[10][2].ToString() + "-" + ws[10][4].ToString() + "-" + ws[10][6].ToString() + "-" + ws[10][8].ToString();
            }
            else if (modeloDictum == 'C')
            {
                value = ws[11][2].ToString() + "-" + ws[11][4].ToString() + "-" + ws[11][6].ToString() + "-" + ws[11][8].ToString();
            }

            if (string.IsNullOrEmpty(value))
            {
                this.log(string.Format("Identificador de plan en dictámen ausente: {0}", value), ImportForm.LOG_ERROR);
                valid = false;
            }
            else
            {
                if (!string.IsNullOrEmpty(plan.Identifier))
                {
                    if (value != plan.Identifier)
                    {
                        this.log(string.Format("No coinciden el dictámen del listado ({0}) con el dictámen indicado en el arhivo ({1})", plan.Identifier, value), ImportForm.LOG_ERROR);
                        return false;
                    }
                }
                else
                {
                    plan.Identifier = value;
                }
            }
            return valid;
        }

        /// <summary>
        /// Extrae el Eje
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="modeloDictum"></param>
        /// <param name="plan"></param>
        /// <returns>bool</returns>
        private bool setField(DataRowCollection ws, char modeloDictum, ImprovementPlan plan)
        {
            var valid = true;
            string value = null;
            string fieldCode = null;
            if (modeloDictum == 'A')
            {
                value = ws[5][1].ToString();
                this.log(string.Format("Eje de plan en dictámen: {0}", value), ImportForm.LOG_ALL);
                var part = value.Split('-');
                if (part.Count() > 0)
                {
                    fieldCode = part[0].Trim().Replace("EJE PROGRAMÁTICO N°", "").Trim().ToUpper();
                }
            }
            else if (modeloDictum == 'B')
            {
                value = ws[7][1].ToString();
                this.log(string.Format("Eje de plan en dictámen: {0}", value), ImportForm.LOG_ALL);
                var part = value.Split(' ');
                if (part.Count() > 0)
                {
                    fieldCode = part[0].Trim().ToUpper();
                }
            }
            else if (modeloDictum == 'C')
            {
                value = ws[8][1].ToString();
                this.log(string.Format("Eje de plan en dictámen: {0}", value), ImportForm.LOG_ALL);
                var part = value.Split(' ');
                if (part.Count() > 0)
                {
                    fieldCode = part[0].Trim().ToUpper();
                }
            }

            if (string.IsNullOrEmpty(value))
            {
                this.log(string.Format("Eje de plan en dictámen ausente: {0}", value), ImportForm.LOG_ERROR);
                valid = false;
            }
            else
            {
                var search = data.GetFields().Where(x => x.StatusId == (int) AxisStatusEnum.Vigente && x.Code.Equals(fieldCode)).FirstOrDefault();
                if (search != null)
                {
                    plan.FieldId = search.Id;
                }
                else
                {
                    this.log(string.Format("Eje de plan en inválido en dictámen: {0}", value), ImportForm.LOG_ERROR);
                    valid = false;
                }
            }
            return valid;
        }
    }
}
