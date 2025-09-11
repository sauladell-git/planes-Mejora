using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Word = DocumentFormat.OpenXml.Wordprocessing;
using INET.Core.Enums;
using INET.Data;
using INET.Services.DTO;
using INET.Utils.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ClosedXML.Excel;

namespace INET.Services
{
    /// <summary>
    /// Servicio de plantillas de documentos
    /// </summary>
    public class TemplateService : BusinessService
    {
        public TemplateService(INETContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Obtiene un template por medio de su Id
        /// </summary>
        /// <param name="id">Id de la plantilla que se desea obtener</param>
        /// <returns>El template correspondiente al id pasado como parámetro</returns>
        public Template Get(int id)
        {
            return Context.Templates.Where(x => x.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// Lista todos los templates existentes
        /// </summary>
        /// <returns>Una lista con todas las plantillas existentes</returns>
        public IList<Template> List()
        {
            return Context.Templates.ToList();
        }

        /// <summary>
        /// Determina si es posible eliminar un template
        /// </summary>
        /// <param name="templateId">Id del template que se desea eliminar</param>
        /// <returns>True en casa de que se pueda eliminar la plantilla. False en caso contrario.</returns>
        public bool CanDeleteTemplate(int templateId)
        {
            return Context.Documents.Where(x => x.TemplateId == templateId).Count() == 0;
        }

        /// <summary>
        /// Elimina una plantilla
        /// </summary>
        /// <param name="templateId">Id de la plantilla que se desea eliminar</param>
        /// <returns>True en caso de que se haya eliminado la plantilla. False en caso contrario.</returns>
        public bool DeleteTemplate(int templateId)
        {
            if (CanDeleteTemplate(templateId))
            {
                var template = Context.Templates.Where(x => x.Id == templateId).First();
                Context.Templates.Remove(template);
                Context.SaveChanges();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Genera una vista previa de un documento
        /// </summary>
        /// <param name="documentId">Id del documento que se desea previsualizar</param>
        /// <param name="content">Contenido HTML a previsualizar</param>
        /// <param name="variables">Variables del documento</param>
        /// <param name="application_path">Path base de la aplicación, viene del request que lo consume para encontrar los css</param>
        /// <param name="format"></param>
        /// <returns>Un memory stream del documento parseado con una vista previa de como quedaría el documento</returns>
        public Byte[] Preview(int documentId, string content, List<DocumentVariableDTO> variables, string application_path, string format = "pdf")
        {
            var document = Context.Documents.Where(x => x.Id == documentId).First();
            var body = document.Template.Content.Replace("[CUERPO]", content);
            var documentVariables = variables.Select(x => new DocumentVariable() { TemplateVariableId = x.TemplateVariableId, Text = x.VariableText }).ToList();
            var preview = Parse(body, document, documentVariables);
            if (format == "pdf")
            {
                return GetContentPDF(preview, application_path + "/content/tinyMCE-custom.css", null, document.Template.Id);
            } else
            {
                return GetContentDocx(preview, null, document.Template.Id, "");
            }
        }

        /// <summary>
        /// Obtiene un PDF en base a un contenido HTML.
        /// </summary>
        /// <param name="html">HTML del contenido del cual se desea generar el PDF</param>
        /// <param name="linkCss">Link a la hoja de estilo con la que se debe parsear el css</param>
        /// <param name="documentId">Id del documento para el que se genera el archivo. Si es nulo, 
        /// el mismo no se salvará en el dico. En caso de que no sea nulo, se guardará en el disco 
        /// y se  actualizará el documento con el path en el que se guardó.
        /// </param>
        /// <param name="templateId">Template de documento a generar</param>
        /// <returns></returns>
        public Byte[] GetContentPDF(string html, string linkCss, int? documentId, int templateId)
        {
            return GetContentPDF(html, linkCss, documentId, templateId, null);
        }

        /// <summary>
        /// Obtiene un PDF en base a un contenido HTML. Sobre escribe el encabezado propio del template usado
        /// </summary>
        /// <param name="html">HTML del contenido del cual se desea generar el PDF</param>
        /// <param name="linkCss">Link a la hoja de estilo con la que se debe parsear el css</param>
        /// <param name="documentId">Id del documento para el que se genera el archivo. Si es nulo, 
        /// el mismo no se salvará en el dico. En caso de que no sea nulo, se guardará en el disco 
        /// y se  actualizará el documento con el path en el que se guardó.
        /// </param>
        /// <param name="templateId">Template de documento a generar</param>
        /// <param name="htmlHeader">HTML del encabezado de las páginas del PDF</param>
        /// <returns>Un MemoryStream correspondiente al PDF generado</returns>
        public Byte[] GetContentPDF(string html, string linkCss, int? documentId, int templateId, string htmlHeader)
        {
            // Obtengo la configuración de la página deseada para el PDF
            var pageSize = ConfigurationManager.AppSettings.Get("PDF.PageSize");

            // Buscamos la plantilla que estamos usando
            var templateType = Context.Templates.Where(x => x.Id == templateId).First();
            htmlHeader = !String.IsNullOrEmpty(htmlHeader) ? htmlHeader : templateType.Header;

            var marginLeft = iTextSharp.text.Utilities.MillimetersToPoints((float)templateType.marginLeft.Value);
            var marginRight = iTextSharp.text.Utilities.MillimetersToPoints((float)templateType.marginRight.Value);
            var marginTop = iTextSharp.text.Utilities.MillimetersToPoints((float)templateType.marginTop.Value);
            var marginBottom = iTextSharp.text.Utilities.MillimetersToPoints((float)templateType.marginBottom.Value);

            // Agregado de parseo de firmas si es un documento con ID (cuando se genera el PDF y se guarda, eso es que no se modifica más)
            if (documentId.HasValue)
            {
                html = ParseDocumentSignatures(html);
            }

            // agregamos el encabezado al html
            if (!String.IsNullOrEmpty(htmlHeader))
            {
                html = @"<table class='inet_mainTable'><thead><tr><th>" + htmlHeader + "</th></tr></thead><tbody><tr><td>" + html + "</td></tr></tbody></table>";
            }

            html = FormatImageLinks(html);

            // http://stackoverflow.com/questions/25164257/how-to-convert-html-to-pdf-using-itextsharp        
            Byte[] bytes;

            //Generamos el PDF para una página tamaño A4
            //Create a stream that we can write to, in this case a MemoryStream
            var ms = new MemoryStream();

            //Create an iTextSharp Document which is an abstraction of a PDF but **NOT** a PDF
            using (var doc = new iTextSharp.text.Document(PageSize.GetRectangle(pageSize), marginLeft, marginRight, marginTop, marginBottom))
            {

                //Create a writer that's bound to our PDF abstraction and our stream
                using (var writer = PdfWriter.GetInstance(doc, ms))
                {

                    //Open the document for writing
                    doc.Open();

                    //In order to read CSS as a string we need to switch to a different constructor
                    //that takes Streams instead of TextReaders.
                    //Below we convert the strings into UTF8 byte array and wrap those in MemoryStreams
                    using (var msCss = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(iTextSharp.text.Utilities.ReadFileToString(HttpContext.Current.Server.MapPath(linkCss)))))
                    {
                        using (var msHtml = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html)))
                        {
                            //Parse the HTML
                            iTextSharp.tool.xml.XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, msHtml, msCss);
                        }
                    }

                    doc.Close();
                }

                //After all of the PDF "stuff" above is done and closed but **before** we
                //close the MemoryStream, grab all of the active bytes from the stream
                bytes = ms.ToArray();

                if (templateType.TemplateTypeId == TemplateTypeEnum.Dictamen.ToInt() || templateType.TemplateTypeId == TemplateTypeEnum.DictamenDeEligibilidad.ToInt())
                {
                    bytes = AddPageNumbers(bytes);
                }
            }

            return bytes;
        }

        /// <summary>
        /// Add page numbers to pdf data
        /// </summary>
        /// <param name="pdf"></param>
        /// <returns></returns>
        public byte[] AddPageNumbers(byte[] pdf)
        {
            MemoryStream ms = new MemoryStream();
            // we create a reader for a certain document
            PdfReader reader = new PdfReader(pdf);
            // we retrieve the total number of pages
            int n = reader.NumberOfPages;
            // we retrieve the size of the first page
            Rectangle psize = reader.GetPageSize(1);

            // step 1: creation of a document-object
            iTextSharp.text.Document document = new iTextSharp.text.Document(psize, 50, 50, 10, 10);
            // step 2: we create a writer that listens to the document
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            // step 3: we open the document

            document.Open();
            // step 4: we add content
            PdfContentByte cb = writer.DirectContent;

            int pN = 0;
            for (int page = 1; page <= reader.NumberOfPages; page++)
            {
                document.NewPage();
                pN++;
                PdfImportedPage importedPage = writer.GetImportedPage(reader, page);
                cb.AddTemplate(importedPage, 0, 0);
                BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                cb.BeginText();
                cb.SetFontAndSize(bf, 10);
                cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, pN.ToString(), importedPage.Width / 2, 15, 0);
                cb.EndText();
            }
            document.Close();
            return ms.ToArray();
        }

