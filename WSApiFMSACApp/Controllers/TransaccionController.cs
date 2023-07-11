using Entidades.Comercial;
using Entidades.Maestros;
using Negocio.Comercial;
using Negocio.Maestros;
using Sunat;
using System.Drawing;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using System.Threading.Tasks;
using Utilitarios;

namespace WSApiFMSACApp.Controllers
{
    [RoutePrefix("servicio/Transaccion")]
    public class TransaccionController : ApiController
    {
        Servicio wsServicio = new Servicio();
        DocumentoElectronico oDocumentoElectronico = new DocumentoElectronico();
        Sunat.PDF oPDF = new Sunat.PDF();

	
		[HttpPost]
		[Route("Guardar")]
		public async Task<int> Guardar([FromBody] Transaccion oTransaccion)
		{
			try
			{
				if (oTransaccion != null && oTransaccion.Transaccion_Id > 0)
				{
					bool result = true;

					var LstTransaccion = new List<Transaccion>();
					LstTransaccion.Add(oTransaccion);

					result = await TransaccionNeg.Instance.GuardarAsync(LstTransaccion);

					if (result)
					{
						bool b_Enviado = await EnviarSunatAsync(LstTransaccion);

						if (b_Enviado)
						{
							#region PDF y Correo
							//try
							//{
							//	bool b_PDF = await CrearPDFAsync(LstTransaccion);

							//	if (b_PDF)
							//	{
							//		return Enumerador.RESPUESTA_ENVIADO_PDF_CREADO;
							//	}

							//	return Enumerador.RESPUESTA_ENVIADO_SUNAT;
							//}
							//catch (Exception ex)
							//{
							//	await LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
							//	return Enumerador.RESPUESTA_ENVIADO_SUNAT;
							//}

							return Enumerador.RESPUESTA_ENVIADO_SUNAT;

							#endregion
						}

						return Enumerador.RESPUESTA_INSERTADO_BD;
					}
				}

				return Enumerador.RESPUESTA_ERROR_INSERTADO_BD;
			}
			catch (Exception ex)
			{
				await LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				return Enumerador.RESPUESTA_ERROR_INSERTADO_BD;
			}
		}

