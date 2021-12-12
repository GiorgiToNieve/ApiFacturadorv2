using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Drawing;
using System.Data.OleDb;
using System.Data;
using System.Collections.Generic;

namespace Utilitarios
{
    public static class Util
    {

        /// <summary>
        /// Elimina caracteres especiales
        /// </summary>
        /// <returns></returns>
        public static string EliminarCaracteresEspeciales(string str)
        {
            return Regex.Replace(str, @"[^0-9A-Za-z]", "", RegexOptions.None);
        }

        public static bool ValidaNumero(String sCadena)
        {
            return Regex.IsMatch(sCadena, "[0-9]+$");
        }


        /// <summary>
        /// [vnieve] Devuelve la imagen en un arreglo de bytes[]
        /// </summary>
        /// <param name="imageIn"></param>
        /// <returns></returns>
        public static byte[] ImageToByteArray(Image imageIn)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    imageIn.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        

        /// <summary>
        /// Devuelve sin acentos la cadena ingresada
        /// parametros caneda="", 
        /// tipo=1 minuscula
        /// tipo=2 mayuscula
        /// </summary>
        /// <param name="cadena"></param>
        /// <param name="TipoCaracter"></param>
        /// <returns></returns>
        public static string QuitarAcentos(string cadena, int TipoCaracter = 0)
        {
            cadena = cadena.ToLower();

            Regex a = new Regex("[á|à|ä|â]", RegexOptions.Compiled);
            Regex e = new Regex("[é|è|ë|ê]", RegexOptions.Compiled);
            Regex i = new Regex("[í|ì|ï|î]", RegexOptions.Compiled);
            Regex o = new Regex("[ó|ò|ö|ô]", RegexOptions.Compiled);
            Regex u = new Regex("[ú|ù|ü|û]", RegexOptions.Compiled);
            Regex n = new Regex("[ñ|Ñ]", RegexOptions.Compiled);
            cadena = a.Replace(cadena, "a");
            cadena = e.Replace(cadena, "e");
            cadena = i.Replace(cadena, "i");
            cadena = o.Replace(cadena, "o");
            cadena = u.Replace(cadena, "u");
            cadena = n.Replace(cadena, "n");

            if (TipoCaracter == 1)
                cadena = cadena.ToUpper();
            else
            {
                if (TipoCaracter == 1)
                    cadena = cadena.ToLower();
                else // devuelve la primera letra de la oracion en mayuscula y el resto
                    // minstulas
                    cadena = Util.CapitalizarPrimeraLetra(cadena);
            }

            return cadena;
        }

        public static string SerializeXML<T>(T obj)
        {
            string returnXml;
            var serializer = new XmlSerializer(typeof(T));
            //using (var writer = new StringWriterUtf8())
            using (var writer = new StringWriterIso8859())
            {
                serializer.Serialize(new XmlTextWriter(writer), obj);
                returnXml = writer.ToString();
            }
            return returnXml;
        }