        /// <summary>
        /// Guarda el MemoryStream correspondiente a un PDF en el disco y actualiza el documento con el path correspondiente
        /// </summary>
        /// <param name="bytes">Datos del PDF a guardar</param>
        /// <param name="documentId">Id del documento para el cual se ha generado el documento</param>
        public void SaveDocument(byte[] bytes, int documentId, string extension)
        {
            var filePath = ConfigurationManager.AppSettings.Get("PDF.SaveDocsPath");
            var fileName = DateTime.Now.Ticks.ToString() + extension;

            if (!filePath.EndsWith("\\"))
                filePath = filePath + "\\";

            filePath = String.Concat(filePath, fileName);

            using (FileStream fileStream = File.Create(filePath, (int)bytes.Length))
            {
                // Guardo el archivo en el disco
                fileStream.Write(bytes, 0, bytes.Length);
            }

            // Actualizo el documento con el Path del documento
            var document = Context.Documents.Where(x => x.Id == documentId).First();
            document.PDF = fileName;

            // Obtengo el ID del documento
            var identifier = "";
            if (document is Dictum)
                identifier = ((Dictum)document).DictumNumber;

            if (document is Resolution)
                identifier = ((Resolution)document).Number.ToString();

            BeginAuditLog();
            Context.SaveChanges();
            if (document is Dictum)
            {
                EndAuditLog(document.ImprovementPlanId.Value, identifier);
            }
            if (document is Resolution)
            {
                var plans = Context.Documents.OfType<Resolution>().Where(x => x.Id == documentId).First().Dictums.Select(x => x.ImprovementPlanId.Value);
                foreach (var planId in plans)
                {
                    EndAuditLog(planId, identifier);
                }
            }

        }

        /// <summary>
        /// Guarda el MemoryStream correspondiente a un PDF en el disco y actualiza el documento con el path correspondiente
        /// </summary>
        /// <param name="bytes">Datos del PDF a guardar</param>
        /// <param name="documentId">Id del documento para el cual se ha generado el documento</param>
        public void SaveAnnex(byte[] bytes, int documentId, string extension)
        {
            var filePath = ConfigurationManager.AppSettings.Get("PDF.SaveDocsPath");
            var fileName = DateTime.Now.Ticks.ToString() + extension;

            if (!filePath.EndsWith("\\"))
                filePath = filePath + "\\";

            filePath = String.Concat(filePath, fileName);

            using (FileStream fileStream = File.Create(filePath, (int)bytes.Length))
            {
                // Guardo el archivo en el disco
                fileStream.Write(bytes, 0, bytes.Length);
            }

            // Actualizo el documento con el Path del documento
            var document = Context.Documents.Where(x => x.Id == documentId).First();
            document.PDF_Annex = fileName;

            Context.SaveChanges();
        }

        /// <summary>
        /// Guarda el MemoryStream correspondiente a un PDF en el disco y actualiza el documento con el path correspondiente
        /// </summary>
        /// <param name="bytes">Datos del PDF a guardar</param>
        /// <param name="documentId">Id del documento para el cual se ha generado el documento</param>
        public void SaveAnnexExtra(byte[] bytes, int documentId, string extension)
        {
            var filePath = ConfigurationManager.AppSettings.Get("PDF.SaveDocsPath");
            var fileName = DateTime.Now.Ticks.ToString() + extension;

            if (!filePath.EndsWith("\\"))
                filePath = filePath + "\\";

            filePath = String.Concat(filePath, fileName);

            using (FileStream fileStream = File.Create(filePath, (int)bytes.Length))
            {
                // Guardo el archivo en el disco
                fileStream.Write(bytes, 0, bytes.Length);
            }

            // Actualizo el documento con el Path del documento
            var document = Context.Documents.Where(x => x.Id == documentId).First();
            document.PDF_Annex_extra = fileName;

            Context.SaveChanges();
        }

        /// <summary>
        /// Obtiene un Docx en base a un contenido HTML. Sobre escribe el encabezado propio del template usado
        /// </summary>
        /// <param name="html">HTML del contenido del cual se desea generar el PDF</param>
        /// <param name="documentId">Id del documento para el que se genera el archivo. Si es nulo, 
        /// el mismo no se salvará en el dico. En caso de que no sea nulo, se guardará en el disco 
        /// y se  actualizará el documento con el path en el que se guardó.
        /// </param>
        /// <param name="templateId">Template de documento a generar</param>
        /// <param name="htmlHeader">HTML del encabezado de las páginas del PDF</param>
        /// <returns>Un MemoryStream correspondiente al Docx generado</returns>
        public Byte[] GetContentDocx(string html, int? documentId, int templateId, string htmlHeader)
        {
            //// Buscamos la plantilla que estamos usando
            var templateType = Context.Templates.Where(x => x.Id == templateId).First();

            var ms = new MemoryStream();
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document))
            {
                // Add a main document part. 
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();

                // Create the document structure
                mainPart.Document = new Word.Document();
                Word.Body body = mainPart.Document.AppendChild(new Word.Body());

                // Doc properties setup
                Word.SectionProperties SecProperties = new Word.SectionProperties();

                // https://startbigthinksmall.wordpress.com/2010/01/04/points-inches-and-emus-measuring-units-in-office-open-xml/
                var pageSize = new Word.PageSize
                {
                    Width = UInt32.Parse(ConvertMM2Twt(ConfigurationManager.AppSettings.Get("Docx.PageSize.width")).ToString()),
                    Height = UInt32.Parse(ConvertMM2Twt(ConfigurationManager.AppSettings.Get("Docx.PageSize.height")).ToString()),
                    Code = 9,
                    Orient = Word.PageOrientationValues.Portrait
                };
                Word.PageMargin pageMargin = new Word.PageMargin
                {
                    Top = ConvertMM2Twt(templateType.marginTop.Value.ToString()),
                    Bottom = ConvertMM2Twt(templateType.marginBottom.Value.ToString()),
                    Right = UInt32.Parse(ConvertMM2Twt(templateType.marginRight.Value.ToString()).ToString()),
                    Left = UInt32.Parse(ConvertMM2Twt(templateType.marginLeft.Value.ToString()).ToString())
                };

                SecProperties.Append(pageSize);
                SecProperties.Append(pageMargin);
                body.Append(SecProperties);

                // Doc Header
                if (!String.IsNullOrEmpty(htmlHeader))
                {
                    AddDocxHtmlHeader(htmlHeader, mainPart);
                }
                else if (!String.IsNullOrEmpty(templateType.Header))
                {
                    AddDocxHtmlHeader(templateType.Header, mainPart);
                }
                // END

                // Doc Body

                if (documentId.HasValue)
                {
                    html = ParseDocumentSignatures(html);
                }

                AddDocxHtmlPart(html, mainPart);
                // END

                wordDocument.Close();
            }

