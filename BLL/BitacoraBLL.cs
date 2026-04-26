using BE;
using BE.Interfaces;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class BitacoraBLL : IObserver
    {
        private static BitacoraBLL instancia;
        private static readonly object bloqueo = new object();

        private BitacoraBLL() { }

        public static BitacoraBLL Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null)
                    {
                        instancia = new BitacoraBLL();
                    }
                    return instancia;
                }
            }
        }

        public void Actualizar(EventoBitacora evento)
        {
            Registrar(evento);
        }

        public int Registrar(EventoBitacora evento)
        {
            return BitacoraDAL.Instancia.Insertar(evento);
        }

        public List<EventoBitacora> Consultar(DateTime? desde, DateTime? hasta, string usuario, string actividad)
        {
            return BitacoraDAL.Instancia.LeerFiltrado(desde, hasta, usuario, actividad);
        }
    }
}