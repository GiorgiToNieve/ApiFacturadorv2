using Entidades.Comercial;
using Entidades.Maestros;
using Negocio.Maestros;
using Sunat;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data;
using System.Web;
using System.Web.Mvc;
using Utilitarios;
using WSApiFMSACApp.Sunat;

namespace WSApiFMSACApp.Controllers
{
    public class HomeController : Controller
    {
        Servicio wsServicio = new Servicio();
        DocumentoElectronico oDocumentoElectronico = new DocumentoElectronico();
        Sunat.PDF oPDF = new Sunat.PDF();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Descargas()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult ConsultarDocumento(string ruc,string serie,string numero,string accion)
        {
			try
			{
				string RUTA_PDF = string.Empty;
				string RUTA_XML = string.Empty;
                string RUTA_CDR = string.Empty;

				#region Validaciones de los parametros
				if (ruc.Length <= 10)
				{
					throw new Exception("La longitud del campo RUC emisor con valor ingresado " + ruc + " no es correcto.");
				}
				if (ruc.Length > 11)
				{
					throw new Exception("La longitud del campo RUC emisor con valor ingresado " + ruc + " no es correcto.");
				}

				if (serie.Length < 4)
				{
					throw new Exception("La longitud del campo Serie con valor ingresado " + serie + " no es correcto.");
				}
				if (serie.Length > 4)
				{
					throw new Exception("La longitud del campo Serie con valor ingresado " + serie + " no es correcto.");
				}

				if (numero.Length <= 0)
				{
					throw new Exception("La longitud del campo número con valor ingresado " + numero + " no es correcto.");
				}
				if (numero.Length > 11)
				{
					throw new Exception("La longitud del campo número con valor ingresado " + numero + " no es correcto.");
				}
				#endregion

				var oEmpresa = EmpresaNeg.Instance.Consultar(ruc);

				if (oEmpresa == null)
				{
					 throw new Exception("La empresa con RUC: "+ ruc +" no es correcto.");
				}

				var Parametros = new Dictionary<string, object>();
				Parametros.Add("Empresa_Id", oEmpresa.Empresa_Id);
				Parametros.Add("sTraSerie", serie);
				Parametros.Add("sTraNumero", numero);

				var Lista = Negocio.Comercial.TransaccionNeg.Instance.Consultar(Parametros);

				if (Lista != null && Lista.Count > 0)
				{
					var oTransaccion = Lista.First();

					if (oTransaccion != null && oTransaccion.Transaccion_Id>0)
					{
						if (oTransaccion.nTraEstadoTransaccionElectronica <= 0)
						{
							throw new Exception("El documento aún no ha sido enviado a sunat y su estado es: " +
								oTransaccion.nTraEstadoTransaccionElectronica);
						}

						string strNombreArchivo = oDocumentoElectronico.GenerarNombreXML(oTransaccion, oEmpresa);

						/*Aqui rehacer el PDF*/
						//bool resultado = GenerarPDF(oTransaccion, oEmpresa);

						#region Completando Rutas

						RUTA_PDF = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_FACTURAS_ELECTRONICAS_PDF;
						RUTA_XML = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_FACTURAS +
									  strNombreArchivo + @"\" +
									  strNombreArchivo + ".zip";


                        RUTA_CDR = ObtenerRutaRespuestaSunat(oTransaccion.TipoDocumento_Id, oEmpresa, strNombreArchivo);

                        if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
						{
							RUTA_PDF = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_BOLETAS_ELECTRONICAS_PDF;
							RUTA_XML = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_BOLETAS +
									  strNombreArchivo + @"\" +
									  strNombreArchivo + ".zip";
						}
						else if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
						{
							RUTA_PDF = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_NOTA_CREDITO_ELECTRONICAS_PDF;
							RUTA_XML = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_CREDITO +
									  strNombreArchivo + @"\" +
									  strNombreArchivo + ".zip";
						}
						else if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION)
						{
							RUTA_PDF = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_GUIA_REMISION_ELECTRONICAS_PDF;
							RUTA_XML = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_GUIA_REMISION +
									  strNombreArchivo + @"\" +
									  strNombreArchivo + ".zip";
						}

						#endregion

						RUTA_PDF = RUTA_PDF +
								   strNombreArchivo + @"\" +
								   strNombreArchivo + ".pdf";

						if (accion == "Descargar PDF")
							if (oPDF.ExisteRuta(RUTA_PDF))
							{
								return File(RUTA_PDF, "application/pdf", strNombreArchivo + ".pdf");
							}

						if (accion == "Descargar XML")
							if (oPDF.ExisteRuta(RUTA_XML))
							{
								return File(RUTA_XML, "application/zip", strNombreArchivo + ".zip");
							}

                        if (accion == "Descargar CDR")
                            if (oPDF.ExisteRuta(RUTA_CDR))
                            {
                                return File(RUTA_CDR, "application/zip", strNombreArchivo + ".xml");
                            }

                    }
				}
                else
                {
                    return RedirectToAction("~/Views/Shared/Error");
                }



                return RedirectToAction("Index");

                // return null;
			}
			catch (Exception ex)
			{
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				throw new Exception(ex.Message);
				//return RedirectToAction("~/Views/Shared/Error");
			}
        }

