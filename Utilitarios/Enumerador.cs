using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilitarios
{
    public static class Enumerador
    {
        /// <summary>
        /// APLICACION EN
        /// 0:PRUEBAS
        /// 1:PRODUCCION
        /// </summary>
        public static int ESTADO_PRODUCCION = 1;

        public static int ESTADO_ACTIVO = 1;
        public static int ESTADO_INACTIVO = 0;

        public static int MONEDA_NACIONAL_SOLES_ID = 1; 
        public static int MONEDA_EXTRANJERA_DOLARES_ID = 2;


        public static string TEMA_VS2010LIGHT = "VisualStudio2012Light";
        public static string TEMA_VS2012DARK = "VisualStudio2012Dark";
        public static string TEMA_TELERIK_METRO_BLUE = "TelerikMetroBlue";
        public static string TEMA_TELERIK_METRO = "TelerikMetro";
        public static string TEMA_TELERIK_METRO_TOUCH = "TelerikMetroTouch";
        public static string TEMA_TELERIK_MATERIAL = "Material";
        public static string TEMA_TELERIK_MATERIAL_TEAL = "MaterialTeal";
        public static string TEMA_TELERIK_MATERIAL_PINK = "MaterialPink";
        public static string TEMA_TELERIK_MATERIAL_BLUE_GRAY = "MaterialBlueGrey";
        public static string TEMA_TELERIK_WINDOWS8 = "Windows8";
        public static string TEMA_HIGHCONTRASTBLACK = "highContrastBlack";



        #region RESPUESTAS DEL API A LAS CONSULTAS

        /// <summary>
        /// se ha insertado en la base de datos pero no se ha podido enviar
        /// a sunat
        /// </summary>
        public static int RESPUESTA_INSERTADO_BD = 0;

        /// <summary>
        /// Enviado a sunat correctamente
        /// </summary>
        public static int RESPUESTA_ENVIADO_SUNAT = 1;

        /// <summary>
        /// HA SIDO ENVIADO A SUNAT Y SE HA CREADO CORRECTAMENTE EL PDF DEL DOCUMENTO
        /// </summary>
        public static int RESPUESTA_ENVIADO_PDF_CREADO = 2;

        /// <summary>
        /// SE HA ENVIADO AL CORREO DEL CLIENTE DE FORMA EXITOSA
        /// </summary>
        public static int RESPUESTA_ENVIADO_EMAIL_CLIENTE = 3;

        /// <summary>
        /// no se pudo insertar en la base de datos
        /// </summary>
        public static int RESPUESTA_ERROR_INSERTADO_BD = -1;

        #endregion

        #region ESTADO BOLETAS ELECTRONICAS SEGUN SUNAT PARA EL RESUMEN

        public static string ESTADO_BOLETA_ELECTRONICA_ACTIVA = "1";
        public static string ESTADO_BOLETA_ELECTRONICA_MODIFICADO = "2";
        public static string ESTADO_BOLETA_ELECTRONICA_BAJA = "3";

        #endregion

        /// <summary>
        /// nombre del end point de produccion
        /// </summary>
        public static string SERVICIO_SUNAT = "BillServicePortProduccion";

        /// <summary>
        /// nombre del end point pruebas
        /// </summary>
        public static string SERVICIO_SUNAT_PRUEBAS = "BillServicePortBeta";

        /// <summary>
        /// nombre del end point de produccion para guias de remision
        /// </summary>
        public static string SERVICIO_SUNATGR = "BillServicePortProduccionGR";

        /// <summary>
        /// nombre del end point pruebas para guias de remision
        /// </summary>
        public static string SERVICIO_SUNAT_PRUEBASGR = "BillServicePortBetaGR";


		/// <summary>
		/// nombre del end point pruebas
		/// </summary>
		public static string SERVICIO_OSE_PRUEBAS = "BillServiceImplPortBetaOSE";

		public static string SERVICIO_OSE_PRODUCCION = "BillServiceImplPortProduccionOSE";


		public enum FACTURA_ELECTRONICA
        {
            #region TIPO DOCUMENTO

            
            TIPO_DOCUMENTO_FACTURA = 1,
            TIPO_DOCUMENTO_BOLETA = 2,
            TIPO_DOCUMENTO_NOTA_CREDITO = 3,
            TIPO_DOCUMENTO_NOTA_DEBITO = 4,
            TIPO_DOCUMENTO_GUIA_REMISION = 7,
            TIPO_DOCUMENTO_LIQUIDACION_COMPRA = 8,

            /// <summary>
            /// exclusivo para el correlativo electronico
            /// de bajas de sunat y es reflejado en Documento Impresion
            /// </summary>
            /*-----------------------------------------------------------*/
            #region CORRELATIVOS SUNAT
            TIPO_DOCUMENTO_BAJA_DOC_ELECTRONICO_SUNAT = 5,
            TIPO_DOCUMENTO_CORRELATIVO_DOC_ELECTRONICO_BOLETA_SUNAT = 6,
            #endregion
            /*----------------------------------------------------------*/

            #endregion

            #region ESTADOS DE DOCUMENTO FACTURACION ELECTRONICA

            /// <summary>
            /// Pendiente de ser enviado a suant
            /// </summary>
            ESTADO_DOC_ELECTRONICO_PENDIENTE = 0,

            /// <summary>
            /// El documento electronico ya se ha enviado a sunat
            /// </summary>
            ESTADO_DOC_ELECTRONICO_ENVIADO_SUNAT = 1,

            /// <summary>
            /// Se ha generado un pdf del documento electronico
            /// </summary>
            ESTADO_DOC_ELECTRONICO_PDF_GENERADO = 2,

            /// <summary>
            /// el pdf generado ya se ha enviado al cliente
            /// </summary>
            ESTADO_DOC_ELECTRONICO_ENVIADO_CLIENTE = 3,

            /// <summary>
            /// El documento electronico ya se ha enviado y aceptado para su baja en sunat
            /// </summary>
            ESTADO_DOC_ELECTRONICO_BAJA_ENVIADA_SUNAT = 4,

            #endregion

            #region TIPO EXCEPCION IGV

            TIPO_IGV_GRABADO = 1,
            TIPO_IGV_EXONERADO = 2,
            TIPO_IGV_NO_GRABADO = 3,

            #endregion

        }


        #region RUTAS DE CARPETAS DE LA FE

        public static string RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_FACTURAS =  @"\FacturacionElectronicaXml\FacturasElectronicas\";
        public static string RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_FACTURAS_RESPUESTA =  @"\FacturacionElectronicaXml\FacturasElectronicasResp\";

        public static string RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_BOLETAS =  @"\FacturacionElectronicaXml\ResumenDiarioBoletasElectronicas\";
        public static string RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_BOLETAS_RESPUESTA =  @"\FacturacionElectronicaXml\ResumenDiarioBoletasElectronicasResp\";

        public static string RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_CREDITO =  @"\FacturacionElectronicaXml\NotaCredito\";
        public static string RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_CREDITO_RESPUESTA =  @"\FacturacionElectronicaXml\NotaCreditoResp\";

        public static string RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_GUIA_REMISION = @"\FacturacionElectronicaXml\GuiaRemision\";
        public static string RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_GUIA_REMISION_RESPUESTA = @"\FacturacionElectronicaXml\GuiaRemisionResp\";

        public static string RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_DEBITO =  @"\FacturacionElectronicaXml\NotaDebito\";
        public static string RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_DEBITO_RESPUESTA =  @"\FacturacionElectronicaXml\NotaDebitoResp\";


        public static string RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_LIQUIDACION_COMPRA = @"\FacturacionElectronicaXml\LiquidacionCompra\";
        public static string RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_LIQUIDACION_COMPRA_RESPUESTA = @"\FacturacionElectronicaXml\LiquidacionCompraResp\";


        public static string RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_BAJAS = @"\FacturacionElectronicaXml\BajaFacturasElectronicas\";
        public static string RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_BAJAS_RESPUESTA =  @"\FacturacionElectronicaXml\BajaFacturaElectronicasResp\";

        public static string RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_CERTIFICADO_RUTA = @"\FacturacionElectronicaXml\certificados\";


		public static string RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_TICKETS_RESPUESTA = @"\FacturacionElectronicaXml\ConsultaTickets\";


		#endregion



		#region RUTAS DE CARPETA DE PDF DE FE

		/// <summary>
		/// Ruta del logo incluido el nombre: todos los logos de todas las empresas deben llamarse Logo con extension .png
		/// </summary>
		public static string RUTA_SERVIDOR_EMPRESA_LOGO_PDF = @"\PDF\Logo\Logo.png";

        /// <summary>
        /// Ruta de la Factura fisica en formato pdf
        /// </summary>
        public static string RUTA_SERVIDOR_FACTURAS_ELECTRONICAS_PDF = @"\PDF\FacturasElectronicas\";

        /// <summary>
        /// Ruta de la Boleta Física en formato pdf
        /// </summary>
        public static string RUTA_SERVIDOR_BOLETAS_ELECTRONICAS_PDF = @"\PDF\BoletasElectronicas\";

        /// <summary>
        /// Ruta de la Nota de Crédito fisica en formato pdf
        /// </summary>
        public static string RUTA_SERVIDOR_NOTA_CREDITO_ELECTRONICAS_PDF = @"\PDF\NotaCredito\";

        /// <summary>
        /// Ruta de la Nota de Debito fisica en formato pdf
        /// </summary>
        public static string RUTA_SERVIDOR_NOTA_DEBITO_ELECTRONICAS_PDF = @"\PDF\NotaDebito\";



        /// <summary>
        /// Ruta de la Guia de remision en formato pdf
        /// </summary>
        public static string RUTA_SERVIDOR_GUIA_REMISION_ELECTRONICAS_PDF = @"\PDF\GuiaRemision\";

        #endregion


    }
}
