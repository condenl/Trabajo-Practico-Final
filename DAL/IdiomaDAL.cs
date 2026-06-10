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
    }
}
