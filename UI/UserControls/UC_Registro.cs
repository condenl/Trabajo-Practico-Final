using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BE;
using BE.Interfaces;
using BLL;

namespace UI.UserControls
{
    public partial class UC_Registro : UserControl
    {
        public event EventHandler OnRegresar;
        private IVisitante usuarioBLL;
        private RolBLL rolBLL;

        public UC_Registro()
        {
            InitializeComponent();
            usuarioBLL = new UsuarioBLL();
            rolBLL = new RolBLL();

            if (txtPassword != null)
                txtPassword.UseSystemPasswordChar = true;
            this.Load += UC_Registro_Load;
        }

        private void UC_Registro_Load(object sender, EventArgs e)
        {
            CargarRoles();
        }

        private void CargarRoles()
        {
            try
            {
                List<Rol> roles = rolBLL.ListarTodos();

                if (roles != null && roles.Count > 0)
                {
                    cmbRol.DataSource = null;
                    cmbRol.DataSource = roles;
                    cmbRol.DisplayMember = "Nombre";
                    cmbRol.ValueMember = "Id";      

                    cmbRol.SelectedIndex = 0; 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la lista de roles: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtDni.Text) ||
                    string.IsNullOrWhiteSpace(txtNombre.Text) ||
                    string.IsNullOrWhiteSpace(txtApellido.Text) ||
                    string.IsNullOrWhiteSpace(txtEmail.Text) ||
                    string.IsNullOrWhiteSpace(txtPassword.Text) ||
                    cmbRol.SelectedValue == null)
                {
                    MessageBox.Show("Por favor, complete todos los campos y seleccione un rol.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!txtDni.Text.All(char.IsDigit))
                {
                    MessageBox.Show("El DNI debe contener solo números.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Usuario nuevoUsuario = new Usuario
                {
                    Dni = txtDni.Text.Trim(),
                    Nombre = txtNombre.Text.Trim(),
                    Apellido = txtApellido.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Activo = true,
                    RolId = (int)cmbRol.SelectedValue
                };

                usuarioBLL.Registrar(nuevoUsuario, txtPassword.Text);

                MessageBox.Show("¡Usuario registrado con éxito! Ya puedes iniciar sesión.", "Registro Exitoso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LimpiarCampos();
                OnRegresar?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al registrar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarCampos()
        {
            txtDni.Clear();
            txtNombre.Clear();
            txtApellido.Clear();
            txtEmail.Clear();
            txtPassword.Clear();
            if (cmbRol.Items.Count > 0) cmbRol.SelectedIndex = 0;
        }

        private void btnVolver_Click(object sender, EventArgs e)
        {
            OnRegresar?.Invoke(this, EventArgs.Empty);
        }
    }
}