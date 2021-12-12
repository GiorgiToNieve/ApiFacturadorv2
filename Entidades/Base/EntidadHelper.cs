using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Base
{
    public static class EntidadHelper
    {
        /// <summary>
        /// Obtiene el atributo de entidad mapeado como Primary Key
        /// </summary>
        /// <typeparam name="T">
        /// Tipo de clase de entidad que será tratada
        /// </typeparam>
        /// <param name="eEntidad">
        /// Instancia de entidad de la que obtendremos la primary key (objeto)
        /// </param>
        /// <returns></returns>
        public static AtributoEntidad GetPrimaryKey<T>(T eEntidad)
        {
            try
            {
                var oType = typeof(T);
                var arPropiedades = oType.GetProperties();

                AtributoEntidad oAtributo = null;
                foreach (var oPropiedad in arPropiedades)
                {
                    var arAttributes = oPropiedad.GetCustomAttributes(false);
                    var oColumnMapping = arAttributes.FirstOrDefault(a => a.GetType() == typeof(BaseEntidad.PrimaryKeyAttribute));
                    if (oColumnMapping != null)
                    {
                        oAtributo = new AtributoEntidad();
                        oAtributo.sNombre = oPropiedad.Name;
                        oAtributo.oTipo = TipoColumna.PrimaryKey;
                        if (eEntidad != null)
                            oAtributo.oValor = oPropiedad.GetValue(eEntidad);
                    }
                }
                return oAtributo;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        ///     Obtiene la lista de los atributos (columnas) de una entidad 
        /// </summary>
        /// <typeparam name="T">
        ///     Clase de entidad que será mapeada
        /// </typeparam>
        /// <param name="eEntidad">
        ///     Instancia de entidad que será tratada (objeto)
        /// </param>
        /// <returns></returns>
        public static List<AtributoEntidad> GetAtributosMapeados<T>(T eEntidad)
        {
            try
            {
                var lstAtributos = new List<AtributoEntidad>();      // Lista de columnas mapeadas en entidad
                var type = typeof(T);
                var propiedades = type.GetProperties();

                foreach (var propiedad in propiedades)
                {
                    var attributes = propiedad.GetCustomAttributes(false);
                    var columnaMapeada = attributes.FirstOrDefault(a => a.GetType() == typeof(BaseEntidad.PrimaryKeyAttribute) || a.GetType() == typeof(BaseEntidad.ForeignKeyAttribute) || a.GetType() == typeof(BaseEntidad.FieldAttribute));
                    if (columnaMapeada != null)
                    {
                        var atributo = new AtributoEntidad();

                        // Seteamos nombre de atributo
                        atributo.sNombre = propiedad.Name;

                        // Seteamos valor de columna
                        if (eEntidad != null)
                            atributo.oValor = propiedad.GetValue(eEntidad);

                        // Seteamos tipo de columna
                        if (columnaMapeada is BaseEntidad.PrimaryKeyAttribute)
                            atributo.oTipo = TipoColumna.PrimaryKey;
                        else if (columnaMapeada is BaseEntidad.ForeignKeyAttribute)
                            atributo.oTipo = TipoColumna.ForeignKey;
                        else
                            atributo.oTipo = TipoColumna.Field;

                        // Agregamos columna a lista
                        lstAtributos.Add(atributo);
                    }
                }
                return lstAtributos;
            }
            catch (Exception)
            {
                throw;
            }
        }



        /// <summary>
        ///     Obtiene el esquema y el nombre de tabla concatenados {Esquema.Tabla}
        /// </summary>
        /// <typeparam name="T">
        ///     Clase de entidad que será tratada
        /// </typeparam>
        /// <returns></returns>
        public static string GetEsquemaTabla<T>()
        {
            try
            {
                var type = typeof(T);

                // Obtenemos el esquema de entidad
                var modulo = type.FullName.Split('.')[2];
                var esquema ="dbo";
                //switch (modulo)
                //{
                //    case "Produccion":
                //        esquema = "PRD";
                //        break;
                //    case "Transportes":
                //        esquema = "TRP";
                //        break;
                //    case "Logistica":
                //        esquema = "LGT";
                //        break;
                //    case "Comercial":
                //        esquema = "COM";
                //        break;
                //    case "Contabilidad":
                //        esquema = "CTB";
                //        break;
                //    case "Finanzas":
                //        esquema = "FIN";
                //        break;
                //    case "Despacho":
                //        esquema = "DPC";
                //        break;
                //    case "GestionHumana":
                //        esquema = "GHU";
                //        break;
                //    case "SIG":
                //        esquema = "SIG";
                //        break;
                //    case "Accesos":
                //        esquema = "ACC";
                //        break;
                //    case "Auditoria":
                //        esquema = "AUD";
                //        break;
                //    case "Calidad":
                //        esquema = "CAL";
                //        break;
                //    default:
                //        esquema = "CMN";
                //        break;
                //}

                // Obtenemos el nombre de tabla                
                var tabla = type.Name.Replace("Entidad", "");

                return string.Format("{0}.{1}", esquema, tabla);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
