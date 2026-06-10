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
            this.cmbPermisosDisponibles = new System.Windows.Forms.ComboBox();
            this.txtNombreRol = new System.Windows.Forms.TextBox();
            this.btnCrearRol = new System.Windows.Forms.Button();
            this.dgvUsuarios = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.btnModificarRol = new System.Windows.Forms.Button();
            this.btnEliminarRol = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.btnQuitarPermiso = new System.Windows.Forms.Button();
            this.btnAgregarPermiso = new System.Windows.Forms.Button();
            this.lbRoles = new System.Windows.Forms.ListBox();
            this.lbPermisosAsignados = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbPermisosDisponibles
            // 
            this.cmbPermisosDisponibles.FormattingEnabled = true;
            this.cmbPermisosDisponibles.Location = new System.Drawing.Point(1061, 54);
            this.cmbPermisosDisponibles.Name = "cmbPermisosDisponibles";
            this.cmbPermisosDisponibles.Size = new System.Drawing.Size(114, 21);
            this.cmbPermisosDisponibles.TabIndex = 52;
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
            // dgvUsuarios
            // 
            this.dgvUsuarios.AllowUserToAddRows = false;
            this.dgvUsuarios.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUsuarios.Location = new System.Drawing.Point(31, 301);
            this.dgvUsuarios.Name = "dgvUsuarios";
            this.dgvUsuarios.ReadOnly = true;
            this.dgvUsuarios.Size = new System.Drawing.Size(1265, 276);
            this.dgvUsuarios.TabIndex = 39;
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
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(969, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 55;
            this.label2.Tag = "lbl_permisos";
            this.label2.Text = "PERMISOS:";
            // 
            // btnQuitarPermiso
            // 
            this.btnQuitarPermiso.Location = new System.Drawing.Point(1093, 124);
            this.btnQuitarPermiso.Name = "btnQuitarPermiso";
            this.btnQuitarPermiso.Size = new System.Drawing.Size(128, 43);
            this.btnQuitarPermiso.TabIndex = 57;
            this.btnQuitarPermiso.Tag = "btn_quitar_permiso";
            this.btnQuitarPermiso.Text = "Quitar Permiso";
            this.btnQuitarPermiso.UseVisualStyleBackColor = true;
            this.btnQuitarPermiso.Click += new System.EventHandler(this.btnQuitarPermiso_Click);
            // 
            // btnAgregarPermiso
            // 
            this.btnAgregarPermiso.Location = new System.Drawing.Point(959, 124);
            this.btnAgregarPermiso.Name = "btnAgregarPermiso";
            this.btnAgregarPermiso.Size = new System.Drawing.Size(128, 43);
            this.btnAgregarPermiso.TabIndex = 56;
            this.btnAgregarPermiso.Tag = "btn_agregar_permiso";
            this.btnAgregarPermiso.Text = "Agregar Permiso";
            this.btnAgregarPermiso.UseVisualStyleBackColor = true;
            this.btnAgregarPermiso.Click += new System.EventHandler(this.btnAgregarPermiso_Click);
            // 
            // lbRoles
            // 
            this.lbRoles.FormattingEnabled = true;
            this.lbRoles.Location = new System.Drawing.Point(31, 174);
            this.lbRoles.Name = "lbRoles";
            this.lbRoles.Size = new System.Drawing.Size(392, 95);
            this.lbRoles.TabIndex = 58;
            // 
            // lbPermisosAsignados
            // 
            this.lbPermisosAsignados.FormattingEnabled = true;
            this.lbPermisosAsignados.Location = new System.Drawing.Point(904, 174);
            this.lbPermisosAsignados.Name = "lbPermisosAsignados";
            this.lbPermisosAsignados.Size = new System.Drawing.Size(392, 95);
            this.lbPermisosAsignados.TabIndex = 59;
            // 
            // UC_Roles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbPermisosAsignados);
            this.Controls.Add(this.lbRoles);
            this.Controls.Add(this.btnQuitarPermiso);
            this.Controls.Add(this.btnAgregarPermiso);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnEliminarRol);
            this.Controls.Add(this.btnModificarRol);
            this.Controls.Add(this.cmbPermisosDisponibles);
            this.Controls.Add(this.txtNombreRol);
            this.Controls.Add(this.btnCrearRol);
            this.Controls.Add(this.dgvUsuarios);
            this.Controls.Add(this.label1);
            this.Name = "UC_Roles";
            this.Size = new System.Drawing.Size(1325, 599);
            this.Load += new System.EventHandler(this.UC_Roles_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsuarios)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbPermisosDisponibles;
        private System.Windows.Forms.TextBox txtNombreRol;
        private System.Windows.Forms.Button btnCrearRol;
        private System.Windows.Forms.DataGridView dgvUsuarios;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnModificarRol;
        private System.Windows.Forms.Button btnEliminarRol;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnQuitarPermiso;
        private System.Windows.Forms.Button btnAgregarPermiso;
        private System.Windows.Forms.ListBox lbRoles;
        private System.Windows.Forms.ListBox lbPermisosAsignados;
    }
}
