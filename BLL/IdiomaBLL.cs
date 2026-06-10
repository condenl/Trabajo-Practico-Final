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
    }
}
