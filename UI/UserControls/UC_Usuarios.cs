using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL;
using Servicios;
using BE.Seguridad;
using BE.Interfaces;

namespace UI.UserControls
{
    public partial class UC_Usuarios : UserControl, IIdiomaObserver
    {
        private UsuarioBLL usuarioBLL;
        private SeguridadBLL seguridadBLL;
        private TraduccionBLL traduccionBLL;
        private Usuario usuarioSeleccionado;

        public UC_Usuarios()
        {
            InitializeComponent();
            usuarioBLL = new UsuarioBLL();
            seguridadBLL = new SeguridadBLL();
            traduccionBLL = new TraduccionBLL();

            if (txtContrasenia != null)
                txtContrasenia.UseSystemPasswordChar = true;

            this.Load += UC_Usuarios_Load;
            dgvUsuarios.CellFormatting += DgvUsuarios_CellFormatting;
        }

        private void UC_Usuarios_Load(object sender, EventArgs e)
        {
            GestorIdioma.Instancia.Suscribir(this);
            ActualizarTextos();
            CargarComboRoles();
            CargarGrilla();
            AplicarPermisosDeSeguridad();
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

        private void AplicarPermisosDeSeguridad()
        {
            Usuario logueado = Session.Instance.Usuario;

            if (logueado == null) return;

            btnAgregar.Enabled = logueado.TienePermiso(TipoPermiso.Usuarios_Alta);
            btnModificar.Enabled = logueado.TienePermiso(TipoPermiso.Usuarios_Modificar);

            btnAlta.Enabled = logueado.TienePermiso(TipoPermiso.Usuarios_Baja);
            btnBaja.Enabled = logueado.TienePermiso(TipoPermiso.Usuarios_Baja);

            if (cmbRoles != null)
            {
                cmbRoles.Enabled = logueado.TienePermiso(TipoPermiso.Roles_Gestion);
            }

            if (!btnAgregar.Enabled && !btnModificar.Enabled)
            {
                txtDni.ReadOnly = true;
                txtNombre.ReadOnly = true;
                txtApellido.ReadOnly = true;
                txtEmail.ReadOnly = true;
                txtContrasenia.ReadOnly = true;
            }
            else
            {
                txtDni.ReadOnly = false;
                txtNombre.ReadOnly = false;
                txtApellido.ReadOnly = false;
                txtEmail.ReadOnly = false;
                txtContrasenia.ReadOnly = false;
            }
        }

        private void CargarComboRoles()
        {
            try
            {
                if (cmbRoles == null) return;

                List<Componente> roles = seguridadBLL.ObtenerRolesDisponibles();
                cmbRoles.DataSource = roles;
                cmbRoles.DisplayMember = "Nombre";
                cmbRoles.ValueMember = "Id";
                cmbRoles.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_error_loading_roles") + ": " + ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarGrilla()
        {
            try
            {
                var listaUsuarios = usuarioBLL.ListarTodos();

                foreach (var u in listaUsuarios)
                {
                    seguridadBLL.CargarEstructuraPermisos(u);
                }

                dgvUsuarios.DataSource = null;
                dgvUsuarios.DataSource = listaUsuarios;

                if (dgvUsuarios.Columns.Contains("Password")) dgvUsuarios.Columns["Password"].Visible = false;
                if (dgvUsuarios.Columns.Contains("Id")) dgvUsuarios.Columns["Id"].Visible = false;
                if (dgvUsuarios.Columns.Contains("DVH")) dgvUsuarios.Columns["DVH"].Visible = false;
                if (dgvUsuarios.Columns.Contains("Permisos")) dgvUsuarios.Columns["Permisos"].Visible = false;
                if (dgvUsuarios.Columns.Contains("IntentosFallidos")) dgvUsuarios.Columns["IntentosFallidos"].Visible = false;

                if (!dgvUsuarios.Columns.Contains("ColRol"))
                {
                    DataGridViewTextBoxColumn colRol = new DataGridViewTextBoxColumn();
                    colRol.Name = "ColRol";
                    colRol.HeaderText = traduccionBLL.Traducir("grid_assigned_role");
                    dgvUsuarios.Columns.Add(colRol);
                }
                else
                {
                    dgvUsuarios.Columns["ColRol"].HeaderText = traduccionBLL.Traducir("grid_assigned_role");
                }

                if (dgvUsuarios.Columns.Contains("Dni")) dgvUsuarios.Columns["Dni"].HeaderText = traduccionBLL.Traducir("lbl_dni");
                if (dgvUsuarios.Columns.Contains("Nombre")) dgvUsuarios.Columns["Nombre"].HeaderText = traduccionBLL.Traducir("lbl_nombre");
                if (dgvUsuarios.Columns.Contains("Apellido")) dgvUsuarios.Columns["Apellido"].HeaderText = traduccionBLL.Traducir("lbl_apellido");
                if (dgvUsuarios.Columns.Contains("Email")) dgvUsuarios.Columns["Email"].HeaderText = traduccionBLL.Traducir("lbl_email");
                if (dgvUsuarios.Columns.Contains("Activo")) dgvUsuarios.Columns["Activo"].HeaderText = traduccionBLL.Traducir("lbl_active");

                dgvUsuarios.Columns["ColRol"].DisplayIndex = dgvUsuarios.Columns.Count - 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_error_loading_grid") + ": " + ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvUsuarios_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvUsuarios.Columns[e.ColumnIndex].Name == "ColRol" && e.RowIndex >= 0)
            {
                Usuario u = (Usuario)dgvUsuarios.Rows[e.RowIndex].DataBoundItem;

                var rol = u.Permisos?.FirstOrDefault(p => p is Familia);
                e.Value = rol != null ? rol.Nombre : traduccionBLL.Traducir("lbl_no_role");
            }
        }

        private void dgvUsuarios_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dgvUsuarios.SelectedRows.Count > 0)
                {
                    usuarioSeleccionado = (Usuario)dgvUsuarios.SelectedRows[0].DataBoundItem;

                    try
                    {
                        txtDni.Text = Servicios.Encriptador.DesencriptarAES(usuarioSeleccionado.Dni);
                    }
                    catch (System.Security.Cryptography.CryptographicException)
                    {
                        txtDni.Text = usuarioSeleccionado.Dni;
                    }

                    txtNombre.Text = usuarioSeleccionado.Nombre;
                    txtApellido.Text = usuarioSeleccionado.Apellido;
                    txtEmail.Text = usuarioSeleccionado.Email;
                    txtContrasenia.Clear();

                    if (cmbRoles != null)
                    {
                        var rolActual = usuarioSeleccionado.Permisos?.FirstOrDefault(p => p is Familia);
                        if (rolActual != null)
                        {
                            cmbRoles.SelectedValue = rolActual.Id;
                        }
                        else
                        {
                            cmbRoles.SelectedIndex = -1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_error_selecting_user") + ": " + ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidarCamposUI(requierePassword: true)) return;

                Usuario nuevoUsuario = new Usuario
                {
                    Dni = txtDni.Text.Trim(),
                    Nombre = txtNombre.Text.Trim(),
                    Apellido = txtApellido.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Activo = true
                };

                usuarioBLL.Insertar(nuevoUsuario, txtContrasenia.Text);

                if (cmbRoles != null && cmbRoles.SelectedItem != null)
                {
                    Usuario usuarioInsertado = usuarioBLL.BuscarPorEmail(nuevoUsuario.Email);
                    Componente rolSeleccionado = (Componente)cmbRoles.SelectedItem;
                    seguridadBLL.AsignarRolAUsuario(usuarioInsertado, rolSeleccionado);
                }

                MessageBox.Show(traduccionBLL.Traducir("msg_user_added_success"), traduccionBLL.Traducir("lbl_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarYRecargar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            try
            {
                if (usuarioSeleccionado == null)
                {
                    MessageBox.Show(traduccionBLL.Traducir("msg_select_user_warn"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!ValidarCamposUI(requierePassword: false)) return;

                usuarioSeleccionado.Dni = txtDni.Text.Trim();
                usuarioSeleccionado.Nombre = txtNombre.Text.Trim();
                usuarioSeleccionado.Apellido = txtApellido.Text.Trim();
                usuarioSeleccionado.Email = txtEmail.Text.Trim();

                usuarioBLL.Modificar(usuarioSeleccionado, txtContrasenia.Text);

                if (cmbRoles != null && cmbRoles.SelectedItem != null)
                {
                    Componente rolSeleccionado = (Componente)cmbRoles.SelectedItem;
                    seguridadBLL.AsignarRolAUsuario(usuarioSeleccionado, rolSeleccionado);
                }

                MessageBox.Show(traduccionBLL.Traducir("msg_user_modified_success"), traduccionBLL.Traducir("lbl_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarYRecargar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBaja_Click(object sender, EventArgs e)
        {
            CambiarEstadoUsuario(false, "dado de baja");
        }

        private void btnAlta_Click(object sender, EventArgs e)
        {
            CambiarEstadoUsuario(true, "dado de alta");
        }

        private void CambiarEstadoUsuario(bool nuevoEstado, string accion)
        {
            try
            {
                if (nuevoEstado == false && usuarioSeleccionado.Email == Session.Instance.Usuario.Email)
                {
                    MessageBox.Show(traduccionBLL.Traducir("msg_self_deactivate_stop"), traduccionBLL.Traducir("lbl_denied"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }

                if (usuarioSeleccionado == null)
                {
                    MessageBox.Show(traduccionBLL.Traducir("msg_select_user_warn"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (usuarioSeleccionado.Activo == nuevoEstado)
                {
                    MessageBox.Show(traduccionBLL.Traducir("msg_user_already_status"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                usuarioSeleccionado.Activo = nuevoEstado;
                usuarioBLL.Modificar(usuarioSeleccionado, "");

                MessageBox.Show(traduccionBLL.Traducir("msg_user_status_changed_success"), traduccionBLL.Traducir("lbl_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarYRecargar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_error_changing_status") + ": " + ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarYRecargar();
        }

        private void LimpiarYRecargar()
        {
            txtDni.Clear();
            txtNombre.Clear();
            txtApellido.Clear();
            txtEmail.Clear();
            txtContrasenia.Clear();

            if (cmbRoles != null) cmbRoles.SelectedIndex = -1;

            usuarioSeleccionado = null;
            CargarGrilla();
        }

        private bool ValidarCamposUI(bool requierePassword)
        {
            if (string.IsNullOrWhiteSpace(txtDni.Text) ||
                string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellido.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_complete_all_text_fields"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (requierePassword && string.IsNullOrWhiteSpace(txtContrasenia.Text))
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_password_required"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!txtDni.Text.All(char.IsDigit))
            {
                MessageBox.Show(traduccionBLL.Traducir("msg_dni_digits_only"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}
