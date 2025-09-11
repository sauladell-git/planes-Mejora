using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace INET.Import2014
{
    class Formatter
    {
        /// <summary>
        /// Log delegate
        /// </summary>
        public LogDelegate log;

        /// <summary>
        /// Data object
        /// </summary>
        public Data data;

        /// <summary>
        /// Ciclo escolar de los planes
        /// </summary>
        public string SchoolYearCycle;

        /// <summary>
        /// Buscar path de documentos en un directorio segun una extensión
        /// </summary>
        /// <param name="document_path">Directorio donde buscar</param>
        /// <param name="expression">Fragmento de expresion regular para validar las extensiones</param>
        /// <returns>Lista de path de documentos</returns>
        internal List<string> FindDocumentsByRegex(string document_path, string expression)
        {
            var documents = new List<string>();
            this.log("Buscando: " + expression, ImportForm.LOG_ALL);
            IEnumerable<string> files = Directory.EnumerateFiles(@document_path).Where(name => Regex.IsMatch(name, expression));
            foreach (var file in files)
            {
                this.log(file, ImportForm.LOG_ALL);
                documents.Add(file);
            }
            return documents;
        }

        /// <summary>
        /// Get dictum number
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        protected string GetDictumNumber(ClosedXML.Excel.IXLRow row)
        {
            var dictumNumber = row.Cell(42).Value.ToString().Trim().ToUpper();
            return dictumNumber;
        }

        /// <summary>
        /// Get File Number
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        protected string GetFileNumber(ClosedXML.Excel.IXLRow row)
        {
            string fileNumber = row.Cell(39).Value.ToString();
            return fileNumber;
        }

    }
}
