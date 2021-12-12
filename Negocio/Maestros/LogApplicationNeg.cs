using Datos.Maestros;
using Entidades.Maestros;
using Negocio.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Maestros
{
    public class LogApplicationNeg: BaseNegocio
    {

        #region "Singleton"

        private static readonly Lazy<LogApplicationNeg> oInstance = new Lazy<LogApplicationNeg>(() => new LogApplicationNeg());

        private LogApplicationNeg()
        {
        }

        public static LogApplicationNeg Instance
        {
            get
            {
                return oInstance.Value;
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oLogApplication"></param>
        public void Guardar(LogApplication oLogApplication)
        {
            try
            {
                
                LogApplicationAdo.Instance.Guardar(oLogApplication);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        public void Guardar(Exception exception)
        {
            try
            {
                var trace = new System.Diagnostics.StackTrace(exception, true);
                var oLogApplication = new LogApplication();
                oLogApplication.sLogProyecto = exception.Source==null?"":exception.Source.ToString();
                oLogApplication.sLogArchivo= trace.FrameCount>0? trace.GetFrame(0).GetMethod().ReflectedType.FullName: "";
                oLogApplication.sLogMetodo = exception.TargetSite==null?"": exception.TargetSite.Name;
                oLogApplication.nLogLinea = trace.FrameCount>0? trace.GetFrame(0).GetFileLineNumber() : 0;
                oLogApplication.sLogMensaje = exception.Message;
                oLogApplication.sLogFecha = DateTime.Now.ToLongDateString();

                LogApplicationAdo.Instance.Guardar(oLogApplication);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dctParametros"></param>
        /// <param name="sColumnas"></param>
        /// <returns></returns>
        public List<LogApplication> Consultar(Dictionary<string, object> dctParametros = null, string sColumnas = "*")
        {
            try
            {
                return LogApplicationAdo.Instance.Consultar(dctParametros, sColumnas);
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// [vnieve] Guarda Log de Aplicaciones
        /// </summary>
        /// <param name="ex"></param>
        public Task<bool> GuardarLogAplicacion(Exception ex, string mensaje="")
        {
            try
            {
                return Task.Run(() => LogApplicationAdo.Instance.GuardarLogAplicacion(ex,mensaje));
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
