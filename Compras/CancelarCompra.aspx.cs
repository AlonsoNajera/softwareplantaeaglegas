using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;

namespace SoftwarePlantas.Compras
{
    public partial class CancelarCompra : System.Web.UI.Page
    {
        string instancia = string.Empty;
        protected void Page_Load(object sender, EventArgs e)
        {
            // Evitar el almacenamiento en caché
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            if (Session["IdUsuario"] == null || Session["Nombre"] == null)
            {
                // Si la sesión ha expirado o el usuario no está autenticado, redirigir a Login.aspx
                Response.Redirect("~/Login.aspx");
            }
            else
            {
                // Establecer el mensaje de bienvenida con el nombre del usuario
                lblWelcome.Text = $"Bienvenido, {Session["Nombre"].ToString()}!";
            }
            if (!IsPostBack)
            {
                CargarEstaciones();

                txtFechaInicial.Text = DateTime.Now.ToString("yyyy-MM-dd");
                txtFechaFinal.Text = DateTime.Now.ToString("yyyy-MM-dd");

            }

            if (Request["__EVENTTARGET"] == "btnEliminar" && Request["__EVENTARGUMENT"] != null)
            {
                string folio = Request["__EVENTARGUMENT"];
                EliminarRecepcion(folio);
            }
         
        }


        private void CargarEstaciones()
        {
            int IdUsuario = Convert.ToInt32(Session["IdUsuario"]);

            string connectionString = WebConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT e.Id, e.Nombre 
            FROM estaciones e
            INNER JOIN UsersEstaciones ue ON e.Id = ue.idEstacion
            WHERE ue.idUsuario = @IdUsuario";

                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                adapter.SelectCommand.Parameters.AddWithValue("@IdUsuario", IdUsuario);

                DataTable dt = new DataTable();
                adapter.Fill(dt);

                ddlEstacion.DataSource = dt;
                ddlEstacion.DataTextField = "Nombre";
                ddlEstacion.DataValueField = "Id";
                ddlEstacion.DataBind();
            }

            ddlEstacion.Items.Insert(0, new ListItem("Selecciona una estación", ""));
        }


        private bool VerificarConexionBaseDatos(string connectionString)
        {
            try
            {
                using (SqlConnection testConnection = new SqlConnection(connectionString))
                {
                    testConnection.Open();
                    return true; // Conexión exitosa
                }
            }
            catch
            {
                return false; // Fallo de conexión
            }
        }

        protected void ddlEstacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            string estacionId = ddlEstacion.SelectedValue;


            string connectionString = ConfigurationManager.ConnectionStrings["ConexionBD"].ConnectionString;

