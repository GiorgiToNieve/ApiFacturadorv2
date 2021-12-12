using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilitarios;
using log4net;

namespace Datos.Base
{
    public static class SQLHelper
    {
        private static ILog _log = LogManager.GetLogger("SQL Server");
        private static string sDatabase = string.Empty;
        private static SqlParameterCollection parameters = null;

        public static int ExecuteUpdate(String sSql, string sCadenaConexion)
        {
            SqlConnection oConnection = null;
            SqlTransaction oTransaction = null;
            try
            {
                oConnection = new SqlConnection(sCadenaConexion);
                oConnection.Open();
                oTransaction = oConnection.BeginTransaction();
                SqlCommand command = new SqlCommand(sSql, oConnection, oTransaction);
                command.CommandType = CommandType.Text;
                int nFilasAfectadas = command.ExecuteNonQuery();
                oTransaction.Commit();
                return nFilasAfectadas;
            }
            catch (SqlException ex)
            {
                _log.Error(Util.MensajeProcedureLog(ex, sDatabase, null));
                throw;
            }
            catch (Exception)
            {
                if (oTransaction != null)
                    oTransaction.Rollback();
                throw;
            }
            finally
            {
                if (oConnection != null)
                    if (oConnection.State == ConnectionState.Open)
                        oConnection.Close();
            }
        }

