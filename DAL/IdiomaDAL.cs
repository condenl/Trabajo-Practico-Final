using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using MPP;

namespace DAL
{
    public class IdiomaDAL
    {
        private static IdiomaDAL instancia;
        private static readonly object bloqueo = new object();

        private IdiomaDAL() { }

        public static IdiomaDAL Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new IdiomaDAL();
                    return instancia;
                }
            }
        }

        public List<Idioma> Listar()
        {
            DataTable dt = Conexion.Instancia.Leer("sp_ListarIdiomas", null);
            return IdiomaMPP.Instancia.MapearLista(dt);
        }

        public void Insertar(Idioma idioma)
        {
            SqlParameter[] p = {
                new SqlParameter("@Nombre", idioma.Nombre),
                new SqlParameter("@EsDefault", idioma.EsDefault)
            };
            Conexion.Instancia.RUD("sp_Idioma_Insertar", p);
        }

        public void Modificar(Idioma idioma)
        {
            SqlParameter[] p = {
                new SqlParameter("@Id", idioma.Id),
                new SqlParameter("@Nombre", idioma.Nombre),
                new SqlParameter("@EsDefault", idioma.EsDefault)
            };
            Conexion.Instancia.RUD("sp_Idioma_Modificar", p);
        }

        public void Eliminar(Idioma idioma)
        {
            SqlParameter[] p = { new SqlParameter("@Id", idioma.Id) };
            Conexion.Instancia.RUD("sp_Idioma_Eliminar", p);
        }
    }
}
