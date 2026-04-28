using System;
using System.Windows.Forms;
using BE;
using UI.UserControls;

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
        }

        public FormPrincipal()
        {
            InitializeComponent();
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
    }
}