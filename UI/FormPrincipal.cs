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
    }
}