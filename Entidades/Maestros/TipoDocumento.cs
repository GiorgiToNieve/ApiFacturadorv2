using Entidades.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Maestros
{
    public class TipoDocumento: BaseEntidad
    {
        #region "Columnas"

        [PrimaryKey]
        public int TipoDocumento_Id { set; get; }

        [Field]
        public string sTDoNombre { set; get; }

        [Field]
        public string sTDoBreve { set; get; }

        [Field]
        public string sTDoCodigoSunat { set; get; }

        [Field]
        public int nTDoMostrar { set; get; }

        [Field]
        public int nTDoEstado { set; get; }


        #endregion


        public TipoDocumento()
        {

        }

    }
}
