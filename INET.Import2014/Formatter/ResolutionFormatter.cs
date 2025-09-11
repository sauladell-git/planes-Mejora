using INET.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INET.Import2014
{
    class ResolutionFormatter:Formatter
    {
        /// <summary>
        /// Dir de resoluciones
        /// </summary>
        public string resolutionsPath;
        /// <summary>
        /// Dir de anexos
        /// </summary>
        public string anexPath;

        public ResolutionFormatter(Data data)
        {
            this.data = data;
        }

        /// <summary>
        /// Parsea las resoluciones
        /// </summary>
        /// <param name="row"></param>
        /// <param name="plan"></param>
        public bool Format(ClosedXML.Excel.IXLRow row, ImprovementPlan plan)
        {
            var valid = false;
            var dictumNumber = GetDictumNumber(row);
            int idx = dictumNumber.LastIndexOf('-');
            var expression = "(.*)" + dictumNumber.Substring(0, idx) + "_" + SchoolYearCycle.Substring(SchoolYearCycle.Length - 2) + "\\.(doc|docx|pdf)$";
            var files = FindDocumentsByRegex(resolutionsPath, expression);
            this.log(string.Format("Se encontraron {0} resoluciones", files.Count), ImportForm.LOG_INFO);

            var resoNum = row.Cell(107).Value.ToString();

            if (files.Count == 1 && !string.IsNullOrEmpty(resoNum))
            {
                var reso = new Resolution();
                reso.TemplateId = plan.ImprovementPlanTypeId == (int)INET.Core.Enums.ImprovementPlanTypeEnum.Institucional ? 33 : 34;
                reso.CreationUserId = 1;
                reso.StatusId = (int)INET.Core.Enums.ResolutionStatusEnum.Protocolizado;
                var _signatureDate = row.Cell(108).Value.ToString();
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
                reso.CreationDate = baseDate > signatureDate ? signatureDate : baseDate;
                reso.EmissionDate = baseDate > signatureDate ? signatureDate : baseDate;
                reso.SignatureDate = signatureDate;
                reso.ShipDate = signatureDate;
                reso.ProtocolizedDate = signatureDate;
                reso.ResolutionNumber = resoNum;
                reso.Body = files[0];
                reso.Annex = dictumNumber;
                var dictums = plan.Documents.OfType<Dictum>().First();
                reso.Dictums.Add(dictums);
                reso.AmountExecuted = reso.Dictums.Sum(x => x.Ammount);

                plan.Documents.Add(reso);

                valid = true;
            }
            else if (files.Count > 0)
            {
                this.log(string.Format("Ignorando, se encontraron más de una resolución ({0})", files.Count), ImportForm.LOG_ERROR);
            }

            return valid;
        }

    }
}
