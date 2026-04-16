using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;

namespace Servicios
{
    public class Session
    {
        private static Session _instance;

        private static object _lock = new object();

        public Usuario Usuario { get; set; }

        public DateTime FechaInicio { get; set; }

        private Session()
        {
        }

        public static Session Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Session();
                    }
                    return _instance;
                }
            }
        }

        public void Login(Usuario usuario)
        {
            this.Usuario = usuario;
            this.FechaInicio = DateTime.Now;
        }

        public void Logout()
        {
            _instance = null;
        }

        public bool IsLogged()
        {
            return this.Usuario != null;
        }
    }
}