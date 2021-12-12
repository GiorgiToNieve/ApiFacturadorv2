using Datos.Base;
using Entidades.Maestros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Maestros
{
    public class TicketAdo : BaseAdo<Ticket>
    {
        #region "Singleton"

        private static readonly Lazy<TicketAdo> instance = new Lazy<TicketAdo>(() => new TicketAdo());

        private TicketAdo()
        {
        }

        public static TicketAdo Instance
        {
            get
            {
                return instance.Value;
            }
        }

        #endregion

    }
}
    