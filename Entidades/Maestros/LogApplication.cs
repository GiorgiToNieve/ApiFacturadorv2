using Entidades.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Maestros
{
    public class LogApplication : BaseEntidad
    {
        [PrimaryKey]
        public int LogApplication_Id { get; set; }
        
        [Field]
        public string sLogProyecto { get; set; }

        [Field]
        public string sLogMetodo { get; set; }

        [Field]
        public string sLogArchivo { get; set; }

        [Field]
        public int nLogLinea { get; set; }

        [Field]
        public string sLogMensaje { get; set; }

        [Field]
        public string sLogFecha { get; set; }


        public LogApplication()
        {
            sLogProyecto = string.Empty;
            sLogMetodo = string.Empty;
            sLogArchivo = string.Empty;
            nLogLinea = 0;
            sLogMensaje = string.Empty;
            sLogFecha = DateTime.Now.ToLongDateString();
        }


    }
}
