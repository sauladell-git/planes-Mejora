using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace INET.Utils.Helpers
{
    /// <summary>
    /// Helper encargado del manejo de strings
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Determina si un texto es un número
        /// </summary>
        /// <param name="text">Texto a verificar si es un número</param>
        /// <returns>True en caso de que el texto sea un número. False en caso contario.</returns>
        public static bool IsNumber(string text)
        {
            Array chars = text.ToCharArray();
            foreach (char c in chars)
                if (!char.IsNumber(c))
                    return false;

            return true;
        }

        /// <summary>
        /// Determina si un texto es un DateTime
        /// </summary>
        /// <param name="date">Texto a verificar si es una fecha</param>
        /// <returns>True en caso de que el texto sea un DateTime válido. False en caso contrario.</returns>
        public static bool IsDateTime(string date)
        {
            try
            {
                Convert.ToDateTime(date);
                return true;
            }
            catch (Exception)
            {
                try
                {
                    IFormatProvider formatProvider = new CultureInfo("en-US");
                    Convert.ToDateTime(date, formatProvider);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        
        /// <summary>
        /// Codifica tags HTML
        /// </summary>
        /// <param name="htmlText">Texto en HTML</param>
        /// <returns>El texto con los tags codificados</returns>
        public static string StripHtmlTags(string htmlText)
        {
            Regex reg = new Regex("<(.|\n)+?>");
            return reg.Replace(htmlText, "");
        }

        /// <summary>
        /// Trunca un texto a una determinada cantidad de caracteres y agrega 
        /// tres puntos suspensivos en caso de que el texto supere el length
        /// </summary>
        /// <param name="instr">Texto</param>
        /// <param name="len">Length maximo a truncar antes de concatenar los tres puntos (...)</param>
        /// <returns>El texto truncado con tres puntos suspensivos</returns>
        public static string Cut(string instr, int len)
        {
            if (!string.IsNullOrEmpty(instr))
            {
                instr = StripHtmlTags(instr);

                if (instr.Length > len)
                {
                    instr = instr.Substring(0, len - 3) + "...";
                }
            }
            return instr;
        }

        /// <summary>
        /// Establece la primera letra de un texto en mayúscula
        /// </summary>
        /// <param name="text">Texto a convertir la primera letra en mayúscula</param>
        /// <returns>El texto con la primer letra convertida a mayúscula</returns>
        public static string FirstLetterToUpper(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            var chars = text.ToCharArray();
            chars[0] = char.ToUpper(chars[0]);
            return new string(chars);
        }

        /// <summary>
        /// Convierte un texto a CamelCase
        /// </summary>
        /// <param name="text">Texto a convertir en camel case</param>
        /// <returns>El texto convertido a Camel Case</returns>
        public static string CamelCase(string text)
        {
            string[] arrWords = text.Split(' ');
            string strTemp2 = string.Empty;

            if (arrWords.Length > 1) //Existe mas de una palabra, Ej ANA MARIA
            {
                foreach (string strTemp in arrWords)
                {
                    strTemp2 += strTemp.Substring(0, 1).ToUpper() + strTemp.Substring(1).ToLower() + " ";
                }
            }
            else
                strTemp2 = arrWords[0].Substring(0, 1).ToUpper() + arrWords[0].Substring(1).ToLower() + " ";

            return strTemp2.Substring(0, strTemp2.Length - 1);
        }

        public static string GenerateSlug(this string phrase)
        {
            string str = phrase.RemoveAccent().ToLower();
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            return str;
        }

        public static string RemoveAccent(this string txt)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        #region Numbers To Letters
        /*
         Este codigo se distribuye bajo la licencia zlib/libpng.
         Para ver la licencia completa visita: http://www.opensource.org/licenses/Zlib///
         [ecrespo] Creado hace bastantes años, migrado múltiples veces a diferentes lenguajes
         [eferron] Migrado a C#
         [slanus]  Correción de un bug si el número era 1000
        */

        private static string[] strCentenas = new string[] { "ciento ", "doscientos ", "trescientos ", "cuatrocientos ", "quinientos ", "seiscientos ", "setecientos ", "ochocientos ", "novecientos " };
        private static string[] strDecenas = new string[] { "diez ", "veinte ", "treinta ", "cuarenta ", "cincuenta ", "sesenta ", "setenta ", "ochenta ", "noventa " };
        private static string[] strDecenas10 = new string[] { "diez ", "once ", "doce ", "trece ", "catorce ", "quince ", "dieciseis ", "diecisiete ", "dieciocho ", "diecinueve " };
        private static string[] strUnidades = new string[] { "un ", "dos ", "tres ", "cuatro ", "cinco ", "seis ", "siete ", "ocho ", "nueve " };

        public enum Currency { Pesos, Dolares }

        /// <summary>
        /// Devuleve un número de caracteres a la izquierda de la cadena
        /// </summary>
        /// <param name="str">Cadena</param>
        /// <param name="length">El número de caracteres a recuperar</param>
        /// <returns>El número de caracteres a la izquierda de la cadena</returns>
        private static string Left(this string str, int length)
        {
            return str.Substring(0, length);
        }

        /// <summary>
        /// Devuleve un número de caracteres a la derecha de la cadena
        /// </summary>
        /// <param name="str">Cadena</param>
        /// <param name="legth">El número de caracteres a recuperar</param>
        /// <returns>El número de caracteres a la derecha de la cadena</returns>        
        private static string Right(this string str, int legth)
        {
            int dif = str.Length - legth;
            if (dif < 0) // si es menor la longitud que lo a calcular, sumarle espacios
                str = str + " ";

            return str.Substring(str.Length - legth, legth);
        }

        /// <summary>
        /// Devuelve un entero representando el contenido de la cadena
        /// </summary>
        /// <param name="str">Cadena</param>        
        /// <returns>Un entero representando el contenido de la cadena</returns>
        private static int ToInt(this string str)
        {
            return System.Convert.ToInt32(str);
        }

        /// <summary>
        /// Devuelve un valor decimal representando el contenido de la cadena
        /// </summary>
        /// <param name="str">Cadena</param>
        /// <param name="value">Valor</param>        
        /// <returns>El valor decimal representando el contenido de la cadena</returns>
        private static int ToInt(this string str, int value)
        {
            int.TryParse(str, out value);
            return value;
        }

        /// <summary>
        /// Devuelve un entero representando el contenido de la cadena
        /// </summary>
        /// <param name="str">Cadena</param>        
        /// <returns>Un entero representando el contenido de la cadena</returns>
        private static decimal ToDecimal(this string str)
        {
            return System.Convert.ToDecimal(str);
        }

        /// <summary>
        /// Devuelve un valor decimal representando el contenido de la cadena
        /// </summary>
        /// <param name="str">Cadena</param>
        /// <param name="value">Valor</param>        
        /// <returns></returns>
        private static decimal ToDecimal(this string str, decimal value)
        {
            decimal.TryParse(str, out value);
            return value;
        }

        /// <summary>
        /// Devuelve un entero representando el contenido de la cadena
        /// </summary>
        /// <param name="str">Cadena</param>
        /// <returns>Un entero representando el contenido de la cadena</returns>
        private static double ToDouble(this string str)
        {
            return System.Convert.ToDouble(str);
        }

        /// <summary>
        /// Devuelve un valor decimal representando el contenido de la cadena
        /// </summary>
        /// <param name="str">Cadena</param>
        /// <param name="value">Valor</param>
        /// <returns>Un valor decimal representando el contenido de la cadena</returns>
        private static double ToDouble(this string str, double value)
        {
            double.TryParse(str, out value);
            return value;
        }

        /// <summary>
        /// Devuelve true si la cadena puede considerarse como verdadera
        /// </summary>
        /// <param name="str">Cadena</param>
        /// <returns>True si la cadena puede considerarse como verdadera. False en caso contrario.</returns>
        private static bool isTrue(this string str)
        {
            return !string.IsNullOrEmpty(str) && string.Compare(str, "false", true) != 0 && str != "0";
        }
        
        /// <summary>
        /// Conviete a palabras la cantidad representada en la cadena
        /// </summary>
        /// <param name="str">Cadena</param>
        /// <param name="currency">Moneda</param>
        /// <returns>La cantidad representada en la cadena en palabras</returns>
        public static string NumberToWords(this string str, Currency currency = Currency.Pesos)
        {
            return ToWords(ToDouble(str), currency);
        }

        /// <summary>
        /// Conviete a palabras la cantidad representada en la variable de tipo double
        /// </summary>
        /// <param name="str">Cadena</param>
        /// <param name="currency">Moneda</param>
        /// <returns>La cantidad representada en la variable de tipo double en palabras</returns>
        public static string NumberToWords(this double str, Currency currency)
        {
            return ToWords(str, currency);
        }        

        /// <summary>
        /// Conviete a palabras la cantidad representada en la variable de tipo decimal
        /// </summary>
        /// <param name="str">Cadena</param>
        /// <param name="currency">Moneda</param>
        /// <returns>La cantidad representada en la variable de tipo decimal en palabras</returns>
        public static string NumberToWords(this decimal str, Currency currency)
        {
            return ToWords(System.Convert.ToDouble(str), currency);
        }        

        /// <summary>
        /// Conviete a palabras la cantidad representada en la variable de tipo int
        /// </summary>
        /// <param name="str">Cadena</param>
        /// <param name="currency">Moneda</param>
        /// <returns>La cantidad representada en la variable de tipo int en palabras</returns>
        public static string NumberToWords(this int str, Currency currency)
        {
            return ToWords(System.Convert.ToDouble(str), currency);
        }        

        /// <summary>
        /// Conviete a palabras la cantidad representada en la variable de tipo uint
        /// </summary>
        /// <param name="str">Cadena</param>
        /// <param name="currency">Moneda</param>
        /// <returns>La cantidad representada en la variable de tipo uint en palabras</returns>
        public static string NumberToWords(this uint str, Currency currency)
        {
            return ToWords(System.Convert.ToDouble(str), currency);
        }        

        /// <summary>
        /// Convierte un número en su equivalente en letras
        /// </summary>
        /// <param name="import">Importe a convertir</param>
        /// <param name="currency">Moneda</param>
        /// <remarks> 
        /// Este método es especialmente útil al mostrar un monto literal 
        /// (por ejemplo, al mostrar un monto tipo factura, recibo o cheque.
        /// </remarks>
        /// <returns>El número redactado en letras</returns>
        private static string ToWords(double import, Currency currency)
        {
            // Recupera la cadena que representa esta cantidad      
            string strTmp = ToWords(import).Trim().Replace("  ", " ");
            string strCurrency = currency == Currency.Pesos ? "pesos" : "dólares";

            // Agrega la porción de la moneda y centavos        
            if (string.IsNullOrEmpty(strTmp))
                return string.Format("cero {0}", Cents(import, currency));
                //return string.Format("cero {0} {1}", strCurrency, Cents(import, currency));
            else if (strTmp == "un")
                return string.Format("un {0}", Cents(import, currency));
                //return string.Format("un {0} {1}", strCurrency, Cents(import, currency));
            else if (strTmp.Right(4) == "ones" | Right(strTmp, 2) == "on")
                return String.Format("{0} de {1}", strTmp, Cents(import, currency));
                //return String.Format("{0} de {1} {2}", strTmp, strCurrency, Cents(import, currency));
            
            return String.Format("{0} {1}", strTmp, Cents(import, currency));
            //return String.Format("{0} {1} {2}", strTmp, strCurrency, Cents(import, currency));
        }

        /// <summary>
        /// Esta es la rutina recursiva que convierte cada segmento en palabras
        /// </summary>
        /// <param name="import">Importe</param>
        /// <returns>El número convertido en letras</returns>
        private static string ToWords(double import)
        {
            Int32 ratio = 0;
            string strImport = import.ToString("000.00");
            //Checar Billones       
            //si el importe es un billon algo...        
            if ((int)(import / 1000000000000.0) == 1)
                return "un billon " + ToWords(ToDouble(Right(strImport, 15)));
            else if ((ratio = (int)(import / 1000000000000.0)) > 1)
                return string.Format("{0} billones {1}", ToWords(ratio), ToWords(ToDouble(Right(strImport, 15))));
            else
            {
                //Checar Millones           
                //si el importe es un millon algo...   
                if ((int)import == 1000)
                    return " un mil " + ToWords(ToDouble(Right(strImport, 6)));
                else if ((int)(import / 1000000) == 1)
                    return "un millon " + ToWords(ToDouble(Right(strImport, 9)));
                else if ((ratio = (int)(import / 1000000)) > 1)
                    return string.Format("{0} millones {1}", ToWords(ratio), ToWords(ToDouble(Right(strImport, 9))));
                else
                {
                    //Checar Millares               
                    if ((int)(import / 1000) == 1)
                        return "mil " + ToWords(ToDouble(Right(strImport, 6)));
                    else if ((ratio = (int)(import / 1000)) > 1)
                        return string.Format("{0} mil {1}", ToWords(ratio), ToWords(ToDouble(Right(strImport, 6))));
                    else
                    {
                        var result = new StringBuilder();
                        int digit1 = System.Convert.ToInt32(strImport[0].ToString());
                        int digit2 = System.Convert.ToInt32(strImport[1].ToString());
                        int digit3 = System.Convert.ToInt32(strImport[2].ToString());

                        //Checar centenas
                        if (digit1 == 1)
                        {
                            //solo cuando es cien exacto es diferente                       
                            if (Math.Abs(import) - 100 == 0)
                                result.Append("cien ");
                            else
                                result.Append(strCentenas[0]);
                        }
                        else if (digit1 > 1)
                            result.Append(strCentenas[digit1 - 1]);

                        //Checar Decenas                    
                        //si es uno le asigna su texto de una vez a las unidades                    
                        if (digit2 == 1)
                        {
                            result.Append(strDecenas10[digit3]);
                        }
                        else
                        {
                            //el veinte es especial   
                            if (digit2 == 2)
                            {
                                if (digit3 == 0)
                                    result.Append(strDecenas[1]);
                                else
                                    result.Append("veinti");
                            }
                            else if (digit2 == 0)
                            {
                                //el cero lo ignora            
                            }
                            else
                            {
                                result.Append(strDecenas[digit2 - 1]);
                                if (digit3 != 0)
                                    result.Append(" y ");
                            }

                            //Si las decenas no es uno asigna unidades.  
                            if (digit3 > 0)
                                result.Append(strUnidades[digit3 - 1]);

                        }

                        return result.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Determina los centavos
        /// </summary>
        /// <param name="import">Importe</param>
        /// <param name="currency">Moneda</param>
        /// <returns>Los centavos de la cadena</returns>
        private static string Cents(double import, Currency currency = Currency.Pesos)
        {
            return string.Format(" con {0}/100", Right(import.ToString("0.00"), 2));
            //return string.Format(" con {0}/100 {1}", Right(import.ToString("0.00"), 2), currency == Currency.Pesos ? " centavos." : string.Empty);
        }

        #endregion
    }
}
