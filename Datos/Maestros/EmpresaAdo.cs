using Datos.Base;
using Entidades.Maestros;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Datos.Maestros
{
    public class EmpresaAdo : BaseAdo<Empresa>
    {
        #region "Singleton"

        private static readonly Lazy<EmpresaAdo> instance = new Lazy<EmpresaAdo>(() => new EmpresaAdo());

        private EmpresaAdo()
        {
        }

        public static EmpresaAdo Instance
        {
            get
            {
                return instance.Value;
            }
        }

		public void GuardarEmpresa(Empresa eEmpresa)
		{
			try
			{
				var Lista = new List<Empresa>();
				Lista.Add(eEmpresa);

				var lstSqlParametros = new List<SqlParameter>();
				var sqlPar = new SqlParameter("@xRegistro", System.Data.SqlDbType.VarChar);
				sqlPar.SqlValue = Util.SerializeXML<List<Empresa>>(Lista);
				lstSqlParametros.Add(sqlPar);

				SQLHelper.ExecuteEscalarProcedure("dbo.pa_GuardarEmpresa", lstSqlParametros, sCadenaConexion);
			}
			catch (Exception)
			{
				throw;
			}
		}

		#endregion

	}
}