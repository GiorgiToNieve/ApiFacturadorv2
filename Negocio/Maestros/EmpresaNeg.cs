using Datos.Maestros;
using Entidades.Maestros;
using Negocio.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilitarios;

namespace Negocio.Maestros
{
    public class EmpresaNeg : BaseNegocio
    {
        #region "Singleton"

        private static readonly Lazy<EmpresaNeg> oInstance = new Lazy<EmpresaNeg>(() => new EmpresaNeg());

        private EmpresaNeg()
        {
        }

        public static EmpresaNeg Instance
        {
            get
            {
                return oInstance.Value;
            }
        }

        #endregion

        #region "Métodos"

        public void Guardar(Empresa eEmpresa)
        {
            try
            {
                if (Validar(eEmpresa))
                    EmpresaAdo.Instance.GuardarEmpresa(eEmpresa);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Eliminar(int Empresa_Id)
        {
            try
            {
                EmpresaAdo.Instance.Eliminar(Empresa_Id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Empresa> Consultar(Dictionary<string, object> dctParametros = null, string sColumnas = "*")
        {
            try
            {
                return EmpresaAdo.Instance.Consultar(dctParametros, sColumnas);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Empresa Consultar(int Empresa_Id, string sColumnas = "*")
        {
            try
            {
                return EmpresaAdo.Instance.Consultar(Empresa_Id, sColumnas);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Consulta al store de pa_ConsultarEmpresa
        /// </summary>
        /// <param name="Empresa_Id"></param>
        /// <returns></returns>
        public Empresa Consultar(int Empresa_Id)
        {
            try
            {
                var Parametros = new Dictionary<string, object>();
                Parametros.Add("Empresa_Id", Empresa_Id);


                var Lista = EmpresaAdo.Instance.Consultar(Parametros);

                if (Lista != null && Lista.Count > 0)
                {
                    return Lista.FirstOrDefault();
                }


                return null;

            }
            catch (Exception)
            {
                throw;
            }
        }


        public bool Validar(Empresa eEmpresa)
        {
            try
            {
                var oSb = new StringBuilder();

                // sEmpNombre
                if (eEmpresa.sEmpNombre.Length == 0)
                    oSb.AppendLine("- Debe ingresar Nombre");

                // sEmpRuc
                if (eEmpresa.sEmpRuc.Length == 0)
                    oSb.AppendLine("- Debe ingresar Ruc");
                else
                    if (!Util.ValidaNumero(eEmpresa.sEmpRuc))
                    oSb.AppendLine("- El Ruc ingresado tiene carácteres no válidos");
                if (eEmpresa.sEmpRuc.Length > 15)
                    oSb.AppendLine("- Longitud de Ruc no válida");

                // sEmpEmail                
                if (eEmpresa.sEmpEmail.Length != 0 && !Util.ValidarEmail(eEmpresa.sEmpEmail))
                    oSb.AppendLine("- El Email ingresado no es válido");

                // sEmpUbigeo
                if (eEmpresa.Ubigeo_Id == 0)
                    oSb.AppendLine("- Debe seleccionar Ubigeo");

                // sEmpDireccion
                if (eEmpresa.sEmpDireccion.Length == 0)
                    oSb.AppendLine("- Debe ingresar Dirección");

                // nEmpEstado
                if (eEmpresa.nEmpEstado != 1 && eEmpresa.nEmpEstado != 0)
                    oSb.AppendLine("- Estado no válido");

                if (!oSb.ToString().Equals(""))
                    throw new Exception("Se han producido errores al intentar guardar Empresa:\n" + oSb.ToString());

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Empresa  Consultar(string sRUCEmpresa)
        {
            try
            {
                string sSql = "SELECT TOP 1 * FROM dbo.Empresa WHERE sEmpRuc=" +sRUCEmpresa;
                return EmpresaAdo.Instance.Consultar(sSql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