            return ms.ToArray();
        }

        /// <summary>
        /// Adds a alt chunk to the main document and returns it to add it to the sub part using it.
        /// </summary>
        /// <param name="htmlString"></param>
        /// <param name="mainPart"></param>
        /// <returns></returns>
        private Word.AltChunk AddDocxHtmlPart(string htmlString, MainDocumentPart mainPart)
        {
            // https://github.com/OfficeDev/office-content/blob/master/en-us/OpenXMLCon/articles/d57e9b7d-b271-4c8d-998f-b7ca3eb6c850.md
            // https://stackoverflow.com/questions/18089921/add-html-string-to-openxml-docx-document

            string altChunkId = "Body_" + DateTime.Now.Ticks;
            altChunkId = altChunkId.Replace(" ", "_");
            var defaultStyle = ConfigurationManager.AppSettings.Get("Docx.DefaultStyle");
            htmlString = FormatImageLinks(htmlString);
            htmlString = $"<html><head></head><body style='{defaultStyle}'>{htmlString}</body></html>";
            MemoryStream ms = new MemoryStream(new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes(htmlString)).ToArray());

            AlternativeFormatImportPart formatImportPart = mainPart.AddAlternativeFormatImportPart(AlternativeFormatImportPartType.Html, altChunkId);
            formatImportPart.FeedData(ms);
            Word.AltChunk altChunk = new Word.AltChunk();
            altChunk.Id = altChunkId;

            mainPart.Document.Body.Append(altChunk);

            return altChunk;
        }

        /// <summary>
        /// Adds a alt chunk to the main document and returns it to add it to the sub part using it.
        /// </summary>
        /// <param name="htmlString"></param>
        /// <param name="mainPart"></param>
        /// <returns></returns>
        private Word.AltChunk AddDocxHtmlHeader(string htmlString, MainDocumentPart mainPart)
        {
            // https://github.com/OfficeDev/office-content/blob/master/en-us/OpenXMLCon/articles/d57e9b7d-b271-4c8d-998f-b7ca3eb6c850.md

            var header = mainPart.AddNewPart<HeaderPart>();

            string altChunkId = "Head_" + DateTime.Now.Ticks;
            altChunkId = altChunkId.Replace(" ", "_");
            var defaultStyle = ConfigurationManager.AppSettings.Get("Docx.DefaultStyle");

            htmlString = FormatImageLinks(htmlString);
            htmlString = $"<html><head></head><body style='{defaultStyle}'>{htmlString}</body></html>";
            MemoryStream ms = new MemoryStream(new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes(htmlString)).ToArray());

            AlternativeFormatImportPart formatImportPart = header.AddAlternativeFormatImportPart(AlternativeFormatImportPartType.Html, altChunkId);
            formatImportPart.FeedData(ms);
            Word.AltChunk altChunk = new Word.AltChunk();
            altChunk.Id = altChunkId;

            header.Header = new Word.Header();
            var paragraph = header.Header.AppendChild(new Word.Paragraph());
            paragraph.Parent.ReplaceChild(altChunk, paragraph);

            string rId = mainPart.GetIdOfPart(header);
            IEnumerable<Word.SectionProperties> sectPrs = mainPart.Document.Body.Elements<Word.SectionProperties>();
            foreach (var sectPr in sectPrs)
            {
                sectPr.PrependChild<Word.HeaderReference>(new Word.HeaderReference() { Id = rId });
            }

            return altChunk;
        }

        /// <summary>
        /// Parse and convert MM measure to Twentieths of a point
        /// </summary>
        /// <param name="mm"></param>
        /// <returns></returns>
        private Int32 ConvertMM2Twt(string mm)
        {
            return Int32.Parse(Math.Ceiling(((Int32.Parse(mm) / 25.4) * 72) * 20).ToString());
        }

        /// <summary>
        /// Add parse signatures in the document
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string ParseDocumentSignatures(string html)
        {
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("SignaturePath_1")))
                html = html.Replace("[FIRMA_1]", string.Format("<img src='{0}' alt='firma' class='signature'/>", ConfigurationManager.AppSettings.Get("SignaturePath_1")));
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("SignaturePath_2")))
                html = html.Replace("[FIRMA_2]", string.Format("<img src='{0}' alt='firma' class='signature'/>", ConfigurationManager.AppSettings.Get("SignaturePath_2")));
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("SignaturePath_3")))
                html = html.Replace("[FIRMA_3]", string.Format("<img src='{0}' alt='firma' class='signature'/>", ConfigurationManager.AppSettings.Get("SignaturePath_3")));
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("SignaturePath_4")))
                html = html.Replace("[FIRMA_4]", string.Format("<img src='{0}' alt='firma' class='signature'/>", ConfigurationManager.AppSettings.Get("SignaturePath_4")));
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("SignaturePath_5")))
                html = html.Replace("[FIRMA_5]", string.Format("<img src='{0}' alt='firma' class='signature'/>", ConfigurationManager.AppSettings.Get("SignaturePath_5")));
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("SignaturePath_6")))
                html = html.Replace("[FIRMA_6]", string.Format("<img src='{0}' alt='firma' class='signature'/>", ConfigurationManager.AppSettings.Get("SignaturePath_6")));
            return html;
        }

        /// <summary>
        /// Formatea los path relativos de las imagenes de un string HTML para poder incluirlos en el PDF
        /// </summary>
        /// <param name="input">Input codificado en HTML en el cual se desea reemplazar los path relativos de las imagenes por path absolutos</param>
        /// <param name="embed">Do embend base64 images or not</param>
        /// <returns>El código HTML con los links de las imágenes formateados</returns>
        private static string FormatImageLinks(string input, bool embed = false)
        {
            if (input == null)
                return string.Empty;
            string tempInput = input;
            const string pattern = @"<img(.|\n)+?>";
            HttpContext context = HttpContext.Current;

            //Change the relative URL's to absolute URL's for an image, if any in the HTML code.
            foreach (Match m in Regex.Matches(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.RightToLeft))
            {
                if (m.Success)
                {
                    string tempM = m.Value;
                    string pattern1 = "src=[\'|\"](.+?)[\'|\"]";
                    Regex reImg = new Regex(pattern1, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    Match mImg = reImg.Match(m.Value);

                    if (mImg.Success)
                    {
                        string src;
                        if (embed == true)
                        {
                            // do embed image
                            var filePath = System.AppDomain.CurrentDomain.BaseDirectory;
                            src = filePath + "\\" + mImg.Value.ToLower().Replace("src=", "").Replace("\"", "").Replace("\'", "");

                            FileStream fs = new FileStream(src, FileMode.Open, FileAccess.Read);
                            byte[] filebytes = new byte[fs.Length];
                            fs.Read(filebytes, 0, Convert.ToInt32(fs.Length));
                            src = "src=\"data:image/png;base64," + Convert.ToBase64String(filebytes, Base64FormattingOptions.None) + "\"";
                        }
                        else
                        {
                            src = mImg.Value.ToLower().Replace("src=", "").Replace("\"", "").Replace("\'", "");
                            if (!src.StartsWith("/"))
                                src = src.Insert(0, "/");

                            if (!src.StartsWith("http://") && !src.StartsWith("https://"))
                            {
                                var scheme = context.Request.Url.Scheme;
                                scheme = "http"; // always use non ssl content

                                var imageUrl = scheme + "://" + context.Request.Url.Authority + context.Request.ApplicationPath + src;
                                src = "src=\"" + imageUrl + "\"";
                            }
                        }
                        if (!String.IsNullOrEmpty(src))
                        {
                            try
                            {
                                tempM = tempM.Remove(mImg.Index, mImg.Length);
                                tempM = tempM.Insert(mImg.Index, src);

                                //insert new url img tag in whole html code
                                tempInput = tempInput.Remove(m.Index, m.Length);
                                tempInput = tempInput.Insert(m.Index, tempM);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                }
            }
            return tempInput;
        }

        /// <summary>
        /// Parsea una plantilla y reemplaza las palabras claves por ejes del plan de mejora
        /// </summary>
        /// <param name="content">Contenido HTML que se desea parsear para reemplazar las palabras clave</param>
        /// <param name="document">Documento</param>
        /// <param name="documentVariables">Variables dinámicas del documento</param>
        /// <returns>El contenido de la plantilla parseado con los valores correspondientes</returns>
        public string Parse(string content, INET.Data.Document document, List<DocumentVariable> documentVariables)
        {
            switch ((TemplateTypeEnum)document.Template.TemplateTypeId)
            {
                case TemplateTypeEnum.Dictamen:
                    content = ParseDictumFields(document as Dictum, content);
                    content = ParseDictumBlocks(document as Dictum, content);
                    break;

                case TemplateTypeEnum.DictamenDeEligibilidad:
                    content = ParseDictumFields(document as Dictum, content);
                    content = ParseDictumBlocks(document as Dictum, content);
                    break;
                case TemplateTypeEnum.Protocolo:
                    break;
                case TemplateTypeEnum.Resolucion:
                    content = ParseResolutionFields(document as Resolution, content);
                    content = ParseResolutionVariables(document as Resolution, content, documentVariables);
                    content = ParseResolutionBlocks(document as Resolution, content);
                    break;
                default:
                    break;
            }

            return content;
        }

        /// <summary>
        /// Parsea un dictamen con todos sus keywords correspondientes
        /// </summary>     
        /// <param name="dictum">Dictamen con el que se debe parsear el contenido</param>
        /// <param name="content">Contenido a parsear</param>
        /// <returns>El contenido parseado con las keywords reemplazadas</returns>
        private string ParseDictumFields(Dictum dictum, string content)
        {
            var templateTypeFields = Context.TemplateTypeFields.Where(x => x.TemplateTypeId == (int)TemplateTypeEnum.Dictamen).ToList();
            var plan = dictum.ImprovementPlan;
            var institution = GetInstitutionData(plan.CUE);
            foreach (var ttf in templateTypeFields)
            {
                switch (ttf.Field)
                {
                    case "[FECHA_EMISION]":
                        content = content.Replace("[FECHA_EMISION]", DateTime.Now.ToString("dd/MM/yyyy"));
                        break;

                    case "[CICLO_LECTIVO]":
                        content = content.Replace("[CICLO_LECTIVO]", plan.SchoolYear.Description.ToString());
                        break;

                    case "[AÑO_RECEPCION]":
                        content = content.Replace("[A&Ntilde;O_RECEPCION]", plan.ReceptionDate.Year.ToString());
                        break;

                    case "[COD_EJE]":
                        content = content.Replace("[COD_EJE]", plan.Field.Code);
                        break;

                    case "[EJE]":
                        content = content.Replace("[EJE]", plan.Field.Description);
                        break;

                    case "[COD_PROV]":
                        var province = Context.Provinces.Where(x => x.Number == plan.CUE.Substring(0, 2)).FirstOrDefault();
                        if (province != null)
                            content = content.Replace("[COD_PROV]", province.Code);
                        break;

                    case "[JURISDICCION]":
                        //content = (institution != null)
                        //    ? content.Replace("[JURISDICCION]", string.Format("{0}, {1}, {2}", institution.Locality, institution.Department, institution.Province))
                        //    : content.Replace("[JURISDICCION]", GetProvinceByNumber(plan.CUE.Substring(0, 2)).Name);
                        content = content.Replace("[JURISDICCION]", GetProvinceByNumber(plan.CUE.Substring(0, 2)).Name);
                        break;

                    case "[CUE]":
                        content = content.Replace("[CUE]", plan.CUE);
                        break;

                    case "[NOMBRE_INSTITUCION]":
                        if (institution != null)
                            content = content.Replace("[NOMBRE_INSTITUCION]", institution.Name);
                        break;

                    case "[NRO_EXPEDIENTE]":
                        content = content.Replace("[NRO_EXPEDIENTE]", dictum.Solicitudes.First().FileNumber);
                        break;

                    case "[NRO_DICTAMEN]":
                        content = content.Replace("[NRO_DICTAMEN]", dictum.DictumNumber.ToString());
                        break;

                    case "[NRO]":
                        content = content.Replace("[NRO]", dictum.Number.ToString());
                        break;

                    case "[TOTAL_SOLICITADO]":
                        content = content.Replace("[TOTAL_SOLICITADO]", dictum.Solicitudes.Sum(x => x.RequestedTotal).ToString("C"));
                        break;

                    case "[TOTAL_APROBADO_TEXTO]":
                        content = content.Replace("[TOTAL_APROBADO_TEXTO]", StringHelper.NumberToWords(dictum.Solicitudes.Sum(x => x.ApprovedTotal ?? 0), StringHelper.Currency.Pesos).ToUpper());
                        break;

                    case "[DIRECCION]":
                        content = content.Replace("[DIRECCION]", plan.Field.Management);
                        break;

                    case "[COD_LINEA]": // TODO: Preguntar si es una sola línea por dictamen
                        content = content.Replace("[COD_LINEA]", dictum.Solicitudes.First().Line.Description);
                        break;
                }
            }

            return content;
        }

        /// <summary>
        /// Parsea una disposición con todas sus keywords correspondientes
        /// </summary>
        /// <param name="plan">Plan con el que se debe parsear el contenido</param>
        /// <param name="content">Contenido a parsear</param>
        /// <returns>El contenido parseado con las keywords reemplazadas</returns>
        private string ParseResolutionFields(Resolution resolution, string content)
        {
            var templateTypeFields = Context.TemplateTypeFields.Where(x => x.TemplateTypeId == (int)TemplateTypeEnum.Resolucion).ToList();
            decimal amountExecuted = resolution.Dictums.Sum(x => x.Ammount.HasValue ? x.Ammount.Value : 0);
            var plans = resolution.Dictums.Select(x => x.ImprovementPlan);
            var cues = resolution.Dictums.Select(x => x.ImprovementPlan.CUE.Substring(0, 2));
            foreach (var ttf in templateTypeFields)
            {
                switch (ttf.Field)
                {
                    case "[AÑO_RECEPCION]":
                        content = content.Replace("[A&Ntilde;O_RECEPCION]", String.Join(", ", plans.Select(x => x.ReceptionDate.Year.ToString())));
                        break;

                    case "[CICLO_LECTIVO]":
                        content = content.Replace("[CICLO_LECTIVO]", String.Join(", ", plans.Select(x => x.SchoolYear.Description.ToString())));
                        break;

                    case "[NRO_EXPEDIENTE]":
                        content = content.Replace("[NRO_EXPEDIENTE]", String.Join(", ", resolution.Dictums.Select(x => x.FileNumber.ToString())));
                        break;

                    case "[NRO_DICTAMEN]":
                        content = content.Replace("[NRO_DICTAMEN]", String.Join(", ", resolution.Dictums.Select(x => x.DictumNumber.ToString())));
                        break;

                    case "[TOTAL_APROBADO]":
                        content = content.Replace("[TOTAL_APROBADO]", amountExecuted.ToString("C"));
                        break;

                    case "[TOTAL_APROBADO_TEXTO]":
                        content = content.Replace("[TOTAL_APROBADO_TEXTO]", StringHelper.NumberToWords(amountExecuted, StringHelper.Currency.Pesos).ToUpper());
                        break;

                    case "[EJE]":
                        content = content.Replace("[EJE]", String.Join(", ", resolution.Dictums.Select(x => x.ImprovementPlan.Field.Description.ToString())));
                        break;
                    case "[JURISDICCION]":
                        var names = ListProvinces().Where(x => cues.Contains(x.Number)).Select(x => x.Name.ToUpper());

                        content = content.Replace("[JURISDICCION]", names.Aggregate((prev, text) => $"{prev}, {text}"));
                        break;
                    case "[MINISTERIO_PROVINCIA]":
                        var ministeriesNames = ListProvinces().Where(x => cues.Contains(x.Number)).Select(x => x.MinistryName);

                        content = content.Replace("[MINISTERIO_PROVINCIA]", ministeriesNames.Aggregate((prev, text) => $"{prev}, {text}"));
                        break;
                }
            }

            return content;
        }

        /// <summary>
        /// Parsea un dictamen con los bloques de acción correspondientes
        /// </summary>
        /// <param name="dictum">Dictamen con el que se debe parsear el contenido</param>
        /// <param name="content">Contenido a parsear</param>
        /// <returns>El contenido parseado con los bloques de acción generados</returns>
        private string ParseDictumBlocks(Dictum dictum, string content)
        {
            var templateTypeBlocks = Context.TemplateTypeBlocks.Where(x => x.TemplateTypeId == (int)TemplateTypeEnum.Dictamen).ToList();
            var plan = dictum.ImprovementPlan;
            foreach (var ttb in templateTypeBlocks)
            {
                switch (ttb.Block)
                {
                    case "{DETALLES_POR_LINEA}":
                        content = content.Replace("{DETALLES_POR_LINEA}", GenerateDetailsByLine(plan, dictum));
                        break;

                    case "{RESUMEN_DICTAMEN_INSTITUCIONAL}":
                        content = content.Replace("{RESUMEN_DICTAMEN_INSTITUCIONAL}", GenerateSummaryForInstitutionalDictum(plan, dictum));
                        break;

                    case "{RESUMEN_DICTAMEN_JURISDICCIONAL}":
                        content = content.Replace("{RESUMEN_DICTAMEN_JURISDICCIONAL}", GenerateSummaryForJurisdiccionalDictum(plan, dictum));
                        break;
                }
            }

            return content;
        }

        /// <summary>
        /// Parsea una resolucion con los bloques de acción correspondientes
        /// </summary>
        /// <param name="resolution">Resolucion con el que se debe parsear el contenido</param>
        /// <param name="content">Contenido a parsear</param>
        /// <returns>El contenido parseado con los bloques de acción generados</returns>
        private string ParseResolutionBlocks(Resolution resolution, string content)
        {
            var templateTypeBlocks = Context.TemplateTypeBlocks.Where(x => x.TemplateTypeId == (int)TemplateTypeEnum.Resolucion).ToList();
            foreach (var ttb in templateTypeBlocks)
            {
                switch (ttb.Block)
                {
                    case "{RESUMEN_POR_EXPEDIENTE}":
                        content = content.Replace("{RESUMEN_POR_EXPEDIENTE}", GenerateDetailByDictum(resolution));
                        break;
                }
            }

            return content;
        }

        /// <summary>
        /// Parsea una disposición con variables dinámicas ingresadas por el usuario.
        /// </summary>
        /// <param name="plan">Plan de mejora al que pertenece la disposición</param>
        /// <param name="content">Contenido a parsear</param>
        /// <returns>El contenido parseado con las variables dinámicas ingresadas por el usuario</returns>
        private string ParseResolutionVariables(Resolution resolution, string content, List<DocumentVariable> documentVariables)
        {
            var templateVariables = Context.TemplateVariables.Where(x => x.TemplateId == resolution.TemplateId).ToList();
            foreach (var templateVariable in templateVariables)
            {
                var documentVariable = documentVariables.Where(x => templateVariable.Id == x.TemplateVariableId).FirstOrDefault();
                if (documentVariable != null && !string.IsNullOrWhiteSpace(documentVariable.Text))
                    content = content.Replace(templateVariable.Variable, documentVariable.Text);
            }

            return content;
        }

        /// <summary>
        /// Genera los detalles de un dictamen para cada una de las líneas que lo componen
        /// </summary>
        /// <param name="plan">Plan de mejora</param>
        /// <param name="dictum">Dictamen</param>
        /// <returns>Un string con el encoding en HTML del correspondiente a los detalles por línea</returns>
        private string GenerateDetailsByLine(ImprovementPlan plan, Dictum dictum)
        {
            var sb = new StringBuilder();
            var solicitudesByLine = dictum.Solicitudes.GroupBy(x => x.Line).ToList();

            foreach (var sbl in solicitudesByLine)
            {
                // Encabezado por lína
                sb.Append("<p style='padding-left: 60px;'><span style=\"font-family: 'Arial Black'; font-size: 18px;\">L&iacute;nea de acci&oacute;n \"" + sbl.Key.Code + "\"</span></p>");
                sb.Append("<p style='padding-left: 60px;'><span style=\"font-family: 'Arial Black'; font-size: 14px;\">\"" + sbl.Key.Description + "\"</span></p>");

                var comments_dictionary = new Dictionary<int, string>();

                var comments_string = ParseComments(sbl, comments_dictionary);

                // Resumen total aprobado por línea
                sb.Append("<p style='padding-left: 30px;'>&nbsp;</p>");
                sb.Append("<table style='width: 80%; float: right;' border='1' cellspacing='0' cellpadding='10'>");
                sb.Append("<tbody>");
                sb.Append("<tr>");
                sb.Append("<td style='padding-left: 30px;' colspan='3' width='423'><span style='font-family: 'Arial Black'; font-size: 16px;'>Total aprobado l&iacute;nea \"" + sbl.Key.Code + "\"</span></td>");
                sb.Append("<td style='width: 136px; text-align: center;' colspan='2' width='136'><span style='font-family: 'Arial Black'; font-size: 14px;'>" + Convert.ToDecimal(sbl.Sum(x => x.ApprovedTotal ?? 0)).ToString("C") + "</span></td>");
                sb.Append("</tr>");
                sb.Append("</tbody>");
                sb.Append("</table>");

                sb.Append("<p style='padding-left: 30px;'>&nbsp;</p><p style='padding-left: 30px;'>&nbsp;</p>");

                // Tabla de solicitados
                sb.Append("<table style='width: 100%;' border='1' cellspacing='0' cellpadding='3'>");
                sb.Append("<thead>");
                sb.Append("<tr>");
                sb.Append("<td align='center' width='100'><span style=\"font-family: 'Arial'; font-size: 12px; font-weight:bold;\">Item</span></td>");
                sb.Append("<td align='center' width='150'><span style=\"font-family: 'Arial'; font-size: 12px; font-weight:bold;\">CUE</span></td>");
                sb.Append("<td align='center' width='90'><span style=\"font-family: 'Arial'; font-size: 12px; font-weight:bold;\">P/E</span></td>");
                sb.Append("<td align='center' width='350'><span style=\"font-family: 'Arial'; font-size: 12px; font-weight:bold;\">Descripción técnica</span></td>");
                sb.Append("<td align='center' width='90'><span style=\"font-family: 'Arial'; font-size: 12px; font-weight:bold;\">Unid.</span></td>");
                sb.Append("<td align='center' width='140'><span style=\"font-family: 'Arial'; font-size: 12px; font-weight:bold;\">Cant. Solic.</span></td>");
                sb.Append("<td align='center' width='140'><span style=\"font-family: 'Arial'; font-size: 12px; font-weight:bold;\">Cant. Aprob.</span></td>");
                sb.Append("<td align='center' width='240'><span style=\"font-family: 'Arial'; font-size: 12px; font-weight:bold;\">Costo total</span></td>");
                sb.Append("<td align='center' width='90'><span style=\"font-family: 'Arial'; font-size: 12px; font-weight:bold;\">Nro. Obs.</span></td>");
                sb.Append("</tr>");
                sb.Append("</thead>");
                sb.Append("<tbody>");

                var i = 1;
                foreach (var solicitude in sbl)
                {
                    decimal approvedCost = solicitude.ApprovedTotal.HasValue ? solicitude.ApprovedTotal.Value : 0;
                    var approvedAmount = solicitude.ApprovedAmount.HasValue ? solicitude.ApprovedAmount.Value : 0;
                    sb.Append("<tr>");
                    sb.Append("<td align='center'><span style=\"font-family: 'Arial'; font-size: 11px;\">" + i + "</span></td>");
                    sb.Append("<td align='center'><span style=\"font-family: 'Arial'; font-size: 11px;\">" + solicitude.CUE + "</span></td>");
                    sb.Append("<td align='center'><span style=\"font-family: 'Arial'; font-size: 11px;\">" + solicitude.Management + "</span></td>");
                    sb.Append("<td><span style=\"font-family: 'Arial'; font-size: 11px;\">" + System.Web.HttpUtility.HtmlEncode(solicitude.Details) + "</span></td>");
                    sb.Append("<td align='center'><span style=\"font-family: 'Arial'; font-size: 11px;\">" + solicitude.MeasurementUnit.Description + "</span></td>");
                    sb.Append("<td align='center'><span style=\"font-family: 'Arial'; font-size: 11px;\">" + solicitude.RequestedAmount.Value.ToString("0.##") + "</span></td>");
                    sb.Append("<td align='center'><span style=\"font-family: 'Arial'; font-size: 11px;\">" + approvedAmount.ToString("0.##") + "</span></td>");
                    sb.Append("<td align='right'><span style=\"font-family: 'Arial'; font-size: 11px;\">" + approvedCost.ToString("C") + "</span></td>"); // TODO: Determinar si es  requested o approved
                    sb.Append("<td align='center'><span style=\"font-family: 'Arial'; font-size: 11px;\">" + comments_dictionary[solicitude.Id] + "</span></td>");
                    sb.Append("</tr>");
                    i++;
                }

                sb.Append("</tbody>");
                sb.Append("</table>");

                sb.Append(comments_string);

                // Si no es la última línea, entonces inserto un salto de página 
                // para comenzar la siguiente línea en una página nueva
                if (sbl != solicitudesByLine.Last())
                    sb.Append("<p><div style='page-break-before:always'>&nbsp;</div></p>");
            }

            return sb.ToString();
        }

        private StringBuilder ParseComments(IGrouping<Line, Solicitude> sbl, Dictionary<int, string> comments_dictionary)
        {
            StringBuilder sb = new StringBuilder();

            bool isTotalLineApproved = Convert.ToDecimal(sbl.Sum(x => x.ApprovedTotal ?? 0)) >= Convert.ToDecimal(sbl.Sum(x => x.RequestedTotal));

            sb.Append("<p style='padding-left: 30px;'>&nbsp;</p>");
            sb.Append("<p style='padding-left: 30px;'><span style=\"font-family: 'Arial Black'; font-size: 14px;\">Observaciones:</span></p>");
            sb.Append("<p style='padding-left: 30px;'><span style=\"font-family: 'Arial Black'; font-size: 12px;\">Se recomienda aprobar " + (isTotalLineApproved ? "totalmente" : "parcialmente") + " teniendo en cuenta las siguientes observaciones:</span></p>");
            sb.Append("<p style='padding-left: 30px;'>------------------------------------------------------------------------------------------------------------</p>");

            if (!sbl.SelectMany(x => x.Comments).Where(x => x.IncludeInDictum == true).Any())
                sb.Append("<p style='padding-left: 30px;'><span style=\"font-family: 'Arial'; font-size: 14px;\">No se registraron observaciones.</span></p>");

            // Observaciones por línea
            var n = 0;

            foreach (var solicitude in sbl)
            {
                var obsIds = "";
                var observations = solicitude.Comments.Where(x => x.IncludeInDictum == true).Select(x => new { Id = x.Id, Text = x.Text.Trim() });

                foreach (var observation in observations)
                {
                    // buscamos si no cargamos uno con el mismo texto
                    var exists = comments_dictionary.Where(x => x.Value == observation.Text.Trim());
                    var _n = -1;
                    if (exists.Count() == 0)
                    {
                        // si no lo cargamos lo agregamos, avanzamos el puntero y lo listamos
                        n++;
                        comments_dictionary.Add(n, observation.Text.Trim());
                        _n = n;
                        sb.Append("<p style='padding-left: 30px;'><span style=\"font-family: 'Arial'; font-size: 14px;\">" + n + " - " + System.Web.HttpUtility.HtmlEncode(observation.Text) + "</span></p>");
                    }
                    else
                    {
                        // si estaba agregado traemos el numerador asignado
                        _n = exists.First().Key;
                    }
                    // lo agregamos a la lista de comentarios de este solicitado para el diccionario
                    obsIds += _n + ",";

                }

                if (!string.IsNullOrEmpty(obsIds))
                    obsIds = obsIds.Remove(obsIds.LastIndexOf(","), 1);

                comments_dictionary.Add(solicitude.Id, obsIds);
            }
            return sb;
        }

        /// <summary>
        /// Genera los detalles de una resolucion para cada una de los expedientes que lo componen
        /// </summary>
        /// <param name="resolution">Resolucion</param>
        /// <returns>Un string con el encoding en HTML del correspondiente a los detalles por línea</returns>
        private string GenerateDetailByDictum(Resolution resolution)
        {
            var sb = new StringBuilder();

            // Encabezado de la tabla
            sb.Append("<table style=\"border-collapse: collapse;\" width='100%' border='0' cellspacing='0' cellpadding='10'>");
            sb.Append("<tbody>");

            foreach (var dictum in resolution.Dictums)
            {
                var fieldData = "Eje " + dictum.ImprovementPlan.Field.Code + " - L&iacute;nea " + String.Join(", ", dictum.ImprovementPlan.Solicitudes.GroupBy(x => x.Line).Select(x => x.Key.Code).ToList());
                sb.Append("<tr>");
                sb.Append("<td style=\"border: solid 1px #000; text-align: left;\" align='left'><span style=\"font-family: 'Arial'; font-size: 12px;\">Exp. " + dictum.FileNumber + "</span></td>");
                sb.Append("<td style=\"border: solid 1px #000; text-align: left;\" align='left'><span style=\"font-family: 'Arial'; font-size: 12px;\">Dict. " + dictum.DictumNumber + "<br />" + fieldData + "</span></td>");
                sb.Append("<td style=\"border: solid 1px #000; text-align: right; white-space: nowrap;\" align='right' ><span style=\"font-family: 'Arial'; font-size: 12px;\">" + (dictum.Ammount.HasValue ? dictum.Ammount.Value.ToString("C") : "-") + "</span></td>");
                sb.Append("</tr>");
            }

            sb.Append("</tbody>");
            sb.Append("</table>");

            return sb.ToString();
        }

        /// <summary>
        /// Genera los detalles de una resolucion para cada una de los expedientes que lo componen
        /// </summary>
        /// <param name="resolution">Resolucion</param>
        /// <returns>Un string con el encoding en HTML del correspondiente a los detalles por línea</returns>
        public string GenerateResolutionAnexDocx(Resolution resolution)
        {
            var sb = new StringBuilder();

            // Encabezado de la tabla
            sb.Append("<h2>Anexo 1</h2>");
            sb.Append("<table style=\"border-collapse: collapse;\" width='100%' border='0' cellspacing='0' cellpadding='10'>");
            sb.Append("<tbody>");

            foreach (var dictum in resolution.Dictums)
            {
                var fieldData = "Eje Estratégico -" + dictum.ImprovementPlan.Field.Code + "- L&iacute;nea " + String.Join(", ", dictum.ImprovementPlan.Solicitudes.GroupBy(x => x.Line).Select(x => x.Key.Code).ToList()) + " - " + String.Join(", ", dictum.ImprovementPlan.Solicitudes.GroupBy(x => x.Line).Select(x => "\"" + x.Key.Description + "\"").ToList());
                sb.Append("<tr>");
                sb.Append("<td style=\"border: solid 1px #000; text-align: left;\" align='left'><span style=\"font-family: 'Arial'; font-size: 12px;\">Dictámen N&deg; " + dictum.DictumNumber + "</span></td>");
                sb.Append("<td style=\"border: solid 1px #000; text-align: left;\" align='left'><span style=\"font-family: 'Arial'; font-size: 12px;\">" + fieldData + "</span></td>");
                sb.Append("<td style=\"border: solid 1px #000; text-align: right; white-space: nowrap;\" align='right' ><span style=\"font-family: 'Arial'; font-size: 12px;\">" + (dictum.Ammount.HasValue ? dictum.Ammount.Value.ToString("C") : "-") + "</span></td>");
                sb.Append("</tr>");
            }

            sb.Append("</tbody>");
            sb.Append("</table>");

            return sb.ToString();
        }

        /// <summary>
        /// Genera los detalles de una resolucion para cada una de los expedientes que lo componen
        /// </summary>
        /// <param name="resolution">Resolucion</param>
        /// <returns>Bytes[] del documento a guardar</returns>
        public Byte[] GenerateResolutionAnexXlsx(Resolution resolution)
        {
            MemoryStream stream = new MemoryStream();
            using (var wb = new XLWorkbook(XLEventTracking.Disabled))
            {
                var ws = wb.Worksheets.Add("Ejecución Presupuestaria");
                ws.Cell(1, 1).Value = "unidad";
                ws.Cell(1, 2).Value = "cue";
                ws.Cell(1, 3).Value = "anexo";
                ws.Cell(1, 4).Value = "imputacion";
                ws.Cell(1, 5).Value = "dictamen";
                ws.Cell(1, 6).Value = "formulario";
                ws.Cell(1, 7).Value = "importe";
                ws.Column(7).Style.NumberFormat.Format = "$ #,##0.00";
                ws.Cell(1, 8).Value = "opealta";

                ws.Cell(2, 1).InsertData(resolution.Dictums.Select(x => new {
                    dictumNumber = x.DictumNumber,
                    totalAmmount = x.Ammount,
                    solicitude = x.Solicitudes.FirstOrDefault()
                })
                    .Select(x => new {
                        unidad = "",
                        cue = x.solicitude.CUE.Substring(0, 7),
                        anexo = x.solicitude.CUE.Substring(6, 2),
                        imputacion = "",
                        dictamen = "D " + x.dictumNumber,
                        formulario = x.solicitude.Line.Field.Code + "-" + x.solicitude.Line.Code,
                        importe = x.totalAmmount
                    })
                    .AsEnumerable()
                    );

                ws.Columns().AdjustToContents();
                wb.SaveAs(stream);
                stream.Position = 0;
            }

            return stream.ToArray();
        }

            /// <summary>
            /// Genera el resumen de un dictamen institucional por línea
            /// </summary>
            /// <param name="plan">Plan de mejora</param>
            /// <param name="dictum">Dictamen para el cual se desea generar el resumen</param>
            /// <returns>Un string con el HTML de la tabla perteneciente al resumen del dictamen institucional</returns>
            private string GenerateSummaryForInstitutionalDictum(ImprovementPlan plan, Dictum dictum)
        {
            var sb = new StringBuilder();
            var lines = Context.Lines.Where(x => x.FieldId == plan.FieldId);

            // Encabezado de la tabla            
            sb.Append("<table style=\"border-collapse: collapse;\" width='100%' border='0' cellspacing='0' cellpadding='0'>");
            sb.Append("<thead>");
            sb.Append("<tr>");
            sb.Append("<td style=\"background: #ccc; border: solid 1px #000; text-align: center;\" align='center' ><span style=\"font-family: 'Arial Black'; font-size: 14px;\">Línea de Acción</span></td>");
            sb.Append("<td style=\"background: #ccc; border: solid 1px #000; text-align: center;\" align='center' ><span style=\"font-family: 'Arial Black'; font-size: 14px;\">Monto Solicitado</span></td>");
            sb.Append("<td style=\"background: #ccc; border: solid 1px #000;\" align='center' ><span style=\"font-family: 'Arial Black'; font-size: 14px;\" >");
            sb.Append("<table style=\"border-collapse: collapse;\" width='100%' border='0' cellspacing='0' cellpadding='10'>");
            sb.Append("<tbody>");
            sb.Append("<tr align='center'>");
            sb.Append("<td style=\"border: solid 1px #000; font-family: 'Arial Black'; font-size: 14px; text-align: center;\" colspan='3'>Monto Aprobado</td>");
            sb.Append("</tr>");
            sb.Append("<tr align='center'>");
            sb.Append("<td style=\"text-align: center; border: solid 1px #000; font-family: 'Arial Black'; font-size: 14px;\" width='300'>Gastos de capital</td>");
            sb.Append("<td style=\"text-align: center; border: solid 1px #000; font-family: 'Arial Black'; font-size: 14px;\" width='300'>Gastos Corrientes</td>");
            sb.Append("<td style=\"text-align: center; border: solid 1px #000; font-family: 'Arial Black'; font-size: 14px;\" width='300'>Total Aprobado</td>");
            sb.Append("</tr>");
            sb.Append("</tbody>");
            sb.Append("</table>");
            sb.Append("</span></td>");
            sb.Append("</tr>");
            sb.Append("</thead>");
            sb.Append("<tbody>");

            // Resumen por línea
            var capitalTotal = 0M;
            var currentTotal = 0M;
            var requestedTotal = 0M;
            var approvedTotal = 0M;

            foreach (var line in lines)
            {
                var solicitudes = dictum.Solicitudes.Where(x => x.LineId == line.Id);
                var requested = solicitudes.Sum(x => x.RequestedTotal);
                var approved = solicitudes.Sum(x => x.ApprovedTotal ?? 0);
                var capital = solicitudes.Where(x => x.ExpenditureTypeId == (int)ExpenditureTypeEnum.GastoCapital).Sum(x => x.ApprovedTotal ?? 0);
                var current = solicitudes.Where(x => x.ExpenditureTypeId == (int)ExpenditureTypeEnum.GastoCorriente).Sum(x => x.ApprovedTotal ?? 0);

                sb.Append("<tr>");
                sb.Append("<td style=\"border: solid 1px #000; text-align: center; \" width='100'><span style=\"font-family: 'Arial'; font-size: 12px;\">" + line.Code + ")</span></td>");
                sb.Append("<td style=\"border: solid 1px #000; text-align: right; \" width='200'><span style=\"font-family: 'Arial'; font-size: 12px;\">" + requested.ToString("C") + "</span></td>");
                sb.Append("<td>");
                sb.Append("<table style='border-collapse: collapse; width: 100%;' border='0' cellspacing='0' cellpadding='10'>");
                sb.Append("<tr>");
                sb.Append("<td style=\"text-align: right; border: solid 1px #000;\" width='300'><span style=\"font-family: 'Arial'; font-size: 12px;\">" + capital.ToString("C") + "</span></td>");
                sb.Append("<td style=\"text-align: right; border: solid 1px #000;'\" width='300'><span style=\"font-family: 'Arial'; font-size: 12px;\">" + current.ToString("C") + "</span></td>");
                sb.Append("<td style=\"text-align: right; border: solid 1px #000; \" width='300'><span style=\"font-family: 'Arial'; font-size: 12px; font-weight: bold; \">" + approved.ToString("C") + "</span></td>");
                sb.Append("</tr>");
                sb.Append("</table>");
                sb.Append("</td>");
                sb.Append("</tr>");

                // Totales por tipo de gasto
                capitalTotal += capital;
                currentTotal += current;

                // Totales solicitados y aprobados
                requestedTotal += requested;
                approvedTotal += approved;
            }

            // Totales
            sb.Append("<tr>");
            sb.Append("<td style=\"border: solid 1px #000; text-align: center; \" width='100'><span style=\"font-family: 'Arial Black'; font-size: 14px;\">Total</span></td>");
            sb.Append("<td style=\"border: solid 1px #000; text-align: right; \" width='200'><span style=\"font-family: 'Arial Black'; font-size: 12px;\">" + requestedTotal.ToString("C") + "</span></td>");
            sb.Append("<td>");
            sb.Append("<table style='border-collapse: collapse; width: 100%;' border='0' cellspacing='0' cellpadding='10'>");
            sb.Append("<tr>");
            sb.Append("<td style=\"text-align: right; border: solid 1px #000;\" width='300'><span style=\"font-family: 'Arial Black'; font-size: 12px;\">" + capitalTotal.ToString("C") + "</span></td>");
            sb.Append("<td style=\"text-align: right; border: solid 1px #000;'\" width='300'><span style=\"font-family: 'Arial Black'; font-size: 12px;\">" + currentTotal.ToString("C") + "</span></td>");
            sb.Append("<td style=\"text-align: right; border: solid 1px #000; \" width='300'><span style=\"font-family: 'Arial Black'; font-size: 14px; \">" + approvedTotal.ToString("C") + "</span></td>");
            sb.Append("</tr>");
            sb.Append("</table>");
            sb.Append("</td>");
            sb.Append("</tr>");

            sb.Append("</tbody>");
            sb.Append("</table>");

            return sb.ToString();
        }

        /// <summary>
        /// Genera el resumen de un dictamen jurisdiccional por línea
        /// </summary>
        /// <param name="plan">Plan de mejora</param>
        /// <param name="dictum">Dictamen para el cual se desea generar el resumen</param>
        /// <returns>Un string con el HTML de la tabla perteneciente al resumen del dictamen jurisdiccional</returns>
        private string GenerateSummaryForJurisdiccionalDictum(ImprovementPlan plan, Dictum dictum)
        {
            var sb = new StringBuilder();
            var solicitudesByLine = dictum.Solicitudes.GroupBy(x => x.Line).ToList();

            // Encabezado de la tabla            
            sb.Append("<table style=\"border-collapse: collapse;\" width='100%' height='190px' border='0' cellspacing='0' cellpadding='0'>");
            sb.Append("<thead>");
            sb.Append("<tr>");
            sb.Append("<td style=\"background: #ccc; border: solid 1px #000; text-align: center;\" align='center' ><span style=\"font-family: 'Arial Black'; font-size: 14px;\">Item</span></td>");
            sb.Append("<td style=\"background: #ccc; border: solid 1px #000; text-align: center;\" align='center' ><span style=\"font-family: 'Arial Black'; font-size: 14px;\">Monto Solicitado</span></td>");
            sb.Append("<td style=\"background: #ccc; border: solid 1px #000;\" align='center' ><span style=\"font-family: 'Arial Black'; font-size: 14px;\" >");
            sb.Append("<table style=\"border-collapse: collapse;\" width='100%' border='0' cellspacing='0' cellpadding='10'>");
            sb.Append("<tbody>");
            sb.Append("<tr align='center'>");
            sb.Append("<td style=\"border: solid 1px #000; font-family: 'Arial Black'; font-size: 14px; text-align: center;\" colspan='3'>Monto Aprobado</td>");
            sb.Append("</tr>");
            sb.Append("<tr align='center'>");
            sb.Append("<td style=\"text-align: center; border: solid 1px #000; font-family: 'Arial Black'; font-size: 14px;\" width='200'>Gastos de capital</td>");
            sb.Append("<td style=\"text-align: center; border: solid 1px #000; font-family: 'Arial Black'; font-size: 14px;\" width='200'>Gastos Corrientes</td>");
            sb.Append("<td style=\"text-align: center; border: solid 1px #000; font-family: 'Arial Black'; font-size: 14px;\" width='200'>Total Aprobado</td>");
            sb.Append("</tr>");
            sb.Append("</tbody>");
            sb.Append("</table>");
            sb.Append("</span></td>");
            sb.Append("</tr>");
            sb.Append("</thead>");
            sb.Append("<tbody>");

            // Resumen por línea
            var capitalTotal = 0M;
            var currentTotal = 0M;
            var requestedTotal = 0M;
            var approvedTotal = 0M;

            foreach (var line in solicitudesByLine)
            {
                var requested = line.Sum(x => x.RequestedTotal);
                var approved = line.Sum(x => x.ApprovedTotal ?? 0);
                var capital = line.Where(x => x.ExpenditureTypeId == (int)ExpenditureTypeEnum.GastoCapital).Sum(x => x.ApprovedTotal ?? 0);
                var current = line.Where(x => x.ExpenditureTypeId == (int)ExpenditureTypeEnum.GastoCorriente).Sum(x => x.ApprovedTotal ?? 0);

                sb.Append("<tr>");
                sb.Append("<td style=\"background: #ccc; border: solid 1px #000; text-align: center; \" width='90'><span style=\"font-family: 'Arial'; font-size: 14px;\">Línea " + line.Key.Code + " - " + line.Key.Description + "</span></td>");
                sb.Append("<td style=\"border: solid 1px #000; text-align: right; \" width='150'><span style=\"font-family: 'Arial'; font-size: 12px;\">" + requested.ToString("C") + "</span></td>");
                sb.Append("<td style='padding:0;'>");
                sb.Append("<table style='border-collapse: collapse; width: 100%; height:100%;' border='0' cellspacing='0' cellpadding='0'>");
                sb.Append("<tr>");
                sb.Append("<td style=\"text-align: right; border: solid 1px #000; height:100%;\" width='300' ><span style=\"font-family: 'Arial'; font-size: 12px;\">" + capital.ToString("C") + "</span></td>");
                sb.Append("<td style=\"text-align: right; border: solid 1px #000; height:100%;\" width='300' ><span style=\"font-family: 'Arial'; font-size: 12px;\">" + current.ToString("C") + "</span></td>");
                sb.Append("<td style=\"text-align: right; border: solid 1px #000; height:100%;\" width='300' ><span style=\"font-family: 'Arial'; font-size: 12px; font-weight: bold; \">" + approved.ToString("C") + "</span></td>");
                sb.Append("</tr>");
                sb.Append("</table>");
                sb.Append("</td>");
                sb.Append("</tr>");

                // Totales por tipo de gasto
                capitalTotal += capital;
                currentTotal += current;

                // Totales solicitados y aprobados
                requestedTotal += requested;
                approvedTotal += approved;
            }

            // Totales
            sb.Append("<tr>");
            sb.Append("<td style=\"background: #ccc; border: solid 1px #000; text-align: center; \" width='190'><span style=\"font-family: 'Arial Black'; font-size: 14px;\">Total</span></td>");
            sb.Append("<td style=\"border: solid 1px #000; text-align: right; \" width='150'><span style=\"font-family: 'Arial Black'; font-size: 12px;\">" + requestedTotal.ToString("C") + "</span></td>");
            sb.Append("<td>");
            sb.Append("<table style='border-collapse: collapse; width: 100%; height: 100%;' border='0' cellspacing='0' cellpadding='10'>");
            sb.Append("<tr>");
            sb.Append("<td style=\"text-align: right; border: solid 1px #000;\" width='300'><span style=\"font-family: 'Arial Black'; font-size: 12px;\">" + capitalTotal.ToString("C") + "</span></td>");
            sb.Append("<td style=\"text-align: right; border: solid 1px #000;'\" width='300'><span style=\"font-family: 'Arial Black'; font-size: 12px;\">" + currentTotal.ToString("C") + "</span></td>");
            sb.Append("<td style=\"text-align: right; border: solid 1px #000; \" width='300'><span style=\"font-family: 'Arial Black'; font-size: 14px; \">" + approvedTotal.ToString("C") + "</span></td>");
            sb.Append("</tr>");
            sb.Append("</table>");
            sb.Append("</td>");
            sb.Append("</tr>");

            sb.Append("</tbody>");
            sb.Append("</table>");

            return sb.ToString();
        }

        /// <summary>
        /// Lista todas las plantillas activas correspondiente a un tipo de plantilla
        /// </summary>
        /// <param name="templateTypeId">Id del tipo de plantilla de la cual se desean listar las plantillas</param>
        /// <returns>La lista de plantillas activas correspondientes al tipo de plantilla pasado como parámetro</returns>
        public List<Template> ListByTemplateType(int templateTypeId)
        {
            return Context.Templates.Where(x => x.Active == true && x.TemplateTypeId == templateTypeId).OrderBy(x => x.Name).ToList();
        }

        /// <summary>
        /// Lista todas las plantillas activas correspondiente a un tipo de plantilla que aplican a un eje
        /// </summary>
        /// <param name="templateTypeId">Id del tipo de plantilla de la cual se desean listar las plantillas</param>
        /// <param name="planId">ID del plan al que corresponde el dictámen</param>
        /// <returns>La lista de plantillas activas correspondientes al tipo de plantilla pasado como parámetro</returns>
        public List<Template> ListByTemplateTypeForPlan(int templateTypeId, int planId)
        {
            var fieldId = Context.ImprovementPlans.Where(x => x.Id == planId).Select(x => x.FieldId).First();
            return Context.Templates.Where(x => x.Active == true && x.TemplateTypeId == templateTypeId && x.Fields.Any(y => y.Id == fieldId)).OrderBy(x => x.Name).ToList();
        }

        /// <summary>
        /// Lista todas las plantillas activas correspondiente a un tipo de plantilla que aplican a un plan y un usuario puede ver
        /// </summary>
        /// <param name="templateTypeId">Id del tipo de plantilla de la cual se desean listar las plantillas</param>
        /// <param name="planId">ID del plan al que corresponde el dictámen</param>
        /// <param name="userName">Nombre del usuario pidiendo las plantillas</param>
        /// <returns>La lista de plantillas activas correspondientes al tipo de plantilla pasado como parámetro</returns>
        public List<Template> ListByTemplateTypeForPlan(int templateTypeId, int planId, string userName)
        {
            var userFields = Context.UserProfiles.Where(x => x.UserName == userName).First().Fields.Select(y => y.Id).ToList();
            var fieldId = Context.ImprovementPlans.Where(x => x.Id == planId).Select(x => x.FieldId).First();
            return Context.Templates.Where(x => x.Active == true && x.TemplateTypeId == templateTypeId && x.Fields.Any(y => userFields.Contains(y.Id) && y.Id == fieldId)).OrderBy(x => x.Name).ToList();
        }

        /// <summary>
        /// Determina si un template es válido o no.
        /// </summary>
        /// <param name="templateTypeId">Id del tipo de plantilla que se desea grabar</param>
        /// <param name="content">Contenido que se desea guardar</param>
        /// <returns>True en caso de que el contenido de la plantilla sea válido. False en caso contrario.</returns>
        public bool IsValid(int templateTypeId, string content)
        {
            if (templateTypeId != TemplateTypeEnum.Resolucion.ToInt() && !content.Contains("[CUERPO]"))
                return false;

            if ((TemplateTypeEnum)templateTypeId != TemplateTypeEnum.DictamenDeEligibilidad && (TemplateTypeEnum)templateTypeId != TemplateTypeEnum.Resolucion)
                return content.Contains("[FIRMA_1]") || content.Contains("[FIRMA_2]") || content.Contains("[FIRMA_3]") ||
                    content.Contains("[FIRMA_4]") || content.Contains("[FIRMA_5]") || content.Contains("[FIRMA_6]");

            return true;
        }

        /// <summary>
        /// Lista todos los templateTypes pertenecientes a dictámenes
        /// </summary>
        /// <returns>Una lista de templatetype para dictamenes</returns>
        public List<TemplateType> ListTemplateTypesForDictums()
        {
            return Context.TemplateTypes.Where(x => x.Id < 3).ToList();
        }

        /// <summary>
        /// Lista los TemplateTypes pertenecientes a resoluciones
        /// </summary>
        /// <returns>Una lista de templatetypes para resoluciones</returns>
        public List<TemplateType> ListTemplateTypesForResolution()
        {
            return Context.TemplateTypes.Where(x => x.Id == 4).ToList();
        }

        /// <summary>
        /// Guarda el template de un documento
        /// </summary>
        /// <param name="dto">DTO del template a guardar</param>
        public void Save(TemplateDTO dto)
        {
            if (!IsValid(dto.TemplateTypeId, dto.Content))
                throw new ApplicationException("La plantilla que se desea grabar no es válida.");

            var template = (dto.Id == 0) ? new Template() : Context.Templates.Where(x => x.Id == dto.Id).First();
            template.Name = dto.Name;
            template.TemplateTypeId = dto.TemplateTypeId;
            template.Active = dto.Active;
            template.Content = dto.Content;
            template.Header = dto.Header;
            template.marginTop = dto.marginTop;
            template.marginBottom = dto.marginBottom;
            template.marginLeft = dto.marginLeft;
            template.marginRight = dto.marginRight;
            template.SignatureImagePath = dto.SignatureImagePath;

            template.Fields.Clear();
            foreach (var fieldId in dto.SelectedFieldsIds)
                template.Fields.Add(Context.Fields.Where(x => x.Id == fieldId).First());

            // Actualizo variables que ya existen
            var variablesToUpdate = new List<TemplateVariable>();
            var dtoVariablesIds = dto.Variables.Select(x => x.TemplateVariableId).ToList();
            foreach (var tv in template.TemplateVariables)
                if (dtoVariablesIds.Contains(tv.Id))
                    variablesToUpdate.Add(Context.TemplateVariables.Where(x => x.Id == tv.Id).First());

            // Elimino variables que ya no se utilicen mas
            var variablesToDelete = new List<TemplateVariable>();
            var templateVariables = template.TemplateVariables.ToList();
            foreach (var tv in templateVariables)
            {
                if (!dtoVariablesIds.Contains(tv.Id))
                {
                    var documentVariable = tv.DocumentVariables.Where(x => x.TemplateVariableId == tv.Id).ToList<DocumentVariable>();
                    Context.DocumentVariables.RemoveRange(documentVariable);
                    Context.TemplateVariables.Remove(tv);
                }
            }

            // Obtengo variables a agregar
            var variablesToAdd = dto.Variables.Where(x => x.TemplateVariableId == 0).ToList();
            foreach (var tv in variablesToAdd)
                template.TemplateVariables.Add(new TemplateVariable() { Variable = tv.VariableName });

            if (dto.Id == 0)
                Context.Templates.Add(template);

            Context.SaveChanges();
        }
    }

}
