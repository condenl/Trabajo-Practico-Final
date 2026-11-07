using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UI.UserControls;
using BE;
using BE.Seguridad;
using Servicios;
using UI;

// Layout-only check for the separate client form. --check never invokes persistence.
internal static class VentasLayoutProbe
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var actor = new Usuario(); actor.Permisos.Add(new Patente { PermisoAsociado = TipoPermiso.Ventas_Registrar }); Session.Instance.Login(actor);
            using (var form = new FormAltaCliente())
            {
                form.CreateControl();
                foreach (float fontSize in new[] { 8.25f, 12f, 16f })
                using (var font = new Font("Segoe UI", fontSize))
                {
                    form.Font = font;
                    foreach (int width in new[] { 600, 800, 1100 })
                    {
                        form.ClientSize = new Size(width, 650);
                        LayoutTree(form);
                        foreach (CheckBox check in new[] { (CheckBox)form.Controls.Find("chkMarketing", true).Single(), (CheckBox)form.Controls.Find("chkMembresia", true).Single() })
                        {
                            Size preferred = check.GetPreferredSize(Size.Empty);
                            if (check.Width < preferred.Width || check.Height < preferred.Height ||
                                !check.Parent.ClientRectangle.Contains(check.Bounds))
                                throw new Exception("Checkbox clipped: " + check.Name + " width=" + width + " font=" + fontSize);
                        }
                        Console.WriteLine("PASS Ventas width=" + width + " font=" + fontSize);
                    }
                    form.Font = Control.DefaultFont;
                }
                form.ClientSize = new Size(800, 600);
                if (!args.Contains("--check")) throw new Exception("Use --check; este probe no admite guardado interactivo con una sesión ficticia.");
            }
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        finally { Session.Instance.Logout(); }
    }

    private static void LayoutTree(Control control)
    {
        control.PerformLayout();
        foreach (Control child in control.Controls) LayoutTree(child);
        control.PerformLayout();
    }
}
