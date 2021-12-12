using Entidades.Base;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Utilitarios;

namespace Entidades.Comercial
{
    public class Transaccion: BaseEntidad
    {
       
        #region columnas

        [XmlAttribute]
        [PrimaryKey]
        public int Transaccion_Id { get; set; }

        [XmlAttribute]
        [ForeignKey]
        public int TipoDocumento_Id { get; set; }

        /// <summary>
        /// Número completo Serie-numero de transaccion
        /// con la finalidad de mostrar en la grilla
        /// </summary>
        public string sNumeroGenerado
        {
            get { return sTraSerie + "-" + sTraNumero; }
        }


        [XmlAttribute, Field]
        public int Empresa_Id { get; set; }

        [XmlAttribute, Field]
        public int Moneda_Id { get; set; }

        [XmlAttribute, Field]
        public int nTraEsGratuito { get; set; }

        /// <summary>
        /// Sumatoria de Bases sin igv
        /// </summary>
        [XmlAttribute, Field]
        public decimal nTraSubTotalSinIGV { get; set; }

        [XmlAttribute, Field]
        public int nTraEsAnticipo { get; set; }

        [XmlAttribute, Field]
        public decimal nTraImporteTotal { get; set; }

        [XmlAttribute, Field]
        public DateTime dTraFecha { get; set; }

        [XmlAttribute, Field]
        public string nCReCodigoSunat { get; set; }

        [XmlAttribute, Field]
        public string sCategoriaReclamo { get; set; }

        /// <summary>
        /// dni o ruc del cliente receptor
        /// </summary>
        [XmlAttribute, Field]
        public string sCliNroIdentidad { get; set; }

        [XmlAttribute, Field]
        public string sCliDomicilioFiscal { get; set; }

        [XmlAttribute, Field]
        public string sCliUbigeoNombre { get; set; }

        /// <summary>
        /// abreviatura del pais del cliente ejemplo PE
        /// </summary>
        [XmlAttribute, Field]
        public string sCliPaisBreve { get; set; }

        [XmlAttribute, Field]
        public string sCliente { get; set; }

        /// <summary>
        /// igv total del documento
        /// </summary>
        [XmlAttribute, Field]
        public decimal nTraMontoIGV { get; set; }

        /// <summary>
        /// serie del documento de la empresa emisora
        /// </summary>
        [XmlAttribute, Field]
        public string sTraSerie { get; set; }

        /// <summary>
        /// numero del documento de la empresa emisora
        /// </summary>
        [XmlAttribute, Field]
        public string sTraNumero { get; set; }

        /// <summary>
        /// Referencia a documentos electrónicos declarados
        /// en sunat 0: No enviado a sunat
        ///          1: Enviado a sunat
        /// </summary>
        [XmlAttribute, Field]
        public int nTraEstadoTransaccionElectronica { get; set; }


        /// <summary>
        /// Codigo Motivo del traslado  -Catálogo N° 20
        /// </summary>
        [XmlAttribute, Field]
        public string sTraCodigoMotivoTraslado { get; set; }


        /// <summary>
        /// Texto del motivo de traslado - Catalogo Nro20 sunat
        /// </summary>
        [XmlAttribute, Field]
        public string sTraMotivoTraslado { get; set; }

        /// <summary>
        /// codigo de la unidad de medida del traslado
        /// </summary>
        [XmlAttribute, Field]
        public string sTraCodigoUnidadMedidaTraslado { get; set; }

        /// <summary>
        /// decimal peso bruto de la guia de remision
        /// </summary>
        [XmlAttribute, Field]
        public decimal nTraPesoBrutoGuiaTraslado { get; set; }

        /// <summary>
        /// Modalidad de Traslado Catalogo Nro 18 01:Publico; 02:Privado
        /// </summary>
        [XmlAttribute, Field]
        public string sTraCodigoModalidadTraslado { get; set; }

        /// <summary>
        /// Fecha de inicio del Traslado
        /// </summary>
        [XmlAttribute, Field]
        public DateTime dTraFechaTraslado { get; set; }

        /// <summary>
        /// Ruc del proveedor Transportista quien realiza el traslado
        /// </summary>
        [XmlAttribute, Field]
        public string sTraRUCTransportistaTraslado { get; set; }

        /// <summary>
        /// Nombre de quien realiza el traslado nombre del proveedor transportista
        /// </summary>
        [XmlAttribute, Field]
        public string sTraNombreTransportistaTraslado { get; set; }

        /// <summary>
        /// Placa del vehiculo de traslado
        /// </summary>
        [XmlAttribute, Field]
        public string sTraPlacaVehiculoTraslado { get; set; }

        /// <summary>
        /// DNI del conductor
        /// </summary>
        [XmlAttribute, Field]
        public string sTraDNIConductorTraslado { get; set; }

        /// <summary>
        /// ubigeo en codigo sunar para el punto de llegada
        /// </summary>
        [XmlAttribute, Field]
        public string sTraCodigoUbigeoPuntoLlegadaTraslado { get; set; }
         
        /// <summary>
        /// Direccion de llegada del traslado
        /// </summary>
        [XmlAttribute, Field]
        public string sTraDireccionPuntoLlegadaTraslado { get; set; }

        /// <summary>
        /// ubigeo codigo sunat del punto de origen o partida
        /// </summary>
        [XmlAttribute, Field]
        public string sTraCodigoUbigeoPuntoPartidaTraslado { get; set; }

        /// <summary>
        /// Direccion del punto de partida o origen
        /// </summary>
        [XmlAttribute, Field]
        public string sTraDireccionPuntoPartidaTraslado { get; set; }
        
