using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BLL;
using Servicios;

namespace UI
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            GestorEventos.Instancia.Suscribir(BitacoraBLL.Instancia);
            GestorEventos.Instancia.Suscribir(LogManager.Instancia);
            Application.Run(new Login());
        }
    }
}