        private bool GenerarPDF(Transaccion oTransaccion, Empresa oEmpresa)
        {
            try
            {
                bool b_resultado = false;
                wsServicio.GenerarCodigoQR(oTransaccion, oEmpresa);

                string RUTA_CODIGO_HASH = string.Empty;
                string RUTA_LOGO_EMPRESA = string.Empty;

                string strNombreArchivo = oDocumentoElectronico.GenerarNombreXML(oTransaccion, oEmpresa);

                #region RUTA DE LA EMPRESA

                RUTA_LOGO_EMPRESA = @""+oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_EMPRESA_LOGO_PDF;

                #endregion

                if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
                {
                    RUTA_CODIGO_HASH = @"" + oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_BOLETAS +
                                       strNombreArchivo + @"\" +
                                       strNombreArchivo + ".bmp";
                }
                else
                    if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA)
                {
                    RUTA_CODIGO_HASH = @"" + oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_FACTURAS +
                                       strNombreArchivo + @"\" +
                                       strNombreArchivo + ".bmp";
                }
                else
                if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
                {
                    RUTA_CODIGO_HASH = @"" + oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_CREDITO +
                                       strNombreArchivo + @"\" +
                                       strNombreArchivo + ".bmp";

                }
                else
                if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO)
                {
                    RUTA_CODIGO_HASH = @"" + oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_DEBITO +
                                       strNombreArchivo + @"\" +
                                       strNombreArchivo + ".bmp";

                }
                else
                if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION)
                {
                    RUTA_CODIGO_HASH = @"" + oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_GUIA_REMISION +
                                       strNombreArchivo + @"\" +
                                       strNombreArchivo + ".bmp";

                }


                #region Validando que exista la ruta del archivo sino no se genera el logo

                byte[] imgLogo = null;

                try
                {
                    var logo = Image.FromFile(RUTA_LOGO_EMPRESA);
                    imgLogo = Util.ImageToByteArray(logo);
                }
                catch
                {
                    //
                }


                #endregion


                if (RUTA_CODIGO_HASH.Length > 0)
                {
                    byte[] imgCodigoHash = null;

                    #region Validando que exista la ruta del archivo sino no se genera el hash
                    /*Con esto valido que un document no necesariamente puede tener hash*/
                    try
                    {
                        var cod = Image.FromFile(RUTA_CODIGO_HASH);
                        imgCodigoHash = Util.ImageToByteArray(cod);
                    }
                    catch
                    {
                        //
                    }

                    #endregion

                    if (oTransaccion.LstTransaccionDetalle.Count > 1)
                    {
                        oTransaccion.LstTransaccionDetalle[0].iLogo = imgLogo;
                        oTransaccion.LstTransaccionDetalle[0].iCodHash = imgCodigoHash;

                        try
                        {
                            int index = oTransaccion.LstTransaccionDetalle.Count - 1;
                            oTransaccion.LstTransaccionDetalle[index].iLogo = imgLogo;
                            oTransaccion.LstTransaccionDetalle[index].iCodHash = imgCodigoHash;
                        }
                        catch
                        {
                            //
                        }
                    }
                    else
                    {
                        oTransaccion.LstTransaccionDetalle[0].iLogo = imgLogo;
                        oTransaccion.LstTransaccionDetalle[0].iCodHash = imgCodigoHash;
                    }
                }
                else
                {
                    oTransaccion.LstTransaccionDetalle[0].iLogo = null;
                    oTransaccion.LstTransaccionDetalle[0].iCodHash = null;
                }

                #region OBTENER UN DATATABLE A PARTIR DE UNA GENERICA

                var dtSource = ObtenerDatosReporte(oTransaccion.LstTransaccionDetalle);

                #endregion

                #region POR TIPO DE DOCUMENTO SE GENERA EL PDF

                switch (oTransaccion.TipoDocumento_Id)
                {
                    #region FACTURA

                    case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:

                        b_resultado = oPDF.ReportarFactura(dtSource, oTransaccion, strNombreArchivo, oEmpresa);

                        if (b_resultado)
                        {
                            //actualizacion en memoria
                            //if (oTransaccion.nTraEstadoTransaccionElectronica !=
                            //    (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_CLIENTE)
                            //{
                            //    oTransaccion.nTraEstadoTransaccionElectronica =
                            //        (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO;

                            //    //actualizacion en bd
                            //    wsServicio.ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id,
                            //        (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO);
                            //}
                        }
                        return b_resultado;

                        break;

                    #endregion

                    #region BOLETA

                    case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:

                        b_resultado = oPDF.ReportarBoleta(dtSource, oTransaccion, strNombreArchivo, oEmpresa);

                        //if (b_resultado)
                        //{
                        //    //actualizacion en memoria
                        //    if (oTransaccion.nTraEstadoTransaccionElectronica !=
                        //        (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_CLIENTE)
                        //    {
                        //        oTransaccion.nTraEstadoTransaccionElectronica =
                        //            (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO;

                        //        //actualizacion en bd
                        //        wsServicio.ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id,
                        //            (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO);
                        //    }
                        //}

                        return b_resultado;
                        break;

                    #endregion

                    #region NOTA DE CREDITO

                    case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:

                        b_resultado = oPDF.ReportarNotaCredito(dtSource, oTransaccion, strNombreArchivo, oEmpresa);

                        //if (b_resultado)
                        //{
                        //    wsServicio.ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id,
                        //        (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO);
                        //}

                        return b_resultado;

                        break;

                    #endregion

                    #region NOTA DE DEBITO

                    //case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:

                    //    b_resultado = oPDF.ReportarNotaDebito(dtSource, oTransaccion, strNombreArchivo);

                    //    if (b_resultado)
                    //    {
                    //        wsServicio.ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id,
                    //            (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO);
                    //    }

                    //    return b_resultado;

                    //    break;

                    #endregion

                    #region GUIA DE REMISION
                    case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION:

                        b_resultado = oPDF.ReportarGuiaRemision(dtSource, oTransaccion, strNombreArchivo, oEmpresa);

                        //if (b_resultado)
                        //{
                        //    wsServicio.ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id,
                        //        (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO);
                        //}

                        return b_resultado;

                        break;

                        #endregion
                }

                #endregion

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable ObtenerDatosReporte(List<TransaccionDetalle> ListaResultado)
        {
            try
            {
                return oPDF.GenerarDataTable(ListaResultado);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string ObtenerRutaRespuestaSunat(int tipoDocumento_Id, Empresa oEmpresa, string pstrNombreArchivoXml)
        {
            string strRespuestaSUNATxml = string.Empty;

			if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA)
			{
				strRespuestaSUNATxml = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_FACTURAS_RESPUESTA + "\\" + "R-" + pstrNombreArchivoXml.Trim('\\') + ".xml";
			}
			else
					if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
			{
				strRespuestaSUNATxml = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_BOLETAS_RESPUESTA + "\\" + "R-" + pstrNombreArchivoXml.Trim('\\') + ".xml";
			}
			else
					if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
			{
				strRespuestaSUNATxml = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_CREDITO_RESPUESTA + "\\" + "R-" + pstrNombreArchivoXml.Trim('\\') + ".xml";
			}
			else
					if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO)
			{
				strRespuestaSUNATxml = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_DEBITO_RESPUESTA + "\\" + "R-" + pstrNombreArchivoXml.Trim('\\') + ".xml";
			}
			else
					if (tipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION)
			{
				strRespuestaSUNATxml = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_GUIA_REMISION_RESPUESTA + "\\" + "R-" + pstrNombreArchivoXml.Trim('\\') + ".xml";
			}

			return strRespuestaSUNATxml;
        }



    }
}