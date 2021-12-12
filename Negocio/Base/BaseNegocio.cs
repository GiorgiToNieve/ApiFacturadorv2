using Utilitarios;

namespace Negocio.Base
{
    public class BaseNegocio
    {
        protected LoginInfo oLoginInfo;

        public BaseNegocio Login(LoginInfo loginInfo)
        {
            this.oLoginInfo = loginInfo;
            return this;
        }
    }
}
