using Entidades.Base;
using System;
using System.Xml.Serialization;
using Utilitarios;

namespace Entidades.Comercial
{
    public class FormaPago : BaseEntidad
    {

        [XmlAttribute]
        [PrimaryKey]
        public int FormaPago_Id { get; set; }

        [XmlAttribute]
        [ForeignKey]
        public int Transaccion_Id { get; set; }

        [XmlAttribute]
        [Field]
        public int nFPaNroCuota { get; set; }

        [XmlAttribute]
        [Field]
        public string sFPaNombreCuota { get; set; }


        [XmlAttribute]
        [Field]
        public decimal nFPaTotalCuota { get; set; }


        [XmlAttribute]
        [Field]
        public DateTime dFPaFechaCuota { get; set; }

        [XmlAttribute]
        [Field]
        public int nFPaEstado { get; set; }


        public FormaPago()
        {

        }


        #region Serializar XML

        public override string SerializarXML()
        {
            return Util.SerializeXML<FormaPago>(this);
        }

        #endregion

    }
}
