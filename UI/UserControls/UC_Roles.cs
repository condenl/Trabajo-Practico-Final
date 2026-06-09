using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BE.Seguridad;
using BLL;

namespace UI.UserControls
{
    public partial class UC_Roles : UserControl
    {
        private SeguridadBLL seguridadBLL;
        private Componente rolSeleccionado;

        public UC_Roles()
        {
            InitializeComponent();
            seguridadBLL = new SeguridadBLL();
            this.Load += UC_Roles_Load;
            lbRoles.SelectedIndexChanged += lbRoles_SelectedIndexChanged;
        }

        private void UC_Roles_Load(object sender, EventArgs e)
        {
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
                MessageBox.Show("Error al cargar roles: " + ex.Message);
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
                MessageBox.Show("Error al cargar permisos: " + ex.Message);
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
                MessageBox.Show("Error al cargar permisos del rol: " + ex.Message);
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
                MessageBox.Show("Ingrese un nombre para el nuevo rol.");
                return;
            }

            seguridadBLL.CrearRol(txtNombreRol.Text.Trim());
            MessageBox.Show("Rol creado con éxito.");
            CargarListaRoles();
        }

        private void btnModificarRol_Click(object sender, EventArgs e)
        {
            if (rolSeleccionado == null) return;

            if (string.IsNullOrWhiteSpace(txtNombreRol.Text))
            {
                MessageBox.Show("El nombre no puede estar vacío.");
                return;
            }

            seguridadBLL.ModificarRol(rolSeleccionado.Id, txtNombreRol.Text.Trim());
            MessageBox.Show("Rol modificado.");
            CargarListaRoles();
        }

        private void btnEliminarRol_Click(object sender, EventArgs e)
        {
            if (rolSeleccionado == null) return;

            var confirm = MessageBox.Show($"¿Desea eliminar el rol '{rolSeleccionado.Nombre}'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
            {
                seguridadBLL.EliminarRol(rolSeleccionado.Id);
                MessageBox.Show("Rol eliminado.");
                CargarListaRoles();
            }
        }

        private void btnAgregarPermiso_Click(object sender, EventArgs e)
        {
            if (rolSeleccionado == null || cmbPermisosDisponibles.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un rol y un permiso disponible.");
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
                MessageBox.Show("Seleccione un permiso asignado para quitar.");
                return;
            }

            Componente permisoAQuitar = (Componente)lbPermisosAsignados.SelectedItem;
            seguridadBLL.QuitarPermisoDeRol(rolSeleccionado, permisoAQuitar);
            CargarPermisosDelRolSeleccionado();
        }
    }
}