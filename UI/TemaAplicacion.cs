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
        public static readonly Color Fondo = Color.FromArgb(23, 26, 31);
        public static readonly Color Superficie = Color.FromArgb(32, 37, 44);
        public static readonly Color Campo = Color.FromArgb(24, 29, 35);
        public static readonly Color Elevado = Color.FromArgb(43, 50, 60);
        public static Color Texto { get { return SystemInformation.HighContrast ? SystemColors.ControlText : Color.FromArgb(231, 235, 241); } }
        public static readonly Color TextoSecundario = Color.FromArgb(164, 175, 190);
        public static readonly Color Error = Color.FromArgb(244, 164, 164);
        public static readonly Color Acento = Color.FromArgb(78, 97, 126);
        public static readonly Color AcentoSuave = Color.FromArgb(48, 61, 80);
        public static readonly Color Borde = Color.FromArgb(65, 75, 90);
        public static readonly Color Navegacion = Color.FromArgb(16, 19, 24);
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
            FormatoPresentacion.Aplicar(control);
            if (SystemInformation.HighContrast) return;
            control.SuspendLayout();
            // El tamaño de los contenedores sigue a cargo de su diseño y del escalado de Windows.
            if (!(control is ContainerControl) && !(control is Panel) && !(control is TabControl))
                control.Font = Fuente(Math.Max(9, control.Font.SizeInPoints), control.Font.Style & ~FontStyle.Underline);
            if (control.ForeColor == SystemColors.ControlText || control.ForeColor == Color.Black)
                control.ForeColor = Texto;
            else if (control.ForeColor == Color.Firebrick) control.ForeColor = Error;

            if (control is Form) control.BackColor = Fondo;
            else if (control is TabPage) { ((TabPage)control).UseVisualStyleBackColor = false; control.BackColor = Superficie; }
            else if (control is UserControl || control is Panel) control.BackColor = Superficie;
            else if (control is Label || control is CheckBox || control is RadioButton || control is GroupBox) {
                if (control.BackColor == SystemColors.Control || control.BackColor == Superficie || control.BackColor == Fondo)
                    control.BackColor = Color.Transparent;
            }
            else if (control is TextBoxBase) {
                var campo = (TextBoxBase)control; campo.BorderStyle = BorderStyle.FixedSingle;
                campo.BackColor = campo.ReadOnly ? Elevado : Campo; campo.ForeColor = Texto;
            }
            else if (control is ComboBox || control is NumericUpDown || control is ListBox || control is TreeView) {
                control.BackColor = Campo; control.ForeColor = Texto;
            }

            var combo = control as ComboBox;
            if (combo != null && combo.DropDownStyle == ComboBoxStyle.DropDownList) {
                combo.FlatStyle = FlatStyle.Flat; combo.DrawMode = DrawMode.OwnerDrawFixed;
                combo.ItemHeight = Math.Max(combo.Font.Height + 4, combo.ItemHeight);
                combo.DrawItem -= DibujarOpcion; combo.DrawItem += DibujarOpcion;
            }

            var boton = control as Button;
            if (boton != null) {
                AplicarBoton(boton);
                boton.EnabledChanged -= CambiarEstadoBoton; boton.EnabledChanged += CambiarEstadoBoton;
                boton.Paint -= PintarBotonDeshabilitado; boton.Paint += PintarBotonDeshabilitado;
            }
            var tabla = control as DataGridView;
            if (tabla != null) AplicarTabla(tabla);
            var menu = control as ToolStrip;
            if (menu != null) {
                menu.Font = Fuente(9, FontStyle.Regular); menu.BackColor = Fondo; menu.ForeColor = Texto;
                menu.Renderer = new RenderizadorMenu();
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

        public static Color ColorMensaje(bool error)
        { return SystemInformation.HighContrast ? SystemColors.ControlText : error ? Error : Texto; }

        private static void DibujarOpcion(object sender, DrawItemEventArgs e)
        {
            var combo = (ComboBox)sender;
            Color fondo = (e.State & DrawItemState.Selected) != 0 ? AcentoSuave : Campo;
            using (var pincel = new SolidBrush(fondo)) e.Graphics.FillRectangle(pincel, e.Bounds);
            string texto = e.Index >= 0 && e.Index < combo.Items.Count ? combo.GetItemText(combo.Items[e.Index]) : combo.Text;
            TextRenderer.DrawText(e.Graphics, texto, combo.Font, e.Bounds,
                combo.Enabled ? Texto : TextoSecundario, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            e.DrawFocusRectangle();
        }

        private static void PintarBotonDeshabilitado(object sender, PaintEventArgs e)
        {
            var boton = (Button)sender;
            if (boton.Enabled || SystemInformation.HighContrast) return;
            e.Graphics.Clear(Fondo);
            using (var borde = new Pen(Borde)) e.Graphics.DrawRectangle(borde, 0, 0, boton.Width - 1, boton.Height - 1);
            TextRenderer.DrawText(e.Graphics, boton.Text, boton.Font, boton.ClientRectangle, TextoSecundario,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }

        private static void AplicarBoton(Button boton)
        {
            bool principal = principales.Contains(boton.Name);
            bool destructivo = boton.Name == "btnBaja" || boton.Name == "btnRechazarOrden";
            boton.FlatStyle = FlatStyle.Flat; boton.UseVisualStyleBackColor = false;
            boton.FlatAppearance.BorderSize = 1;
            boton.FlatAppearance.BorderColor = principal && boton.Enabled ? Acento : Borde;
            boton.FlatAppearance.MouseOverBackColor = principal ? Color.FromArgb(94, 115, 147) : AcentoSuave;
            boton.FlatAppearance.MouseDownBackColor = principal ? Color.FromArgb(60, 77, 103) : Borde;
            boton.BackColor = !boton.Enabled ? Fondo : principal ? Acento : Elevado;
            boton.ForeColor = !boton.Enabled ? TextoSecundario : principal ? Color.White : destructivo ? Error : Texto;
        }

        public static void AplicarNavegacion(Button boton, bool seleccionado)
        {
            if (SystemInformation.HighContrast) return;
            boton.FlatStyle = FlatStyle.Flat; boton.UseVisualStyleBackColor = false;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = Elevado;
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
            tabla.ColumnHeadersDefaultCellStyle.BackColor = Elevado;
            tabla.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            tabla.ColumnHeadersDefaultCellStyle.SelectionBackColor = Elevado;
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
            tabla.RowHeadersDefaultCellStyle.ForeColor = Texto;
            tabla.RowHeadersDefaultCellStyle.SelectionBackColor = AcentoSuave;
            tabla.RowHeadersDefaultCellStyle.SelectionForeColor = Texto;
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

        private sealed class RenderizadorMenu : ToolStripProfessionalRenderer
        {
            public RenderizadorMenu() : base(new ColoresMenu()) { }
            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            { e.TextColor = e.Item.Enabled ? Texto : TextoSecundario; base.OnRenderItemText(e); }
        }

        private sealed class ColoresMenu : ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground { get { return Superficie; } }
            public override Color MenuStripGradientBegin { get { return Fondo; } }
            public override Color MenuStripGradientEnd { get { return Fondo; } }
            public override Color MenuItemSelectedGradientBegin { get { return AcentoSuave; } }
            public override Color MenuItemSelectedGradientEnd { get { return AcentoSuave; } }
            public override Color MenuItemPressedGradientMiddle { get { return AcentoSuave; } }
            public override Color SeparatorDark { get { return Borde; } }
            public override Color SeparatorLight { get { return Borde; } }
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
