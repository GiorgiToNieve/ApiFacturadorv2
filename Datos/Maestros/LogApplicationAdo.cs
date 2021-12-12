using Datos.Base;
using Entidades.Maestros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Maestros
{
    public class LogApplicationAdo : BaseAdo<LogApplication>
    {
        #region "Singleton"

        private static readonly Lazy<LogApplicationAdo> instance = new Lazy<LogApplicationAdo>(() => new LogApplicationAdo());

        private LogApplicationAdo()
        {
        }

        public static LogApplicationAdo Instance
        {
            get
            {
                return instance.Value;
            }
        }

        #endregion



        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public new bool GuardarLogAplicacion(Exception exception, string mensaje="")
        {
            try
            {
				LogApplication Logs = new LogApplication();
				Logs.LogApplication_Id = 0;

				if (exception != null)
				{
					var trace = new System.Diagnostics.StackTrace(exception, true);

					Logs.sLogProyecto = exception.Source;
					Logs.sLogMetodo = exception.TargetSite.Name;
					Logs.sLogArchivo = trace.GetFrame(0).GetMethod().ReflectedType.FullName;
					Logs.nLogLinea = trace.GetFrame(0).GetFileLineNumber();
					Logs.sLogMensaje = exception.Message + "mensaje: "+mensaje;
					Logs.sLogFecha = DateTime.Now.ToLongDateString();
				}
				else
				{
					Logs.sLogMensaje = mensaje;
				}

				base.Guardar(Logs);

				return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

	}
}
