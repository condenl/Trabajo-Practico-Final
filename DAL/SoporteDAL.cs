using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE.Soporte;
using MPP;

namespace DAL
{
    public class SoporteDAL
    {
        private static SoporteDAL instancia;
        private static readonly object bloqueo = new object();

        private SoporteDAL() { }

        public static SoporteDAL Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new SoporteDAL();
                    return instancia;
                }
            }
        }

        public void Insertar(SolicitudSoporte solicitud)
        {
            SqlParameter[] p = {
                new SqlParameter("@Email", solicitud.EmailSolicitante),
                new SqlParameter("@IdTipoProblema", solicitud.TipoProblema.Id),
                new SqlParameter("@Descripcion", solicitud.Descripcion)
            };
            Conexion.Instancia.RUD("sp_Soporte_InsertarSolicitud", p);
        }

        public void Actualizar(SolicitudSoporte solicitud)
        {
            SqlParameter[] p = {
                new SqlParameter("@Id", solicitud.Id),
                new SqlParameter("@IdEstado", solicitud.Estado.Id),
                new SqlParameter("@Observaciones", solicitud.ObservacionesAdmin ?? (object)DBNull.Value)
            };
            Conexion.Instancia.RUD("sp_Soporte_ActualizarSolicitud", p);
        }

        public List<SolicitudSoporte> Listar(int? idEstado = null)
        {
            SqlParameter[] p = {
                new SqlParameter("@IdEstado", idEstado.HasValue ? (object)idEstado.Value : DBNull.Value)
            };
            DataTable dt = Conexion.Instancia.Leer("sp_Soporte_ListarSolicitudes", p);
            return SoporteMPP.Instancia.MapearSolicitudes(dt);
        }

        public List<TipoProblema> ObtenerTipos()
        {
            DataTable dt = Conexion.Instancia.Leer("sp_Soporte_ListarTipos", null);
            return SoporteMPP.Instancia.MapearTipos(dt);
        }

        public List<EstadoSolicitud> ObtenerEstados()
        {
            DataTable dt = Conexion.Instancia.Leer("sp_Soporte_ListarEstados", null);
            return SoporteMPP.Instancia.MapearEstados(dt);
        }
    }
}