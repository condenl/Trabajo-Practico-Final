using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

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

            if (!BLL.IntegrityBLL.CheckDatabaseConnection())
            {
                MessageBox.Show("Error crítico: No se puede establecer conexión con la base de datos.", "Error de Integridad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                BLL.BitacoraBLL.Instancia.Actualizar(new BE.EventoBitacora("SYSTEM", "STARTUP_FAIL", "Falla de conexión a base de datos en arranque."));
                return;
            }

            BLL.BitacoraBLL.Instancia.Actualizar(new BE.EventoBitacora("SYSTEM", "STARTUP", "Sistema iniciado correctamente."));
            
            Application.Run(new Login());
        }
    }
}
