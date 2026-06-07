using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BLL;

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

            var errores = new UsuarioBLL().VerificarIntegridad();
            if (errores.Count > 0)
            {
                string msg = "Se han detectado errores de integridad en la base de datos:\n\n" + 
                             string.Join("\n", errores) + 
                             "\n\nEl sistema se cerrará por seguridad. Contacte al administrador.";
                MessageBox.Show(msg, "Error de Integridad Crítico", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                BLL.BitacoraBLL.Instancia.Actualizar(new BE.EventoBitacora("SYSTEM", "FALLA_INTEGRIDAD", "Brecha de integridad detectada: " + string.Join(" | ", errores)));
                return;
            }
            Application.Run(new Login());
        }
    }
}
