using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilitarios
{
    public class LoginInfo
    {
        public int Usuario_Id { get; set; }
        public string sUsuLogin { private set; get; }
        public string sAppNombre { private set; get; }
        public string sUsuDispositivo { private set; get; }
        public int Empresa_Id { private set; get; }
        public int Personal_Id { get; set; }
        public int Area_Id { get; set; }
        public string sAreDescripcion { get; set; }
        public string sPerPrimer_Nombre { get; set; }
        public string sPerSegundo_Nombre { get; set; }
        public string sPerApellido_Paterno { get; set; }
        public string sPerApellido_Materno { get; set; }
        public int nEstado { get; set; }

        public string sUsuToken { get { return sUsuLogin + "|" + sAppNombre + "|" + sUsuDispositivo; } }


        public LoginInfo(string sUsuLogin, string sAppNombre, string sUsuDispositivo, int EmpresaId)
        {
            this.sUsuLogin = sUsuLogin;
            this.sAppNombre = sAppNombre;
            this.sUsuDispositivo = sUsuDispositivo;
            this.Empresa_Id = EmpresaId;
        }
        public LoginInfo(int nUsuario_Id, string sUsuLogin, string sAppNombre, string sUsuDispositivo, int EmpresaId, int Personal_Id, int Area_Id, string sAreDescripcion,
                         string sPerPrimer_Nombre, string sPerSegundo_Nombre, string sPerApellido_Paterno, string sPerApellido_Materno, int nEstado)
        {
            this.Usuario_Id = nUsuario_Id;
            this.sUsuLogin = sUsuLogin;
            this.sAppNombre = sAppNombre;
            this.sUsuDispositivo = sUsuDispositivo;
            this.Empresa_Id = EmpresaId;
            this.Personal_Id = Personal_Id;
            this.Area_Id = Area_Id;
            this.sAreDescripcion = sAreDescripcion;
            this.sPerPrimer_Nombre = sPerPrimer_Nombre;
            this.sPerSegundo_Nombre = sPerSegundo_Nombre;
            this.sPerApellido_Paterno = sPerApellido_Paterno;
            this.sPerApellido_Materno = sPerApellido_Materno;
            this.nEstado = nEstado;

        }
    }
}
