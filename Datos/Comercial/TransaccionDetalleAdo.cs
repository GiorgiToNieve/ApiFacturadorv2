using Datos.Base;
using Entidades.Comercial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Comercial
{
    public class TransaccionDetalleAdo : BaseAdo<TransaccionDetalle>
    {
        #region "Singleton"

        private static readonly Lazy<TransaccionDetalleAdo> instance =
            new Lazy<TransaccionDetalleAdo>(() => new TransaccionDetalleAdo());

        private TransaccionDetalleAdo()
        {
        }

        public static TransaccionDetalleAdo Instance
        {
            get { return instance.Value; }
        }

        #endregion

    }
}
