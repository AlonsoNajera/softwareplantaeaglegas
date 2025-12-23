using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SoftwarePlantas
{
    public partial class MenuLateral : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            // Cerrar la sesión del usuario
            Session.Clear(); // Limpia todos los datos de la sesión
            Session.Abandon(); // Termina la sesión actual
            FormsAuthentication.SignOut(); // Cierra la autenticación

            // Redirigir a la página de inicio de sesión
            Response.Redirect("~/Login.aspx");
        }
    }
}