            // Consulta para obtener el valor de 'instancia' usando el ID de la estación seleccionada
            string query = "SELECT instancia FROM estaciones WHERE Id = @Id";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", estacionId);
                    connection.Open();
                    instancia = command.ExecuteScalar()?.ToString();
                    connection.Close();
                }
            }


            if (instancia is null)
            {
                // Si la conexión falla, muestra un mensaje de error y detiene la ejecución
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "Swal.fire('Error', 'Selecciona una Estacion.', 'error');", true);

                return;

            }

            else
            {
                // Construye la cadena de conexión de la instancia seleccionada
                string instanciaConnectionString = $"Server={instancia};Database=Admingas;User Id=sa;Password=$Aes220213Nu4&;";

                // Llama al método para verificar la conexión
                if (VerificarConexionBaseDatos(instanciaConnectionString))
                {
                    // Si la conexión es exitosa, guarda la instancia en la sesión
                    Session["instanciaSeleccionada"] = instanciaConnectionString;
                    // Recargar los datos en el GridView
                    btnBuscar_Click(null, EventArgs.Empty);
                }
                else
                {
                    // Si la conexión falla, muestra un mensaje de error y detiene la ejecución
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "Swal.fire('Error', 'No se pudo establecer conexión con la base de datos.', 'error');", true);
                }
            }

        }

        private void EliminarRecepcion(string folio)
        {
            if (string.IsNullOrEmpty(folio))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "Swal.fire('Error', 'No se recibió el Folio.', 'error');", true);
                return;
            }

            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();
            if (string.IsNullOrEmpty(instanciaConnectionString))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "Swal.fire('Error', 'Instancia no seleccionada.', 'error');", true);
                return;
            }

            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    // 🔄 Actualizar Recepciones usando Folio
                    string updateRecepcion = "UPDATE Recepciones SET Estado = 0,UUID='',TransCRE='',ClaveVehiculo='',VolumenPemex=0,FolioDocumento=0,FechaDocumento=NULL,TipoDocumento='',TeminalAlmacenamiento='' WHERE Folio = @Folio";
                    using (SqlCommand cmd1 = new SqlCommand(updateRecepcion, conn, trans))
                    {
                        cmd1.Parameters.AddWithValue("@Folio", folio);
                        cmd1.ExecuteNonQuery();
                    }

                    // ❌ Eliminar de RecepcionesCap usando Folio
                    string deleteRelacionada = "DELETE FROM Recepcionescap WHERE Folio = @Folio";
                    using (SqlCommand cmd2 = new SqlCommand(deleteRelacionada, conn, trans))
                    {
                        cmd2.Parameters.AddWithValue("@Folio", folio);
                        cmd2.ExecuteNonQuery();
                    }

                    trans.Commit(); // ✅ Confirmar ambas acciones
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    ScriptManager.RegisterStartupScript(this, GetType(), "error",
                        $"Swal.fire('Error', 'Error al cancelar: {ex.Message}', 'error');", true);
                    return;
                }
            }

            // ✅ Mensaje de éxito
            ScriptManager.RegisterStartupScript(this, GetType(), "exito",
                $"Swal.fire('Eliminado', 'Folio {folio} cancelado correctamente.', 'success');", true);

            // 🔄 Recargar datos
            btnBuscar_Click(null, null);
        }



        protected void gvRecepciones_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvRecepciones.EditIndex = e.NewEditIndex;
            // 🔄 Recargar datos
            btnBuscar_Click(null, null);
        }

        protected void gvRecepciones_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvRecepciones.EditIndex = -1;
            // 🔄 Recargar datos
            btnBuscar_Click(null, null);
        }

        protected void gvRecepciones_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();
            GridViewRow row = gvRecepciones.Rows[e.RowIndex];
            string id = gvRecepciones.DataKeys[e.RowIndex].Value.ToString(); // Ahora usa Id en lugar de Folio

            string fecha = ((TextBox)row.FindControl("txtFecha")).Text;
            string volumenCapturado = ((TextBox)row.FindControl("txtVolumenCapturado")).Text;
            string folioDoc = ((TextBox)row.FindControl("txtFolioDocumento")).Text;
            string uuid = ((TextBox)row.FindControl("txtUUID")).Text;
            string claveVehiculo = ((TextBox)row.FindControl("txtClaveVehiculo")).Text;
            string transCRE = ((TextBox)row.FindControl("txtTransCRE")).Text;
            string precioCompra = ((TextBox)row.FindControl("txtPrecioCompra")).Text;

            using (SqlConnection con = new SqlConnection(instanciaConnectionString))
            {
                con.Open();
                string query = @"UPDATE RecepcionesCap
                         SET FechaDocumento = @Fecha, VolumenPemex = @VolumenCapturado,
                             FolioDocumento = @FolioDocumento, UUID = @UUID, ClaveVehiculo = @ClaveVehiculo,
                             TransCRE = @TransCRE, PrecioCompra = @PrecioCompra
                         WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Fecha", DateTime.ParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture));
                    cmd.Parameters.AddWithValue("@VolumenCapturado", volumenCapturado);
                    cmd.Parameters.AddWithValue("@FolioDocumento", folioDoc);
                    cmd.Parameters.AddWithValue("@UUID", uuid);
                    cmd.Parameters.AddWithValue("@ClaveVehiculo", claveVehiculo);
                    cmd.Parameters.AddWithValue("@TransCRE", transCRE);
                    cmd.Parameters.AddWithValue("@PrecioCompra", precioCompra);
                    cmd.Parameters.AddWithValue("@Id", id); // Usa Id en lugar de Folio
                    cmd.ExecuteNonQuery();
                }
            }

            gvRecepciones.EditIndex = -1;
            btnBuscar_Click(null, null);
        }


        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            string fechaInicio = txtFechaInicial.Text;
            string fechaFin = txtFechaFinal.Text;

            string instanciaConnectionString = Session["instanciaSeleccionada"]?.ToString();

            if (string.IsNullOrEmpty(instanciaConnectionString))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "error",
                    "Swal.fire('Error', 'Instancia no seleccionada.', 'error');", true);
                return;
            }


            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(instanciaConnectionString))
            {
                string query = @" SELECT R.Folio,R.Tanque,R.Producto,R.VolumenRecepcion,RCAP.FechaDocumento,RCAP.VolumenPemex AS VolumenCapturado,RCAP.FolioDocumento,
                                    RCAP.UUID,RCAP.ClaveVehiculo,RCAP.TransCRE,RCAP.PrecioCompra,RCAP.Id FROM Recepciones R INNER JOIN Recepcionescap RCAP ON R.Folio=RCAP.Folio
                                    WHERE CAST(RCAP.FechaDocumento AS DATE) BETWEEN @Inicio AND @Fin ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Inicio", fechaInicio);
                cmd.Parameters.AddWithValue("@Fin", fechaFin);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            gvRecepciones.DataSource = dt;
            gvRecepciones.DataBind();
            pnlResultados.Visible = true;
        }
    }
}