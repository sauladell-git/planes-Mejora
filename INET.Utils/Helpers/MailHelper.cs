using OpenPop.Pop3;
using System;
using System.Configuration;
using System.IO;
using System.Net.Configuration;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace INET.Utils.Helpers
{
    /// <summary>
    /// Conf abstraction
    /// </summary>
    public class Pop3Data
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool SSL { get; set; }
    }

    /// <summary>
    /// Helper encargado del manejo de mails
    /// </summary>
    public static class MailHelper
    {

        /// <summary>
        /// Obtiene la conexione disponible del archivo de configuracion
        /// </summary>
        /// <returns></returns>
        private static Pop3Data getPop3DataFromConfiguration()
        {
            System.String last_string = System.Configuration.ConfigurationManager.AppSettings["pop_before_smtp"];
            Pop3Data pop3_data = new Pop3Data();
            if (!System.String.IsNullOrEmpty(last_string))
            {
                String[] parts = last_string.Split(';');
                foreach (String part in parts)
                {
                    String[] _part = part.Split('=');
                    if (_part.Length == 2)
                    {
                        switch (_part[0])
                        {
                            case "host":
                                pop3_data.Host = _part[1].Trim();
                                break;
                            case "port":
                                pop3_data.Port = Int32.Parse(_part[1].Trim());
                                break;
                            case "username":
                                pop3_data.Username = _part[1].Trim();
                                break;
                            case "password":
                                pop3_data.Password = _part[1].Trim();
                                break;
                            case "ssl":
                                pop3_data.SSL = _part[1].Trim() == "true" ? true : false;
                                break;
                        }
                    }
                }
            }
            if (String.IsNullOrEmpty(pop3_data.Host))
            {
                throw new Exception("No se encontraron datos para una conexión POP para validar envío de mails (pop_before_smtp)");
            }
            return pop3_data;
        }

        /// <summary>
        /// Envía un EMail
        /// </summary>
        /// <param name="email">Dirección de correo electronico a la que se desea enviar el mail</param>
        /// <param name="subject">Asunto del Email</param>
        /// <param name="body">Cuerpo del Mensaje</param>
        public static void Send(string email, string subject, string body)
        {
            MailMessage msg = new MailMessage();
            msg.To.Add(new MailAddress(email));
            msg.Subject = subject;
            msg.IsBodyHtml = true;
            msg.Body = ParseTemplate(subject, body);

            try
            {
                SmtpClient client = new SmtpClient();
                client.Send(msg);
            }
            catch (Exception e)
            {
                try
                {
                    using (Pop3Client pop_client = new Pop3Client())
                    {
                        var pop3_data = getPop3DataFromConfiguration();
                        pop_client.Connect(pop3_data.Host, pop3_data.Port, pop3_data.SSL, 10000, 10000, certificateValidator);
                        pop_client.Authenticate(pop3_data.Username, pop3_data.Password);
                    }
                }
                finally
                {
                    SmtpClient client = new SmtpClient();
                    client.Send(msg);
                }
            }
        }

        /// <summary>
        /// Override certificate validation for self signed ones
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslpolicyerrors"></param>
        /// <returns></returns>
        private static bool certificateValidator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            // We should check if there are some SSLPolicyErrors, but here we simply say that
            // the certificate is okay - we trust it.
            return true;
        }

        /// <summary>
        /// Parsea un template de un mail
        /// </summary>
        /// <param name="body">Cuerpo del mensaje a parsear</param>
        /// <param name="title">Titulo del mail</param>
        /// <returns>El cuerpo parseado del mensaje</returns>
        private static string ParseTemplate(string subject, string body)
        {
            var template = "";
            var templatePath = ConfigurationManager.AppSettings["MailTemplateLocation"];

            if (File.Exists(templatePath))
            {
                template = File.ReadAllText(templatePath);

                template = template.Replace("[SITEURL]", ConfigurationManager.AppSettings["SiteUrl"]);
                template = template.Replace("[TITLE]", subject);
                template = template.Replace("[BODY]", body);
            }

            return template;
        }
    }
}
