using DestockFMSAC.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telerik.WinControls;

namespace DestockFMSAC
{
    public partial class frmVistaPreviaPDF : BaseForm
    {

        private String _sRutaPDF = String.Empty;

        public frmVistaPreviaPDF()
        {
            InitializeComponent();
        }

        private void frmVistaPreviaPDF_Load(object sender, EventArgs e)
        {
            try
            {
                InicializarFormulario();

                if (_sRutaPDF != string.Empty)
                {

                    axcVisorPDF.LoadDocument(_sRutaPDF);
                    //axcVisorPDF.ScaleFactor = 0.6f;
                }
                this.WindowState = FormWindowState.Maximized;
            }
            catch (Exception ex)
            {
                RadMessageBox.Show(ex.Message);
            }
        }

        private void InicializarFormulario()
        {
            if (Tag != null)
            {
                var dctTag = Utilitarios.Util.StringToDictionary(Tag.ToString());
                if (dctTag.ContainsKey("sRutaPDF"))
                    _sRutaPDF = dctTag["sRutaPDF"].ToString();
            }
        }

        private void frmVistaPreviaPDF_FormClosed(object sender, FormClosedEventArgs e)
        {
            axcVisorPDF.UnloadDocument();
        }
    }
}
