using Entidades.Comercial;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls;
using System.Threading.Tasks;
using Negocio.Comercial;
using Utilitarios;
using Entidades.Maestros;
using System.Linq;
using Negocio.Maestros;
using Sunat;
using DestockFMSAC.Base;
using System.IO;
using Telerik.WinControls.UI;
using Telerik.WinControls.Primitives;

namespace DestockFMSAC
{
    public partial class Inicio : BaseForm
    {
        #region Variables Globales
        Servicio wsServicio = new Servicio();
        DocumentoElectronico oDocumentoElectronico = new DocumentoElectronico();
        DesktopFMSAC.Sunat.PDF oPDF = new DesktopFMSAC.Sunat.PDF();
        List<Transaccion> LstTransaccion = new List<Transaccion>();
        List<TransaccionDetalle> LstTransaccionDetalle = new List<TransaccionDetalle>();
        private GridViewTextBoxColumn textBoxColumn = null;
        List<Transaccion> LstTraBoletas = new List<Transaccion>();
        int nContar = 0;

        #endregion


        #region Constructor
        public Inicio()
        {
            InitializeComponent();
        }
        #endregion

        private async void btnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                radWaitingBarElement1.Visibility = ElementVisibility.Visible;
                radWaitingBarElement1.StartWaiting();

                LstTransaccion = new List<Transaccion>();
                LstTransaccion = await ObtenerTransaccionPendienteAsync();

                dgvTransaccion.DataSource = null;

                if (LstTransaccion != null && LstTransaccion.Count > 0)
                {
                    dgvTransaccion.DataSource = LstTransaccion;

                    #region Transaccion Venta Detalle

                    LstTransaccionDetalle = new List<TransaccionDetalle>();

                    foreach (var oVenta in LstTransaccion)
                    {
                        foreach (var oDetalle in oVenta.LstTransaccionDetalle)
                        {
                            LstTransaccionDetalle.Add(oDetalle);
                        }
                    }

                    #endregion


                    #region Creación de Template y sus Columnas del Template

                    var templateDetalle = new GridViewTemplate();

                    textBoxColumn = new GridViewTextBoxColumn();
                    textBoxColumn.FieldName = "TransaccionDetalle_Id";
                    textBoxColumn.Name = "TransaccionDetalle_Id";
                    textBoxColumn.HeaderText = @"TransaccionDetalle_Id";
                    textBoxColumn.Width = 100;
                    textBoxColumn.IsVisible = false;
                    textBoxColumn.ReadOnly = true;
                    textBoxColumn.TextAlignment = ContentAlignment.MiddleCenter;
                    templateDetalle.Columns.Add(textBoxColumn);

                    textBoxColumn = new GridViewTextBoxColumn();
                    textBoxColumn.FieldName = "Transaccion_Id";
                    textBoxColumn.Name = "Transaccion_Id";
                    textBoxColumn.HeaderText = @"Transaccion_Id";
                    textBoxColumn.Width = 100;
                    textBoxColumn.IsVisible = false;
                    textBoxColumn.ReadOnly = true;
                    textBoxColumn.TextAlignment = ContentAlignment.MiddleCenter;
                    templateDetalle.Columns.Add(textBoxColumn);

                    textBoxColumn = new GridViewTextBoxColumn();
                    textBoxColumn.FieldName = "Producto_Id";
                    textBoxColumn.Name = "Producto_Id";
                    textBoxColumn.HeaderText = @"Producto_Id";
                    textBoxColumn.Width = 100;
                    textBoxColumn.IsVisible = true;
                    textBoxColumn.ReadOnly = true;
                    textBoxColumn.TextAlignment = ContentAlignment.MiddleCenter;
                    templateDetalle.Columns.Add(textBoxColumn);

                    textBoxColumn = new GridViewTextBoxColumn();
                    textBoxColumn.FieldName = "sProNombre";
                    textBoxColumn.Name = "sProNombre";
                    textBoxColumn.HeaderText = @"Producto";
                    textBoxColumn.Width = 200;
                    textBoxColumn.IsVisible = true;
                    textBoxColumn.ReadOnly = true;
                    templateDetalle.Columns.Add(textBoxColumn);

                    textBoxColumn = new GridViewTextBoxColumn();
                    textBoxColumn.FieldName = "sUMeDescripcion";
                    textBoxColumn.Name = "sUMeDescripcion";
                    textBoxColumn.HeaderText = @"Unidad Medida";
                    textBoxColumn.Width = 100;
                    textBoxColumn.IsVisible = true;
                    textBoxColumn.ReadOnly = true;
                    templateDetalle.Columns.Add(textBoxColumn);

                    textBoxColumn = new GridViewTextBoxColumn();
                    textBoxColumn.FieldName = "nTDeCantidad";
                    textBoxColumn.Name = "nTDeCantidad";
                    textBoxColumn.HeaderText = @"Cantidad";
                    textBoxColumn.Width = 100;
                    textBoxColumn.FormatString = @"{0:N2}";
                    textBoxColumn.TextAlignment = ContentAlignment.MiddleRight;
                    textBoxColumn.IsVisible = true;
                    textBoxColumn.ReadOnly = true;
                    templateDetalle.Columns.Add(textBoxColumn);


                    textBoxColumn = new GridViewTextBoxColumn();
                    textBoxColumn.FieldName = "nTDePrecio";
                    textBoxColumn.Name = "nTDePrecio";
                    textBoxColumn.HeaderText = @"Precio";
                    textBoxColumn.Width = 100;
                    textBoxColumn.FormatString = @"{0:N5}";
                    textBoxColumn.TextAlignment = ContentAlignment.MiddleRight;
                    textBoxColumn.IsVisible = true;
                    textBoxColumn.ReadOnly = true;
                    templateDetalle.Columns.Add(textBoxColumn);

                    textBoxColumn = new GridViewTextBoxColumn();
                    textBoxColumn.FieldName = "nTDeBase";
                    textBoxColumn.Name = "nTDeBase";
                    textBoxColumn.HeaderText = @"Base";
                    textBoxColumn.Width = 100;
                    textBoxColumn.FormatString = @"{0:N2}";
                    textBoxColumn.TextAlignment = ContentAlignment.MiddleRight;
                    textBoxColumn.IsVisible = true;
                    textBoxColumn.ReadOnly = true;
                    templateDetalle.Columns.Add(textBoxColumn);

                    textBoxColumn = new GridViewTextBoxColumn();
                    textBoxColumn.FieldName = "nTDeIGV";
                    textBoxColumn.Name = "nTDeIGV";
                    textBoxColumn.HeaderText = @"IGV";
                    textBoxColumn.Width = 100;
                    textBoxColumn.FormatString = @"{0:N2}";
                    textBoxColumn.TextAlignment = ContentAlignment.MiddleRight;
                    textBoxColumn.IsVisible = true;
                    textBoxColumn.ReadOnly = true;
                    templateDetalle.Columns.Add(textBoxColumn);

                    textBoxColumn = new GridViewTextBoxColumn();
                    textBoxColumn.FieldName = "nTDeSubtotal";
                    textBoxColumn.Name = "nTDeSubtotal";
                    textBoxColumn.HeaderText = @"Subtotal";
                    textBoxColumn.Width = 100;
                    textBoxColumn.FormatString = @"{0:N2}";
                    textBoxColumn.TextAlignment = ContentAlignment.MiddleRight;
                    textBoxColumn.IsVisible = true;
                    textBoxColumn.ReadOnly = true;
                    templateDetalle.Columns.Add(textBoxColumn);

                    templateDetalle.AllowAddNewRow = false;

                    #endregion

                    #region Llenado del Template y Asignacion a MasterTemplate

                    templateDetalle.DataSource = null;
                    templateDetalle.DataSource = LstTransaccionDetalle;

                    dgvTransaccion.MasterTemplate.Templates.Clear();

                    if (dgvTransaccion.MasterTemplate.Templates.Count > 0)
                        dgvTransaccion.MasterTemplate.Templates.RemoveAt(0);

                    dgvTransaccion.MasterTemplate.Templates.Add(templateDetalle);
                    dgvTransaccion.MasterTemplate.ShowChildViewCaptions = true;
                    dgvTransaccion.Templates[0].Caption = @"Detalle";

                    #endregion

                    #region Creación y agregado de Relacion

                    var relation = new GridViewRelation(dgvTransaccion.MasterTemplate);

                    relation.ChildTemplate = templateDetalle;

                    relation.RelationName = "VentaDetalles";
                    relation.ParentColumnNames.Add("dgcTransaccion_Id");
                    relation.ChildColumnNames.Add("Transaccion_Id");

                    dgvTransaccion.Relations.Add(relation);

                    #endregion


                }

