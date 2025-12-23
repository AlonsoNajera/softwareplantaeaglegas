using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SoftwarePlantas
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Evitar el almacenamiento en caché
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            // Verificar licencia antes de cargar la página
            if (!VerificarLicencia())
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showAlert",
                "Swal.fire({title: 'Error', text: 'LICENCIA SUSPENDIDA', icon: 'error'});", true);
                btnLogin.Enabled = false;
                txtPassword.Enabled = false;
                txtUsername.Enabled = false;
                lblVersion.Text = "LICENCIA SUSPENDIDA";
                return;
            }


            if (IsPostBack)
            {
                lblMessage.Text = "";

            }
            if (!IsPostBack)
            {
                lblVersion.Text = "LICENCIA ACTIVA - Version 3.0";
            }
        }

        private bool VerificarLicencia()
        {
            bool licenciaValida = false;
            string connectionString = "Server=68.168.211.248;Database=TIAdmingas;User Id=Licencias;Password=Licencias2025;";
            string estacion = ConfigurationManager.AppSettings["Estacion"]; // Obtener el valor de la estación desde Web.config
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT Status FROM LicenciaPlantas WHERE IdLicencia = @Estacion"; // Ajusta según tu esquema
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Estacion", estacion); // Parámetro para Estacion
                        object result = cmd.ExecuteScalar();
                        if (result != null && Convert.ToInt32(result) == 1)

                        {
                            licenciaValida = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Loggear error si es necesario
                    lblMessage.Text = "Error de conexión con la licencia: " + ex.Message;
                }
            }

            return licenciaValida;
        }


        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string nombreUsuario = AuthenticateUser(username, password);
            if (!string.IsNullOrEmpty(nombreUsuario))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert",
                    $"Swal.fire({{title: 'Bienvenido', text: '¡Hola, {nombreUsuario}!', icon: 'success'}}).then(function() {{ window.location = 'Default.aspx'; }});", true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert",
                    "Swal.fire({title: 'Error', text: 'Usuario o contraseña incorrectos.', icon: 'error'});", true);
            }
        }

        private string AuthenticateUser(string username, string password)
        {
            string nombre = null;
            string connectionString = WebConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;
            string query = "SELECT IdUsuario, Nombre, Usuario, Status FROM Usuarios WHERE Usuario = @username AND Password = @password";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool isActive = (bool)reader["Status"];
                            if (isActive)
                            {
                                Session["IdUsuario"] = reader["IdUsuario"];
                                Session["Nombre"] = reader["Nombre"];
                                Session["Usuario"] = reader["Usuario"];
                                nombre = reader["Nombre"].ToString();
                            }
                            else
                            {
                                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert",
                                    "Swal.fire({ title: 'Error', text: 'El usuario no está activo.', icon: 'error' });", true);
                            }
                        }
                        else
                        {
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "alert",
                                "Swal.fire({ title: 'Error', text: 'Usuario o contraseña incorrectos.', icon: 'error' });", true);
                        }
                    }
                    connection.Close();
                }
            }
            return nombre;
        }
    }
}