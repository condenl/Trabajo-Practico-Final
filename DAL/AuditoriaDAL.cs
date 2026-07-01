using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using MPP;

namespace DAL
{
    public class AuditoriaDAL
    {
        private static AuditoriaDAL instancia;
        private static readonly object bloqueo = new object();

        private AuditoriaDAL() { }

        public static AuditoriaDAL Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new AuditoriaDAL();
                    return instancia;
                }
            }
        }

        public void GuardarSnapshot(string accion, Usuario u, string responsable)
        {
            // OJO: El DNI en el objeto C# está desencriptado. Para guardarlo en SQL, 
            // hay que volver a encriptarlo para mantener la seguridad.
            string dniCifrado = Servicios.Encriptador.EncriptarAES(u.Dni);

            SqlParameter[] p = {
                new SqlParameter("@AuditAction", accion),
                new SqlParameter("@Id", u.Id),
                new SqlParameter("@Dni", dniCifrado),
                new SqlParameter("@Nombre", u.Nombre),
                new SqlParameter("@Apellido", u.Apellido),
                new SqlParameter("@Email", u.Email),
                new SqlParameter("@Password", u.Password),
                new SqlParameter("@FechaAlta", u.FechaAlta),
                new SqlParameter("@Activo", u.Activo),
                new SqlParameter("@IntentosFallidos", u.IntentosFallidos),
                new SqlParameter("@DVH", u.DVH ?? (object)DBNull.Value),
                new SqlParameter("@UsuarioResponsable", responsable)
            };
            Conexion.Instancia.RUD("sp_Auditoria_InsertarSnapshot", p);
        }

        public List<UsuarioAuditoria> Listar()
        {
            DataTable dt = Conexion.Instancia.Leer("sp_Auditoria_Listar", null);
            return AuditoriaMPP.Instancia.MapearLista(dt);
        }

        public void RestaurarEliminado(Usuario u)
        {
            string dniCifrado = Servicios.Encriptador.EncriptarAES(u.Dni);

            SqlParameter[] p = {
                new SqlParameter("@Id", u.Id),
                new SqlParameter("@Dni", dniCifrado),
                new SqlParameter("@Nombre", u.Nombre),
                new SqlParameter("@Apellido", u.Apellido),
                new SqlParameter("@Email", u.Email),
                new SqlParameter("@Password", u.Password),
                new SqlParameter("@FechaAlta", u.FechaAlta),
                new SqlParameter("@Activo", u.Activo),
                new SqlParameter("@IntentosFallidos", u.IntentosFallidos),
                new SqlParameter("@DVH", u.DVH ?? (object)DBNull.Value)
            };
            Conexion.Instancia.RUD("sp_RestaurarUsuarioEliminado", p);
        }
    }
}