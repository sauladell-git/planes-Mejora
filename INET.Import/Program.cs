using INET.Data;
using INET.Services;
using System;
using System.Configuration;

namespace INET.Import
{
    class Program
    {
        static void Main(string[] args)
        {
            // Inicializamos log4net
            log4net.Config.XmlConfigurator.Configure();

            var inputPath = ConfigurationManager.AppSettings.Get("InputFolderPath");

            // Inicializo consola
            Console.Clear();
            Console.WriteLine("-------------------------------");
            Console.WriteLine("IMPORTACIÓN DE PLANES DE MEJORA");
            Console.WriteLine("-------------------------------");
            Console.WriteLine("");
            Console.WriteLine("Se importarán los archivos Excels existentes en la siguiente carpeta:");
            Console.WriteLine(inputPath);
            Console.WriteLine("");

            if (CanStartProcess())
            {
                Console.WriteLine("");
                Console.WriteLine("Iniciando...");

                using (var db = new INETContext())
                {
                    var service = new ImportService(db, new FileNumberService(db));
                    var result = service.Import(inputPath);
                    Console.WriteLine("Finalizando...");

                    var message = (result.Errors == 1)
                        ? "Se procesaron {0} archivo(s) con {1} error"
                        : "Se procesaron {0} archivo(s) con {1} errores";

                    Console.WriteLine("");
                    Console.WriteLine(string.Format(message, result.Processed, result.Errors));
                }

                Console.ReadKey();
            }            
        }

        static bool CanStartProcess()
        {
            ConsoleKey response; 
            do
            {
                while (Console.KeyAvailable) 
                    Console.ReadKey();

                Console.Write("¿Desea continuar? S/N: ");
                response = Console.ReadKey().Key;
                Console.WriteLine();
            } while (response != ConsoleKey.S && response != ConsoleKey.N); 

            return response == ConsoleKey.S;
        }
    }
}
