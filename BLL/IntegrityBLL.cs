using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;

namespace BLL
{
    public class IntegrityBLL
    {
        public static bool CheckDatabaseConnection()
        {
            try
            {
                Conexion.Instancia.Leer("SELECT 1", null);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
