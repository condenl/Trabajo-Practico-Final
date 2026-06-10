using System;
using System.Collections.Generic;
using System.Data;
using BE;

namespace MPP
{
    public class TraduccionMPP
    {
        private static TraduccionMPP instancia;
        private static readonly object bloqueo = new object();

        private TraduccionMPP() { }

        public static TraduccionMPP Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new TraduccionMPP();
                    return instancia;
                }
            }
        }

        public Traduccion MapearTraduccion(DataRow fila)
        {
            Traduccion traduccion = new Traduccion();
            traduccion.Id = Convert.ToInt32(fila["Id"]);
            traduccion.NombreEtiqueta = fila["NombreEtiqueta"].ToString();
            traduccion.Texto = fila["Texto"].ToString();
            
            traduccion.Idioma = new Idioma { Id = Convert.ToInt32(fila["IdIdioma"]) };
            
            return traduccion;
        }

        public List<Traduccion> MapearLista(DataTable dt)
        {
            List<Traduccion> lista = new List<Traduccion>();
            if (dt != null)
            {
                foreach (DataRow fila in dt.Rows)
                    lista.Add(MapearTraduccion(fila));
            }
            return lista;
        }
    }
}
