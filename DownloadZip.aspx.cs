using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SoftwarePlantas
{
    public partial class DownloadZip : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string rutaZip = Session["rutaZipFisica"]?.ToString();
            string nombreZip = Session["nombreZip"]?.ToString();

            if (!string.IsNullOrEmpty(rutaZip) && File.Exists(rutaZip))
            {
                Response.Clear();
                Response.ContentType = "application/zip";
                Response.AddHeader("Content-Disposition", $"attachment; filename={nombreZip}");
                Response.TransmitFile(rutaZip);
                Response.End();
            }
        }
    }
}