        public static T DeserializeXML<T>(string sXml)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(T));
            TextReader reader = new StringReader(sXml);
            object obj = deserializer.Deserialize(reader);
            T XmlData = (T)obj;
            reader.Close();
            return XmlData;
        }

        public static T DeserializeXML<T>(string sXml, string sRoot)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(T), new XmlRootAttribute(sRoot));
            TextReader reader = new StringReader(sXml);
            object obj = deserializer.Deserialize(reader);
            T XmlData = (T)obj;
            reader.Close();
            return XmlData;
        }

        public static T CopyValues<T>(T target, T source)
        {
            var t = typeof(T);

            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(source, null);
                if (value != null)
                    prop.SetValue(target, value, null);
            }
            return target;
        }

        public static T CopyProperties<T>(this object source, T destination)
        {
            if (source == null || destination == null)
                throw new Exception("Source or/and Destination Objects are null");
            Type typeDest = destination.GetType();
            Type typeSrc = source.GetType();

            PropertyInfo[] srcProps = typeSrc.GetProperties();
            foreach (PropertyInfo srcProp in srcProps)
            {
                if (!srcProp.CanRead)
                {
                    continue;
                }
                PropertyInfo targetProperty = typeDest.GetProperty(srcProp.Name);
                if (targetProperty == null)
                {
                    continue;
                }
                //Se cambio temporalmente 20170907
                if (srcProp.Name.Contains("Serializable"))
                {
                    continue;
                }
                if (!targetProperty.CanWrite)
                {
                    continue;
                }
                if (targetProperty.GetSetMethod(true) != null && targetProperty.GetSetMethod(true).IsPrivate)
                {
                    continue;
                }
                if ((targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0)
                {
                    continue;
                }
                if (!targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType))
                {
                    continue;
                }
                // Passed all tests, lets set the value
                targetProperty.SetValue(destination, srcProp.GetValue(source, null), null);
            }
            return destination;
        }

        public static string DateTimeToString(DateTime dFecha, string sFormatoFecha = "yyyyMMdd")
        {
            if (sFormatoFecha.Equals("yyyyMMdd"))
            {
                char[] delim = { '/' };
                string[] arrFecha = dFecha.ToShortDateString().Split(delim);
                return (arrFecha[2] + arrFecha[1] + arrFecha[0] == "00010101"
                    ? ""
                    : arrFecha[2] + arrFecha[1] + arrFecha[0]);
            }
            if (sFormatoFecha.Equals("dd/MM/yyyy"))
            {
                return (dFecha.ToString("dd/MM/yyyy").Contains("01/01/0001") ? "" : dFecha.ToString("dd/MM/yyyy"));
            }
            return "";
        }

        public static DateTime StringToDatetime(string sFecha, string sFormatoFecha = "dd/MM/yyyy")
        {
            try
            {
                if (sFormatoFecha == "dd/MM/yyyy")
                {
                    char[] arDelimitador = { '/' };
                    string[] arFecha = sFecha.Split(arDelimitador);
                    return new DateTime(int.Parse(arFecha[2]), int.Parse(arFecha[1]), int.Parse(arFecha[0]));
                }
                return new DateTime();
            }
            catch (Exception)
            {
                return new DateTime();
            }
        }



        /// <summary>
        /// Metodos Metodos de Encriptar y desencriptar para una cadena.  
        /// </summary>
        public static string key = "ABCDEFG54669525PQRSTUVWXYZabcdef852846opqrstuvwxyz";

        public static string Encriptar(string cadena)
        {
            //arreglo de bytes donde guardaremos la llave
            byte[] keyArray;
            //arreglo de bytes donde guardaremos el texto
            //que vamos a encriptar
            byte[] Arreglo_a_Cifrar =
                Encoding.UTF8.GetBytes(cadena);

            //se utilizan las clases de encriptación
            //provistas por el Framework
            //Algoritmo MD5
            MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
            //se guarda la llave para que se le realice
            //hashing
            keyArray = hashmd5.ComputeHash(
                Encoding.UTF8.GetBytes(key));

            hashmd5.Clear();

            //Algoritmo 3DAS
            TripleDESCryptoServiceProvider tdes =
                new TripleDESCryptoServiceProvider();

            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            //se empieza con la transformación de la cadena
            ICryptoTransform cTransform =
                tdes.CreateEncryptor();

            //arreglo de bytes donde se guarda la
            //cadena cifrada
            byte[] ArrayResultado =
                cTransform.TransformFinalBlock(Arreglo_a_Cifrar,
                    0, Arreglo_a_Cifrar.Length);

            tdes.Clear();

            //se regresa el resultado en forma de una cadena
            return Convert.ToBase64String(ArrayResultado,
                0, ArrayResultado.Length);
        }

        public static string Desencriptar(string clave)
        {
            byte[] keyArray;
            //convierte el texto en una secuencia de bytes
            byte[] Array_a_Descifrar =
                Convert.FromBase64String(clave);

            //se llama a las clases que tienen los algoritmos
            //de encriptación se le aplica hashing
            //algoritmo MD5
            MD5CryptoServiceProvider hashmd5 =
                new MD5CryptoServiceProvider();

            keyArray = hashmd5.ComputeHash(
                Encoding.UTF8.GetBytes(key));

            hashmd5.Clear();

            TripleDESCryptoServiceProvider tdes =
                new TripleDESCryptoServiceProvider();

            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform =
                tdes.CreateDecryptor();

            byte[] resultArray =
                cTransform.TransformFinalBlock(Array_a_Descifrar,
                    0, Array_a_Descifrar.Length);

            tdes.Clear();
            //se regresa en forma de cadena
            return Encoding.UTF8.GetString(resultArray);
        }


        #region  Formato decimal 

        public static string formatoDecimal(decimal value)
        {
            return value.ToString("N2", new CultureInfo("en-US"));
        }

        #endregion


        /// <summary>[vnieve] 
        /// Validar que los caracteres sean Números enteros:
        /// True->Valido  False->Inválido
        /// </summary>
        /// <param name="Enteros"></param>
        /// <returns>true si es válido/false si no es válido</returns>
        public static bool ValidarNumeros(string Enteros)
        {
            Regex rex = new Regex(@"^[+-]?\d+(\.\d+)?$");
            if (rex.IsMatch(Enteros))
                return true;
            return false;
        }

        /// <summary>[vnieve]
        ///  Validar que los caracteres sean Números decimales
        /// </summary>
        /// <param name="NumeroDecimal"></param>
        /// <returns>true si es válido/false si no es válido</returns>
        public static bool ValidarDecimal(string NumeroDecimal)
        {
            Regex rex = new Regex(@"^(\d|-)?(\d|,)*\.?\d*$");
            if (NumeroDecimal == string.Empty)
                return false;
            if (NumeroDecimal.Equals(".") || NumeroDecimal.Equals(","))
                return false;
            if (rex.IsMatch(NumeroDecimal))
                return true;
            return false;
        }

        /// <summary>[vnieve]
        ///  Valida que el formato del email sea válido
        /// </summary>
        /// <param name="Email"></param>
        /// <returns>true si es válido/false si no es válido</returns>
        public static bool ValidarEmail(string Email)
        {
            string expresion = "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";

            if (Regex.IsMatch(Email, expresion))
            {
                if (Regex.Replace(Email, expresion, String.Empty).Length == 0)
                {
                    return true;
                }
                return false;
            }

            return false;
        }

        /// <summary>VNieve
        /// Valida que el formato del teléfono sea válido
        /// </summary>
        /// <param name="Telefono"></param>
        /// <returns>true si es válido/false si no es válido</returns>
        public static bool ValidarTelefono(string Telefono)
        {
            String expresion;
            expresion = "^\\+?\\d{1,3}?[- .]?\\(?(?:\\d{2,3})\\)?[- .]?\\d\\d\\d[- .]?\\d\\d\\d\\d$";
            if (Regex.IsMatch(Telefono, expresion))
            {
                if (Regex.Replace(Telefono, expresion, String.Empty).Length == 0)
                {
                    return true;
                }
                return false;
            }

            return false;
        }

        /// <summary>VNieve
        /// Valida que el formato del Url sea válido
        /// </summary>
        /// <param name="Url"></param>
        /// <returns>true si es válido/false si no es válido</returns>
        public static bool ValidarUrl(string Url)
        {
            String expresion;
            expresion =
                @"^(http|https|ftp|)\://|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
            if (Regex.IsMatch(Url, expresion))
            {
                if (Regex.Replace(Url, expresion, String.Empty).Length == 0)
                {
                    return true;
                }
                return false;
            }

            return false;
        }

        /// <summary> VNieve
        /// Convertir la primera letra de la oración
        /// a mayúsculas
        /// </summary>
        /// <param string="" name="Oracion"></param>
        /// <returns></returns>
        public static string CapitalizarPrimeraLetra(string Oracion)
        {
            var myTI = new CultureInfo("en-US", false).TextInfo;
            Oracion = myTI.ToLower(Oracion);

            return myTI.ToTitleCase(Oracion);
        }


        #region Convertir

        /// <summary>VNieve
        /// Convierte un número entero o decimal
        /// a formato de letras
        /// </summary>
        /// <param name="numero">Número que se desea convertir</param>
        /// <param name="moneda">Moneda que se desea adjuntar al texto</param>
        /// <returns>Número en formato texto</returns>
        public static string ConvertirNumerosAletras(decimal numero, string moneda)
        {
            var res = new StringBuilder();
            try
            {

               //string parteEntera = numero.ToString("#0.00").Split('.')[0];
               //string parteDecimal = numero.ToString("#0.00").Split('.')[1];

                string parteEntera = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", numero).Split('.')[0];
                string parteDecimal = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", numero).Split('.')[1];

                var numeroFormateado = new StringBuilder();
                int cuenta3 = 0;
                for (int p = parteEntera.Length - 1; p >= 0; p--)
                {
                    if (cuenta3 == 3)
                    {
                        numeroFormateado.Insert(0, "|");
                        cuenta3 = 0;
                    }
                    numeroFormateado.Insert(0, parteEntera.Substring(p, 1));
                    cuenta3++;
                }
                string[] numeros = numeroFormateado.ToString().Split('|');

                for (int p = 0; p < numeros.Length; p++)
                {
                    string n = num2texto(numeros[p]);
                    res.Append(n);
                    if (!n.Trim().Equals(""))
                    {
                        res.Append(grupos(numeros.Length - p - 1));
                    }
                }

                res = res.Replace("DIECIUNO", "ONCE");
                res = res.Replace("DIECIDOS", "DOCE");
                res = res.Replace("DIECITRES", "TRECE");
                res = res.Replace("DIECICUATRO", "CATORCE");
                res = res.Replace("DIECICINCO", "QUINCE");
                res = res.Replace("  ", " ");
                res = res.Replace("  ", " ");
                if (res.ToString().StartsWith("UNO MILLONES"))
                {
                    res = res.Replace("UNO MILLONES", "UN MILLON");
                }
                res = res.Replace("UNO MIL", "UN MIL");


                return res + "Y " + parteDecimal + "/100 " + moneda;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static string ConvertirNumerosAletrasRetencion(decimal numero, string moneda)
        {
            var res = new StringBuilder();
            try
            {
                string parteEntera = numero.ToString("#0.00").Split('.')[0];
                string parteDecimal = numero.ToString("#0.00").Split('.')[1];

                var numeroFormateado = new StringBuilder();
                int cuenta3 = 0;
                for (int p = parteEntera.Length - 1; p >= 0; p--)
                {
                    if (cuenta3 == 3)
                    {
                        numeroFormateado.Insert(0, "|");
                        cuenta3 = 0;
                    }
                    numeroFormateado.Insert(0, parteEntera.Substring(p, 1));
                    cuenta3++;
                }
                string[] numeros = numeroFormateado.ToString().Split('|');

                for (int p = 0; p < numeros.Length; p++)
                {
                    string n = num2texto(numeros[p]);
                    res.Append(n);
                    if (!n.Trim().Equals(""))
                    {
                        res.Append(grupos(numeros.Length - p - 1));
                    }
                }

                res = res.Replace("DIECIUNO", "ONCE");
                res = res.Replace("DIECIDOS", "DOCE");
                res = res.Replace("DIECITRES", "TRECE");
                res = res.Replace("DIECICUATRO", "CATORCE");
                res = res.Replace("DIECICINCO", "QUINCE");
                res = res.Replace("  ", " ");
                res = res.Replace("  ", " ");
                if (res.ToString().StartsWith("UNO MILLONES"))
                {
                    res = res.Replace("UNO MILLONES", "MILLON");
                }
                res = res.Replace("UNO MIL", "MIL");


                return res + " CON " + parteDecimal + "/100 " + moneda;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private static string grupos(int p)
        {
            string res = string.Empty;
            switch (p)
            {
                case 1:
                    res = " MIL ";
                    break;
                case 2:
                    res = " MILLONES ";
                    break;
                case 3:
                    res = " MIL ";
                    break;
            }
            return res;
        }

        private static string num2texto(string numero)
        {
            string res = string.Empty;

            switch (numero.Length)
            {
                case 1:
                    res += unidad2texto(int.Parse(numero.Substring(0, 1)));
                    break;

                case 2:
                    res += decena2texto(int.Parse(numero.Substring(0, 1)));

                    if (int.Parse(numero.Substring(0, 1)) > 0
                        && int.Parse(numero.Substring(1, 1)) == 0)
                    {
                        res = res.Replace("DIECI", "DIEZ ");
                        res = res.Replace("VEINTI", "VEINTE ");
                    }


                    if (int.Parse(numero.Substring(0, 1)) >= 3
                        && int.Parse(numero.Substring(1, 1)) > 0)
                    {
                        res += " Y ";
                    }
                    if (int.Parse(numero.Substring(1, 1)) > 0)
                    {
                        res += unidad2texto(int.Parse(numero.Substring(1, 1)));
                    }
                    break;

                case 3:
                    res += centena2texto(int.Parse(numero.Substring(0, 1)));

                    if (int.Parse(numero.Substring(0, 1)) > 0
                        && int.Parse(numero.Substring(1, 2)) == 0)
                    {
                        res = res.Replace("CIENTO", "CIEN ");
                    }

                    if (int.Parse(numero.Substring(1, 2)) > 0)
                    {
                        if (int.Parse(numero.Substring(1, 1)) > 0)
                        {
                            res += decena2texto(int.Parse(numero.Substring(1, 1)));

                            if (int.Parse(numero.Substring(1, 1)) > 0
                                && int.Parse(numero.Substring(2, 1)) == 0)
                            {
                                res = res.Replace("DIECI", "DIEZ ");
                                res = res.Replace("VEINTI", "VEINTE ");
                            }

                            if (int.Parse(numero.Substring(1, 1)) >= 3
                                && int.Parse(numero.Substring(1, 1)) > 0)
                            {
                                if (int.Parse(numero.Substring(2, 1)) > 0)
                                {
                                    res += "  Y ";
                                }
                                else
                                {
                                    res += "  ";
                                }



                                //String nnn = numero.Substring(1, 1);


                            }
                        }

                        if (int.Parse(numero.Substring(2, 1)) > 0)
                        {
                            res += unidad2texto(int.Parse(numero.Substring(2, 1)));
                        }
                    }
                    break;
            }
            return res;
        }

        private static string centena2texto(int numero)
        {
            string res = string.Empty;
            switch (numero)
            {
                case 1:
                    res = "CIENTO ";
                    break;
                case 2:
                    res = "DOSCIENTOS ";
                    break;
                case 3:
                    res = "TRESCIENTOS ";
                    break;
                case 4:
                    res = "CUATROCIENTOS ";
                    break;
                case 5:
                    res = "QUINIENTOS ";
                    break;
                case 6:
                    res = "SEISCIENTOS ";
                    break;
                case 7:
                    res = "SETECIENTOS ";
                    break;
                case 8:
                    res = "OCHOCIENTOS ";
                    break;
                case 9:
                    res = "NOVECIENTOS ";
                    break;
            }
            return res;
        }

        private static string decena2texto(int numero)
        {
            string res = string.Empty;
            switch (numero)
            {
                case 1:
                    res = "DIECI";
                    break;
                case 2:
                    res = "VEINTI";
                    break;
                case 3:
                    res = "TREINTA ";
                    break;
                case 4:
                    res = "CUARENTA ";
                    break;
                case 5:
                    res = "CINCUENTA ";
                    break;
                case 6:
                    res = "SESENTA ";
                    break;
                case 7:
                    res = "SETENTA ";
                    break;
                case 8:
                    res = "OCHENTA ";
                    break;
                case 9:
                    res = "NOVENTA ";
                    break;
            }
            return res;
        }

        private static string unidad2texto(int numero)
        {
            string res = string.Empty;
            switch (numero)
            {
                case 0:
                    res = "CERO ";
                    break;
                case 1:
                    res = "UNO ";
                    break;
                case 2:
                    res = "DOS ";
                    break;
                case 3:
                    res = "TRES ";
                    break;
                case 4:
                    res = "CUATRO ";
                    break;
                case 5:
                    res = "CINCO ";
                    break;
                case 6:
                    res = "SEIS ";
                    break;
                case 7:
                    res = "SIETE ";
                    break;
                case 8:
                    res = "OCHO";
                    break;
                case 9:
                    res = "NUEVE ";
                    break;
            }
            return res;
        }

        #endregion

        #region Rellenar Ceros a Correlativo Factura-Boleta

        /// <summary>
        /// Rellena de Ceros a la izquiera para el correlativo
        /// de facturas o boletas
        /// </summary>
        /// <param name="nroCadena"></param>
        /// <returns></returns>
        public static string nroCorrelativo(string nroCadena)
        {
            int caracteres = 0;
            caracteres = nroCadena.Length;

            switch (caracteres)
            {
                case 1:
                    nroCadena = "000000" + nroCadena;
                    break;
                case 2:
                    nroCadena = "00000" + nroCadena;
                    break;
                case 3:
                    nroCadena = "0000" + nroCadena;
                    break;
                case 4:
                    nroCadena = "000" + nroCadena;
                    break;
                case 5:
                    nroCadena = "00" + nroCadena;
                    break;
                case 6:
                    nroCadena = "0" + nroCadena;
                    break;
            }

            return nroCadena;
        }

        /// <summary>
        /// Rellena de Ceros a la izquierda 
        /// para el número de serie de la Factura o Boleta
        /// </summary>
        /// <param name="nroCadena"></param>
        /// <returns></returns>
        public static string nroSerie(string nroCadena) //para la serie
        {
            int caracteres = 0;
            caracteres = nroCadena.Length;

            switch (caracteres)
            {
                case 1:
                    nroCadena = "00" + nroCadena;
                    break;
                case 2:
                    nroCadena = "0" + nroCadena;
                    break;
            }

            return nroCadena;
        }

        /// <summary>
        /// Rellena de Ceros a la izquiera para el correlativo
        /// </summary>
        /// <param name="nroDigitos"></param>
        /// <param name="nroCorrelativo"></param>/// 
        /// <returns></returns>
        public static string nroCorrelativo(int nroDigitos, long nroCorrelativo)
        {
            string nroCadena = string.Empty;
            nroCadena = new string('0', nroDigitos);
            nroCadena += (nroCorrelativo).ToString();
            nroCadena = nroCadena.Substring(nroCadena.Length - nroDigitos);
            return nroCadena;
        }

        #endregion

        /// <summary>
        /// Conversion de Variables
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbValue"></param>
        /// <returns></returns>
        public static T Valor<T>(object dbValue) where T : struct
        {
            T Item;
            if (dbValue == null)
            {
                return default(T);
            }
            if (dbValue.Equals(DBNull.Value))
            {
                return default(T);
            }
            if (string.IsNullOrEmpty(dbValue.ToString()))
            {
                Item = default(T);
            }
            try
            {
                var conv = TypeDescriptor.GetConverter(typeof(T));
                Item = (T)conv.ConvertFrom(dbValue.ToString());
            }
            catch
            {
                Item = default(T);
            }
            return Item;
        }

        public static T? DbValueToNullable<T>(Object dbValue) where T : struct
        {
            T? returnValue = null;
            if (string.IsNullOrWhiteSpace(dbValue.Recortar()))
            {
                return null;
            }
            if (dbValue != null && !dbValue.Equals(DBNull.Value))
            {
                var conv = TypeDescriptor.GetConverter(typeof(T));
                returnValue = (T)conv.ConvertFrom(dbValue.ToString());
            }
            return returnValue;
        }

        /// <summary>
        /// Quitar Espacios
        /// </summary>
        /// <param name="dbValue"></param>
        /// <returns></returns>
        public static string Recortar(this object dbValue)
        {
            string valor = Convert.ToString(dbValue).Trim();
            return valor;
        }

        /// <summary>
        ///  Convierte un Objeto en Entero 32bits
        /// </summary>
        /// <param name="dbValue"></param>
        /// <returns></returns>
        public static int toInt(this object dbValue)
        {
            try
            {
                if (dbValue == null)
                    return 0;
                if (dbValue.ToString().Trim().Equals("")) return 0;
                int valor = Convert.ToInt32(dbValue);
                return valor;
            }
            catch
            {
                return 0;
            }
        }


        /// <summary>
        /// [vnieve] Convierte un Objeto a DateTime respetando
        /// la configuración regional del dispositivo
        /// </summary>
        /// <param name="Valor"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this object Valor)
        {
            try
            {
                var FechaValor = Convert.ToDateTime(Valor);
                return Convert.ToDateTime(FechaValor.ToString("s"));
            }
            catch
            {
                return new DateTime(1901, 9, 9, 9, 9, 9);
            }
        }

        /// <summary>
        ///  Convierte un Objeto a Decimal con formato 0.00
        /// </summary>
        /// <param name="dbValue"></param>
        /// <param name="bmoneda"></param>
        /// <returns></returns>
        public static decimal toDecimal(this object dbValue, bool bmoneda = false)
        {
            try
            {
                if (dbValue == null)
                    return 0;
                if (dbValue.ToString().Trim().Equals("")) return 0;
                decimal valor = Convert.ToDecimal(dbValue);

                if (bmoneda)
                    return Convert.ToDecimal(formatoDecimal(valor));

                return valor;
            }
            catch
            {
                return 0;
            }
        }
        public static bool toBoolean(this object dbValue, bool bmoneda = false)
        {
            try
            {
                if (dbValue == null)
                    return false;
                if (dbValue is Boolean)
                    return (bool)dbValue;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }


        public static int nroDecimales(this decimal dbValue)
        {
            try
            {
                int priceDecimalPlaces = dbValue.ToString(CultureInfo.InvariantCulture).Split('.').Count() > 1
                    ? dbValue.ToString(CultureInfo.InvariantCulture).Split('.').ToList().ElementAt(1).Length
                    : 0;
                string Cadena = dbValue.ToString(CultureInfo.InvariantCulture);

                foreach (var item in Cadena.Reverse())
                {
                    if (item == '0')
                        priceDecimalPlaces--;
                    else
                        break;
                }
                return priceDecimalPlaces;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Convierte un Objeto a string
        /// </summary>
        /// <param name="dbValue"></param>
        /// <returns></returns>
        public static string texto(this object dbValue)
        {
            return dbValue == null ? "" : dbValue.ToString().Replace((char)(0x1F), ' ').Trim();
        }


        /// <summary>
        /// quita los caracteres de comillas simples
        /// </summary>
        /// <param name="dbValue"></param>
        /// <returns></returns>
        public static string QuitarComillas(this string dbValue)
        {
            if (dbValue.IndexOf("'", StringComparison.Ordinal) > -1)
            {
                dbValue = dbValue.Replace("'", @"");
            }
            return dbValue;
        }

    


    public static void BorraFichero(string Ruta)
        {
            if (!string.IsNullOrWhiteSpace(Ruta))
            {
                if (File.Exists(Ruta))
                {
                    File.Delete(Ruta);
                }
            }
        }

        /// <summary>
        ///  Obtiene El obj[0]=IGV, obj[1]=Base y obj[2]Total 
        /// </summary>
        /// <param name="nIncluye"></param>
        /// <param name="nExcepcionIGV"></param>
        /// <param name="nPrecio"></param>
        /// <param name="nCantidad"></param>
        /// <param name="nIGV"></param>
        /// <returns></returns>
        public static object calcularBase(int nIncluye, int nExcepcionIGV, decimal nPrecio, decimal nCantidad,
            decimal nIGV)
        {
            //nIGV = Math.Round(ImpuestoNeg.Instance.ObtenerValorImpuesto(IGV_Id) / 100, 2);
            decimal nValorIGV = 0;
            decimal nValorBase = 0;
            decimal nValorTotal = 0;

            if (nIncluye == 1 && nExcepcionIGV == 1)
            {
                nValorIGV = decimal.Parse((((nPrecio * nCantidad) * (nIGV * 100)) / (100 + (nIGV * 100))).ToString("N2"));
                nValorBase = decimal.Parse(((nPrecio * nCantidad) - (nValorIGV)).ToString("N2"));
                nValorTotal = decimal.Parse((nPrecio * nCantidad).ToString("N2"));
            }
            if (nIncluye == 1 && nExcepcionIGV == 1) //Incluye IGV: SI
            {
                nValorIGV = decimal.Parse((((nPrecio * nCantidad) * (nIGV * 100)) / (100 + (nIGV * 100))).ToString("N2"));
                nValorBase = decimal.Parse(((nPrecio * nCantidad) - (nValorIGV)).ToString("N2"));
                nValorTotal = decimal.Parse((nPrecio * nCantidad).ToString("N2"));
            }
            else if (nIncluye == 2 && nExcepcionIGV == 1) //Incluye IGV: NO
            {
                nValorIGV = decimal.Parse((((nPrecio * nCantidad) * nIGV)).ToString("N2"));
                nValorBase = decimal.Parse((nPrecio * nCantidad).ToString("N2"));
                nValorTotal = decimal.Parse(((nPrecio * nCantidad) + (nValorIGV)).ToString("N2"));
            }
            else
            {
                nValorIGV = decimal.Parse("0.00");
                nValorBase = decimal.Parse(((nPrecio * nCantidad) - (nValorIGV)).ToString("N2"));
                nValorTotal = decimal.Parse((nPrecio * nCantidad).ToString("N2"));
            }

            return new object[] { nValorIGV, nValorBase, nValorTotal };
        }


        public static string FechaFormatoMilitar(String fecha)
        {
            char[] delim = { '/' };

            string[] arrFecha = fecha.Split(delim);

            return arrFecha[2] + arrFecha[1] + arrFecha[0];
        }


        /// <summary>
        ///  Dada una Fecha devuelve el nombre del dia al que pertenece esa fecha en C.R. español
        /// </summary>
        /// <param name="dFecha"></param>
        /// <returns></returns>
        public static string ConsultarDiaNombre(DateTime dFecha)
        {
            try
            {
                return dFecha.ToString("dddd", CultureInfo.CreateSpecificCulture("es-ES"));
            }
            catch (Exception)
            {
                throw;
            }
        }


        public static Dictionary<string, object> StringToDictionary(string sCadena, char sDelimitadorElementos = '&',
         char sDelimitadorxElemento = '=')
        {
            try
            {
                var dct = new Dictionary<string, object>();
                string[] arItems = sCadena.Split(sDelimitadorElementos);
                if (arItems.Length > 0)
                {
                    foreach (string sKeyValueItem in arItems)
                    {
                        var arKeyValue = sKeyValueItem.Split(sDelimitadorxElemento);
                        if (arKeyValue.Length == 2)
                        {
                            dct.Add(arKeyValue[0].Trim(), arKeyValue[1].Trim());
                        }
                        else
                        {
                            throw new Exception("Longitud de elemento llave valor no es válida (" +
                                                arKeyValue.Length + ")");
                        }
                    }
                }
                else
                {
                    throw new Exception("No se pueden distinguir elementos dentro de la cadena");
                }
                return dct;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region Mensajes Para Log

        /// <summary>
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="sDatabase"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string MensajeProcedureLog(SqlException exception, string sDatabase, SqlParameterCollection parameters,string sNombreUsuario="")
        {
            var sb = new StringBuilder();
            string parametros = string.Empty;

            try
            {
                if (exception == null)
                {
                    throw new ArgumentNullException("excepcion");
                }

                if (parameters != null)
                {
                    foreach (SqlParameter parameter in parameters)
                    {
                        parametros += parameter.ParameterName + " = " + parameter.Value.ToString() + ", ";
                    }
                }

                sb.AppendLine("SQL Server");
                sb.AppendLine(string.Format("Usuario: {0}", sNombreUsuario));
                sb.AppendLine(string.Format("Servidor: {0}", exception.Server));
                sb.AppendLine(string.Format("Base de Datos: {0}", sDatabase));
                sb.AppendLine(string.Format("Procedimiento: {0}", exception.Procedure));
                sb.AppendLine(string.Format("Parametros: {0}", parametros));
                sb.AppendLine(string.Format("Mensaje: {0}", exception.Message));
            }
            catch (Exception ex)
            {
                sb.AppendLine("SQL Server");
                sb.AppendLine(string.Format("Usuario: {0}", sNombreUsuario));
                sb.AppendLine("Error en generación de log: " + DateTime.Now);
                sb.AppendLine(string.Format("Mensaje: {0}", ex.Message));
            }
            finally
            {
                sb.AppendLine("===================================================================================================================================");
            }

            return sb.ToString();
        }

      




        /// <summary>
        /// [vnieve] Construye el mensaje de log application
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string MensajeAplicacionLog(Exception exception, string sNombreUsuario="")
        {
			var sb = new StringBuilder();

			if (exception != null)
			{
				var trace = new System.Diagnostics.StackTrace(exception, true);

				try
				{
					sb.AppendLine("NEVADASUN ERP");
					sb.AppendLine(string.Format("Usuario: {0}", sNombreUsuario));
					sb.AppendLine(string.Format("Estación: {0}", Environment.MachineName));
					sb.AppendLine(string.Format("Proyecto: {0}", exception.Source));
					sb.AppendLine(string.Format("Archivo: {0}", trace.GetFrame(0).GetMethod().ReflectedType.FullName));
					sb.AppendLine(string.Format("Método: {0}", exception.TargetSite.Name));
					sb.AppendLine(string.Format("Linea: {0}", trace.GetFrame(0).GetFileLineNumber()));
					sb.AppendLine(string.Format("Mensaje: {0}", exception.Message));
				}
				catch (Exception ex)
				{
					sb.AppendLine("NEVADASUN ERP");
					sb.AppendLine(string.Format("Usuario: {0}", sNombreUsuario));
					sb.AppendLine("Error en generación de log: " + DateTime.Now.ToLongDateString());
					sb.AppendLine(string.Format("Mensaje: {0}", ex.Message));

				}
				finally
				{
					sb.AppendLine("===================================================================================================================================");
				}
			}
			else
			{
				sb.AppendLine("NEVADASUN ERP");
			}



            return sb.ToString();
        }


        #endregion


        /// <summary>
        /// [vnieve] Devuelve una cadena con la representación de los tipos
        /// de extensiones de los archivos para filtrar al escoger
        /// los adjuntos de archivos
        /// </summary>
        /// <returns></returns>
        public static string FiltroArchivos()
        {
            return @"Archivos (*.pdf;*.xlsx;*.xls;*.png;*.jpg;*.bmp)|*.pdf;*.xlsx;*.xls;*.png;*.jpg;*.bmp";
        }
    }

    public class StringWriterUtf8 : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }

    public class StringWriterIso8859 : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.GetEncoding("ISO-8859-1"); }
        }
    }

}
