using System;
using System.Collections.Generic;
using BE;
using DAL;
using Servicios;

namespace BLL
{
    public class TraduccionBLL
    {
        public Traduccion ObtenerTraduccion(Idioma idioma, string etiqueta)
        {
            return TraduccionDAL.Instancia.ObtenerTraduccion(idioma, etiqueta);
        }

        public List<Traduccion> ObtenerTraduccionesPorIdioma(Idioma idioma)
        {
            return TraduccionDAL.Instancia.ObtenerTraduccionesPorIdioma(idioma);
        }

        public string Traducir(string key)
        {
            Idioma idioma = GestorIdioma.Instancia.IdiomaActual;
            if (idioma == null) return key;

            Traduccion traduccion = ObtenerTraduccion(idioma, key);
            if (traduccion != null) return traduccion.Texto;

            return $"[{key}]";
        }
    }
}
