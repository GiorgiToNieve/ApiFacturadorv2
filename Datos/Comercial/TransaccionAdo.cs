using Datos.Base;
using Entidades.Comercial;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos.Comercial
{
    public class TransaccionAdo : BaseAdo<Transaccion>
    {
        #region "Singleton"

        private static readonly Lazy<TransaccionAdo> instance =
            new Lazy<TransaccionAdo>(() => new TransaccionAdo());

        private TransaccionAdo()
        {
        }

        public static TransaccionAdo Instance
        {
            get { return instance.Value; }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="LstTransaccion"></param>
        public void Guardar(List<Transaccion> LstTransaccion)
        {
            try
            {
                var lstSqlParametros = new List<SqlParameter>();
                var sqlPar = new SqlParameter("@xRegistro", System.Data.SqlDbType.VarChar);
                sqlPar.SqlValue = Util.SerializeXML<List<Transaccion>>(LstTransaccion);
                lstSqlParametros.Add(sqlPar);

                SQLHelper.ExecuteEscalarProcedure("dbo.pa_GuardarTransaccion", lstSqlParametros, sCadenaConexion);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public int ConsultarEstadoElectronico(Dictionary<string, object> parametros)
        {
            try
            {
                var result = SQLHelper.ExecuteEscalar("dbo.pa_ConsultarEstadoElectronico", parametros, sCadenaConexion);

                return result.toInt();
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
        /// <returns></returns>
        public List<Transaccion> ConsultarTransaccion(Dictionary<string, object> dctParametros)
        {
            try
            {
                var LstTransaccion = new List<Transaccion>();

                var oEscalar = SQLHelper.ExecuteEscalar("[dbo].[pa_ConsultarTransaccion]", dctParametros,
                     sCadenaConexion);
                if (oEscalar == null) return LstTransaccion;
                if (oEscalar.ToString().Length > 0)
                    LstTransaccion = Util.DeserializeXML<List<Transaccion>>(oEscalar.ToString(),
                        "LstTransaccion");

                return LstTransaccion;
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
        public void EnviarCorreo(Dictionary<string, object> dctParametros)
        {
            try
            {
                SQLHelper.ExecuteQueryProcedure<Transaccion>("[dbo].[pa_EnviarCorreo]", dctParametros, sCadenaConexion);

            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