		[HttpGet]
        public async Task<List<Transaccion>> Get()
        {
            try
            {
                var Parametros = new Dictionary<string, object>();

                return await TransaccionNeg.Instance.ConsultarAsync(Parametros);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]
        [Route("Consultar/{sEmpRuc}/{sTraSerie}/{sTraNumero}")]
        public async Task<List<Transaccion>> Consultar(string sEmpRuc, string sTraSerie, string sTraNumero)
        {
            try
            {
                var oEmpresa = EmpresaNeg.Instance.Consultar(sEmpRuc);

                var Parametros = new Dictionary<string, object>();
                Parametros.Add("Empresa_Id", oEmpresa.Empresa_Id);
                Parametros.Add("sTraSerie", sTraSerie);
                Parametros.Add("sTraNumero", sTraNumero);

                return await TransaccionNeg.Instance.ConsultarAsync(Parametros);
            }
            catch (Exception)
            {

                throw;
            }
        }


        [HttpGet]
        [Route("EnviarCorreoCliente/{sEmpRuc}/{sTraSerie}/{sTraNumero}")]
        public int EnviarCorreoCliente(string sEmpRuc, string sTraSerie, string sTraNumero)
        {
            try
            {
                var oEmpresa = EmpresaNeg.Instance.Consultar(sEmpRuc);

                var Parametros = new Dictionary<string, object>();
                Parametros.Add("Empresa_Id", oEmpresa.Empresa_Id);
                Parametros.Add("sTraSerie", sTraSerie);
                Parametros.Add("sTraNumero", sTraNumero);

                var Lista = TransaccionNeg.Instance.Consultar(Parametros);

                if (Lista != null && Lista.Count > 0)
                {
                    var oTransaccion = Lista.FirstOrDefault();

                    if (oTransaccion.nTraEstadoTransaccionElectronica <= 0)
                    {
                        new Exception("El documento aún no ha sido enviado a sunat y su estado es: " + oTransaccion.nTraEstadoTransaccionElectronica);
                    }

                    GenerarPDF(oTransaccion, oEmpresa);

                    //string strNombreArchivo = oDocumentoElectronico.GenerarNombreXML(oTransaccion, oEmpresa);
                    //oPDF.EnviarPDFCorreo(oTransaccion, oEmpresa, strNombreArchivo);
                    //wsServicio.ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id, (int)Enumerador.RESPUESTA_ENVIADO_EMAIL_CLIENTE);

                    /*EXITO*/
                    return Enumerador.ESTADO_ACTIVO;
                }

                return Enumerador.ESTADO_INACTIVO;

            }
            catch (Exception ex)
            {
                LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
                return Enumerador.RESPUESTA_ERROR_INSERTADO_BD;
            }
        }


		[HttpGet]
		[Route("ConsultarTicket/{sEmpRuc}/{sTraticket}")]
		public async Task<int> ConsultarTicket(string sEmpRuc, string sTraticket="")
		{
			try
			{
				//1.obtener la empresa
				var oEmpresa = EmpresaNeg.Instance.Consultar(sEmpRuc);

				bool b_Enviado = await ConsultarTicketAsync(oEmpresa,sTraticket);

				if (b_Enviado)
				{
					return Enumerador.RESPUESTA_ENVIADO_SUNAT;
				}

				return Enumerador.RESPUESTA_ERROR_INSERTADO_BD;

			}
			catch (Exception ex)
			{
				await LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				return Enumerador.RESPUESTA_ERROR_INSERTADO_BD;
			}
		}


		/// <summary>
		/// Reenvia el documento directamente a sunat: Para esto el documento
		/// xml respectivo ha tenido que ser ya insertado y generado su xml
		/// respectivo
		/// </summary>
		/// <param name="sEmpRuc"></param>
		/// <param name="sTraSerie"></param>
		/// <param name="sTraNumero"></param>
		/// <returns></returns>
		[HttpGet]
        [Route("ReenviarSunat/{sEmpRuc}/{sTraSerie}/{sTraNumero}")]
        public async Task<int> ReenviarSunat(string sEmpRuc, string sTraSerie, string sTraNumero)
        {
            try
            {
                var oEmpresa = EmpresaNeg.Instance.Consultar(sEmpRuc);

                var Parametros = new Dictionary<string, object>();
                Parametros.Add("Empresa_Id", oEmpresa.Empresa_Id);
                Parametros.Add("sTraSerie", sTraSerie);
                Parametros.Add("sTraNumero", sTraNumero);

                var LstTransaccion = TransaccionNeg.Instance.Consultar(Parametros);

                if (LstTransaccion != null && LstTransaccion.Count > 0)
                {

                    bool b_Enviado = await EnviarSunatAsync(LstTransaccion);

                    if (b_Enviado)
                    {
                        #region PDF y Correo
                        try
                        {
                            bool b_PDF = await CrearPDFAsync(LstTransaccion);

                            if (b_PDF)
                            {
                                try
                                {
									return Enumerador.RESPUESTA_ENVIADO_PDF_CREADO;
								}
                                catch (Exception ex)
                                {
                                    await LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
                                    return Enumerador.RESPUESTA_ENVIADO_PDF_CREADO;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
                            return Enumerador.RESPUESTA_ENVIADO_SUNAT;
                        }
                        #endregion
                    }
                }

                return Enumerador.RESPUESTA_ERROR_INSERTADO_BD;
            }
            catch (Exception ex)
            {
                await LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
                return Enumerador.RESPUESTA_ERROR_INSERTADO_BD;
            }
        }

        /// <summary>
        /// Devuelve el estado del documento consultado
        /// </summary>
        /// <param name="sEmpRuc"></param>
        /// <param name="sTraSerie"></param>
        /// <param name="sTraNumero"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ConsultarEstado/{sEmpRuc}/{sTraSerie}/{sTraNumero}")]
        public int ConsultarEstado(string sEmpRuc, string sTraSerie, string sTraNumero)
        {
            try
            {
                var Parametros = new Dictionary<string, object>();
                Parametros.Add("sEmpRuc", sEmpRuc);
                Parametros.Add("sTraSerie", sTraSerie);
                Parametros.Add("sTraNumero", sTraNumero);

                return TransaccionNeg.Instance.ConsultarEstadoElectronico(Parametros);
            }
            catch (Exception ex)
            {
                LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
                return Enumerador.RESPUESTA_ERROR_INSERTADO_BD;
            }
        }

        // POST: api/Transaccion
        [ResponseType(typeof(List<Transaccion>))]
        public async Task<int> Post(List<Transaccion> LstTransaccion)
        {
            try
            {
                if (LstTransaccion != null && LstTransaccion.Count > 0)
                {
                    bool result = true;

                    result = await TransaccionNeg.Instance.GuardarAsync(LstTransaccion);

                    if (result)
                    {
                        bool b_Enviado = await EnviarSunatAsync(LstTransaccion);

                        if (b_Enviado)
                        {
                            #region PDF y Correo
                            try
                            {
                                bool b_PDF = await CrearPDFAsync(LstTransaccion);

                                if (b_PDF)
                                {
									return Enumerador.RESPUESTA_ENVIADO_PDF_CREADO;
								}
                            }
                            catch (Exception ex)
                            {
                                await LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
                                return Enumerador.RESPUESTA_ENVIADO_SUNAT;
                            }
                            #endregion
                        }

                        return Enumerador.RESPUESTA_INSERTADO_BD;
                    }
                }

                return Enumerador.RESPUESTA_ERROR_INSERTADO_BD;
            }
            catch (Exception ex)
            {
                await LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
                return Enumerador.RESPUESTA_ERROR_INSERTADO_BD;
            }
        }


		/*CAMBIO 24-03-2020*/
		/// <summary>
		/// resurso que guarda todos los documentos de forma masiva
		/// sin acceder a sunat, su finalidad es solo guardar en Base 
		/// de Datos.
		/// </summary>
		/// <param name="LstTransaccion"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("GuardarMasivo")]
		[ResponseType(typeof(List<Transaccion>))]
		public async Task<int> GuardarMasivo(List<Transaccion> LstTransaccion)
		{
			try
			{
				if (LstTransaccion != null && LstTransaccion.Count > 0)
				{
					bool result = true;

					result = await TransaccionNeg.Instance.GuardarAsync(LstTransaccion);

					if (result)
					{
						return Enumerador.RESPUESTA_INSERTADO_BD;
					}
				}

				return Enumerador.RESPUESTA_ERROR_INSERTADO_BD;
			}
			catch (Exception ex)
			{
				await LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				return Enumerador.RESPUESTA_ERROR_INSERTADO_BD;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="lstTransaccion"></param>
		/// <returns></returns>
		private Task<bool> EnviarEmailClienteAsync(List<Transaccion> lstTransaccion)
        {
            try
            {
                return Task.Run(() => EnviarEmailCliente(lstTransaccion));
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstTransaccion"></param>
        /// <returns></returns>
        private bool EnviarEmailCliente(List<Transaccion> lstTransaccion)
        {
            try
            {
                foreach (var item in lstTransaccion)
                {
                    var oEmpresa = EmpresaNeg.Instance.Consultar(item.sTraRUCEmpresa);

                    var Parametros = new Dictionary<string, object>();
                    Parametros.Add("Empresa_Id", oEmpresa.Empresa_Id);
                    Parametros.Add("TipoDocumento_Id", item.TipoDocumento_Id);
                    Parametros.Add("sTraSerie", item.sTraSerie);
                    Parametros.Add("sTraNumero", item.sTraNumero);

                    var Lista = TransaccionNeg.Instance.Consultar(Parametros);

                    if (Lista != null && Lista.Count > 0)
                    {
                        var oTransaccion = Lista.FirstOrDefault();

                        string strNombreArchivo = oDocumentoElectronico.GenerarNombreXML(oTransaccion, oEmpresa);
                        oPDF.EnviarPDFCorreo(oTransaccion, oEmpresa, strNombreArchivo);
                        wsServicio.ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id, (int)Enumerador.RESPUESTA_ENVIADO_EMAIL_CLIENTE);
                    }
                }

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
        /// <param name="lstTransaccion"></param>
        /// <returns></returns>
        private Task<bool> CrearPDFAsync(List<Transaccion> lstTransaccion)
        {
            try
            {
                return Task.Run(() => CrearPDF(lstTransaccion));
            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool CrearPDF(List<Transaccion> lstTransaccion)
        {
            try
            {
                foreach (var item in lstTransaccion)
                {
                    var oEmpresa = EmpresaNeg.Instance.Consultar(item.sTraRUCEmpresa);

                    var Parametros = new Dictionary<string, object>();
                    Parametros.Add("Empresa_Id", oEmpresa.Empresa_Id);
                    Parametros.Add("TipoDocumento_Id", item.TipoDocumento_Id);
                    Parametros.Add("sTraSerie", item.sTraSerie);
                    Parametros.Add("sTraNumero", item.sTraNumero);

                    var Lista = TransaccionNeg.Instance.Consultar(Parametros);

                    if (Lista != null && Lista.Count > 0)
                    {
                        var oTransaccion = Lista.FirstOrDefault();
						LogApplicationNeg.Instance.GuardarLogAplicacion(null,"Entrando a Generar PDF");
						GenerarPDF(oTransaccion, oEmpresa);
						LogApplicationNeg.Instance.GuardarLogAplicacion(null, "Saliendo de generar PDF");
					}
                }

                return true;
            }
            catch (Exception ex)
            {
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);

                throw;
				
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
                    RUTA_CODIGO_HASH = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_BOLETAS +
                                       strNombreArchivo + @"\" +
                                       strNombreArchivo + ".bmp";
                }
                else
                    if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA)
                {
                    RUTA_CODIGO_HASH = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_FACTURAS +
                                       strNombreArchivo + @"\" +
                                       strNombreArchivo + ".bmp";
                }
                else
                if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
                {
                    RUTA_CODIGO_HASH = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_CREDITO +
                                       strNombreArchivo + @"\" +
                                       strNombreArchivo + ".bmp";

                }
                else
                if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO)
                {
                    RUTA_CODIGO_HASH = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_DEBITO +
                                       strNombreArchivo + @"\" +
                                       strNombreArchivo + ".bmp";

                }
                else
                if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION)
                {
                    RUTA_CODIGO_HASH = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_GUIA_REMISION +
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
                            if (oTransaccion.nTraEstadoTransaccionElectronica !=
                                (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_CLIENTE)
                            {
                                oTransaccion.nTraEstadoTransaccionElectronica =
                                    (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO;

                                //actualizacion en bd
                                wsServicio.ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id,
                                    (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO);
                            }
                        }
                        return b_resultado;

                        break;

                    #endregion

                    #region BOLETA

                    case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:

                        b_resultado = oPDF.ReportarBoleta(dtSource, oTransaccion, strNombreArchivo, oEmpresa);

                        if (b_resultado)
                        {
                            //actualizacion en memoria
                            if (oTransaccion.nTraEstadoTransaccionElectronica !=
                                (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_CLIENTE)
                            {
                                oTransaccion.nTraEstadoTransaccionElectronica =
                                    (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO;

                                //actualizacion en bd
                                wsServicio.ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id,
                                    (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO);
                            }
                        }

                        return b_resultado;
                        break;

                    #endregion

                    #region NOTA DE CREDITO

                    case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:

                        b_resultado = oPDF.ReportarNotaCredito(dtSource, oTransaccion, strNombreArchivo, oEmpresa);

                        if (b_resultado)
                        {
                            wsServicio.ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id,
                                (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO);
                        }

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

                        if (b_resultado)
                        {
                            wsServicio.ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id,
                                (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO);
                        }

                        return b_resultado;

                        break;

                        #endregion
                }

                #endregion

                return true;
            }
            catch (Exception ex)
            {
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstTransaccion"></param>
        /// <returns></returns>
        private Task<bool> EnviarSunatAsync(List<Transaccion> lstTransaccion)
        {
            try
            {
                return Task.Run(() => EnviarSunat2(lstTransaccion));
            }
            catch (Exception)
            {
                throw;
            }
        }

		private Task<bool> ConsultarTicketAsync(Empresa p_oEmpresa, string sTraTicket)
		{
			try
			{
				return Task.Run(() => ConsultarTicket(p_oEmpresa, sTraTicket));
			}
			catch (Exception)
			{
				throw;
			}
		}

		private bool ConsultarTicket(Empresa p_oEmpresa, string sTraTicket)
		{
			try
			{
				bool rpta = false;

				if (p_oEmpresa.nEmpAfilidadoOSE == Enumerador.ESTADO_ACTIVO)
				{
					rpta = wsServicio.ConsultarTicketOSE(p_oEmpresa, sTraTicket);
				}
				else {
					rpta = wsServicio.ConsultarTicket(p_oEmpresa, sTraTicket);
				}

				return rpta;
			}
			catch (Exception ex)
			{
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);

				return false;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="LstTransaccion"></param>
		/// <returns></returns>
		private async Task<bool> EnviarSunat(List<Transaccion> LstTransaccion)
        {
            try
            {
                #region Facturas, NC, ND

                foreach (var item in LstTransaccion.Where(x => x.TipoDocumento_Id !=
                (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA))
                {
                    var oEmpresa = EmpresaNeg.Instance.Consultar(item.sTraRUCEmpresa);

                    var Parametros = new Dictionary<string, object>();
                    Parametros.Add("Empresa_Id", oEmpresa.Empresa_Id);
                    Parametros.Add("TipoDocumento_Id", item.TipoDocumento_Id);
                    Parametros.Add("sTraSerie", item.sTraSerie);
                    Parametros.Add("sTraNumero", item.sTraNumero);

                    var Lista = TransaccionNeg.Instance.Consultar(Parametros);

                    if (Lista != null && Lista.Count > 0)
                    {
                        var oTransaccion = Lista.FirstOrDefault();

                        bool rpta = false;

                        if (oTransaccion.nTraEstadoTransaccionElectronica > Enumerador.RESPUESTA_INSERTADO_BD)
                        {
                            new Exception("El documento ya tiene un estado superior " + oTransaccion.nTraEstadoTransaccionElectronica);
                        }

						if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION)
						{
							rpta = wsServicio.GenerarArchivoGuiaRemisionXmlyZip(oTransaccion, oEmpresa);
							if (rpta)
							{
								return wsServicio.EnviarArchivoGuiaRemision(oTransaccion, oEmpresa);
							}
						}
						else
							if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
								{
							//verificar si es en alusion a una boleta
									if (oTransaccion.sTraSerie.Substring(0, 1).Equals("B"))
									{
										//Llamar al resumen de notas de credito
										if(oEmpresa.nEmpAfilidadoOSE== Enumerador.ESTADO_ACTIVO)
										return wsServicio.EnviarResumenNotaCreditoSunatOSE(oTransaccion, oEmpresa);
										else
										return wsServicio.EnviarResumenNotaCreditoSunat(oTransaccion, oEmpresa);
							}
									else
									{
										rpta = wsServicio.GenerarArchivoXmlyZip(oTransaccion, oEmpresa);
										if (rpta)
										{
											return wsServicio.EnviarArchivo(oTransaccion, oEmpresa);
										}
									}
							
								}
							else
							{
							/*FACTURA - BOLETA - NC - ND*/
							rpta = wsServicio.GenerarArchivoXmlyZip(oTransaccion, oEmpresa);
							if (rpta)
							{
								return wsServicio.EnviarArchivo(oTransaccion, oEmpresa);
							}
						}
                    }
                }
                #endregion

                #region Boletas

                //Resumen diario

                var LstTraBoletas = new List<Transaccion>();
                Empresa MiEmpresa = new Empresa();

                foreach (var item in LstTransaccion
                                     .Where(x => x.TipoDocumento_Id
                                            == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA))
                {
                    var oEmpresa = EmpresaNeg.Instance.Consultar(item.sTraRUCEmpresa);
                    var Parametros = new Dictionary<string, object>();
                    Parametros.Add("Empresa_Id", oEmpresa.Empresa_Id);
                    Parametros.Add("TipoDocumento_Id", item.TipoDocumento_Id);
                    Parametros.Add("sTraSerie", item.sTraSerie);
                    Parametros.Add("sTraNumero", item.sTraNumero);

                    var Lista = TransaccionNeg.Instance.Consultar(Parametros);

                    if (Lista != null && Lista.Count > 0)
                    {
                        var oTransaccion = Lista.FirstOrDefault();

                        if (oTransaccion.nTraEstadoTransaccionElectronica > Enumerador.RESPUESTA_INSERTADO_BD)
                        {
                            new Exception("El documento ya tiene un estado superior " + oTransaccion.nTraEstadoTransaccionElectronica);
                        }

                        LstTraBoletas.Add(oTransaccion);
                    }

                    MiEmpresa = oEmpresa;
                }

                if (LstTraBoletas.Count > 0)
                {
                    return await wsServicio.EnviarResumenBoletasSunat(LstTraBoletas, MiEmpresa);
                }

                #endregion

                return false;
            }
            catch (Exception ex)
            {
				await LogApplicationNeg.Instance.GuardarLogAplicacion(ex);

                return false;
            }
        }



		private bool EnviarSunat2(List<Transaccion> LstTransaccion)
		{
			try
			{
				#region Proceso

				foreach (var item in LstTransaccion)
				{
					var oEmpresa = EmpresaNeg.Instance.Consultar(item.sTraRUCEmpresa);
					
					#region Obtener la Transaccion
					var Parametros = new Dictionary<string, object>();
					Parametros.Add("Empresa_Id", oEmpresa.Empresa_Id);
					Parametros.Add("TipoDocumento_Id", item.TipoDocumento_Id);
					Parametros.Add("sTraSerie", item.sTraSerie);
					Parametros.Add("sTraNumero", item.sTraNumero);

					var Lista = TransaccionNeg.Instance.Consultar(Parametros);
					#endregion

					#region xml y Enviar a Sunat
					if (Lista != null && Lista.Count > 0)
					{
						var oTransaccion = Lista.FirstOrDefault();

						bool rpta = false;

						if (oTransaccion.nTraEstadoTransaccionElectronica > Enumerador.RESPUESTA_INSERTADO_BD)
						{
							new Exception("El documento ya tiene un estado superior " + oTransaccion.nTraEstadoTransaccionElectronica);
						}

                        #region LIQUIDACION DE COMPRA
                        if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_LIQUIDACION_COMPRA)
                        {
                            rpta = wsServicio.GenerarArchivoLiquidacionCompraXmlyZip(oTransaccion, oEmpresa);
                            if (rpta)
                            {

                                if (oEmpresa.nEmpAfilidadoOSE == Enumerador.ESTADO_ACTIVO)
                                {
                                    return wsServicio.EnviarArchivoOSE(oTransaccion, oEmpresa);
                                }
                                else return wsServicio.EnviarArchivo(oTransaccion, oEmpresa);
                            }
                        }
                        #endregion

                        else
                        #region GUIA DE REMISION
                        if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION)
						{
							rpta = wsServicio.GenerarArchivoGuiaRemisionXmlyZip(oTransaccion, oEmpresa);
							if (rpta)
							{

								if (oEmpresa.nEmpAfilidadoOSE == Enumerador.ESTADO_ACTIVO)
								{
									return wsServicio.EnviarArchivoOSE(oTransaccion, oEmpresa);
								}
								else return wsServicio.EnviarArchivoGuiaRemision(oTransaccion, oEmpresa);
							}
						}
						#endregion

						#region FACTURAS-BOLETAS-NC-ND
						else
						{ 
								if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
								{
									rpta = wsServicio.GenerarArchivoXmlyBoletaZipUBL2_1(oTransaccion, oEmpresa);
								}
								else
								{
									/*FACTURA - NC - ND*/
									rpta = wsServicio.GenerarArchivoXmlyZip(oTransaccion, oEmpresa);
								}
							/*FINALMENTE ENVIAR EL ARCHIVO AL SERVICIO*/
							if (rpta)
							{
								if (oEmpresa.nEmpAfilidadoOSE == Enumerador.ESTADO_ACTIVO)
								{
									return wsServicio.EnviarArchivoOSE(oTransaccion, oEmpresa);
								}
								else
								{
									return wsServicio.EnviarArchivo(oTransaccion, oEmpresa);
								}
							}
						}
						#endregion
					}
					#endregion
				}
				#endregion

				return false;
			}
			catch (Exception ex)
			{
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);

				return false;
			}
		}


		[HttpGet]
        [Route("PruebaEnvio/{sEmpRuc}/{sTraSerie}/{sTraNumero}")]
        public string PruebaEnvio(string sEmpRuc, string sTraSerie, string sTraNumero)
        {
            try
            {
                var oEmpresa = EmpresaNeg.Instance.Consultar(sEmpRuc);

                var Parametros = new Dictionary<string, object>();
                Parametros.Add("Empresa_Id", oEmpresa.Empresa_Id);
                Parametros.Add("sTraSerie", sTraSerie);
                Parametros.Add("sTraNumero", sTraNumero);

                var Lista = TransaccionNeg.Instance.Consultar(Parametros);

                if (Lista != null && Lista.Count > 0)
                {
                    //bool rpta = false;
                    //rpta = wsServicio.GenerarArchivoXmlyZip(Lista.FirstOrDefault(), oEmpresa);

                    //if (rpta)
                    //{
                    //    if(wsServicio.EnviarArchivo(Lista.FirstOrDefault(), oEmpresa))
                    //    {
                    //        return "ok";
                    //    }
                    //}
                    //else
                    //{
                    //    return "Prueba Fallida!";
                    //}

                    if (wsServicio.AnularArchivo(Lista.FirstOrDefault(), oEmpresa))
                    {
                        return "ok";
                    }
                }

                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// anula el documento electronico sea
        /// boleta, factura, nc, nd
        /// </summary>
        /// <param name="sEmpRuc"></param>
        /// <param name="sTraSerie"></param>
        /// <param name="sTraNumero"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("AnularSunat/{sEmpRuc}/{sTraSerie}/{sTraNumero}")]
        public async Task<int> AnularSunat(string sEmpRuc, string sTraSerie, string sTraNumero)
        {
            try
            {
                var oEmpresa = EmpresaNeg.Instance.Consultar(sEmpRuc);

                var Parametros = new Dictionary<string, object>();
                Parametros.Add("Empresa_Id", oEmpresa.Empresa_Id);
                Parametros.Add("sTraSerie", sTraSerie);
                Parametros.Add("sTraNumero", sTraNumero);

                var Lista = TransaccionNeg.Instance.Consultar(Parametros);

                if (Lista != null && Lista.Count > 0)
                {
                    var oTransaccion = Lista.FirstOrDefault();

                    if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
                    {
						if (oEmpresa.nEmpAfilidadoOSE == Enumerador.ESTADO_ACTIVO)
							wsServicio.EnviarResumenBoletasSunatOSE(Lista, oEmpresa, Enumerador.ESTADO_BOLETA_ELECTRONICA_BAJA);
						else
							await wsServicio.EnviarResumenBoletasSunat(Lista, oEmpresa, Enumerador.ESTADO_BOLETA_ELECTRONICA_BAJA);

						return Enumerador.RESPUESTA_ENVIADO_SUNAT;
                    }
					else
					if (oTransaccion.TipoDocumento_Id == 
						(int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
					{
						//verificar si es en alusion a una boleta
						if (oTransaccion.sTraSerie.Substring(0, 1).Equals("B"))
						{
							//Llamar al resumen de notas de credito
							if (oEmpresa.nEmpAfilidadoOSE == Enumerador.ESTADO_ACTIVO)
								wsServicio.EnviarResumenNotaCreditoSunatOSE(oTransaccion, oEmpresa, Enumerador.ESTADO_BOLETA_ELECTRONICA_BAJA);
							else
								wsServicio.EnviarResumenNotaCreditoSunat(oTransaccion, oEmpresa, Enumerador.ESTADO_BOLETA_ELECTRONICA_BAJA);

							return Enumerador.RESPUESTA_ENVIADO_SUNAT;
						}
						else
						{
							wsServicio.AnularArchivo(oTransaccion, oEmpresa);

							return Enumerador.RESPUESTA_ENVIADO_SUNAT;
						}
					}
					else
                        if (wsServicio.AnularArchivo(oTransaccion, oEmpresa))
                    {
                        return Enumerador.RESPUESTA_ENVIADO_SUNAT;
                    }
                }

                return Enumerador.RESPUESTA_INSERTADO_BD;
            }
            catch (Exception ex)
            {
                await LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
                return Enumerador.RESPUESTA_ERROR_INSERTADO_BD;
            }
        }

		// PUT: api/Transaccion/5
		public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Transaccion/5
        public void Delete(int id)
        {
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




        /*PRUEBA DE ENVIO AL SERVICIO DE SUNAT MEDIANTE UNA RUTA XML*/

        [HttpGet]
        [Route("PruebaEnvioArchivo/{sEmpRuc}/{sNombreArchivo}")]
        public string PruebaEnvioArchivo(string sEmpRuc, string sNombreArchivo)
        {
            try
            {
                var oEmpresa = EmpresaNeg.Instance.Consultar(sEmpRuc);

                string sRutaArchivo = @"C:\FMSAC_DOC_ELECTRONICO\20482092897\FacturacionElectronicaXml\LiquidacionCompra\20482092897-04-F700-1\20482092897-04-F700-1.zip";
                
                wsServicio.EnviarArchivoRutaPrueba(oEmpresa, sRutaArchivo, sNombreArchivo);

                return "OK";
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}
