using Entidades.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Maestros
{
    public class Ticket : BaseEntidad
    {
        #region "Columnas"

        [PrimaryKey]
        public int Ticket_Id { set; get; }

        [ForeignKey]
        public int TipoDocumento_Id { set; get; }

        [ForeignKey]
        public int Empresa_Id { set; get; }

        [Field]
        public int nTicUltimoNumero { set; get; }

        [Field]
        public int nTicEstado { set; get; }
        
        #endregion
        
        #region "Constructor"

        public Ticket()
        {
            Ticket_Id = 0;
            TipoDocumento_Id = 0;
            nTicEstado = 0;
        }

        #endregion

    }
}
