using System;
using System.Collections.Generic;
using System.Data;
using BE.Soporte;

namespace MPP
{
    public class SoporteMPP
    {
        private static SoporteMPP instancia;
        private static readonly object bloqueo = new object();

        private SoporteMPP() { }

        public static SoporteMPP Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new SoporteMPP();
                    return instancia;
                }
            }
        }

        public List<SolicitudSoporte> MapearSolicitudes(DataTable dt)
        {
            List<SolicitudSoporte> lista = new List<SolicitudSoporte>();
            foreach (DataRow row in dt.Rows)
            {
                SolicitudSoporte s = new SolicitudSoporte
                {
                    Id = Convert.ToInt32(row["Id"]),
                    EmailSolicitante = row["EmailSolicitante"].ToString(),
                    Descripcion = row["Descripcion"].ToString(),
                    FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                    ObservacionesAdmin = row["ObservacionesAdmin"] != DBNull.Value ? row["ObservacionesAdmin"].ToString() : string.Empty
                };
                s.TipoProblema.Id = Convert.ToInt32(row["IdTipoProblema"]);
                s.TipoProblema.Nombre = row["TipoProblemaNombre"].ToString();

                s.Estado.Id = Convert.ToInt32(row["IdEstado"]);
                s.Estado.Nombre = row["EstadoNombre"].ToString();

                lista.Add(s);
            }
            return lista;
        }

        public List<TipoProblema> MapearTipos(DataTable dt)
        {
            List<TipoProblema> lista = new List<TipoProblema>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new TipoProblema { Id = Convert.ToInt32(row["Id"]), Nombre = row["Nombre"].ToString() });
            }
            return lista;
        }

        public List<EstadoSolicitud> MapearEstados(DataTable dt)
        {
            List<EstadoSolicitud> lista = new List<EstadoSolicitud>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new EstadoSolicitud { Id = Convert.ToInt32(row["Id"]), Nombre = row["Nombre"].ToString() });
            }
            return lista;
        }
    }
}