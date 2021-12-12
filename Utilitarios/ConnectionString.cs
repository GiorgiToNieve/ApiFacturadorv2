using System.Configuration;

namespace Utilitarios
{
    public static class ConnectionString
    {

        public static string Conn = ConfigurationManager.ConnectionStrings["conexion_BD"].ConnectionString;

        //public const string
        //    Conn = "Data Source=ASUS; Initial Catalog=BD_FMSAC;Persist Security Info=True;User ID=admin;Password=admin;";


    }
}
