using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Base
{
    public class BaseEntidad : IEntidad
    {

        public virtual string SerializarXML()
        {
            return "";
        }

        public class CustomAttribute : Attribute
        {
            public String sCampoNombre { set; get; }
            public bool lPermiteNulo { set; get; }
            public String sValoresCheck { set; get; }

            public CustomAttribute()
            {
                this.sCampoNombre = String.Empty;
                this.lPermiteNulo = true;
                this.sValoresCheck = String.Empty;
            }
        }


        public class PrimaryKeyAttribute : CustomAttribute
        {
            public PrimaryKeyAttribute()
                : base()
            {
            }
        }

        public class FieldAttribute : CustomAttribute
        {
            public FieldAttribute()
                : base()
            {
            }
        }

        public class ForeignKeyAttribute : CustomAttribute
        {
            public ForeignKeyAttribute()
                : base()
            {
            }
        }
    }

    public class StringWriterUtf8 : System.IO.StringWriter
    {
        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }
    }
}