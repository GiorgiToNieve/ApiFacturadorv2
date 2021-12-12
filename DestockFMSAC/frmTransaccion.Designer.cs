namespace DestockFMSAC
{
    partial class frmTransaccion
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
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			Telerik.WinControls.UI.GridViewTextBoxColumn gridViewTextBoxColumn1 = new Telerik.WinControls.UI.GridViewTextBoxColumn();
			Telerik.WinControls.UI.GridViewTextBoxColumn gridViewTextBoxColumn2 = new Telerik.WinControls.UI.GridViewTextBoxColumn();
			Telerik.WinControls.UI.GridViewTextBoxColumn gridViewTextBoxColumn3 = new Telerik.WinControls.UI.GridViewTextBoxColumn();
			Telerik.WinControls.UI.GridViewTextBoxColumn gridViewTextBoxColumn4 = new Telerik.WinControls.UI.GridViewTextBoxColumn();
			Telerik.WinControls.UI.GridViewTextBoxColumn gridViewTextBoxColumn5 = new Telerik.WinControls.UI.GridViewTextBoxColumn();
			Telerik.WinControls.UI.GridViewTextBoxColumn gridViewTextBoxColumn6 = new Telerik.WinControls.UI.GridViewTextBoxColumn();
			Telerik.WinControls.UI.GridViewTextBoxColumn gridViewTextBoxColumn7 = new Telerik.WinControls.UI.GridViewTextBoxColumn();
			Telerik.WinControls.UI.GridViewTextBoxColumn gridViewTextBoxColumn8 = new Telerik.WinControls.UI.GridViewTextBoxColumn();
			Telerik.WinControls.UI.GridViewTextBoxColumn gridViewTextBoxColumn9 = new Telerik.WinControls.UI.GridViewTextBoxColumn();
			Telerik.WinControls.UI.GridViewTextBoxColumn gridViewTextBoxColumn10 = new Telerik.WinControls.UI.GridViewTextBoxColumn();
			Telerik.WinControls.UI.TableViewDefinition tableViewDefinition1 = new Telerik.WinControls.UI.TableViewDefinition();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTransaccion));
			this.crystalTheme1 = new Telerik.WinControls.Themes.CrystalTheme();
			this.btnEnviar = new Telerik.WinControls.UI.RadButton();
			this.dgvTransaccion = new Telerik.WinControls.UI.RadGridView();
			this.btnBuscar = new Telerik.WinControls.UI.RadButton();
			this.visualStudio2012DarkTheme1 = new Telerik.WinControls.Themes.VisualStudio2012DarkTheme();
			this.radWaitingBar1 = new Telerik.WinControls.UI.RadWaitingBar();
			this.segmentedRingWaitingBarIndicatorElement1 = new Telerik.WinControls.UI.SegmentedRingWaitingBarIndicatorElement();
			this.telerikMetroTheme1 = new Telerik.WinControls.Themes.TelerikMetroTheme();
			this.radLabel1 = new Telerik.WinControls.UI.RadLabel();
			this.lblTotal = new Telerik.WinControls.UI.RadLabel();
			this.materialTheme1 = new Telerik.WinControls.Themes.MaterialTheme();
			this.materialTheme2 = new Telerik.WinControls.Themes.MaterialTheme();
			this.materialPinkTheme1 = new Telerik.WinControls.Themes.MaterialPinkTheme();
			this.materialTheme3 = new Telerik.WinControls.Themes.MaterialTheme();
			this.lblResumen = new Telerik.WinControls.UI.RadLabel();
			((System.ComponentModel.ISupportInitialize)(this.btnEnviar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvTransaccion)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvTransaccion.MasterTemplate)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.btnBuscar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radWaitingBar1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radLabel1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.lblTotal)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.lblResumen)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
			this.SuspendLayout();
			// 
			// btnEnviar
			// 
			this.btnEnviar.Location = new System.Drawing.Point(18, 34);
			this.btnEnviar.Name = "btnEnviar";
			this.btnEnviar.Size = new System.Drawing.Size(120, 36);
			this.btnEnviar.TabIndex = 1;
			this.btnEnviar.Text = "Enviar a Sunat";
			this.btnEnviar.ThemeName = "VisualStudio2012Dark";
			this.btnEnviar.Click += new System.EventHandler(this.btnEnviar_Click);
			// 
			// dgvTransaccion
			// 
			this.dgvTransaccion.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dgvTransaccion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
			this.dgvTransaccion.Cursor = System.Windows.Forms.Cursors.Default;
			this.dgvTransaccion.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.dgvTransaccion.ForeColor = System.Drawing.Color.Black;
			this.dgvTransaccion.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.dgvTransaccion.Location = new System.Drawing.Point(453, 75);
			// 
			// 
			// 
			this.dgvTransaccion.MasterTemplate.AllowAddNewRow = false;
			this.dgvTransaccion.MasterTemplate.AllowSearchRow = true;
			gridViewTextBoxColumn1.EnableExpressionEditor = false;
			gridViewTextBoxColumn1.FieldName = "Transaccion_Id";
			gridViewTextBoxColumn1.HeaderText = "Transaccion_Id";
			gridViewTextBoxColumn1.Name = "dgcTransaccion_Id";
			gridViewTextBoxColumn1.Width = 110;
			gridViewTextBoxColumn2.EnableExpressionEditor = false;
			gridViewTextBoxColumn2.FieldName = "sTraRUCEmpresa";
			gridViewTextBoxColumn2.HeaderText = "RUC Empresa";
			gridViewTextBoxColumn2.Name = "dgcsTraRUCEmpresa";
			gridViewTextBoxColumn2.Width = 120;
			gridViewTextBoxColumn3.EnableExpressionEditor = false;
			gridViewTextBoxColumn3.FieldName = "sEmpNombre";
			gridViewTextBoxColumn3.HeaderText = "Empresa";
			gridViewTextBoxColumn3.Name = "dgcsEmpNombre";
			gridViewTextBoxColumn3.Width = 200;
			gridViewTextBoxColumn4.EnableExpressionEditor = false;
			gridViewTextBoxColumn4.FieldName = "sTraTipoDocumento";
			gridViewTextBoxColumn4.HeaderText = "TipoDocumento";
			gridViewTextBoxColumn4.Name = "dgcsTraTipoDocumento";
			gridViewTextBoxColumn4.Width = 120;
			gridViewTextBoxColumn5.EnableExpressionEditor = false;
			gridViewTextBoxColumn5.FieldName = "sTraSerie";
			gridViewTextBoxColumn5.HeaderText = "Serie";
			gridViewTextBoxColumn5.Name = "dgcsTraSerie";
			gridViewTextBoxColumn5.Width = 60;
			gridViewTextBoxColumn6.EnableExpressionEditor = false;
			gridViewTextBoxColumn6.FieldName = "sTraNumero";
			gridViewTextBoxColumn6.HeaderText = "Numero";
			gridViewTextBoxColumn6.Name = "dgcsTraNumero";
			gridViewTextBoxColumn6.Width = 100;
			gridViewTextBoxColumn7.EnableExpressionEditor = false;
			gridViewTextBoxColumn7.FieldName = "sCliNroIdentidad";
			gridViewTextBoxColumn7.HeaderText = "RUC Cliente";
			gridViewTextBoxColumn7.Name = "dgcsCliNroIdentidad";
			gridViewTextBoxColumn7.Width = 120;
			gridViewTextBoxColumn8.EnableExpressionEditor = false;
			gridViewTextBoxColumn8.FieldName = "sCliente";
			gridViewTextBoxColumn8.HeaderText = "Cliente";
			gridViewTextBoxColumn8.Name = "dgcsCliente";
			gridViewTextBoxColumn8.Width = 250;
			gridViewTextBoxColumn9.EnableExpressionEditor = false;
			gridViewTextBoxColumn9.FieldName = "sTraEstadoTransaccionElectronica";
			gridViewTextBoxColumn9.HeaderText = "Estado Electrónico";
			gridViewTextBoxColumn9.Name = "dgcsTraEstadoTransaccionElectronica";
			gridViewTextBoxColumn9.Width = 150;
			gridViewTextBoxColumn10.EnableExpressionEditor = false;
			gridViewTextBoxColumn10.FieldName = "sTraEmail";
			gridViewTextBoxColumn10.HeaderText = "Email Cliente";
			gridViewTextBoxColumn10.Name = "dgcsTraEmail";
			gridViewTextBoxColumn10.Width = 200;
			this.dgvTransaccion.MasterTemplate.Columns.AddRange(new Telerik.WinControls.UI.GridViewDataColumn[] {
            gridViewTextBoxColumn1,
            gridViewTextBoxColumn2,
            gridViewTextBoxColumn3,
            gridViewTextBoxColumn4,
            gridViewTextBoxColumn5,
            gridViewTextBoxColumn6,
            gridViewTextBoxColumn7,
            gridViewTextBoxColumn8,
            gridViewTextBoxColumn9,
            gridViewTextBoxColumn10});
			this.dgvTransaccion.MasterTemplate.EnableFiltering = true;
			this.dgvTransaccion.MasterTemplate.ViewDefinition = tableViewDefinition1;
			this.dgvTransaccion.Name = "dgvTransaccion";
			this.dgvTransaccion.ReadOnly = true;
			this.dgvTransaccion.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.dgvTransaccion.Size = new System.Drawing.Size(70, 446);
			this.dgvTransaccion.TabIndex = 0;
			this.dgvTransaccion.ThemeName = "VisualStudio2012Dark";
			this.dgvTransaccion.Visible = false;
			// 
			// btnBuscar
			// 
			this.btnBuscar.Location = new System.Drawing.Point(150, 34);
			this.btnBuscar.Name = "btnBuscar";
			this.btnBuscar.Size = new System.Drawing.Size(120, 36);
			this.btnBuscar.TabIndex = 2;
			this.btnBuscar.Text = "Buscar";
			this.btnBuscar.ThemeName = "VisualStudio2012Dark";
			this.btnBuscar.Click += new System.EventHandler(this.btnBuscar_Click);
			// 
			// radWaitingBar1
			// 
			this.radWaitingBar1.ForeColor = System.Drawing.Color.Cyan;
			this.radWaitingBar1.Location = new System.Drawing.Point(12, 12);
			this.radWaitingBar1.Name = "radWaitingBar1";
			this.radWaitingBar1.ShowText = true;
			this.radWaitingBar1.Size = new System.Drawing.Size(371, 233);
			this.radWaitingBar1.StretchIndicatorsHorizontally = true;
			this.radWaitingBar1.TabIndex = 2;
			this.radWaitingBar1.Text = "Procesando";
			this.radWaitingBar1.ThemeName = "VisualStudio2012Dark";
			this.radWaitingBar1.WaitingIndicators.Add(this.segmentedRingWaitingBarIndicatorElement1);
			this.radWaitingBar1.WaitingIndicatorSize = new System.Drawing.Size(100, 100);
			this.radWaitingBar1.WaitingSpeed = 50;
			this.radWaitingBar1.WaitingStyle = Telerik.WinControls.Enumerations.WaitingBarStyles.SegmentedRing;
			// 
			// segmentedRingWaitingBarIndicatorElement1
			// 
			this.segmentedRingWaitingBarIndicatorElement1.Name = "segmentedRingWaitingBarIndicatorElement1";
			// 
			// radLabel1
			// 
			this.radLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
			this.radLabel1.Location = new System.Drawing.Point(10, 270);
			this.radLabel1.Name = "radLabel1";
			this.radLabel1.Size = new System.Drawing.Size(163, 22);
			this.radLabel1.TabIndex = 3;
			this.radLabel1.Text = "Total Comprobantes";
			this.radLabel1.ThemeName = "VisualStudio2012Dark";
			// 
			// lblTotal
			// 
			this.lblTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.lblTotal.Location = new System.Drawing.Point(196, 270);
			this.lblTotal.Name = "lblTotal";
			this.lblTotal.Size = new System.Drawing.Size(17, 22);
			this.lblTotal.TabIndex = 4;
			this.lblTotal.Text = "0";
			this.lblTotal.ThemeName = "VisualStudio2012Dark";
			// 
			// lblResumen
			// 
			this.lblResumen.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.lblResumen.Location = new System.Drawing.Point(10, 331);
			this.lblResumen.Name = "lblResumen";
			this.lblResumen.Size = new System.Drawing.Size(14, 22);
			this.lblResumen.TabIndex = 5;
			this.lblResumen.Text = "*";
			this.lblResumen.ThemeName = "VisualStudio2012Dark";
			// 
			// frmTransaccion
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(417, 549);
			this.Controls.Add(this.lblResumen);
			this.Controls.Add(this.lblTotal);
			this.Controls.Add(this.radLabel1);
			this.Controls.Add(this.radWaitingBar1);
			this.Controls.Add(this.btnEnviar);
			this.Controls.Add(this.dgvTransaccion);
			this.Controls.Add(this.btnBuscar);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "frmTransaccion";
			// 
			// 
			// 
			this.RootElement.ApplyShapeToControl = true;
			this.Text = "Proceso Batch FMSAC - Lite Versión";
			this.ThemeName = "VisualStudio2012Dark";
			this.Load += new System.EventHandler(this.frmTransaccion_Load);
			((System.ComponentModel.ISupportInitialize)(this.btnEnviar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvTransaccion.MasterTemplate)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvTransaccion)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.btnBuscar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radWaitingBar1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radLabel1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.lblTotal)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.lblResumen)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private Telerik.WinControls.Themes.CrystalTheme crystalTheme1;
        private Telerik.WinControls.UI.RadButton btnEnviar;
        private Telerik.WinControls.UI.RadGridView dgvTransaccion;
        private Telerik.WinControls.UI.RadButton btnBuscar;
        private Telerik.WinControls.Themes.VisualStudio2012DarkTheme visualStudio2012DarkTheme1;
        private Telerik.WinControls.UI.RadWaitingBar radWaitingBar1;
        private Telerik.WinControls.UI.SegmentedRingWaitingBarIndicatorElement segmentedRingWaitingBarIndicatorElement1;
        private Telerik.WinControls.Themes.TelerikMetroTheme telerikMetroTheme1;
        private Telerik.WinControls.UI.RadLabel radLabel1;
        private Telerik.WinControls.UI.RadLabel lblTotal;
		private Telerik.WinControls.Themes.MaterialTheme materialTheme1;
		private Telerik.WinControls.Themes.MaterialTheme materialTheme2;
		private Telerik.WinControls.Themes.MaterialPinkTheme materialPinkTheme1;
		private Telerik.WinControls.Themes.MaterialTheme materialTheme3;
		private Telerik.WinControls.UI.RadLabel lblResumen;
	}
}
