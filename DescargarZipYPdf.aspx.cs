using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SoftwarePlantas
{
    public partial class DescargarZipYPdf : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string rutaZip = Session["rutaZipFisica"]?.ToString();
            string nombreZip = Session["nombreZip"]?.ToString();
            string rutaPdf = Session["rutaPdfWeb"]?.ToString();

            if (!IsPostBack && rutaZip != null && System.IO.File.Exists(rutaZip))
            {
                // 🧠 Enviar script que primero descarga el ZIP y luego abre el PDF
                string script = $@"
            <script type='text/javascript'>
                function iniciarDescargas() {{
                    var a = document.createElement('a');
                    a.href = 'DownloadZip.aspx';
                    a.download = '{nombreZip}';
                    document.body.appendChild(a);
                    a.click();
                    setTimeout(function () {{
                        window.open('{ResolveUrl(rutaPdf)}', '_blank');
                    }}, 2000);
                }}
                window.onload = iniciarDescargas;
            </script>";

                litScript.Text = script;
            }
        }
    }
}