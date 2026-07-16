namespace UI.UserControls
{
    partial class UC_Roles
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtNombreRol = new System.Windows.Forms.TextBox();
            this.btnCrearRol = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnModificarRol = new System.Windows.Forms.Button();
            this.btnEliminarRol = new System.Windows.Forms.Button();
            this.btnQuitarPermiso = new System.Windows.Forms.Button();
            this.lbPermisosAsignados = new System.Windows.Forms.ListBox();
            this.tvRoles = new System.Windows.Forms.TreeView();
            this.cmbPatentes = new System.Windows.Forms.ComboBox();
            this.btnAgregarPatente = new System.Windows.Forms.Button();
            this.cmbRoles = new System.Windows.Forms.ComboBox();
            this.btnAgregarRol = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtNombreRol
            // 
            this.txtNombreRol.Location = new System.Drawing.Point(108, 13);
            this.txtNombreRol.Name = "txtNombreRol";
            this.txtNombreRol.Size = new System.Drawing.Size(114, 20);
            this.txtNombreRol.TabIndex = 46;
            // 
            // btnCrearRol
            // 
            this.btnCrearRol.Location = new System.Drawing.Point(27, 82);
            this.btnCrearRol.Name = "btnCrearRol";
            this.btnCrearRol.Size = new System.Drawing.Size(128, 43);
            this.btnCrearRol.TabIndex = 45;
            this.btnCrearRol.Tag = "btn_crear_rol";
            this.btnCrearRol.Text = "Crear Rol";
            this.btnCrearRol.UseVisualStyleBackColor = true;
            this.btnCrearRol.Click += new System.EventHandler(this.btnCrearRol_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 37;
            this.label1.Tag = "lbl_rol";
            this.label1.Text = "ROL:";
            // 
            // btnModificarRol
            // 
            this.btnModificarRol.Location = new System.Drawing.Point(161, 82);
            this.btnModificarRol.Name = "btnModificarRol";
            this.btnModificarRol.Size = new System.Drawing.Size(128, 43);
            this.btnModificarRol.TabIndex = 53;
            this.btnModificarRol.Tag = "btn_modificar_rol";
            this.btnModificarRol.Text = "Modificar Rol";
            this.btnModificarRol.UseVisualStyleBackColor = true;
            this.btnModificarRol.Click += new System.EventHandler(this.btnModificarRol_Click);
            // 
            // btnEliminarRol
            // 
            this.btnEliminarRol.Location = new System.Drawing.Point(295, 82);
            this.btnEliminarRol.Name = "btnEliminarRol";
            this.btnEliminarRol.Size = new System.Drawing.Size(128, 43);
            this.btnEliminarRol.TabIndex = 54;
            this.btnEliminarRol.Tag = "btn_eliminar_rol";
            this.btnEliminarRol.Text = "Eliminar Rol";
            this.btnEliminarRol.UseVisualStyleBackColor = true;
            this.btnEliminarRol.Click += new System.EventHandler(this.btnEliminarRol_Click);
            // 
            // btnQuitarPermiso
            // 
            this.btnQuitarPermiso.Location = new System.Drawing.Point(1038, 82);
            this.btnQuitarPermiso.Name = "btnQuitarPermiso";
            this.btnQuitarPermiso.Size = new System.Drawing.Size(128, 43);
            this.btnQuitarPermiso.TabIndex = 57;
            this.btnQuitarPermiso.Tag = "btn_quitar_permiso";
            this.btnQuitarPermiso.Text = "Quitar Permiso";
            this.btnQuitarPermiso.UseVisualStyleBackColor = true;
            this.btnQuitarPermiso.Click += new System.EventHandler(this.btnQuitarPermiso_Click);
            // 
            // lbPermisosAsignados
            // 
            this.lbPermisosAsignados.FormattingEnabled = true;
            this.lbPermisosAsignados.Location = new System.Drawing.Point(904, 174);
            this.lbPermisosAsignados.Name = "lbPermisosAsignados";
            this.lbPermisosAsignados.Size = new System.Drawing.Size(392, 316);
            this.lbPermisosAsignados.TabIndex = 59;
            // 
            // tvRoles
            // 
            this.tvRoles.Location = new System.Drawing.Point(31, 172);
            this.tvRoles.Name = "tvRoles";
            this.tvRoles.Size = new System.Drawing.Size(392, 301);
            this.tvRoles.TabIndex = 61;
            // 
            // cmbPatentes
            // 
            this.cmbPatentes.FormattingEnabled = true;
            this.cmbPatentes.Location = new System.Drawing.Point(973, 16);
            this.cmbPatentes.Name = "cmbPatentes";
            this.cmbPatentes.Size = new System.Drawing.Size(114, 21);
            this.cmbPatentes.TabIndex = 62;
            // 
            // btnAgregarPatente
            // 
            this.btnAgregarPatente.Location = new System.Drawing.Point(904, 82);
            this.btnAgregarPatente.Name = "btnAgregarPatente";
            this.btnAgregarPatente.Size = new System.Drawing.Size(128, 43);
            this.btnAgregarPatente.TabIndex = 63;
            this.btnAgregarPatente.Tag = "btn_agregar_permiso";
            this.btnAgregarPatente.Text = "Agregar Permiso";
            this.btnAgregarPatente.UseVisualStyleBackColor = true;
            this.btnAgregarPatente.Click += new System.EventHandler(this.btnAgregarPatente_Click);
            // 
            // cmbRoles
            // 
            this.cmbRoles.FormattingEnabled = true;
            this.cmbRoles.Location = new System.Drawing.Point(307, 12);
            this.cmbRoles.Name = "cmbRoles";
            this.cmbRoles.Size = new System.Drawing.Size(114, 21);
            this.cmbRoles.TabIndex = 64;
            // 
            // btnAgregarRol
            // 
            this.btnAgregarRol.Location = new System.Drawing.Point(429, 82);
            this.btnAgregarRol.Name = "btnAgregarRol";
            this.btnAgregarRol.Size = new System.Drawing.Size(128, 43);
            this.btnAgregarRol.TabIndex = 65;
            this.btnAgregarRol.Tag = "btn_agregar_permiso";
            this.btnAgregarRol.Text = "Añadir subrol";
            this.btnAgregarRol.UseVisualStyleBackColor = true;
            this.btnAgregarRol.Click += new System.EventHandler(this.btnAgregarRol_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(247, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 66;
            this.label2.Tag = "lbl_rol";
            this.label2.Text = "SUBROL:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(901, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 67;
            this.label3.Tag = "lbl_rol";
            this.label3.Text = "PERMISOS:";
            // 
            // UC_Roles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnAgregarRol);
            this.Controls.Add(this.cmbRoles);
            this.Controls.Add(this.btnAgregarPatente);
            this.Controls.Add(this.cmbPatentes);
            this.Controls.Add(this.tvRoles);
            this.Controls.Add(this.lbPermisosAsignados);
            this.Controls.Add(this.btnQuitarPermiso);
            this.Controls.Add(this.btnEliminarRol);
            this.Controls.Add(this.btnModificarRol);
            this.Controls.Add(this.txtNombreRol);
            this.Controls.Add(this.btnCrearRol);
            this.Controls.Add(this.label1);
            this.Name = "UC_Roles";
            this.Size = new System.Drawing.Size(1325, 599);
            this.Load += new System.EventHandler(this.UC_Roles_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtNombreRol;
        private System.Windows.Forms.Button btnCrearRol;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnModificarRol;
        private System.Windows.Forms.Button btnEliminarRol;
        private System.Windows.Forms.Button btnQuitarPermiso;
        private System.Windows.Forms.ListBox lbPermisosAsignados;
        private System.Windows.Forms.TreeView tvRoles;
        private System.Windows.Forms.ComboBox cmbPatentes;
        private System.Windows.Forms.Button btnAgregarPatente;
        private System.Windows.Forms.ComboBox cmbRoles;
        private System.Windows.Forms.Button btnAgregarRol;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}
