using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BE;
using BLL;
using Servicios;

namespace UI.UserControls
{
    public partial class UC_Usuarios : UserControl
    {
        private UsuarioBLL usuarioBLL;
        private Usuario usuarioSeleccionado;

        public UC_Usuarios()
        {
            InitializeComponent();
            usuarioBLL = new UsuarioBLL();

            if (txtContrasenia != null)
                txtContrasenia.UseSystemPasswordChar = true;

            this.Load += UC_Usuarios_Load;
        }

        private void UC_Usuarios_Load(object sender, EventArgs e)
        {
            CargarGrilla();
        }

        private void CargarGrilla()
        {
            try
            {
                dgvUsuarios.DataSource = null;
                dgvUsuarios.DataSource = usuarioBLL.ListarTodos();

                if (dgvUsuarios.Columns.Contains("Password")) dgvUsuarios.Columns["Password"].Visible = false;
                if (dgvUsuarios.Columns.Contains("Id")) dgvUsuarios.Columns["Id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la grilla: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvUsuarios_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dgvUsuarios.SelectedRows.Count > 0)
                {
                    usuarioSeleccionado = (Usuario)dgvUsuarios.SelectedRows[0].DataBoundItem;
                    txtDni.Text = usuarioSeleccionado.Dni; 
                    txtNombre.Text = usuarioSeleccionado.Nombre;
                    txtApellido.Text = usuarioSeleccionado.Apellido;
                    txtEmail.Text = usuarioSeleccionado.Email;
                    txtContrasenia.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al seleccionar usuario: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                MessageBox.Show("Usuario agregado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarYRecargar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al agregar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            try
            {
                if (usuarioSeleccionado == null)
                {
                    MessageBox.Show("Debe seleccionar un usuario de la grilla.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!ValidarCamposUI(requierePassword: false)) return;

                usuarioSeleccionado.Dni = txtDni.Text.Trim();
                usuarioSeleccionado.Nombre = txtNombre.Text.Trim();
                usuarioSeleccionado.Apellido = txtApellido.Text.Trim();
                usuarioSeleccionado.Email = txtEmail.Text.Trim();

                usuarioBLL.Modificar(usuarioSeleccionado, txtContrasenia.Text);
                MessageBox.Show("Usuario modificado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarYRecargar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al modificar", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show("Por cuestiones de seguridad, no podés dar de baja tu propia cuenta mientras estás en sesión.", "Acción Denegada", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }

                if (usuarioSeleccionado == null)
                {
                    MessageBox.Show("Debe seleccionar un usuario de la grilla.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (usuarioSeleccionado.Activo == nuevoEstado)
                {
                    MessageBox.Show($"El usuario ya se encuentra {accion}.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                usuarioSeleccionado.Activo = nuevoEstado;
                usuarioBLL.Modificar(usuarioSeleccionado, "");
                MessageBox.Show($"El usuario fue {accion} correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarYRecargar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar estado: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("Por favor, complete todos los campos de texto.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (requierePassword && string.IsNullOrWhiteSpace(txtContrasenia.Text))
            {
                MessageBox.Show("La contraseña es obligatoria para crear un usuario.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!txtDni.Text.All(char.IsDigit))
            {
                MessageBox.Show("El DNI debe contener solo números.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}