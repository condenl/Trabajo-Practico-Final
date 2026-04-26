using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP
{
    public class BitacoraMPP
    {
        private static BitacoraMPP instancia;
        private static readonly object bloqueo = new object();

        private BitacoraMPP() { }

        public static BitacoraMPP Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null)
                    {
                        instancia = new BitacoraMPP();
                    }
                    return instancia;
                }
            }
        }

        public List<EventoBitacora> MapearLista(DataTable dt)
        {
            List<EventoBitacora> lista = new List<EventoBitacora>();

            foreach (DataRow row in dt.Rows)
            {
                EventoBitacora evento = new EventoBitacora();
                evento.Id = Convert.ToInt32(row["Id"]);
                evento.FechaHora = Convert.ToDateTime(row["FechaHora"]);
                evento.UsuarioEmail = row["Usuario"].ToString();
                evento.Actividad = row["Actividad"].ToString();
                evento.InformacionAsociada = row["Informacion"].ToString();

                lista.Add(evento);
            }

            return lista;
        }
    }
}