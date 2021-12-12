using Entidades.Base;
using System.Xml.Serialization;
using Utilitarios;

namespace Entidades.Comercial
{
    public class TransaccionDetalle : BaseEntidad
    {
        #region Columnas

        [XmlAttribute]
        [PrimaryKey]
        public int TransaccionDetalle_Id { get; set; }

        [XmlAttribute]
        [ForeignKey]
        public int Transaccion_Id { get; set; }

        [XmlAttribute]
        [Field]
        public int Producto_Id { get; set; }

        [XmlAttribute]
        [Field]
        public string sProNombre { get; set; }

        [XmlAttribute]
        [ForeignKey]
        public decimal nTDeCantidad { get; set; }

        [XmlAttribute]
        [Field]
        public decimal nTDePrecio { get; set; }

        [XmlAttribute]
        [Field]
        public decimal nTDeBase { get; set; }

        [XmlAttribute]
        [Field]
        public decimal nTDeIGV { get; set; }

		[XmlAttribute]
        [Field]
        public decimal nTDeICBPER { get; set; }

        [XmlAttribute]
        [Field]
        public decimal nTDeSubtotal { get; set; }

        [XmlAttribute]
        [Field]
        public int nTDeEstadoIGV { get; set; }

        [XmlAttribute]
        [Field]
        public int nTDeEstado { get; set; }

        #endregion

        #region Otros

        [XmlAttribute]
        public string sTDeEstado { get; set; }

        /// <summary>
        /// texto con el Codigo de Sunat para la
        /// facturacion electronica
        /// </summary>
        [XmlAttribute]
        [Field]
        public string sUMeCodigoSunat { get; set; }

        /// <summary>
        /// Nombre de la unidad de medida
        /// </summary>
        [XmlAttribute]
        public string sUMeDescripcion { get; set; }


        #region Campos para Gestión de Nota de Credito

        #endregion


        public byte[] iLogo { get; set; }

        public byte[] iCodHash { get; set; }


        #endregion

        #region Constructor

        public TransaccionDetalle()
        {
            sTDeEstado = "";
            sUMeCodigoSunat = string.Empty;
            sUMeDescripcion = string.Empty;
			nTDeICBPER = 0;
			nTDeIGV = 0;
		}

        #endregion

        #region Serializar XML

        public override string SerializarXML()
        {
            return Util.SerializeXML<TransaccionDetalle>(this);
        }

        #endregion


    }
}