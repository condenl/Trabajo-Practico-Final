using System;
using System.Collections.Generic;
using System.Linq;
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

        public UC_AuditoriaUsuarios()
        {
            InitializeComponent();
            auditoriaBLL = new AuditoriaBLL();
            traduccionBLL = new TraduccionBLL();
            this.Load += UC_AuditoriaUsuarios_Load;
        }

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
                MostrarEnGrilla(historialCompleto);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la auditoría: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarEnGrilla(List<UsuarioAuditoria> lista)
        {
            dgvHistorial.DataSource = null;
            dgvHistorial.DataSource = lista;

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
            if (dgvHistorial.CurrentRow == null)
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_seleccione_registro_auditoria") ?? "Seleccione un registro histórico de la grilla para restaurar.",
                                traduccionBLL.Traducir("lbl_warning") ?? "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UsuarioAuditoria snapshot = (UsuarioAuditoria)dgvHistorial.CurrentRow.DataBoundItem;

            var confirm = MessageBox.Show(
                $"¿Está seguro que desea revertir los datos del usuario {snapshot.Email} al estado exacto que tenía el {snapshot.AuditDate:dd/MM/yyyy HH:mm:ss}?\n\nAcción original que se revertirá: {snapshot.AuditAction}",
                "Confirmar Rollback",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    auditoriaBLL.HacerRollback(snapshot);

                    MessageBox.Show("El Rollback se ejecutó correctamente y los Dígitos Verificadores (DVH/DVV) fueron recalculados.",
                                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    CargarHistorial();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error crítico durante el Rollback: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}