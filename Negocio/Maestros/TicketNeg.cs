using Datos.Maestros;
using Entidades.Maestros;
using Negocio.Base;
using System;
using System.Collections.Generic;

namespace Negocio.Maestros
{
    public class TicketNeg : BaseNegocio
    {
        #region "Singleton"

        private static readonly Lazy<TicketNeg> oInstance = new Lazy<TicketNeg>(() => new TicketNeg());

        private TicketNeg()
        {
        }

        public static TicketNeg Instance
        {
            get
            {
                return oInstance.Value;
            }
        }

        #endregion

        #region "Métodos"

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oTicket"></param>
        public void Guardar(Ticket oTicket)
        {
            try
            {
                TicketAdo.Instance.Guardar(oTicket);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Ticket_Id"></param>
        public void Eliminar(int Ticket_Id)
        {
            try
            {
                TicketAdo.Instance.Eliminar(Ticket_Id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dctParametros"></param>
        /// <param name="sColumnas"></param>
        /// <returns></returns>
        public List<Ticket> Consultar(Dictionary<string, object> dctParametros = null,
            string sColumnas = "*")
        {
            try
            {
                return TicketAdo.Instance.Consultar(dctParametros, sColumnas);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Ticket_Id"></param>
        /// <param name="sColumnas"></param>
        /// <returns></returns>
        public Ticket Consultar(int Ticket_Id, string sColumnas = "*")
        {
            try
            {
                return TicketAdo.Instance.Consultar(Ticket_Id, sColumnas);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtcColumnaValor"></param>
        /// <param name="dtcColumnaId"></param>
        public void Actualizar(Dictionary<string, object> dtcColumnaValor, Dictionary<string,object> dtcColumnaId)
        {
            try
            {
                TicketAdo.Instance.Actualizar(dtcColumnaValor, dtcColumnaId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticket_Id"></param>
        /// <param name="nTicUltimoNumero"></param>
        public void AumentarCorrelativo(int ticket_Id, int nTicUltimoNumero)
        {
            try
            {
                var ParametrosValor = new Dictionary<string, object>();
                ParametrosValor.Add("nTicUltimoNumero", (nTicUltimoNumero + 1));

                var Parametros_Id = new Dictionary<string, object>();
                Parametros_Id.Add("ticket_Id", ticket_Id);

                TicketAdo.Instance.Actualizar(ParametrosValor, Parametros_Id);
            }
            catch (Exception)
            {
                throw;
            }
        }


        #endregion

    }
}
