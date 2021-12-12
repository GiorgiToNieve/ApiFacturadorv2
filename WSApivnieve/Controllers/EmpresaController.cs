using Entidades.Maestros;
using Negocio.Maestros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WSApivnieve.Controllers
{
    [RoutePrefix("servicio/Empresa")]
    public class EmpresaController : ApiController
    {
        // GET: api/Empresa
        [HttpGet]//poner siempre quesea get
        public List<Empresa> Get()
        {
            try
            {
              return  EmpresaNeg.Instance.Consultar();
            }
            catch (Exception)
            {

                throw;
            }
        }

        // GET: api/Empresa/5
        public Empresa Get(int id)
        {
            try
            {
                return EmpresaNeg.Instance.Consultar(id, "*");
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        // POST: api/Empresa
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Empresa/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Empresa/5
        public void Delete(int id)
        {
        }
    }
}
