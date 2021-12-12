using Entidades.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Datos.Base
{
    public static class QueryHelper
    {        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entidad"></param>
        /// <returns></returns>
        public static string GetDeleteStatement<T>(T entidad)
        {
            var atributos = EntidadHelper.GetAtributosMapeados<T>(entidad);

            var builder = new StringBuilder();
            builder.AppendLine("UPDATE " + EntidadHelper.GetEsquemaTabla<T>() + " SET ");            
            foreach (AtributoEntidad atributo in atributos)
            {
                if (atributo.sNombre.Contains("Estado"))
                {
                    builder.AppendLine(atributo.sNombre + " = 0");
                    break;
                }
            }

            AtributoEntidad primaryKey = EntidadHelper.GetPrimaryKey<T>(entidad);
            builder.AppendLine(" WHERE " + primaryKey.sNombre + " = " + primaryKey.oValor + ";");

            return builder.ToString();
        }

        public static string GetRestoreStatement<T>(T entidad)
        {
            var atributos = EntidadHelper.GetAtributosMapeados<T>(entidad);

            var builder = new StringBuilder();
            builder.AppendLine("UPDATE " + EntidadHelper.GetEsquemaTabla<T>() + " SET ");
            foreach (AtributoEntidad atributo in atributos)
            {
                if (atributo.sNombre.Contains("Estado"))
                {
                    builder.AppendLine(atributo.sNombre + " = 1");
                    break;
                }
            }

            AtributoEntidad primaryKey = EntidadHelper.GetPrimaryKey<T>(entidad);
            builder.AppendLine(" WHERE " + primaryKey.sNombre + " = " + primaryKey.oValor + ";");

            return builder.ToString();
        }

        /// <summary>
        ///     Obtiene la sentencia de actualización desde una entidad
        /// </summary>
        /// <typeparam name="T">
        ///     Clase que será tratada (Type)
        /// </typeparam>
        /// <param name="entidad">
        ///     Instancia de entidad que será tratada (objeto)
        /// </param>
        /// <returns></returns>
        public static string GetUpdateStatement<T>(T entidad)
        {
            try
            {
                var builder = new StringBuilder();       // Cadena de concatenación
                bool primero = true;                     // Indica si es el primer parámetro recorrido
                
                List<AtributoEntidad> atributos = EntidadHelper.GetAtributosMapeados<T>(entidad);     // Obtenemos los atributos mapeados

                builder.AppendLine("UPDATE " + EntidadHelper.GetEsquemaTabla<T>() + " SET ");
                foreach (AtributoEntidad atributo in atributos)
                {
                    if (atributo.oTipo != TipoColumna.PrimaryKey)
                    {
                        string sItem = "";
                        if (atributo.oTipo == TipoColumna.ForeignKey)
                        {
                            sItem = (atributo.sNombre + " = " + (atributo.oValor.ToString().Equals("0") ? "null" : atributo.oValor.ToString()));
                        }
                        else
                        {
                            if (atributo.oValor is string || atributo.oValor is char)
                                sItem = (atributo.sNombre + " = '" + atributo.oValor + "'");
                            else if (atributo.oValor is DateTime)
                                if (atributo.oValor.ToString().Contains("01/01/0001") || atributo.oValor.ToString().Contains("1/01/0001"))
                                    sItem = (atributo.sNombre + " = null");
                                else
                                    sItem = (atributo.sNombre + " = '" + atributo.oValor + "'").Replace(".", "");
                            else if (atributo.oValor is bool)
                                sItem = atributo.sNombre + " = " + ((bool)atributo.oValor ? "1" : "0");
                            else
                                sItem = (atributo.sNombre + " = " + atributo.oValor);
                        }

                        if (primero)
                        {
                            builder.AppendLine(sItem);
                            primero = false;
                        }
                        else
                            builder.AppendLine(", " + sItem);
                    }
                }

                AtributoEntidad primaryKey = EntidadHelper.GetPrimaryKey<T>(entidad);
                builder.AppendLine(" WHERE " + primaryKey.sNombre + " = " + primaryKey.oValor + ";");

                return builder.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        ///     Obtiene la sentencia de inserción desde una entidad
        /// </summary>
        /// <typeparam name="T">
        ///     Clase que será tratada (Type)
        /// </typeparam>
        /// <param name="entidad">
        ///     Instancia de entidad que será tratada (objeto)
        /// </param>
        /// <returns>
        ///     Sentencia de Inserción
        /// </returns>
        public static string GetInsertStatement<T>(T entidad)
        {
            try
            {
                bool primero = true;                            // Indica si es el primer parámetro recorrido
                var builderColumnas = new StringBuilder();      // Concatenador de nombres de columnas
                var builderValores = new StringBuilder();       // Concatenador de valores de columnas

                builderColumnas.AppendLine("INSERT INTO " + EntidadHelper.GetEsquemaTabla<T>() + " (");
                builderValores.AppendLine(") VALUES(");

                // Obtenemos lista de atributos mapeados como columnas
                var atributos = EntidadHelper.GetAtributosMapeados<T>(entidad);

                // Recorremos lista de atributos y armamos sql
                foreach (AtributoEntidad atributo in atributos)
                {
                    if (atributo.oTipo != TipoColumna.PrimaryKey)
                    {
                        // Construimos sql de columnas
                        builderColumnas.Append((primero ? "" : ", "));
                        builderColumnas.AppendLine(atributo.sNombre);

                        // Construimos sql de valores
                        builderValores.Append((primero ? "" : ", "));
                        if (atributo.oTipo == TipoColumna.ForeignKey)
                        {
                            builderValores.AppendLine(atributo.oValor.ToString().Equals("0") ? "null" : atributo.oValor.ToString());
                        }
                        else
                        {
                            if (atributo.oValor == null)
                            {
                                builderValores.AppendLine("null");
                            }else if (atributo.oValor is DateTime)
                                if(atributo.oValor.ToString().Contains("01/01/0001") || atributo.oValor.ToString().Contains("1/01/0001"))
                                    builderValores.AppendLine("null");
                                else
                                    builderValores.AppendLine(("'" + atributo.oValor + "'").Replace(".", ""));
                            else if (atributo.oValor is string || atributo.oValor is char)
                                builderValores.AppendLine("'" + atributo.oValor + "'");
                            else if (atributo.oValor is bool)
                                builderValores.AppendLine(((bool)atributo.oValor ? "1" : "0"));
                            else
                                builderValores.AppendLine(atributo.oValor.ToString());
                        }

                        if (primero)
                            primero = false;
                    }
                }

                builderValores.AppendLine(");");
                builderValores.AppendLine("SELECT SCOPE_IDENTITY();");

                return builderColumnas.ToString() + builderValores.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }


        public static string GetWhereClause(Dictionary<string, object> dctParametros = null, string sAlias = "")
        {
            try
            {
                var oSb = new StringBuilder();
                oSb.Append(" WHERE ");
                oSb.AppendLine("1 = 1");
                if (dctParametros != null)
                {
                    if (dctParametros.Count > 0)
                    {
                        foreach (KeyValuePair<string, object> oItem in dctParametros)
                        {
                            if (oItem.Value is string || oItem.Value is char)
                                oSb.AppendLine(" AND " + (String.IsNullOrEmpty(sAlias) ? "" : sAlias + ".") + oItem.Key + " LIKE '" + oItem.Value + "'");
                            else if (oItem.Value is DateTime )
                                if (oItem.Value.ToString().Contains("01/01/0001") || oItem.Value.ToString().Contains("1/01/0001"))
                                    oSb.AppendLine(" AND " + (String.IsNullOrEmpty(sAlias) ? "" : sAlias + ".") + oItem.Key + " = null");
                                else
                                    oSb.AppendLine((" AND " + (String.IsNullOrEmpty(sAlias) ? "" : sAlias + ".") + oItem.Key + " = '" + oItem.Value + "'").Replace(".", ""));
                            else if (oItem.Value is bool)
                                oSb.AppendLine(" AND " + (String.IsNullOrEmpty(sAlias) ? "" : sAlias + ".") + oItem.Key + " = " + ((bool)oItem.Value ? "1" : "0"));                            
                            else
                                oSb.AppendLine(" AND " + (String.IsNullOrEmpty(sAlias) ? "" : sAlias + ".") + oItem.Key + " = " + oItem.Value);
                        }
                    }
                }
                return oSb.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}
