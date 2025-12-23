using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SoftwarePlantas
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            DatabaseConnection dbConnection = new DatabaseConnection("MiConexionBD");
            bool isConnected = dbConnection.TestConnection();
            if (Session["IdUsuario"] == null || Session["Nombre"] == null)
            {
                Response.Redirect("Login.aspx");
            }
            else
            {
               
            }
           
        }
    }
}