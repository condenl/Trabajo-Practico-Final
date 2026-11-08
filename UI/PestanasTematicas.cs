using System;
using System.Drawing;
using System.Windows.Forms;

namespace UI
{
    // TabControl ignora BackColor en su marco nativo. Solo se reemplaza su pintura;
    // selección, teclado, eventos y distribución de páginas siguen siendo de WinForms.
    internal sealed class PestanasTematicas : TabControl
    {
        public PestanasTematicas()
        {
            if (!SystemInformation.HighContrast)
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (SystemInformation.HighContrast) { base.OnPaint(e); return; }
            e.Graphics.Clear(TemaAplicacion.Fondo);
            var contenido = DisplayRectangle; contenido.Inflate(1, 1);
            using (var borde = new Pen(TemaAplicacion.Borde)) e.Graphics.DrawRectangle(borde, contenido);
            for (int i = 0; i < TabPages.Count; i++) {
                Rectangle caja = GetTabRect(i); bool activa = i == SelectedIndex;
                using (var fondo = new SolidBrush(activa ? TemaAplicacion.AcentoSuave : TemaAplicacion.Elevado))
                    e.Graphics.FillRectangle(fondo, caja);
                TextRenderer.DrawText(e.Graphics, TabPages[i].Text, Font, caja,
                    activa ? TemaAplicacion.Texto : TemaAplicacion.TextoSecundario,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                if (activa) {
                    using (var linea = new SolidBrush(TemaAplicacion.TextoSecundario))
                        e.Graphics.FillRectangle(linea, caja.Left, caja.Bottom - 2, caja.Width, 2);
                    if (Focused && ShowFocusCues) {
                        caja.Inflate(-3, -3);
                        ControlPaint.DrawFocusRectangle(e.Graphics, caja, TemaAplicacion.Texto, TemaAplicacion.AcentoSuave);
                    }
                }
            }
        }

        protected override void OnSelectedIndexChanged(EventArgs e) { base.OnSelectedIndexChanged(e); Invalidate(); }
        protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); Invalidate(); }
    }
}
