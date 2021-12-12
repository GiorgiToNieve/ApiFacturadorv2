using Entidades.Maestros;
using Negocio.Maestros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace WSApiFMSACApp.Controllers
{
	public class EmpresaController : ApiController
	{
		// GET: api/Empresa
		[HttpGet]
		public async Task<List<Empresa>> Get()
		{
			try
			{
				var Parametros = new Dictionary<string, object>();
				return await Task.Run(() => EmpresaNeg.Instance.Consultar(Parametros));
			}
			catch (Exception)
			{
				throw;
			}
		}

		// GET: api/Empresa/5
		public string Get(int id)
		{
			return "value";
		}

		// POST: api/Empresa
		[ResponseType(typeof(List<Empresa>))]
		[Route("Guardar")]
		public async Task<Empresa> Guardar(List<Empresa> LstEmpresa)
		{
			try
			{
				EmpresaNeg.Instance.Guardar(LstEmpresa.FirstOrDefault());
				return LstEmpresa.FirstOrDefault();
			}
			catch (Exception ex)
			{
				await LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				return LstEmpresa.FirstOrDefault();
			}
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
