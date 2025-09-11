using System;
using System.Linq;
using INET.Data;

namespace INET.Services 
{
    /// <summary>
    /// Servicio de numeros de carpetas
    /// </summary>
    public class FileNumberService : BusinessService
    {
        public FileNumberService(INETContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Devuelve el próximo FileNumber (nro. de carpeta universal) a utilizar.
        /// </summary>
        /// <returns>El próximo FileNumber a utilizar</returns>
        public string LastFileNumber()
        {
            var fileNumber = Context.FileNumbers.FirstOrDefault();
            if (fileNumber == null)
            {
                fileNumber = new FileNumber();
                fileNumber.LastFileNumber = "0";
                Context.FileNumbers.Add(fileNumber);
            }

            fileNumber.LastFileNumber = (Convert.ToInt16(fileNumber.LastFileNumber) + 1).ToString().PadLeft(6, '0');
            
            return fileNumber.LastFileNumber;
        }
    }
}
