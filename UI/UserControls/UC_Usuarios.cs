using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using BE;
using BLL;
using Servicios;
using BE.Seguridad;
using BE.Interfaces;

namespace UI.UserControls
{
    public partial class UC_Usuarios : UserControl, IIdiomaObserver, IVistaConCambiosPendientes
    {
        public bool TieneCambiosPendientes { get {
            if (!btnAgregar.Enabled && !btnModificar.Enabled) return false;
            if (usuarioSeleccionado == null) return new[] { txtDni, txtNombre, txtApellido, txtEmail, txtContrasenia }.Any(t => !string.IsNullOrWhiteSpace(t.Text)) || cmbRoles.SelectedIndex >= 0;
            var rol = usuarioSeleccionado.Permisos.FirstOrDefault(p => p is Familia);
            return txtDni.Text.Trim() != usuarioSeleccionado.Dni || txtNombre.Text.Trim() != usuarioSeleccionado.Nombre || txtApellido.Text.Trim() != usuarioSeleccionado.Apellido ||
                txtEmail.Text.Trim() != usuarioSeleccionado.Email || txtContrasenia.Text.Length > 0 || (cmbRoles.Enabled && (cmbRoles.SelectedValue as int?) != (rol == null ? (int?)null : rol.Id));
        } }
        private UsuarioBLL usuarioBLL;
        private SeguridadBLL seguridadBLL;
        private TraduccionBLL traduccionBLL;
        private Usuario usuarioSeleccionado;
        private readonly Label modo = new Label { Name = "lblModoUsuario", AutoSize = true, Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold), Margin = new Padding(3, 8, 3, 8) };
        private readonly Label aviso = new Label { Name = "lblEstadoUsuarios", AutoSize = true, Margin = new Padding(3, 8, 3, 8) };
        private Button nuevo;

