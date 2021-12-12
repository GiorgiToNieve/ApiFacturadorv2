using Entidades.Comercial;
using System;
using System.Data;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.IO;
using System.Linq;
using System.Web;
using Utilitarios;
using Entidades.Maestros;
using System.Collections.Generic;
using System.ComponentModel;
using Negocio.Comercial;

namespace WSApivnieve.Sunat
{
    public class PDF
    {
        private Transaccion oTransaccionVenta = new Transaccion();
        private CrystalDecisions.Windows.Forms.CrystalReportViewer rpvVista;
        Empresa oEmpresa = new Empresa();

        #region GENERAR DATATABLE GENERICO
        public DataTable GenerarDataTable<T>(List<T> iList)
        {
            try
            {
                var dataTable = new DataTable();
                var propertyDescriptorCollection = TypeDescriptor.GetProperties(typeof(T));
                for (int i = 0; i < propertyDescriptorCollection.Count; i++)
                {
                    var propertyDescriptor = propertyDescriptorCollection[i];
                    var type = propertyDescriptor.PropertyType;

                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        type = Nullable.GetUnderlyingType(type);

                    dataTable.Columns.Add(propertyDescriptor.Name, type);
                }
                var values = new object[propertyDescriptorCollection.Count];

                foreach (var iListItem in iList)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        values[i] = propertyDescriptorCollection[i].GetValue(iListItem);
                    }
                    dataTable.Rows.Add(values);
                }
                return dataTable;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region ENVIAR CORREO CON EL PDF AL CLIENTE