                radWaitingBarElement1.StopWaiting();
                radWaitingBarElement1.Visibility = ElementVisibility.Hidden;
                ContarElementos();
                

            }
            catch (Exception ex)
            {
                radWaitingBarElement1.StopWaiting();
                radWaitingBarElement1.Visibility = ElementVisibility.Hidden;
                lblMensaje.Text = ex.Message;
            }
        }


        public void ContarElementos()
        {
            lblMensaje.Text = " " + dgvTransaccion.Rows.Count + " registros encontrados";
        }


        private Task<List<Transaccion>> ObtenerTransaccionPendienteAsync()
        {
            try
            {
                return Task.Run(() => ObtenerTransaccionPendiente());
            }
            catch (Exception)
            {

                throw;
            }
        }

        private List<Transaccion> ObtenerTransaccionPendiente()
        {
            try
            {
                var Parametros = new Dictionary<string, object>();
                Parametros.Add("dFechaInicio", dFechaInicio.Value.ToDateTime());
                Parametros.Add("dFechaFinal", dFechaFin.Value.ToDateTime());

                if(cboEstadoSunat.SelectedItem.Tag.toInt() != -1)
                Parametros.Add("nTraEstadoTransaccionElectronica",  cboEstadoSunat.SelectedItem.Tag.toInt());

                Parametros.Add("TipoDocumento_Id", cboTipoDocumento.SelectedValue.toInt());

                Parametros.Add("nTraEstado", Enumerador.ESTADO_ACTIVO);
                return TransaccionNeg.Instance.Consultar(Parametros);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Inicio_Load(object sender, EventArgs e)
        {
            try
            {
                InicializarFormulario();
                btnBuscar.PerformClick();
            }
            catch (Exception ex)
            {
                RadMessageBox.Show(ex.Message);
            }
        }

        private void InicializarFormulario()
        {
            try
            {
            dFechaInicio.Value = DateTime.Now.AddDays(-7);
            dFechaFin.Value = DateTime.Now;
            dgvTransaccion.MasterTemplate.ShowHeaderCellButtons = true;
            dgvTransaccion.MasterTemplate.ShowFilteringRow = false;
            cboEstadoSunat.SelectedIndex = 1;
            cboEstadoSunat_SelectedValueChanged(null, null);
            CargarTipoDocumento();

            }
            catch (Exception)
            {

                throw;
            }

        }

        private void CargarTipoDocumento()
        {
            try
            {
                var Parametros = new Dictionary<string, object>();
                Parametros.Add("nTDoEstado", Enumerador.ESTADO_ACTIVO);

                var ListaDocumentos = TipoDocumentoNeg.Instance.Consultar(Parametros);

                if (ListaDocumentos != null && ListaDocumentos.Count > 0)
                {
                    cboTipoDocumento.DisplayMember = "sTDoNombre";
                    cboTipoDocumento.ValueMember = "TipoDocumento_Id";
                    cboTipoDocumento.DataSource = ListaDocumentos.Where(x => x.nTDoMostrar == Enumerador.ESTADO_ACTIVO).ToList();

                }

            }
            catch (Exception)
            {

                throw;
            }
            
        }

        private async void btnBajaSunat_Click(object sender, EventArgs e)
        {
            try
            {
                if (Validar())
                {

                    if (MostrarPregunta("Esta seguro que desea dar Comunicación de baja  a Sunat a los documentos seleccionados?") == DialogResult.Yes)
                    {

                        radWaitingBarElement1.Visibility = ElementVisibility.Visible;
                        radWaitingBarElement1.StartWaiting();
                        await ComunicacionBajaSunatAync();
                        MostrarMensaje("El Proceso ha Terminado Correctamente");

                        btnBuscar.PerformClick();
                        radWaitingBarElement1.StopWaiting();
                        radWaitingBarElement1.Visibility = ElementVisibility.Hidden;
                    }
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = ex.Message;
            }
        }

        private Task ComunicacionBajaSunatAync()
        {
            try
            {
                return Task.Run(() => ComunicacionBaja());
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool ComunicacionBaja()
        {
            try
            {
                LstTraBoletas = new List<Transaccion>();

                foreach (var row in dgvTransaccion.Rows)
                {
                    if (row.Cells["dgcOk"].Value != null)
                    {
                        if ((bool)row.Cells["dgcOk"].Value)
                        {
                            int index = row.Index;

                            var oTransaccion = LstTransaccion.FirstOrDefault(x => x.Transaccion_Id == row.Cells["dgcTransaccion_Id"].Value.toInt());

                            if (oTransaccion != null)
                            {
                                var oEmpresa = EmpresaNeg.Instance.Consultar(oTransaccion.sTraRUCEmpresa);
                                
                                #region BOLETAS

                                if (oTransaccion.TipoDocumento_Id ==
                                    (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
                                {
                                    LstTraBoletas = new List<Transaccion>();
                                    LstTraBoletas.Add(oTransaccion);

                                    bool result = false;

                                    result = wsServicio.EnviarResumenBoletasSunat(LstTraBoletas, oEmpresa, Enumerador.ESTADO_BOLETA_ELECTRONICA_BAJA);

                                    if (result)
                                    {
                                        #region Enviar Correo Baja
                                        try
                                        {
                                            TransaccionNeg.Instance.EnviarCorreoBajaGenerico(oTransaccion, oEmpresa);
                                        }
                                        catch
                                        {
                                        }
                                        #endregion

                                        //Actualizar Grilla
                                        if (dgvTransaccion.InvokeRequired)
                                        {
                                            dgvTransaccion.Invoke(new Action(delegate ()
                                            {
                                                foreach (var item in dgvTransaccion.Rows)
                                                {
                                                    if (((Transaccion)item.DataBoundItem).Transaccion_Id ==
                                                        oTransaccion.Transaccion_Id)
                                                    {
                                                        item.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "Baja enviada";
                                                        break;
                                                    }
                                                }
                                            }));
                                        }
                                    }
                                    else
                                    {
                                        //Actualizar Grilla
                                        if (dgvTransaccion.InvokeRequired)
                                        {
                                            dgvTransaccion.Invoke(new Action(delegate ()
                                            {
                                                foreach (var item in dgvTransaccion.Rows)
                                                {
                                                    if (((Transaccion)item.DataBoundItem).Transaccion_Id ==
                                                        oTransaccion.Transaccion_Id)
                                                    {
                                                        item.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "Error de Baja";
                                                        break;
                                                    }
                                                }
                                            }));
                                        }
                                    }
                                }
                                #endregion

                                else
                                #region FACTURAS
                                    if (wsServicio.AnularArchivo(oTransaccion, oEmpresa))
                                {
                                    #region Enviar Correo Baja
                                    try
                                    {
                                        TransaccionNeg.Instance.EnviarCorreoBajaGenerico(oTransaccion, oEmpresa);
                                    }
                                    catch
                                    {
                                    }
                                    #endregion

                                    //Actualizar Grilla
                                    if (dgvTransaccion.InvokeRequired)
                                    {
                                        dgvTransaccion.Invoke(new Action(delegate ()
                                        {
                                            foreach (var item in dgvTransaccion.Rows)
                                            {
                                                if (((Transaccion)item.DataBoundItem).Transaccion_Id ==
                                                    oTransaccion.Transaccion_Id)
                                                {
                                                    item.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "Baja enviada";
                                                    break;
                                                }
                                            }
                                        }));
                                    }
                                }
                                else
                                {
                                    //Actualizar Grilla
                                    if (dgvTransaccion.InvokeRequired)
                                    {
                                        dgvTransaccion.Invoke(new Action(delegate ()
                                        {
                                            foreach (var item in dgvTransaccion.Rows)
                                            {
                                                if (((Transaccion)item.DataBoundItem).Transaccion_Id ==
                                                    oTransaccion.Transaccion_Id)
                                                {
                                                    item.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "Error de Baja";
                                                    break;
                                                }
                                            }
                                        }));
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                }
               
                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }

        private void cboTipoDocumento_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                dgvTransaccion.DataSource = null;
                ContarElementos();
            }
            catch (Exception ex)
            {
                RadMessageBox.Show(ex.Message);
            }
        }

        private void cboEstadoSunat_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {

                //cboEstadoSunat.SelectedItem.Tag.toInt()
                if (cboEstadoSunat.SelectedItem != null &&
                    cboEstadoSunat.SelectedItem.Tag.toInt() >= (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO)
                {
                    btnEnviarCorreo.Enabled = true;
                    btnEnviar.Enabled = false;
                }
                else
                {
                    if (cboEstadoSunat.SelectedItem != null &&
                        cboEstadoSunat.SelectedItem.Tag.toInt().ToString() == "-1")
                    {
                        btnEnviarCorreo.Enabled = false;
                        btnEnviar.Enabled = false;
                        btnBajaSunat.Enabled = false;
                    }
                    else
                    {
                        btnEnviarCorreo.Enabled = false;
                        btnEnviar.Enabled = true;
                        btnBajaSunat.Enabled = false;
                    }
                }

                if (cboEstadoSunat.SelectedItem != null &&
                    cboEstadoSunat.SelectedItem.Tag.toInt() >=
                    (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_SUNAT)
                {
                    btnEnviar.Enabled = false;
                    btnBajaSunat.Enabled = true;
                    btnEnviarCorreo.Enabled = true;

                }
                if (cboEstadoSunat.SelectedItem != null &&
                   cboEstadoSunat.SelectedItem.Tag.toInt() >=
                   (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_BAJA_ENVIADA_SUNAT)
                {
                    btnEnviar.Enabled = false;
                    btnBajaSunat.Enabled = false;
                }

                dgvTransaccion.DataSource = null;

            }
            catch (Exception ex)
            {
                lblMensaje.Text=ex.Message;
            }
        }

        private void btnExportarExcel_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvTransaccion.Rows.Count > 0)
                {
                    GenerarExcelCompleto("DocumentosElectronicos", dgvTransaccion);
                }
                else
                {
                    lblMensaje.Text = "No se han encontrado Elementos la Lista está vacía";
                }
            }
            catch (Exception ex)
            {
                LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
                lblMensaje.Text = ex.Message;
            }
            
        }

        private async void MasterTemplate_CommandCellClick(object sender, Telerik.WinControls.UI.GridViewCellEventArgs e)
        {
            try
            {
                int TransaccionVenta_Id = dgvTransaccion.Rows[e.RowIndex].Cells["dgcTransaccion_Id"].Value.toInt();

                if (e.Column.Name.Contains("VistaPrevia"))
                {

                    if (MostrarPregunta("Esta seguro que desea Visualizar PDF") == DialogResult.Yes)
                    {
                        if (TransaccionVenta_Id > 0)
                        {
                            radWaitingBarElement1.Visibility = ElementVisibility.Visible;
                            radWaitingBarElement1.StartWaiting();
                            string ruta = string.Empty;

                            ruta = await Task.Run(() => VistaPrevia(TransaccionVenta_Id));
                            radWaitingBarElement1.StopWaiting();
                            if (ruta.Length > 0)
                            {
                                VistaPReviaMostrar(ruta);
                            }

                            radWaitingBarElement1.Visibility = ElementVisibility.Hidden;
                            ContarElementos();
                        }
                    }
                    
                }

            }
            catch (Exception ex)
            {
                radWaitingBarElement1.StopWaiting();
                radWaitingBarElement1.Visibility = ElementVisibility.Hidden;
                lblMensaje.Text = ex.Message;
            }
        }

        private string VistaPrevia(int transaccionVenta_Id)
        {
            try
            {
                var oTransaccion = LstTransaccion.FirstOrDefault(x => x.Transaccion_Id == transaccionVenta_Id);
                var oEmpresa = EmpresaNeg.Instance.Consultar(oTransaccion.Empresa_Id,"*");
                GenerarPDF(oTransaccion, oEmpresa);
                string strNombreArchivo = oDocumentoElectronico.GenerarNombreXML(oTransaccion, oEmpresa);
                return VistaPreviaPDF(oTransaccion, oEmpresa, strNombreArchivo);

            }
            catch (Exception)
            {

                throw;
            }

        }

        private string  VistaPreviaPDF(Transaccion oTransaccionVenta, Empresa p_oEmpresa, string strNombreArchivo)
        {
            try
            {
                string RUTA_PDF = p_oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_FACTURAS_ELECTRONICAS_PDF;

                if (oTransaccionVenta.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
                {
                    RUTA_PDF = p_oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_BOLETAS_ELECTRONICAS_PDF;
                }
                else if (oTransaccionVenta.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
                {
                    RUTA_PDF = p_oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_NOTA_CREDITO_ELECTRONICAS_PDF;
                }
                else if (oTransaccionVenta.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO)
                {
                    RUTA_PDF = p_oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_NOTA_DEBITO_ELECTRONICAS_PDF;
                }

                //Finalmanete: 
                RUTA_PDF = RUTA_PDF + strNombreArchivo + @"\" +  strNombreArchivo + ".pdf";

                if (File.Exists(RUTA_PDF))
                {
                    return RUTA_PDF;
                }
                return string.Empty;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void VistaPReviaMostrar(string rUTA_PDF)
        {
            try
            {
                VistaPreviaPDF(rUTA_PDF);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool GenerarPDF(Transaccion oTransaccion, Empresa oEmpresa)
        {
            try
            {
                bool b_resultado = false;
                wsServicio.GenerarCodigoQR(oTransaccion, oEmpresa);

                string RUTA_CODIGO_HASH = string.Empty;
                string RUTA_LOGO_EMPRESA = string.Empty;

                string strNombreArchivo = oDocumentoElectronico.GenerarNombreXML(oTransaccion, oEmpresa);

                #region RUTA DE LA EMPRESA

                RUTA_LOGO_EMPRESA = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_EMPRESA_LOGO_PDF;

                #endregion

                if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
                {
                    RUTA_CODIGO_HASH = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_BOLETAS +
                                       strNombreArchivo + @"\" +
                                       strNombreArchivo + ".bmp";
                }
                else
                    if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA)
                {
                    RUTA_CODIGO_HASH = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_FACTURAS +
                                       strNombreArchivo + @"\" +
                                       strNombreArchivo + ".bmp";
                }
                if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
                {
                    RUTA_CODIGO_HASH = oEmpresa.sEmpRuta + Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_CREDITO +
                                       strNombreArchivo + @"\" +
                                       strNombreArchivo + ".bmp";

                }


                #region Validando que exista la ruta del archivo sino no se genera el logo

                byte[] imgLogo = null;

                try
                {
                    var logo = Image.FromFile(RUTA_LOGO_EMPRESA);
                    imgLogo = Util.ImageToByteArray(logo);
                }
                catch
                {
                    //
                }


                #endregion


                if (RUTA_CODIGO_HASH.Length > 0)
                {
                    byte[] imgCodigoHash = null;

                    #region Validando que exista la ruta del archivo sino no se genera el hash
                    /*Con esto valido que un document no necesariamente puede tener hash*/
                    try
                    {
                        var cod = Image.FromFile(RUTA_CODIGO_HASH);
                        imgCodigoHash = Util.ImageToByteArray(cod);
                    }
                    catch
                    {
                        //
                    }

                    #endregion

                    if (oTransaccion.LstTransaccionDetalle.Count > 1)
                    {
                        oTransaccion.LstTransaccionDetalle[0].iLogo = imgLogo;
                        oTransaccion.LstTransaccionDetalle[0].iCodHash = imgCodigoHash;

                        try
                        {
                            int index = oTransaccion.LstTransaccionDetalle.Count - 1;
                            oTransaccion.LstTransaccionDetalle[index].iLogo = imgLogo;
                            oTransaccion.LstTransaccionDetalle[index].iCodHash = imgCodigoHash;
                        }
                        catch
                        {
                            //
                        }
                    }
                    else
                    {
                        oTransaccion.LstTransaccionDetalle[0].iLogo = imgLogo;
                        oTransaccion.LstTransaccionDetalle[0].iCodHash = imgCodigoHash;
                    }
                }
                else
                {
                    oTransaccion.LstTransaccionDetalle[0].iLogo = null;
                    oTransaccion.LstTransaccionDetalle[0].iCodHash = null;
                }

                #region OBTENER UN DATATABLE A PARTIR DE UNA GENERICA

                var dtSource = ObtenerDatosReporte(oTransaccion.LstTransaccionDetalle);

                #endregion

                #region POR TIPO DE DOCUMENTO SE GENERA EL PDF

                switch (oTransaccion.TipoDocumento_Id)
                {
                    #region FACTURA

                    case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:

                        b_resultado = oPDF.ReportarFactura(dtSource, oTransaccion, strNombreArchivo, oEmpresa);

                        if (b_resultado)
                        {
                            //actualizacion en memoria
                            if (oTransaccion.nTraEstadoTransaccionElectronica !=
                                (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_CLIENTE)
                            {
                                oTransaccion.nTraEstadoTransaccionElectronica =
                                    (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO;

                                //actualizacion en bd
                                wsServicio.ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id,
                                    (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO);
                            }
                        }
                        return b_resultado;

                        break;

                    #endregion

                    #region BOLETA

                    case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:

                        b_resultado = oPDF.ReportarBoleta(dtSource, oTransaccion, strNombreArchivo, oEmpresa);

                        if (b_resultado)
                        {
                            //actualizacion en memoria
                            if (oTransaccion.nTraEstadoTransaccionElectronica !=
                                (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_CLIENTE)
                            {
                                oTransaccion.nTraEstadoTransaccionElectronica =
                                    (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO;

                                //actualizacion en bd
                                wsServicio.ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id,
                                    (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO);
                            }
                        }

                        return b_resultado;
                        break;

                    #endregion

                    #region NOTA DE CREDITO

                    case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:

                        b_resultado = oPDF.ReportarNotaCredito(dtSource, oTransaccion, strNombreArchivo, oEmpresa);

                        if (b_resultado)
                        {
                            wsServicio.ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id,
                                (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO);
                        }

                        return b_resultado;

                        break;

                        #endregion

                        #region NOTA DE DEBITO

                        //case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:

                        //    b_resultado = oPDF.ReportarNotaDebito(dtSource, oTransaccion, strNombreArchivo);

                        //    if (b_resultado)
                        //    {
                        //        wsServicio.ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id,
                        //            (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO);
                        //    }

                        //    return b_resultado;

                        //    break;

                        #endregion
                }

                #endregion

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private DataTable ObtenerDatosReporte(List<TransaccionDetalle> ListaResultado)
        {
            try
            {
                return oPDF.GenerarDataTable(ListaResultado);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async void btnEnviar_Click(object sender, EventArgs e)
        {
            try
            {
                if (Validar())
                {
                    if(MostrarPregunta("¿Esta seguro que desea Enviar Los Documentos Seleccionados a Sunat?")== DialogResult.Yes)
                    {
                        radWaitingBarElement1.Visibility= ElementVisibility.Visible;
                        radWaitingBarElement1.StartWaiting();

                        bool b_Resultado = await Task.Run(() => GenerarFacturacionElectronicaAsync());

                        if (b_Resultado)
                        {
                            lblMensaje.Text = "Registros Actualizados";
                            bool b_ResultadoPDFCorreo = await Task.Run(() => GenerarPDFyEnviarCorreoClienteAsync());
                        }
                        else
                        {
                            lblMensaje.Text = "No se ha podido Enviar";
                        }


                        MostrarMensaje("El Proceso ha Terminado Corectamente!");
                        radWaitingBarElement1.StopWaiting();
                        radWaitingBarElement1.Visibility = ElementVisibility.Hidden;
                        btnBuscar.PerformClick();

                    }
                }
            }
            catch (Exception ex)
            {

                RadMessageBox.Show(ex.Message);
            }
        }

        private bool GenerarPDFyEnviarCorreoClienteAsync()
        {
            bool task1 = false;

            task1 = GenerarPDFClienteAsync();

            if (task1)
            {
                return EnviarPDFClienteAsync().Result;
            }

            return false;
        }


        private bool GenerarPDFClienteAsync()
        {
            return GenerarPDFCliente();
        }

        private Task<bool> EnviarPDFClienteAsync()
        {
            return Task.Run(() => EnviarPDFCliente());
        }

        private bool EnviarPDFCliente()
        {
            foreach (var row in dgvTransaccion.Rows)
            {
                if (row.Cells["dgcOK"].Value != null)
                {
                    if ((bool)row.Cells["dgcOK"].Value)
                    {
                        int index = row.Index;
                        var oTransaccion = LstTransaccion.FirstOrDefault(x => x.Transaccion_Id == row.Cells["dgcTransaccion_Id"].Value.toInt());

                        /*Sen enviara el correo siempre y cuando haya pdf generado si ya se envio el correo lo volvera a enviar*/
                        if (row.Cells["dgcnTraEstadoTransaccionElectronica"].Value.toInt() >= (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO)
                        {
                            var oEmpresa = EmpresaNeg.Instance.Consultar(oTransaccion.sTraRUCEmpresa);

                            string strNombreArchivo = oDocumentoElectronico.GenerarNombreXML(oTransaccion, oEmpresa);
                            bool b_resultado = oPDF.EnviarPDFCorreo(oTransaccion, oEmpresa, strNombreArchivo);
                            wsServicio.ActualizarDocumentoElectronico(oTransaccion.Transaccion_Id, (int)Enumerador.RESPUESTA_ENVIADO_EMAIL_CLIENTE);

                            #region Actualizar Exito

                            if (b_resultado)
                            {
                                if (dgvTransaccion.InvokeRequired)
                                {
                                    dgvTransaccion.Invoke(new Action(delegate ()
                                    {
                                        foreach (var item in dgvTransaccion.Rows)
                                        {
                                            if (((Transaccion)item.DataBoundItem).Transaccion_Id == oTransaccion.Transaccion_Id)
                                            {
                                                item.Cells["dgcnTraEstadoTransaccionElectronica"].Value = (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_CLIENTE;
                                                item.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "Email Enviado";
                                            }
                                        }
                                    }));
                                }
                                else
                                {
                                    row.Cells["dgcnTraEstadoTransaccionElectronica"].Value = (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_CLIENTE;
                                    row.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "Email Enviado";
                                }
                            }
                            #endregion

                            else

                            #region Actualizar Falla

                            if (dgvTransaccion.InvokeRequired)
                            {
                                dgvTransaccion.Invoke(new Action(delegate ()
                                {
                                    foreach (var item in dgvTransaccion.Rows)
                                    {
                                        if (((Transaccion)item.DataBoundItem).Transaccion_Id == oTransaccion.Transaccion_Id)
                                        {
                                            item.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "Email No Enviado";
                                        }
                                    }
                                }));
                            }
                            else
                            {
                                row.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "Email No Enviado";
                            }

                            #endregion
                        }
                    }
                }
            }

            return true;
        }




        private bool GenerarPDFCliente()
        {
            foreach (var row in dgvTransaccion.Rows)
            {

                if (row.Cells["dgcOK"].Value != null)
                {
                    if ((bool)row.Cells["dgcOK"].Value)
                    {
                        int index = row.Index;

                        var oTransaccion = LstTransaccion.FirstOrDefault(x => x.Transaccion_Id == row.Cells["dgcTransaccion_Id"].Value.toInt());
                        var oEmpresa = EmpresaNeg.Instance.Consultar(oTransaccion.sTraRUCEmpresa);

                        if (row.Cells["dgcnTraEstadoTransaccionElectronica"].Value.toInt() >=
                            (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_SUNAT)
                        {
                            bool b_resultado = GenerarPDF(oTransaccion, oEmpresa);

                            if (b_resultado)
                            {
                                #region Actualizar exito

                                if (dgvTransaccion.InvokeRequired)
                                {
                                    dgvTransaccion.Invoke(new Action(delegate ()
                                    {
                                        foreach (var item in dgvTransaccion.Rows)
                                        {
                                            if (((Transaccion)item.DataBoundItem).Transaccion_Id ==
                                                oTransaccion.Transaccion_Id)
                                            {
                                                item.Cells["dgcnTraEstadoTransaccionElectronica"].Value = (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO;
                                                item.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "PDF Generado";
                                            }
                                        }
                                    }));
                                }
                                else
                                {
                                    row.Cells["dgcnTraEstadoTransaccionElectronica"].Value = (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PDF_GENERADO;
                                    row.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "PDF Generado";
                                }

                                #endregion

                            }
                            else
                            #region Actualizar Falla

                                if (dgvTransaccion.InvokeRequired)
                            {
                                dgvTransaccion.Invoke(new Action(delegate ()
                                {
                                    foreach (var item in dgvTransaccion.Rows)
                                    {
                                        if (((Transaccion)item.DataBoundItem).Transaccion_Id ==
                                            oTransaccion.Transaccion_Id)
                                        {
                                            item.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "PDF No Generado";
                                        }
                                    }
                                }));
                            }
                            else
                            {
                                row.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "PDF No Generado";
                            }

                            #endregion
                        }
                    }
                }
            }

            return true;
        }




        private bool GenerarFacturacionElectronicaAsync()
        {
            try
            {
                return GenerarFacturacionElectronica().Result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<bool> GenerarFacturacionElectronica()
        {
            try
            {
                return await GenerarArchivoXmlyZipAsync();
            }
            catch (Exception)
            {

                throw;
            }
            

        }


        private Task<bool> GenerarArchivoXmlyZipAsync()
        {
            return Task.Run(() => GenerarArchivoXmlyZip());
        }

        private bool GenerarArchivoXmlyZip()
        {
            LstTraBoletas = new List<Transaccion>();

            foreach (var row in dgvTransaccion.Rows)
            {
                if (row.Cells["dgcOk"].Value != null)
                {
                    if ((bool)row.Cells["dgcOk"].Value)
                    {
                        int index = row.Index;

                        var oTransaccion = LstTransaccion.FirstOrDefault(x => x.Transaccion_Id == row.Cells["dgcTransaccion_Id"].Value.toInt());

                        if (oTransaccion.nTraEstadoTransaccionElectronica ==
                            (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_PENDIENTE)
                        {
                            if(oTransaccion.TipoDocumento_Id != (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
                            ConxtruiryEnviarXMLFacturasNC(oTransaccion,row);

                            else
                            {
                                //SI SON BOLETAS DEBE AGRUPARSE para el Resumem
                                LstTraBoletas.Add(oTransaccion);
                            }
                        }
                    }
                }
            }
            //Verificando las boletas agrupadas

            if(LstTraBoletas != null && LstTraBoletas.Count > 0)
            {
                try
                {
                    EnviarResumenDiarioBoletas(LstTraBoletas);
                }
                catch 
                {
                }
            }
            return true;
        }

        private void EnviarResumenDiarioBoletas(List<Transaccion> lstTraBoletas)
        {
            try
            {
                if (lstTraBoletas.Count > 0)
                {
                    var oTra = LstTraBoletas.FirstOrDefault();
                    var MiEmpresa = EmpresaNeg.Instance.Consultar(oTra.sTraRUCEmpresa);
                    bool rpta = false;
                    rpta = wsServicio.EnviarResumenBoletasSunat(lstTraBoletas, MiEmpresa);
                    if (rpta)
                    {
                        //Actualizar Grilla
                        if (dgvTransaccion.InvokeRequired)
                        {
                            dgvTransaccion.Invoke(new Action(delegate ()
                            {
                                foreach (var oTraBoleta in LstTraBoletas)
                                    foreach (var item in dgvTransaccion.Rows)
                                {
                                    if (((Transaccion)item.DataBoundItem).Transaccion_Id ==
                                        oTraBoleta.Transaccion_Id)
                                    {
                                        item.Cells["dgcnTraEstadoTransaccionElectronica"].Value = (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_SUNAT;
                                        item.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "Enviado";
                                         break;
                                    }
                                }
                            }));
                        }
                    }
                    else //si no ha pasado las boletas entomces actualizamos con error de envio
                    {
                        //Actualizar Grilla
                        if (dgvTransaccion.InvokeRequired)
                        {
                            dgvTransaccion.Invoke(new Action(delegate ()
                            {
                                foreach (var oTraBoleta in LstTraBoletas)
                                    foreach (var item in dgvTransaccion.Rows)
                                    {
                                        if (((Transaccion)item.DataBoundItem).Transaccion_Id ==
                                            oTraBoleta.Transaccion_Id)
                                        {
                                            item.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "Error de Envío";
                                            break;
                                        }
                                    }
                            }));
                        }

                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void ConxtruiryEnviarXMLFacturasNC(Transaccion oTransaccion, GridViewRowInfo row=null)
        {
            #region Metodo
            var oEmpresa = EmpresaNeg.Instance.Consultar(oTransaccion.sTraRUCEmpresa);

            bool rpta = false;

            rpta = wsServicio.GenerarArchivoXmlyZip(oTransaccion, oEmpresa);
            if (rpta)
            {
                #region Actualizando a Generado xml

                if (dgvTransaccion.InvokeRequired)
                {
                    dgvTransaccion.Invoke(new Action(delegate ()
                    {
                        foreach (var item in dgvTransaccion.Rows)
                        {
                            if (((Transaccion)item.DataBoundItem).Transaccion_Id ==
                                oTransaccion.Transaccion_Id)
                            {
                                item.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "Generado";
                            }
                        }
                    }));
                }
                else
                {
                    if(row != null)
                    row.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "Generado";
                }

                #endregion


                #region Enviar a Sunat

                bool resultado = false;
                resultado = wsServicio.EnviarArchivo(oTransaccion, oEmpresa);

                if (resultado)
                {
                    #region Actualizar exito

                    if (dgvTransaccion.InvokeRequired)
                    {
                        dgvTransaccion.Invoke(new Action(delegate ()
                        {
                            foreach (var item in dgvTransaccion.Rows)
                            {
                                if (((Transaccion)item.DataBoundItem).Transaccion_Id ==
                                    oTransaccion.Transaccion_Id)
                                {
                                    item.Cells["dgcnTraEstadoTransaccionElectronica"].Value = (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_SUNAT;
                                    item.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "Enviado";
                                }
                            }
                        }));
                    }
                    else
                    {
                        if (row != null)
                        {
                            row.Cells["dgcnTraEstadoTransaccionElectronica"].Value = (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_SUNAT;
                            row.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "Enviado";
                        }
                    }

                    #endregion

                }
                else
                #region Actualizar Falla

                   if (dgvTransaccion.InvokeRequired)
                {
                    dgvTransaccion.Invoke(new Action(delegate ()
                    {
                        foreach (var item in dgvTransaccion.Rows)
                        {
                            if (((Transaccion)item.DataBoundItem).Transaccion_Id ==
                                oTransaccion.Transaccion_Id)
                            {
                                item.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "Error de Envio";
                            }
                        }
                    }));
                }
                else
                {
                    if (row != null)
                        row.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "Error de Envio";
                }

                #endregion

                #endregion
            }

            #endregion
        }

        private bool Validar()
        {

            ContarElementos();

            if (dgvTransaccion.Rows.Count <= 0)
            {
                lblMensaje.Text=@"No se han encontrado Elementos en la lista";
                return false;
            }

            if(LstTransaccion != null && LstTransaccion.Count <= 0)
            {
                lblMensaje.Text = @"No se han encontrado Elementos en la Lista de Documentos LstTransaccion";
                return false;
            }


            nContar = ContarCheckBoxGridView(dgvTransaccion);

            if (nContar <= 0)
            {
                lblMensaje.Text="Seleccione al menos un registro de la Lista";
                return false;
            }
            return true;
        }

        private void MasterTemplate_CellFormatting(object sender, CellFormattingEventArgs e)
        {
            try
            {
                if (e.Column.Name.Contains("VistaPrevia"))
                {

                    if (e.CellElement.Children.Count <= 0) return;
                    var oBtnColumn = (RadButtonElement)e.CellElement.Children[0];

                    oBtnColumn.ImageAlignment = ContentAlignment.MiddleCenter;

                    if (e.Column.Name.Contains("VistaPrevia"))
                    {
                        oBtnColumn.ToolTipText = @"Ver PDF";

                        if (((Transaccion)e.Row.DataBoundItem).nTraEstadoTransaccionElectronica >
                              (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_ENVIADO_SUNAT)
                            oBtnColumn.Enabled = true;
                        else
                            oBtnColumn.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text=ex.Message;
            }
        }

        private async void btnEnviarCorreo_Click(object sender, EventArgs e)
        {
            try
            {
                if (Validar())
                {

                    if (MostrarPregunta("Esta seguro que desea enviar Correo al cliente con los Documentos Seleccionados?") == DialogResult.Yes) {

                        radWaitingBarElement1.Visibility = ElementVisibility.Visible;
                        radWaitingBarElement1.StartWaiting();


                    bool b_Resultado = false;
                    if (cboEstadoSunat.SelectedItem.Tag.toInt() == (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_BAJA_ENVIADA_SUNAT)
                    {
                        b_Resultado = await Task.Run(() => EnviarCorreoBajaClienteAsync());
                    }
                    else
                    {
                        b_Resultado = await Task.Run(() => GenerarPDFyEnviarCorreoClienteAsync());
                    }

                    if (b_Resultado)
                    {
                        MostrarMensaje("El Proceso de envío de Correo ha Terminado Correctamente");
                    }

                    radWaitingBarElement1.StopWaiting();
                    radWaitingBarElement1.Visibility = ElementVisibility.Hidden;
                    btnBuscar.PerformClick();
                }
                }
            }
            catch (Exception ex)
            {
                radWaitingBarElement1.Visibility = ElementVisibility.Hidden;
                radWaitingBarElement1.StopWaiting();
                lblMensaje.Text = ex.Message;
            }
        }

        private bool EnviarCorreoBajaClienteAsync()
        {
            try
            {
                return EnviarCorreoBajaCliente().Result;

            }
            catch (Exception)
            {

                throw;
            }
        }


        private Task<bool> EnviarCorreoBajaCliente()
        {
            return Task.Run(() => EnviarCorreoBaja());
        }

        private bool EnviarCorreoBaja()
        {
            foreach (var row in dgvTransaccion.Rows)
            {
                int index = row.Index;

                if (row.Cells["dgcOk"].Value != null)
                {
                    if ((bool)row.Cells["dgcOk"].Value)
                    {
                        var oTransaccion = LstTransaccion.FirstOrDefault(
                            x => x.Transaccion_Id == row.Cells["dgcTransaccion_Id"].Value.toInt()
                            );

                        var oEmpresa = EmpresaNeg.Instance.Consultar(oTransaccion.sTraRUCEmpresa);

                        if (row.Cells["dgcnTVeEstadoTransaccionElectronica"].Value.toInt() ==
                            (int)Enumerador.FACTURA_ELECTRONICA.ESTADO_DOC_ELECTRONICO_BAJA_ENVIADA_SUNAT)
                        {
                            TransaccionNeg.Instance.EnviarCorreoBaja(oTransaccion, oEmpresa);

                            if (dgvTransaccion.InvokeRequired)
                            {
                                dgvTransaccion.Invoke(new Action(delegate ()
                                {
                                    dgvTransaccion.Rows[index].Cells["dgcsTraEstadoTransaccionElectronica"]
                                        .Value = "Baja Enviada cliente";
                                }));
                            }
                            else
                            {
                                row.Cells["dgcsTraEstadoTransaccionElectronica"].Value = "Baja Enviada cliente";
                            }
                        }
                    }
                }
            }

            return true;
        }

        private void cboEstadoSunat_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            cboEstadoSunat_SelectedValueChanged(null, null);
        }
    }

}
