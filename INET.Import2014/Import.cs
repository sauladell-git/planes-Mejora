using ClosedXML.Excel;
using INET.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace INET.Import2014
{

    /// <summary>
    /// Log delegate
    /// </summary>
    /// <param name="txt">Texto a loguear</param>
    /// <param name="level">Nivel de logueo</param>
    public delegate void LogDelegate(string txt, int level);

    class Import
    {

        /// <summary>
        /// Log delegate
        /// </summary>
        public LogDelegate log;

        /// <summary>
        /// Data context
        /// </summary>
        private INETContext context;

        /// <summary>
        /// Data object
        /// </summary>
        private Data data;

        public Import()
        { }

        /// <summary>
        /// Ejecuta importación de planes desde una planilla tomando los archivos de directorios
        /// </summary>
        /// <param name="xlsPath"></param>
        /// <param name="dictumsPath"></param>
        /// <param name="resolutionsPath"></param>
        /// <param name="anexPath"></param>
        /// <param name="startLine"></param>
        /// <param name="lineLimit"></param>
        public void Process(string xlsPath, string dictumsPath, string resolutionsPath, string anexPath, int startLine, int lineLimit)
        {
            this.log("Comenzando...", ImportForm.LOG_INFO);
            this.log("Abriendo archivo de planes: " + xlsPath, ImportForm.LOG_ALL);

            this.context = new INETContext();
            this.data = new Data(context);

            using (var workbook = new XLWorkbook(xlsPath))
            {
                var ws = workbook.Worksheet(1);
                var rows = ws.RowsUsed(false);
                this.log(String.Format("Verificando {0} filas", rows.Count()), ImportForm.LOG_ALL);

                var plans = new List<ImprovementPlan>();
                var planFormatter = new PlanFormatter(data);
                planFormatter.SchoolYearCycle = "2014";
                planFormatter.log = log;

                var dictumFormatter = new DictumFormatter(data);
                dictumFormatter.dictumsPath = dictumsPath;
                dictumFormatter.SchoolYearCycle = "2014";
                dictumFormatter.log = log;

                var resolutionFormatter = new ResolutionFormatter(data);
                resolutionFormatter.resolutionsPath = resolutionsPath;
                resolutionFormatter.anexPath = anexPath;
                resolutionFormatter.SchoolYearCycle = "2014";
                resolutionFormatter.log = log;

                var lineCount = 0;
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var row in rows)
                        {
                            if (row.RowNumber() < startLine) continue;
                            var plan = planFormatter.Format(row);
                            if (plan != null)
                            {
                                if (dictumFormatter.Format(row, plan))
                                {
                                    if (resolutionFormatter.Format(row, plan))
                                    {
                                        context.ImprovementPlans.Add(plan);
                                        context.SaveChanges();
                                        this.log("Plan guardado", ImportForm.LOG_INFO);
                                        plans.Add(plan);
                                    }
                                }
                            }
                            lineCount = lineCount + 1;
                            if (lineLimit <= lineCount) break;
                        }
                        dbContextTransaction.Commit();
                        if (plans.Count() > 0)
                        {
                            CopyDocumentFiles(plans, dictumFormatter, resolutionFormatter);
                            this.log(String.Format("{0} planes procesados", plans.Count()), ImportForm.LOG_INFO);
                        }
                    }
                    catch (Exception e)
                    {
                        this.log("ERROR: No se guardaron los datos", ImportForm.LOG_INFO);
                        this.log(e.ToString(), ImportForm.LOG_ERROR);
                        trackErrors();
                        dbContextTransaction.Rollback();
                    }
                }
            }
        }

        private void CopyDocumentFiles(List<ImprovementPlan> plans, DictumFormatter dictumFormatter, ResolutionFormatter resolutionFormatter)
        {
            // save document files
            var documents = plans.SelectMany(x => x.Documents);
            var dictumsImportedDir = dictumFormatter.dictumsPath + "\\imported";
            var resolutionsImportedDir = resolutionFormatter.resolutionsPath + "\\imported";
            var anexPath = resolutionFormatter.anexPath + "\\imported";
            if (!TestCreateDirectory(dictumsImportedDir))
            {
                this.log(String.Format("No se pudo encontrar ni crear el directorio ({0})", dictumsImportedDir), ImportForm.LOG_ERROR);
            }
            else if (!TestCreateDirectory(resolutionsImportedDir))
            {
                this.log(String.Format("No se pudo encontrar ni crear el directorio ({0})", resolutionsImportedDir), ImportForm.LOG_ERROR);
            }
            else if (!TestCreateDirectory(anexPath))
            {
                this.log(String.Format("No se pudo encontrar ni crear el directorio ({0})", anexPath), ImportForm.LOG_ERROR);
            }
            else
            {
                var copiedCount = 0;
                using (MD5 md5Hash = MD5.Create())
                {
                    foreach (var document in documents)
                    {
                        var docType = document.GetType().Name;
                        var path = (docType == "Dictum") ? dictumsImportedDir : resolutionsImportedDir;
                        var file = document.Body.Trim();
                        string hash = GetMd5Hash(md5Hash, Path.GetFileName(file));
                        var destination = path + "\\" + hash + Path.GetExtension(file);
                        try
                        {
                            File.Copy(@file, @destination, true);
                            document.PDF = Path.GetFileName(destination);
                            document.Body = "IMPORTANTE: Documento Importado. Ver adjunto para detalle completo.";
                            this.log(String.Format("Documento importado: {0}", document.PDF), ImportForm.LOG_ALL);
                            copiedCount++;
                            // add anex files
                            if (docType == "Resolution")
                            {
                                var dictumNumber = document.CastTo<Resolution>().Annex;
                                int idx = dictumNumber.LastIndexOf('-');
                                var anexExpression = "(.*)" + dictumNumber.Substring(0, idx) + "_" + resolutionFormatter.SchoolYearCycle.Substring(resolutionFormatter.SchoolYearCycle.Length - 2) + "\\.(.{3})$";
                                var anexFiles = resolutionFormatter.FindDocumentsByRegex(resolutionFormatter.anexPath, anexExpression);
                                this.log(string.Format("Se encontraron {0} anexos", anexFiles.Count), ImportForm.LOG_INFO);
                                string hashAnex = GetMd5Hash(md5Hash, dictumNumber + " anexo");
                                var destinationAnex = "";
                                docType = "Anexo";
                                if (anexFiles.Count == 1)
                                {
                                    destinationAnex = anexPath + "\\" + hash + Path.GetExtension(anexFiles[0]);
                                    File.Copy(@anexFiles[0], destinationAnex, true);
                                }
                                else if (anexFiles.Count > 1)
                                {
                                    // zip them
                                    destinationAnex = path + "\\" + hash + ".zip";
                                    using (FileStream zipToOpen = new FileStream(destinationAnex, FileMode.Create))
                                    {
                                        using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                                        {
                                            foreach (var _af in anexFiles)
                                            {
                                                archive.CreateEntryFromFile(@_af, Path.GetFileName(_af));
                                            }
                                        }
                                    }
                                }
                                if (!string.IsNullOrEmpty(destinationAnex))
                                {
                                    document.CastTo<Resolution>().Annex = Path.GetFileName(destinationAnex);
                                    this.log(String.Format("Anexo importado: {0}", document.CastTo<Resolution>().Annex), ImportForm.LOG_ALL);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            this.log(String.Format("No se pudo copiar el documento ({0}): {1}", docType, e.ToString()), ImportForm.LOG_ERROR);
                        }
                    }
                }
                if (copiedCount > 0)
                {
                    context.SaveChanges();
                }
                else
                {
                    this.log(String.Format("No se copiaron archivos de documentos"), ImportForm.LOG_ERROR);
                }
                this.log(String.Format("Documentos copiados: {0}", copiedCount), ImportForm.LOG_INFO);
            }
        }

        /// <summary>
        /// Testea si existe un directorio y si no lo crea
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected bool TestCreateDirectory(string path)
        {
            var valid = false;
            if (Directory.Exists(path))
            {
                valid = true;
            }
            else
            {
                try
                {
                    var dir = Directory.CreateDirectory(path);
                    valid = true;
                }
                catch (Exception)
                {
                    this.log(string.Format("No se pudo crear el directorio: {0}", path), ImportForm.LOG_ERROR);
                }
            }
            return valid;
        }

        protected string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        /// <summary>
        /// Track errors in context if any
        /// </summary>
        public bool trackErrors()
        {
            var errors = context.GetValidationErrors();
            foreach (var error in errors)
            {
                foreach (var _e in error.ValidationErrors)
                {
                    if (error.Entry.Entity.GetType().Name == "Dictum")
                    {
                        this.log("ERROR Dictamen " + error.Entry.CurrentValues.GetValue<String>("DictumNumber") + ": " + _e.ErrorMessage, ImportForm.LOG_ERROR);
                    }
                    else if (error.Entry.Entity.GetType().Name == "ImprovementPlan")
                    {
                        this.log("ERROR Plan " + error.Entry.CurrentValues.GetValue<String>("Identifier") + ": " + _e.ErrorMessage, ImportForm.LOG_ERROR);
                    }
                }
            }
            return errors.Count() > 0;
        }

    }
}
