namespace UI.UserControls
{
    partial class UC_Traducciones
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Servicios.GestorIdioma.Instancia.Desuscribir(this);
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.gbIdiomas = new System.Windows.Forms.GroupBox();
            this.btnEliminarIdioma = new System.Windows.Forms.Button();
            this.btnModificarIdioma = new System.Windows.Forms.Button();
            this.btnAgregarIdioma = new System.Windows.Forms.Button();
            this.chkEsDefault = new System.Windows.Forms.CheckBox();
            this.txtNombreIdioma = new System.Windows.Forms.TextBox();
            this.lblNombreIdioma = new System.Windows.Forms.Label();
            this.dgvIdiomas = new System.Windows.Forms.DataGridView();
            this.gbTraducciones = new System.Windows.Forms.GroupBox();
            this.lblTextoTraduccion = new System.Windows.Forms.Label();
            this.txtTextoTraduccion = new System.Windows.Forms.TextBox();
            this.btnEliminarTraduccion = new System.Windows.Forms.Button();
            this.btnModificarTraduccion = new System.Windows.Forms.Button();
            this.btnAgregarTraduccion = new System.Windows.Forms.Button();
            this.txtNombreEtiqueta = new System.Windows.Forms.TextBox();
            this.lblNombreEtiqueta = new System.Windows.Forms.Label();
            this.dgvTraducciones = new System.Windows.Forms.DataGridView();
            this.gbIdiomas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIdiomas)).BeginInit();
            this.gbTraducciones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTraducciones)).BeginInit();
            this.SuspendLayout();
            // 
            // gbIdiomas
            // 
            this.gbIdiomas.Controls.Add(this.btnEliminarIdioma);
            this.gbIdiomas.Controls.Add(this.btnModificarIdioma);
            this.gbIdiomas.Controls.Add(this.btnAgregarIdioma);
            this.gbIdiomas.Controls.Add(this.chkEsDefault);
            this.gbIdiomas.Controls.Add(this.txtNombreIdioma);
            this.gbIdiomas.Controls.Add(this.lblNombreIdioma);
            this.gbIdiomas.Controls.Add(this.dgvIdiomas);
            this.gbIdiomas.Location = new System.Drawing.Point(15, 15);
            this.gbIdiomas.Name = "gbIdiomas";
            this.gbIdiomas.Size = new System.Drawing.Size(450, 550);
            this.gbIdiomas.TabIndex = 0;
            this.gbIdiomas.TabStop = false;
            this.gbIdiomas.Tag = "traducciones_gb_idiomas";
            this.gbIdiomas.Text = "Idiomas";
            // 
            // btnEliminarIdioma
            // 
            this.btnEliminarIdioma.Location = new System.Drawing.Point(300, 430);
            this.btnEliminarIdioma.Name = "btnEliminarIdioma";
            this.btnEliminarIdioma.Size = new System.Drawing.Size(130, 35);
            this.btnEliminarIdioma.TabIndex = 6;
            this.btnEliminarIdioma.Tag = "btn_eliminar";
            this.btnEliminarIdioma.Text = "Eliminar";
            this.btnEliminarIdioma.UseVisualStyleBackColor = true;
            this.btnEliminarIdioma.Click += new System.EventHandler(this.btnEliminarIdioma_Click);
            // 
            // btnModificarIdioma
            // 
            this.btnModificarIdioma.Location = new System.Drawing.Point(158, 430);
            this.btnModificarIdioma.Name = "btnModificarIdioma";
            this.btnModificarIdioma.Size = new System.Drawing.Size(130, 35);
            this.btnModificarIdioma.TabIndex = 5;
            this.btnModificarIdioma.Tag = "btn_modificar";
            this.btnModificarIdioma.Text = "Modificar";
            this.btnModificarIdioma.UseVisualStyleBackColor = true;
            this.btnModificarIdioma.Click += new System.EventHandler(this.btnModificarIdioma_Click);
            // 
            // btnAgregarIdioma
            // 
            this.btnAgregarIdioma.Location = new System.Drawing.Point(15, 430);
            this.btnAgregarIdioma.Name = "btnAgregarIdioma";
            this.btnAgregarIdioma.Size = new System.Drawing.Size(130, 35);
            this.btnAgregarIdioma.TabIndex = 4;
            this.btnAgregarIdioma.Tag = "btn_agregar";
            this.btnAgregarIdioma.Text = "Agregar";
            this.btnAgregarIdioma.UseVisualStyleBackColor = true;
            this.btnAgregarIdioma.Click += new System.EventHandler(this.btnAgregarIdioma_Click);
            // 
            // chkEsDefault
            // 
            this.chkEsDefault.AutoSize = true;
            this.chkEsDefault.Location = new System.Drawing.Point(15, 390);
            this.chkEsDefault.Name = "chkEsDefault";
            this.chkEsDefault.Size = new System.Drawing.Size(112, 17);
            this.chkEsDefault.TabIndex = 3;
            this.chkEsDefault.Tag = "traducciones_chk_default";
            this.chkEsDefault.Text = "Idioma por Defecto";
            this.chkEsDefault.UseVisualStyleBackColor = true;
            // 
            // txtNombreIdioma
            // 
            this.txtNombreIdioma.Location = new System.Drawing.Point(15, 350);
            this.txtNombreIdioma.Name = "txtNombreIdioma";
            this.txtNombreIdioma.Size = new System.Drawing.Size(415, 20);
            this.txtNombreIdioma.TabIndex = 2;
            // 
            // lblNombreIdioma
            // 
            this.lblNombreIdioma.AutoSize = true;
            this.lblNombreIdioma.Location = new System.Drawing.Point(15, 325);
            this.lblNombreIdioma.Name = "lblNombreIdioma";
            this.lblNombreIdioma.Size = new System.Drawing.Size(102, 13);
            this.lblNombreIdioma.TabIndex = 1;
            this.lblNombreIdioma.Tag = "traducciones_lbl_nom_idioma";
            this.lblNombreIdioma.Text = "Nombre del Idioma:";
            // 
            // dgvIdiomas
            // 
            this.dgvIdiomas.AllowUserToAddRows = false;
            this.dgvIdiomas.AllowUserToDeleteRows = false;
            this.dgvIdiomas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvIdiomas.Location = new System.Drawing.Point(15, 25);
            this.dgvIdiomas.MultiSelect = false;
            this.dgvIdiomas.Name = "dgvIdiomas";
            this.dgvIdiomas.ReadOnly = true;
            this.dgvIdiomas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvIdiomas.Size = new System.Drawing.Size(415, 280);
            this.dgvIdiomas.TabIndex = 0;
            this.dgvIdiomas.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvIdiomas_CellClick);
            // 
            // gbTraducciones
            // 
            this.gbTraducciones.Controls.Add(this.lblTextoTraduccion);
            this.gbTraducciones.Controls.Add(this.txtTextoTraduccion);
            this.gbTraducciones.Controls.Add(this.btnEliminarTraduccion);
            this.gbTraducciones.Controls.Add(this.btnModificarTraduccion);
            this.gbTraducciones.Controls.Add(this.btnAgregarTraduccion);
            this.gbTraducciones.Controls.Add(this.txtNombreEtiqueta);
            this.gbTraducciones.Controls.Add(this.lblNombreEtiqueta);
            this.gbTraducciones.Controls.Add(this.dgvTraducciones);
            this.gbTraducciones.Location = new System.Drawing.Point(480, 15);
            this.gbTraducciones.Name = "gbTraducciones";
            this.gbTraducciones.Size = new System.Drawing.Size(800, 550);
            this.gbTraducciones.TabIndex = 1;
            this.gbTraducciones.TabStop = false;
            this.gbTraducciones.Tag = "traducciones_gb_traducciones";
            this.gbTraducciones.Text = "Traducciones";
            // 
            // lblTextoTraduccion
            // 
            this.lblTextoTraduccion.AutoSize = true;
            this.lblTextoTraduccion.Location = new System.Drawing.Point(15, 385);
            this.lblTextoTraduccion.Name = "lblTextoTraduccion";
            this.lblTextoTraduccion.Size = new System.Drawing.Size(87, 13);
            this.lblTextoTraduccion.TabIndex = 7;
            this.lblTextoTraduccion.Tag = "traducciones_lbl_texto";
            this.lblTextoTraduccion.Text = "Texto Traducido:";
            // 
            // txtTextoTraduccion
            // 
            this.txtTextoTraduccion.Location = new System.Drawing.Point(15, 405);
            this.txtTextoTraduccion.Multiline = true;
            this.txtTextoTraduccion.Name = "txtTextoTraduccion";
            this.txtTextoTraduccion.Size = new System.Drawing.Size(765, 55);
            this.txtTextoTraduccion.TabIndex = 8;
            // 
            // btnEliminarTraduccion
            // 
            this.btnEliminarTraduccion.Location = new System.Drawing.Point(300, 480);
            this.btnEliminarTraduccion.Name = "btnEliminarTraduccion";
            this.btnEliminarTraduccion.Size = new System.Drawing.Size(130, 35);
            this.btnEliminarTraduccion.TabIndex = 6;
            this.btnEliminarTraduccion.Tag = "btn_eliminar";
            this.btnEliminarTraduccion.Text = "Eliminar";
            this.btnEliminarTraduccion.UseVisualStyleBackColor = true;
            this.btnEliminarTraduccion.Click += new System.EventHandler(this.btnEliminarTraduccion_Click);
            // 
            // btnModificarTraduccion
            // 
            this.btnModificarTraduccion.Location = new System.Drawing.Point(158, 480);
            this.btnModificarTraduccion.Name = "btnModificarTraduccion";
            this.btnModificarTraduccion.Size = new System.Drawing.Size(130, 35);
            this.btnModificarTraduccion.TabIndex = 5;
            this.btnModificarTraduccion.Tag = "btn_modificar";
            this.btnModificarTraduccion.Text = "Modificar";
            this.btnModificarTraduccion.UseVisualStyleBackColor = true;
            this.btnModificarTraduccion.Click += new System.EventHandler(this.btnModificarTraduccion_Click);
            // 
            // btnAgregarTraduccion
            // 
            this.btnAgregarTraduccion.Location = new System.Drawing.Point(15, 480);
            this.btnAgregarTraduccion.Name = "btnAgregarTraduccion";
            this.btnAgregarTraduccion.Size = new System.Drawing.Size(130, 35);
            this.btnAgregarTraduccion.TabIndex = 4;
            this.btnAgregarTraduccion.Tag = "btn_agregar";
            this.btnAgregarTraduccion.Text = "Agregar";
            this.btnAgregarTraduccion.UseVisualStyleBackColor = true;
            this.btnAgregarTraduccion.Click += new System.EventHandler(this.btnAgregarTraduccion_Click);
            // 
            // txtNombreEtiqueta
            // 
            this.txtNombreEtiqueta.Location = new System.Drawing.Point(15, 350);
            this.txtNombreEtiqueta.Name = "txtNombreEtiqueta";
            this.txtNombreEtiqueta.Size = new System.Drawing.Size(765, 20);
            this.txtNombreEtiqueta.TabIndex = 2;
            // 
            // lblNombreEtiqueta
            // 
            this.lblNombreEtiqueta.AutoSize = true;
            this.lblNombreEtiqueta.Location = new System.Drawing.Point(15, 325);
            this.lblNombreEtiqueta.Name = "lblNombreEtiqueta";
            this.lblNombreEtiqueta.Size = new System.Drawing.Size(126, 13);
            this.lblNombreEtiqueta.TabIndex = 1;
            this.lblNombreEtiqueta.Tag = "traducciones_lbl_etiqueta";
            this.lblNombreEtiqueta.Text = "Nombre de Etiqueta/Clave:";
            // 
            // dgvTraducciones
            // 
            this.dgvTraducciones.AllowUserToAddRows = false;
            this.dgvTraducciones.AllowUserToDeleteRows = false;
            this.dgvTraducciones.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTraducciones.Location = new System.Drawing.Point(15, 25);
            this.dgvTraducciones.MultiSelect = false;
            this.dgvTraducciones.Name = "dgvTraducciones";
            this.dgvTraducciones.ReadOnly = true;
            this.dgvTraducciones.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTraducciones.Size = new System.Drawing.Size(765, 280);
            this.dgvTraducciones.TabIndex = 0;
            this.dgvTraducciones.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvTraducciones_CellClick);
            // 
            // UC_Traducciones
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbTraducciones);
            this.Controls.Add(this.gbIdiomas);
            this.Name = "UC_Traducciones";
            this.Size = new System.Drawing.Size(1300, 580);
            this.Load += new System.EventHandler(this.UC_Traducciones_Load);
            this.gbIdiomas.ResumeLayout(false);
            this.gbIdiomas.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIdiomas)).EndInit();
            this.gbTraducciones.ResumeLayout(false);
            this.gbTraducciones.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTraducciones)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbIdiomas;
        private System.Windows.Forms.Button btnEliminarIdioma;
        private System.Windows.Forms.Button btnModificarIdioma;
        private System.Windows.Forms.Button btnAgregarIdioma;
        private System.Windows.Forms.CheckBox chkEsDefault;
        private System.Windows.Forms.TextBox txtNombreIdioma;
        private System.Windows.Forms.Label lblNombreIdioma;
        private System.Windows.Forms.DataGridView dgvIdiomas;
        private System.Windows.Forms.GroupBox gbTraducciones;
        private System.Windows.Forms.Button btnEliminarTraduccion;
        private System.Windows.Forms.Button btnModificarTraduccion;
        private System.Windows.Forms.Button btnAgregarTraduccion;
        private System.Windows.Forms.TextBox txtNombreEtiqueta;
        private System.Windows.Forms.Label lblNombreEtiqueta;
        private System.Windows.Forms.DataGridView dgvTraducciones;
        private System.Windows.Forms.Label lblTextoTraduccion;
        private System.Windows.Forms.TextBox txtTextoTraduccion;
    }
}
