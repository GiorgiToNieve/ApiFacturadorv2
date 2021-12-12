using Datos.Base;
using Entidades.Maestros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Maestros
{
    public class TipoDocumentoAdo : BaseAdo<TipoDocumento>
    {
        #region "Singleton"

        private static readonly Lazy<TipoDocumentoAdo> instance = new 
            Lazy<TipoDocumentoAdo>(() => new TipoDocumentoAdo());

        private TipoDocumentoAdo()
        {
        }

        public static TipoDocumentoAdo Instance
        {
            get
            {
                return instance.Value;
            }
        }

        #endregion

    }
}