        /// <summary>
        /// Corresponde al codigo de sunat para este tipo de documento
        /// </summary>
        [XmlAttribute, Field]
        public string sTDoCodigoSunat { get; set; }

        [XmlAttribute, Field]
        public string sCliTipoDocumentoSunat { get; set; }
        
        
        [XmlAttribute, Field]
        public string sTraTextoQR { get; set; }

        /// <summary>
        ///  0: Inactivo; 1:Activo
        /// </summary>
        [XmlAttribute, Field]
        public int nTraEstado { get; set; }

        #endregion
       

        /// <summary>
        /// Estado de la factura electronica
        /// </summary>
        [XmlAttribute]
        public string sTraEstadoTransaccionElectronica { get; set; }

        #region Campos para la Facturacion Electrónica

        /// <summary>
        /// Indica si a la factura se le aplica la detraccion
        /// 0: No Tiene Detracción
        /// 1: Si Tiene Detraccion
        /// </summary>
        [XmlAttribute, Field]
        public int nTraTieneDetraccion { get; set; }

        /// <summary>
        /// % de Detraccion del producto en en el detalle que tiene
        /// el mayor porcentaje de detraccion esto es calculado en el
        /// momento de realizar la conversion en el xml
        /// </summary>
        [XmlAttribute, Field]
        public decimal nTraPctjDetraccion { get; set; }

        /// <summary>
        /// Monto de la Detraccion que viene a ser
        /// El Total de la factura por el MAYOR % pocentaje de
        /// detraccion de los Productos de la Factura
        /// </summary>
        [XmlAttribute, Field]
        public decimal nTraMontoDetraccion { get; set; }

        #endregion


        /*CAMPOS DE NOTA DE CREDITO VIENEN DESDE EL STORE*/
        /// <summary>
        /// Tipo de documento relacionado
        /// </summary>
        [XmlAttribute, Field]
        public int nTipoDocRelacionado { get; set; }

        /// <summary>
        /// Indica el numero de serie de un relacionado
        /// en el caso de notas de credito este campo se llena con
        /// el numero de serie + numero correlativo de la 
        /// factura asociada
        /// </summary>
        [XmlAttribute, Field]
        public string sSerieNumeroRelacionado { get; set; }

        /// <summary>
        /// Contienen los correos a quienes se les enviará el email
        /// estos correos son separados concatenados por un punto y coma
        /// </summary>
        [XmlAttribute, Field]
        public string sTraEmail { get; set; }

        /// <summary>
        /// Texto del documento, este texto debe de ir en la Factura, boleta, nc
        /// en el PDF
        /// </summary>
        [XmlAttribute, Field]
        public string sTraObservaciones { get; set; } 
        
        [XmlAttribute, Field]
        public string sTraCodigoRespuestaSunat { get; set; } 
        
        
        [XmlAttribute, Field]
        public string sTraRespuestaValidacion { get; set; } 
        
        
        [XmlAttribute, Field]
        public string sTraNroTicket { get; set; }

        //sTraRUCEmpresa

        /// <summary>
        /// RUC DE LA EMPRESA EMISORA DE FE
        /// </summary>
        [XmlAttribute]
        public string sTraRUCEmpresa { get; set; }

        [XmlAttribute]
        public string sEmpNombre { get; set; }

        [XmlAttribute]
        public string sTraTipoDocumento { get; set; } 
        
       
		#region  IMPUESTO ICBPER

		[XmlAttribute]
		public int nTraICBPER { get; set; }

		[XmlAttribute]
		public decimal nTraMontoICBPER { get; set; }

		[XmlAttribute]
		public int nCliImportacion { get; set; }

        #endregion


        #region Listas

        public List<TransaccionDetalle> LstTransaccionDetalle { get; set; }
        public List<FormaPago> LstFormaPago { get; set; }

        #endregion



        /// <summary>
        /// tipo de identidad del cliente 
        /// para soporte a clientes extranjeros
        /// 13-02-2021
        /// </summary>
        [XmlAttribute, Field]
        public string sCliTipoIdentidad { get; set; }


        /// <summary>
        /// numero de telefono del cliente
        /// en caso se reciba desde la api y
        /// va en el xml
        /// </summary>
        [XmlAttribute, Field]
        public string sCliTelefono { get; set; }


        [XmlAttribute, Field]
        public string sCliEmail { get; set; }


        /// <summary>
        /// 1:Contado
        /// 2:Credito
        /// </summary>
        [XmlAttribute, Field]
        public int nTraFormaPago { get; set; }





        #region Constructor

        public Transaccion()
        {
            nTraEsAnticipo = 0;
            LstTransaccionDetalle = new List<TransaccionDetalle>();
            LstFormaPago = new List<FormaPago>();
            nTipoDocRelacionado = 0;
            sSerieNumeroRelacionado = string.Empty;
            nTraEsGratuito = 0;
			nTraICBPER = 0;
            nTraImporteTotal = 0;
			nTraMontoICBPER = 0;
			nCliImportacion = 0;
            sEmpNombre = string.Empty;
            sTraObservaciones = string.Empty;
            sTDoCodigoSunat = string.Empty;
            sTraCodigoRespuestaSunat = string.Empty;
            ///13-02-2021
            sCliTipoIdentidad = string.Empty;
            sCliTelefono = string.Empty;
            sCliEmail = string.Empty;
            sTraRespuestaValidacion = string.Empty;
        }

        #endregion

        #region Serializar XML

        public override string SerializarXML()
        {
            return Util.SerializeXML<Transaccion>(this);
        }

        #endregion
    }
}