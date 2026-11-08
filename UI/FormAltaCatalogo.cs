using System;
using System.Drawing;
using System.Windows.Forms;
using BE;
using BE.Seguridad;
using BLL;
using Servicios;

namespace UI
{
    public abstract class FormAltaCatalogo : Form
    {
        private readonly TableLayoutPanel campos;
        private readonly Label error = new Label { Name = "lblErrorCatalogo", AutoSize = true, ForeColor = Color.Firebrick, Dock = DockStyle.Top, Padding = new Padding(12, 6, 12, 6) };
        protected FormAltaCatalogo(string titulo)
        {
            ExigirPermiso();
            Text = titulo; StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Font; ClientSize = new Size(560, 410);
            MinimumSize = new Size(500, 430); MaximizeBox = false; MinimizeBox = false;
            campos = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(16) };
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            var contenido = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            contenido.Controls.Add(campos);
            var pie = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(12) };
            var guardar = new Button { Text = "Guardar", Name = "btnGuardarCatalogo", AutoSize = true, Padding = new Padding(9, 4, 9, 4) };
            var cancelar = new Button { Text = "Cancelar", Name = "btnCancelarCatalogo", AutoSize = true, Padding = new Padding(9, 4, 9, 4), DialogResult = DialogResult.Cancel };
            guardar.Click += delegate { Guardar(); };
            pie.Controls.Add(guardar); pie.Controls.Add(cancelar);
            Controls.Add(contenido); Controls.Add(error); Controls.Add(pie);
            AcceptButton = guardar; CancelButton = cancelar;
            TemaAplicacion.Aplicar(this);
        }
        protected void Campo(string etiqueta, Control control)
        {
            int fila = campos.RowCount++;
            campos.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            campos.Controls.Add(new Label { Text = etiqueta, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 10, 8, 10) }, 0, fila);
            control.Dock = DockStyle.Fill; control.Margin = new Padding(0, 7, 0, 7);
            campos.Controls.Add(control, 1, fila);
        }
        protected static void ExigirPermiso()
        {
            var actor = Session.Instance.Usuario;
            if (actor == null || !actor.TienePermiso(TipoPermiso.Aprovisionamiento_Gestion))
                throw new UnauthorizedAccessException("El alta de catálogos requiere una sesión de Compras o Dueño.");
        }
        private void Guardar()
        {
            try { ExigirPermiso(); GuardarRegistro(); DialogResult = DialogResult.OK; Close(); }
            catch (Exception ex) { error.Text = ex.Message; }
        }
        protected abstract void GuardarRegistro();
    }

    public sealed class FormAltaProveedor : FormAltaCatalogo
    {
        private readonly TextBox razon = new TextBox { Name = "txtProveedorRazon", MaxLength = 150 };
        private readonly TextBox cuit = new TextBox { Name = "txtProveedorCuit", MaxLength = 20 };
        private readonly TextBox contacto = new TextBox { Name = "txtProveedorContacto", MaxLength = 200 };
        private readonly NumericUpDown antiguedad = new NumericUpDown { Name = "nudProveedorAntiguedad", Maximum = 200 };
        private readonly NumericUpDown prestigio = new NumericUpDown { Name = "nudProveedorPrestigio", Maximum = 100, DecimalPlaces = 2, Value = 50 };
        public FormAltaProveedor() : base("Nuevo proveedor")
        {
            Campo("Razón social", razon); Campo("CUIT (11 dígitos)", cuit); Campo("Contacto", contacto);
            Campo("Antigüedad (años)", antiguedad); Campo("Prestigio (0 a 100)", prestigio);
        }
        protected override void GuardarRegistro()
        {
            new ProveedorBLL().Guardar(new Proveedor { RazonSocial = razon.Text.Trim(), Cuit = cuit.Text.Trim(), Contacto = contacto.Text.Trim(),
                AntiguedadMercado = (int)antiguedad.Value, IndicePrestigio = prestigio.Value });
        }
    }

    public sealed class FormAltaPerfume : FormAltaCatalogo
    {
        private readonly TextBox codigo = new TextBox { Name = "txtProductoCodigo", MaxLength = 50 };
        private readonly TextBox nombre = new TextBox { Name = "txtProductoNombre", MaxLength = 150 };
        private readonly TextBox marca = new TextBox { Name = "txtProductoMarca", MaxLength = 100 };
        private readonly TextBox familia = new TextBox { Name = "txtProductoFamilia", MaxLength = 100 };
        private readonly NumericUpDown precio = new NumericUpDown { Name = "nudProductoPrecio", Maximum = 100000000, DecimalPlaces = 2, ThousandsSeparator = true };
        private readonly NumericUpDown minimo = new NumericUpDown { Name = "nudProductoStockMinimo", Maximum = 100000 };
        public FormAltaPerfume() : base("Nuevo perfume")
        {
            Campo("Código", codigo); Campo("Nombre", nombre); Campo("Marca", marca); Campo("Familia olfativa", familia);
            Campo("Precio de venta", precio); Campo("Stock mínimo", minimo);
        }
        protected override void GuardarRegistro()
        {
            new ProductoBLL().Guardar(new Producto { Codigo = codigo.Text.Trim(), Nombre = nombre.Text.Trim(), Marca = marca.Text.Trim(),
                FamiliaOlfativa = familia.Text.Trim(), PrecioVenta = precio.Value, StockMinimo = (int)minimo.Value });
        }
    }
}
