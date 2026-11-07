using System;
using System.Drawing;
using System.Windows.Forms;
using BE;

namespace UI
{
    // Read-only preview; persistence happens only after explicit confirmation by the caller.
    public sealed class FormRestaurarUsuario : Form
    {
        public int CantidadCambios { get; private set; }
        public FormRestaurarUsuario(UsuarioAuditoria version, Usuario actual)
        {
            Text = "Revisar restauración de usuario"; StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.Font; ClientSize = new Size(780, 460); MinimumSize = new Size(620, 420);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(12) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(new Label { AutoSize = true, Text = "Usuario: " + version.Email + "\nVersión del " + version.AuditDate.ToString("dd/MM/yyyy HH:mm:ss") + " · " + (actual == null ? "Cuenta eliminada" : "Cuenta existente"), Padding = new Padding(0, 0, 0, 10) }, 0, 0);
            var grilla = new DataGridView { Name = "dgvCambiosRestauracion", Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false };
            grilla.Columns.Add("Campo", "Dato"); grilla.Columns.Add("Actual", "Valor actual"); grilla.Columns.Add("Restaurado", "Valor que quedará");
            Action<string, string, string> cambio = delegate(string campo, string antes, string despues) {
                if (actual == null || antes != despues) { grilla.Rows.Add(campo, actual == null ? "Cuenta inexistente" : antes, despues); CantidadCambios++; }
            };
            cambio("DNI", actual == null ? null : actual.Dni, version.Dni);
            cambio("Nombre", actual == null ? null : actual.Nombre, version.Nombre);
            cambio("Apellido", actual == null ? null : actual.Apellido, version.Apellido);
            cambio("Correo", actual == null ? null : actual.Email, version.Email);
            cambio("Estado", actual == null ? null : (actual.Activo ? "Activo" : "Inactivo"), version.Activo ? "Activo" : "Inactivo");
            if (actual == null || actual.Password != version.Password) {
                grilla.Rows.Add("Contraseña", actual == null ? "Cuenta inexistente" : "Credencial actual", "Credencial de la versión elegida (oculta)"); CantidadCambios++;
            }
            if (actual == null) {
                cambio("Fecha de alta", null, version.FechaAlta.ToString("dd/MM/yyyy HH:mm"));
                cambio("Intentos fallidos", null, version.IntentosFallidos.ToString());
            }
            layout.Controls.Add(grilla, 0, 1);
            var aviso = new Label { Name = "lblAlcanceRestauracion", AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 10),
                Text = CantidadCambios == 0 ? "No hay diferencias que restaurar." : "Se restaurarán únicamente los datos mostrados. Los roles y las operaciones comerciales no cambian." };
            layout.Controls.Add(aviso, 0, 2);
            var pie = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
            var confirmar = new Button { Name = "btnConfirmarRestauracion", Text = "Confirmar restauración", AutoSize = true, Padding = new Padding(6), DialogResult = DialogResult.OK, Enabled = CantidadCambios > 0 };
            var cancelar = new Button { Name = "btnCancelarRestauracion", Text = "Cancelar", AutoSize = true, Padding = new Padding(6), DialogResult = DialogResult.Cancel };
            pie.Controls.Add(confirmar); pie.Controls.Add(cancelar); layout.Controls.Add(pie, 0, 3); Controls.Add(layout);
            CancelButton = cancelar; // Enter must not accidentally commit a restore.
        }
    }
}
