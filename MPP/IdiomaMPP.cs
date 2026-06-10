using System;
using System.Collections.Generic;
using System.Data;
using BE;

namespace MPP
{
    public class IdiomaMPP
    {
        private static IdiomaMPP instancia;
        private static readonly object bloqueo = new object();

        private IdiomaMPP() { }

        public static IdiomaMPP Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new IdiomaMPP();
                    return instancia;
                }
            }
        }

        public Idioma MapearIdioma(DataRow fila)
        {
            Idioma idioma = new Idioma();
            idioma.Id = Convert.ToInt32(fila["Id"]);
            idioma.Nombre = fila["Nombre"].ToString();
            idioma.EsDefault = Convert.ToBoolean(fila["EsDefault"]);
            return idioma;
        }

        public List<Idioma> MapearLista(DataTable dt)
        {
            List<Idioma> lista = new List<Idioma>();
            if (dt != null)
            {
                foreach (DataRow fila in dt.Rows)
                    lista.Add(MapearIdioma(fila));
            }
            return lista;
        }
    }
}
