using System;
using System.Collections.Generic;
using BE;
using DAL;

namespace BLL
{
    public class IdiomaBLL
    {
        public List<Idioma> Listar()
        {
            return IdiomaDAL.Instancia.Listar();
        }

        public void Guardar(Idioma idioma)
        {
            if (idioma.Id == 0)
            {
                IdiomaDAL.Instancia.Insertar(idioma);
            }
            else
            {
                IdiomaDAL.Instancia.Modificar(idioma);
            }
        }

        public void Eliminar(Idioma idioma)
        {
            IdiomaDAL.Instancia.Eliminar(idioma);
        }
    }
}