        public static int ExecuteUpdateProcedure(String sProcedimiento, 
            Dictionary<string, object> dctParametros = null,
            string sCadenaConexion="")
        {
            try
            {
                using (SqlConnection oConnection = new SqlConnection(sCadenaConexion))
                {
                    oConnection.Open();
                    using (SqlTransaction oTransaction = oConnection.BeginTransaction())
                    {
                        using (SqlCommand oCommand = new SqlCommand(sProcedimiento, oConnection, oTransaction))
                        {
                            sDatabase = oCommand.Connection.Database;
                            oCommand.CommandType = CommandType.StoredProcedure;
                            oCommand.CommandTimeout = 1600;
                            if (dctParametros != null)
                            {
                                foreach (var oParametro in dctParametros)
                                {
                                    var oSqlParametro = new SqlParameter("@" + oParametro.Key, oParametro.Value);
                                    if (!(oParametro.Value is DateTime))
                                    {
                                        oSqlParametro.SqlDbType = SqlDbType.VarChar;
                                    }
                                    oSqlParametro.Direction = ParameterDirection.Input;
                                    oCommand.Parameters.Add(oSqlParametro);
                                }
                                parameters = oCommand.Parameters;
                            }

                            int nFilasAfectadas = oCommand.ExecuteNonQuery();
                            oTransaction.Commit();
                            return nFilasAfectadas;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                _log.Error(Util.MensajeProcedureLog(ex, sDatabase, parameters));
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static int ExecuteUpdateProcedure(String sProcedimiento, 
            List<SqlParameter> lstParametros = null,string sCadenaConexion="")
        {
            SqlConnection oConnection = null;
            SqlTransaction oTransaction = null;
            try
            {
                oConnection = new SqlConnection(sCadenaConexion);
                oConnection.Open();
                oTransaction = oConnection.BeginTransaction();
                SqlCommand oCommand = new SqlCommand(sProcedimiento, oConnection, oTransaction);
                oCommand.CommandType = CommandType.StoredProcedure;

                if (lstParametros != null)
                {
                    foreach (SqlParameter oSqlParametro in lstParametros)
                    {
                        oCommand.Parameters.Add(oSqlParametro);
                    }
                }

                int nFilasAfectadas = oCommand.ExecuteNonQuery();
                oTransaction.Commit();
                return nFilasAfectadas;
            }
            catch (Exception)
            {
                if (oTransaction != null)
                    oTransaction.Rollback();
                throw;
            }
            finally
            {
                if (oConnection != null)
                    if (oConnection.State == ConnectionState.Open)
                        oConnection.Close();
            }
        }

        public static object ExecuteEscalar(String sSql, string sCadenaConexion)
        {
            try
            {
                using (SqlConnection oConnection = new SqlConnection(sCadenaConexion))
                {
                    oConnection.Open();
                    using (SqlTransaction oTransaction = oConnection.BeginTransaction())
                    {
                        using (SqlCommand oCommand = new SqlCommand(sSql, oConnection, oTransaction))
                        {
                            sDatabase = oCommand.Connection.Database;
                            oCommand.CommandType = CommandType.Text;

                            var oEscalar = oCommand.ExecuteScalar();

                            oTransaction.Commit();
                            return oEscalar;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                _log.Error(Util.MensajeProcedureLog(ex, sDatabase, null));
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static object ExecuteEscalar(String sProcedimiento, Dictionary<string, object> dctParametros = null,
            string sCadenaConexion = "")
        {
            try
            {
                using (SqlConnection oConnection = new SqlConnection(sCadenaConexion))
                {
                    oConnection.Open();
                    using (SqlTransaction oTransaction = oConnection.BeginTransaction())
                    {
                        using (SqlCommand oCommand = new SqlCommand(sProcedimiento, oConnection, oTransaction))
                        {
                            sDatabase = oCommand.Connection.Database;
                            oCommand.CommandType = CommandType.StoredProcedure;
                            oCommand.CommandTimeout = 3600;
                            if (dctParametros != null)
                            {
                                foreach (var oParametro in dctParametros)
                                {
                                    var oSqlParametro = new SqlParameter("@" + oParametro.Key, oParametro.Value);
                                    oSqlParametro.Direction = ParameterDirection.Input;
                                    if (!(oParametro.Value is DateTime))
                                    {
                                        oSqlParametro.SqlDbType = SqlDbType.VarChar;
                                    }
                                    oCommand.Parameters.Add(oSqlParametro);
                                }
                                parameters = oCommand.Parameters;
                            }

                            var oEscalar = oCommand.ExecuteScalar();
                            oTransaction.Commit();
                            return oEscalar;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                _log.Error(Util.MensajeProcedureLog(ex, sDatabase, parameters));
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static object ExecuteEscalarProcedure(String sProcedimiento, 
            List<SqlParameter> lstParametros = null,
            string sCadenaConexion = "")
        {
            try
            {
                using (SqlConnection oConnection = new SqlConnection(sCadenaConexion))
                {
                    oConnection.Open();
                    using (SqlTransaction oTransaction = oConnection.BeginTransaction())
                    {
                        using (SqlCommand oCommand = new SqlCommand(sProcedimiento, oConnection, oTransaction))
                        {
                            sDatabase = oCommand.Connection.Database;
                            oCommand.CommandType = CommandType.StoredProcedure;
                            oCommand.CommandTimeout = 1300;
                            if (lstParametros != null)
                            {
                                foreach (SqlParameter oParam in lstParametros)
                                {
                                    oCommand.Parameters.Add(oParam);
                                }
                                parameters = oCommand.Parameters;
                            }

                            var escalar = oCommand.ExecuteScalar();
                            oTransaction.Commit();
                            return escalar;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                _log.Error(Util.MensajeProcedureLog(ex, sDatabase, parameters));
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<T> ExecuteQuery<T>(String sSql, string sCadenaConexion = "")
        {
            try
            {
                using (SqlConnection oConnection = new SqlConnection(sCadenaConexion))
                {
                    using (SqlCommand oCommand = new SqlCommand(sSql, oConnection))
                    {
                        sDatabase = oCommand.Connection.Database;
                        oCommand.CommandType = CommandType.Text;
                        oConnection.Open();

                        return GenericHelper.GetAsList<T>(oCommand.ExecuteReader());
                    }
                }
            }
            catch (SqlException ex)
            {
                _log.Error(Util.MensajeProcedureLog(ex, sDatabase, null));
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<T> ExecuteQueryProcedure<T>(String sProcedimiento,
            Dictionary<string, object> dctParametros = null,
            string sCadenaConexion = "")
        {
            try
            {
                using (SqlConnection oConnection = new SqlConnection(sCadenaConexion))
                {
                    using (SqlCommand oCommand = new SqlCommand(sProcedimiento, oConnection))
                    {
                        sDatabase = oCommand.Connection.Database;
                        oCommand.CommandType = CommandType.StoredProcedure;
                        oCommand.CommandTimeout = 9000;
                        if (dctParametros != null)
                        {
                            foreach (var oParametro in dctParametros)
                            {
                                var oSqlParametro = new SqlParameter("@" + oParametro.Key, oParametro.Value);

                                if (!(oParametro.Value is DateTime))
                                {
                                    oSqlParametro.SqlDbType = SqlDbType.VarChar;
                                }

                                oSqlParametro.Direction = ParameterDirection.Input;
                                oCommand.Parameters.Add(oSqlParametro);
                            }
                        }

                        parameters = oCommand.Parameters;
                        oConnection.Open();

                        return GenericHelper.GetAsList<T>(oCommand.ExecuteReader());
                    }
                }
            }
            catch (SqlException ex)
            {
                _log.Error(Util.MensajeProcedureLog(ex, sDatabase, parameters));
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void SaveLogApplication(Exception ex, string mensaje="")
        {
            try
            {
                _log.Error(Util.MensajeAplicacionLog(ex)+mensaje);
				
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

