using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace UI
{
    // Equivalente a una hoja de estilos para los controles nativos de WinForms.
    // Solo presentación: no modifica datos, permisos, eventos Click ni etiquetas de traducción.
    public static class TemaAplicacion
    {
        public static readonly Color Fondo = Color.FromArgb(243, 246, 248);
        public static readonly Color Superficie = Color.White;
        public static readonly Color Texto = Color.FromArgb(36, 52, 63);
        public static readonly Color Acento = Color.FromArgb(32, 91, 101);
        public static readonly Color AcentoSuave = Color.FromArgb(228, 240, 242);
        public static readonly Color Borde = Color.FromArgb(211, 222, 228);
        public static readonly Color Navegacion = Color.FromArgb(30, 47, 60);
        public static readonly Color Positivo = Color.FromArgb(61, 117, 93);
        public static readonly Color Negativo = Color.FromArgb(164, 65, 65);
        public static readonly Color Neutro = Color.FromArgb(159, 173, 182);
        private static readonly Dictionary<string, Font> fuentes = new Dictionary<string, Font>();
        private static readonly HashSet<string> principales = new HashSet<string> {
            "btnIniciarSesion", "btnGuardarCatalogo", "btnRegistrarCliente", "btnConfirmarVenta",
            "btnCrearOrden", "btnEnviarRevision", "btnAprobarOrden", "btnRegistrarRecepcion",
            "btnRegistrarCotizacion", "btnCompararProveedores", "btnConfirmarProveedor", "btnAgregar", "btnModificar"
        };

        public static void Aplicar(Control control)
        {
            if (SystemInformation.HighContrast) return;
            control.SuspendLayout();
            // El tamaño de los contenedores sigue a cargo de su diseño y del escalado de Windows.
            if (!(control is ContainerControl) && !(control is Panel) && !(control is TabControl))
                control.Font = Fuente(Math.Max(9, control.Font.SizeInPoints), control.Font.Style & ~FontStyle.Underline);
            if (control.ForeColor == SystemColors.ControlText || control.ForeColor == Color.Black)
                control.ForeColor = Texto; // Conserva los colores explícitos de errores y advertencias.

            if (control is Form) control.BackColor = Fondo;
            else if (control is TabPage) { ((TabPage)control).UseVisualStyleBackColor = false; control.BackColor = Superficie; }
            else if (control is UserControl || control is Panel) control.BackColor = Superficie;
            else if (control is Label || control is CheckBox || control is RadioButton || control is GroupBox) {
                if (control.BackColor == SystemColors.Control || control.BackColor == Superficie || control.BackColor == Fondo)
                    control.BackColor = Color.Transparent;
            }
            else if (control is TextBoxBase) {
                var campo = (TextBoxBase)control; campo.BorderStyle = BorderStyle.FixedSingle;
                campo.BackColor = campo.ReadOnly ? Fondo : Superficie; campo.ForeColor = Texto;
            }
            else if (control is ComboBox || control is NumericUpDown || control is ListBox || control is TreeView) {
                control.BackColor = Superficie; control.ForeColor = Texto;
            }

            var boton = control as Button;
            if (boton != null) {
                AplicarBoton(boton);
                boton.EnabledChanged -= CambiarEstadoBoton; boton.EnabledChanged += CambiarEstadoBoton;
            }
            var tabla = control as DataGridView;
            if (tabla != null) AplicarTabla(tabla);
            var menu = control as ToolStrip;
            if (menu != null) {
                menu.Font = Fuente(9, FontStyle.Regular); menu.BackColor = Fondo; menu.ForeColor = Texto;
                menu.Renderer = new ToolStripProfessionalRenderer(new ColoresMenu());
            }
            var grafico = control as Chart;
            if (grafico != null) AplicarGrafico(grafico);

            // No interviene en los controles internos del selector numérico, calendario o grilla.
            if (control is Form || control is UserControl || control is Panel || control is GroupBox || control is TabControl || control is TabPage || control is SplitContainer) {
                control.ControlAdded -= ControlAgregado; control.ControlAdded += ControlAgregado;
                foreach (Control hijo in control.Controls) Aplicar(hijo);
            }
            control.ResumeLayout(true);
        }

        private static Font Fuente(float puntos, FontStyle estilo)
        {
            string clave = puntos.ToString(CultureInfo.InvariantCulture) + ":" + estilo;
            Font fuente;
            if (!fuentes.TryGetValue(clave, out fuente)) {
                fuente = new Font("Segoe UI", puntos, estilo); fuentes.Add(clave, fuente);
            }
            return fuente;
        }

        private static void ControlAgregado(object sender, ControlEventArgs e) { Aplicar(e.Control); }
        private static void CambiarEstadoBoton(object sender, EventArgs e) { AplicarBoton((Button)sender); }

        private static void AplicarBoton(Button boton)
        {
            bool principal = principales.Contains(boton.Name);
            bool destructivo = boton.Name == "btnBaja" || boton.Name == "btnRechazarOrden";
            boton.FlatStyle = FlatStyle.Flat; boton.UseVisualStyleBackColor = false;
            boton.FlatAppearance.BorderSize = 1;
            boton.FlatAppearance.BorderColor = principal && boton.Enabled ? Acento : Borde;
            boton.FlatAppearance.MouseOverBackColor = principal ? Color.FromArgb(25, 73, 82) : AcentoSuave;
            boton.FlatAppearance.MouseDownBackColor = principal ? Color.FromArgb(19, 59, 66) : Borde;
            boton.BackColor = !boton.Enabled ? Fondo : principal ? Acento : Superficie;
            boton.ForeColor = !boton.Enabled ? Color.FromArgb(107, 121, 132) : principal ? Color.White : destructivo ? Negativo : Texto;
        }

        public static void AplicarNavegacion(Button boton, bool seleccionado)
        {
            if (SystemInformation.HighContrast) return;
            boton.FlatStyle = FlatStyle.Flat; boton.UseVisualStyleBackColor = false;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = Color.FromArgb(47, 72, 87);
            boton.FlatAppearance.MouseDownBackColor = Acento;
            boton.BackColor = seleccionado ? Acento : Navegacion; boton.ForeColor = Color.White;
            boton.Font = Fuente(9, seleccionado ? FontStyle.Bold : FontStyle.Regular);
        }

        private static void AplicarTabla(DataGridView tabla)
        {
            tabla.BackgroundColor = Superficie; tabla.BorderStyle = BorderStyle.None;
            tabla.GridColor = Borde; tabla.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            tabla.EnableHeadersVisualStyles = false;
            tabla.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            tabla.ColumnHeadersDefaultCellStyle.BackColor = Acento;
            tabla.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            tabla.ColumnHeadersDefaultCellStyle.SelectionBackColor = Acento;
            tabla.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            tabla.ColumnHeadersDefaultCellStyle.Font = Fuente(9, FontStyle.Bold);
            tabla.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            tabla.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            tabla.DefaultCellStyle.Font = Fuente(9, FontStyle.Regular);
            tabla.DefaultCellStyle.BackColor = Superficie; tabla.DefaultCellStyle.ForeColor = Texto;
            tabla.DefaultCellStyle.SelectionBackColor = AcentoSuave; tabla.DefaultCellStyle.SelectionForeColor = Texto;
            tabla.DefaultCellStyle.Padding = new Padding(4, 3, 4, 3);
            tabla.AlternatingRowsDefaultCellStyle.BackColor = Fondo;
            tabla.RowHeadersDefaultCellStyle.BackColor = Fondo;
            tabla.RowHeadersDefaultCellStyle.SelectionBackColor = AcentoSuave;
            tabla.RowTemplate.Height = Math.Max(28, tabla.RowTemplate.Height);
            foreach (DataGridViewRow fila in tabla.Rows) fila.MinimumHeight = Math.Max(28, fila.MinimumHeight);
        }

        private static void AplicarGrafico(Chart grafico)
        {
            grafico.BackColor = Superficie; grafico.BorderlineColor = Borde;
            foreach (Series serie in grafico.Series) serie.Font = Fuente(9, FontStyle.Regular);
            foreach (Title titulo in grafico.Titles) { titulo.ForeColor = Texto; titulo.Font = Fuente(10, FontStyle.Bold); }
            foreach (ChartArea area in grafico.ChartAreas) {
                area.BackColor = Superficie;
                foreach (Axis eje in new[] { area.AxisX, area.AxisY }) {
                    eje.LabelStyle.Font = Fuente(9, FontStyle.Regular); eje.LabelStyle.ForeColor = Texto;
                    eje.LineColor = Borde; eje.MajorGrid.LineColor = Borde; eje.MajorTickMark.LineColor = Borde;
                }
            }
        }

        private sealed class ColoresMenu : ProfessionalColorTable
        {
            public override Color ToolStripGradientBegin { get { return Fondo; } }
            public override Color ToolStripGradientMiddle { get { return Fondo; } }
            public override Color ToolStripGradientEnd { get { return Fondo; } }
            public override Color MenuItemSelected { get { return AcentoSuave; } }
            public override Color MenuItemBorder { get { return Borde; } }
            public override Color MenuBorder { get { return Borde; } }
            public override Color MenuItemPressedGradientBegin { get { return AcentoSuave; } }
            public override Color MenuItemPressedGradientEnd { get { return AcentoSuave; } }
            public override Color ImageMarginGradientBegin { get { return Fondo; } }
            public override Color ImageMarginGradientMiddle { get { return Fondo; } }
            public override Color ImageMarginGradientEnd { get { return Fondo; } }
        }
    }
}
