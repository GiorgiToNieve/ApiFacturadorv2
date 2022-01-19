using Entidades.Comercial;
using Entidades.Maestros;
using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Utilitarios;
using Ionic.Zip;
using Negocio.Comercial;
using Negocio.Maestros;

namespace Sunat
{
    public class Servicio : DocumentoElectronico
    {
        #region Variables

        private string RUTA_REPOSITORIO_ELECTRONICO_FACTURAS = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_FACTURAS;
        private string RUTA_REPOSITORIO_ELECTRONICO_RESP = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_FACTURAS_RESPUESTA;

        private string RUTA_REPOSITORIO_ELECTRONICO_BAJAS = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_BAJAS;
        private string RUTA_REPOSITORIO_ELECTRONICO_BAJAS_RESP = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_BAJAS_RESPUESTA;

        private string RUTA_REPOSITORIO_ELECTRONICO_BOLETAS = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_BOLETAS;
        /// <summary>
        /// ruta de las respuestas de las subidas del resumen de Boletas Electronicas
        /// </summary>
        private string RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_BOLETAS_RESPUESTA;


        private string RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_CREDITO;
        /// <summary>
        /// ruta de las respuesta de la nota de credito
        /// </summary>
        private string RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO_RESP = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_CREDITO_RESPUESTA;

        
        private string RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_DEBITO;
        private string RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO_RESP = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_DEBITO_RESPUESTA;

        private string RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_GUIA_REMISION;
        private string RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION_RESP = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_GUIA_REMISION_RESPUESTA;


		private string RUTA_REPOSITORIO_DOCUMENTO_ELECTRONICO_LIQUIDACION_COMPRA = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_LIQUIDACION_COMPRA;
		private string RUTA_REPOSITORIO_DOCUMENTO_ELECTRONICO_LIQUIDACION_COMPRA_RESP = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_LIQUIDACION_COMPRA_RESPUESTA;


		/// <summary>
		/// RUTA DE RESPUESTA DE LOS TICKETS DE CONSULTA DE LA SUNAT
		/// </summary>
		private string RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_TICKETS_RESPUESTA_RESP = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_TICKETS_RESPUESTA;

        XmlDocument xdocCR = new XmlDocument();
        
        #endregion

        #region Enviar Archivo Subida Sunat

