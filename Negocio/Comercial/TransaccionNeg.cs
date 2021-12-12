using Datos.Comercial;
using Entidades.Comercial;
using Entidades.Maestros;
using Negocio.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilitarios;

namespace Negocio.Comercial
{
    public class TransaccionNeg : BaseNegocio
    {
        #region "Singleton"

        private static readonly Lazy<TransaccionNeg> instance =
            new Lazy<TransaccionNeg>(() => new TransaccionNeg());

        private TransaccionNeg()
        {
        }

        public static TransaccionNeg Instance
        {
            get { return instance.Value; }
        }

        #endregion

        #region Metodos
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="LstTransaccion"></param>
        /// <returns></returns>
        public Task<bool> GuardarAsync(List<Transaccion> LstTransaccion)
        {
            try
            {
                return Task.Run(() => TransaccionNeg.Instance.Guardar(LstTransaccion));
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="LstTransaccion"></param>
        /// <returns></returns>
        public bool Guardar(List<Transaccion> LstTransaccion)
        {
            try
            {
                TransaccionAdo.Instance.Guardar(LstTransaccion);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public int ConsultarEstadoElectronico(Dictionary<string, object> parametros)
        {
            try
            {
               return TransaccionAdo.Instance.ConsultarEstadoElectronico(parametros);
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// devuelve un result asyncrono
        /// </summary>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public Task<List<Transaccion>> ConsultarAsync(Dictionary<string, object> parametros = null)
        {
            try
            {
                return Task.Run(() => TransaccionAdo.Instance.ConsultarTransaccion(parametros));
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public List<Transaccion> Consultar(Dictionary<string, object> parametros)
        {
            try
            {
                return TransaccionAdo.Instance.ConsultarTransaccion(parametros);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtcParametrosValor"></param>
        /// <param name="dtcParametrosId"></param>
        public void Actualizar(Dictionary<string, object> dtcParametrosValor, Dictionary<string, object>
            dtcParametrosId)
        {
            try
            {
                TransaccionAdo.Instance.Actualizar(dtcParametrosValor, dtcParametrosId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sRutaPDF"></param>
        /// <param name="oEmpresa"></param>
        /// <param name="oTransaccion"></param>
        /// <param name="sAsunto"></param>
        /// <param name="sMensajeTexto"></param>
        public void EnviarCorreo(string sRutaPDF, Empresa oEmpresa, Transaccion oTransaccion, string sAsunto, string sMensajeTexto)
        {
            string sMensaje = "";

            string sTitulo = "";
            switch (oTransaccion.TipoDocumento_Id)
            {
                #region 02 - Factura 

                case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:

                    sTitulo = "Factura N°";
                    break;

                #endregion

                #region 01 - Boleta 

                case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:

                    sTitulo = "Boleta N°";
                    break;

                #endregion

                #region 19 - Nota de Crédito

                case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:

                    sTitulo = "Nota Credito N°";
                    break;

                #endregion

                #region 49 - Nota de Debito

                case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:

                    sTitulo = "Nota Debito N°";
                    break;

                    #endregion
            }

            sMensaje +=
                        "<html>" +
                        "<div id='contenido' style='margin-left: 20px;	margin-top: 10px;	margin-right: 50px;	margin-bottom: 0px;	width: 100%; height: 100%;' >";
            sMensaje += "<h3>Estimado cliente "+oTransaccion.sCliente+ "</h3>";
            sMensaje += "<p>" + sMensajeTexto + "</p>";
            sMensaje += "<p>Puede consultar sus facturas a través de nuestro servicio www.hormiguita.pe/consultar</p>";
            sMensaje += "<p>Gracias por su preferencia.</p>";
            sMensaje += "<hr>" +
                         "<span style='font-size:10px; color:gray'; font-style: italic; >" +
                         "La siguiente información contenida en este mensaje es confidencial y de uso exclusivo del Cliente. " +
                         "Su divulgación, copia y/o adulteración están prohibidas y sólo debe ser conocida por la persona a quien se dirige este mensaje." +
                         " Si Ud. ha recibido este mensaje por error por favor proceda a eliminarlo y notificar al remitente. " +
                         "</span>" +
                         "<br/>" +
                         "<span style='font-size:10px; color:gray'; font-family:courier>" +
                         "Servicio de Facturación Electrónica brindado por Felipe Marroquín S.A.C, si desea contratar este servicio comuníquese con nosotros en: www.felipemarroquin.com " +
                         "</span>" +
                         "</div>" +
                        "</html>";

            //<span style="font-size: 11.0pt; font-family: &quot;Calibri&quot;,sans-serif; color: #1F3864; mso-fareast-language: EN-US"><!-- o ignored --></span>
            //<span style="font-size: 10.0pt; color: gray">La siguiente información contenida en este mensaje es confidencial y de uso exclusivo del Cliente. Su divulgación, copia y/o adulteración están prohibidas y sólo debe ser conocida por la persona a quien se dirige este mensaje. Si Ud. ha recibido este mensaje por error por favor proceda a eliminarlo y notificar al remitente. <!-- o ignored --></span>
            var dctParametrosCl = new Dictionary<string, object>();

            if (oTransaccion.sTraEmail.Length > 0)
                dctParametrosCl.Add("sDestinatario", "vinievema@gmail.com;" + oEmpresa.sEmpEmail + ";" + oTransaccion.sTraEmail);
            else
                dctParametrosCl.Add("sDestinatario", "vinievema@gmail.com;" + oEmpresa.sEmpEmail);


            dctParametrosCl.Add("sAsunto", oEmpresa.sEmpNombre + " - " + sAsunto);
            dctParametrosCl.Add("sEmpresa", oEmpresa.sEmpNombre+" ");
            dctParametrosCl.Add("sContenido", sMensaje);
            dctParametrosCl.Add("sArchivosAdjuntos", sRutaPDF);
            dctParametrosCl.Add("sCorreoRespuesta", oEmpresa.sEmpEmail);

            TransaccionAdo.Instance.EnviarCorreo(dctParametrosCl);
        }


        /// <summary>
        /// envia el correo con el diseño generico sin empresa dueña
        /// </summary>
        /// <param name="sRutaPDF"></param>
        /// <param name="oEmpresa"></param>
        /// <param name="oTransaccion"></param>
        /// <param name="sAsunto"></param>
        /// <param name="sMensajeTexto"></param>
        public void EnviarCorreoGenerico(string sRutaPDF, Empresa oEmpresa, Transaccion oTransaccion, string sAsunto, string sMensajeTexto)
        {
            string sMensaje = "";

            string sTitulo = "";
            switch (oTransaccion.TipoDocumento_Id)
            {
                #region 02 - Factura 

                case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:

                    sTitulo = "Factura N°";
                    break;

                #endregion

                #region 01 - Boleta 

                case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:

                    sTitulo = "Boleta N°";
                    break;

                #endregion

                #region 19 - Nota de Crédito

                case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:

                    sTitulo = "Nota Credito N°";
                    break;

                #endregion

                #region 49 - Nota de Debito

                case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:

                    sTitulo = "Nota Debito N°";
                    break;

                    #endregion
            }

            sMensaje +=
                        "<html>" +
                        "<div id='contenido' style='margin-left: 20px;	margin-top: 10px;	margin-right: 50px;	margin-bottom: 0px;	width: 100%; height: 100%;' >";
            sMensaje += "<h3>Estimado cliente " + oTransaccion.sCliente + "</h3>";
            sMensaje += "<p>" + sMensajeTexto + "</p>";
            sMensaje += "<p>Gracias por su preferencia!!!</p>";
            sMensaje += "<hr>" +
                         "<span style='font-size:10px; color:gray'; font-style: italic; >" +
                         "La siguiente información contenida en este mensaje es confidencial y de uso exclusivo del Cliente. " +
                         "Su divulgación, copia y/o adulteración están prohibidas y sólo debe ser conocida por la persona a quien se dirige este mensaje." +
                         " Si Ud. ha recibido este mensaje por error por favor proceda a eliminarlo y notificar al remitente. " +
                         "</span>" +
                         "<br/>" +
                         "</div>" +
                        "</html>";

            var dctParametrosCl = new Dictionary<string, object>();

            if (oTransaccion.sTraEmail.Length > 0)
                dctParametrosCl.Add("sDestinatario", "vinievema@gmail.com;" + oEmpresa.sEmpEmail + ";" + oTransaccion.sTraEmail);
            else
                dctParametrosCl.Add("sDestinatario", "vinievema@gmail.com;" + oEmpresa.sEmpEmail);


            dctParametrosCl.Add("sAsunto", oEmpresa.sEmpNombre + " - " + sAsunto);
            dctParametrosCl.Add("sEmpresa", oEmpresa.sEmpNombre + " ");
            dctParametrosCl.Add("sContenido", sMensaje);
            dctParametrosCl.Add("sArchivosAdjuntos", sRutaPDF);
            dctParametrosCl.Add("sCorreoRespuesta", oEmpresa.sEmpEmail);

            TransaccionAdo.Instance.EnviarCorreo(dctParametrosCl);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="oTransaccion"></param>
        /// <param name="oEmpresa"></param>
        public void EnviarCorreoBaja(Transaccion oTransaccion, Empresa oEmpresa)
        {
            string strDescripcionAnulacion_correo = string.Empty;

            #region Enviar Correo de Anulacion al Cliente

            #region TIPO DE DOCUMENTO PARA CONSTRUIR EL MENSAJE DE CORREO SUNAT

            switch (oTransaccion.TipoDocumento_Id)
            {
                #region 02 - Factura - > sunat 01

                case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:
                    strDescripcionAnulacion_correo = "FACTURA";
                    break;

                #endregion

                #region 01 - Boleta - > sunat 03

                case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:
                    strDescripcionAnulacion_correo = "BOLETA";
                    break;

                #endregion

                #region 19 - Nota de Crédito - > sunat 07

                case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:
                    strDescripcionAnulacion_correo = "NOTA DE CREDITO";
                    break;

                #endregion

                #region 49 - Nota de Debito - > sunat 08

                case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:
                    strDescripcionAnulacion_correo = "NOTA DE DEBITO";
                    break;

                    #endregion
            }

            #endregion

            string sMensajeTexto = "Estimado Cliente: " + oTransaccion.sCliente +
                                   " su comprobante electrónico " + strDescripcionAnulacion_correo +
                                   " con serie " + oTransaccion.sTraSerie +
                                   " y número " + oTransaccion.sTraNumero +
                                   " ha sido Anulado y notificado en Comunicación de Baja a Sunat";

            TransaccionNeg.Instance.EnviarCorreo(string.Empty,
                oEmpresa,
                oTransaccion,
                "Anulacion de comprobante con baja en Sunat",
                sMensajeTexto
                );

            #endregion


        }


        public void EnviarCorreoBajaGenerico(Transaccion oTransaccion, Empresa oEmpresa)
        {
            string strDescripcionAnulacion_correo = string.Empty;

            #region Enviar Correo de Anulacion al Cliente

            #region TIPO DE DOCUMENTO PARA CONSTRUIR EL MENSAJE DE CORREO SUNAT

            switch (oTransaccion.TipoDocumento_Id)
            {
                #region 02 - Factura - > sunat 01

                case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:
                    strDescripcionAnulacion_correo = "FACTURA";
                    break;

                #endregion

                #region 01 - Boleta - > sunat 03

                case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:
                    strDescripcionAnulacion_correo = "BOLETA";
                    break;

                #endregion

                #region 19 - Nota de Crédito - > sunat 07

                case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:
                    strDescripcionAnulacion_correo = "NOTA DE CREDITO";
                    break;

                #endregion

                #region 49 - Nota de Debito - > sunat 08

                case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:
                    strDescripcionAnulacion_correo = "NOTA DE DEBITO";
                    break;

                    #endregion
            }

            #endregion

            string sMensajeTexto = "Estimado Cliente: " + oTransaccion.sCliente +
                                   " su comprobante electrónico " + strDescripcionAnulacion_correo +
                                   " con serie " + oTransaccion.sTraSerie +
                                   " y número " + oTransaccion.sTraNumero +
                                   " ha sido Anulado y notificado en Comunicación de Baja a Sunat";

            TransaccionNeg.Instance.EnviarCorreoGenerico(string.Empty,
                oEmpresa,
                oTransaccion,
                "Anulacion de comprobante con baja en Sunat",
                sMensajeTexto
                );

            #endregion


        }


























        #endregion
    }
}
