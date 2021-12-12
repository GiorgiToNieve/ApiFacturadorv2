using Entidades.Comercial;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls;
using System.Threading.Tasks;
using Negocio.Comercial;
using Utilitarios;
using Entidades.Maestros;
using System.Linq;
using Negocio.Maestros;
using Sunat;

namespace DestockFMSAC
{
    public partial class frmTransaccion : Telerik.WinControls.UI.RadForm
    {
        #region Vairbales Globales
        Servicio wsServicio = new Servicio();
        DocumentoElectronico oDocumentoElectronico = new DocumentoElectronico();
        DesktopFMSAC.Sunat.PDF oPDF = new DesktopFMSAC.Sunat.PDF();
        List<Transaccion> LstTransaccion = new List<Transaccion>();
        #endregion

        #region Constructor
        public frmTransaccion()
        {
            InitializeComponent();
        }
        #endregion

        private async void btnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                LstTransaccion = await ObtenerTransaccionPendienteAsync();

                dgvTransaccion.DataSource = null;

                if (LstTransaccion!= null &&  LstTransaccion.Count > 0)
                {

                    lblTotal.Text = ""+LstTransaccion.Count;
                    dgvTransaccion.DataSource = LstTransaccion;
                    btnEnviar_Click(null, null);
                }
                else
                {
                    Close();
                }

            }
            catch(Exception ex)
            {
               await LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
                Close();
            }
        }

        private Task<List<Transaccion>> ObtenerTransaccionPendienteAsync()
        {
            try
            {
                return Task.Run(() => ObtenerTransaccionPendiente());
            }
            catch (Exception)
            {

                throw;
            }
        }

        private List<Transaccion> ObtenerTransaccionPendiente()
        {
            try
            {
                var Parametros = new Dictionary<string, object>();
                Parametros.Add("nTraEstadoTransaccionElectronica", Enumerador.RESPUESTA_INSERTADO_BD);
                Parametros.Add("nTraEstado", Enumerador.ESTADO_ACTIVO);
				
				/*Regla de negocio: 25032020
				 Solo se consultaran los documentos desde 7 dias antes.
				 Peticion: Felipe Marroquin
				 Cambio: Victor Nieve
				 */
				Parametros.Add("dFechaInicio", DateTime.Now.Date.AddDays(-7));
				Parametros.Add("dFechaFinal", DateTime.Now.Date);

				return TransaccionNeg.Instance.Consultar(Parametros);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async void btnEnviar_Click(object sender, EventArgs e)
        {
            try
            {
                radWaitingBar1.StartWaiting();
                bool b_resultado = false;

                if (LstTransaccion.Count > 0)
                {
                    radWaitingBar1.Visible = true;
                    
                    int i = await ReenviarSunatAsync(LstTransaccion);

                    if (i > 0)
                    {
                        //MessageBox.Show("El Proceso ah terminado!");
                        LogApplicationNeg.Instance.Guardar(new Exception("Proceso Reenvio: Terminado Exitosamente"));
                        Close();
                    }
                }
                else
                {
                    LogApplicationNeg.Instance.Guardar(new Exception("Proceso Reenvio: No se han encontrado registros"));
                    radWaitingBar1.StopWaiting();
                    Close();
                }
            }
            catch (Exception ex)
            {
                LogApplicationNeg.Instance.Guardar(ex);
                radWaitingBar1.StopWaiting();
                Close();
                
            }
        }

        private async Task<int> ReenviarSunatAsync(List<Transaccion> LstTransaccionEnvio)
        {
            try
            {
                if (LstTransaccionEnvio != null && LstTransaccionEnvio.Count > 0)
                {
                    bool b_Enviado = await EnviarSunatAsync(LstTransaccionEnvio);

                    if (b_Enviado)
                    {
                        #region PDF y Correo
                        try
                        {
                            bool b_PDF = await CrearPDFAsync(LstTransaccionEnvio);

                            if (b_PDF)
                            {
                                try
                                {
                                    //bool b_Email = await EnviarEmailClienteAsync(LstTransaccionEnvio);
                                    //if (b_Email)
                                    //{
                                    //    return Enumerador.RESPUESTA_ENVIADO_EMAIL_CLIENTE;
                                    //}
                                }
                                catch (Exception ex)
                                {
                                    await LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
                                    return Enumerador.RESPUESTA_ENVIADO_PDF_CREADO;
                                }

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
                }

                return Enumerador.RESPUESTA_ERROR_INSERTADO_BD;
            }
            catch (Exception ex)
            {
                await LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
                return Enumerador.RESPUESTA_ERROR_INSERTADO_BD;
            }
        }

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

        private bool EnviarSunat(List<Transaccion> LstTransaccion)
        {
            try
            {
                #region Facturas, NC, ND
                bool b_factura = false;

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
                            continue;
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
                            continue;
                        }

                        LstTraBoletas = new List<Transaccion>();
                        LstTraBoletas.Add(oTransaccion);
                    }

                    MiEmpresa = oEmpresa;

                    if (LstTraBoletas.Count > 0)
                    {
                       wsServicio.EnviarResumenBoletasSunat(LstTraBoletas, MiEmpresa);
                    }
                }

                #endregion

                return true;
            }
            catch (Exception ex)
            {
                LogApplicationNeg.Instance.GuardarLogAplicacion(ex);

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

					#region Texto
					if (lblResumen.InvokeRequired)
					{
						lblResumen.Invoke(new Action(delegate ()
						{
							lblResumen.Text = "" + "Proceso Reenvio: " + oEmpresa.Empresa_Id + "-" + oEmpresa.sEmpBreve + "\n documento: " + item.sTraSerie + "-" + item.sTraNumero;
						}));
					}
					else
					{
						lblResumen.Text = "" + "Proceso Reenvio: " + oEmpresa.Empresa_Id + "-" + oEmpresa.sEmpBreve + "\n documento: " + item.sTraSerie + "-" + item.sTraNumero;
					}
					#endregion


					LogApplicationNeg.Instance.Guardar(new Exception("Proceso Reenvio: "+oEmpresa.Empresa_Id+"-"+oEmpresa.sEmpNombre+" documento: "+item.sTraSerie+"-"+item.sTraNumero));

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
						#region GUIA DE REMISION
						if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION)
						{
							rpta = wsServicio.GenerarArchivoGuiaRemisionXmlyZip(oTransaccion, oEmpresa);
							if (rpta)
							{
								LogApplicationNeg.Instance.Guardar(new Exception("Proceso Archivo Generado(GR): " + oEmpresa.Empresa_Id + "-" + oEmpresa.sEmpNombre + " documento: " + item.sTraSerie + "-" + item.sTraNumero));

								if (oEmpresa.nEmpAfilidadoOSE == Enumerador.ESTADO_ACTIVO)
								{
									rpta=wsServicio.EnviarArchivoOSE(oTransaccion, oEmpresa);
								}
								else rpta=wsServicio.EnviarArchivoGuiaRemision(oTransaccion, oEmpresa);
							}
						}
						#endregion

						#region FACTURAS-BOLETAS-NC-ND
						else
						{
							string document = "";
							if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
							{
								document = "(BOL)";
								rpta = wsServicio.GenerarArchivoXmlyBoletaZipUBL2_1(oTransaccion, oEmpresa);
							}
							else
							{
								/*FACTURA - NC - ND*/
								if(oTransaccion.TipoDocumento_Id== (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA)
									document = "(FA)";
								if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
									document = "(NC)";
								if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO)
									document = "(ND)";

								rpta = wsServicio.GenerarArchivoXmlyZip(oTransaccion, oEmpresa);
							}
							/*FINALMENTE ENVIAR EL ARCHIVO AL SERVICIO*/
							if (rpta)
							{
								LogApplicationNeg.Instance.Guardar(new Exception("Proceso Archivo Generado"+document+": " + oEmpresa.Empresa_Id + "-" + oEmpresa.sEmpNombre + " documento: " + item.sTraSerie + "-" + item.sTraNumero));

								if (oEmpresa.nEmpAfilidadoOSE == Enumerador.ESTADO_ACTIVO)
								{
									rpta=wsServicio.EnviarArchivoOSE(oTransaccion, oEmpresa);
								}
								else
								{
									rpta=wsServicio.EnviarArchivo(oTransaccion, oEmpresa);
								}
							}
						}
						#endregion

						string texto = string.Empty;

						if (rpta)
						{
							texto = "Enviado";
							LogApplicationNeg.Instance.Guardar(new Exception("Proceso Enviado: " + oEmpresa.Empresa_Id + "-" + oEmpresa.sEmpNombre + " documento: " + item.sTraSerie + "-" + item.sTraNumero));

						}
						else
						{
							texto = "Error de Envío";
							LogApplicationNeg.Instance.Guardar(new Exception("Proceso Error de Envío: " + oEmpresa.Empresa_Id + "-" + oEmpresa.sEmpNombre + " documento: " + item.sTraSerie + "-" + item.sTraNumero));

						}

						#region Texto
						if (lblResumen.InvokeRequired)
						{
							lblResumen.Invoke(new Action(delegate ()
							{
								lblResumen.Text = "" + "Proceso Envío: " + oEmpresa.Empresa_Id + "-" + oEmpresa.sEmpBreve + "\n documento: " + item.sTraSerie + "-" + item.sTraNumero+
								"\n"+ texto;
							}));
						}
						else
						{
							lblResumen.Text = "" + "Proceso Envío: " + oEmpresa.Empresa_Id + "-" + oEmpresa.sEmpBreve + "\n documento: " + item.sTraSerie + "-" + item.sTraNumero +
								"\n" + texto;
						}
						#endregion
					}
					#endregion
				}
				#endregion

				return true;
			}
			catch (Exception ex)
			{
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);

				return false;
			}
		}


		private Task<bool> CrearPDFAsync(List<Transaccion> lstTransaccion)
        {
            try
            {
                return Task.Run(() => CrearPDF(lstTransaccion));
            }
            catch (Exception ex)
            {
				
				throw;
            }
        }

        private bool CrearPDF(List<Transaccion> lstTransaccion)
        {
			string serie = string.Empty;
			string numero = string.Empty;
			int Transaccion_id = 0;
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

					serie = item.sTraSerie;
					numero = item.sTraNumero;

					if (Lista != null && Lista.Count > 0)
                    {
                        var oTransaccion = Lista.FirstOrDefault();
						Transaccion_id = oTransaccion.Transaccion_Id;
						if (oTransaccion.LstTransaccionDetalle.Count > 0)
						{
							if (oTransaccion.nTraEstadoTransaccionElectronica > (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_SUNAT)
							{
								GenerarPDF(oTransaccion, oEmpresa);
								#region Texto
								if (lblResumen.InvokeRequired)
								{
									lblResumen.Invoke(new Action(delegate ()
									{
										lblResumen.Text = "" + "Proceso Crear PDF: " + oEmpresa.Empresa_Id + "-" + oEmpresa.sEmpBreve + "\n documento: " + item.sTraSerie + "-" + item.sTraNumero +
										" PDF ha sido Generado";
									}));
								}
								else
								{
									lblResumen.Text = "" + "Proceso Crear PDF: " + oEmpresa.Empresa_Id + "-" + oEmpresa.sEmpBreve + "\n documento: " + item.sTraSerie + "-" + item.sTraNumero +
										" PDF ha sido Generado";
								}
								#endregion
							}
						}
						else
						{

							#region Texto
							if (lblResumen.InvokeRequired)
							{
								lblResumen.Invoke(new Action(delegate ()
								{
									lblResumen.Text = "" + "Proceso Crear PDF: " + oEmpresa.Empresa_Id + "-" + oEmpresa.sEmpBreve + "\n documento: " + item.sTraSerie + "-" + item.sTraNumero +
									" PDF ERROR-La Transaccion no tiene detalles";
								}));
							}
							else
							{
								lblResumen.Text = "" + "Proceso Crear PDF: " + oEmpresa.Empresa_Id + "-" + oEmpresa.sEmpBreve + "\n documento: " + item.sTraSerie + "-" + item.sTraNumero +
									" PDF ERROR-La Transaccion no tiene detalles";
							}
							#endregion

							LogApplicationNeg.Instance.Guardar(new Exception("Proceso Crear PDF: " + oEmpresa.Empresa_Id + "-" + oEmpresa.sEmpNombre + " documento: " + Transaccion_id + "|" + item.sTraSerie + "-" + item.sTraNumero + "" +
								" La Transacción no tiene detalle"));
						}
                    }
				}

                return true;
            }
            catch (Exception ex)
            {
				LogApplicationNeg.Instance.GuardarLogAplicacion(new Exception((ex.Message+"Crear PDF: "+Transaccion_id +""+ serie+"-"+numero)));
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

                RUTA_LOGO_EMPRESA = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_EMPRESA_LOGO_PDF;

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

        private void frmTransaccion_Load(object sender, EventArgs e)
        {
            try
            {
                radWaitingBar1.StartWaiting();
                dgvTransaccion.MasterTemplate.ShowHeaderCellButtons = true;
                dgvTransaccion.MasterTemplate.ShowFilteringRow = false;
                btnBuscar_Click(null, null);
                //btnEnviar_Click(null, null);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
