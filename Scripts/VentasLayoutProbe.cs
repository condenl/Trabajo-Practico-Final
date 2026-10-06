using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UI.UserControls;

// Read-only layout check. Uses the same database config as UI.exe to load products.
internal static class VentasLayoutProbe
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (var form = new Form { Text = "Verificacion de layout - Ventas", ClientSize = new Size(800, 600) })
            using (var ventas = new UC_Ventas())
            {
                form.Controls.Add(ventas);
                form.CreateControl();
                ventas.CreateControl();
                foreach (float fontSize in new[] { 8.25f, 12f, 16f })
                using (var font = new Font("Segoe UI", fontSize))
                {
                    ventas.Font = font;
                    foreach (int width in new[] { 600, 800, 1100 })
                    {
                        form.ClientSize = new Size(width, 650);
                        LayoutTree(form);
                        var options = ventas.Controls.Find("opcionesCliente", true).Single();
                        foreach (CheckBox check in options.Controls.OfType<CheckBox>())
                        {
                            Size preferred = check.GetPreferredSize(Size.Empty);
                            if (check.Width < preferred.Width || check.Height < preferred.Height ||
                                !options.ClientRectangle.Contains(check.Bounds))
                                throw new Exception("Checkbox clipped: " + check.Name + " width=" + width + " font=" + fontSize);
                        }
                        Console.WriteLine("PASS Ventas width=" + width + " font=" + fontSize);
                    }
                    ventas.Font = Control.DefaultFont;
                }
                form.ClientSize = new Size(800, 600);
                if (!args.Contains("--check")) Application.Run(form);
            }
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    private static void LayoutTree(Control control)
    {
        control.PerformLayout();
        foreach (Control child in control.Controls) LayoutTree(child);
        control.PerformLayout();
    }
}
