using Entidades.Base;
using System.Xml.Serialization;

namespace Entidades.Maestros
{
    public class Empresa: BaseEntidad
    {
        #region "Columnas"

        [XmlAttribute]
        [PrimaryKey]
        public int Empresa_Id { set; get; }

        [XmlAttribute]
        [Field]
        public string sEmpRuc { set; get; }

        [XmlAttribute]
        [Field]
        public string sEmpNombre { set; get; }

        [XmlAttribute]
        [Field]
        public string sEmpBreve { set; get; }

        [XmlAttribute]
        [Field]
        public string sEmpEmail { set; get; }

        [XmlAttribute]
        [Field]
        public string sEmpTelefono { set; get; }

        [XmlAttribute]
        [ForeignKey]
        public int Ubigeo_Id { set; get; }

        /// <summary>
        /// Direccion fiscal de la empresa emisora
        /// </summary>
        [XmlAttribute]
        [Field]
        public string sEmpDireccion { set; get; }


        #region Campos de Login para la Facturacion Electronica



        [XmlAttribute]
        [Field]
        public string sEmpUsuarioFE { set; get; }

        /// <summary>
        /// 
        /// </summary>
        [Field]
        [XmlAttribute]
        public string sEmpPasswordFE { get; set; }

        /// <summary>
        /// Clave del certificado digital de la empresa
        /// </summary>
        [Field]
        [XmlAttribute]
        public string sEmpClaveCertificadoFE { get; set; }

        /// <summary>
        /// ruta raiz donde se guardan las demas carpetas del proceso de FE
        /// </summary>
        [Field]
        [XmlAttribute]
        public string sEmpRuta { get; set; }

        /// <summary>
        /// nombre del certificado digital de la empresa
        /// para que sea localizado por el mismo
        /// </summary>
        [Field]
        [XmlAttribute]
        public string sEmpNombreCertificado { get; set; }


        /// <summary>
        /// 0:Pruebas
        /// 1:Produccion
        /// </summary>
        [Field]
        [XmlAttribute]
        public int nEmpProduccion { get; set; }

        #endregion

        #endregion

        #region "Otros"

        /// <summary>
        /// codigo del ubigeo de la empresa a 4 jerarquias
        /// esto esta seteado desde la base de datos
        /// y esta en duracell va en la fact_elect
        /// </summary>
        [Field]
        [XmlAttribute]
        public string sEmpCodigoUbigeoSunat { set; get; }

        /// <summary>
        /// concatenacion de los ubigeos en jerarquia
        /// de los ubigeos pero de la direccion de operaciones
        /// de la empresa, esta seteadoen el store en duracell
        /// va en la ciudad de la facturacion electronica
        /// </summary>
        [Field]
        [XmlAttribute]
        public string sEmpCiudad { set; get; }

        /// <summary>
        /// campo abreviado del pais en este caso PE de Perú
        /// </summary>
        [Field]
        [XmlAttribute]
        public string sEmpAbreviaturaPais { set; get; }

        /// <summary>
        /// codigo de identificacion de la empresa emisora
        /// por default es 06 y viene tbn desde la base de datos
        /// </summary>
        [Field]
        [XmlAttribute]
        public string sEmpTipoIdentificadorEmpresaSunat { set; get; }

        /// <summary>
        /// codigo de identificacion del cliente por default es 06
        /// y viene desde la base de datos
        /// </summary>
        [Field]
        [XmlAttribute]
        public string sEmpTipoIdentificadorClienteSunat { set; get; }

        /// <summary>
        /// cuenta de banco de la nacion de la empresa para las detracciones
        /// </summary>
        [Field]
        [XmlAttribute]
        public string sEmpCtaBancoDetracciones { set; get; }

        [Field]
        [XmlAttribute]
        public string sEmpCodigoDetraccion { set; get; }

		/// <summary>
		/// 0: SIN RELACION; 1: LA EMPRESA ES INAFECTO AL IGV
		/// </summary>
		[Field]
        [XmlAttribute]
        public int nEmpExonerado { set; get; }

		[Field]
        [XmlAttribute]
        public int nEmpEstado { set; get; }

		[Field]
        [XmlAttribute]
        public int nEmpAfilidadoOSE { set; get; }

		#endregion

		#region "Constructor"

		public Empresa()
        {
            sEmpRuc = string.Empty;
            sEmpNombre = string.Empty;
            sEmpBreve = string.Empty;
            sEmpEmail = string.Empty;
            sEmpTelefono = string.Empty;
            sEmpDireccion = string.Empty;
            sEmpTipoIdentificadorClienteSunat = string.Empty;
            sEmpTipoIdentificadorEmpresaSunat = string.Empty;
            sEmpCiudad = string.Empty;
            sEmpCodigoUbigeoSunat = string.Empty;
            sEmpUsuarioFE = string.Empty;
            sEmpPasswordFE = string.Empty;
            sEmpClaveCertificadoFE = string.Empty;
            sEmpCodigoDetraccion = string.Empty;
            nEmpProduccion = 0;
			nEmpAfilidadoOSE = 0;
        }

        #endregion

    }
}
