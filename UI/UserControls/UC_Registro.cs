using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BE.Interfaces;
using BLL;

namespace UI.UserControls
{
    public partial class UC_Registro: UserControl
    {
        public event EventHandler OnRegresar;
        private IVisitante usuarioBLL;
        private RolBLL rolBLL;
        public UC_Registro()
        {
            InitializeComponent();
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {

        }

        private void btnVolver_Click(object sender, EventArgs e)
        {

        }
    }
}
