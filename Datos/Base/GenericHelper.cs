using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading;

namespace Datos.Base
{
    internal static class GenericHelper
    {
        // Dictionary to store cached properites
        private static IDictionary<string, PropertyInfo[]> propertiesCache = new Dictionary<string, PropertyInfo[]>();
        // Help with locking
        private static ReaderWriterLockSlim propertiesCacheLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Get an array of PropertyInfo for this type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>PropertyInfo[] for this type</returns>
        public static PropertyInfo[] GetCachedProperties<T>()
        {
            var props = new PropertyInfo[0];
            if (propertiesCacheLock.TryEnterUpgradeableReadLock(100))
            {
                try
                {
                    if (!propertiesCache.TryGetValue(typeof(T).FullName, out props))
                    {
                        props = typeof(T).GetProperties();
                        if (propertiesCacheLock.TryEnterWriteLock(100))
                        {
                            try
                            {
                                propertiesCache.Add(typeof(T).FullName, props);
                            }
                            finally
                            {
                                propertiesCacheLock.ExitWriteLock();
                            }
                        }
                    }
                }
                finally
                {
                    propertiesCacheLock.ExitUpgradeableReadLock();
                }
                return props;
            }
            else
            {
                return typeof(T).GetProperties();
            }
        }

        /// <summary>
        /// Return a list from the current reader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader">The reader</param>
        /// <returns></returns>
        public static List<T> GetAsList<T>(SqlDataReader reader)
        {
            var objetList = new List<T>();
            // Get all the properties in our Object
            var props = GetCachedProperties<T>();
            // For each property get the data from the reader to the object
            List<string> columnList = null;
            if (reader.HasRows)
            {
                columnList = GetColumnList(reader);
            }
            try
            {

                while (reader.Read())
                {
                    // Create a new Object
                    var newObjectToReturn = Activator.CreateInstance<T>();
                    foreach (var f in props)
                    {
                        if (columnList.Contains(f.Name) && reader[f.Name] != DBNull.Value)
                        {
                            var o = reader[f.Name];
                            var targetType = IsNullableType(f.PropertyType)
                                ? Nullable.GetUnderlyingType(f.PropertyType)
                                : f.PropertyType;
                            if (o.GetType() != typeof(DBNull))
                                f.SetValue(newObjectToReturn, Convert.ChangeType(o, targetType), null);
                        }
                    }
                    objetList.Add(newObjectToReturn);
                }
            }
            catch(SqlException)
            {
                throw;
            }
            catch (Exception)
            {

                throw;
            }
            return objetList;
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Get a list of column names from the reader
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <returns></returns>
        public static List<string> GetColumnList(SqlDataReader reader)
        {
            var columnList = new List<string>();
            var readerSchema = reader.GetSchemaTable();
            for (int i = 0; i < readerSchema.Rows.Count; i++)
                columnList.Add(readerSchema.Rows[i]["ColumnName"].ToString());
            return columnList;
        }
    }
}
