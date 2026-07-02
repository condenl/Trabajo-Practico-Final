using System;
using System.Collections.Generic;
using System.Linq;
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

            tvRoles.AfterSelect += tvRoles_AfterSelect;
            tvRoles.MouseDown += tvRoles_MouseDown; 
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
            CargarArbolRoles();
            CargarComboPatentes();
        }

        private void CargarArbolRoles()
        {
            try
            {
                tvRoles.Nodes.Clear();
                var todosLosRoles = seguridadBLL.ObtenerRolesDisponibles();

                HashSet<int> idsHijos = new HashSet<int>();
                foreach (var r in todosLosRoles)
                {
                    var subRoles = seguridadBLL.ObtenerPermisosDeRol(r.Id).Where(c => c is Familia);
                    foreach (var sub in subRoles) idsHijos.Add(sub.Id);
                }

                foreach (var rol in todosLosRoles.Where(r => !idsHijos.Contains(r.Id)))
                {
                    TreeNode nodoRaiz = new TreeNode(rol.Nombre);
                    nodoRaiz.Tag = rol;

                    CargarRolesHijos(nodoRaiz, rol);

                    tvRoles.Nodes.Add(nodoRaiz);
                }
                tvRoles.ExpandAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{traduccionBLL.Traducir("msg_error_loading_roles")}: {ex.Message}", traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarRolesHijos(TreeNode nodoPadre, Componente rolPadre)
        {
            var hijos = seguridadBLL.ObtenerPermisosDeRol(rolPadre.Id);

            foreach (var hijo in hijos)
            {
                if (hijo is Familia)
                {
                    TreeNode nodoHijo = new TreeNode(hijo.Nombre);
                    nodoHijo.Tag = hijo;
                    CargarRolesHijos(nodoHijo, hijo);
                    nodoPadre.Nodes.Add(nodoHijo);
                }
            }
        }

        private void tvRoles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tvRoles.SelectedNode != null)
            {
                rolSeleccionado = (Componente)tvRoles.SelectedNode.Tag;
                txtNombreRol.Text = rolSeleccionado.Nombre;
                CargarPermisosDelRolSeleccionado();
            }
        }

        private void tvRoles_MouseDown(object sender, MouseEventArgs e)
        {
            if (tvRoles.HitTest(e.X, e.Y).Node == null)
            {
                tvRoles.SelectedNode = null;
                LimpiarDetalleRol();
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
                cmbPermisosDisponibles.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar componentes: " + ex.Message);
            }
        }

        private void CargarPermisosDelRolSeleccionado()
        {
            if (rolSeleccionado == null) return;

            try
            {
                Familia rolEnsamblado = seguridadBLL.ObtenerFamiliaCompleta(rolSeleccionado.Id);

                if (rolEnsamblado != null)
                {
                    var permisosAplanados = rolEnsamblado.ObtenerPatentes();

                    lbPermisosAsignados.DataSource = null;
                    lbPermisosAsignados.DataSource = permisosAplanados;
                    lbPermisosAsignados.DisplayMember = "Nombre";
                    lbPermisosAsignados.ValueMember = "Id";
                }
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
            string nombreNuevoRol = txtNombreRol.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombreNuevoRol))
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_enter_role_name"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                seguridadBLL.CrearRol(nombreNuevoRol);

                if (rolSeleccionado != null)
                {
                    var rolRecienCreado = seguridadBLL.ObtenerRolesDisponibles().FirstOrDefault(r => r.Nombre == nombreNuevoRol);
                    if (rolRecienCreado != null)
                    {
                        seguridadBLL.AsignarPermisoARol(rolSeleccionado, rolRecienCreado);
                    }
                }

                MessageBox.Show(traduccionBLL.Traducir("msg_role_created_success"), traduccionBLL.Traducir("lbl_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarArbolRoles();
                LimpiarDetalleRol();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
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
            CargarArbolRoles();
        }

        private void btnEliminarRol_Click(object sender, EventArgs e)
        {
            if (rolSeleccionado == null) return;

            string confirmMsg = string.Format(traduccionBLL.Traducir("msg_confirm_delete_role"), rolSeleccionado.Nombre);
            var confirm = MessageBox.Show(confirmMsg, traduccionBLL.Traducir("msg_confirm_delete_role_title"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                if (tvRoles.SelectedNode.Parent != null)
                {
                    Componente rolPadre = (Componente)tvRoles.SelectedNode.Parent.Tag;
                    seguridadBLL.QuitarPermisoDeRol(rolPadre, rolSeleccionado);
                }
                else
                {
                    seguridadBLL.EliminarRol(rolSeleccionado.Id);
                }

                MessageBox.Show(traduccionBLL.Traducir("msg_role_deleted_success"), traduccionBLL.Traducir("lbl_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarDetalleRol();
                CargarArbolRoles();
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

            try
            {
                seguridadBLL.AsignarPermisoARol(rolSeleccionado, permiso);
                CargarPermisosDelRolSeleccionado();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
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