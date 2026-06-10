using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BE;
using BE.Interfaces;
using BE.Seguridad;
using BLL;
using Servicios;

namespace UI.UserControls
{
    public partial class UC_Roles : UserControl, IIdiomaObserver
    {
        private SeguridadBLL seguridadBLL;
        private Componente rolSeleccionado;
        private TraduccionBLL traduccionBLL = new TraduccionBLL();

        public UC_Roles()
        {
            InitializeComponent();
            seguridadBLL = new SeguridadBLL();
            this.Load += UC_Roles_Load;
            lbRoles.SelectedIndexChanged += lbRoles_SelectedIndexChanged;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GestorIdioma.Instancia.Desuscribir(this);
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        public void ActualizarIdioma(Idioma idioma)
        {
            ActualizarTextos();
        }

        private void ActualizarTextos()
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

        private void UC_Roles_Load(object sender, EventArgs e)
        {
            GestorIdioma.Instancia.Suscribir(this);
            ActualizarTextos();
            CargarListaRoles();
            CargarComboPatentes();
        }

        private void CargarListaRoles()
        {
            try
            {
                lbRoles.DataSource = null;
                lbRoles.DataSource = seguridadBLL.ObtenerRolesDisponibles();
                lbRoles.DisplayMember = "Nombre";
                lbRoles.ValueMember = "Id";
                lbRoles.SelectedIndex = -1;
                LimpiarDetalleRol();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{traduccionBLL.Traducir("msg_error_loading_roles")}: {ex.Message}", traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarComboPatentes()
        {
            try
            {
                cmbPermisosDisponibles.DataSource = null;
                cmbPermisosDisponibles.DataSource = seguridadBLL.ObtenerPatentesDisponibles();
                cmbPermisosDisponibles.DisplayMember = "Nombre";
                cmbPermisosDisponibles.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{traduccionBLL.Traducir("msg_error_loading_permissions")}: {ex.Message}", traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lbRoles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbRoles.SelectedItem != null)
            {
                rolSeleccionado = (Componente)lbRoles.SelectedItem;
                txtNombreRol.Text = rolSeleccionado.Nombre;
                CargarPermisosDelRolSeleccionado();
            }
        }

        private void CargarPermisosDelRolSeleccionado()
        {
            if (rolSeleccionado == null) return;

            try
            {
                lbPermisosAsignados.DataSource = null;
                lbPermisosAsignados.DataSource = seguridadBLL.ObtenerPermisosDeRol(rolSeleccionado.Id);
                lbPermisosAsignados.DisplayMember = "Nombre";
                lbPermisosAsignados.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{traduccionBLL.Traducir("msg_error_loading_role_permissions")}: {ex.Message}", traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarDetalleRol()
        {
            rolSeleccionado = null;
            txtNombreRol.Clear();
            lbPermisosAsignados.DataSource = null;
        }

        private void btnCrearRol_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombreRol.Text))
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_enter_role_name"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            seguridadBLL.CrearRol(txtNombreRol.Text.Trim());
            MessageBox.Show(traduccionBLL.Traducir("msg_role_created_success"), traduccionBLL.Traducir("lbl_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            CargarListaRoles();
        }

        private void btnModificarRol_Click(object sender, EventArgs e)
        {
            if (rolSeleccionado == null) return;

            if (string.IsNullOrWhiteSpace(txtNombreRol.Text))
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_name_empty_warn"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            seguridadBLL.ModificarRol(rolSeleccionado.Id, txtNombreRol.Text.Trim());
            MessageBox.Show(traduccionBLL.Traducir("msg_role_modified_success"), traduccionBLL.Traducir("lbl_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            CargarListaRoles();
        }

        private void btnEliminarRol_Click(object sender, EventArgs e)
        {
            if (rolSeleccionado == null) return;

            string confirmMsg = string.Format(traduccionBLL.Traducir("msg_confirm_delete_role"), rolSeleccionado.Nombre);
            var confirm = MessageBox.Show(confirmMsg, traduccionBLL.Traducir("msg_confirm_delete_role_title"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
            {
                seguridadBLL.EliminarRol(rolSeleccionado.Id);
                MessageBox.Show(traduccionBLL.Traducir("msg_role_deleted_success"), traduccionBLL.Traducir("lbl_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarListaRoles();
            }
        }

        private void btnAgregarPermiso_Click(object sender, EventArgs e)
        {
            if (rolSeleccionado == null || cmbPermisosDisponibles.SelectedItem == null)
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_select_role_permission_warn"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Componente permiso = (Componente)cmbPermisosDisponibles.SelectedItem;
            seguridadBLL.AsignarPermisoARol(rolSeleccionado, permiso);
            CargarPermisosDelRolSeleccionado();
        }

        private void btnQuitarPermiso_Click(object sender, EventArgs e)
        {
            if (rolSeleccionado == null || lbPermisosAsignados.SelectedItem == null)
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_select_assigned_permission_warn"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Componente permisoAQuitar = (Componente)lbPermisosAsignados.SelectedItem;
            seguridadBLL.QuitarPermisoDeRol(rolSeleccionado, permisoAQuitar);
            CargarPermisosDelRolSeleccionado();
        }
    }
}
