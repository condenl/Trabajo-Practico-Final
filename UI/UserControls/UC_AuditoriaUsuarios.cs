using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using BE;
using BE.Interfaces;
using BLL;
using Servicios;

namespace UI.UserControls
{
    public partial class UC_AuditoriaUsuarios : UserControl, IIdiomaObserver
    {
        private AuditoriaBLL auditoriaBLL;
        private TraduccionBLL traduccionBLL;
        private List<UsuarioAuditoria> historialCompleto;
        private readonly Label aviso = new Label { Name = "lblEstadoAuditoria", AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(3, 8, 3, 8) };

        public UC_AuditoriaUsuarios()
        {
            InitializeComponent();
            BindingContext = new BindingContext();
            auditoriaBLL = new AuditoriaBLL();
            traduccionBLL = new TraduccionBLL();
            ConfigurarInterfaz();
            this.Load += UC_AuditoriaUsuarios_Load;
        }

        private void ConfigurarInterfaz()
        {
            Controls.Clear(); Dock = DockStyle.Fill;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(12) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var barra = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            label3.Tag = null; label3.Text = "Buscar usuario, responsable o acción"; label3.Margin = new Padding(3, 10, 3, 3); txtBuscar.Width = 220;
            barra.Controls.Add(label3); barra.Controls.Add(txtBuscar);
            btnRollback.Tag = null; btnRollback.Text = "Revisar restauración...";
            foreach (var b in new[] { btnBuscar, btnRollback }) { b.AutoSize = true; b.Margin = new Padding(3); b.Padding = new Padding(5); barra.Controls.Add(b); }
            var actualizar = new Button { Name = "btnActualizarAuditoria", Text = "Actualizar", AutoSize = true, Padding = new Padding(5) };
            actualizar.Click += delegate { CargarHistorial(); }; barra.Controls.Add(actualizar);
            layout.Controls.Add(barra, 0, 0); layout.Controls.Add(aviso, 0, 1); dgvHistorial.Dock = DockStyle.Fill;
            layout.Controls.Add(dgvHistorial, 0, 2); Controls.Add(layout);
            dgvHistorial.CurrentCellChanged += delegate {
                var version = dgvHistorial.CurrentRow == null ? null : dgvHistorial.CurrentRow.DataBoundItem as UsuarioAuditoria;
                btnRollback.Enabled = version != null && (version.AuditAction == "UPDATE" || version.AuditAction == "DELETE");
            };
            btnRollback.Enabled = false;
            txtBuscar.KeyDown += delegate(object s, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; btnBuscar_Click(s, EventArgs.Empty); } };
        }

        private void Informar(string texto, bool error = false) { aviso.Text = texto; aviso.ForeColor = error ? Color.Firebrick : SystemColors.ControlText; }

        public void ActualizarIdioma(Idioma idioma)
        {
            ActualizarTextosRecursivo(this.Controls);
        }

        private void ActualizarTextosRecursivo(Control.ControlCollection controles)
        {
            foreach (Control control in controles)
            {
                if (control.Tag != null && !string.IsNullOrWhiteSpace(control.Tag.ToString()))
                {
                    control.Text = traduccionBLL.Traducir(control.Tag.ToString());
                }
                if (control.HasChildren)
                {
                    ActualizarTextosRecursivo(control.Controls);
                }
            }
        }

        private void UC_AuditoriaUsuarios_Load(object sender, EventArgs e)
        {
            GestorIdioma.Instancia.Suscribir(this);
            ActualizarTextosRecursivo(this.Controls);
            CargarHistorial();
        }

        private void CargarHistorial()
        {
            try
            {
                historialCompleto = auditoriaBLL.ListarHistorial();
                btnBuscar_Click(this, EventArgs.Empty);
                Informar("Seleccione una versión y revise los cambios antes de restaurarla.");
            }
            catch (Exception ex)
            {
                Informar("No se pudo cargar la auditoría: " + ex.Message, true);
            }
        }

