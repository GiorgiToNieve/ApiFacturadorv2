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
    public class TipoDocumentoNeg : BaseNegocio
    {

        #region "Singleton"

        private static readonly Lazy<TipoDocumentoNeg> oInstance =
            new Lazy<TipoDocumentoNeg>(() => new TipoDocumentoNeg ());

        private TipoDocumentoNeg()
        {
        }

        public static TipoDocumentoNeg Instance
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
        /// <param name="oTipoDocumento"></param>
        public void Guardar(TipoDocumento oTipoDocumento)
        {
            try
            {
                TipoDocumentoAdo.Instance.Guardar(oTipoDocumento);
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Ticket_Id"></param>
        public void Eliminar(int TipoDocumento_Id)
        {
            try
            {
                TipoDocumentoAdo.Instance.Eliminar(TipoDocumento_Id);
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
        public List<TipoDocumento> Consultar(Dictionary<string, object> dctParametros = null,
            string sColumnas = "*")
        {
            try
            {
                return TipoDocumentoAdo.Instance.Consultar(dctParametros, sColumnas);
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}
