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
        protected Panel pnlMessage;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Evitar el almacenamiento en caché
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            if (!IsPostBack)
            {
                // Verificar licencia antes de cargar la página
                if (!VerificarLicencia())
                {
                    MostrarError("LICENCIA SUSPENDIDA", true);
                    btnLogin.Enabled = false;
                    txtPassword.Enabled = false;
                    txtUsername.Enabled = false;
                    lblVersion.Text = "LICENCIA SUSPENDIDA - Version 4.0";
                    return;
                }

                lblVersion.Text = "LICENCIA ACTIVA - Version 4.0";
            }
        }

        private bool VerificarLicencia()
        {
            bool licenciaValida = false;
            string connectionString = "Server=68.168.211.248;Database=TIAdmingas;User Id=Licencias;Password=Licencias2025;";
            string estacion = ConfigurationManager.AppSettings["Estacion"];

            if (string.IsNullOrEmpty(estacion))
            {
                return false;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT Status FROM LicenciaPlantas WHERE IdLicencia = @Estacion";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Estacion", estacion);
                        object result = cmd.ExecuteScalar();
                        if (result != null && Convert.ToInt32(result) == 1)
                        {
                            licenciaValida = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log error
                    System.Diagnostics.Debug.WriteLine("Error de licencia: " + ex.Message);
                }
            }

            return licenciaValida;
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MostrarError("Por favor ingrese usuario y contraseña");
                return;
            }

            string nombreUsuario = AuthenticateUser(username, password);
            
            if (!string.IsNullOrEmpty(nombreUsuario))
            {
                // Login exitoso - redirección directa
                Response.Redirect("Default.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                MostrarError("Usuario o contraseña incorrectos");
            }
        }

        private string AuthenticateUser(string username, string password)
        {
            string nombre = null;
            string connectionString = WebConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;
            string query = "SELECT IdUsuario, Nombre, Usuario, Status FROM Usuarios WHERE Usuario = @username AND Password = @password";

            try
            {
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
                                    MostrarError("El usuario no está activo");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarError("Error de conexión: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Error en AuthenticateUser: " + ex.Message);
            }

            return nombre;
        }

        private void MostrarError(string mensaje, bool esLicencia = false)
        {
            pnlMessage.Visible = true;
            lblMessage.Text = mensaje;
             
            if (!esLicencia)
            {
                string script = $@"
                    Swal.fire({{
                        title: 'Error',
                        text: '{mensaje}',
                        icon: 'error',
                        confirmButtonColor: '#3498db'
                    }});
                ";
                ClientScript.RegisterStartupScript(this.GetType(), "loginError", script, true);
            }
        }
    }
}