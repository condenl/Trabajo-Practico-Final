using System;
using System.Windows.Forms;
using BE;
using UI.UserControls;
using BE.Seguridad;

namespace UI
{
    public partial class FormPrincipal : Form
    {
        private Usuario _usuarioLogueado;

        public FormPrincipal(Usuario usuario)
        {
            InitializeComponent();
            _usuarioLogueado = usuario;

            this.Text = $"Menú Principal - Usuario: {_usuarioLogueado.Nombre}";

            ConfigurarAccesos();
        }

        public FormPrincipal()
        {
            InitializeComponent();
        }

        private void ConfigurarAccesos()
        {
            if (_usuarioLogueado == null) return;

            btnUsuarios.Visible = _usuarioLogueado.TienePermiso(TipoPermiso.Usuarios_Ver);
            btnRoles.Visible = _usuarioLogueado.TienePermiso(TipoPermiso.Roles_Gestion);
            btnBitacora.Visible = _usuarioLogueado.TienePermiso(TipoPermiso.VerReportes);
        }

        private void btnBitacora_Click(object sender, EventArgs e)
        {
            UC_Bitacora bitacora = new UC_Bitacora();
            panel1.Controls.Clear();
            panel1.Controls.Add(bitacora);
            bitacora.Show();
        }

        private void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            DialogResult respuesta = MessageBox.Show(
                "¿Está seguro que desea cerrar la sesión actual?",
                "Confirmar Cierre de Sesión",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (respuesta == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void btnUsuarios_Click(object sender, EventArgs e)
        {
            UC_Usuarios usuarios = new UC_Usuarios();
            panel1.Controls.Clear();
            panel1.Controls.Add(usuarios);
            usuarios.Show();
        }

        private void btnRoles_Click(object sender, EventArgs e)
        {
            UC_Roles roles = new UC_Roles();
            panel1.Controls.Clear();
            panel1.Controls.Add(roles);
            roles.Show();
        }
    }
}