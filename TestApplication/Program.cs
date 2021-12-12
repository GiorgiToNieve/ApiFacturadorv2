using Negocio.Comercial;
using Negocio.Maestros;
using Sunat;
using WSApivnieve.Sunat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.Comercial;
using Entidades.Maestros;
using Utilitarios;
using System.Drawing;
using System.Data;

namespace TestApplication
{
    public class Program
    {

        static Servicio wsServicio = new Servicio();
        static DocumentoElectronico oDocumentoElectronico = new DocumentoElectronico();
        static WSApivnieve.Sunat.PDF oPDF = new WSApivnieve.Sunat.PDF();

        static void Main(string[] args)
        {

            string ruc = "20482092897";
            string serie = "F100";
            string Numero = "4";

            var oEmpresa = EmpresaNeg.Instance.Consultar(ruc);

            var Parametros = new Dictionary<string, object>();
            Parametros.Add("Empresa_Id", 3);
            Parametros.Add("TipoDocumento_Id", 1);
            Parametros.Add("sTraSerie", serie);
            Parametros.Add("sTraNumero", Numero);

            var Lista = TransaccionNeg.Instance.Consultar(Parametros);

            if (Lista != null && Lista.Count > 0)
            {
                var oTransaccion = Lista.FirstOrDefault();
                GenerarPDF(oTransaccion, oEmpresa);

                string strNombreArchivo = oDocumentoElectronico.GenerarNombreXML(oTransaccion, oEmpresa);
                oPDF.EnviarPDFCorreo(oTransaccion, oEmpresa, strNombreArchivo);
            }
        }


        public  static bool GenerarPDF(Transaccion oTransaccion, Empresa oEmpresa)
        {
            try
            {
                bool b_resultado = false;
                wsServicio.GenerarCodigoHash(oTransaccion, oEmpresa);

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
                if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
                {
                    RUTA_CODIGO_HASH = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_CREDITO +
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
                }

                #endregion

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }


        private static DataTable ObtenerDatosReporte(List<TransaccionDetalle> ListaResultado)
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


    }
}
