using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE.Seguridad;
using MPP;

namespace DAL
{
    public class SeguridadDAL
    {
        private static SeguridadDAL instancia;
        private static readonly object bloqueo = new object();

        private SeguridadDAL() { }

        public static SeguridadDAL Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new SeguridadDAL();
                    return instancia;
                }
            }
        }

        public List<Componente> ObtenerPermisosDeUsuario(int idUsuario)
        {
            DataTable dtComponentes = Conexion.Instancia.Leer("sp_Seguridad_ObtenerComponentes", null);
            DataTable dtRelaciones = Conexion.Instancia.Leer("sp_Seguridad_ObtenerFamiliaHijos", null);

            SqlParameter[] param = { new SqlParameter("@IdUsuario", idUsuario) };
            DataTable dtAsignaciones = Conexion.Instancia.Leer("sp_Seguridad_ObtenerComponentesPorUsuario", param);
            return SeguridadMPP.Instancia.ConstruirArbolUsuario(dtComponentes, dtRelaciones, dtAsignaciones);
        }

        public Familia ObtenerFamiliaCompleta(int idFamilia)
        {
            DataTable dtComponentes = Conexion.Instancia.Leer("sp_Seguridad_ObtenerComponentes", null);
            DataTable dtRelaciones = Conexion.Instancia.Leer("sp_Seguridad_ObtenerFamiliaHijos", null);

            return SeguridadMPP.Instancia.ConstruirFamilia(dtComponentes, dtRelaciones, idFamilia);
        }

        public void AsignarComponenteAUsuario(int idUsuario, int idComponente)
        {
            SqlParameter[] param = {
                new SqlParameter("@IdUsuario", idUsuario),
                new SqlParameter("@IdComponente", idComponente)
            };

            Conexion.Instancia.RUD("sp_Seguridad_AsignarComponenteAUsuario", param);
        }

        public List<Componente> ObtenerTodasLasFamilias()
        {
            DataTable dt = Conexion.Instancia.Leer("sp_Seguridad_ObtenerFamilias", null);

            List<Componente> familias = new List<Componente>();
            foreach (DataRow row in dt.Rows)
            {
                Familia f = new Familia();
                f.Id = Convert.ToInt32(row["Id"]);
                f.Nombre = row["Nombre"].ToString();
                familias.Add(f);
            }
            return familias;
        }

        public void InsertarFamilia(string nombre)
        {
            SqlParameter[] param = { new SqlParameter("@Nombre", nombre) };
            Conexion.Instancia.RUD("sp_Seguridad_InsertarFamilia", param);
        }

        public void ModificarFamilia(int id, string nombre)
        {
            SqlParameter[] param = { new SqlParameter("@Id", id), new SqlParameter("@Nombre", nombre) };
            Conexion.Instancia.RUD("sp_Seguridad_ModificarFamilia", param);
        }

        public void EliminarFamilia(int id)
        {
            SqlParameter[] param = { new SqlParameter("@Id", id) };
            Conexion.Instancia.RUD("sp_Seguridad_EliminarFamilia", param);
        }

        public List<Componente> ObtenerPatentes()
        {
            DataTable dt = Conexion.Instancia.Leer("sp_Seguridad_ObtenerPatentes", null);
            List<Componente> lista = new List<Componente>();
            foreach (DataRow row in dt.Rows)
            {
                Patente p = new Patente
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nombre = row["Nombre"].ToString(),
                    PermisoAsociado = (TipoPermiso)Enum.Parse(typeof(TipoPermiso), row["PermisoAsociado"].ToString())
                };
                lista.Add(p);
            }
            return lista;
        }

        public List<Componente> ObtenerPermisosDeRol(int idRol)
        {
            SqlParameter[] param = { new SqlParameter("@IdRol", idRol) };
            DataTable dt = Conexion.Instancia.Leer("sp_Seguridad_ObtenerHijosDeRol", param);

            List<Componente> lista = new List<Componente>();
            foreach (DataRow row in dt.Rows)
            {
                Componente c;

                if (row["Tipo"].ToString() == "Familia")
                {
                    c = new Familia();
                }
                else
                {
                    c = new Patente();
                    if (row["PermisoAsociado"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["PermisoAsociado"].ToString()))
                    {
                        ((Patente)c).PermisoAsociado = (TipoPermiso)Enum.Parse(typeof(TipoPermiso), row["PermisoAsociado"].ToString());
                    }
                }

                c.Id = Convert.ToInt32(row["Id"]);
                c.Nombre = row["Nombre"].ToString();
                lista.Add(c);
            }
            return lista;
        }

        public void AgregarPermisoARol(int idRol, int idPermiso)
        {
            SqlParameter[] param = { new SqlParameter("@IdRol", idRol), new SqlParameter("@IdPermiso", idPermiso) };
            Conexion.Instancia.RUD("sp_Seguridad_AgregarPermisoARol", param);
        }

        public void QuitarPermisoDeRol(int idRol, int idPermiso)
        {
            SqlParameter[] param = { new SqlParameter("@IdRol", idRol), new SqlParameter("@IdPermiso", idPermiso) };
            Conexion.Instancia.RUD("sp_Seguridad_QuitarPermisoDeRol", param);
        }
    }
}