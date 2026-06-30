using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using MPP;

namespace DAL
{
    public class TraduccionDAL
    {
        private static TraduccionDAL instancia;
        private static readonly object bloqueo = new object();

        private TraduccionDAL() { }

        public static TraduccionDAL Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new TraduccionDAL();
                    return instancia;
                }
            }
        }

        public Traduccion ObtenerTraduccion(Idioma idioma, string etiqueta)
        {
            SqlParameter[] p = {
                new SqlParameter("@IdIdioma", idioma.Id),
                new SqlParameter("@NombreEtiqueta", etiqueta)
            };
            DataTable dt = Conexion.Instancia.Leer("sp_ObtenerTraduccion", p);

            if (dt.Rows.Count > 0)
                return TraduccionMPP.Instancia.MapearTraduccion(dt.Rows[0]);

            return null;
        }

        public List<Traduccion> ObtenerTraduccionesPorIdioma(Idioma idioma)
        {
            SqlParameter[] p = { new SqlParameter("@IdIdioma", idioma.Id) };
            DataTable dt = Conexion.Instancia.Leer("sp_ObtenerTraduccionesPorIdioma", p);
            return TraduccionMPP.Instancia.MapearLista(dt);
        }

        public void Insertar(Traduccion traduccion)
        {
            SqlParameter[] p = {
                new SqlParameter("@IdIdioma", traduccion.Idioma.Id),
                new SqlParameter("@NombreEtiqueta", traduccion.NombreEtiqueta),
                new SqlParameter("@Texto", traduccion.Texto)
            };
            Conexion.Instancia.RUD("sp_Traduccion_Insertar", p);
        }

        public void Modificar(Traduccion traduccion)
        {
            SqlParameter[] p = {
                new SqlParameter("@Id", traduccion.Id),
                new SqlParameter("@IdIdioma", traduccion.Idioma.Id),
                new SqlParameter("@NombreEtiqueta", traduccion.NombreEtiqueta),
                new SqlParameter("@Texto", traduccion.Texto)
            };
            Conexion.Instancia.RUD("sp_Traduccion_Modificar", p);
        }

        public void Eliminar(Traduccion traduccion)
        {
            SqlParameter[] p = { new SqlParameter("@Id", traduccion.Id) };
            Conexion.Instancia.RUD("sp_Traduccion_Eliminar", p);
        }
    }
}
