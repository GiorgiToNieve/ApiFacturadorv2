using Entidades.Maestros;
using Negocio.Maestros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace WSApiFMSACApp.Controllers
{

	[RoutePrefix("servicio/Empresa")]
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
		//[ResponseType(typeof(List<Empresa>))]
		[HttpPost]
		[Route("Guardar1")]
		public async Task<Empresa> Guardar1(List<Empresa> LstEmpresa)
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

		[HttpPost]
		[Route("Guardar")]
		public async Task<Empresa> Guardar(Empresa oEmpresa)
		{
			try
			{
				EmpresaNeg.Instance.Guardar(oEmpresa);
				return oEmpresa;
			}
			catch (Exception ex)
			{
				await LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				return oEmpresa;
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
