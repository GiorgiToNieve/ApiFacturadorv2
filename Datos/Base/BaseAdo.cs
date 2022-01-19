using Entidades.Base;
using Utilitarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Datos.Base
{
    public abstract class BaseAdo<T> : IAdo<T>
    {
        public string sCadenaConexion = ConnectionString.Conn;

        // Constructor
        public BaseAdo() { }

        // Métodos
        public BaseAdo<T> Conexion(string cadenaConexion)
        {
            this.sCadenaConexion = cadenaConexion;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eEntidad"></param>
        public virtual void Guardar(T eEntidad)
        {
            string sql = string.Empty;

            try
            {
                Conexion(ConnectionString.Conn);
                var primaryKey = EntidadHelper.GetPrimaryKey<T>(eEntidad);
                int id = int.Parse(primaryKey.oValor.ToString());
                if (id <= 0)
                {
                    // Inserción
                    sql = QueryHelper.GetInsertStatement<T>(eEntidad);
                    id = int.Parse(SQLHelper.ExecuteEscalar(sql, sCadenaConexion).ToString());
                    typeof(T).GetProperty(primaryKey.sNombre).SetValue(eEntidad, id);
                }
                else
                {
                    // Actualización
                    sql = QueryHelper.GetUpdateStatement<T>(eEntidad);

                    SQLHelper.ExecuteUpdate(sql, sCadenaConexion);
                }

        
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="eEntidad">Objeto entidad a guardar</param>
        /// <param name="oLoginInfo">Información de auditoría</param>
        public virtual Object GuardarXml(T eEntidad)
        {
            String sProcedimiento = String.Empty;
            try
            {
                object oEscalar = null;
                Conexion(ConnectionString.Conn);
                // Obtenemos el número 
                String[] arrEsquemaTabla = EntidadHelper.GetEsquemaTabla<T>().Split(new char[] { '.' });
                if (arrEsquemaTabla.Length == 2)
                {
                    sProcedimiento = arrEsquemaTabla[0] + ".pa_Guardar" + arrEsquemaTabla[1];

                    Dictionary<String, Object> dctParametros = new Dictionary<String, Object>();
                    dctParametros.Add("x" + arrEsquemaTabla[1], Util.SerializeXML<T>(eEntidad));

                    oEscalar = SQLHelper.ExecuteEscalar(sProcedimiento, dctParametros, sCadenaConexion);
                    if (oEscalar == null)
                        throw new Exception("El procedimiento no ha devuelto una respuesta");
                }
                else
                {
                    throw new Exception("Se ha producido un error al obtener el esquema y nombre de la entidad");
                }
            
                return oEscalar;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual void Actualizar(Dictionary<string, object> dctColumnaValor,
                                        Dictionary<string, object> dctColumnaId)
        {
            try
            {
                Conexion(ConnectionString.Conn);
                var oSb = new StringBuilder();
                var oPrimaryKey = EntidadHelper.GetPrimaryKey<T>(Activator.CreateInstance<T>());

                oSb.AppendLine("UPDATE " + EntidadHelper.GetEsquemaTabla<T>() + " SET ");
                bool lComa = true;

                foreach (var oItem in dctColumnaValor)
                {
                    string sColumna = oItem.Key;
                    object oValor = oItem.Value;
                    if (!lComa)
                    {
                        oSb.Append(" , ");
                    }
                    else
                    {
                        lComa = false;
                    }

                    if (oValor is string || oValor is char)
                        oSb.AppendLine(sColumna + " = '" + oValor + "'");
                    else if (oValor is DateTime)
                        if (oValor.ToString().Contains("01/01/0001"))
                            oSb.AppendLine(sColumna + " = null");
                        else
                            oSb.AppendLine(sColumna + " = '" + oValor.ToString().Replace(".", "") + "'");
                    else if (oValor is bool)
                        oSb.AppendLine(sColumna + " = " + ((bool)oValor ? "1" : "0"));
                    else
                        oSb.AppendLine(sColumna + " = " + oValor);
                }

                oSb.AppendLine(QueryHelper.GetWhereClause(dctColumnaId));

                SQLHelper.ExecuteUpdate(oSb.ToString(), sCadenaConexion);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="columnas"></param>
        /// <returns></returns>
        public virtual T Consultar(int id, string columnas = "*")
        {
            try
            {
                // Obtenemos atributo PrimaryKey
                var primaryKey = EntidadHelper.GetPrimaryKey<T>(Activator.CreateInstance<T>());

                // Construimos sentencia SQL
                var builder = new StringBuilder();
                builder.AppendLine("SELECT ");
                builder.AppendLine(columnas);
                builder.AppendLine(" FROM ");
                builder.AppendLine(EntidadHelper.GetEsquemaTabla<T>());
                builder.AppendLine(" WHERE ");
                builder.AppendLine(primaryKey.sNombre + " = " + id);

                // Ejecutamos consulta
                Conexion(ConnectionString.Conn);
                return SQLHelper.ExecuteQuery<T>(builder.ToString(), sCadenaConexion).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlSentencia"></param>
        /// <returns></returns>
        public virtual T Consultar(string sqlSentencia)
        {
            try
            {
                // Construimos sentencia SQL
                var builder = new StringBuilder();
                builder.AppendLine(sqlSentencia);

                // Ejecutamos consulta
                Conexion(ConnectionString.Conn);
                return SQLHelper.ExecuteQuery<T>(builder.ToString(), sCadenaConexion).FirstOrDefault();
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
        /// <param name="columnas"></param>
        /// <param name="sOrderBy"></param>
        /// <returns></returns>
        public virtual List<T> Consultar(Dictionary<string, object> parametros = null,
            string columnas = "*", string sOrderBy = "")
        {
            try
            {
                var builder = new StringBuilder();
                builder.AppendLine("SELECT ");
                builder.AppendLine(columnas);
                builder.AppendLine(" FROM ");
                builder.AppendLine(EntidadHelper.GetEsquemaTabla<T>());
                builder.AppendLine(QueryHelper.GetWhereClause(parametros));
                if (sOrderBy != "")
                {
                    builder.AppendLine(" ORDER BY ");
                    builder.AppendLine(sOrderBy);
                }
                Conexion(ConnectionString.Conn);
                return SQLHelper.ExecuteQuery<T>(builder.ToString(), sCadenaConexion);
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
        public virtual List<T> Consultar(Dictionary<String, Object> dctParametros = null)
        {
            try
            {
                Conexion(ConnectionString.Conn);
                string sEsquemaTabla = EntidadHelper.GetEsquemaTabla<T>();
                string sProcedimiento = string.Format("{0}.pa_Consultar{1}", sEsquemaTabla.Split('.')[0],
                    sEsquemaTabla.Split('.')[1]);

                bool bConsultarAsXml = false;
                if (dctParametros != null)
                {
                    bConsultarAsXml = dctParametros.ContainsKey("bConsultarAsXml") && Convert.ToBoolean(dctParametros["bConsultarAsXml"]);
                }

                if (bConsultarAsXml)
                {
                    var oEscalar = SQLHelper.ExecuteEscalar(sProcedimiento, dctParametros, sCadenaConexion);
                    if (oEscalar != null)
                    {
                        if (!String.IsNullOrEmpty(oEscalar.ToString()))
                        {
                            return Util.DeserializeXML<List<T>>(oEscalar.ToString(), "lstEntidades");
                        }

                        return new List<T>();
                    }

                    throw new ArgumentException("El procedimiento no ha devuelto un escalar");
                }

                return SQLHelper.ExecuteQueryProcedure<T>(sProcedimiento, dctParametros, sCadenaConexion);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Entidad_Id"></param>
        /// <returns></returns>
        public virtual int Eliminar(int Entidad_Id)
        {
            try
            {
                Conexion(ConnectionString.Conn);
                var entidad = Consultar(Entidad_Id);
                string sql = QueryHelper.GetDeleteStatement<T>(entidad);

                return SQLHelper.ExecuteUpdate(sql, sCadenaConexion);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Entidad_Id"></param>
        /// <returns></returns>
        public virtual int Restaurar(int Entidad_Id)
        {
            try
            {
                Conexion(ConnectionString.Conn);
                var entidad = Consultar(Entidad_Id);
                string sql = QueryHelper.GetRestoreStatement<T>(entidad);

                return SQLHelper.ExecuteUpdate(sql, sCadenaConexion);

            }
            catch (Exception)
            {
                throw;
            }
        }

        protected void GuardarLogAplicacion(Exception ex,string mensaje="")
        {
            try
            {
                SQLHelper.SaveLogApplication(ex,mensaje);
            }
            catch
            {
                // ignorar
            }
        }
    }
}