        private void MostrarEnGrilla(List<UsuarioAuditoria> lista)
        {
            dgvHistorial.DataSource = null;
            dgvHistorial.DataSource = lista;
            foreach (DataGridViewColumn columna in dgvHistorial.Columns)
                columna.Visible = new[] { "AuditDate", "AuditAction", "UsuarioResponsable", "Email" }.Contains(columna.Name);
            dgvHistorial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHistorial.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvHistorial.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvHistorial.ClearSelection(); dgvHistorial.CurrentCell = null; btnRollback.Enabled = false;

            if (dgvHistorial.Columns.Contains("Password")) dgvHistorial.Columns["Password"].Visible = false;
            if (dgvHistorial.Columns.Contains("DVH")) dgvHistorial.Columns["DVH"].Visible = false;
            if (dgvHistorial.Columns.Contains("Id")) dgvHistorial.Columns["Id"].Visible = false;
            if (dgvHistorial.Columns.Contains("AuditId")) dgvHistorial.Columns["AuditId"].Visible = false;
            if (dgvHistorial.Columns.Contains("TipoEntidad")) dgvHistorial.Columns["TipoEntidad"].Visible = false;
            if (dgvHistorial.Columns.Contains("NombreCompleto")) dgvHistorial.Columns["NombreCompleto"].Visible = false;

            if (dgvHistorial.Columns.Contains("AuditDate"))
            {
                dgvHistorial.Columns["AuditDate"].HeaderText = "Fecha de Alteración";
                dgvHistorial.Columns["AuditDate"].DisplayIndex = 0;
                dgvHistorial.Columns["AuditDate"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            }
            if (dgvHistorial.Columns.Contains("AuditAction"))
            {
                dgvHistorial.Columns["AuditAction"].HeaderText = "Acción";
                dgvHistorial.Columns["AuditAction"].DisplayIndex = 1;
            }
            if (dgvHistorial.Columns.Contains("UsuarioResponsable"))
            {
                dgvHistorial.Columns["UsuarioResponsable"].HeaderText = "Admin Responsable";
                dgvHistorial.Columns["UsuarioResponsable"].DisplayIndex = 2;
            }
            if (dgvHistorial.Columns.Contains("Email"))
            {
                dgvHistorial.Columns["Email"].HeaderText = "Usuario Afectado";
                dgvHistorial.Columns["Email"].DisplayIndex = 3;
            }
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            if (historialCompleto == null) return;

            string filtro = txtBuscar.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(filtro))
            {
                MostrarEnGrilla(historialCompleto);
            }
            else
            {
                var filtrados = historialCompleto.Where(u =>
                    (u.Email != null && u.Email.ToLower().Contains(filtro)) ||
                    (u.UsuarioResponsable != null && u.UsuarioResponsable.ToLower().Contains(filtro)) ||
                    (u.AuditAction != null && u.AuditAction.ToLower().Contains(filtro))
                ).ToList();

                MostrarEnGrilla(filtrados);
            }
        }

        private void btnRollback_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvHistorial.CurrentRow == null || !btnRollback.Enabled) { Informar("Seleccione una versión restaurable del listado.", true); return; }
                var snapshot = (UsuarioAuditoria)dgvHistorial.CurrentRow.DataBoundItem;
                var actual = new UsuarioBLL().ListarTodos().FirstOrDefault(u => u.Id == snapshot.Id);
                if (snapshot.AuditAction == "UPDATE" && actual == null) { Informar("La cuenta ya no existe. Seleccione su versión anterior a la eliminación.", true); return; }
                if (snapshot.AuditAction == "DELETE" && actual != null) { Informar("Esta cuenta ya existe. Actualice el historial.", true); return; }
                using (var revision = new FormRestaurarUsuario(snapshot, actual)) {
                    if (revision.ShowDialog(this) != DialogResult.OK) return;
                    var vigente = new UsuarioBLL().ListarTodos().FirstOrDefault(u => u.Id == snapshot.Id);
                    if ((actual == null) != (vigente == null) || (actual != null && actual.DVH != vigente.DVH)) {
                        Informar("El usuario cambió mientras revisaba la versión. Actualice y revise nuevamente.", true); return;
                    }
                    auditoriaBLL.HacerRollback(snapshot); CargarHistorial(); Informar("Se restauraron los datos de la versión elegida.");
                }
            }
            catch (Exception ex) { Informar("No se pudo restaurar el usuario: " + ex.Message, true); }
        }
    }
}
