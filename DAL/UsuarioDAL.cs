using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using BE;
using MPP;
using Servicios;

namespace DAL
{
    public class UsuarioDAL
    {
        private static UsuarioDAL instancia;
        private static readonly object bloqueo = new object();

        private UsuarioDAL() { }

        public static UsuarioDAL Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new UsuarioDAL();
                    return instancia;
                }
            }
        }

        public List<Usuario> Listar()
        {
            DataTable dt = Conexion.Instancia.Leer("sp_ListarUsuarios", null);
            return UsuarioMPP.Instancia.MapearLista(dt);
        }

        public Usuario BuscarPorEmail(string email)
        {
            SqlParameter[] p = { new SqlParameter("@Email", email) };
            DataTable dt = Conexion.Instancia.Leer("sp_BuscarUsuarioPorEmail", p);

            if (dt.Rows.Count > 0)
                return UsuarioMPP.Instancia.MapearUsuario(dt.Rows[0]);

            return null;
        }

        public bool ExisteDni(string dniCifrado)
        {
            SqlParameter[] p = { new SqlParameter("@Dni", dniCifrado) };
            DataTable dt = Conexion.Instancia.Leer("sp_BuscarUsuarioPorDni", p);
            return dt.Rows.Count > 0;
        }

        public int Insertar(Usuario usuario)
        {
            SqlParameter[] p = CrearParametrosUsuario(usuario);
            int res = Conexion.Instancia.RUD("SP_InsertarUsuario", p);
            ModificarDV(usuario.Email);
            return res;
        }

        public int Registrar(Usuario usuario) => Insertar(usuario);

        public int Modificar(Usuario usuario)
        {
            List<SqlParameter> lista = new List<SqlParameter>(CrearParametrosUsuario(usuario));
            lista.Insert(0, new SqlParameter("@Id", usuario.Id));
            int res = Conexion.Instancia.RUD("SP_ModificarUsuario", lista.ToArray());
            ModificarDV(usuario.Email);
            return res;
        }

        public void ModificarDV(string email)
        {
            var usuario = BuscarPorEmail(email);
            if (usuario == null)
            {
                return;
            }
            usuario.DVH = Servicios.Integridad.CalcularDVH(usuario);
            SqlParameter[] p = {
                new SqlParameter("@Id", usuario.Id),
                new SqlParameter("@DVH", usuario.DVH)
            };
            Conexion.Instancia.RUD("sp_ModificarDVH", p);
            ActualizarDVV();
        }

        public int Eliminar(Usuario usuario)
        {
            SqlParameter[] p = { new SqlParameter("@Id", usuario.Id) };
            int res = Conexion.Instancia.RUD("SP_EliminarUsuario", p);
            ActualizarDVV();
            return res;
        }

        public void SumarIntentoFallido(string email)
        {
            SqlParameter[] p = { new SqlParameter("@Email", email) };
            Conexion.Instancia.RUD("sp_RegistrarFalloLogin", p);
            ModificarDV(email);
        }

        public void ResetearIntentos(string email)
        {
            SqlParameter[] p = { new SqlParameter("@Email", email) };
            Conexion.Instancia.RUD("sp_ResetearIntentos", p);
            ModificarDV(email);
        }

        public void ActualizarDVV()
        {
            List<Usuario> todos = Listar();
            string dvv = Servicios.Integridad.CalcularDVV(todos);
            DVVDAL.Instancia.Guardar("Usuario", dvv);
        }

        public void RecalcularDV()
        {
            List<Usuario> usuarios = Listar();
            foreach (var u in usuarios)
            {
                string dvhCalculado = Servicios.Integridad.CalcularDVH(u);
                if (u.DVH != dvhCalculado)
                {
                    SqlParameter[] p = {
                        new SqlParameter("@Id", u.Id),
                        new SqlParameter("@DVH", dvhCalculado)
                    };
                    Conexion.Instancia.RUD("sp_ModificarDVH", p);
                }
            }
            ActualizarDVV();
        }

        private SqlParameter[] CrearParametrosUsuario(Usuario usuario)
        {
            return new SqlParameter[]
            {
                new SqlParameter("@Dni", usuario.Dni),
                new SqlParameter("@Nombre", usuario.Nombre),
                new SqlParameter("@Apellido", usuario.Apellido),
                new SqlParameter("@Email", usuario.Email),
                new SqlParameter("@Password", usuario.Password),
                new SqlParameter("@FechaAlta", usuario.FechaAlta),
                new SqlParameter("@Activo", usuario.Activo),
                new SqlParameter("@DVH", usuario.DVH != null ? usuario.DVH : (object) DBNull.Value)
            };
        }
    }
}