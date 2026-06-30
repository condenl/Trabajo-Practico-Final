using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BE;
using BLL;
using BE.Interfaces;
using Servicios;

namespace UI.UserControls
{
    public partial class UC_Traducciones : UserControl, IIdiomaObserver
    {
        private readonly IdiomaBLL _idiomaBLL;
        private readonly TraduccionBLL _traduccionBLL;
        private Idioma _idiomaSeleccionado;
        private Traduccion _traduccionSeleccionada;

        public UC_Traducciones()
        {
            InitializeComponent();
            _idiomaBLL = new IdiomaBLL();
            _traduccionBLL = new TraduccionBLL();
        }

        private void UC_Traducciones_Load(object sender, EventArgs e)
        {
            GestorIdioma.Instancia.Suscribir(this);
            ActualizarTextos();
            CargarIdiomas();
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
                    control.Text = _traduccionBLL.Traducir(control.Tag.ToString());
                }
                if (control.HasChildren)
                {
                    ActualizarTextosRecursivo(control.Controls);
                }
            }
        }

        private void CargarIdiomas()
        {
            dgvIdiomas.DataSource = null;
            dgvIdiomas.DataSource = _idiomaBLL.Listar();
            ConfigurarColumnasIdiomas();
            dgvIdiomas.ClearSelection();
            _idiomaSeleccionado = null;
            LimpiarCamposIdioma();
            CargarTraducciones();
        }

        private void ConfigurarColumnasIdiomas()
        {
            if (dgvIdiomas.Columns.Count > 0)
            {
                dgvIdiomas.Columns["Id"].Width = 50;
                dgvIdiomas.Columns["Nombre"].Width = 200;
                dgvIdiomas.Columns["EsDefault"].Width = 100;
            }
        }

        private void CargarTraducciones()
        {
            dgvTraducciones.DataSource = null;
            if (_idiomaSeleccionado != null)
            {
                dgvTraducciones.DataSource = _traduccionBLL.ObtenerTraduccionesPorIdioma(_idiomaSeleccionado);
                ConfigurarColumnasTraducciones();
            }
            dgvTraducciones.ClearSelection();
            _traduccionSeleccionada = null;
            LimpiarCamposTraduccion();
        }

        private void ConfigurarColumnasTraducciones()
        {
            if (dgvTraducciones.Columns.Count > 0)
            {
                dgvTraducciones.Columns["Id"].Width = 50;
                dgvTraducciones.Columns["NombreEtiqueta"].Width = 250;
                dgvTraducciones.Columns["Texto"].Width = 400;
                if (dgvTraducciones.Columns.Contains("Idioma"))
                {
                    dgvTraducciones.Columns["Idioma"].Visible = false;
                }
            }
        }

        private void LimpiarCamposIdioma()
        {
            txtNombreIdioma.Clear();
            chkEsDefault.Checked = false;
        }

        private void LimpiarCamposTraduccion()
        {
            txtNombreEtiqueta.Clear();
            txtTextoTraduccion.Clear();
        }

        private void dgvIdiomas_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvIdiomas.CurrentRow != null)
            {
                _idiomaSeleccionado = (Idioma)dgvIdiomas.CurrentRow.DataBoundItem;
                txtNombreIdioma.Text = _idiomaSeleccionado.Nombre;
                chkEsDefault.Checked = _idiomaSeleccionado.EsDefault;
                CargarTraducciones();
            }
        }

        private void dgvTraducciones_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvTraducciones.CurrentRow != null)
            {
                _traduccionSeleccionada = (Traduccion)dgvTraducciones.CurrentRow.DataBoundItem;
                txtNombreEtiqueta.Text = _traduccionSeleccionada.NombreEtiqueta;
                txtTextoTraduccion.Text = _traduccionSeleccionada.Texto;
            }
        }

        private void btnAgregarIdioma_Click(object sender, EventArgs e)
        {
            try
            {
                string nombre = txtNombreIdioma.Text.Trim();
                if (string.IsNullOrEmpty(nombre))
                {
                    MessageBox.Show(_traduccionBLL.Traducir("msg_complete_all_fields"), _traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Idioma nuevo = new Idioma
                {
                    Nombre = nombre,
                    EsDefault = chkEsDefault.Checked
                };

                _idiomaBLL.Guardar(nuevo);
                MessageBox.Show(_traduccionBLL.Traducir("msg_success_action") ?? "Operación completada con éxito.", _traduccionBLL.Traducir("lbl_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarIdiomas();
                GestorIdioma.Instancia.NotificarCambio();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, _traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnModificarIdioma_Click(object sender, EventArgs e)
        {
            try
            {
                if (_idiomaSeleccionado == null)
                {
                    MessageBox.Show(_traduccionBLL.Traducir("msg_select_item_warn") ?? "Por favor, seleccione un elemento.", _traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string nombre = txtNombreIdioma.Text.Trim();
                if (string.IsNullOrEmpty(nombre))
                {
                    MessageBox.Show(_traduccionBLL.Traducir("msg_complete_all_fields"), _traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _idiomaSeleccionado.Nombre = nombre;
                _idiomaSeleccionado.EsDefault = chkEsDefault.Checked;

                _idiomaBLL.Guardar(_idiomaSeleccionado);
                MessageBox.Show(_traduccionBLL.Traducir("msg_success_action") ?? "Operación completada con éxito.", _traduccionBLL.Traducir("lbl_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarIdiomas();
                GestorIdioma.Instancia.NotificarCambio();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, _traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEliminarIdioma_Click(object sender, EventArgs e)
        {
            try
            {
                if (_idiomaSeleccionado == null)
                {
                    MessageBox.Show(_traduccionBLL.Traducir("msg_select_item_warn") ?? "Por favor, seleccione un elemento.", _traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult result = MessageBox.Show(_traduccionBLL.Traducir("msg_confirm_delete") ?? "¿Está seguro de que desea eliminar este elemento?", _traduccionBLL.Traducir("lbl_confirm") ?? "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    _idiomaBLL.Eliminar(_idiomaSeleccionado);
                    MessageBox.Show(_traduccionBLL.Traducir("msg_success_action") ?? "Operación completada con éxito.", _traduccionBLL.Traducir("lbl_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarIdiomas();
                    GestorIdioma.Instancia.NotificarCambio();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, _traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAgregarTraduccion_Click(object sender, EventArgs e)
        {
            try
            {
                if (_idiomaSeleccionado == null)
                {
                    MessageBox.Show(_traduccionBLL.Traducir("msg_select_language_warn") ?? "Debe seleccionar un idioma primero.", _traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string etiqueta = txtNombreEtiqueta.Text.Trim();
                string texto = txtTextoTraduccion.Text;

                if (string.IsNullOrEmpty(etiqueta) || string.IsNullOrEmpty(texto))
                {
                    MessageBox.Show(_traduccionBLL.Traducir("msg_complete_all_fields"), _traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Traduccion nueva = new Traduccion
                {
                    Idioma = _idiomaSeleccionado,
                    NombreEtiqueta = etiqueta,
                    Texto = texto
                };

                _traduccionBLL.Guardar(nueva);
                MessageBox.Show(_traduccionBLL.Traducir("msg_success_action") ?? "Operación completada con éxito.", _traduccionBLL.Traducir("lbl_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarTraducciones();
                GestorIdioma.Instancia.NotificarCambio();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, _traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnModificarTraduccion_Click(object sender, EventArgs e)
        {
            try
            {
                if (_traduccionSeleccionada == null)
                {
                    MessageBox.Show(_traduccionBLL.Traducir("msg_select_item_warn") ?? "Por favor, seleccione un elemento.", _traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string etiqueta = txtNombreEtiqueta.Text.Trim();
                string texto = txtTextoTraduccion.Text;

                if (string.IsNullOrEmpty(etiqueta) || string.IsNullOrEmpty(texto))
                {
                    MessageBox.Show(_traduccionBLL.Traducir("msg_complete_all_fields"), _traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _traduccionSeleccionada.NombreEtiqueta = etiqueta;
                _traduccionSeleccionada.Texto = texto;

                _traduccionBLL.Guardar(_traduccionSeleccionada);
                MessageBox.Show(_traduccionBLL.Traducir("msg_success_action") ?? "Operación completada con éxito.", _traduccionBLL.Traducir("lbl_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarTraducciones();
                GestorIdioma.Instancia.NotificarCambio();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, _traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEliminarTraduccion_Click(object sender, EventArgs e)
        {
            try
            {
                if (_traduccionSeleccionada == null)
                {
                    MessageBox.Show(_traduccionBLL.Traducir("msg_select_item_warn") ?? "Por favor, seleccione un elemento.", _traduccionBLL.Traducir("lbl_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult result = MessageBox.Show(_traduccionBLL.Traducir("msg_confirm_delete") ?? "¿Está seguro de que desea eliminar este elemento?", _traduccionBLL.Traducir("lbl_confirm") ?? "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    _traduccionBLL.Eliminar(_traduccionSeleccionada);
                    MessageBox.Show(_traduccionBLL.Traducir("msg_success_action") ?? "Operación completada con éxito.", _traduccionBLL.Traducir("lbl_success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarTraducciones();
                    GestorIdioma.Instancia.NotificarCambio();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, _traduccionBLL.Traducir("lbl_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