        public UC_Usuarios()
        {
            InitializeComponent();
            BindingContext = new BindingContext();
            usuarioBLL = new UsuarioBLL();
            seguridadBLL = new SeguridadBLL();
            traduccionBLL = new TraduccionBLL();

            if (txtContrasenia != null)
                txtContrasenia.UseSystemPasswordChar = true;

            ConfigurarInterfaz();
            dgvUsuarios.CellFormatting += DgvUsuarios_CellFormatting;
            dgvUsuarios.KeyUp += delegate(object s, KeyEventArgs e) {
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Home || e.KeyCode == Keys.End || e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown)
                    dgvUsuarios_CellClick(s, new DataGridViewCellEventArgs(0, dgvUsuarios.CurrentRow == null ? -1 : dgvUsuarios.CurrentRow.Index));
            };
        }

        private void UC_Usuarios_Load(object sender, EventArgs e)
        {
            GestorIdioma.Instancia.Suscribir(this);
            ActualizarTextos();
            CargarComboRoles();
            CargarGrilla();
            AplicarPermisosDeSeguridad();
            LimpiarSeleccion();
        }

        private void ConfigurarInterfaz()
        {
            Controls.Clear(); Dock = DockStyle.Fill;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, ColumnCount = 1, RowCount = 5, Padding = new Padding(12) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 5; i++) layout.RowStyles.Add(new RowStyle(i == 4 ? SizeType.Percent : SizeType.AutoSize, i == 4 ? 100 : 0));
            layout.Controls.Add(modo, 0, 0);
            var campos = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 4 };
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100)); campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100)); campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            Control[] etiquetas = { label1, label5, label6, label7, label8, new Label { Text = "Rol", AutoSize = true } };
            Control[] entradas = { txtDni, txtNombre, txtApellido, txtEmail, txtContrasenia, cmbRoles };
            for (int i = 0; i < entradas.Length; i++) {
                etiquetas[i].Anchor = AnchorStyles.Left; etiquetas[i].Margin = new Padding(3, 8, 3, 8);
                entradas[i].Dock = DockStyle.Fill; entradas[i].Margin = new Padding(3, 6, 12, 6);
                campos.Controls.Add(etiquetas[i], (i % 2) * 2, i / 2); campos.Controls.Add(entradas[i], (i % 2) * 2 + 1, i / 2);
            }
            cmbRoles.DropDownStyle = ComboBoxStyle.DropDownList; layout.Controls.Add(campos, 0, 1);
            var acciones = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            nuevo = new Button { Name = "btnNuevoUsuario", Text = "Nuevo usuario", AutoSize = true, Padding = new Padding(5) };
            nuevo.Click += delegate { LimpiarSeleccion(); aviso.Text = "Complete los datos del nuevo usuario."; txtDni.Focus(); };
            acciones.Controls.Add(nuevo);
            foreach (var boton in new[] { btnAgregar, btnModificar, btnAlta, btnBaja }) { boton.AutoSize = true; boton.Margin = new Padding(3); boton.Padding = new Padding(5); acciones.Controls.Add(boton); }
            layout.Controls.Add(acciones, 0, 2); layout.Controls.Add(aviso, 0, 3);
            dgvUsuarios.Dock = DockStyle.Fill; dgvUsuarios.MultiSelect = false; dgvUsuarios.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUsuarios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; layout.Controls.Add(dgvUsuarios, 0, 4); Controls.Add(layout);
        }

        private void LimpiarSeleccion()
        {
            usuarioSeleccionado = null; foreach (var caja in new[] { txtDni, txtNombre, txtApellido, txtEmail, txtContrasenia }) caja.Clear();
            cmbRoles.SelectedIndex = -1; dgvUsuarios.ClearSelection(); dgvUsuarios.CurrentCell = null; AplicarPermisosDeSeguridad();
        }

        private void MostrarAviso(string mensaje, string titulo, MessageBoxButtons botones, MessageBoxIcon icono)
        {
            aviso.Text = mensaje; aviso.ForeColor = icono == MessageBoxIcon.Error || icono == MessageBoxIcon.Warning || icono == MessageBoxIcon.Stop ? Color.Firebrick : SystemColors.ControlText;
        }

        public void ActualizarIdioma(Idioma idioma)
        {
            ActualizarTextos();
        }

        private void ActualizarTextos()
        {
            ActualizarTextosRecursivo(this.Controls);
            AplicarPermisosDeSeguridad();
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

            bool edita = usuarioSeleccionado != null;
            modo.Text = edita ? "Editar usuario: " + usuarioSeleccionado.Email : "Nuevo usuario";
            btnAgregar.Text = "Guardar nuevo usuario"; btnModificar.Text = "Guardar cambios";
            label8.Text = edita ? "Nueva contraseña" : "Contraseña *";
            nuevo.Enabled = logueado != null && logueado.TienePermiso(TipoPermiso.Usuarios_Alta);
            btnAgregar.Enabled = !edita && nuevo.Enabled;
            btnModificar.Enabled = edita && logueado != null && logueado.TienePermiso(TipoPermiso.Usuarios_Modificar);
            btnAlta.Enabled = edita && !usuarioSeleccionado.Activo && logueado != null && logueado.TienePermiso(TipoPermiso.Usuarios_Baja);
            btnBaja.Enabled = edita && usuarioSeleccionado.Activo && logueado != null && logueado.TienePermiso(TipoPermiso.Usuarios_Baja) && usuarioSeleccionado.Id != logueado.Id;

            if (cmbRoles != null)
            {
                cmbRoles.Enabled = logueado != null && logueado.TienePermiso(TipoPermiso.Roles_Gestion) && (btnAgregar.Enabled || btnModificar.Enabled);
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
                MostrarAviso(traduccionBLL.Traducir("msg_error_loading_roles") + ": " + ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                if (dgvUsuarios.Columns.Contains("TipoEntidad")) dgvUsuarios.Columns["TipoEntidad"].Visible = false;
                if (dgvUsuarios.Columns.Contains("NombreCompleto")) dgvUsuarios.Columns["NombreCompleto"].Visible = false;
                if (dgvUsuarios.Columns.Contains("FechaAlta")) {
                    dgvUsuarios.Columns["FechaAlta"].HeaderText = "Fecha de alta";
                    dgvUsuarios.Columns["FechaAlta"].DefaultCellStyle.Format = "dd/MM/yyyy";
                }
                dgvUsuarios.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                dgvUsuarios.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

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
                MostrarAviso(traduccionBLL.Traducir("msg_error_loading_grid") + ": " + ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                    // The mapper already returns the decrypted DNI.
                    txtDni.Text = usuarioSeleccionado.Dni;

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
                    AplicarPermisosDeSeguridad();
                    aviso.Text = "Edite los datos y pulse Guardar cambios. Deje la contraseña vacía para conservarla.";
                }
            }
            catch (Exception ex)
            {
                MostrarAviso(traduccionBLL.Traducir("msg_error_selecting_user") + ": " + ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                if (usuarioSeleccionado != null || !btnAgregar.Enabled) return;
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

                MostrarAviso(traduccionBLL.Traducir("msg_user_added_success"), traduccionBLL.Traducir("lbl_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarYRecargar();
            }
            catch (Exception ex)
            {
                MostrarAviso(ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            try
            {
                if (usuarioSeleccionado == null)
                {
                    MostrarAviso(traduccionBLL.Traducir("msg_select_user_warn"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!ValidarCamposUI(requierePassword: false)) return;

                if (!btnModificar.Enabled) return;
                var editado = (Usuario)usuarioSeleccionado.Clone();
                editado.Dni = txtDni.Text.Trim(); editado.Nombre = txtNombre.Text.Trim();
                editado.Apellido = txtApellido.Text.Trim(); editado.Email = txtEmail.Text.Trim();
                usuarioBLL.Modificar(editado, txtContrasenia.Text);

                if (cmbRoles != null && cmbRoles.SelectedItem != null)
                {
                    Componente rolSeleccionado = (Componente)cmbRoles.SelectedItem;
                    seguridadBLL.AsignarRolAUsuario(editado, rolSeleccionado);
                }

                MostrarAviso(traduccionBLL.Traducir("msg_user_modified_success"), traduccionBLL.Traducir("lbl_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarYRecargar();
            }
            catch (Exception ex)
            {
                MostrarAviso(ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                if (usuarioSeleccionado == null)
                {
                    MostrarAviso("Seleccione un usuario del listado.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
                }
                if (nuevoEstado == false && usuarioSeleccionado.Id == Session.Instance.Usuario.Id)
                {
                    MostrarAviso(traduccionBLL.Traducir("msg_self_deactivate_stop"), traduccionBLL.Traducir("lbl_denied"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }

                if (usuarioSeleccionado.Activo == nuevoEstado)
                {
                    MostrarAviso(traduccionBLL.Traducir("msg_user_already_status"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!(nuevoEstado ? btnAlta.Enabled : btnBaja.Enabled)) return;
                var editado = (Usuario)usuarioSeleccionado.Clone(); editado.Activo = nuevoEstado;
                usuarioBLL.Modificar(editado, "");

                MostrarAviso(traduccionBLL.Traducir("msg_user_status_changed_success"), traduccionBLL.Traducir("lbl_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarYRecargar();
            }
            catch (Exception ex)
            {
                MostrarAviso(traduccionBLL.Traducir("msg_error_changing_status") + ": " + ex.Message, traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarYRecargar();
        }

        private void LimpiarYRecargar()
        {
            CargarGrilla();
            LimpiarSeleccion();
        }

        private bool ValidarCamposUI(bool requierePassword)
        {
            if (string.IsNullOrWhiteSpace(txtDni.Text) ||
                string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellido.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MostrarAviso(traduccionBLL.Traducir("msg_complete_all_text_fields"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (requierePassword && string.IsNullOrWhiteSpace(txtContrasenia.Text))
            {
                MostrarAviso(traduccionBLL.Traducir("msg_password_required"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!txtDni.Text.All(char.IsDigit))
            {
                MostrarAviso(traduccionBLL.Traducir("msg_dni_digits_only"), traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}
