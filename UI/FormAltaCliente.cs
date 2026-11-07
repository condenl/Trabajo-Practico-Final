using System;
using System.Drawing;
using System.Windows.Forms;
using BE;
using BE.Seguridad;
using BLL;
using Servicios;

namespace UI
{
    public sealed class FormAltaCliente : Form
    {
        public Cliente ClienteCreado { get; private set; }
        private readonly TextBox nombre = new TextBox { Name = "txtNombre", MaxLength = 100 };
        private readonly TextBox apellido = new TextBox { Name = "txtApellido", MaxLength = 100 };
        private readonly TextBox email = new TextBox { Name = "txtEmail", MaxLength = 200 };
        private readonly TextBox telefono = new TextBox { Name = "txtTelefono", MaxLength = 50 };
        private readonly TextBox preferencias = new TextBox { Name = "txtPreferencias", MaxLength = 250 };
        private readonly CheckBox marketing = new CheckBox { Name = "chkMarketing", Text = "Autoriza campañas de marketing", AutoSize = true };
        private readonly CheckBox membresia = new CheckBox { Name = "chkMembresia", Text = "Crear membresía", Checked = true, AutoSize = true };
        private readonly NumericUpDown descuento = new NumericUpDown { Name = "nudDescuento", Minimum = 0, Maximum = 30, Value = 10 };
        private readonly Label estado = new Label { Name = "lblEstadoAltaCliente", AutoSize = true, ForeColor = Color.Firebrick, Dock = DockStyle.Top, Padding = new Padding(12, 5, 12, 5) };
        public FormAltaCliente(string correo = "")
        {
            ExigirPermiso(); Text = "Nuevo cliente"; StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(590, 460); MinimumSize = new Size(540, 460); AutoScaleMode = AutoScaleMode.Font; MinimizeBox = false; MaximizeBox = false;
            email.Text = correo;
            var campos = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(16) };
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160)); campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            string[] etiquetas = { "Nombre *", "Apellido *", "Correo electrónico *", "Teléfono", "Preferencias olfativas", "Marketing", "Membresía", "Descuento (%)" };
            Control[] controles = { nombre, apellido, email, telefono, preferencias, marketing, membresia, descuento };
            for (int i = 0; i < controles.Length; i++) {
                campos.RowCount++; campos.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                campos.Controls.Add(new Label { Text = etiquetas[i], AutoSize = true, Margin = new Padding(0, 9, 6, 9) }, 0, i);
                controles[i].Dock = DockStyle.Fill; controles[i].Margin = new Padding(0, 7, 0, 7); campos.Controls.Add(controles[i], 1, i);
            }
            membresia.CheckedChanged += delegate { descuento.Enabled = membresia.Checked; };
            // Constrain the text to the input column so larger fonts wrap instead of clipping.
            campos.SizeChanged += delegate {
                int ancho = Math.Max(100, campos.ClientSize.Width - (int)campos.ColumnStyles[0].Width - campos.Padding.Horizontal - 8);
                marketing.MaximumSize = membresia.MaximumSize = new Size(ancho, 0);
            };
            var cuerpo = new Panel { Dock = DockStyle.Fill, AutoScroll = true }; cuerpo.Controls.Add(campos);
            var pie = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(12) };
            var guardar = new Button { Name = "btnRegistrarCliente", Text = "Guardar y seleccionar", AutoSize = true, Padding = new Padding(5) };
            var cancelar = new Button { Name = "btnCancelarCliente", Text = "Cancelar", AutoSize = true, Padding = new Padding(5), DialogResult = DialogResult.Cancel };
            guardar.Click += delegate { Guardar(); }; pie.Controls.Add(guardar); pie.Controls.Add(cancelar);
            Controls.Add(cuerpo); Controls.Add(estado); Controls.Add(pie); AcceptButton = guardar; CancelButton = cancelar;
        }
        private static void ExigirPermiso() {
            if (Session.Instance.Usuario == null || !Session.Instance.Usuario.TienePermiso(TipoPermiso.Ventas_Registrar))
                throw new UnauthorizedAccessException("Se requiere una sesión con permiso de Ventas para registrar clientes.");
        }
        private void Guardar() {
            try {
                ExigirPermiso(); var cliente = new Cliente { Nombre = nombre.Text.Trim(), Apellido = apellido.Text.Trim(), Email = email.Text.Trim(),
                    Telefono = telefono.Text.Trim(), PreferenciasOlfativas = preferencias.Text.Trim(), AutorizaMarketing = marketing.Checked };
                new VentaBLL().RegistrarCliente(cliente, membresia.Checked, descuento.Value);
                ClienteCreado = cliente; DialogResult = DialogResult.OK; Close();
            } catch (Exception ex) { estado.Text = ex.Message; }
        }
    }
}