        /// <summary>
        /// Envia el pdf generado del documento electronico al correo especificado
        /// en este caso al correo del cliente
        /// </summary>
        /// <param name="oTransaccionVenta"></param>
        /// <param name="p_oEmpresa"></param>
        /// <returns></returns>
        public bool EnviarPDFCorreo(Transaccion oTransaccionVenta, Empresa p_oEmpresa,string strNombreArchivo)
        {
            try
            {
                string RUTA_PDF = p_oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_FACTURAS_ELECTRONICAS_PDF;
                string sAsunto = oTransaccionVenta.sTraSerie + "-" + oTransaccionVenta.sTraNumero + " Validada en Sunat";
                //string sMensajeTexto = "Estimado Cliente, Su factura ha sido emitida a SUNAT";
                string sMensajeTexto = "Su comprobante electrónico " + oTransaccionVenta.sTraSerie + "-" + oTransaccionVenta.sTraNumero +
                                        ", Se encuentra validado en SUNAT. Anexamos su representación en PDF y XML";

                if (oTransaccionVenta.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
                {
                    RUTA_PDF = p_oEmpresa.sEmpRuta +  Enumerador.RUTA_SERVIDOR_BOLETAS_ELECTRONICAS_PDF;
                    //sAsunto = "Te ha llegado una Boleta Electrónica";
                    //sMensajeTexto = "Estimado Cliente, Su Boleta ha sido emitida a SUNAT";
                }
                else if (oTransaccionVenta.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
                {
                    RUTA_PDF = p_oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_NOTA_CREDITO_ELECTRONICAS_PDF;
                    //sAsunto = "Te ha llegado una Nota de Crédito Electrónica";
                    //sMensajeTexto = "Estimado Cliente, Su Nota de Crédito ha sido emitida a SUNAT";
                }
                else if (oTransaccionVenta.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO)
                {
                    RUTA_PDF = p_oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_NOTA_DEBITO_ELECTRONICAS_PDF;
                    //sAsunto = "Te ha llegado una Nota de Débito Electrónica";
                    //sMensajeTexto = "Estimado Cliente, Su Nota de Débito ha sido emitida a SUNAT";
                }

                RUTA_PDF =  RUTA_PDF +
                           strNombreArchivo + @"\" +
                           strNombreArchivo + ".pdf";

                #region ADICIONAR ARCHIVO XML DEL PDF DE LA FACTURA

                if (oTransaccionVenta.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA)
                {
                    string RUTA_XML = string.Empty;

                    RUTA_XML = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_FACTURAS +
                               strNombreArchivo + @"\" +
                               strNombreArchivo + ".xml";

                    if (File.Exists(RUTA_XML))
                    {
                        RUTA_PDF = RUTA_PDF + ";" + RUTA_XML;
                    }

                }
                #endregion

                else
                    if (oTransaccionVenta.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
                {
                    string RUTA_XML = string.Empty;

                    RUTA_XML = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_BOLETAS +
                               strNombreArchivo + @"\" +
                               strNombreArchivo + ".xml";

                    if (File.Exists(RUTA_XML))
                    {
                        RUTA_PDF = RUTA_PDF + ";" + RUTA_XML;
                    }
                }
                else
                        if (oTransaccionVenta.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
                {
                    string RUTA_XML = string.Empty;

                    RUTA_XML = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_CREDITO +
                               strNombreArchivo + @"\" +
                               strNombreArchivo + ".xml";

                    if (File.Exists(RUTA_XML))
                    {
                        RUTA_PDF = RUTA_PDF + ";" + RUTA_XML;
                    }
                }

                TransaccionNeg.Instance.EnviarCorreo(RUTA_PDF,
                    p_oEmpresa,
                    oTransaccionVenta,
                   sAsunto, sMensajeTexto
                    );

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion


        #region FACTURA
        public bool ReportarFactura(DataTable dtSource, Transaccion oTransaccion, string strNombreArchivo, Empresa oEmpresa)
        {
            try
            {
                oTransaccionVenta = null;
                oTransaccionVenta = oTransaccion;

                var reporte = new rptFormatoFactura();
                return MostrarReporteFacturas(dtSource, reporte, strNombreArchivo, oEmpresa);
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region BOLETA
        public bool ReportarBoleta(DataTable dtSource, Transaccion oTransaccion, string strNombreArchivo, Empresa oEmpresa)
        {
            try
            {
                oTransaccionVenta = null;
                oTransaccionVenta = oTransaccion;

                var reporte = new rptFormatoBoleta();
                return MostrarReporteBoletas(dtSource, reporte, strNombreArchivo, oEmpresa);
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region NOTA DE CREDITO
        public bool ReportarNotaCredito(DataTable dtSource, Transaccion oTransaccion, string strNombreArchivo, Empresa oEmpresa)
        {
            try
            {
                oTransaccionVenta = null;
                oTransaccionVenta = oTransaccion;

                var reporte = new rptFormatoNotas();
                return MostrarReporte(dtSource, reporte, strNombreArchivo, oEmpresa);
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region NOTA DE DEBITO
        /// <summary>
        /// NO ESTA CONTEMPLADO AUN
        /// </summary>
        /// <param name="dtSource"></param>
        /// <param name="oTransaccion"></param>
        /// <param name="strNombreArchivo"></param>
        /// <param name="oEmpresa"></param>
        /// <returns></returns>
        public bool ReportarNotaDebito(DataTable dtSource, Transaccion oTransaccion, string strNombreArchivo, Empresa oEmpresa)
        {
            try
            {
                oTransaccionVenta = null;
                oTransaccionVenta = oTransaccion;

                var reporte = new rptFormatoFactura();
                return false; //NO SE TIENE CONTEMPLADO // MostrarReporte(dtSource, reporte, strNombreArchivo);
            }
            catch
            {
                return false;
            }
        }
        #endregion


        private bool MostrarReporteFacturas(DataTable dtSource, rptFormatoFactura sReporte, string strNombreArchivo, Empresa p_oEmpresa)
        {
            try
            {
                //igualando a la variable global
                oEmpresa = p_oEmpresa;

                Directory.CreateDirectory(oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_FACTURAS_ELECTRONICAS_PDF +
                                          strNombreArchivo);


                string RUTA_PDF = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_FACTURAS_ELECTRONICAS_PDF +
                                  strNombreArchivo + @"\" +
                                  strNombreArchivo + ".pdf";

                rpvVista = new CrystalDecisions.Windows.Forms.CrystalReportViewer();

                sReporte.SetDataSource(dtSource);
                sReporte.RefreshReport += sReporte_RefreshReport;

                rpvVista.ReportSource = sReporte;
                sReporte.Refresh();
                sReporte.ExportToDisk(ExportFormatType.PortableDocFormat, RUTA_PDF);


                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sReporte_RefreshReport(object sender, EventArgs e)
        {
            try
            {
                /*oEmpresa-> Se carga en el llamado como variable global para que pueda pasar el método*/

                var sReporte = ((ReportDocument)sender);
                var pctj = string.Empty;
                var nTotalIGV = oTransaccionVenta.LstTransaccionDetalle.Sum(x => x.nTDeIGV);
                if (oTransaccionVenta.nTraTieneDetraccion == 1)
                    pctj = "Comprobante sujeto a Detracción del " + Decimal.Round(oTransaccionVenta.nTraPctjDetraccion, 2) +
                           " % " + "(" + Decimal.Round(oTransaccionVenta.nTraMontoDetraccion, 2) + " " +
                           (oTransaccionVenta.Moneda_Id == Enumerador.MONEDA_NACIONAL_SOLES_ID ? "Soles" : "Dólares") + ") N° de cta. en el BN: " + oEmpresa.sEmpCtaBancoDetracciones;

                #region Parametros

                sReporte.SetParameterValue("prmCliente", oTransaccionVenta.sCliente);
                sReporte.SetParameterValue("prmRucCliente", oTransaccionVenta.sCliNroIdentidad);
                sReporte.SetParameterValue("prmDireccionCliente", oTransaccionVenta.sCliDomicilioFiscal);
                sReporte.SetParameterValue("prmRuc", oEmpresa.sEmpRuc);
                sReporte.SetParameterValue("prmNumeroCorrelativo", oTransaccionVenta.sNumeroGenerado);
                sReporte.SetParameterValue("prmFechaEmisión", oTransaccionVenta.dTraFecha);
                sReporte.SetParameterValue("prmGuiaEmision", "");
                sReporte.SetParameterValue("prmTipoMoneda", oTransaccionVenta.Moneda_Id == Enumerador.MONEDA_NACIONAL_SOLES_ID ? "Soles" : "Dólares Americanos");
                sReporte.SetParameterValue("prmSon", Util.ConvertirNumerosAletras(oTransaccionVenta.nTraImporteTotal, oTransaccionVenta.Moneda_Id == Enumerador.MONEDA_NACIONAL_SOLES_ID ? "Soles" : "Dólares Americanos"));
                sReporte.SetParameterValue("prmTotalPercepcion", 0);
                sReporte.SetParameterValue("prmTotalGravado", oTransaccionVenta.nTraSubTotalSinIGV);
                sReporte.SetParameterValue("prmTotalNoGravado", 0);
                sReporte.SetParameterValue("prmTotalExonerado", 0);
                sReporte.SetParameterValue("prmDescripcionPercepción", pctj);
                sReporte.SetParameterValue("prmTotalIgv", nTotalIGV);
                sReporte.SetParameterValue("prmAutorizacion", "");
                sReporte.SetParameterValue("prmConsideradoAgenteRetencion", "");
                sReporte.SetParameterValue("prmImporteTotal", oTransaccionVenta.nTraImporteTotal);
                sReporte.SetParameterValue("prmTipoMonedaEntero", oTransaccionVenta.Moneda_Id);
                sReporte.SetParameterValue("prmObservaciones", oTransaccionVenta.sTraObservaciones);
                sReporte.SetParameterValue("prmDireccionEmpresa", oEmpresa.sEmpDireccion);

                #endregion
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtSource"></param>
        /// <param name="sReporte"></param>
        /// <param name="strNombreArchivo"></param>
        /// <returns></returns>
        private bool MostrarReporte(DataTable dtSource, rptFormatoNotas sReporte, string strNombreArchivo, Empresa p_oEmpresa)
        {
            try
            {

                oEmpresa = p_oEmpresa;

                Directory.CreateDirectory(oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_NOTA_CREDITO_ELECTRONICAS_PDF +
                                          strNombreArchivo);


                string RUTA_PDF = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_NOTA_CREDITO_ELECTRONICAS_PDF +
                                  strNombreArchivo + @"\" +
                                  strNombreArchivo + ".pdf";

                rpvVista = new CrystalDecisions.Windows.Forms.CrystalReportViewer();

                sReporte.SetDataSource(dtSource);
                sReporte.RefreshReport += sReporteNotas_RefreshReport;

                rpvVista.ReportSource = sReporte;
                sReporte.Refresh();
                sReporte.ExportToDisk(ExportFormatType.PortableDocFormat, RUTA_PDF);


                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool MostrarReporteBoletas(DataTable dtSource, rptFormatoBoleta sReporte, string strNombreArchivo, Empresa p_oEmpresa)
        {
            try
            {
                //igualando a la variable global
                oEmpresa = p_oEmpresa;

                Directory.CreateDirectory(oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_BOLETAS_ELECTRONICAS_PDF +
                                          strNombreArchivo);


                string RUTA_PDF = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_BOLETAS_ELECTRONICAS_PDF +
                                  strNombreArchivo + @"\" +
                                  strNombreArchivo + ".pdf";

                rpvVista = new CrystalDecisions.Windows.Forms.CrystalReportViewer();

                sReporte.SetDataSource(dtSource);
                sReporte.RefreshReport += sReporte_RefreshReport;

                rpvVista.ReportSource = sReporte;
                sReporte.Refresh();
                sReporte.ExportToDisk(ExportFormatType.PortableDocFormat, RUTA_PDF);


                return true;
            }
            catch
            {
                return false;
            }
        }


        private void sReporteNotas_RefreshReport(object sender, EventArgs e)
        {
            try
            {
                var sReporte = ((ReportDocument)sender);
                var pctj = string.Empty;
                var nTotalIGV = oTransaccionVenta.LstTransaccionDetalle.Sum(x => x.nTDeIGV);
                if (oTransaccionVenta.nTraTieneDetraccion == 1)
                    pctj = "Comprobante sujeto a Detracción del " + Decimal.Round(oTransaccionVenta.nTraPctjDetraccion, 2) +
                           " % " + "(" + Decimal.Round(oTransaccionVenta.nTraMontoDetraccion, 2) + " " +
                           (oTransaccionVenta.Moneda_Id== Enumerador.MONEDA_NACIONAL_SOLES_ID? "Soles": "Dólares Americanos") + ") N° de cta. en el BN: " + oEmpresa.sEmpCtaBancoDetracciones;

                #region Parametros

                sReporte.SetParameterValue("prmCliente", oTransaccionVenta.sCliente);
                sReporte.SetParameterValue("prmDireccionCliente", oTransaccionVenta.sCliDomicilioFiscal);
                sReporte.SetParameterValue("prmRucCliente", oTransaccionVenta.sCliNroIdentidad);

                if (oTransaccionVenta.sCliNroIdentidad.Length > 8)
                {
                    sReporte.SetParameterValue("prmDocumentoCliente", "RUC");
                }
                else
                {
                    sReporte.SetParameterValue("prmDocumentoCliente", "DNI");
                }

                sReporte.SetParameterValue("prmRuc", oEmpresa.sEmpRuc);
                sReporte.SetParameterValue("prmNumeroCorrelativo", oTransaccionVenta.sNumeroGenerado);
                sReporte.SetParameterValue("prmFechaEmisión", oTransaccionVenta.dTraFecha);
                sReporte.SetParameterValue("prmTipoMoneda", (oTransaccionVenta.Moneda_Id == Enumerador.MONEDA_NACIONAL_SOLES_ID ? "Soles" : "Dólares Americanos"));
                sReporte.SetParameterValue("prmSon", Util.ConvertirNumerosAletras(oTransaccionVenta.nTraImporteTotal, (oTransaccionVenta.Moneda_Id == Enumerador.MONEDA_NACIONAL_SOLES_ID ? "Soles" : "Dólares Americanos")));
                sReporte.SetParameterValue("prmTotalGravado", oTransaccionVenta.nTraSubTotalSinIGV);
                sReporte.SetParameterValue("prmTotalIgv", nTotalIGV);
                sReporte.SetParameterValue("prmImporteTotal", oTransaccionVenta.nTraImporteTotal);
                sReporte.SetParameterValue("prmAutorizacion", "");
                sReporte.SetParameterValue("prmConsideradoAgenteRetencion", "");

                /*OTROS NOMBRE SEGUN FORMATO DE NOTAS DE CREDITO*/
                sReporte.SetParameterValue("prmNombreDocumento", "NOTA DE CREDITO ELECTRONICA");
                sReporte.SetParameterValue("prmAfectoDetraccion", " ");

                if (oTransaccionVenta.nTipoDocRelacionado == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA)
                {
                    sReporte.SetParameterValue("prmTipoDocumento", "Factura");
                    sReporte.SetParameterValue("prmNumeroAfecto", "" + oTransaccionVenta.sSerieNumeroRelacionado);
                }
                else
                    if (oTransaccionVenta.nTipoDocRelacionado == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
                {
                    sReporte.SetParameterValue("prmTipoDocumento", "Factura");
                    sReporte.SetParameterValue("prmNumeroAfecto", "" + oTransaccionVenta.sSerieNumeroRelacionado);
                }
                else
                {
                    sReporte.SetParameterValue("prmTipoDocumento", "");
                    sReporte.SetParameterValue("prmNumeroAfecto", "");
                }

                sReporte.SetParameterValue("prmObservaciones", oTransaccionVenta.sTraObservaciones);
                sReporte.SetParameterValue("prmTipoMonedaEntero", oTransaccionVenta.Moneda_Id);
                sReporte.SetParameterValue("prmDireccionEmpresa", oEmpresa.sEmpDireccion);


                #endregion
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}