        /// <summary>
        /// [vnieve] Envia a Sunat por el servicio web y obtiene una respuesta
        /// true: enviado correctamente y su respuesta positiva
        /// false: enviado pero incorrecto
        /// Nota: actualiza el campo nTVeEstadoTransaccionElectronica =1
        /// si el envio ha sido aceptado en sunat
        /// </summary>
        /// <param name="oTransaccion"></param>
        /// <param name="oEmpresa"></param>
        public bool EnviarArchivo(Transaccion oTransaccion, Empresa oEmpresa)
        {
            try
            {
                #region Variables

                string RUTA_ZIP = string.Empty;

                string strNombreArchivoXml = GenerarNombreXML(oTransaccion, oEmpresa);
                string strNombreArchivoZip = strNombreArchivoXml.Trim('\\') + ".zip";
                
                RUTA_ZIP = ObtenerRuta(oTransaccion.TipoDocumento_Id,oEmpresa, strNombreArchivoXml);

                byte[] byContenidoArchivo = File.ReadAllBytes(RUTA_ZIP);
                byte[] byRespuestaArchivo = null;

                ServicePointManager.UseNagleAlgorithm = true;
                ServicePointManager.Expect100Continue = false;
                ServicePointManager.CheckCertificateRevocationList = true;

                #endregion

                if (oEmpresa.nEmpProduccion == Enumerador.ESTADO_ACTIVO)
                {
                    #region Sesion de Producccion

                    var wServiceClient = new Sunat.wsServicioSunat_Produccion.billServiceClient(Enumerador.SERVICIO_SUNAT);
                    var behavior = new PasswordDigestBehavior(string.Concat(oEmpresa.sEmpRuc,oEmpresa.sEmpUsuarioFE), oEmpresa.sEmpPasswordFE);
                    wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
                    byRespuestaArchivo = wServiceClient.sendBill(strNombreArchivoZip, byContenidoArchivo, string.Empty);

                    #endregion

                }
                else
                {
                    #region Sesion de Pruebas

                    var wServiceClient = new wsServicioSunat_beta.billServiceClient(Enumerador.SERVICIO_SUNAT_PRUEBAS);
                    var behavior = new PasswordDigestBehavior(string.Concat(oEmpresa.sEmpRuc, oEmpresa.sEmpUsuarioFE), oEmpresa.sEmpPasswordFE);
                    wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
                    byRespuestaArchivo = wServiceClient.sendBill(strNombreArchivoZip, byContenidoArchivo, string.Empty);

                    #endregion

                }

                #region Descomprimir archivo zip de CDR SUNAT

                /*2.Respuesta del servicio*/
                string GuardarRespuesta = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESP + "\\" + "R-" + strNombreArchivoZip;
                File.WriteAllBytes(GuardarRespuesta, byRespuestaArchivo);

                string strRespuestaSUNATxml = string.Empty;
                strRespuestaSUNATxml = ObtenerRutaRespuestaSunat(oTransaccion.TipoDocumento_Id, oEmpresa, strNombreArchivoXml);

                using (var zip = new ZipFile(GuardarRespuesta))
                {
                    if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA)
                    {
                        //Asegurando que haya un solo documento y que no genere error de existencia
                        Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta+RUTA_REPOSITORIO_ELECTRONICO_RESP),
                            delegate (string path)
                            {
                                if (path != null &&
                                    path.Equals(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESP + "\\" + "R-" +
                                                strNombreArchivoXml.Trim('\\') + ".xml"))
                                    File.Delete(path);
                            });

                        if (File.Exists(strRespuestaSUNATxml))
                        {
                            File.Delete(strRespuestaSUNATxml);
                        }

                        zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESP + "\\");
                    }
					else
						if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
					{
						//Asegurando que haya un solo documento y que no genere error de existencia
						Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP),
							delegate (string path)
							{
								if (path != null &&
									path.Equals(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "\\" + "R-" +
												strNombreArchivoXml.Trim('\\') + ".xml"))
									File.Delete(path);
							});

						if (File.Exists(strRespuestaSUNATxml))
						{
							File.Delete(strRespuestaSUNATxml);
						}

						zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "\\");
					}
					else
                        if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
                    {
                        //Asegurando que haya un solo documento y que no genere error de existencia
                        Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO_RESP),
                            delegate (string path)
                            {
                                if (path != null &&
                                    path.Equals(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO_RESP + "\\" + "R-" +
                                                strNombreArchivoXml.Trim('\\') + ".xml"))
                                    File.Delete(path);
                            });

                        if (File.Exists(strRespuestaSUNATxml))
                        {
                            File.Delete(strRespuestaSUNATxml);
                        }

                        zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO_RESP + "\\");
                    }
                    else
                        if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO)
                    {
                        //Asegurando que haya un solo documento y que no genere error de existencia
                        Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO_RESP),
                            delegate (string path)
                            {
                                if (path != null &&
                                    path.Equals(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO_RESP + "\\" + "R-" +
                                                strNombreArchivoXml.Trim('\\') + ".xml"))
                                    File.Delete(path);
                            });

                        if (File.Exists(strRespuestaSUNATxml))
                        {
                            File.Delete(strRespuestaSUNATxml);
                        }

                        zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO_RESP + "\\");
                    }
                    else
                        if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION)
                    {
                        //Asegurando que haya un solo documento y que no genere error de existencia
                        Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION_RESP),
                            delegate (string path)
                            {
                                if (path != null &&
                                    path.Equals(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION_RESP + "\\" + "R-" +
                                                strNombreArchivoXml.Trim('\\') + ".xml"))
                                    File.Delete(path);
                            });

                        if (File.Exists(strRespuestaSUNATxml))
                        {
                            File.Delete(strRespuestaSUNATxml);
                        }

                        zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION_RESP + "\\");
                    }

					else
						if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_LIQUIDACION_COMPRA)
					{
						//Asegurando que haya un solo documento y que no genere error de existencia
						Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_DOCUMENTO_ELECTRONICO_LIQUIDACION_COMPRA_RESP),
							delegate (string path)
							{
								if (path != null &&
									path.Equals(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_DOCUMENTO_ELECTRONICO_LIQUIDACION_COMPRA_RESP + "\\" + "R-" +
												strNombreArchivoXml.Trim('\\') + ".xml"))
									File.Delete(path);
							});

						if (File.Exists(strRespuestaSUNATxml))
						{
							File.Delete(strRespuestaSUNATxml);
						}

						zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_DOCUMENTO_ELECTRONICO_LIQUIDACION_COMPRA_RESP + "\\");
					}


				}
                #endregion

                #region Verificar Respuesta
                /*3. Verificar Respuesta*/
                if (VerificarRespuesta(strRespuestaSUNATxml, oTransaccion.Transaccion_Id))
				{
					ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id, 
                        (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_SUNAT);

					return true;
                }
                #endregion

                //No se ha podido enviar
                return false;
            }
            catch (Exception ex)
            {
				ActualizarExcepcionDocumentoElectronico(oTransaccion, ex);
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
                return false;
            }
        }


		public bool EnviarArchivoOSE(Transaccion oTransaccion, Empresa oEmpresa)
		{
			try
			{
				#region Variables

				string strNombreArchivoXml = GenerarNombreXML(oTransaccion, oEmpresa);
				string strNombreArchivoZip = strNombreArchivoXml.Trim('\\') + ".zip";

				string Username = string.Empty;
				string Password = string.Empty;

				string RUTA_ZIP = string.Empty;

				RUTA_ZIP = ObtenerRuta(oTransaccion.TipoDocumento_Id, oEmpresa, strNombreArchivoXml);

				byte[] byContenidoArchivo = File.ReadAllBytes(RUTA_ZIP);
				byte[] byRespuestaArchivo = null;

				ServicePointManager.UseNagleAlgorithm = true;
				ServicePointManager.Expect100Continue = false;
				ServicePointManager.CheckCertificateRevocationList = true;

				#region Credenciales
				Username = oEmpresa.sEmpUsuarioFE.Trim();
				Password = oEmpresa.sEmpPasswordFE.Trim();
				#endregion


				#endregion

				if (oEmpresa.nEmpProduccion == Enumerador.ESTADO_ACTIVO)
				{
					#region Sesion de Login de Producccion

					var wServiceClient = new wsServicioOSE_Produccion.BillServiceClient(Enumerador.SERVICIO_OSE_PRODUCCION);
					var behavior = new PasswordDigestBehavior(Username, Password);
					wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
					byRespuestaArchivo = wServiceClient.sendBill(strNombreArchivoZip, byContenidoArchivo);

					#endregion
				}
				else
				{
					#region Sesion de Login de Pruebas
					
					var wServiceClient = new wsServicioOSE_beta.BillServiceClient(Enumerador.SERVICIO_OSE_PRUEBAS);
					var behavior = new PasswordDigestBehavior(Username, Password);
					wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
					byRespuestaArchivo = wServiceClient.sendBill(strNombreArchivoZip, byContenidoArchivo);

					#endregion
					
				}

				string GuardarRespuesta = ObtenerRutaRespuestaOSE(oTransaccion.TipoDocumento_Id, oEmpresa, strNombreArchivoZip);

				File.WriteAllBytes(GuardarRespuesta, byRespuestaArchivo);

				string strRespuestaSUNATxml = string.Empty;
				strRespuestaSUNATxml = ObtenerRutaRespuestaSunat(oTransaccion.TipoDocumento_Id, oEmpresa, strNombreArchivoXml);

				ExtraerRespuesta(oTransaccion.TipoDocumento_Id, strNombreArchivoXml,oEmpresa, GuardarRespuesta);

				#region VERIFICAR

				if (VerificarRespuestaOSE(strRespuestaSUNATxml, oTransaccion.Transaccion_Id))
				{
					ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id,
						(int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_SUNAT);

					return true;
				}

				#endregion

				//No se ha podido enviar
				return false;
			}
			catch (Exception ex)
			{
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				return false;
			}
		}

		private void ExtraerRespuesta(int TipoDocumento_Id, string strNombreArchivoXml,Empresa oEmpresa, string GuardarRespuesta)
		{
			#region EXTRAER RESPUESTAS

			using (var zip = new ZipFile(GuardarRespuesta))
			{

				if (TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA)
				{
					//Asegurando que haya un solo documento y que no genere error de existencia

				   Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESP),
						delegate (string path)
						{
							if (path != null &&
								path.Equals(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESP + "\\" + "R-" +
											strNombreArchivoXml.Trim('\\') + ".xml"))
								File.Delete(path);
						});

					zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESP + "\\");
				}
				else
					if (TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
				{
					//Asegurando que haya un solo documento y que no genere error de existencia
					Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP),
						delegate (string path)
						{
							if (path != null &&
								path.Equals(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "\\" + "R-" +
											strNombreArchivoXml.Trim('\\') + ".xml"))
								File.Delete(path);
						});

					zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "\\");
				}
				else
						if (TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
				{
					//Asegurando que haya un solo documento y que no genere error de existencia
					Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO_RESP),
						delegate (string path)
						{
							if (path != null &&
								path.Equals(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO_RESP + "\\" + "R-" +
											strNombreArchivoXml.Trim('\\') + ".xml"))
								File.Delete(path);
						});

					zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO_RESP + "\\");
				}
				else
							if (TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO)
				{
					//Asegurando que haya un solo documento y que no genere error de existencia
					Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO_RESP),
						delegate (string path)
						{
							if (path != null &&
								path.Equals(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO_RESP + "\\" + "R-" +
											strNombreArchivoXml.Trim('\\') + ".xml"))
								File.Delete(path);
						});

					zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO_RESP + "\\");
				}
				else
							if (TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION)
				{
					//Asegurando que haya un solo documento y que no genere error de existencia
					Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION_RESP),
						delegate (string path)
						{
							if (path != null &&
								path.Equals(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION_RESP + "\\" + "R-" +
											strNombreArchivoXml.Trim('\\') + ".xml"))
								File.Delete(path);
						});

					zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION_RESP + "\\");
				}
			}

			#endregion
		}

		/// <summary>
		/// enviar guias de remision electronicas
		/// </summary>
		/// <param name="oTransaccion"></param>
		/// <param name="oEmpresa"></param>
		/// <returns></returns>
		public bool EnviarArchivoGuiaRemision(Transaccion oTransaccion, Empresa oEmpresa)
        {
            try
            {
                #region Variables

                string RUTA_ZIP = string.Empty;

                string strNombreArchivoXml = GenerarNombreXML(oTransaccion, oEmpresa);
                string strNombreArchivoZip = strNombreArchivoXml.Trim('\\') + ".zip";

                RUTA_ZIP = ObtenerRuta(oTransaccion.TipoDocumento_Id, oEmpresa, strNombreArchivoXml);

                byte[] byContenidoArchivo = File.ReadAllBytes(RUTA_ZIP);
                byte[] byRespuestaArchivo = null;

                ServicePointManager.UseNagleAlgorithm = true;
                ServicePointManager.Expect100Continue = false;
                ServicePointManager.CheckCertificateRevocationList = true;

                #endregion

                if (oEmpresa.nEmpProduccion == Enumerador.ESTADO_ACTIVO)
                {
                    #region Sesion de Producccion

                    var wServiceClient = new Sunat.wsServicioSunat_ProduccionGR.billServiceClient(Enumerador.SERVICIO_SUNATGR);
                    var behavior = new PasswordDigestBehavior(string.Concat(oEmpresa.sEmpRuc, oEmpresa.sEmpUsuarioFE), oEmpresa.sEmpPasswordFE);
                    wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
                    byRespuestaArchivo = wServiceClient.sendBill(strNombreArchivoZip, byContenidoArchivo, string.Empty);

                    #endregion

                }
                else
                {
                    #region Sesion de Pruebas

                    var wServiceClient = new Sunat.BillServicePortBetaGR.billServiceClient(Enumerador.SERVICIO_SUNAT_PRUEBASGR);
                    var behavior = new PasswordDigestBehavior(string.Concat(oEmpresa.sEmpRuc, oEmpresa.sEmpUsuarioFE), oEmpresa.sEmpPasswordFE);
                    wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
                    byRespuestaArchivo = wServiceClient.sendBill(strNombreArchivoZip, byContenidoArchivo, string.Empty);

                    #endregion

                }

                #region Descomprimir archivo zip de CDR SUNAT

                /*2.Respuesta del servicio*/
                string GuardarRespuesta = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION_RESP + "\\" + "R-" + strNombreArchivoZip;
                File.WriteAllBytes(GuardarRespuesta, byRespuestaArchivo);

                string strRespuestaSUNATxml = string.Empty;
                strRespuestaSUNATxml = ObtenerRutaRespuestaSunat(oTransaccion.TipoDocumento_Id, oEmpresa, strNombreArchivoXml);

                using (var zip = new ZipFile(GuardarRespuesta))
                {
                        //Asegurando que haya un solo documento y que no genere error de existencia
                        Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION_RESP),
                            delegate (string path)
                            {
                                if (path != null &&
                                    path.Equals(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION_RESP + "\\" + "R-" +
                                                strNombreArchivoXml.Trim('\\') + ".xml"))
                                    File.Delete(path);
                            });

                        if (File.Exists(strRespuestaSUNATxml))
                        {
                            File.Delete(strRespuestaSUNATxml);
                        }

                        zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION_RESP + "\\");

                }
				#endregion

				#region Verificar Respuesta
				/*3. Verificar Respuesta*/
				if (VerificarRespuesta(strRespuestaSUNATxml, oTransaccion.Transaccion_Id))
				{
                    ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id,
                        (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_SUNAT);

                    return true;
                }
                #endregion

                //No se ha podido enviar
                return false;
            }
            catch (Exception ex)
            {
				ActualizarExcepcionDocumentoElectronico(oTransaccion, ex);
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
                return false;
            }
        }

        private string ObtenerRutaRespuestaSunat(int tipoDocumento_Id, Empresa oEmpresa, string pstrNombreArchivoXml)
        {
            string strRespuestaSUNATxml = string.Empty;

            if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA)
            {
                strRespuestaSUNATxml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESP + "\\" + "R-" + pstrNombreArchivoXml.Trim('\\') + ".xml";
            }
			else
					if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
			{
				strRespuestaSUNATxml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "\\" + "R-" + pstrNombreArchivoXml.Trim('\\') + ".xml";
			}
			else
                    if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
            {
                strRespuestaSUNATxml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO_RESP + "\\" + "R-" + pstrNombreArchivoXml.Trim('\\') + ".xml";
            }
            else
                    if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO)
            {
                strRespuestaSUNATxml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO_RESP + "\\" + "R-" + pstrNombreArchivoXml.Trim('\\') + ".xml";
            }
            else
                    if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION)
            {
                strRespuestaSUNATxml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION_RESP + "\\" + "R-" + pstrNombreArchivoXml.Trim('\\') + ".xml";
            }

			else
					if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_LIQUIDACION_COMPRA)
			{
				strRespuestaSUNATxml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_DOCUMENTO_ELECTRONICO_LIQUIDACION_COMPRA_RESP + "\\" + "R-" + pstrNombreArchivoXml.Trim('\\') + ".xml";
			}



			return strRespuestaSUNATxml;
        }


		/// <summary>
		/// consulta y guarda la rspuesta del ticket de sunat
		/// </summary>
		/// <param name="oEmpresa"></param>
		/// <param name="sTraTicket"></param>
		/// <returns></returns>
		public bool ConsultarTicket(Empresa oEmpresa, string sTraTicket)
		{
			
			#region Variables
			byte[] byRespuestaArchivo = null;
			ServicePointManager.UseNagleAlgorithm = true;
			ServicePointManager.Expect100Continue = false;
			ServicePointManager.CheckCertificateRevocationList = true;
			string strRespuestaSUNATxml = "";
			#endregion


			string strNombreArchivo = oEmpresa.sEmpRuc + "_" + sTraTicket;
			string strNombreArchivo1 = strNombreArchivo.Trim('\\') + ".zip";


			#region Servicios
			/*1.llamar al servicio*/
			if (oEmpresa.nEmpProduccion == Enumerador.ESTADO_ACTIVO)
			{
				var wServiceClient = new Sunat.wsServicioSunat_Produccion.billServiceClient(Enumerador.SERVICIO_SUNAT);
				var behavior = new PasswordDigestBehavior(string.Concat(oEmpresa.sEmpRuc, oEmpresa.sEmpUsuarioFE), oEmpresa.sEmpPasswordFE);
				wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
				byRespuestaArchivo = wServiceClient.getStatus(sTraTicket).content;
			}
			else
			{
				var wServiceClient = new Sunat.wsServicioSunat_beta.billServiceClient(Enumerador.SERVICIO_SUNAT_PRUEBAS);
				var behavior = new PasswordDigestBehavior(string.Concat(oEmpresa.sEmpRuc, oEmpresa.sEmpUsuarioFE), oEmpresa.sEmpPasswordFE);
				wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
				byRespuestaArchivo = wServiceClient.getStatus(sTraTicket).content;
			}
			#endregion


			#region Respuesta

			string GuardarRespuesta = oEmpresa.sEmpRuta + RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_TICKETS_RESPUESTA_RESP + "\\" + "R-" + strNombreArchivo1;
			File.WriteAllBytes(GuardarRespuesta, byRespuestaArchivo);

			using (var zip = new ZipFile(GuardarRespuesta))
			{
				//Asegurando que haya un solo documento y que no genere error de existencia
				Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_TICKETS_RESPUESTA_RESP),
					delegate (string path)
					{
						if (path != null &&
							path.Equals(oEmpresa.sEmpRuta + RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_TICKETS_RESPUESTA_RESP + "\\" + "R-" +
										strNombreArchivo.Trim('\\') + ".xml"))
							File.Delete(path);
					});

				if (File.Exists(strRespuestaSUNATxml))
				{
					File.Delete(strRespuestaSUNATxml);
				}

				zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_TICKETS_RESPUESTA_RESP + "\\");
			}

			strRespuestaSUNATxml = oEmpresa.sEmpRuta + RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_TICKETS_RESPUESTA_RESP + "\\" + "R-" +
								   strNombreArchivo.Trim('\\') + ".xml";

			#endregion


			#region Verificar Respuesta
			/*3. Verificar Respuesta*/
			if (VerificarRespuesta(strRespuestaSUNATxml))
			{
				return true;
			}
			#endregion



			return false;
		}

		public bool ConsultarTicketOSE(Empresa p_oEmpresa, string sTraTicket)
		{
			//byte[] byRespuestaArchivo = null;

			ServicePointManager.UseNagleAlgorithm = true;
			ServicePointManager.Expect100Continue = false;
			ServicePointManager.CheckCertificateRevocationList = true;

			return false;
		}

		private string ObtenerRutaRespuestaOSE(int tipoDocumento_Id, Empresa oEmpresa, string pstrNombreArchivoXml)
		{
			string strRespuestaSUNATxml = string.Empty;

			if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA)
			{
				strRespuestaSUNATxml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESP + "\\" + "R-" + pstrNombreArchivoXml;
			}
			else
					if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
			{
				strRespuestaSUNATxml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "\\" + "R-" + pstrNombreArchivoXml;
			}
			else
					if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
			{
				strRespuestaSUNATxml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO_RESP + "\\" + "R-" + pstrNombreArchivoXml;
			}
			else
					if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO)
			{
				strRespuestaSUNATxml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO_RESP + "\\" + "R-" + pstrNombreArchivoXml;
			}
			else
					if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION)
			{
				strRespuestaSUNATxml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION_RESP + "\\" + "R-" + pstrNombreArchivoXml;
			}

			else
					if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_LIQUIDACION_COMPRA)
			{
				strRespuestaSUNATxml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_DOCUMENTO_ELECTRONICO_LIQUIDACION_COMPRA_RESP + "\\" + "R-" + pstrNombreArchivoXml;
			}


			return strRespuestaSUNATxml;
		}

		private string ObtenerRuta(int tipoDocumento_Id, Empresa oEmpresa,string pstrNombreArchivoXml)
        {

            string RUTA_ZIP_ = string.Empty;

            if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA)
            {
                RUTA_ZIP_ = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_FACTURAS +
                                   pstrNombreArchivoXml + @"\" +
                                   pstrNombreArchivoXml + ".zip";
            }
			else

				   if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
			{
				RUTA_ZIP_ = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS +
						   pstrNombreArchivoXml + @"\" +
						   pstrNombreArchivoXml + ".zip";
			}
			else

                   if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
            {
                RUTA_ZIP_ = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO +
                           pstrNombreArchivoXml + @"\" +
                           pstrNombreArchivoXml + ".zip";
            }
            else

                   if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO)
            {
                RUTA_ZIP_ = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO +
                           pstrNombreArchivoXml + @"\" +
                           pstrNombreArchivoXml + ".zip";
            }
            else

                   if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION)
            {
                RUTA_ZIP_ = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION +
                           pstrNombreArchivoXml + @"\" +
                           pstrNombreArchivoXml + ".zip";
            }


			else

				   if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_LIQUIDACION_COMPRA)
			{
				RUTA_ZIP_ = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_DOCUMENTO_ELECTRONICO_LIQUIDACION_COMPRA +
						   pstrNombreArchivoXml + @"\" +
						   pstrNombreArchivoXml + ".zip";
			}


			return RUTA_ZIP_;
        }

        #endregion


        /// <summary>
        /// status: 1: Activo; 2: Modificaco; 3:Baja
        /// por default mandamos activo
        /// </summary>
        /// <param name="LstTraBoletas"></param>
        /// <param name="oEmpresa"></param>
        /// <returns></returns>

        public bool EnviarResumenBoletasSunat(List<Transaccion> LstTraBoletas, Empresa oEmpresa, string status="1")
        {
            try
            {
                #region Variables
                string respuesta = string.Empty;
                string strRespuestaSUNATxml = string.Empty;
                string strNombreArchivo = GenerarArchivoXmlyZipResumenBoletas(LstTraBoletas, oEmpresa,status);
                GuardaryComprimirXML(oEmpresa.sEmpRuta+RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + @"\" + strNombreArchivo);

                string RUTA_ZIP = oEmpresa.sEmpRuta+RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + @"\" + strNombreArchivo + ".zip";

                var byContenidoArchivo = File.ReadAllBytes(RUTA_ZIP);
                byte[] byRespuestaArchivo = null;

                string strNombreArchivo1 = strNombreArchivo.Trim('\\') + ".zip";

                ServicePointManager.UseNagleAlgorithm = true;
                ServicePointManager.Expect100Continue = false;
                ServicePointManager.CheckCertificateRevocationList = true;

                #endregion

                #region Call Service
                /*1.llamar al servicio*/
                if (oEmpresa.nEmpProduccion == Enumerador.ESTADO_ACTIVO)
                {
                    var wServiceClient = new Sunat.wsServicioSunat_Produccion.billServiceClient(Enumerador.SERVICIO_SUNAT);
                    var behavior = new PasswordDigestBehavior(string.Concat(oEmpresa.sEmpRuc,oEmpresa.sEmpUsuarioFE),oEmpresa.sEmpPasswordFE);

                    wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
                    respuesta = wServiceClient.sendSummary(strNombreArchivo1, byContenidoArchivo, string.Empty);
                    byRespuestaArchivo = wServiceClient.getStatus(respuesta).content;
                }
                else
                {
                    var wServiceClient = new Sunat.wsServicioSunat_beta.billServiceClient(Enumerador.SERVICIO_SUNAT_PRUEBAS);
                    var behavior = new PasswordDigestBehavior(string.Concat(oEmpresa.sEmpRuc,oEmpresa.sEmpUsuarioFE), oEmpresa.sEmpPasswordFE);
                    wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
                    respuesta = wServiceClient.sendSummary(strNombreArchivo1, byContenidoArchivo, string.Empty);
                    byRespuestaArchivo = wServiceClient.getStatus(respuesta).content;
                }
                #endregion

                #region Gestionar Respuesta

                string GuardarRespuesta = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "\\" + "R-" + strNombreArchivo1;
                File.WriteAllBytes(GuardarRespuesta, byRespuestaArchivo);

                using (var zip = new ZipFile(GuardarRespuesta))
                {
                    //Asegurando que haya un solo documento y que no genere error de existencia
                    Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP),
                        delegate (string path)
                        {
                            var test = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "" + "R-" +
                                       strNombreArchivo.Trim('\\') + ".xml";

                            if (path != null && path.Equals(test))
                                File.Delete(path);
                        });

                    if (File.Exists(strRespuestaSUNATxml))
                    {
                        File.Delete(strRespuestaSUNATxml);
                    }

                    zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "\\");
                }

                strRespuestaSUNATxml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "\\" + "R-" + strNombreArchivo.Trim('\\') + ".xml";

                #endregion

                //Verificar si la repuesta es exitosa, si es 0 el codigo es ok , si tiene error se termina el proceso

                bool rpta = false;
				int Transaccion_id = 0;

				if (LstTraBoletas.Count == 1)
				{
					Transaccion_id = LstTraBoletas.FirstOrDefault().Transaccion_Id;
				}


				if (VerificarRespuesta(strRespuestaSUNATxml, Transaccion_id))
				{
                    if (status == "3")
                    {
                        ActualizarDocumentoElectronico(LstTraBoletas, (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_BAJA_ENVIADA_SUNAT);
                    }
                    else
                    {
                        ActualizarDocumentoElectronico(LstTraBoletas, (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_SUNAT);

                        #region GENERAR ARCHIVO XML DE LAS BOLETAS EN FORMATO DE FACTURA
                        try
                        {
                            foreach (var oTraBoleta in LstTraBoletas)
                            {
                                GenerarArchivoBoletaXml(oTraBoleta, oEmpresa);
                            }
                        }
                        catch
                        {
                        }
                        
                        #endregion
                    }

                    rpta = true;
                }

                return rpta;

            }
            catch (Exception ex)
            {
				ActualizarExcepcionDocumentoElectronico(LstTraBoletas.FirstOrDefault(), ex);
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
                return false;
            }

        }

		public bool EnviarResumenBoletasSunatOSE(List<Transaccion> LstTraBoletas, Empresa oEmpresa, string status = "1")
		{
			try
			{
				#region Variables
				string respuesta = string.Empty;
				string strRespuestaSUNATxml = string.Empty;
				string Username = string.Empty;
				string Password = string.Empty;

				string strNombreArchivo = GenerarArchivoXmlyZipResumenBoletas(LstTraBoletas, oEmpresa, status);
				GuardaryComprimirXML(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + @"\" + strNombreArchivo);

				string RUTA_ZIP = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + @"\" + strNombreArchivo + ".zip";

				var byContenidoArchivo = File.ReadAllBytes(RUTA_ZIP);
				byte[] byRespuestaArchivo = null;

				string strNombreArchivo1 = strNombreArchivo.Trim('\\') + ".zip";

				ServicePointManager.UseNagleAlgorithm = true;
				ServicePointManager.Expect100Continue = false;
				ServicePointManager.CheckCertificateRevocationList = true;

				#region Credenciales
				Username = oEmpresa.sEmpUsuarioFE;
				Password = oEmpresa.sEmpPasswordFE;
				#endregion

				#endregion

				#region Call Service
				/*1.llamar al servicio*/
				if (oEmpresa.nEmpProduccion == Enumerador.ESTADO_ACTIVO)
				{
					#region Sesion de Produccion

					var wServiceClient = new wsServicioOSE_Produccion.BillServiceClient(Enumerador.SERVICIO_OSE_PRODUCCION);
					var behavior = new
						PasswordDigestBehavior(Username, Password);
					wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
					wServiceClient.Endpoint.Binding.OpenTimeout = new TimeSpan(0, 5, 0);
					wServiceClient.Endpoint.Binding.CloseTimeout = new TimeSpan(0, 5, 0);
					wServiceClient.Endpoint.Binding.SendTimeout = new TimeSpan(0, 5, 0);
					wServiceClient.Endpoint.Binding.ReceiveTimeout = new TimeSpan(0, 5, 0);

					respuesta = wServiceClient.sendSummary(strNombreArchivo1, byContenidoArchivo);
					byRespuestaArchivo = wServiceClient.getStatus(respuesta).content;

					#endregion
				}
				else
				{

					#region Sesion de Pruebas

					var wServiceClient = new wsServicioOSE_beta.BillServiceClient(Enumerador.SERVICIO_OSE_PRUEBAS);
					var behavior = new
						PasswordDigestBehavior(Username, Password);
					wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
					respuesta = wServiceClient.sendSummary(strNombreArchivo1, byContenidoArchivo);
					byRespuestaArchivo = wServiceClient.getStatus(respuesta).content;

					#endregion

				}
				#endregion

				#region Gestionar Respuesta

				string GuardarRespuesta = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "\\" + "R-" + strNombreArchivo1;
				File.WriteAllBytes(GuardarRespuesta, byRespuestaArchivo);

				using (var zip = new ZipFile(GuardarRespuesta))
				{
					//Asegurando que haya un solo documento y que no genere error de existencia
					Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP),
						delegate (string path)
						{
							var test = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "" + "R-" +
									   strNombreArchivo.Trim('\\') + ".xml";

							if (path != null && path.Equals(test))
								File.Delete(path);
						});

					if (File.Exists(strRespuestaSUNATxml))
					{
						File.Delete(strRespuestaSUNATxml);
					}

					zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "\\");
				}

				strRespuestaSUNATxml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "\\" + "R-" + strNombreArchivo.Trim('\\') + ".xml";

				#endregion

				//Verificar si la repuesta es exitosa, si es 0 el codigo es ok , si tiene error se termina el proceso

				bool rpta = false;

				if (VerificarRespuestaOSE(strRespuestaSUNATxml))
				{
					if (status == "3")
					{
						ActualizarDocumentoElectronico(LstTraBoletas, (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_BAJA_ENVIADA_SUNAT);
					}
					else
					{
						ActualizarDocumentoElectronico(LstTraBoletas, (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_SUNAT);
						#region GENERAR ARCHIVO XML DE LAS BOLETAS EN FORMATO DE FACTURA
						//try
						//{
						//	foreach (var oTraBoleta in LstTraBoletas)
						//	{
						//		GenerarArchivoBoletaXml(oTraBoleta, oEmpresa);
						//	}
						//}
						//catch
						//{
						//}

						#endregion
					}

					rpta = true;
				}

				return rpta;

			}
			catch (Exception ex)
			{
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				return false;
			}

		}

		public bool EnviarResumenNotaCreditoSunat(Transaccion oTransaccionNCBoleta, Empresa oEmpresa, string status = "1")
		{
			try
			{
				var LstTraBoletas = new List<Transaccion>();
				LstTraBoletas.Add(oTransaccionNCBoleta);

				#region Variables
				string respuesta = string.Empty;
				string strRespuestaSUNATxml = string.Empty;
				string strNombreArchivo = GenerarArchivoXmlyZipResumenBoletas(LstTraBoletas, oEmpresa, status,Enumerador.ESTADO_ACTIVO);
				GuardaryComprimirXML(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + @"\" + strNombreArchivo);

				string RUTA_ZIP = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + @"\" + strNombreArchivo + ".zip";

				var byContenidoArchivo = File.ReadAllBytes(RUTA_ZIP);
				byte[] byRespuestaArchivo = null;

				string strNombreArchivo1 = strNombreArchivo.Trim('\\') + ".zip";

				ServicePointManager.UseNagleAlgorithm = true;
				ServicePointManager.Expect100Continue = false;
				ServicePointManager.CheckCertificateRevocationList = true;

				#endregion

				#region Call Service
				/*1.llamar al servicio*/
				if (oEmpresa.nEmpProduccion == Enumerador.ESTADO_ACTIVO)
				{
					var wServiceClient = new Sunat.wsServicioSunat_Produccion.billServiceClient(Enumerador.SERVICIO_SUNAT);
					var behavior = new PasswordDigestBehavior(string.Concat(oEmpresa.sEmpRuc, oEmpresa.sEmpUsuarioFE), oEmpresa.sEmpPasswordFE);

					wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
					respuesta = wServiceClient.sendSummary(strNombreArchivo1, byContenidoArchivo, string.Empty);
					byRespuestaArchivo = wServiceClient.getStatus(respuesta).content;
				}
				else
				{
					var wServiceClient = new Sunat.wsServicioSunat_beta.billServiceClient(Enumerador.SERVICIO_SUNAT_PRUEBAS);
					var behavior = new PasswordDigestBehavior(string.Concat(oEmpresa.sEmpRuc, oEmpresa.sEmpUsuarioFE), oEmpresa.sEmpPasswordFE);
					wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
					respuesta = wServiceClient.sendSummary(strNombreArchivo1, byContenidoArchivo, string.Empty);
					byRespuestaArchivo = wServiceClient.getStatus(respuesta).content;
				}
				#endregion

				#region Gestionar Respuesta

				string GuardarRespuesta = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "\\" + "R-" + strNombreArchivo1;
				File.WriteAllBytes(GuardarRespuesta, byRespuestaArchivo);

				using (var zip = new ZipFile(GuardarRespuesta))
				{
					//Asegurando que haya un solo documento y que no genere error de existencia
					Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP),
						delegate (string path)
						{
							var test = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "" + "R-" +
									   strNombreArchivo.Trim('\\') + ".xml";

							if (path != null && path.Equals(test))
								File.Delete(path);
						});

					if (File.Exists(strRespuestaSUNATxml))
					{
						File.Delete(strRespuestaSUNATxml);
					}

					zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "\\");
				}

				strRespuestaSUNATxml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "\\" + "R-" + strNombreArchivo.Trim('\\') + ".xml";

				#endregion

				//Verificar si la repuesta es exitosa, si es 0 el codigo es ok , si tiene error se termina el proceso

				bool rpta = false;

				if (VerificarRespuesta(strRespuestaSUNATxml, oTransaccionNCBoleta.Transaccion_Id))
				{
					if (status == "3")
					{
						ActualizarDocumentoElectronico(LstTraBoletas, (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_BAJA_ENVIADA_SUNAT);
					}
					else
					{
						ActualizarDocumentoElectronico(LstTraBoletas, (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_SUNAT);

						#region GENERAR ARCHIVO XML DE LAS BOLETAS EN FORMATO DE FACTURA
						try
						{
							//foreach (var oTraBoleta in LstTraBoletas)
							//{
							//	if(oTraBoleta.TipoDocumento_Id== (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
							//	GenerarArchivoBoletaXml(oTraBoleta, oEmpresa);
							//	else
							//	{
							//		GenerarArchivoXmlyZip(oTraBoleta, oEmpresa);
							//	}
							//}
						}
						catch
						{
						}

						#endregion

					}

					rpta = true;
				}

				return rpta;

			}
			catch (Exception ex)
			{
				ActualizarExcepcionDocumentoElectronico(oTransaccionNCBoleta, ex);
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				return false;
			}

		}

		public bool EnviarResumenNotaCreditoSunatOSE(Transaccion oTransaccionNCBoleta, Empresa oEmpresa, string status = "1")
		{
			try
			{
				var LstTraBoletas = new List<Transaccion>();
				LstTraBoletas.Add(oTransaccionNCBoleta);

				#region Variables

				string respuesta = string.Empty;
				string strRespuestaSUNATxml = string.Empty;
				string Username = string.Empty;
				string Password = string.Empty;
				string strNombreArchivo = GenerarArchivoXmlyZipResumenBoletas(LstTraBoletas, oEmpresa, status, Enumerador.ESTADO_ACTIVO);
				GuardaryComprimirXML(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + @"\" + strNombreArchivo);

				string RUTA_ZIP = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + @"\" + strNombreArchivo + ".zip";

				var byContenidoArchivo = File.ReadAllBytes(RUTA_ZIP);
				byte[] byRespuestaArchivo = null;

				string strNombreArchivo1 = strNombreArchivo.Trim('\\') + ".zip";

				ServicePointManager.UseNagleAlgorithm = true;
				ServicePointManager.Expect100Continue = false;
				ServicePointManager.CheckCertificateRevocationList = true;

				#region Credenciales
				Username = oEmpresa.sEmpUsuarioFE;
				Password = oEmpresa.sEmpPasswordFE;
				#endregion

				#endregion

				#region Call Service
				/*1.llamar al servicio*/
				if (oEmpresa.nEmpProduccion == Enumerador.ESTADO_ACTIVO)
				{
					#region Sesion de Produccion

					var wServiceClient = new wsServicioOSE_Produccion.BillServiceClient(Enumerador.SERVICIO_OSE_PRODUCCION);
					var behavior = new
						PasswordDigestBehavior(Username, Password);
					wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
					wServiceClient.Endpoint.Binding.OpenTimeout = new TimeSpan(0, 5, 0);
					wServiceClient.Endpoint.Binding.CloseTimeout = new TimeSpan(0, 5, 0);
					wServiceClient.Endpoint.Binding.SendTimeout = new TimeSpan(0, 5, 0);
					wServiceClient.Endpoint.Binding.ReceiveTimeout = new TimeSpan(0, 5, 0);
					respuesta = wServiceClient.sendSummary(strNombreArchivo1, byContenidoArchivo);
					byRespuestaArchivo = wServiceClient.getStatus(respuesta).content;

					#endregion
				}
				else
				{

					#region Sesion de Pruebas

					var wServiceClient = new wsServicioOSE_beta.BillServiceClient(Enumerador.SERVICIO_OSE_PRUEBAS);
					var behavior = new
						PasswordDigestBehavior(Username, Password);
					wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
					respuesta = wServiceClient.sendSummary(strNombreArchivo1, byContenidoArchivo);
					byRespuestaArchivo = wServiceClient.getStatus(respuesta).content;

					#endregion
					
				}
				#endregion

				#region Gestionar Respuesta

				string GuardarRespuesta = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "\\" + "R-" + strNombreArchivo1;
				File.WriteAllBytes(GuardarRespuesta, byRespuestaArchivo);

				using (var zip = new ZipFile(GuardarRespuesta))
				{
					//Asegurando que haya un solo documento y que no genere error de existencia
					Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP),
						delegate (string path)
						{
							var test = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "" + "R-" +
									   strNombreArchivo.Trim('\\') + ".xml";

							if (path != null && path.Equals(test))
								File.Delete(path);
						});

					if (File.Exists(strRespuestaSUNATxml))
					{
						File.Delete(strRespuestaSUNATxml);
					}

					zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "\\");
				}

				strRespuestaSUNATxml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESUMEN_BOLETAS_RESP + "\\" + "R-" + strNombreArchivo.Trim('\\') + ".xml";

				#endregion

				//Verificar si la repuesta es exitosa, si es 0 el codigo es ok , si tiene error se termina el proceso

				bool rpta = false;

				if (VerificarRespuestaOSE(strRespuestaSUNATxml))
				{
					if (status == "3")
					{
						ActualizarDocumentoElectronico(LstTraBoletas, (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_BAJA_ENVIADA_SUNAT);
					}
					else
					{
						ActualizarDocumentoElectronico(LstTraBoletas, (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_SUNAT);

						#region GENERAR ARCHIVO XML DE LAS BOLETAS EN FORMATO DE FACTURA
						try
						{
							//foreach (var oTraBoleta in LstTraBoletas)
							//{
							//	if(oTraBoleta.TipoDocumento_Id== (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
							//	GenerarArchivoBoletaXml(oTraBoleta, oEmpresa);
							//	else
							//	{
							//		GenerarArchivoXmlyZip(oTraBoleta, oEmpresa);
							//	}
							//}
						}
						catch
						{
						}

						#endregion

					}

					rpta = true;
				}

				return rpta;

			}
			catch (Exception ex)
			{
				ActualizarExcepcionDocumentoElectronico(oTransaccionNCBoleta, ex);
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				return false;
			}

		}


		#region Anular Archivo Bajas

		/// <summary>
		/// crea el xml de baja lo comprime y lo envia a sunat
		/// </summary>
		/// <param name="oTransaccion"></param>
		/// <param name="oEmpresa"></param>
		/// <returns></returns>
		public bool AnularArchivo(Transaccion oTransaccion, Empresa oEmpresa)
        {

			string strNombreArchivo = string.Empty;
			string strRuta = string.Empty;

			try
            {
                #region Variables

                string sFechaCabecera = string.Empty;
                string sFechaFormato = string.Empty;
                //para ser llenado en el xml
                string sNombre_Doc_RA = string.Empty;
                sFechaFormato = oTransaccion.dTraFecha.ToString("yyyyMMdd");
                sFechaCabecera = oTransaccion.dTraFecha.ToString("yyyy-MM-dd");
                string strDescripcionAnulacion_correo = string.Empty;

                #endregion

                #region Obtener Correlativo

                var Parametros = new Dictionary<string, object>();
                Parametros.Add("Empresa_Id", oEmpresa.Empresa_Id);
                Parametros.Add("TipoDocumento_Id", (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BAJA_DOC_ELECTRONICO_SUNAT);

                var Lista = TicketNeg.Instance.Consultar(Parametros);
                //el nuevo numero correlativo aumentado en 1

                if (Lista == null || Lista.Count <= 0) throw new Exception("No se Encuentra el Correlativo de BAJAS");

                var oTicket = Lista.FirstOrDefault();
                int nCorrelativoBajas = oTicket.nTicUltimoNumero;
                nCorrelativoBajas = nCorrelativoBajas + 1;

                #endregion

                #region Nombre del archivo de bajas

                //formar el nombre del arcvhivo
                strNombreArchivo = oEmpresa.sEmpRuc + "-RA-" + sFechaFormato + "-" + nCorrelativoBajas;

                //exclusivo para el documento xml se necesita en una parte
                sNombre_Doc_RA = "RA-" + sFechaFormato + "-" + nCorrelativoBajas;

                #endregion

                #region Creacion de la ruta del archivo

                strRuta = oEmpresa.sEmpRuta+RUTA_REPOSITORIO_ELECTRONICO_BAJAS + strNombreArchivo;

				#endregion

				#region Actualizar Correlativo
				//el correlativo siempre se aumenta para que no haya conflicto por si no se responde en sunat
				TicketNeg.Instance.AumentarCorrelativo(oTicket.Ticket_Id, oTicket.nTicUltimoNumero);
				#endregion


				#region Crear  yEscribir el xml de bajada

				var Wrt = new XmlTextWriter(strRuta + ".xml", Encoding.GetEncoding("ISO-8859-1"));
                Wrt.Formatting = Formatting.Indented;

                GenerarArchivoXmlBajas(Wrt, sNombre_Doc_RA, sFechaCabecera, oTransaccion, oEmpresa);

                #endregion

                #region Incrustar Certificado Digital

                CertificadoBajas(strRuta, oEmpresa);

                #endregion

                #region Guardar y Comprimir Ruta del Archivo de Baja

                GuardaryComprimirXML(strRuta);

				#endregion

				

				#region Gestionar Resultados y Enviar Baja

				bool b_Resultado = false;

				if (oEmpresa.nEmpAfilidadoOSE == Enumerador.ESTADO_ACTIVO)
					b_Resultado = EnviarBajaSunatOSE(strNombreArchivo, strRuta, oEmpresa, oTransaccion);
				else
					b_Resultado = EnviarBajaSunat(strNombreArchivo, strRuta, oEmpresa, oTransaccion);

				if (b_Resultado)
                {
                    #region Actualizar Transaccion

                    ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id, (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_BAJA_ENVIADA_SUNAT);

                    #endregion

                    return true;
                }

                return false;

                #endregion
            }
			catch (Exception ex)
			{
				int contar = 0;
				//nuevo: si en la transaccion existe un numero de ticket entonces debemos volver a consultar ese ticket
				//
				var parametros = new Dictionary<string, object>();
				parametros.Add("Transaccion_Id", oTransaccion.Transaccion_Id);
				var result = TransaccionNeg.Instance.Consultar(parametros);

				oTransaccion = result.FirstOrDefault();

				if (oTransaccion != null && oTransaccion.sTraNroTicket.Length > 0)
				{
				volverTicket:
					if (ConsultarTicketBajaSunat(strNombreArchivo, strRuta, oEmpresa, oTransaccion))
					{

						#region Actualizar Transaccion

						ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id, (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_BAJA_ENVIADA_SUNAT);

						#endregion

						return true;

					}
					else
					{
						contar++;
						if (contar <= 5)
							goto volverTicket;

					}
				}

				ActualizarExcepcionDocumentoElectronico(oTransaccion, ex);
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				return false;
			}
		}



		private bool ConsultarTicketBajaSunat(string strNombreArchivo, string strRuta, Empresa oEmpresa, Transaccion oTransaccion = null)
		{
			try
			{
				#region Variables

				string filename = strRuta + ".zip";
				string respuesta = string.Empty;
				string strRespuestaSUNATxml = string.Empty;
				string strNombreArchivo1 = strNombreArchivo.Trim('\\') + ".zip";


				byte[] byRespuestaArchivo = null;

				ServicePointManager.UseNagleAlgorithm = true;
				ServicePointManager.Expect100Continue = false;
				ServicePointManager.CheckCertificateRevocationList = true;
				int contar = 0;
				#endregion

				respuesta = oTransaccion.sTraNroTicket;

				/*1.llamar al servicio*/
				if (oEmpresa.nEmpProduccion == Enumerador.ESTADO_ACTIVO)
				{
					#region Sesion de Produccion

					var wServiceClient = new Sunat.wsServicioSunat_Produccion.billServiceClient(Enumerador.SERVICIO_SUNAT);
					var behavior = new PasswordDigestBehavior(string.Concat(oEmpresa.sEmpRuc, oEmpresa.sEmpUsuarioFE), oEmpresa.sEmpPasswordFE);
					wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);

				ValidarTicketProd:
					byRespuestaArchivo = wServiceClient.getStatus(respuesta).content;

					//validando que se obtenga una respuesta
					if (respuesta.Length > 0 && byRespuestaArchivo == null && contar <= 5)
					{
						contar++;
						goto ValidarTicketProd;
					}

					#endregion

				}
				else
				{
					#region Sesion de Pruebas

					var wServiceClient = new Sunat.wsServicioSunat_beta.billServiceClient(Enumerador.SERVICIO_SUNAT_PRUEBAS);
					var behavior = new PasswordDigestBehavior(string.Concat(oEmpresa.sEmpRuc, oEmpresa.sEmpUsuarioFE), oEmpresa.sEmpPasswordFE);
					wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);

				ValidarTicket:
					byRespuestaArchivo = wServiceClient.getStatus(respuesta).content;

					//validando que se obtenga una respuesta
					if (respuesta.Length > 0 && byRespuestaArchivo == null && contar <= 5)
					{
						contar++;
						goto ValidarTicket;
					}

					#endregion
				}

				#region Respuesta

				string GuardarRespuesta = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BAJAS_RESP + "\\" + "R-" + strNombreArchivo1;

				if (File.Exists(GuardarRespuesta))
				{
					File.Delete(GuardarRespuesta);
				}

				File.WriteAllBytes(GuardarRespuesta, byRespuestaArchivo);

				using (var zip = new ZipFile(GuardarRespuesta))
				{
					//Asegurando que haya un solo documento y que no genere error de existencia
					Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BAJAS_RESP),
						delegate (string path)
						{
							if (path != null &&
								path.Equals(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BAJAS_RESP + "\\" + "R-" +
											strNombreArchivo.Trim('\\') + ".xml"))
								File.Delete(path);
						});

					if (File.Exists(strRespuestaSUNATxml))
					{
						File.Delete(strRespuestaSUNATxml);
					}

					zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BAJAS_RESP + "\\");
				}

				strRespuestaSUNATxml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BAJAS_RESP + "\\" + "R-" +
									   strNombreArchivo.Trim('\\') + ".xml";

				#endregion

				int Transaccion_id = 0;

				if (oTransaccion != null && oTransaccion.Transaccion_Id > 0)
				{
					Transaccion_id = oTransaccion.Transaccion_Id;
				}

				return VerificarRespuesta(strRespuestaSUNATxml, Transaccion_id);

			}
			catch (Exception ex)
			{
				ActualizarExcepcionDocumentoElectronico(oTransaccion, ex);
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				return false;
			}
		}




		#endregion

		#region Enviar Archivo de Bajada Sunat

		/// <summary>
		/// [vnieve] Envia a Sunat por el servicio web y obtiene una respuesta
		/// </summary>
		/// <param name="strNombreArchivo"></param>
		/// <param name="strRuta"></param>
		/// <param name="oEmpresa"></param>
		/// <returns></returns>
		private bool EnviarBajaSunat(string strNombreArchivo, string strRuta, Empresa oEmpresa, Transaccion oTransaccion = null)
		{
            try
            {
                #region Variables

                string filename = strRuta + ".zip";
                string respuesta = string.Empty;
                string strRespuestaSUNATxml = string.Empty;
                string strNombreArchivo1 = strNombreArchivo.Trim('\\') + ".zip";

                byte[] byContenidoArchivo = File.ReadAllBytes(filename);
                byte[] byRespuestaArchivo = null;

                ServicePointManager.UseNagleAlgorithm = true;
                ServicePointManager.Expect100Continue = false;
                ServicePointManager.CheckCertificateRevocationList = true;
				int contar = 0;
				#endregion

				/*1.llamar al servicio*/
				if (oEmpresa.nEmpProduccion == Enumerador.ESTADO_ACTIVO)
                {
                    #region Sesion de Produccion

                    var wServiceClient = new Sunat.wsServicioSunat_Produccion.billServiceClient(Enumerador.SERVICIO_SUNAT);
					var behavior = new PasswordDigestBehavior(string.Concat(oEmpresa.sEmpRuc, oEmpresa.sEmpUsuarioFE), oEmpresa.sEmpPasswordFE);
					wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
                    respuesta = wServiceClient.sendSummary(strNombreArchivo1, byContenidoArchivo, string.Empty);
					GuardarNroTicket(oTransaccion.Transaccion_Id, respuesta);


				    ValidarTicketProd:
					byRespuestaArchivo = wServiceClient.getStatus(respuesta).content;

					//validando que se obtenga una respuesta
					if (respuesta.Length > 0 && byRespuestaArchivo == null && contar <= 5)
					{
						contar++;
						goto ValidarTicketProd;
					}

					#endregion

				}
                else
                {
                    #region Sesion de Pruebas

                    var wServiceClient = new Sunat.wsServicioSunat_beta.billServiceClient(Enumerador.SERVICIO_SUNAT_PRUEBAS);
					var behavior = new PasswordDigestBehavior(string.Concat(oEmpresa.sEmpRuc, oEmpresa.sEmpUsuarioFE), oEmpresa.sEmpPasswordFE);
					wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
                    respuesta = wServiceClient.sendSummary(strNombreArchivo1, byContenidoArchivo, string.Empty);
					GuardarNroTicket(oTransaccion.Transaccion_Id, respuesta);

				   ValidarTicket:
					byRespuestaArchivo = wServiceClient.getStatus(respuesta).content;

					//validando que se obtenga una respuesta
					if (respuesta.Length > 0 && byRespuestaArchivo == null && contar <= 5)
					{
						contar++;
						goto ValidarTicket;
					}

					#endregion
				}

                #region Respuesta

                string GuardarRespuesta = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BAJAS_RESP + "\\" + "R-" + strNombreArchivo1;
                File.WriteAllBytes(GuardarRespuesta, byRespuestaArchivo);

                using (var zip = new ZipFile(GuardarRespuesta))
                {
                    //Asegurando que haya un solo documento y que no genere error de existencia
                    Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta+RUTA_REPOSITORIO_ELECTRONICO_BAJAS_RESP),
                        delegate (string path)
                        {
                            if (path != null &&
                                path.Equals(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BAJAS_RESP + "\\" + "R-" +
                                            strNombreArchivo.Trim('\\') + ".xml"))
                                File.Delete(path);
                        });

                    if (File.Exists(strRespuestaSUNATxml))
                    {
                        File.Delete(strRespuestaSUNATxml);
                    }

                    zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BAJAS_RESP + "\\");
                }

                strRespuestaSUNATxml = oEmpresa.sEmpRuta+ RUTA_REPOSITORIO_ELECTRONICO_BAJAS_RESP + "\\" + "R-" +
                                       strNombreArchivo.Trim('\\') + ".xml";

				#endregion

				//Verificar si la repuesta es exitosa, si es 0 el codigo es ok , si tiene error se termina el proceso

				int Transaccion_id = 0;

				if (oTransaccion != null && oTransaccion.Transaccion_Id > 0)
				{
					Transaccion_id = oTransaccion.Transaccion_Id;
				}


				//Verificar si la repuesta es exitosa, si es 0 el codigo es ok , si tiene error se termina el proceso
				return VerificarRespuestaBajas(strRespuestaSUNATxml, Transaccion_id);

			}
            catch (Exception)
            {
                throw;
            }
        }


		private bool EnviarBajaSunatOSE(string strNombreArchivo, string strRuta, Empresa oEmpresa, Transaccion oTransaccion = null)
		{
			try
			{
				#region Variables

				string filename = strRuta + ".zip";
				string respuesta = string.Empty;
				string strRespuestaSUNATxml = string.Empty;
				string strNombreArchivo1 = strNombreArchivo.Trim('\\') + ".zip";

				string Username = string.Empty;
				string Password = string.Empty;

				byte[] byContenidoArchivo = File.ReadAllBytes(filename);
				byte[] byRespuestaArchivo = null;

				ServicePointManager.UseNagleAlgorithm = true;
				ServicePointManager.Expect100Continue = false;
				ServicePointManager.CheckCertificateRevocationList = true;
				int contar = 0;
				#region Credenciales
				Username = oEmpresa.sEmpUsuarioFE;
				Password = oEmpresa.sEmpPasswordFE;
				#endregion

				#endregion

				#region Call Services
				/*1.llamar al servicio*/
				if (oEmpresa.nEmpProduccion == Enumerador.ESTADO_ACTIVO)
				{
					#region Sesion de Produccion

					var wServiceClient = new wsServicioOSE_Produccion.
						BillServiceClient(Enumerador.SERVICIO_OSE_PRODUCCION);
					var behavior = new
						PasswordDigestBehavior(Username, Password);
					wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
					wServiceClient.Endpoint.Binding.OpenTimeout = new TimeSpan(0, 5, 0);
					wServiceClient.Endpoint.Binding.CloseTimeout = new TimeSpan(0, 5, 0);
					wServiceClient.Endpoint.Binding.SendTimeout = new TimeSpan(0, 5, 0);
					wServiceClient.Endpoint.Binding.ReceiveTimeout = new TimeSpan(0, 5, 0);
					
					respuesta = wServiceClient.sendSummary(strNombreArchivo1, byContenidoArchivo);
					GuardarNroTicket(oTransaccion.Transaccion_Id, respuesta);

				   ValidarTicketProd:
					byRespuestaArchivo = wServiceClient.getStatus(respuesta).content;

					 //validando que se obtenga una respuesta
					 if (respuesta.Length > 0 && byRespuestaArchivo == null && contar <= 5)
					 {
						contar++;
						goto ValidarTicketProd;
					 }

					#endregion

				}
				else
				{
					#region Sesion de Pruebas

					var wServiceClient = new wsServicioOSE_beta.BillServiceClient(Enumerador.SERVICIO_OSE_PRUEBAS);
					var behavior = new
						PasswordDigestBehavior(Username, Password);
					wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
					
					respuesta = wServiceClient.sendSummary(strNombreArchivo1, byContenidoArchivo);
					GuardarNroTicket(oTransaccion.Transaccion_Id, respuesta);


				    ValidarTicket:
					byRespuestaArchivo = wServiceClient.getStatus(respuesta).content;

					//validando que se obtenga una respuesta
					if (respuesta.Length > 0 && byRespuestaArchivo == null && contar <= 5)
					{
						contar++;
						goto ValidarTicket;
					}

					#endregion
				}
				#endregion

				#region Respuesta

				string GuardarRespuesta = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BAJAS_RESP + "\\" + "R-" + strNombreArchivo1;
				File.WriteAllBytes(GuardarRespuesta, byRespuestaArchivo);

				using (var zip = new ZipFile(GuardarRespuesta))
				{
					//Asegurando que haya un solo documento y que no genere error de existencia
					Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BAJAS_RESP),
						delegate (string path)
						{
							if (path != null &&
								path.Equals(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BAJAS_RESP + "\\" + "R-" +
											strNombreArchivo.Trim('\\') + ".xml"))
								File.Delete(path);
						});

					if (File.Exists(strRespuestaSUNATxml))
					{
						File.Delete(strRespuestaSUNATxml);
					}

					zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BAJAS_RESP + "\\");
				}

				strRespuestaSUNATxml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BAJAS_RESP + "\\" + "R-" +
									   strNombreArchivo.Trim('\\') + ".xml";

				#endregion


				//Verificar si la repuesta es exitosa, si es 0 el codigo es ok , si tiene error se termina el proceso

				int Transaccion_id = 0;

				if (oTransaccion != null && oTransaccion.Transaccion_Id > 0)
				{
					Transaccion_id = oTransaccion.Transaccion_Id;
				}

				return VerificarRespuestaBajas(strRespuestaSUNATxml, Transaccion_id);

			}
			catch (Exception)
			{
				throw;
			}
		}


		#endregion

		#region Verificar Respuesta

		/// <summary>
		/// [vnieve]
		/// true: OK
		/// false: No ha sido posible por algún error
		/// </summary>
		/// <param name="sRutaRespuesta"></param>
		/// <returns></returns>
		private bool VerificarRespuesta(string sRutaRespuesta, int Transaccion_Id = 0)
		{
            try
            {
            var xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(sRutaRespuesta);

            //Traer el Tag xml del documento con el nombre DocumentResponse
            var DocumentResponse = xmlDoc.GetElementsByTagName("cac:DocumentResponse");

            //Capturar el primer resultado de la respuesta
            var lista = ((XmlElement)DocumentResponse[0]).GetElementsByTagName("cac:Response");

            string strRespuestaCodigo = string.Empty;
            string strDescripcion = string.Empty;

            foreach (XmlElement nodo in lista)
            {
                const int i = 0;
                var ResponseCode = nodo.GetElementsByTagName("cbc:ResponseCode");
                strRespuestaCodigo = ResponseCode[i].InnerText;

				//MODIFICADO PARA GUARDAR LA DESCRIPCION DEL XML DE SUNAT
				var DescriptionXML = nodo.GetElementsByTagName("cbc:Description");

				if (DescriptionXML[i] != null)
				{
					strDescripcion = DescriptionXML[i].InnerText;
				}

			}

				//para tener documentado en base la aceptacion del documento
				ActualizarValidacionConRespuesta(Transaccion_Id, strDescripcion, strRespuestaCodigo);

			//0-> Correcto en el xml
			if (strRespuestaCodigo == "0")
            {
                return true;
            }

            //mandar a error
            return false;

            }
            catch (Exception ex)
            {
                LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
                return false;
            }
        }


		private bool VerificarRespuestaOSE(string sRutaRespuesta, int Transaccion_Id = 0)
		{
			try
			{
				var xmlDoc = new XmlDocument();
				xmlDoc.PreserveWhitespace = true;
				xmlDoc.Load(sRutaRespuesta);

				//Traer el Tag xml del documento con el nombre DocumentResponse
				var DocumentResponse = xmlDoc.GetElementsByTagName("ns3:DocumentResponse");

				//Capturar el primer resultado de la respuesta
				var lista = ((XmlElement)DocumentResponse[0]).GetElementsByTagName("ns3:Response");

				string strRespuestaCodigo = string.Empty;
				string strDescripcion = string.Empty;

				foreach (XmlElement nodo in lista)
				{
					const int i = 0;
					var ResponseCode = nodo.GetElementsByTagName("ResponseCode");
					strRespuestaCodigo = ResponseCode[i].InnerText;

					//MODIFICADO PARA GUARDAR LA DESCRIPCION DEL XML DE SUNAT
					var DescriptionXML = nodo.GetElementsByTagName("cbc:Description");

					if (DescriptionXML[i] != null)
					{
						strDescripcion = DescriptionXML[i].InnerText;
					}
				}

				//para tener documentado en base la aceptacion del documento
				ActualizarValidacionConRespuesta(Transaccion_Id, strDescripcion, strRespuestaCodigo);

				//0-> Correcto en el xml
				if (strRespuestaCodigo == "0")
				{
					return true;
				}

				//mandar a error
				return false;

			}
			catch (Exception ex)
			{
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				return false;
			}
		}



		private bool VerificarRespuestaBajas(string sRutaRespuesta, int Transaccion_Id = 0)
		{
			try
			{
				var xmlDoc = new XmlDocument();
				xmlDoc.PreserveWhitespace = true;
				xmlDoc.Load(sRutaRespuesta);

				//Traer el Tag xml del documento con el nombre DocumentResponse
				var DocumentResponse = xmlDoc.GetElementsByTagName("cac:DocumentResponse");

				//Capturar el primer resultado de la respuesta
				var lista = ((XmlElement)DocumentResponse[0]).GetElementsByTagName("cac:Response");

				string strRespuestaCodigo = string.Empty;
				string strDescripcion = string.Empty;

				foreach (XmlElement nodo in lista)
				{
					const int i = 0;
					var ResponseCode = nodo.GetElementsByTagName("cbc:ResponseCode");
					strRespuestaCodigo = ResponseCode[i].InnerText;

					//MODIFICADO PARA GUARDAR LA DESCRIPCION DEL XML DE SUNAT
					var DescriptionXML = nodo.GetElementsByTagName("cbc:Description");

					if (DescriptionXML[i] != null)
					{
						strDescripcion = DescriptionXML[i].InnerText;
					}

				}

				//para tener documentado en base la aceptacion del documento
				ActualizarValidacionConRespuesta(Transaccion_Id, strDescripcion, strRespuestaCodigo);

				//0-> Correcto en el xml
				if (strRespuestaCodigo == "0")
				{
					var ParametrosValor = new Dictionary<string, object>();
					ParametrosValor.Add("nTraEstadoTransaccionElectronica", (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_BAJA_ENVIADA_SUNAT);
					string mensaje = " Sistema: Enviado a Baja Sunat(Actualización Automática), " + " de ser necesario verificar en portal Sunat.";
					ParametrosValor.Add("sTraRespuestaValidacion", mensaje);

					var ParametrosId = new Dictionary<string, object>();
					ParametrosId.Add("Transaccion_Id", Transaccion_Id);
					TransaccionNeg.Instance.Actualizar(ParametrosValor, ParametrosId);


					if(strDescripcion.Contains("La Comunicacion de baja"))
						if (strDescripcion.Contains("ha sido aceptada"))
						{
							//para tener documentado en base la aceptacion del documento
							ActualizarValidacionConRespuesta(Transaccion_Id, strDescripcion, strRespuestaCodigo);
						}

							return true;
				}


				//mandar a error
				return false;

			}
			catch (Exception ex)
			{
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				return false;
			}
		}



		#endregion

		#region Actualizar Documento Electronico

		/// <summary>
		/// 
		/// </summary>
		/// <param name="TransaccionVenta_Id"></param>
		/// <param name="nEstadoDoc_Electronico"></param>
		public void ActualizarDocumentoElectronico(int TransaccionVenta_Id, int nEstadoDoc_Electronico)
        {
            try
            {
                var ParametrosValor = new Dictionary<string, object>();
                ParametrosValor.Add("nTraEstadoTransaccionElectronica", nEstadoDoc_Electronico);

                var ParametrosId = new Dictionary<string, object>();
                ParametrosId.Add("Transaccion_Id", TransaccionVenta_Id);

                TransaccionNeg.Instance.Actualizar(ParametrosValor, ParametrosId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Actualiza el estado de los documento electronicos al estado recibido como parametro
        /// </summary>
        /// <param name="LstTransaccionVenta"></param>
        /// <param name="nEstadoDoc_Electronico"></param>
        public void ActualizarDocumentoElectronico(List<Transaccion> LstTransaccion, int nEstadoDoc_Electronico)
        {
            try
            {
                foreach (var oItem in LstTransaccion)
                    ActualizarDocumentoElectronico(oItem.Transaccion_Id, nEstadoDoc_Electronico);
            }
            catch (Exception ex)
            {
                LogApplicationNeg.Instance.GuardarLogAplicacion(ex);

                throw;
            }
        }

		#endregion



		public bool EnviarArchivoRutaPrueba(Empresa oEmpresa, string RutaZip="", string strNombreArchivoZip="")
		{
			try
			{
				#region Variables

				string RUTA_ZIP = RutaZip;
				

				byte[] byContenidoArchivo = File.ReadAllBytes(RUTA_ZIP);
				byte[] byRespuestaArchivo = null;

				ServicePointManager.UseNagleAlgorithm = true;
				ServicePointManager.Expect100Continue = false;
				ServicePointManager.CheckCertificateRevocationList = true;

				#endregion

				if (oEmpresa.nEmpProduccion == Enumerador.ESTADO_ACTIVO)
				{
					#region Sesion de Producccion

					var wServiceClient = new Sunat.wsServicioSunat_Produccion.billServiceClient(Enumerador.SERVICIO_SUNAT);
					var behavior = new PasswordDigestBehavior(string.Concat(oEmpresa.sEmpRuc, oEmpresa.sEmpUsuarioFE), oEmpresa.sEmpPasswordFE);
					wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
					byRespuestaArchivo = wServiceClient.sendBill(strNombreArchivoZip, byContenidoArchivo, string.Empty);

					#endregion

				}
				else
				{
					#region Sesion de Pruebas

					var wServiceClient = new wsServicioSunat_beta.billServiceClient(Enumerador.SERVICIO_SUNAT_PRUEBAS);
					var behavior = new PasswordDigestBehavior(string.Concat(oEmpresa.sEmpRuc, oEmpresa.sEmpUsuarioFE), oEmpresa.sEmpPasswordFE);
					wServiceClient.Endpoint.EndpointBehaviors.Add(behavior);
					byRespuestaArchivo = wServiceClient.sendBill(strNombreArchivoZip + ".zip", byContenidoArchivo, string.Empty);

					#endregion

				}

				#region Descomprimir archivo zip de CDR SUNAT

				/*2.Respuesta del servicio*/
				string GuardarRespuesta = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESP + "\\" + "R-" + strNombreArchivoZip + ".zip";
				File.WriteAllBytes(GuardarRespuesta, byRespuestaArchivo);

				string strRespuestaSUNATxml = string.Empty;
				strRespuestaSUNATxml = ObtenerRutaRespuestaSunat((int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA, oEmpresa, strNombreArchivoZip);

				using (var zip = new ZipFile(GuardarRespuesta))
				{
						//Asegurando que haya un solo documento y que no genere error de existencia
						Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESP),
							delegate (string path)
							{
								if (path != null &&
									path.Equals(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESP + "\\" + "R-" +
												strNombreArchivoZip.Trim('\\') + ".xml"))
									File.Delete(path);
							});

						if (File.Exists(strRespuestaSUNATxml))
						{
							File.Delete(strRespuestaSUNATxml);
						}

						zip.ExtractAll(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_RESP + "\\");

				}
				#endregion

				#region Verificar Respuesta
				/*3. Verificar Respuesta*/
				if (VerificarRespuesta(strRespuestaSUNATxml))
				{
					

					return true;
				}
				#endregion

				//No se ha podido enviar
				return false;
			}
			catch (Exception ex)
			{
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				return false;
			}
		}

		private void GuardarNroTicket(int transaccion_Id, string sTraNroTicket)
		{
			var ParametrosValor = new Dictionary<string, object>();


			if (sTraNroTicket.texto().QuitarComillas().Length > 0)
				ParametrosValor.Add("sTraNroTicket", sTraNroTicket.texto().QuitarComillas());
			else
				ParametrosValor.Add("sTraNroTicket", "No se ha obtenido un numero de ticket en el servicio EnviarBajaSunat");

			var ParametrosId = new Dictionary<string, object>();
			ParametrosId.Add("Transaccion_Id", transaccion_Id);

			TransaccionNeg.Instance.Actualizar(ParametrosValor, ParametrosId);

		}



		private void ActualizarValidacionConRespuesta(int transaccion_Id, string strDescripcion, string sTraCodigoRespuestaSunat = "")
		{
			var ParametrosValor = new Dictionary<string, object>();
			ParametrosValor.Add("sTraRespuestaValidacion", strDescripcion.texto().QuitarComillas());

			if(sTraCodigoRespuestaSunat.texto().QuitarComillas().Length>0)
			ParametrosValor.Add("sTraCodigoRespuestaSunat", sTraCodigoRespuestaSunat.texto().QuitarComillas());

			var ParametrosId = new Dictionary<string, object>();
			ParametrosId.Add("Transaccion_Id", transaccion_Id);

			TransaccionNeg.Instance.Actualizar(ParametrosValor, ParametrosId);
		}

		/// <summary>
		/// Este metodo guardara el error generado desde la excepcion para que sea visual
		/// </summary>
		/// <param name="transaccion_Id"></param>
		/// <param name="ex"></param>
		private void ActualizarExcepcionDocumentoElectronico(Transaccion oTransaccion, Exception ex)
		{
			try
			{
				string numeroDocumento = oTransaccion.sTraSerie + "-" + oTransaccion.sTraNumero;
				var ParametrosValor = new Dictionary<string, object>();
				string mensaje = ex.Message;

				if (ex.Message.Contains("El comprobante " + numeroDocumento + " fue informado anteriormente"))
				{
					//aqui se deben colocar los errores conocidos para que no vuelva a subir la factura.
					ParametrosValor.Add("nTraEstadoTransaccionElectronica",
						(int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_SUNAT);

					mensaje = mensaje + " Sistema: Enviado a Sunat(Actualización Automática), " +
						"de ser necesario verificar en portal Sunat.";
				}

				//Limpiando el mensaje de las comillas para su insercion en la BD
				mensaje = Util.QuitarComillas(mensaje).texto();

				ParametrosValor.Add("sTraRespuestaValidacion", mensaje);

				var ParametrosId = new Dictionary<string, object>();
				ParametrosId.Add("Transaccion_Id", oTransaccion.Transaccion_Id);

				TransaccionNeg.Instance.Actualizar(ParametrosValor, ParametrosId);
			}
			catch (Exception)
			{
				throw;
			}
		}


	}
}
