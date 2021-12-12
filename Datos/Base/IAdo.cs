using Utilitarios;
using System;
using System.Collections.Generic;

namespace Datos.Base
{
    public interface IAdo<T>
    {
        void Guardar(T eEntidad);

        Object GuardarXml(T eEntidad);

        void Actualizar(Dictionary<string, object> dctColumnaValor, Dictionary<string, object> dctColumnaId);

        int Eliminar(int Entidad_Id);

        T Consultar(int Entidad_Id, string sColumnas);

        List<T> Consultar(Dictionary<string, object> dctParametros = null, string sColumns = "*", string sOrderBy = "");

        List<T> Consultar(Dictionary<string, object> dctParametros = null);
    }
}
