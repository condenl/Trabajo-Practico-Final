using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using MPP;

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

        public bool ExisteDni(int dni)
        {
            SqlParameter[] p = { new SqlParameter("@Dni", dni) };
            DataTable dt = Conexion.Instancia.Leer("sp_BuscarUsuarioPorDni", p);
            return dt.Rows.Count > 0;
        }

        public int Insertar(Usuario usuario)
        {
            SqlParameter[] p = CrearParametrosUsuario(usuario);
            return Conexion.Instancia.RUD("SP_InsertarUsuario", p);
        }

        public int Registrar(Usuario usuario) => Insertar(usuario);

        public int Modificar(Usuario usuario)
        {
            List<SqlParameter> lista = new List<SqlParameter>(CrearParametrosUsuario(usuario));
            lista.Insert(0, new SqlParameter("@Id", usuario.Id));
            return Conexion.Instancia.RUD("SP_ModificarUsuario", lista.ToArray());
        }

        public int Eliminar(Usuario usuario)
        {
            SqlParameter[] p = { new SqlParameter("@Id", usuario.Id) };
            return Conexion.Instancia.RUD("SP_EliminarUsuario", p);
        }

        public void SumarIntentoFallido(string email)
        {
            SqlParameter[] p = { new SqlParameter("@Email", email) };
            Conexion.Instancia.RUD("sp_RegistrarFalloLogin", p);
        }

        public void ResetearIntentos(string email)
        {
            SqlParameter[] p = { new SqlParameter("@Email", email) };
            Conexion.Instancia.RUD("sp_ResetearIntentos", p);
        }

        public void BloquearUsuario(string email)
        {
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
                new SqlParameter("@RolId", usuario.RolId)
            };
        }
    }
}